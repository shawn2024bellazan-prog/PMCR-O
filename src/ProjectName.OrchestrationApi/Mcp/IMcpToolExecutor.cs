// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Mcp/IMcpToolExecutor.cs
// Identity   : TYPE 1 Gate Executor Contract
// Law Anchor : ARCH-NEW-001, ARCH-MCP-NAT-001
// ThoughtLock: 2026-05-05 → MAF+MCP
//
// ARCHITECTURE NOTE:
//   ExecuteType2Async() has been REMOVED from this contract.
//   TYPE 2 (read-only reconnaissance) tool calls are now executed natively
//   by MAF's ChatClientAgent agentic loop — agents receive MCP tools as
//   AITool objects in ChatClientAgentOptions.Tools and MAF handles the
//   invocation lifecycle automatically.
//
//   ExecuteType1Async() is RETAINED — mutative operations (WriteFile, RunCommand,
//   etc.) must still pass through explicit Orchestrator gate control before
//   physical execution. The Orchestrator emits a McpDispatchDecision, the
//   DispatchExecutor validates it, and only then calls ExecuteType1Async().
//   This preserves the ARCH-NEW-001 TYPE 1 governance semantics.
// ═══════════════════════════════════════════════════════════════════════════════

using ProjectName.OrchestrationApi.Models;

namespace ProjectName.OrchestrationApi.Mcp;

/// <summary>
/// I AM the IMcpToolExecutor.
/// I define the contract for TYPE 1 (mutative) tool dispatch via the Orchestrator gate.
/// TYPE 2 (read-only) calls are handled natively by MAF's agentic tool loop.
/// </summary>
public interface IMcpToolExecutor
{
    /// <summary>
    /// Executes a mutative (TYPE 1) tool call from a validated Maker dispatch decision.
    /// Must only be called after Orchestrator gate approval — never from phase agents directly.
    /// </summary>
    Task<McpToolResult> ExecuteType1Async(
        McpDispatchDecision decision,
        CancellationToken ct = default);
}