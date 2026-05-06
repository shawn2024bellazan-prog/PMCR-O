---
title: "MCP Type Classification"
---

# MCP Type Classification

*Author: Shawn Bellazan · ThoughtLock: 2026-05-05 · Law Anchor: ARCH-NEW-001, EC-002*

---

## The Accountability Boundary

The PMCRO substrate draws one hard line through all tool use: **does this tool change the world?**

A tool that changes the world has side effects. Side effects need one accountable point. That point is the Orchestrator. No phase agent — Planner, Maker, Checker, Reflector — ever calls a world-changing tool. The Maker reasons about WHAT to execute and returns a `McpDispatchDecision`. The Orchestrator executes it.

A tool that informs the mind has no side effects. It extends cognition. Phase agents call these directly during their own MicroWorkflows.

This is not a performance constraint. This is the accountability architecture. When a file is written, the system knows exactly one agent is responsible: the Orchestrator, acting on a decision produced by the Maker.

---

## TYPE 1 — Execution MCPs (World-Changing)

**Caller:** OrchestratorService only, via `IMcpToolExecutor.ExecuteType1Async()`.  
**No phase agent may call these. `IMcpToolExecutor` is not injected into phase agents.**

| MCP | Tools | What changes |
|---|---|---|
| `Mcp.FileSystem` (write mode) | `WriteFile`, `DeletePath` | Files on disk |
| `Mcp.Terminal` | `RunCommand` | Shell state, spawned processes |
| `Mcp.ProjectManagement` | `SpawnAgent` | New sovereign Console agent projects |

**EC-002 applies:** `dispatch_decisions` contains only TYPE 1 tools. `RunCommand` routes to the `terminal` actuator only — never `filesystem`. For cognitive-only cycles, `dispatch_decisions` is always empty (`[]`), never absent.

---

## TYPE 2 — Cognitive Tool MCPs (Reasoning Aids)

**Caller:** Phase agents, called natively during their MicroWorkflow.  
**These extend cognition. They do not change the world.**

| Service | TYPE 2 MCP | Purpose |
|---|---|---|
| Planner | `Mcp.FileSystem` (read) | `ReadFile`, `ListDirectory`, `GetFileInfo` — understand the workspace |
| Planner | `Mcp.Playwright` | Deep web research — runs its own internal PMCRO loop |
| Maker | `Mcp.Roslyn` | Compile and validate generated C# before returning dispatch decision |
| Maker | `Mcp.FileSystem` (read) | Read existing code as context |
| Checker | `Mcp.Ml` | Dimensional scoring trained on CognitiveTrail frames |
| Reflector | `Mcp.FineTuning` | Submit cycle data as a training signal |

---

## The Decision Test

Before classifying any tool call, apply this test in order:

1. **Does this call create, modify, or delete a file?** → TYPE 1 → Orchestrator only.
2. **Does this call execute a shell command or spawn a process?** → TYPE 1 → Orchestrator only.
3. **Does this call create or modify external system state (tickets, APIs, databases)?** → TYPE 1 → Orchestrator only.
4. **Does this call read, search, compile, score, or analyse without side effects?** → TYPE 2 → Phase agent calls natively.

If the answer is uncertain, classify as TYPE 1. The cost of misclassifying a TYPE 2 as TYPE 1 is a slightly slower loop. The cost of misclassifying a TYPE 1 as TYPE 2 is unaccountable world-changing actions from phase agents — a fracture.

---

## Mcp.Playwright — A Strange Loop Inside a Strange Loop

The Planner calls `Mcp.Playwright` with a research request. Playwright internally runs its own OBSERVE → PLAN → ACT → CHECK → REFLECT loop until the research is complete. The Planner never sees the inner loop. The inner loop's result surfaces as a completed research frame.

This is a Strange Loop inside a Strange Loop — the recursive PMCRO applied to a sub-task, invisibly, as a cognitive tool. The Planner's MicroWorkflow checkpoint fires before and after the Playwright call. The Planner cannot distinguish this from any other TYPE 2 tool call at the interface level.

---

## FileSystem MCP — Dual-Type Operation

`Mcp.FileSystem` operates in both TYPE classifications depending on the tool called:

| Tool | Type | Caller |
|---|---|---|
| `ReadFile` | TYPE 2 | Any phase agent |
| `ListDirectory` | TYPE 2 | Any phase agent |
| `GetFileInfo` | TYPE 2 | Any phase agent |
| `WriteFile` | TYPE 1 | Orchestrator only |
| `DeletePath` | TYPE 1 | Orchestrator only |

The sandbox boundary is absolute: all paths resolve relative to `FileSystemRoot`. No traversal outside this root succeeds regardless of TYPE classification.

---

## See Also

- [The PMCRO Loop](pmcro-loop.md) — where TYPE 1 dispatch executes (Phase 3.5)
- [Earned Laws Registry](laws.md) — EC-002: dispatch boundary integrity
- @"ProjectName.OrchestrationApi.Mcp.IMcpToolExecutor" — generated API reference
