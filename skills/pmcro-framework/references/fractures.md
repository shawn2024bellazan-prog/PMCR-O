# PMCRO v4.0 — Orchestrator-Gated TYPE 1 Execution
## ThoughtLock: 2026-05-05 | Session: 009 (Part 2)

---

## Fracture Closed

### FRAC-007 — DispatchExecutor was a no-op stub
**Severity:** BLOCKING  
**Symptom:** `DispatchExecutor` cleared `state.DispatchResults` and returned. TYPE 1 tools
(WriteFile, RunCommand, etc.) were planned by the Maker, validated by the Checker, but
**never executed**. The Checker's physical verification always failed silently because no
file was ever written to disk. Every cycle that required a file write was structurally broken.

**Fix:** `DispatchExecutor` now injects `IMcpToolExecutor` and calls `ExecuteType1Async()`
for every `McpDispatchDecision` in `state.MakerFrame.DispatchDecisions`. Results populate
`state.DispatchResults` which the `CheckerExecutor` passes to the Checker agent in its
evidence payload.

**Law earned:** `CONTROLLER-002: I NEVER register DispatchExecutor without IMcpToolExecutor.
The Orchestrator gate is structurally empty without the executor it is meant to fire.`

---

## Architecture Clarified

```
COGNITIVE LAYER (reason, plan, score, crystallise — never touch the world):
  PlannerExecutor  → calls TYPE 2 MCP tools natively via MAF (ReadFile, ListDirectory)
  MakerExecutor    → calls TYPE 2 MCP tools natively via MAF, emits dispatch_decisions
  CheckerExecutor  → calls TYPE 2 MCP tools natively via MAF (ReadFile to verify)
  ReflectorExecutor→ crystallises constraints, no tool calls

EXECUTION LAYER (one accountable point — the Orchestrator's arm):
  DispatchExecutor → ONLY node with IMcpToolExecutor injected
                     Iterates dispatch_decisions → ExecuteType1Async() → populates dispatch_results
                     Gate rejects non-TYPE-1 tools before they fire
```

---

## Files Changed

| File | Change |
|---|---|
| `Workflows/PmcroWorkflow.cs` | `DispatchExecutor` now calls `IMcpToolExecutor.ExecuteType1Async()` per decision |
| `Skills/MakerSkill.cs` | `@type1-gate` sharpened — Maker knows its decisions WILL be executed |
| `Skills/CheckerSkill.cs` | `@dispatch-result-scoring` added — Checker scores against actual Orchestrator outcomes |
| `Program.cs` | `DispatchExecutor` DI comment clarifies it is the ONLY executor with `IMcpToolExecutor` |

## Files NOT Changed (already correct)

| File | Status |
|---|---|
| `Mcp/McpToolExecutor.cs` | ✓ Gate logic correct — rejects non-TYPE-1 tools |
| `Mcp/IMcpToolExecutor.cs` | ✓ Contract correct — `ExecuteType1Async()` only |
| `Mcp/McpClientRegistry.cs` | ✓ Correct — TYPE 2 split already implemented |
| `Controllers/IntentController.cs` | ✓ Correct — from previous session |
| `Skills/FederationBoardSkill.cs` | ✓ Correct — from previous session |
| `Skills/OrchestratorSkill.cs` | ✓ Correct — from previous session |
| `Skills/PlannerSkill.cs` | ✓ No change needed |
| `Skills/ReflectorSkill.cs` | ✓ No change needed |
| `Models/IntentEnvelope.cs` | ✓ No change needed |
| `Models/CycleModels.cs` | ✓ No change needed |

---

## New Laws Earned

| Code | Statement | Fracture |
|---|---|---|
| CONTROLLER-002 | I NEVER register DispatchExecutor without IMcpToolExecutor. | FRAC-007 |
| ARCH-035 | I NEVER treat a workflow executor as a stub. Every executor must perform its contract or it does not exist. | FRAC-007 |

Add to `skills/pmcro-framework/references/laws-with-provenance.md`.

---

## Full Data Flow (end-to-end, working)

```
POST /intent/submit { rawIntent: "create a README.md" }
  ↓
IntentController
  ↓
FederationBoardSkill.RefineAsync() → sealed IntentEnvelope (federation_shielded: true)
  ↓
PmcroWorkflowFactory.Build() → Workflow
  ↓
PlannerExecutor
  → [TYPE 2] ListDirectory, ReadFile (via MAF native loop)
  → PlannerResponse { truest_intent, plan: { steps } }
  ↓
MakerExecutor
  → [TYPE 2] ReadFile existing files if needed (via MAF native loop)
  → MakerFrame {
      artifacts: ["# README\n..."],
      dispatch_decisions: [{
        mcp: "filesystem",
        tool: "WriteFile",
        args: { relativePath: "README.md", content: "...", overwrite: "true" }
      }]
    }
  ↓
DispatchExecutor  ← ONLY executor with IMcpToolExecutor
  → IMcpToolExecutor.ExecuteType1Async({ mcp: filesystem, tool: WriteFile, ... })
    → HTTP POST /mcp → projectname-mcp-filesystem → WriteFile fires on disk
  → state.DispatchResults = [{ success: true, content: "Written", mcp: "filesystem", tool: "WriteFile" }]
  ↓
CheckerExecutor
  → [TYPE 2] ReadFile("README.md") via MAF — confirms file exists on disk
  → QualityFrame { verdict: "ACCEPT", correctness: 0.9, completeness: 0.9, ... }
  ↓
ConstraintInjectorExecutor
  → merges earned_constraints into Envelope.LockedConstraints
  ↓
Switch → ACCEPT → ReflectorExecutor
  → ReflectorFrame { crystallised_constraint, next_cycle_seed, locked_cot }
  ↓
CycleResult.Accepted → HTTP 200
```