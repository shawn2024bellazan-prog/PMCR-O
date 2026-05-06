---
title: "Earned Laws Registry"
---

# Earned Laws Registry

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · v1.0*

---

## What Earned Laws Are

Every constraint in this registry was produced by a real fracture. A cycle ran. Something failed at a specific point. The root cause was identified. The fix was applied. The law was crystallized so the same fracture cannot occur again.

Earned laws are not guidelines. They are executable behavioral rules written in first person by the system about itself. The word "I" is not decoration — it is the enforcement mechanism. An agent cannot read "I NEVER put TYPE 2 tools in `dispatch_decisions`" and then claim not to know the rule. The rule is stated as the agent's own identity.

These laws live in `OrchestratorSkill.cs` and `PmcroWorkflow.cs` as resources injected into every cycle. Every phase agent reads them at the start of every cycle via the earned-laws resource in the IntentEnvelope. A constraint crystallized in trail `121E0F` is present in trail `999ZZZ`.

---

## EC-001 — Economic Gate Verb Coverage

**Fracture:** FRAC-ECONOMIC-GATE-001  
**Trail:** 121E0F, Loop 0 (pre-cycle)  
**Symptom:** The Federation Board returned `ROUND_TABLE_NOT_ECONOMICALLY_JUSTIFIED` for intents containing `list`, `summarise`, and `analyse` because `RunEconomicGate`'s allowlist only contained mutative verbs. Read and analysis intents have bounded, cycle-closeable outcomes and must pass the gate.

**Laws:**
- I ALWAYS treat the following verbs as economically justified: `list`, `find`, `read`, `show`, `summarise`, `summarize`, `search`, `get`, `check`, `review`, `analyse`, `analyze`, `generate`, `describe`, `report`.
- I NEVER require a mutative outcome for a cycle to pass the economic gate.

---

## EC-002 — Dispatch Boundary Integrity

**Fracture:** FRAC-MAKER-TYPE1-001  
**Trails:** 121E0F, D90A76 (10 combined loops)  
**Symptom:** The Maker dispatched `ReadFile`, `ListDirectory`, and `RunCommand` (on the wrong actuator) as TYPE 1 tools in `dispatch_decisions` every loop. The gate rejected them correctly but the model never self-corrected because the prior dispatch results feedback was buried in an array and the skill had no explicit forbidden list.

**Laws:**
- I NEVER put `ReadFile`, `ListDirectory`, `GetFileInfo`, `FindFiles`, `RunReadOnlyCommand`, or `RunCommand` with actuator `filesystem` in `dispatch_decisions` — these are TYPE 2 or wrong-actuator and gate-reject every time.
- I ALWAYS put `RunCommand` on actuator `terminal`, never `filesystem`.
- I ALWAYS use empty `dispatch_decisions: []` for analysis and summarisation goals — the result goes in the `artifacts` array as a string.

---

## EC-003 — Cognitive-Only Success Pattern

**Fracture:** FRAC-CHECKER-COGNITIVE-001  
**Trail:** 0A3E2A (6 loops, composite 0.90 → LOOP)  
**Symptom:** The Checker scored composite 0.90 on a cognitive-only cycle (no `WriteFile`, no `dispatch_results`), then returned a LOOP verdict. The `@physical-verification` rule required `ReadFile` confirmation even when no files were written. The model could not satisfy the verification step and defaulted to LOOP despite its own score.

**Laws:**
- I ALWAYS skip physical disk verification on cognitive-only cycles — a cycle where `dispatch_decisions: []` AND `dispatch_results: []` has nothing to verify on disk.
- I NEVER call `ReadFile` as part of physical verification unless at least one `dispatch_result` has `tool: "WriteFile"` AND `success: true`.
- A composite score ≥ 0.85 on a cognitive-only cycle MUST result in an ACCEPT verdict — not LOOP.

---

## EC-004 — Membrane Output Robustness

**Fracture:** FRAC-OUTPUT-001  
**Symptom:** The `CycleResult` could not be read from the workflow membrane under certain MAF SDK version variations because the output was expected at a fixed property name that changed between builds.

**Law:**
- I ALWAYS use multi-property dynamic extraction (`Result`, `Data`, `Value`, `Output`) when retrieving a `CycleResult` from the workflow membrane — this ensures output reliability regardless of SDK variation.

---

## EC-005 — Semantic Documentation Law

**Source:** Crystallized from the identity injection principle (P2, P10) applied to the DocFX pipeline.

**Laws:**
- I ALWAYS decorate public and internal members with DocFX-compliant XML documentation.
- Every class `<summary>` MUST begin with "I AM the...".
- Every non-trivial member MUST include Law Anchors (e.g., `ARCH-NNN` or `EC-NNN`) in `<remarks>`.

---

## How New Laws Enter This Registry

1. A fracture occurs during a cycle run.
2. The fix is applied immediately.
3. The fracture is recorded in `skills/pmcro-framework/references/fractures.md` with: symptom, root cause, fix, service, trail ID, date.
4. The law is ratified: new behavior pattern → new law ID here. Extends existing → addendum to existing entry.
5. The law is added to `OrchestratorSkill.cs` as an earned-laws resource, injected into every future cycle.
6. The constraint locks into `IntentEnvelope.LockedConstraints` — the fracture cannot recur.

---

## See Also

- [What PMCRO Is](what-pmcro-is.md) — Primitive 4: Constraint Accumulation as Learning
- [The PMCRO Loop](pmcro-loop.md) — where laws are injected (Orchestrator) and crystallized (Reflector)
- `skills/pmcro-framework/references/fractures.md` — the full fracture record
