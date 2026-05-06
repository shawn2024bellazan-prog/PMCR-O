// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATOR SERVICE
// File       : Program.cs
// Identity   : The Conductor Lobe
// Law Anchor : PROTO-008
// ThoughtLock: 2026-05-06
//
// FIX (Bug 4 — FRAC-ORCHESTRATOR-DISPATCH-001):
//   ExecuteType1Async previously called httpClientFactory.CreateClient() (unnamed)
//   and posted to hardcoded URIs like "http://mcp-filesystem/mcp". Even though
//   AppHost.cs already wires orchestratorService.WithReference(mcpFilesystem/
//   mcpTerminal/mcpPlaywright), relying on the ResolvingHttpDelegatingHandler's
//   hostname substitution is fragile: the handler silently falls through to DNS
//   when the configuration key is absent or when the client is unnamed.
//
//   Resolution:
//     Register named HttpClients for each MCP actuator here, with BaseAddresses
//     set directly from the Aspire-injected configuration keys:
//       services:mcp-filesystem:http:0
//       services:mcp-terminal:http:0
//       services:mcp-playwright:http:0
//     OrchestratorGrpcService.ExecuteType1Async resolves the correct named client
//     per actuator and posts to the relative path "/mcp" — no hostname resolution
//     required at call time, no silent DNS fallback.
//
// FIX (FRAC-MCP-406-002 — Accept header):
//   All MCP dispatch calls were returning HTTP 406 Not Acceptable:
//     "Not Acceptable: Client must accept both application/json and text/event-stream"
//   PostAsJsonAsync only sets Content-Type: application/json and sends no Accept
//   header. The MCP SDK's Streamable HTTP transport requires:
//     Accept: application/json, text/event-stream
//   Added to DefaultRequestHeaders inside RegisterMcpClient so all three actuator
//   clients (filesystem, terminal, playwright) carry it automatically.
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Http.Resilience;
using ProjectName.OrchestratorService.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add gRPC and HTTP Client (for TYPE 1 Dispatch calls)
builder.Services.AddGrpc();
builder.Services.AddHttpClient();

// ── Named HttpClients for MCP Actuators ──────────────────────────────────────
// Register one named client per actuator. BaseAddress is read from the Aspire
// config key injected by WithReference(...) in AppHost.cs.
// OrchestratorGrpcService.ExecuteType1Async resolves clients by name.
void RegisterMcpClient(string name, string configKeyHttp, string configKeyHttps)
{
    var url = builder.Configuration[configKeyHttp]
              ?? builder.Configuration[configKeyHttps];

    if (url is null)
    {
        Console.Error.WriteLine(
            $"[WARN] OrchestratorService: Aspire endpoint for '{name}' not found in configuration. " +
            $"Ensure AppHost.cs wires orchestratorService.WithReference({name.Replace("-", "")}). " +
            $"TYPE 1 MCP dispatch to '{name}' will fail.");
    }

    builder.Services.AddHttpClient(name, client =>
    {
        if (url is not null)
            client.BaseAddress = new Uri(url);

        // FIX (FRAC-MCP-406-002): MCP Streamable HTTP transport requires both media types.
        // PostAsJsonAsync only sets Content-Type — it never sets Accept. Without this header
        // the MCP server returns 406 Not Acceptable on every call.
        client.DefaultRequestHeaders.Add("Accept", "application/json, text/event-stream");
    });
}

RegisterMcpClient("mcp-filesystem",
    "services:mcp-filesystem:http:0",
    "services:mcp-filesystem:https:0");

RegisterMcpClient("mcp-terminal",
    "services:mcp-terminal:http:0",
    "services:mcp-terminal:https:0");

RegisterMcpClient("mcp-playwright",
    "services:mcp-playwright:http:0",
    "services:mcp-playwright:https:0");

// ── Phase Service gRPC Clients ────────────────────────────────────────────────
void RegisterOrchestratorClient<TClient>(string serviceName) where TClient : class
{
    builder.Services.AddGrpcClient<TClient>(o =>
        o.Address = new Uri($"https://{serviceName}")) // https enforces HTTP/2 ALPN negotiation
    .AddServiceDiscovery()
    .AddStandardResilienceHandler()
    .Configure(options =>
    {
        options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
        options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(9);
        options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(20);
        options.Retry.MaxRetryAttempts = 1;
    });
}

RegisterOrchestratorClient<ProjectName.Contracts.Planner.PlannerClient>("planner-service");
RegisterOrchestratorClient<ProjectName.Contracts.Maker.MakerClient>("maker-service");
RegisterOrchestratorClient<ProjectName.Contracts.Checker.CheckerClient>("checker-service");
RegisterOrchestratorClient<ProjectName.Contracts.Reflector.ReflectorClient>("reflector-service");

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGrpcService<OrchestratorGrpcService>();

app.MapGet("/", () => "ProjectName.OrchestratorService — gRPC Conductor Lobe.");

app.Run();