---
title: "PMCRO Deep Dive Course"
---

# PMCRO Deep Dive Course

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchor: ARCH-003, ARCH-027, P1–P11*

---

## What This Course Is

This course is a complete, production-grounded tour of the PMCRO AI Agent Company architecture as implemented in this substrate. It is not a theory course. Every concept is anchored to a real file, a real class, a real fracture that was resolved, or a real earned law that came out of a real cycle.

You will leave this course understanding not just *what* the system does but *why* every design decision was made — and more importantly, why the alternatives that weren't chosen would have caused fractures identical to the ones already in the fracture log.

This course is designed to be listened to. Every module is written in first-person, present-tense prose so that TTS is as natural as reading. You can work through it at a screen or let it read to you while you walk.

---

## What You Will Be Able to Do After This Course

After completing all ten modules, you will be able to:

- Explain the PMCRO loop and each of its phases from memory, using the exact terminology the code uses.
- Read any file in `src/ProjectName.OrchestrationApi/` and immediately know which PMCRO primitive or earned law it implements.
- Write a new `AgentClassSkill<T>` from scratch, following EC-005 and all law anchors, without consulting existing skills as a template.
- Add a new MCP actuator — filesystem, terminal, playwright, or something new — and wire it correctly into `McpToolExecutor` and `PmcroWorkflow`.
- Diagnose a cycle fracture: read the logs, identify which phase failed, determine whether it was a TYPE 1 / TYPE 2 confusion, a cognitive-only cycle misclassification, an economic gate rejection, or a context window truncation.
- Add a new earned law: record the fracture, ratify the law, inject it into `OrchestratorSkill`, and update `laws.md`.
- Turn any recurring prompt pattern into a first-class `AgentClassSkill` with a typed output contract and a DocFX-documented identity.
- Design new agents for personal use — calendar, workout, reflection — and understand exactly how they feed the Trail and train the professional company agents.

---

## Prerequisites

| Requirement | Why |
|-------------|-----|
| .NET 10 SDK | The substrate compiles and runs on .NET 10. |
| Aspire AppHost familiarity | `AppHost.cs` orchestrates all services. |
| Basic C# comfort | Skills, executors, and models are C# classes. |
| `docfx build` running locally | You are reading this in the site it builds. |
| The PMCRO Framework SKILL.md read at least once | Module 01 assumes you have seen the eleven primitives. |

You do not need deep MAF expertise. Module 02 covers everything you need from MAF in the context of how this substrate uses it.

---

## How the Modules Are Organized

The course follows the same shape as the loop itself.

**Module 00 — Orientation** sets up the mental model before touching any code. It answers the question "what are we actually building and why does it matter?"

**Module 01 — PMCRO Fundamentals** covers the eleven primitives and the Colony Laws in depth. This is the conceptual spine everything else hangs from.

**Module 02 — MAF to PMCRO Mapping** establishes the 1:1 correspondences between Microsoft Agent Framework primitives and PMCRO concepts. After this module, you read MAF documentation and PMCRO documentation in the same breath.

**Module 03 — The Federation Board** deep-dives `FederationBoardSkill.cs`, the `IntentEnvelope`, and the upstream membrane pattern. Nothing in the loop happens without passing through this.

**Module 04 — The Workflow Engine** opens `PmcroWorkflow.cs` and walks every executor in the graph — `PlannerExecutor`, `MakerExecutor`, `DispatchExecutor`, `CheckerExecutor`, `ReflectorExecutor`, `EscalateExecutor`, `ConstraintInjectorExecutor` — and explains the routing logic that connects them.

**Module 05 — The MCP Layer** covers the TYPE 1 / TYPE 2 tool split, the three actuators (filesystem, terminal, playwright), `McpToolExecutor`, and the economic and security guarantees this architecture provides.

**Module 06 — The Cognitive Trail** covers `CycleState`, every Frame type, how Trails are built across cycles, and the Trail commercialisation model from Primitive 11.

**Module 07 — Writing Skills** is a hands-on guide to the `AgentClassSkill<T>` pattern, `IHasInstructions`, `[AgentSkillResource]`, the EC-005 documentation law, and everything you need to write a new skill that integrates cleanly.

**Module 08 — Earned Laws** explains how constraints accumulate from fractures, how to write a constraint that the system can actually enforce, and the exact process from fracture discovery to law ratification.

**Module 09 — Prompt Engineering as Skills** takes your recurring prompt patterns — phrases you type over and over to get good output — and shows you how to crystallize them into first-class `AgentClassSkill` agents with typed output contracts.

**Module 10 — The AI Agent Company** is the capstone. It zooms out to the full vision: a self-improving multi-agent company, the motivation architecture, how personal-use agents feed professional agents, and how the Trail becomes transferable expertise.

---

> **Start here:** [Module 00 — Orientation](00-orientation.md)