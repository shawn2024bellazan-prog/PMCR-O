// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — MAKER SERVICE
// File       : Program.cs
// Identity   : The ArtifactMaker Lobe (Cognitive)
// Law Anchor : WORKFLOW-010, MAF-001
// ThoughtLock: 2026-05-06
//
// FIX (Bug 1 — FRAC-MAKER-MCP-001):
//   MakerGrpcService does not currently make TYPE 2 MCP read calls, but the
//   Maker produces McpDispatchDecision records targeting "mcp-filesystem" that
//   the Orchestrator executes as TYPE 1 calls via its own named client. To
//   ensure the Aspire service endpoint is available to this process for any
//   future TYPE 2 reads (e.g. ReadFile to verify pre-conditions before write),
//   and to mirror the PlannerService/CheckerService pattern, we register a
//   named "mcp-filesystem" HttpClient here.
//
//   AppHost.cs now also wires makerService.WithReference(mcpFilesystem) so
//   Aspire injects services__mcp-filesystem__http__0 into this process.
// ═══════════════════════════════════════════════════════════════════════════════

using ProjectName.MakerService.Services;
using ProjectName.Skills.Core;
using ProjectName.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Standard Aspire defaults + centralized AI configuration (fixes URI parsing)
builder.AddServiceDefaults();
builder.AddAIDefaults();

// Register the cognitive skill
builder.Services.AddSingleton<MakerSkill>();

// ── Named HttpClient for the MCP Filesystem actuator ─────────────────────────
// Aspire injects services__mcp-filesystem__http__0 via WithReference(mcpFilesystem)
// in AppHost.cs. Reading it explicitly here avoids relying on the
// ResolvingHttpDelegatingHandler's hostname substitution.
{
    var mcpFsUrl =
        builder.Configuration["services:mcp-filesystem:http:0"]
        ?? builder.Configuration["services:mcp-filesystem:https:0"];

    if (mcpFsUrl is null)
    {
        Console.Error.WriteLine(
            "[WARN] MakerService: Aspire service endpoint for 'mcp-filesystem' not found in " +
            "configuration. Ensure AppHost.cs wires makerService.WithReference(mcpFilesystem). " +
            "Any TYPE 2 MCP tool calls will fail with DNS errors.");
    }

    builder.Services.AddHttpClient("mcp-filesystem", client =>
    {
        if (mcpFsUrl is not null)
            client.BaseAddress = new Uri(mcpFsUrl);
    });
}

builder.Services.AddGrpc();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGrpcService<MakerGrpcService>();

app.MapGet("/", () => "ProjectName.MakerService — gRPC ArtifactMaker Lobe.");

app.Run();