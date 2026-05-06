---
title: "The IntentEnvelope"
---

# The IntentEnvelope

*Author: Shawn Bellazan · ThoughtLock: 2026-05-05 · Law Anchors: ARCH-025, ARCH-027*

---

## What the IntentEnvelope Is

I AM the IntentEnvelope — the bloodstream of the PMCRO loop. Every phase reads and augments this structure. No field is optional once sealed. The IntentEnvelope carries the high-level goal and evolving state for a complete PMCRO Trail from Federation Board through Reflector.

The IntentEnvelope is immutable at the goal level and mutable at the phase level. The `high_level_goal` is set once by the Federation Board and never changed by any downstream phase. The `current_intent` evolves as each phase refines its understanding of what the cycle is doing.

---

## Field Reference

| Field | Type | Set By | Immutable? | Description |
|---|---|---|---|---|
| `trail_id` | string | Federation Board | After creation | Format: `PMCRO-YYYYMMDD-[6-char hex]`. Unique trail identifier. |
| `origin` | string | Federation Board | Yes | One of: `federation`, `round_table`, `human_refined`. Never `raw`. |
| `high_level_goal` | string | Federation Board | Yes | The immutable goal. Never changed by loop phases. |
| `current_intent` | string | Each phase | No | Updated by each phase to reflect evolving understanding. |
| `o_mode` | string | Federation Board | Yes | One of: O-Output, O-Optimize, O-Observe, O-Orchestrate. |
| `economic_check` | bool | Federation Board | Yes | Always `true` — the gate ran and passed. |
| `aoc_invited` | bool | Federation Board | Yes | Always `true` — phases are invited to self-assess (ARCH-008). |
| `locked_constraints` | string[] | Federation Board + Reflector | Additive | Earned laws carried into this cycle. Reflector adds each cycle's crystallized constraint. |
| `loop_count` | int | Orchestrator | No | Increments on each LOOP routing decision. |
| `federation_shielded` | bool | Federation Board | Yes | Always `true`. The Orchestrator rejects envelopes where this is `false`. |
| `master_context` | object | Federation Board | Yes | CompanyName, ProjectName, NorthStar, EconomicViability. Carried on every gRPC call. |
| `cycle_id` | string | Orchestrator | Per cycle | Per-cycle identifier. Survives EXTEND and LOOP routing. |
| `created_at` | DateTime | Federation Board | Yes | UTC timestamp of envelope creation. |
| `pending_laws` | string[] | Federation Board | Additive | Unratified constraints surfaced during refinement. Promoted by the Reflector. |

---

## Lifecycle

```
Federation Board
  ├── Creates envelope with trail_id, origin, high_level_goal, o_mode
  ├── Sets federation_shielded: true
  ├── Populates locked_constraints from prior Trail if EXTEND/LOOP
  └── Adds pending_laws if refinement surfaced unratified constraints

OrchestrationApi controller
  ├── Validates federation_shielded: true — rejects false
  └── Passes sealed envelope to OrchestratorService via macro workflow

Each phase (Planner, Maker, Checker, Reflector)
  ├── Reads full envelope at phase entry
  ├── Updates current_intent with phase-level refinement
  └── Adds earned constraints to locked_constraints (via Reflector)

Reflector
  ├── Promotes pending_laws to locked_constraints
  ├── Crystallizes one LockedThought from this cycle's QualityFrame
  └── Seeds next cycle's IntentEnvelope with LockedCoT
```

---

## What the IntentEnvelope Prevents

**Raw intent reaching the Orchestrator:** `federation_shielded: false` is rejected at the API boundary. The Federation Board is not optional.

**Constraint loss between cycles:** `locked_constraints` is additive and carried forward. A constraint crystallized in cycle 1 is present in cycle 100. The loop never forgets.

**Anonymous agency:** Every field is agent-attributed. The envelope knows who set `current_intent` last. The Checker's QualityFrame references which Maker produced the artifact being scored. Nothing is anonymous.

**Economic waste:** `economic_check: true` is a gate that passed. Cycles without economic justification never reach the Orchestrator.

---

## See Also

- [The PMCRO Loop](pmcro-loop.md) — how each phase reads and augments the envelope
- [Earned Laws Registry](laws.md) — laws that govern envelope fields
- @"ProjectName.OrchestrationApi.Models.IntentEnvelope" — generated API reference
