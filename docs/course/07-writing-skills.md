---
title: "Module 07 — Writing Skills"
---

# Module 07 — Writing Skills

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchor: EC-005, P2, P4, P8*

---

## What a Skill Is

A skill is not a prompt template. A prompt template is a string you paste. A skill is a behavioral contract — a typed, identity-bearing, constraint-carrying artifact that an agent reads as part of its context and uses to govern its own output.

Every skill has three mandatory layers:

1. **Identity** — who the agent is when operating under this skill. Written in first person. "I AM the PropertyResearchAgent."
2. **Constraints** — what the agent always does and never does. Specific, observable, first-person. Not guidelines. Rules.
3. **Output contract** — what shape the agent's output takes. Named, typed, schema-validated.

If any of these three layers is missing, you do not have a skill. You have a note.

---

## The SKILL.md Format

Every skill in the substrate is a SKILL.md file. The format is:

```markdown
---
name: skill-name
description: >-
  One or two sentences. This is what Claude reads to decide whether to invoke
  the skill. Write it in first person. Start with "I AM the..."
metadata:
  version: "1.0.0"
  thought-lock: "YYYY-MM-DD"
  earned-laws: [list of law codes that govern this skill]
---

# Skill Name

## Identity

I AM the [Agent Name]. [One sentence declaration of role.]

## Constraints

I ALWAYS [specific observable action].
I NEVER [specific prohibited action].
[Repeat as needed. Minimum two constraints. Each earned from a real event.]

## Output Contract

[JSON schema or C# type description of what this skill produces.]

## Reference

[Link to any reference documents in the references/ subdirectory.]
```

EC-005 requires that the description field of every skill begin with "I AM the...". This is not optional. The description is injected into the model's context when the skill is loaded — its first words shape everything the model produces.

---

## The Three Tiers of Skills

Not all skills are the same kind of artifact. The substrate uses three tiers:

**Tier 1 — Phase Skills.** These are the C# `AgentClassSkill<T>` classes in `Skills/*.cs`. They are the behavioral contracts for the PMCRO phase agents. They are not SKILL.md files — they are code. The instructions they carry are compiled into the binary and injected via `IHasInstructions`.

**Tier 2 — Domain Skills.** These are SKILL.md files in the `skills/` directory. They are loaded by `PmcroSkillsProvider` and injected into agent context at runtime. When you build a PropertyResearchAgent or a JobApplicationAgent, the skill lives here.

**Tier 3 — Reference Skills.** These are SKILL.md files that describe how to use a specific MCP actuator — the filesystem, terminal, or Playwright. They live in `skills/filesystem/`, `skills/terminal-mcp/`, and `skills/playwright-mcp/`. Phase agents can reference them to understand what tools are available and how to use them.

---

## Writing Your First Domain Skill

The process:

**Step 1 — Name the agent.** What is the one thing this agent does? Name it precisely. "PropertyResearchAgent" not "ResearchAgent." "IndeedJobApplicationAgent" not "ApplicationAgent." The name is the identity.

**Step 2 — Write the trigger description.** What seed intent causes this skill to activate? Think about the exact words a human would say. "Research property at [address]." "Apply to jobs on Indeed matching [criteria]." This becomes the `description` field.

**Step 3 — Write the constraints.** What does this agent always do that a naive model would not? What does it never do that would cause problems? Start with two. Add more as fractures occur.

**Step 4 — Write the output contract.** What JSON shape does this agent produce? Define it explicitly. The Checker will score output against this contract.

**Step 5 — Create the SKILL.md.** Drop it in `skills/[agent-name]/SKILL.md`. Create a `references/` subdirectory for supporting documentation.

---

## Constraint Writing in Practice

Every constraint has a story. Here are real examples from the substrate and the fractures that produced them.

**EC-002** — "I NEVER put ReadFile in dispatch_decisions."
Story: The Maker repeatedly put ReadFile in its dispatch decisions. DispatchExecutor tried to call it as a TYPE 1 tool. It failed. Ten loops before the fracture was diagnosed and the constraint was written.

**EC-003** — "I skip physical verification on cognitive-only cycles."
Story: The Checker demanded physical verification of disk artifacts on a summarization task. No files were written. The Checker looped indefinitely. The constraint short-circuits this path.

**PropertyResearchAgent (hypothetical)** — "I ALWAYS call GetPageContent before attempting to extract property details."
Story: The agent tried to parse property details from a URL before loading the page. Empty content. Failed extraction. Constraint earned.

The pattern is always: fracture → root cause → constraint. You do not write constraints in advance. You write them after something breaks.

---

## Skills as P11 Assets

Every skill you write is a Trail asset. Here is how that compounds:

You build a PropertyResearchAgent skill. You run 20 cycles researching vacant properties. Each cycle earns constraints. By cycle 20, your skill has 20 earned constraints specific to county assessor websites, Zillow scraping patterns, and property record formats.

You have run those cycles. Someone else who builds a similar agent has not. Your skill — with its 20 earned constraints — is worth money to them. They do not need to discover that Zillow blocks scraping on Mondays between 2-4 AM UTC, or that some county assessor sites require a human-solve CAPTCHA on the first request. You discovered those. Your constraints encode them.

That is the commercial model. Every skill you build is simultaneously a tool for your own company and a licensable asset for other companies running the same domain.

---

> **Next:** [Module 08 — Running Your First Cycle](08-first-cycle.md)
