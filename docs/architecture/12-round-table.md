---
title: "Module 12 — The Round Table Federation Board"
---

# Module 12 — The Round Table Federation Board

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchors: EC-006, EC-007, ARCH-RT-001*

---

## What This Module Covers

Module 03 documented the Federation Board as a four-stage refinement gate. That was correct for v1.2.0. This module supersedes the Federation Board section of Module 03 with the v1.3.0 model.

The Federation Board is not a gate. It is a Round Table — the loop agents themselves, activated simultaneously, reasoning in-character, speaking to each other about the seed before anything downstream runs.

---

## The Core Shift

In Module 03, the Federation Board ran four procedural stages: Surface, Excavate, Elevate, Shield. These stages were correct as a *description of what the output should be*. They were incomplete as a description of *how that output is produced*.

The output of the Federation Board has always been the result of agents reasoning in-character. The Planner's instinct was always in the scope statement. The Checker's adversarial reflex was always in the constraint challenge. The Reflector's compulsion was always in the founding constraint. The Round Table model makes this reasoning explicit, logged, and identity-accountable.

Nothing in the four-stage model is discarded. The stages now map to agent turns:

| Stage (Module 03) | Agent Turn (Module 12) |
|---|---|
| Surface — what did the human say? | Orchestrator — frames the domain |
| Excavate — what does the system need? | Planner — speaks bare-minimum scope |
| Elevate — cycle-ready instruction | Maker — confirms buildability |
| Shield — economic gate, seal envelope | Checker + Reflector — name the constraint, seal |

---

## Running Your First Round Table

You will recognise this from Module 03's four-stage example. Now you run it as a dialogue.

**Seed intent:** `"I need to automate my git workflow"`

**Step 1 — Orchestrator checks skill library.**

Is there a Git Agent in the library? If yes, Round Table is refinement. If no, Round Table is generative. For this example: no Git Agent exists. Round Table is generative.

**Step 2 — Round Table runs.**

> **Orchestrator:** We need version control automation. No Git Agent exists. Round Table is generative.
>
> **Planner:** Hey Orchestrator — bare minimum this cycle: commit, push, branch, status. No merge, no rebase, no hooks. Satellite risk: direct main pushes.
>
> **Maker:** I can build this. Required tools: `git_commit`, `git_push`, `git_status`, `git_branch`. No blockers at this scope.
>
> **Checker:** What prevents a direct main push? Name the constraint before we proceed.
>
> **Reflector:** Earning: *I NEVER push directly to main. I ALWAYS verify current branch before any write operation.* Git Agent v1 ready to crystallise.

**Step 3 — Skill crystallises.**

The transcript above is the Git Agent SKILL.md skeleton. The Planner's scope is CORE. The Reflector's output is EARNED. The Maker's list is the tool manifest. The Orchestrator registers it in the library.

**Step 4 — Loop proceeds.**

The standard PMCRO loop runs, now driven by the Git Agent. The cycle closes. The Reflector earns additional constraints. Git Agent v2 is written.

---

## The Speaking Order Is a Law

EC-006 is not a convention. It is a constraint earned from a fracture.

The fracture: in early Round Table sessions without enforced speaking order, the Maker would revise its tool list after the Checker challenged an assumption. This created a feedback loop — Checker challenges, Maker revises, Checker challenges again, Planner adjusts scope — that never reached consensus. The Round Table became a negotiation rather than a crystallisation.

EC-006 ends this. Each agent speaks once. In order. In role. The Checker's challenge stands even if the Maker could address it. The Reflector crystallises what exists at consensus, not what might exist after further negotiation. If the challenge is severe enough to block consensus, the Orchestrator escalates to HIL — it does not allow the Round Table to cycle.

---

## When You Already Have the Skill

After cycle 1, you have a Git Agent. Next time version control is needed, the Round Table does not regenerate it. This is EC-007.

The Orchestrator reads the current skill version at the start of the Round Table. Agents speak *against* it — they know what the agent already knows, and they speak only what this cycle adds.

This is refinement mode. The Planner does not re-state CORE capabilities. It confirms they still apply and names any extensions. The Reflector does not re-earn founding constraints. It earns one new constraint specific to this cycle's context.

The skill grows. The library stays clean. The cycle stays efficient.

---

## What You Should Be Able to Do After This Module

- Identify whether a Round Table should run in generative or refinement mode
- Run a Round Table in speaking-order sequence with agents in-character
- Recognise EC-006 violations (out-of-order speech, identity contamination between turns)
- Recognise EC-007 violations (re-generating an existing skill)
- Explain why the Round Table transcript is the skill file, not a precursor to it

---

## Exercises

**Exercise 1 — Classify the mode.**

You need a Playwright automation agent. Check your skill library. If `playwright-agent` exists, the Round Table is refinement mode. If not, it is generative. Run the appropriate Round Table.

**Exercise 2 — Spot the violation.**

Read the following Round Table fragment and identify the EC-006 violation:

> **Planner:** Bare minimum: read file, write file, list directory.
> **Maker:** I can build this. Required: `read_file`, `write_file`, `list_dir`.
> **Checker:** What about path traversal? The Maker should add a path validation constraint.
> **Maker:** Good point. Adding path validation to the tool call.

The violation: the Maker spoke a second time in response to the Checker. EC-006 prohibits this. The Checker's challenge stands. The Reflector names the constraint. The Maker does not revise mid-Round-Table.

**Exercise 3 — Write the skill skeleton.**

Given the following Round Table transcript, produce the SKILL.md v1 skeleton:

> **Orchestrator:** Email operations needed. No Email Agent exists. Generative mode.
> **Planner:** Bare minimum: send, read inbox, reply. No bulk send, no attachment handling this cycle.
> **Maker:** Required: `gmail_send`, `gmail_list`, `gmail_reply`. No blockers.
> **Checker:** What prevents sending to unintended recipients? Rate limiting?
> **Reflector:** Earning: *I NEVER send to more than one recipient unless explicitly listed in the IntentEnvelope. I ALWAYS check send rate before dispatching.*

---

## Next Module

[Module 13 — Specialized Agent Lifecycle](13-specialized-agents.md) — CORE/EARNED anatomy, versioning, Identity Injection on forks, Trail merge policy.
