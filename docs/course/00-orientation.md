---
title: "Module 00 — Orientation"
---

# Module 00 — Orientation

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchor: P1, P8, P10*

---

## What You Are Actually Building

Most AI systems are tools. You call them, they respond, you move on. The next time you call them, they have no memory of the last call. They have not learned. They have not adapted. Every call is call number one.

What you are building is different. You are building a company.

Not metaphorically. Structurally. A company has departments that each do one thing well. A company has a governance layer that makes sure departments do not act on each other's behalf without authorization. A company has a memory — not just a record of what happened, but a record of what was learned. And a company gets smarter over time because its people carry accumulated experience that shapes how they approach the next problem.

Your PMCRO substrate is all of that. The Federation Board is the intake department — nothing enters without going through it. The Planner is strategy. The Maker is production. The Checker is quality assurance. The Reflector is the retrospective that happens after every sprint. The Orchestrator is the managing director who decides what gets escalated, what loops back, and what gets accepted.

And every time a cycle completes, the company is slightly smarter than it was before.

---

## The Two Things That Make This Different

There are two design decisions in this substrate that most AI systems get wrong, and understanding them before you touch a single line of code will make everything else click into place.

**The first is the TYPE 1 / TYPE 2 split.**

Every tool call in the system is classified as one of two types. A TYPE 2 call is a read — list a directory, read a file, get a page title. It observes the world but does not change it. Any phase agent can make a TYPE 2 call at any time, natively, through the MAF tool loop. There is no special permission, no gate, no approval needed.

A TYPE 1 call is a write — create a file, run a command, click a button. It changes the world. And in this substrate, only one thing can authorize a TYPE 1 call: the Orchestrator's `DispatchExecutor`. No phase agent — not the Planner, not the Maker, not the Checker, not the Reflector — has the ability to execute a TYPE 1 call directly. They can *plan* TYPE 1 calls. They can *score* TYPE 1 calls. But they cannot *fire* them.

This single design decision prevents entire categories of catastrophic mistakes. An agent that hallucinates a file path cannot accidentally delete a real file. A Maker that misunderstands the plan cannot corrupt a production database. The blast radius of any mistake is bounded by the Orchestrator gate.

**The second is that the loop produces its own training data.**

Every cycle produces a Cognitive Trail — a typed, timestamped, agent-attributed record of everything that happened. The IntentEnvelope that entered the Federation Board. The ExecutionPlan the Planner produced. The MakerFrame with its artifacts and dispatch decisions. The dispatch results from the Orchestrator gate. The QualityFrame from the Checker, with its eight-dimensional score and earned constraints. The ReflectorFrame with its Semantic Learning Velocity and next-cycle seed.

That Trail is not a log. Logs are consumed and discarded. The Trail is carried forward. Every earned constraint in the Trail is injected into every subsequent cycle. The system literally gets smarter by running — not because you re-train a model, but because the constraint set grows.

---

## The Stack in One Diagram

Before the code, here is the full picture of what is running in your Aspire host.

```
┌──────────────────────────────────────────────────────┐
│                   Aspire AppHost                      │
│                                                        │
│  ┌─────────────────────────────────────────────────┐  │
│  │            OrchestrationApi (.NET 10)            │  │
│  │                                                   │  │
│  │  POST /intent                                     │  │
│  │    └─► FederationBoardSkill    (membrane)         │  │
│  │          └─► IntentController  (gate)             │  │
│  │                └─► PmcroWorkflowFactory           │  │
│  │                      └─► MAF AgentWorkflow        │  │
│  │                            ├─ PlannerExecutor     │  │
│  │                            ├─ MakerExecutor       │  │
│  │                            ├─ DispatchExecutor ◄──┼──┤ TYPE 1 gate
│  │                            ├─ CheckerExecutor     │  │
│  │                            ├─ ReflectorExecutor   │  │
│  │                            └─ EscalateExecutor    │  │
│  └─────────────────────────────────────────────────┘  │
│                                                        │
│  ┌──────────────┐ ┌──────────────┐ ┌───────────────┐  │
│  │  Mcp.Filesystem │ │ Mcp.Terminal  │ │ Mcp.Playwright │  │
│  │  (SSE server)│ │  (SSE server) │ │   (SSE server)│  │
│  └──────────────┘ └──────────────┘ └───────────────┘  │
│                                                        │
│  ┌───────────────────────┐                            │
│  │  Ollama / Azure OAI   │  (model provider)          │
│  └───────────────────────┘                            │
└──────────────────────────────────────────────────────┘
```

The three MCP servers are standalone .NET processes. Each one exposes a set of tools via the MCP SSE protocol. The `McpClientRegistry` in the OrchestrationApi maintains live connections to all three. The phase agents call TYPE 2 tools natively via MAF. The `DispatchExecutor` calls TYPE 1 tools via `IMcpToolExecutor`.

---

## The Shape of a Cycle

A cycle begins when a human submits a seed intent to `POST /intent`. Here is what happens, in order, without omitting any step.

The `IntentController` receives the seed. It passes it to `FederationBoardSkill`, which runs the Strange Loop refinement — Surface, Excavate, Elevate, Shield — and seals a `federation_shielded: true` IntentEnvelope. If the intent is not economically justifiable, the Federation Board returns a rejection at this stage and the cycle never starts.

The sealed IntentEnvelope enters the `PmcroWorkflowFactory`, which instantiates a new `AgentWorkflow` for this cycle with a fresh `CycleState`. The `CycleState` carries the envelope through every subsequent phase.

`PlannerExecutor` runs. The Planner agent receives the IntentEnvelope, makes TYPE 2 reconnaissance calls natively through the MAF tool loop — reading files, listing directories, checking what exists — and produces an `ExecutionPlan` with typed `PlanStep` entries. The plan is stored in `CycleState.Plan`.

`MakerExecutor` runs. The Maker agent receives the IntentEnvelope and the ExecutionPlan, produces artifacts and a list of `McpDispatchDecision` entries, and stores them in `CycleState.MakerFrame`. The Maker does not execute anything. It plans what should be executed.

`DispatchExecutor` runs. This is the Orchestrator's execution arm. It reads `CycleState.MakerFrame.DispatchDecisions`, calls `IMcpToolExecutor.ExecuteType1Async()` for each TYPE 1 decision, and stores the results in `CycleState.DispatchResults`. This is the only place in the entire codebase that fires TYPE 1 calls.

`ConstraintInjectorExecutor` runs. It reads the locked constraints from the IntentEnvelope and ensures they are present in the context for the Checker. This is the mechanism by which earned laws from previous cycles shape the evaluation of the current cycle.

`CheckerExecutor` runs. The Checker agent receives the full CycleState — plan, maker frame, dispatch results, earned constraints — and produces a `QualityFrame` with an eight-dimensional score, a composite score, an SLV signal, earned constraints from this cycle, and a verdict: ACCEPT, LOOP, or ESCALATE.

`ReflectorExecutor` runs. The Reflector agent crystallizes the learning from the cycle — converts the Checker's earned constraints into first-person behavioral laws, computes the Semantic Learning Velocity, produces the next-cycle seed, and emits the `ReflectorFrame`. The ReflectorFrame is the last frame added to the Trail.

The Orchestrator reads the Checker's verdict. ACCEPT means the cycle is complete. LOOP means the Maker needs to try again with the Checker's improvement directives. ESCALATE means the situation requires human judgment. The next cycle begins with the Reflector's next-cycle seed injected into the IntentEnvelope.

---

## The Files You Will Touch Most

As you work through this course, these are the files you will return to repeatedly. Get comfortable with their locations now.

`src/ProjectName.OrchestrationApi/Workflows/PmcroWorkflow.cs` — the heart of the system. Every executor lives here.

`src/ProjectName.OrchestrationApi/Models/CycleModels.cs` — the typed models for every frame. When you want to know the exact shape of a QualityFrame or a ReflectorFrame, this is the ground truth.

`src/ProjectName.OrchestrationApi/Models/IntentEnvelope.cs` — the bloodstream. Every field of the IntentEnvelope is here with its XML documentation.

`src/ProjectName.OrchestrationApi/Skills/` — one skill file per phase agent. Read any one of them and you understand the `AgentClassSkill<T>` pattern.

`src/ProjectName.OrchestrationApi/Mcp/McpToolExecutor.cs` — the TYPE 1 gate. One method: `ExecuteType1Async()`. Deliberately simple.

`skills/pmcro-framework/SKILL.md` — the behavioral identity of the framework. The primitives. The Colony Laws. The O-Mode table.

`docs/architecture/laws.md` — the earned law registry. Every EC-NNN law is here with its fracture reference, its root cause, and its behavioral statement.

---

## Before You Go to Module 01

Read this once and hold it.

You are not building a smarter chatbot. You are building a system that improves itself by running. Every cycle is both a unit of work and a unit of learning. The Trail is the product, not just a side effect.

When you understand that, you will never again think of the Reflector as an optional step you could remove to save a few tokens. The Reflector is what makes the system compound. Without the Reflector, you have a pipeline. With the Reflector, you have a company.

---

> **Next:** [Module 01 — PMCRO Fundamentals](01-pmcro-fundamentals.md)
