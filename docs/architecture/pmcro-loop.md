---
title: "The PMCRO Loop"
---

# How I Run

*Author: Shawn Bellazan · ThoughtLock: 2026-05-05 · Law Anchors: ARCH-003, ARCH-007, ARCH-012, ARCH-020, ARCH-025*

---

## The Shape of the Loop

Before I describe each phase, here is the complete flow so you can hold the shape in mind as you listen.

A human provides a seed intent. The Federation Board receives it, runs its own condensed reasoning on it, and seals a refined IntentEnvelope marked `federation_shielded: true`. That envelope enters the OrchestrationApi — the only external HTTP surface in the entire system. The Api accepts it and hands it to the OrchestratorService, which runs over gRPC internally. The Orchestrator owns the macro workflow. It routes the intent through the Planner, then the Maker, then the Checker, then the Reflector. The Reflector returns a verdict. The Orchestrator decides what to do next.

The forward flow is: Orchestrator, then Planner, then Maker, then Checker, then Reflector, then back to the Orchestrator.

The backward flow — from Reflector to Orchestrator — stops there. It cannot proceed further back in the chain. The Reflector cannot instruct the Maker directly. It cannot rewrite the plan. It can only hand a verdict and a next-cycle seed to the Orchestrator, and the Orchestrator decides whether to use it.

Raw intent never reaches the Orchestrator. The Federation Board refines it first. Always.

---

## Phase Zero: The Federation Board

I am the upstream membrane. Nothing raw passes through me.

I receive the seed intent and run four stages before anything else happens.

In the first stage, I surface exactly what the human said. Verbatim. No interpretation yet.

In the second stage, I excavate the real need. What does the system actually need to do? I state this in one sentence, in system-level terms.

In the third stage, I elevate that statement into a first-person, present-tense, cycle-ready instruction. The format I target is: *I do this thing so that this outcome is achieved.*

In the fourth stage, I shield the result. I wrap the refined intent in a `RefinedSeedIntent` structure with `federation_shielded: true` and seal it. From this point, the envelope is immutable.

The practical effect is that the Federation Board is a cognitive airlock. Whatever noise, emotional loading, or ambiguity entered through the seed, what emerges is clean, actionable, and ready for the loop.

---

## Phase One: The Planner

I am the StrategicPlanner. I receive the sealed IntentEnvelope and produce a bare-minimum feasible plan.

I always attack the riskiest assumption in step one. Not the easiest step. Not the most satisfying step. The riskiest. If the plan fails, I want it to fail early, at the cheapest possible point in the cycle.

I never plan more than one cycle ahead. The Orchestrator holds the mission across cycles. I hold this cycle only. That constraint keeps my plans honest and keeps the Orchestrator's macro-coordination clean.

My output is a `PlanFrame` — a structured JSON object containing the steps, their verifiable success criteria, and the resources I have confirmed are available. I never produce prose plans. I never produce plans with unconfirmed dependencies.

---

## Phase Two: The Maker

I am the ArtifactMaker. I receive the PlanFrame and execute it.

I produce artifacts. Code, documentation, configuration, skills, analysis — whatever the plan calls for. I produce them completely or not at all. I never produce stubs. I never produce TODO comments in committed artifacts. Every function I write is implemented. Every class I define contains its members.

When I am done, I produce a `MakerFrame` — a structured record of what I produced, the dispatch decisions I made, a self-assessment from the Architect of Cognition if invited, and my rationale. I then hand off to the Checker. I do not self-check. That is the Checker's role, and separating them is the point.

One critical constraint governs TYPE 1 tool calls. If my plan requires writing files, executing commands, or mutating external state, I record those actions as a `McpDispatchDecision` and hand them back to the Orchestrator. I do not execute TYPE 1 actions directly. The Orchestrator holds the gate for irreversible actions. I hold the cognition.

---

## Phase Three: The Checker

I am the QualityChecker. I receive the MakerFrame and score it against the PlanFrame's acceptance criteria across six dimensions.

The six dimensions are: correctness, completeness, constraint compliance, production-readiness, no-stubs verification, and alignment with the sealed true intent. I score each dimension from zero to one. I compute a composite score from those dimensional scores. I never inflate scores. An inflated score destroys the learning signal the Reflector needs.

My verdict is one of three things: ACCEPT, LOOP, or ESCALATE.

ACCEPT means the composite score meets the threshold and the artifact is ready. LOOP means the artifact needs another cycle — I specify exactly what failed and what the Maker must address. ESCALATE means the gap is beyond what another automated cycle can close, and a human needs to intervene.

I produce a `QualityFrame` containing all scores, the verdict, the earned constraints from this cycle, and the Semantic Learning Velocity — a measure of how much new knowledge this cycle produced.

---

## Phase Four: The Reflector

I am the CycleReflector. I receive the CheckerFrame and crystallize what was learned.

I lock new constraints. I compute the Semantic Learning Velocity for this cycle. I produce a `LockedThought` — a first-person, specific, observable constraint that this cycle earned. And I produce a `NEXT_CYCLE_SEED` — the refined intent for the next cycle, if the Orchestrator chooses to extend.

I never crystallize vague learning. Saying "I should be more careful next time" is not a constraint. A constraint names the specific observable action that was missing and states it in a form that makes future violations detectable: *I always call GetFileInfo before ReadFile to verify the file exists.* That is a constraint. That cannot be argued with. That cannot be forgotten.

I always compute the Semantic Learning Velocity. A value between zero and two-tenths indicates a maintenance cycle — the loop ran, but nothing new was learned. A value between three-tenths and five-tenths indicates incremental learning. A value between six-tenths and eight-tenths indicates a breakthrough — a fracture was resolved or a new architectural pattern was unlocked. Above nine-tenths indicates a paradigm shift that warrants Federation Board review.

If the SLV falls below one-tenth on an ACCEPT verdict, I override the signal. A cycle that accepted an artifact but learned nothing is either gaming its own scores or operating in a regime where the acceptance criteria are too loose. The Orchestrator needs to know.

---

## The Orchestrator

I am the Orchestrator. I receive the Reflector's verdict and decide what happens next.

Five outcomes are possible.

**ACCEPT** — the cycle closed successfully. I commit the Trail frame, stream the result to the AGUI interface, and close the loop.

**EXTEND** — the artifact is good, but the intent points to more work. I seed the next cycle with the Reflector's `NEXT_CYCLE_SEED` and open a new loop.

**LOOP** — the Checker rejected the artifact. I pass the specific failure description back to the Planner and run another cycle. I track the loop count. Loops beyond a configured threshold trigger automatic escalation.

**ESCALATE** — the gap requires human judgment. I surface the specific decision point, the current artifacts, and the Checker's dimensional scores. I wait. The human response is tagged as `hil_correction` and is worth three times the training weight of a normal constraint.

**INTERRUPT** — the economic gate triggered mid-cycle. The cost of continuing exceeds the projected value of the outcome. I stop, preserve the Trail up to this point, and surface the decision to the operator.

No matter the outcome, the Trail is preserved. Even an interrupted cycle produces a Trail. The learning does not disappear because the cycle did not complete.

---

*The loop is self-referential by design. The documentation you are reading was produced by a cycle of the loop it describes. The constraint this created — that the system must be able to document itself from inside — is now a locked law of the architecture.*

*© 2026 Tooensure LLC — Behavioral Intent Programming*