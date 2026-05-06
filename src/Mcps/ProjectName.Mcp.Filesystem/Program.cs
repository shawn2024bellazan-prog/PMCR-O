// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.FILESYSTEM
// File   : Program.cs
// Type   : DUAL — TYPE 1 (write) + TYPE 2 (read) per ARCH-NEW-001
// Law    : GOV-001, ARCH-NEW-001, SUB-LAW-003, FRAC-FS-TRAVERSAL-001
// ThoughtLock: 2026-05-01
//
// I AM the Mcp.FileSystem host — a dual-type PMCRO MCP actuator.
// I serve TYPE 2 read tools to phase agents.
// I serve TYPE 1 write tools to OrchestratorService via McpDispatchDecision.
// I enforce the FileSystemRoot boundary via ResolveSafe() in FileSystemTools.
// ═══════════════════════════════════════════════════════════════════════════════

using ModelContextProtocol.AspNetCore;
using ProjectName.Mcp.FileSystem.Configuration;
using ProjectName.Mcp.FileSystem.Prompts;
using ProjectName.Mcp.FileSystem.Resources;
using ProjectName.Mcp.FileSystem.Tools;
using ProjectName.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// ── Bind FileSystemRoot ───────────────────────────────────────────────────────
// Set via Aspire environment variable: Parameters__project-root → Parameters:project-root
// Default: two levels above ContentRootPath (the solution root in typical layouts).
var root = builder.Configuration["Parameters:project-root"]
           ?? Path.GetFullPath(
               Path.Combine(builder.Environment.ContentRootPath, "..", ".."));

// Register as singleton — FileSystemConfig is immutable and shared across requests.
builder.Services.AddSingleton(new FileSystemConfig { FileSystemRoot = root });

// ── Register MCP Components ───────────────────────────────────────────────────
builder.Services.AddSingleton<FileSystemResources>();
builder.Services.AddSingleton<FileSystemPrompts>();

// SUB-LAW-003: MCPs do NOT register proto/gRPC services — only MCP tools.
// GOV-001: FileSystemTools is the only domain-specific code. Program.cs is boilerplate.
builder.Services
    .AddMcpServer()
    .WithHttpTransport(options => options.Stateless = true) // <-- ADD OPTIONS HERE
    .WithTools<FileSystemTools>()
    .WithResources<FileSystemResources>()
    .WithPrompts<FileSystemPrompts>();

var app = builder.Build();
app.MapDefaultEndpoints();

// Maps Server-Sent Events (SSE) and HTTP POST endpoints for the MCP client.
app.MapMcp("/mcp");

// Diagnostic root endpoint — confirms active configuration.
app.MapGet("/", () =>
    $"Mcp.FileSystem — DUAL TYPE 1/2 MCP per ARCH-NEW-001. Sandbox Root: {root}");

app.Run();