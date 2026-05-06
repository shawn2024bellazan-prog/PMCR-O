---
title: "Round Table Protocol"
---

# Round Table Protocol

*ThoughtLock: 2026-05-06 · Reference file for pmcro-framework skill*

---

## Speaking Order

EC-006 is absolute. Violations are protocol fractures.

```
Turn 0 — Orchestrator
    - Check skill library (EC-007)
    - Declare: GENERATIVE or REFINEMENT mode
    - Frame the problem domain
    - One turn. Done.

Turn 1 — Planner
    - Address: "Hey Orchestrator, ..."
    - State bare-minimum scope
    - Reference current CORE if refinement mode
    - Flag satellite risk
    - One turn. Done.

Turn 2 — Maker
    - Confirm buildability at stated scope
    - Name required MCP tools
    - Raise blockers only — no new scope
    - One turn. Done.

Turn 3 — Checker
    - Challenge the weakest assumption
    - Demand at least one constraint be named
    - Do not propose solutions — only surface gaps
    - One turn. Done.

Turn 4 — Reflector
    - Name the constraint this cycle earns
    - Confirm skill version readiness
    - Declare: "[Agent] vN ready to crystallise"
    - One turn. Done.

Consensus → Skill crystallises
```

---

## Transcript Schema

```json
{
  "round_table": {
    "mode": "generative | refinement",
    "domain": "string",
    "existing_skill_version": "string | null",
    "turns": [
      { "agent": "Orchestrator", "content": "string" },
      { "agent": "Planner",      "content": "string" },
      { "agent": "Maker",        "content": "string" },
      { "agent": "Checker",      "content": "string" },
      { "agent": "Reflector",    "content": "string" }
    ],
    "output": {
      "skill_name": "string",
      "version": "string",
      "core_capabilities": ["string"],
      "mcp_tools": ["string"],
      "earned_constraints": ["string"]
    }
  }
}
```

---

## Fracture Log

**FRAC-RT-001 — Unconstrained dialogue**
Before EC-006, Round Table sessions without enforced speaking order produced negotiation loops. Maker revised scope after Checker challenge. Checker challenged revision. Planner adjusted. No consensus reached. Fix: EC-006 — one turn per agent, in order, no revisions mid-session.

**FRAC-RT-002 — Skill re-generation**
A Round Table was run for a Git Agent that already existed in the library. Produced a conflicting v1 with different founding constraints. Caused identity split — two Git Agents with the same name but different CORE. Fix: EC-007 — Orchestrator checks skill library before any agent speaks.

---

## Mode Decision Tree

```
Seed intent arrives
    ↓
Orchestrator: does skill exist for this domain?
    ├── NO  → GENERATIVE mode
    │         Agents speak skill into existence
    │         Reflector names founding constraints
    │         Skill v1 crystallises
    │
    └── YES → REFINEMENT mode
              Orchestrator reads current version
              Agents speak against existing CORE/EARNED
              Planner confirms CORE or declares extension
              Reflector earns one new EARNED constraint
              Skill vN+1 crystallises
```
