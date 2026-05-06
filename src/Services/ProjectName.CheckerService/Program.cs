// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — CHECKER SERVICE
// File       : Program.cs
// Identity   : The QualityChecker Lobe (Cognitive)
// Law Anchor : PROTO-008, MAF-001
// ThoughtLock: 2026-05-06
//
// FIX (Bug 2 — FRAC-CHECKER-MCP-001):
//   CheckerGrpcService.ExecuteMcpRead previously called
//     httpClientFactory.CreateClient()   (unnamed, no BaseAddress)
//   and posted to the hardcoded URI "http://mcp-filesystem/mcp".
//   ConfigureHttpClientDefaults wires the ResolvingHttpDelegatingHandler to
//   every client, but that handler substitutes the host ONLY when the env var
//     services__mcp-filesystem__http__0
//   is present in the process environment — which only happens when AppHost.cs
//   wires checkerService.WithReference(mcpFilesystem).
//
//   Without that reference (the original bug), the env var is absent, the
//   handler falls through to DNS, and every TYPE 2 ReadFile call fails with
//   SocketException 11001 after 4 Polly retries (~18 s wasted). The LLM then
//   scores the cycle without file evidence and responds in plain English prose,
//   causing the Orchestrator to crash with:
//     JsonException: 'G' is an invalid start of a value.
//
//   Resolution (two-part):
//     1. AppHost.cs now wires checkerService.WithReference(mcpFilesystem) so
//        Aspire injects services__mcp-filesystem__http__0 into this process.
//     2. Here we register a named "mcp-filesystem" HttpClient whose BaseAddress
//        is read directly from that Aspire configuration key. CheckerGrpcService
//        requests the named client and uses the relative path "/mcp" — no
//        hostname magic required at call time, no silent DNS fallback.
//
// FIX (FRAC-MCP-406-002 — Accept header):
//   All MCP calls were returning HTTP 406 Not Acceptable:
//     "Not Acceptable: Client must accept both application/json and text/event-stream"
//   PostAsJsonAsync only sets Content-Type: application/json and sends no Accept
//   header. The MCP SDK's Streamable HTTP transport requires:
//     Accept: application/json, text/event-stream
//   on every POST to /mcp. Added to DefaultRequestHeaders at client construction
//   so every call site benefits automatically.
// ═══════════════════════════════════════════════════════════════════════════════

using ProjectName.CheckerService.Services;
using ProjectName.Skills.Core;
using ProjectName.ServiceDefaults; // Brings in AddAIDefaults()

var builder = WebApplication.CreateBuilder(args);

// Standard Aspire defaults + centralized AI configuration (fixes URI parsing)
builder.AddServiceDefaults();
builder.AddAIDefaults();

// Register the cognitive skill
builder.Services.AddSingleton<CheckerSkill>();

// ── Named HttpClient for the MCP Filesystem actuator ─────────────────────────
// Aspire injects services__mcp-filesystem__http__0 via WithReference(mcpFilesystem)
// in AppHost.cs. Reading it explicitly here avoids relying on the
// ResolvingHttpDelegatingHandler's hostname substitution, which silently
// falls through to DNS when the key is missing.
{
    var mcpFsUrl =
        builder.Configuration["services:mcp-filesystem:http:0"]   // http endpoint (preferred)
        ?? builder.Configuration["services:mcp-filesystem:https:0"]; // https endpoint (fallback)

    if (mcpFsUrl is null)
    {
        Console.Error.WriteLine(
            "[WARN] CheckerService: Aspire service endpoint for 'mcp-filesystem' not found in " +
            "configuration. Ensure AppHost.cs wires checkerService.WithReference(mcpFilesystem). " +
            "TYPE 2 MCP tool calls (ReadFile) will fail with DNS errors and the Checker LLM " +
            "will respond in prose, crashing the Orchestrator with a JsonException.");
    }

    builder.Services.AddHttpClient("mcp-filesystem", client =>
    {
        if (mcpFsUrl is not null)
            client.BaseAddress = new Uri(mcpFsUrl);

        // FIX (FRAC-MCP-406-002): MCP Streamable HTTP transport requires both media types.
        // PostAsJsonAsync only sets Content-Type — it never sets Accept. Without this header
        // the MCP server returns 406 Not Acceptable on every call.
        client.DefaultRequestHeaders.Add("Accept", "application/json, text/event-stream");
    });
}

builder.Services.AddGrpc();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGrpcService<CheckerGrpcService>();

app.MapGet("/", () => "ProjectName.CheckerService — gRPC QualityChecker Lobe.");

app.Run();