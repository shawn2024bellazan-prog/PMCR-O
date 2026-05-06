// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — PLANNER SERVICE
// File       : Program.cs
// Identity   : The Strategist Lobe (Cognitive)
// Law Anchor : MAF-004 (AsAIAgent), DI-006 (single OllamaApiClient instance)
// ThoughtLock: 2026-05-06
//
// FIX (Bug 1 — FRAC-PLANNER-MCP-001):
//   ExecuteMcpRead previously used httpClientFactory.CreateClient() (unnamed client)
//   posting to the hardcoded URI "http://mcp-filesystem/mcp". Although
//   ConfigureHttpClientDefaults adds the ResolvingHttpDelegatingHandler to every
//   client, that handler can only substitute the host when the env var
//     services__mcp-filesystem__http__0=http://localhost:{PORT}
//   is present in the process environment. That env var is only injected when
//   AppHost.cs wires .WithReference(mcpFilesystem) → plannerService.
//
//   Even with WithReference in AppHost, relying on hostname-based substitution
//   is fragile: the handler silently falls through to DNS when the key is absent,
//   producing SocketException 11001 with no actionable log entry.
//
//   Resolution:
//     Register a named "mcp-filesystem" HttpClient here and set its BaseAddress
//     directly from the Aspire-injected configuration key. PlannerGrpcService
//     requests this named client and uses a relative "/mcp" path — no hostname
//     magic required at call time.
//
//   Configuration key written by Aspire (via WithReference):
//     services__mcp-filesystem__http__0  →  services:mcp-filesystem:http:0
//
//   If the key is absent (WithReference missing or wrong service name), startup
//   logs a clear warning and the client falls back to the unresolved URI so the
//   failure surface remains the same as before — but now it's visible at boot,
//   not buried in per-request stack traces.
//
// FIX (FRAC-MCP-406-002 — Accept header):
//   All MCP calls were returning HTTP 406 Not Acceptable:
//     "Not Acceptable: Client must accept both application/json and text/event-stream"
//   PostAsJsonAsync only sets Content-Type: application/json and sends no Accept
//   header. The MCP SDK's Streamable HTTP transport requires:
//     Accept: application/json, text/event-stream
//   Added to DefaultRequestHeaders at client construction so every call site
//   benefits automatically.
// ═══════════════════════════════════════════════════════════════════════════════

using ProjectName.PlannerService.Services;
using ProjectName.Skills.Core;
using ProjectName.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Standard Aspire defaults + centralized AI configuration (fixes URI parsing)
builder.AddServiceDefaults();
builder.AddAIDefaults();

// Register the cognitive skill
builder.Services.AddSingleton<PlannerSkill>();

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
        // This warning fires at startup, making the missing WithReference immediately visible.
        Console.Error.WriteLine(
            "[WARN] PlannerService: Aspire service endpoint for 'mcp-filesystem' not found in " +
            "configuration. Ensure AppHost.cs wires plannerService.WithReference(mcpFilesystem). " +
            "TYPE 2 MCP tool calls (ReadFile, ListDirectory) will fail with DNS errors.");
    }

    builder.Services.AddHttpClient("mcp-filesystem", client =>
    {
        // When the Aspire URL is available, set it as the base address so that
        // PlannerGrpcService can use a relative "/mcp" path. When it is absent,
        // no BaseAddress is set — the client will still carry the service-discovery
        // handler from ConfigureHttpClientDefaults and attempt hostname resolution.
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
app.MapGrpcService<PlannerGrpcService>();

app.MapGet("/", () => "ProjectName.PlannerService — gRPC Strategist Lobe.");

app.Run();