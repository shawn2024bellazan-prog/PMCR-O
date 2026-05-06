// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.FILESYSTEM
// File   : Program.cs
// Type   : DUAL — TYPE 1 (write) + TYPE 2 (read) per ARCH-NEW-001
// Law    : GOV-001, ARCH-NEW-001, SUB-LAW-003, FRAC-FS-TRAVERSAL-001
// ThoughtLock: 2026-05-06
//
// TRANSPORT HISTORY — read before changing WithHttpTransport():
//
// Attempt 1 — Stateless = true, no Accept header on clients:
//   Result: HTTP 406 Not Acceptable on every tool call.
//   Cause:  Streamable HTTP transport requires clients to send
//             Accept: application/json, text/event-stream
//           PostAsJsonAsync sends no Accept header, so the server rejected every
//           request before it reached any tool handler.
//
// Attempt 2 — Stateless = false (default SSE transport):
//   Result: HTTP 400 Bad Request on every tool call.
//   Error:  "A new session can only be created by an initialize request.
//            Include a valid Mcp-Session-Id header for non-initialize requests,
//            or enable stateless mode..."
//   Cause:  SSE/stateful transport requires an MCP initialize handshake first,
//           producing a Mcp-Session-Id that must be included in all subsequent
//           calls. PostAsJsonAsync fires tool calls directly with no session
//           setup, so the server rejects every call.
//
// FIX (FRAC-MCP-400-001) — correct combination:
//   Stateless = true  +  Accept header on all calling HttpClients.
//
//   Stateless = true  → server accepts direct tool calls with no initialize
//                        handshake and no Mcp-Session-Id header required.
//   Accept header     → satisfies the Streamable HTTP transport's media-type
//                        negotiation requirement (406 guard).
//
//   The Accept header is set in DefaultRequestHeaders inside every AddHttpClient
//   lambda in OrchestratorService/Program.cs, CheckerService/Program.cs, and
//   PlannerService/Program.cs:
//     client.DefaultRequestHeaders.Add("Accept", "application/json, text/event-stream");
//
//   Do NOT remove Stateless = true without also removing the Accept header from
//   all three service registrations AND implementing MCP session management
//   (initialize → Mcp-Session-Id) in every PostAsJsonAsync call site.
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
//
// FIX (FRAC-MCP-400-001): Stateless = true is required because phase services call
// tool endpoints directly via PostAsJsonAsync with no MCP session handshake.
// The stateful SSE transport (Stateless = false) demands a Mcp-Session-Id header
// on every non-initialize request — PostAsJsonAsync never sends one → HTTP 400.
// Stateless = true removes the session requirement entirely.
// The accompanying 406 guard (Accept header) is set on all client registrations.
builder.Services
    .AddMcpServer()
    .WithHttpTransport(options => options.Stateless = true)
    .WithTools<FileSystemTools>()
    .WithResources<FileSystemResources>()
    .WithPrompts<FileSystemPrompts>();

var app = builder.Build();
app.MapDefaultEndpoints();

// Maps Streamable HTTP endpoints for the MCP client.
app.MapMcp("/mcp");

// Diagnostic root endpoint — confirms active configuration.
app.MapGet("/", () =>
    $"Mcp.FileSystem — DUAL TYPE 1/2 MCP per ARCH-NEW-001. Sandbox Root: {root}");

app.Run();