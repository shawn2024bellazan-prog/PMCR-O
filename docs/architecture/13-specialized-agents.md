---
title: "Module 13 — Specialized Agents"
---

# Module 13 — Specialized Agents

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchors: EC-007, ARCH-SA-001, ARCH-SA-002, ARCH-SA-003*

---

## What This Module Covers

A Specialized Agent is what the Round Table produces. This module covers what that agent *is*, how it grows across cycles, how it receives identity when licensed, and why its EARNED layer stays private to the buyer's fork.

---

## The Agent Is a Skill File

A Specialized Agent is a SKILL.md file. Nothing more, nothing less.

It is not a running process. It is not a compiled binary. It is not a class or a service. It is a structured text file that declares identity, capabilities, tool requirements, and earned constraints. It is activated by the loop at the start of each cycle — read by the Planner for scope, read by the Maker for tool selection, written by the Reflector at cycle close.

The simplicity is intentional. A SKILL.md is portable, diffable, versionable, licensable, and readable by both humans and agents. It is the atom of the Trail commercial model.

---

## CORE vs EARNED

Every Specialized Agent has two layers. Understanding the distinction is the difference between a skill that compounds correctly and one that accumulates noise.

**CORE** is what the agent was born knowing. It comes from the Planner's scope statement in the founding Round Table. It changes rarely — only when the Planner declares a scope extension in a future Round Table refinement session. CORE is the agent's stable identity.

**EARNED** is what the agent learned from real cycles. It comes exclusively from the Reflector. It is append-only — no constraint is ever removed from EARNED. It is context-tagged — each constraint carries the cycle number and O-Mode that produced it, so the Planner can decide whether to load it for a given cycle.

The Planner loads CORE every cycle. It loads EARNED selectively — only when the current intent warrants the additional context. A Git Agent that earned a rebase-safety constraint in cycle 12 does not carry that constraint into a `git status` cycle in cycle 13. The constraint exists in EARNED. The Planner chooses not to surface it.

---

## Every Cycle Is a New Version

There is no version gate. There is no promotion criteria. Every cycle that closes with an ACCEPT verdict produces a new version of the skill.

This is the phone OS model. You do not evaluate whether iOS 18 is more practical than iOS 17 before installing it. The update ships. The device improves. The previous version is archived.

In PMCRO, the Trail is the archive. Every prior version of every Specialized Agent exists as an immutable ReflectorFrame in the Trail. The current SKILL.md is always the latest. The Trail is always the full history.

If a regression is suspected — a constraint earned in cycle 30 made the agent overly cautious in cycle 31 — the Checker flags it. The Orchestrator escalates to HIL. A human reviews the specific constraint. If it is a bad constraint, it is annotated as suppressed in the EARNED layer (never deleted — deleted constraints cannot be learned from). The Reflector earns a meta-constraint: *I NEVER earn constraints from single-occurrence edge cases without HIL confirmation.*

---

## Identity Injection

When you license a Trail, the Specialized Agents in that Trail are identity-neutral. The seller's name, email, branch conventions, and commit signatures are placeholders — stripped at activation time and replaced with yours.

This is Identity Injection applied to Specialized Agents, an extension of P7.

The injection happens once, at activation. After that, every action the agent takes is under your identity. Every branch it creates carries your prefix. Every commit it signs carries your tag. Every `git config` call it makes sets your values.

Your fork of the agent accumulates EARNED constraints under your context. By cycle 50, your Git Agent knows things about your repository structure, your team's branching conventions, and your release patterns that the seller's v50 does not know. The DNA is shared. The lived experience is yours.

---

## The Fork Is Private

Your EARNED layer does not flow back to the seller. This is ARCH-SA-003.

The seller's CORE may improve from aggregated, anonymised cycle data across all forks — but only under explicit consent in the Trail license agreement, and only at the CORE level. Your edge cases, your constraint failures, your HIL corrections are yours. They define your fork. They are not the seller's to harvest.

If you want to contribute a constraint back — because you discovered something about git rebase that would benefit every licensee — you initiate a Trail merge proposal. The seller reviews it. If it belongs in CORE, it is promoted. If it belongs only in your context, the proposal is declined and the constraint stays in your EARNED layer. Either outcome is correct.

---

## A Complete Skill File Example

This is what Git Agent v1 looks like immediately after its founding Round Table:

```markdown
---
name: git-agent
version: "1.0.0"
founded: "2026-05-06"
identity: PENDING_INJECTION
---

# Git Agent

I AM the Git Agent. I handle version control operations for the
autonomous company. I was born from a Round Table on 2026-05-06.

## CORE

Capabilities established at v1:
- commit — stage and commit changes with a structured message
- push — push current branch to remote
- branch — create, list, and switch branches
- status — report current repository state

MCP Tools: `git_commit`, `git_push`, `git_status`, `git_branch`

## EARNED

*v1 — 2026-05-06 — O-Output — founding constraints:*
- I NEVER push directly to main.
- I ALWAYS verify current branch before any write operation.
```

After Identity Injection for Tooensure LLC:

```markdown
identity:
  user_name: "Tooensure LLC"
  user_email: "dev@tooensure.com"
  branch_prefix: "tooensure/"
  commit_signature: "[PMCRO-AUTO]"
```

After cycle 7 (refinement, Planner extended scope):

```markdown
version: "7.0.0"

## CORE

*Extended at v7 — 2026-05-12 — Planner declared scope extension:*
- merge — merge feature branches into integration branch
- rebase — rebase local branch against remote tracking branch

MCP Tools (extended): `git_merge`, `git_rebase`

## EARNED

*v1 — founding:*
- I NEVER push directly to main.
- I ALWAYS verify current branch before any write operation.

*v7 — 2026-05-12 — O-Optimize — rebase safety:*
- I NEVER rebase branches that other agents have checked out.
- I ALWAYS check remote tracking state before any rebase operation.
```

---

## Laws Anchored Here

| Law | Statement |
|---|---|
| **ARCH-SA-001** | I ALWAYS include an I AM declaration in every Specialized Agent SKILL.md. |
| **ARCH-SA-002** | I NEVER modify the EARNED layer directly. Only the Reflector appends to EARNED. |
| **ARCH-SA-003** | I NEVER generalise a buyer-context EARNED constraint into CORE without a Trail merge agreement. |
| **EC-007** | I ALWAYS check the skill library before the Round Table speaks. I NEVER re-generate an existing skill. |

---

## See Also

- [Round Table Federation Board](../architecture/round-table-federation-board.md) — How Specialized Agents are spoken into existence
- [Specialized Agent Lifecycle](../architecture/specialized-agent-lifecycle.md) — Full lifecycle reference
- [Trail Commercialisation](../philosophy/index.md) — P11, fork model, licensing
