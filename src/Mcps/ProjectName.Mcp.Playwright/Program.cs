// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — PLAYWRIGHT MCP ACTUATOR
// File       : Program.cs
// Identity   : Browser Automation Actuator Boot Sequence
// Law Anchor : ARCH-012, ARCH-013, PW-LAW-005, PW-LAW-006, SUB-LAW-002
// ThoughtLock: 2026-05-05
// ───────────────────────────────────────────────────────────────────────────────
// ARCHITECTURE MANDATE (ARCH-012 / ARCH-013):
//   This project is the ONLY internal host for PlaywrightTools.
//   The OrchestrationApi must NEVER carry a <ProjectReference> to this project.
//   All inter-service communication flows through Aspire service discovery over HTTP.
//
// DI REGISTRATION ORDER (CRITICAL):
//   Infrastructure singletons (PlaywrightConfig, PlaywrightSessionManager) MUST
//   be registered BEFORE AddMcpServer(). The MCP SDK resolves PlaywrightTools from
//   DI when the first tool call arrives — if config/session are not in the container
//   yet, an ObjectDisposedException or NullReferenceException will occur.
//
//   Registration order:
//     1. Infrastructure singletons: PlaywrightConfig, PlaywrightSessionManager
//     2. PlaywrightTools (explicit singleton — MCP SDK resolves from container)
//     3. PlaywrightResources, PlaywrightPrompts (explicit singletons)
//     4. AddMcpServer().WithTools/Resources/Prompts — resolves from container
//
// PLAYWRIGHT BROWSER INSTALL:
//   The Playwright Chromium binaries must be present at container startup.
//   The Dockerfile runs: RUN dotnet tool install ... && playwright install chromium
//   Without this, GetPageAsync() throws PlaywrightException on first call.
// ═══════════════════════════════════════════════════════════════════════════════

using ProjectName.Mcp.Playwright.Configuration;
using ProjectName.Mcp.Playwright.Prompts;
using ProjectName.Mcp.Playwright.Resources;
using ProjectName.Mcp.Playwright.Tools;
using ProjectName.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Aspire integration — registers health checks, telemetry, service discovery
builder.AddServiceDefaults();

// ── 1. INFRASTRUCTURE SINGLETONS ──────────────────────────────────────────────
// PlaywrightConfig reads Parameters:project-root and Playwright:* config keys.
// PlaywrightSessionManager owns the physical Chromium process lifecycle.
// Both MUST be registered before AddMcpServer() (DI REGISTRATION ORDER law above).
builder.Services.AddSingleton<PlaywrightConfig>();
builder.Services.AddSingleton<PlaywrightSessionManager>();

// ── 2. MCP PILLAR SINGLETONS ──────────────────────────────────────────────────
// Explicit singleton registration ensures the MCP SDK's DI resolution succeeds.
// Do NOT rely on transient — PlaywrightSessionManager is stateful (browser process).
builder.Services.AddSingleton<PlaywrightTools>();
builder.Services.AddSingleton<PlaywrightResources>();
builder.Services.AddSingleton<PlaywrightPrompts>();

// ── 3. MCP SERVER — THREE-PILLAR SURFACE ──────────────────────────────────────
// WithHttpTransport(): Stateless JSON-RPC 2.0 over HTTP POST — no text/event-stream.
// All three pillars registered. Agents access tools, resources, and prompts.
builder.Services
    .AddMcpServer()
    .WithHttpTransport(options => options.Stateless = true) // <-- ADD OPTIONS HERE
    .WithTools<PlaywrightTools>()
    .WithResources<PlaywrightResources>()
    .WithPrompts<PlaywrightPrompts>();

var app = builder.Build();

app.MapDefaultEndpoints();

// ── MCP ENDPOINT ───────────────────────────────────────────────────────────────
// FRAC-MCP-TRANSPORT-001: StatelessHttp at /mcp. McpToolExecutor posts here.
app.MapMcp("/mcp");

app.MapGet("/", () =>
    "ProjectName.Mcp.Playwright — Browser Automation Actuator Online. All tools are TYPE 2.");

app.Run();