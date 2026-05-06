// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Mcp/IMcpClientRegistry.cs
// Identity   : Native MCP Client Pool Contract
// Law Anchor : ARCH-NEW-001, ARCH-MCP-NAT-001
// ThoughtLock: 2026-05-05 → MAF+MCP
//
// ARCHITECTURE CHANGE — MAF-Native MCP Integration
//
//   BEFORE (manual HTTP relay):
//     Planner/Maker call McpToolExecutor.ExecuteType2Async() or ExecuteType1Async()
//     which POSTs raw JSON-RPC to each MCP actuator HTTP endpoint.
//     MAF's function-calling loop never fires — tool dispatch is hand-rolled.
//
//   AFTER (native MAF integration):
//     Each MCP actuator's tools are loaded at startup via McpClientFactory
//     and converted to AITool objects via mcpTools.Cast<AITool>().
//     These are fed into ChatClientAgentOptions.Tools so MAF's ChatClientAgent
//     own the tool loop — it detects tool_calls from the model, invokes the
//     MCP client, and feeds results back automatically.
//     McpToolExecutor is RETAINED only for TYPE 1 Orchestrator dispatch
//     (write/mutate operations that need explicit gate control).
//
//   LOCKED CONSTRAINT (ARCH-MCP-NAT-001):
//     I NEVER manually invoke MCP tools for TYPE 2 (reconnaissance) operations.
//     I ALWAYS let MAF's built-in agentic loop handle tool invocation for agents.
//     TYPE 1 (mutative) dispatch remains explicit via McpToolExecutor for
//     Orchestrator gate control (ARCH-NEW-001 gate semantics preserved).
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.AI;

namespace ProjectName.OrchestrationApi.Mcp;

/// <summary>
/// I AM the IMcpClientRegistry.
/// I hold the live AITool collections for each MCP actuator, loaded at startup
/// via McpClientFactory + SseClientTransport.
/// Phase agents pull their tool sets from here at registration time.
/// </summary>
public interface IMcpClientRegistry : IAsyncDisposable
{
    /// <summary>
    /// All tools available from the Filesystem MCP actuator.
    /// TYPE 2 (ReadFile, ListDirectory, GetFileInfo) safe for all phases.
    /// TYPE 1 (WriteFile, DeletePath) — included but only dispatched via Orchestrator gate.
    /// </summary>
    IReadOnlyList<AITool> FilesystemTools { get; }

    /// <summary>
    /// All tools available from the Terminal MCP actuator.
    /// TYPE 2 (RunReadOnlyCommand) — safe for Planner reconnaissance.
    /// TYPE 1 (RunCommand) — Orchestrator gate required.
    /// </summary>
    IReadOnlyList<AITool> TerminalTools { get; }

    /// <summary>
    /// All tools available from the Playwright MCP actuator.
    /// TYPE 2 (NavigateTo, GetPageContent, TakeScreenshot) — reconnaissance.
    /// </summary>
    IReadOnlyList<AITool> PlaywrightTools { get; }

    /// <summary>
    /// Aggregated TYPE 2 (read-only) tools from all actuators.
    /// Safe to inject into Planner, Checker, Reflector.
    /// </summary>
    IReadOnlyList<AITool> AllType2Tools { get; }

    /// <summary>
    /// Full tool set from all actuators.
    /// Use only for the Maker, which may dispatch TYPE 1 with Orchestrator approval.
    /// </summary>
    IReadOnlyList<AITool> AllTools { get; }
}