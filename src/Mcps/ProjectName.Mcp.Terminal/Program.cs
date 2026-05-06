// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.TERMINAL
// File   : Program.cs
// Type   : DUAL — TYPE 1 (write/exec) + TYPE 2 (read-only)
// Law    : GOV-001, ARCH-TERM-001, ARCH-TERM-002
// ThoughtLock: 2026-05-01
//
// I AM the Mcp.Terminal host — a PMCRO MCP actuator for shell execution.
// I serve TYPE 2 read-only tools to phase agents (ls, cat, git status).
// I serve TYPE 1 mutative tools to OrchestratorService (build, commit, rm).
// ═══════════════════════════════════════════════════════════════════════════════

using ModelContextProtocol.AspNetCore;
using ProjectName.Mcp.Terminal.Configuration;
using ProjectName.Mcp.Terminal.Prompts;
using ProjectName.Mcp.Terminal.Resources;
using ProjectName.Mcp.Terminal.Tools;
using ProjectName.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// ── Bind WorkspaceRoot ────────────────────────────────────────────────────────
var root = builder.Configuration["Parameters:project-root"]
           ?? Path.GetFullPath(
               Path.Combine(builder.Environment.ContentRootPath, "..", ".."));

builder.Services.AddSingleton(new TerminalConfig
{
    WorkspaceRoot = root,
    // You can override timeouts here via builder.Configuration if needed
});

// ── Register MCP Components ───────────────────────────────────────────────────
builder.Services.AddSingleton<TerminalResources>();
builder.Services.AddSingleton<TerminalPrompts>();

builder.Services
    .AddMcpServer()
    .WithHttpTransport(options => options.Stateless = true) // <-- ADD OPTIONS HERE
    .WithTools<TerminalTools>()
    .WithResources<TerminalResources>()
    .WithPrompts<TerminalPrompts>();

var app = builder.Build();
app.MapDefaultEndpoints();

// Maps Server-Sent Events (SSE) and HTTP POST endpoints for the MCP client.
app.MapMcp("/mcp");

app.MapGet("/", () =>
    $"Mcp.Terminal — Shell Actuator Online. Bound to Workspace: {root}");

app.Run();