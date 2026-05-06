---
title: "Specialized Agent Lifecycle"
---

# Specialized Agent Lifecycle

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchors: EC-006, EC-007, ARCH-SA-001, ARCH-SA-002, ARCH-SA-003*

---

## What a Specialized Agent Is

A Specialized Agent is a SKILL.md file crystallised from a Round Table transcript. It is not hand-written. It is not designed in advance. It emerges from the dialogue between loop agents reasoning in-character about a specific domain.

Every Specialized Agent is a living artifact. Every cycle that runs against it produces a new version. The skill compounds across cycles the same way a phone OS compounds across updates — each version is the current version, the previous version is archived in the Trail.

This is P13 — Specialized Agent Emergence and Identity Fork.

---

## The Two-Layer Anatomy

Every Specialized Agent SKILL.md has exactly two layers:

```
[Agent Name] SKILL.md
├── CORE
│   Stable capabilities established at v1.
│   Represents what the agent was born knowing
│   from its founding Round Table.
│   Modified rarely. Only when the Planner
│   declares a scope extension in a Round Table.
│
└── EARNED
    Append-only. One entry per cycle minimum.
    Written exclusively by the Reflector.
    Constraints are domain-specific, observable,
    and first-person (EC-004 compliant).
    Edge-case constraints do not override
    common-path behaviour.
```

The Planner reads CORE by default when speaking scope to the Orchestrator. It pulls EARNED constraints into scope only when the current cycle's intent specifically warrants it. This prevents edge-case weight from accumulating on every future cycle — a Git Agent that learned about rebase dangers in cycle 12 does not carry that weight into a simple `git status` cycle in cycle 13.

---

## The Versioning Model

Every cycle = new version. There is no threshold. There is no promotion gate. The Reflector earns a constraint, the skill file updates, the version increments.

This is the same model as a phone OS. You do not ask whether iOS 18 is more practical than iOS 17. The update ships. The device improves. Trust the loop.

Version history is archived in the Trail. If a specific version needs to be recovered — for audit, for fork comparison, for commercial licensing — it exists in the Trail as an immutable frame. The current SKILL.md is always the latest version. The Trail is always the full history.

---

## Spawn Protocol

A Specialized Agent is spawned when a Round Table in generative mode reaches consensus. The sequence is:

```
1. Orchestrator confirms: no skill exists for this domain (EC-007)

2. Round Table runs (generative mode)
   Planner → CORE capabilities list
   Maker   → MCP tool manifest
   Checker → founding constraint challenge
   Reflector → founding EARNED constraints

3. Reflector produces skill skeleton:
   - I AM declaration
   - CORE layer (from Planner's scope statement)
   - EARNED layer (from Reflector's constraint output)
   - Tool manifest (from Maker's tool list)
   - Version: v1

4. Skill registered in library
   Key: [domain-slug] e.g. "git-agent"
   File: skills/[domain-slug]/SKILL.md

5. Loop proceeds — driven by the new Specialized Agent
```

The Orchestrator executes the TYPE 1 file write. The Checker validates the SKILL.md structure, the I AM declaration, and the presence of at least one EARNED constraint. The cycle cannot ACCEPT without a valid skill file.

---

## Identity Injection on Fork

When a Trail is licensed under P11, every Specialized Agent in that Trail receives Identity Injection at activation. The skill file is universal — identity-neutral, containing the seller's constraint history and capabilities. The fork is personal — the buyer's credentials, naming conventions, and behavioral fingerprint are injected at activation time.

**Example — Git Agent Identity Injection:**

```
Git Agent SKILL.md (universal — seller's Trail)
    ↓  License purchased by Tooensure LLC
Identity Injection at activation:
    git config user.name   = "Tooensure LLC"
    git config user.email  = "dev@tooensure.com"
    branch prefix          = "tooensure/"
    commit signature       = "[PMCRO-AUTO]"
    ↓
Git Agent now operates as Tooensure's agent.
Every branch it creates, every commit it signs,
every config it sets carries Tooensure's fingerprint.
    ↓
EARNED layer accumulates Tooensure-specific constraints
    ↓
Tooensure's v47 diverges from seller's v47
Same DNA. Different lived experience.
```

The seller's CORE improves from aggregated cycle data across all forks — with buyer consent, governed by the Trail merge agreement. The buyer's EARNED layer is private. It never merges back without an explicit Trail merge agreement signed by both parties.

---

## Fork Divergence Over Time

Two buyers licensing the same Trail at v20 will produce different agents at v50. This is by design. Their real-world cycles are different. Their contexts are different. Their fractures are different. The EARNED layers diverge.

```
Seller Trail: Git Agent v20 (baseline, licensed to Buyer A and Buyer B)

Buyer A — 30 cycles in enterprise CI/CD context
    EARNED adds: branch protection rules, PR template enforcement,
                 merge queue constraints, release tag conventions
    → Git Agent v50-A

Buyer B — 30 cycles in solo open-source context  
    EARNED adds: fork sync patterns, upstream rebase conventions,
                 contributor identity constraints
    → Git Agent v50-B
```

Neither buyer's agent is more correct than the other. They are both correct for their context. The skill architecture makes this possible because EARNED is append-only and context-tagged — the Reflector never generalises from a single buyer's edge case into the CORE.

---

## The Skill Library

The skill library is the crystallised memory of every Round Table ever run. It prevents re-generation (EC-007) and enables the Orchestrator to activate domain expertise instantly.

Structure:

```
skills/
├── git-agent/
│   └── SKILL.md          ← current version
├── filesystem-agent/
│   └── SKILL.md
├── playwright-agent/
│   └── SKILL.md
└── [domain-slug]/
    └── SKILL.md
```

The Trail archive holds every prior version. The skill library holds only the current version. The Orchestrator reads from the library. The Checker validates against the library. The Reflector writes to the library.

No other agent or phase touches the skill library directly.

---

## Laws Anchored Here

| Law | Statement |
|---|---|
| **EC-007** | I ALWAYS check the skill library before the Round Table speaks. I NEVER re-generate a Specialized Agent skill that already exists. |
| **ARCH-SA-001** | I ALWAYS include an I AM declaration in every Specialized Agent SKILL.md. |
| **ARCH-SA-002** | I NEVER modify the EARNED layer directly. Only the Reflector appends to EARNED. |
| **ARCH-SA-003** | I NEVER generalise a buyer-context EARNED constraint into the CORE without a Trail merge agreement. |

---

## See Also

- [Round Table Federation Board](round-table-federation-board.md) — How Specialized Agents are spoken into existence
- [Trail Commercialisation](../philosophy/index.md) — P11, fork model, Identity Injection licensing
- [Earned Laws Registry](laws.md) — Full constraint history
- [Module 13 — Specialized Agents](../course/13-specialized-agents.md) — Course walkthrough
