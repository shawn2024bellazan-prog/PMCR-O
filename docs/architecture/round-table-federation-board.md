---
title: "The Round Table Federation Board"
---

# The Round Table Federation Board

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchors: EC-006, EC-007, ARCH-RT-001, ARCH-RT-002*

---

## What Changed

The Federation Board was previously documented as a pre-processing gate — a membrane sitting *above* the loop that refined seed intent before handing it to the Orchestrator. That model was structurally incomplete.

The Federation Board is not above the loop. It **is** the loop agents themselves — activated simultaneously, reasoning in-character, speaking to each other about the seed before any external execution begins. The conversation between them *is* the intent distillation. The Round Table transcript *is* the skill.

This is P12 — Round Table Federation Board.

---

## Why This Model Is Correct

In the previous model, the Federation Board used the agents' wisdom without involving the agents. The Planner's bare-minimum instinct, the Checker's adversarial reflex, the Reflector's constraint-earning compulsion — these were implicit in the Board's output, but not explicit in its structure. They happened in a black box.

The Round Table makes that reasoning a first-class artifact. Each agent speaks once, in role, in sequence. The exchange is logged as a typed frame. The consensus is the true intent. Nothing is hidden.

This is consistent with P2 (Self-Reference as Enforcement) and P8 (Everything as Agent). If the framework claims everything is an agent and everything is flooded with self-reference, then the Federation Board itself must be expressed as agents in dialogue — not as a procedure that silently invokes their instincts.

---

## The Speaking Order

EC-006 governs the Round Table. Violation of speaking order is a protocol fracture.

| Turn | Agent | Responsibility |
|---|---|---|
| 0 | **Orchestrator** | Checks skill library. Frames problem space. Declares generative or refinement mode. |
| 1 | **Planner** | Speaks bare-minimum scope to Orchestrator. References current skill version if exists. Flags satellite risk. |
| 2 | **Maker** | Confirms buildability. Names required MCP tools. Raises blockers. |
| 3 | **Checker** | Challenges assumptions. Demands at least one constraint be named before proceeding. |
| 4 | **Reflector** | Names the constraint this cycle will earn. Confirms skill version readiness. |
| — | **Consensus** | Specialized Agent skill crystallised from transcript. Loop proceeds. |

Agents do not respond to each other outside their turn. The Planner does not react to the Checker. The Maker does not revise after the Reflector speaks. Identity contamination across turns is a fracture. Each agent speaks once, in role, and is done.

---

## Skill Library Lookup — EC-007

The Orchestrator's first action in every Round Table is the skill library lookup. This happens before any agent speaks.

```
Orchestrator checks: does a Specialized Agent skill exist for this domain?

YES → Round Table is REFINEMENT mode
      Agents speak against the existing skill version
      Planner references current CORE and EARNED layers
      Reflector targets one new EARNED constraint

NO  → Round Table is GENERATIVE mode
      Agents speak the skill into existence from scratch
      Reflector names the founding constraints
      Skill v1 crystallises from the transcript
```

EC-007 is absolute: **I NEVER re-generate a Specialized Agent skill that already exists in the library.** Re-generation wastes cycles and produces skill drift — two versions of the same agent with different founding constraints.

---

## The Transcript Is the Skill

This is the architectural insight that defines P12.

In generative mode, the Round Table agents are not deciding what to build and then writing a skill file. They are speaking the skill into existence through their exchange. By the time Reflector names the founding constraints, the skill is already written — the transcript *is* the SKILL.md skeleton.

**Example — Git Agent, first invocation (generative mode):**

> **Orchestrator:** We need version control operations. No Git Agent exists in the skill library. Round Table is generative.
>
> **Planner:** Hey Orchestrator — bare minimum this cycle: commit, push, branch, status. Nothing else. Satellite risk: pushing to main directly. I ALWAYS confirm branch before any write.
>
> **Maker:** I can build this. Required tools: `git_commit`, `git_push`, `git_status`, `git_branch`. No blockers at current scope.
>
> **Checker:** What stops this agent from pushing to main? That constraint must be named before we proceed. What is the branch verification protocol?
>
> **Reflector:** This cycle earns: *I NEVER push directly to main. I ALWAYS verify current branch before any write operation.* Git Agent v1 is ready to crystallise.

The Planner's statement became the CORE capability list. The Checker's challenge became the founding constraint question. The Reflector's output became the first EARNED entry. The transcript *is* the skill file. Nothing was written separately.

---

## Refinement Mode

When a skill already exists, the Round Table does not regenerate it. Agents speak *against* the current version — they read what the skill already knows and speak only what this cycle adds.

**Example — Git Agent, cycle 7 (refinement mode):**

> **Orchestrator:** Version control operations needed. Git Agent v6 exists in skill library. CORE: commit, push, branch, status. EARNED: 5 constraints from prior cycles. Round Table is refinement.
>
> **Planner:** Hey Orchestrator — v6 covers current CORE. This cycle we also need git workflow branching for the feature release pattern. Extending CORE scope.
>
> **Maker:** Confirmed. Adding `git_merge` and `git_rebase` to tool list. No blockers at this scope.
>
> **Checker:** Rebase on shared branches is dangerous. What is the remote tracking check? That constraint must be named.
>
> **Reflector:** Earned: *I NEVER rebase branches that other agents have checked out. I ALWAYS check remote tracking state before any rebase operation.* Git Agent v7 ready.

The skill grows by one earned constraint per cycle. v7 is better than v6 the same way iOS 18 is better than iOS 17 — the update ships, the device improves, the previous version is archived in the Trail.

---

## What the Round Table Produces

At the end of every Round Table, exactly one thing is produced: a Specialized Agent skill ready to crystallise or update. This artifact contains:

- **Identity declaration** — `I AM the [Name] Agent.`
- **CORE layer** — capabilities established in this Round Table (generative) or confirmed unchanged (refinement)
- **EARNED layer** — constraints named by the Reflector, appended to prior EARNED entries
- **Tool manifest** — MCP tools declared by the Maker
- **Version tag** — incremented from prior version, or v1 if generative

The Orchestrator then proceeds with the standard loop, driven by this Specialized Agent.

---

## Laws Anchored Here

| Law | Statement |
|---|---|
| **EC-006** | I NEVER allow agents to speak out of role or out of order during a Round Table. Orchestrator frames first, then Planner, Maker, Checker, Reflector — one turn each. |
| **EC-007** | I ALWAYS check the skill library before the Round Table speaks. I NEVER re-generate a Specialized Agent skill that already exists. |
| **PMCRO-LAW-006** | I ALWAYS check the skill library before speaking a Specialized Agent into existence. |
| **PMCRO-LAW-007** | I NEVER allow agents to speak out of role or out of order during a Round Table. |
| **PMCRO-LAW-008** | I NEVER re-generate a Specialized Agent skill that already exists in the library. |

---

## See Also

- [Specialized Agent Lifecycle](specialized-agent-lifecycle.md) — Spawn protocol, CORE/EARNED anatomy, versioning, Identity Injection
- [The PMCRO Loop](pmcro-loop.md) — How the Round Table feeds the standard loop
- [Earned Laws Registry](laws.md) — EC-006, EC-007 fracture history
- [Module 12 — Round Table](../course/12-round-table.md) — Step-by-step course module
