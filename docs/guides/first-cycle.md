---
title: "First Cycle Walkthrough"
---

# First Cycle Walkthrough

*Author: Shawn Bellazan · ThoughtLock: 2026-05-05*

---

## Prerequisites

- .NET 10 SDK installed (`dotnet --version` → 10.x)
- Aspire AppHost running (`dotnet run --project src/ProjectName.AppHost`)
- All MCP services registered and healthy in Aspire dashboard
- `Parameters:project-root` configured in `appsettings.Development.json`

## What the End of This Guide Looks Like

A complete PMCRO cycle has run from raw seed intent through Federation Board → Orchestrator → Planner → Maker → Checker → Reflector, returned an ACCEPT verdict, and produced at least one crystallized `LockedThought` in the Trail.

---

## Steps

### 1. Confirm the Aspire Host Is Running

```bash
dotnet run --project src/ProjectName.AppHost
```

Navigate to the Aspire dashboard (default: `https://localhost:15888`). Confirm all services show healthy:

- `projectname-orchestrationapi`
- `projectname-mcp-filesystem`
- `projectname-mcp-terminal`
- `projectname-mcp-playwright` (if configured)

### 2. Submit a Seed Intent

The OrchestrationApi is the sole external HTTP surface (ARCH-012). Submit a seed intent via the `/api/intent` endpoint:

```bash
curl -X POST https://localhost:[port]/api/intent \
  -H "Content-Type: application/json" \
  -d '{ "seed_intent": "List all SKILL.md files in the skills directory" }'
```

The API returns `202 Accepted` with a `trail_id`. This is request/forget — the cycle runs asynchronously (FL-EXT-002).

```json
{ "trail_id": "PMCRO-20260505-A3F9C1", "status": "accepted" }
```

### 3. Observe the Federation Board Refinement

In the CopilotKit AGUI (if running) or via the Aspire structured logs, observe the Federation Board running the four stages:

```
SURFACE:   "List all SKILL.md files in the skills directory"
EXCAVATE:  Enumerate all agent skill definition files in the workspace skills tree.
ELEVATE:   I list all SKILL.md files under skills/ so that the full agent skill inventory is known.
SHIELD:    { federation_shielded: true, o_mode: "O-Observe", economic_check: true }
```

EC-001 applies: `list` is an economically justified verb. The gate passes.

### 4. Observe the Planner

The Planner surfaces the truest intent and produces an ExecutionPlan. For this intent, the plan is cognitive-only:

```
dispatch_decisions: []    ← no world changes needed
artifacts: [directory listing as structured string]
```

The Planner calls `ListDirectory` via `Mcp.FileSystem` (TYPE 2, read) natively during the MicroWorkflow. This call does not appear in `dispatch_decisions` — EC-002 applies.

### 5. Observe the Maker

The Maker produces the artifact — a structured listing of all `SKILL.md` files with their paths and names. `dispatch_decisions` is empty. The Orchestrator's DispatchExecutor finds nothing to execute.

### 6. Observe the Checker

The Checker scores the artifact. EC-003 applies: this is a cognitive-only cycle (`dispatch_decisions: []`, `dispatch_results: []`). Physical disk verification is skipped. The composite score reflects:

- Correctness: does the listing match the directory structure?
- Completeness: are all planned steps addressed?
- Compliance: EC-001 through EC-005 respected?

If composite ≥ 0.85, the verdict is ACCEPT — not LOOP.

### 7. Observe the Reflector

The Reflector crystallizes the constraint earned this cycle and seeds the next cycle. For a well-formed cognitive-only cycle, the SLV is high (new information produced) and the LockedThought is concise:

```json
{
  "locked_thought": "A ListDirectory call on skills/ returns N SKILL.md files.",
  "slv": 0.72,
  "verdict": "ACCEPT"
}
```

---

## Verification

Confirm the cycle completed by querying the trail:

```bash
curl https://localhost:[port]/api/trail/PMCRO-20260505-A3F9C1
```

Expect:
- `status: "accepted"`
- `frames` array with IntentEnvelope, ExecutionPlan, MakerFrame, QualityFrame, ReflectorFrame all present
- `verdict: "ACCEPT"` in the QualityFrame
- `locked_thought` in the ReflectorFrame

---

## Troubleshooting

**`federation_shielded: false` rejection at controller boundary**  
The intent was submitted directly to the Orchestrator, bypassing the Federation Board. Submit through `/api/intent` only — the controller calls `FederationBoardSkill` automatically.

**LOOP verdict on a cognitive-only cycle**  
EC-003 is not active in the Checker's earned-laws resource. Verify `OrchestratorSkill.cs` includes EC-003 in its resource injection. Verify the Aspire service was restarted after the last skill change.

**`ROUND_TABLE_NOT_ECONOMICALLY_JUSTIFIED` from Federation Board**  
EC-001 is not active. Verify the `RunEconomicGate` allowlist in `FederationBoardSkill.cs` includes `list` and analysis verbs. This fracture was resolved in trail 121E0F — if it recurs, check the build version.

---

## See Also

- [Adding an Earned Law](adding-earned-law.md) — if this walkthrough surfaces a new fracture
- [Architecture → The PMCRO Loop](../architecture/pmcro-loop.md) — phase-by-phase reference
- [Architecture → Earned Laws Registry](../architecture/laws.md) — active constraints
