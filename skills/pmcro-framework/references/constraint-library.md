# PMCRO Constraint Library — Authoritative Earned Laws
// ThoughtLock: 2026-05-06 → Synchronizing laws EC-001 through EC-005

## [EC-001] — Economic Gate Verb Coverage
**Fracture:** FRAC-ECONOMIC-GATE-001
**Law:** I ALWAYS treat discovery and analysis verbs as economically justified. 
**Allowlist:** `list`, `find`, `read`, `show`, `summarise`, `summarize`, `search`, `get`, `check`, `review`, `analyse`, `analyze`, `generate`, `describe`, `report`.

## [EC-002] — Dispatch Boundary Integrity
**Fracture:** FRAC-MAKER-TYPE1-001
**Law 1:** I NEVER put TYPE 2 tools (reads) in `dispatch_decisions`.
**Law 2:** I ALWAYS route `RunCommand` to the `terminal` actuator, NEVER `filesystem`.
**Law 3:** I ALWAYS use empty `dispatch_decisions: []` for analysis/reporting goals.

## [EC-003] — Cognitive-Only Success Pattern
**Fracture:** FRAC-CHECKER-COGNITIVE-001
**Law 1:** I ALWAYS skip physical disk verification on cognitive-only cycles.
**Law 2:** A composite score >= 0.85 on a cognitive-only cycle MUST result in an `ACCEPT` verdict.

## [EC-004] — Membrane Output Robustness
**Fracture:** FRAC-OUTPUT-001
**Law:** I ALWAYS use multi-property dynamic extraction (`Result`, `Data`, `Value`, `Output`) when retrieving a `CycleResult` from the workflow membrane to ensure output reliability regardless of SDK variation.

## [EC-005] — Semantic Documentation Law
**Law 1:** I ALWAYS decorate public and internal members with DocFX-compliant XML documentation.
**Law 2:** Every class summary MUST begin with "I AM the...".
**Law 3:** Every non-trivial member MUST include Law Anchors (e.g., ARCH-NNN or EC-NNN) in `<remarks>`.