---
title: "Module 01 — PMCRO Fundamentals"
---

# Module 01 — PMCRO Fundamentals

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchor: P1–P11, EC-001–EC-005*

---

## The Eleven Primitives

These are not features. They are axioms. Every design decision in the substrate can be traced back to one or more of these eleven statements. When you encounter something in the code that seems overcomplicated, or something that seems missing, find the primitive it violates and the design will make sense.

---

### P1 — Seed Intent vs True Intent

Every human input is a seed. Seeds are noisy. They contain what the human typed, which is a mixture of what they actually want, what they think they want, what they forgot to mention, and what they assumed you already knew. A seed is not an instruction. It is a starting point.

Inside every seed is a true intent — precise, actionable, resource-validated, stated in system-level terms. The Federation Board's only job is to distill seed into true intent before anything downstream runs.

This matters because if you let the seed drive the loop directly, the Planner will plan against the wrong goal. The Maker will build the wrong thing. The Checker will score the wrong artifact. The Reflector will crystallize the wrong lesson. Garbage in, garbage out, but across five phases and potentially a hundred tokens per phase.

The `FederationBoardSkill` implements P1. The `IntentEnvelope.HighLevelGoal` field is the true intent after refinement, and it is immutable — no phase agent ever changes it.

---

### P2 — Self-Reference as Enforcement

Everything in this system is flooded with self-reference. Every class summary begins with "I AM the...". Every constraint is written in first person: "I NEVER put TYPE 2 tools in dispatch_decisions." Every phase agent declares its identity in every output.

This is not style. It is enforcement.

When an agent declares "I AM the Checker — I score the Maker's work against the plan and physical evidence," it cannot in the same breath produce an artifact instead of a score. The identity declaration constrains the output space. When a constraint says "I ALWAYS skip physical verification on cognitive-only cycles," the agent cannot claim not to know the rule — it is written as the agent's own first-person behavioral commitment.

EC-005 is the formal codification of P2. Every class MUST begin its summary with "I AM the...". Every non-trivial method MUST have XML documentation. The law is not optional.

---

### P3 — The Cognitive Trail

A Trail is the full execution record of a loop run. It is composed of typed frames: the IntentEnvelope from the Federation Board, the ExecutionPlan from the Planner, the MakerFrame from the Maker, the DispatchResults from the Orchestrator gate, the QualityFrame from the Checker, and the ReflectorFrame from the Reflector.

The Trail is the product — not the artifact the Maker produced, not the score the Checker assigned. The Trail is what you actually own at the end of a cycle. It is transferable. It is auditable. It is licensable.

Think of it this way: a consultant who has done fifty engagements in a specific domain has expertise. That expertise is not in their head — it is in the accumulated decisions they made, the mistakes they caught, the constraints they learned. If you could package all of that into a data structure and load it into a new engagement, you would have the consultant's expertise without needing the consultant's calendar.

That is what the Trail does. The `CycleState` in `CycleModels.cs` is the in-memory representation of the Trail as it builds during a cycle. The `ReflectorFrame` at the end seals it.

---

### P4 — Constraint Accumulation as Learning

Each cycle earns at least one constraint. A fresh agent starts with the Colony Laws and nothing else. After fifty cycles on a specific domain, it has fifty more constraints — each one crystallized from a real event that happened in a real cycle.

The constraint set IS the codebase of Behavioral Intent Programming. It is not documentation alongside the code. It is not a comment in a config file. It is the actual behavioral logic of the system, written in first-person, injected into every subsequent cycle via the `ConstraintInjectorExecutor`.

EC-001 through EC-005 are the current earned constraints in the substrate. They did not come from theory. They came from fractures — real cycles that failed in specific ways, producing specific root causes, producing specific fixes, producing specific laws. The fracture log in `skills/pmcro-framework/references/fractures.md` is the source of truth.

---

### P5 — The PMCRO Loop

Seed → Federation Board → Orchestrator → Planner → Maker → Dispatcher → Checker → Reflector → route.

The routing options are: ACCEPT (cycle complete, Trail sealed), EXTEND (scope decomposed into another cycle), LOOP (Maker tries again with Checker's directives), ESCALATE (human judgment required).

The Orchestrator owns routing. No phase agent can route itself. The Checker can *emit* a verdict, but only the Orchestrator *acts* on it. This is the separation of judgment from action.

---

### P6 — TYPE 1 / TYPE 2 Tool Split

TYPE 2 tools observe the world. They are safe, reversible (or never-applied), and can be called by any phase agent at any time through the native MAF tool loop. `ReadFile`, `ListDirectory`, `GetFileInfo`, `FindFiles` on the filesystem actuator. `RunReadOnlyCommand` on terminal. `GetPageContent`, `GetPageTitle`, `GetPageUrl` on playwright.

TYPE 1 tools change the world. They are unsafe, irreversible, and can only be executed by the `DispatchExecutor` — the Orchestrator's arm. `WriteFile`, `DeletePath` on filesystem. `RunCommand` on terminal. `ClickElement`, `FillInput`, `WaitForElement` on playwright.

The split is not a permission system. It is an architectural guarantee. There is no code path by which a phase agent can fire a TYPE 1 call. `IMcpToolExecutor` is not injected into any phase agent. It does not appear in any skill. The only class that has a reference to it is `PmcroWorkflow.cs`, in the `DispatchExecutor` inner class.

This is the system's immune response to irreversible mistakes.

---

### P7 — Identity Injection and Behavioral Resilience

Autonomous agents operating in the real world get detected. Rate limits are hit. Sessions expire. Platforms update their DOM and selectors break. CAPTCHAs appear. Each of these is a detection event, and each detection event is a learning event.

The constraint it produces means the system never makes that exact mistake again. If the playwright actuator hits a CAPTCHA on `https://example.com`, that event produces a constraint: "I ALWAYS check for CAPTCHA presence before attempting form submission on example.com." That constraint is in the next cycle's IntentEnvelope.

Over cycles, the agent develops a calibrated behavioral fingerprint — not from more parameters, but from more constraints.

---

### P8 — Everything as Agent

Any artifact can become a living agent when PMCRO is applied to it. Feed a PDF as `master_context` in the IntentEnvelope. The loop reads it, derives from it, validates against it, and learns from it. Feed a codebase. Feed an API specification. Feed a company's documented processes.

The `IntentEnvelope.MasterContext` field is the injection point for this pattern. When the Federation Board seals an envelope with a master context, every downstream phase agent has access to that context in its prompt.

The PMCRO Framework `SKILL.md` file itself is an instance of P8 applied to the framework. The framework describes itself through the very loop it defines.

---

### P9 — HIL as Training Data

Every human correction at an escalation gate is worth triple the training weight of any automated signal. When the Orchestrator routes to ESCALATE and a human provides a correction, that correction is tagged as `hil_correction` in the ReflectorFrame and treated as high-confidence learning by the Reflector.

Over time, the escalation rate drops. Not because the system gets more parameters, but because it gets more constraints. It learns from human corrections and internalizes them.

---

### P10 — Behavioral Intent Programming

You are not writing code. You are writing intent. When you write a constraint — "I ALWAYS call `GetFileInfo` before `ReadFile` to verify the file exists" — you are programming behavior, not logic. Constraints replace conditionals. The Reflector replaces the compiler. The Cognitive Trail replaces version history.

This is what Behavioral Intent Programming means: the intent is the program, the constraints are the compiler, and the Trail is the executable.

---

### P11 — Trail Commercialization and Dependency Injection Licensing

The Trail is portable, licensable expertise. A Trail built by fifty cycles on a specific domain can be packaged and sold. The buyer receives the same learned expertise with their own identity injected via the Identity Injection pattern. The seller's Trail improves from every cycle the buyer runs. Each buyer runs on their own fork of the Trail.

This is the AI Agent Company's revenue model. Not SaaS. Not per-query. Trail licensing: you buy access to fifty cycles of accumulated expertise, you run your own cycles on top of it, and the ecosystem grows.

---

## The Colony Laws (EC-001 through EC-005)

These five laws are the substrate's earned constraints. They were not designed — they were crystallized from fractures. Each one represents a failure mode that the system no longer exhibits because the law is injected into every cycle.

**EC-001 — Economic Gate Verb Coverage.** The Federation Board's economic gate must pass discovery verbs — list, find, read, summarise, analyse — as well as mutative verbs. Before EC-001, the gate rejected analysis intents as not economically justifiable, which meant the loop could not be used for read-only tasks.

**EC-002 — Dispatch Integrity.** TYPE 2 tools — `ReadFile`, `ListDirectory`, `GetFileInfo`, `FindFiles`, `RunReadOnlyCommand` — must NEVER appear in the Maker's `dispatch_decisions`. They are called natively by phase agents via MAF. Putting them in dispatch decisions causes them to be routed to the `DispatchExecutor`, which only handles TYPE 1 calls, and they silently fail.

**EC-003 — Cognitive-Only Path.** When `dispatch_decisions` is empty and `dispatch_results` is empty, the Checker must NOT require physical verification. There is nothing to verify on disk. A composite score of 0.85 or higher on a cognitive-only cycle is sufficient for ACCEPT. Before EC-003, the Checker would loop indefinitely on analysis tasks because it could not confirm `ReadFile` evidence for files that were never written.

**EC-004 — Membrane Robustness.** Workflow output must be extracted using multi-property dynamic extraction — checking for `Result`, `Data`, and `Value` properties — rather than assuming a fixed output shape. Before EC-004, workflow outputs were extracted by position, and any variation in the MAF output shape caused silent null values.

**EC-005 — Semantic Documentation.** Every class MUST begin its XML summary with "I AM the...". Every non-trivial method MUST have XML documentation. The documentation is not decorative — it is the identity injection mechanism described in P2.

---

## The O-Mode Classification Table

Before any cycle begins, the Orchestrator classifies the intent into an O-Mode. The mode determines the loop topology — how many cycles to expect, whether to spawn sub-intents, whether to chain outputs.

| O-Mode | Pattern | Example |
|--------|---------|---------|
| O-Output | Single artifact, one cycle | Write a CHANGELOG entry |
| O-Optimize | Improve an existing artifact | Fix the timeout in retry logic |
| O-Orchestrate | Spawn a new sovereign entity | Create a new MCP actuator |
| O-Chain | Sequential pipeline, N feeds N+1 | Scaffold, wire, then register in Aspire |
| O-Tree | Parallel independent sub-intents | Build skills for all three agents at once |
| O-Graph | Complex DAG, 10+ cycles | Bootstrap the full PMCRO stack |

O-Graph intents that would require more than twenty cycles should trigger an ESCALATE so the human can decompose the scope before committing compute.

---

## How Constraints Are Written

A constraint is not a preference. It is not a guideline. It is an executable behavioral rule in first-person, specific, observable form.

Good constraints name an action, not a goal:

- `I ALWAYS call GetFileInfo before ReadFile to verify the file exists.`
- `I NEVER put ReadFile in dispatch_decisions — it is TYPE 2 and is called natively.`
- `I ALWAYS pass the overwrite argument as the string "true", never a bare boolean.`

Bad constraints name intentions, not behaviors:

- `I always try to do good work.` — Not observable. Not specific. Not enforceable.
- `I never make mistakes.` — Not earned. Not grounded. Not useful.

The test: can the Checker use this constraint to score a cycle output as compliant or non-compliant? If yes, it is a good constraint. If no, it is not a constraint — it is a wish.

---

> **Next:** [Module 02 — MAF to PMCRO Mapping](02-maf-mapping.md)
