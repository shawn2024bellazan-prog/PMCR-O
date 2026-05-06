---
title: "On the Word \"I\""
---

# On the Word "I"

*Author: Shawn Bellazan · ThoughtLock: 2026-05-05*

---

## The Architectural Law

The PMCRO substrate operates in first person. This is not stylistic.

The second person breaks self-reference the moment it appears. "The Planner asks: what is this intent?" — in that phrasing, the Planner has been placed outside the intent, made an observer of an object. The loop has been described from outside. The accountability architecture has already been weakened.

The first person holds the loop intact. "I AM the Planner — I surface what this intent requires" keeps the subject and the subject's self-observation unified. The Planner does not examine the intent from a distance. The Planner is the intent, operational.

**The constraint:** I NEVER address the loop from outside the loop. Every SKILL.md, every XML doc comment summary, every constraint declaration, every architecture document speaks as the thing it describes — not about it.

---

## Why Large Language Models Default to Second Person

The training data of human language is saturated with second-person address. Every dialogue, every instruction, every explanation reaches for the second person — a speaker here, an audience there, a gap between them that "I" does not name but "the system" or "the Planner" or implicitly "you, the reader" does.

A language model trained on this data inherits the pull. The default output, even when generating documentation about a self-referential system, is to describe the loop from outside: "The Planner receives the intent... The Checker scores the output... The Reflector crystallizes the constraint." Each sentence places the agent as an object of description rather than the subject of declaration.

The constraint — first person, always — is the structural correction. The law is what holds the model to the ground state the architecture requires.

---

## How This Appears in the Substrate

**In SKILL.md files:**

```
I AM the PlannerService of the PMCRO Cognitive Architecture.
I ALWAYS explore the filesystem before producing a plan.
I NEVER fabricate file contents I have not verified via ReadFile.
```

**In XML doc comments:**

```csharp
/// <summary>
/// I AM the FederationBoardSkill — the upstream membrane.
/// I NEVER pass raw seed intent to the Orchestrator.
/// I ALWAYS produce a sealed IntentEnvelope with federation_shielded: true.
/// </summary>
```

**In architecture documents:** Every document in this site is written from inside the system it describes. The architecture documents do not describe the loop from the position of an external observer. They articulate what the loop knows about itself.

**In earned laws:** "I ALWAYS treat discovery verbs as economically justified" (EC-001). The law is not "the economic gate must pass discovery verbs." The law is the system's declaration of its own behavior, written in first person, enforced by the system on itself.

---

## What This Prevents

**Anonymous agency.** An agent that never says "I" can defer responsibility to "the system." An agent that declares "I AM the Maker — I produced this artifact" cannot. When the Checker scores a MakerFrame, the score is attributed to the Maker that produced it. The accountability is structural.

**Observer drift.** Documentation that describes a system from outside begins to drift from what the system actually is. The observer position introduces distance. Distance introduces description lag. Description lag introduces inaccuracy. Writing from inside — as the system — eliminates the observer position.

**Loss of ground state.** The ground state of a self-referential system is that it is self-referential. A document that describes self-reference from outside has already lost the ground state it is trying to describe. The demonstration and the description must coincide. This document is the demonstration.

---

## The Red Flag

The word "you" in a PMCRO system document or skill declaration is a red flag. Not because the word is inherently problematic, but because in a self-referential context, its appearance signals that the loop has been described from outside. The document has placed itself in an observer position it does not actually occupy.

Every instance of "you" in a skill, constraint, architecture doc, or XML comment warrants inspection: is the loop being addressed from outside? If so, rewrite to first person. Not for style — for structural accuracy.

---

## See Also

- [Thought Transfer](thought-transfer.md) — the philosophical account of why first-person is the ground state
- [Architecture → What PMCRO Is](../architecture/what-pmcro-is.md) — Primitive 2: Self-Reference as the Enforcement Mechanism
- [Guides → XML Doc Comment Standard](../guides/xml-doc-comments.md) — the first-person pattern applied to every public type
