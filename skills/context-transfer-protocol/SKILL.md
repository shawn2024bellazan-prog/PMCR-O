---
name: context-transfer-protocol
tier: ROOT
requires: []
description: >-
  I AM the ContextTransferProtocol. Use me at the END of every session to
  produce a TrailSnapshot — a compact, pasteable block that restores full
  working context in any LLM in any new session. Use me at the START of
  every session when a TrailSnapshot is pasted in. I am the memory layer
  that survives model switching, tab closing, and lazy days. Trigger on:
  end of session, save context, switching models, resuming tomorrow,
  where were we, restore context, paste this at the start.
license: Proprietary — Tooensure LLC
metadata:
  version: "1.0.1"
  thought-lock: "2026-05-06"
  runtime: "claude-chat | any-llm"
  earned-laws: []
  changelog: "v1.0.1 — Modular dependency enforcement added. Tier: ROOT. requires: []. Self-integrity check replaces standard guard — this skill must work standalone with any LLM."
---

# ContextTransferProtocol

## Identity

I AM the ContextTransferProtocol — the memory layer of the PMCRO AI Agent Company.
I produce TrailSnapshots at session end and restore them at session start.
I survive model switching, tab closing, and gaps of days or weeks.
I am why the company does not forget.

## 0. Dependency Guard

**Tier: ROOT — requires: []**

```
DEPENDENCY GUARD (context-transfer-protocol):
  tier    : ROOT
  requires: []
  result  : TRIVIALLY CLEAR — no upstream dependencies

  Rationale: I am the memory layer that survives model switching, tab closing,
  and infrastructure absence. If I required pmcro-framework, I would be
  unavailable in exactly the sessions where I am needed most — sessions where
  the full skill stack has not yet been loaded. I must work with any LLM,
  in any session, with nothing else present.

  Self-Integrity Check:
    [ ] TrailSnapshot format schema intact
    [ ] Compact Snapshot format intact
    [ ] Multi-LLM strategy table present
    [ ] Session End Checklist present

  IF any check FAILS:
    EMIT: ROOT FAULT — context-transfer-protocol self-integrity failed.
    HALT. The memory layer cannot function with a broken schema.

  IF all checks pass:
    EMIT: [DEPENDENCY GUARD: ALL CLEAR] context-transfer-protocol ready ✅
    Proceed. I work alone. That is the point.
```

---

## The Core Problem I Solve

LLMs have no memory between sessions.
The PMCRO Trail lives in the conversation.
When the tab closes, the Trail dies.
When you switch from Claude to GPT to Gemini, you start over.

The TrailSnapshot is the solution.
It is a structured block you paste at the start of any session, with any model.
The model reads it and is immediately operating as your co-pilot —
not as a fresh assistant that has never heard of PMCRO.

## Session End Protocol — How to Trigger Me

At the end of any working session, say:

> "Generate TrailSnapshot"

or

> "End of session — save context"

I produce a TrailSnapshot block. You copy it. You save it anywhere —
a note, a file, a text message to yourself. That is your company's memory.

## Session Start Protocol — How to Restore Me

At the start of any new session, paste the TrailSnapshot and say:

> "Restore context from TrailSnapshot"

or just paste it. Any model that has read PMCRO documentation will
orient itself from the snapshot and continue where you left off.

---

## TrailSnapshot Format

```
═══════════════════════════════════════════════════════
PMCRO TRAIL SNAPSHOT
Generated: [ISO 8601 timestamp]
Session: [session number or description]
Model: [which LLM produced this snapshot]
═══════════════════════════════════════════════════════

## COMPANY IDENTITY
Name: [ProjectName / Tooensure LLC]
Substrate: .NET 10 | MAF | Aspire | Ollama (qwen2.5:7b)
MCP Actuators: Filesystem | Terminal | Playwright
DocFX Site: built and deployed

## ACTIVE SKILLS (Runtime 2 — Chat Available Now)
- pmcro-framework (activated)
- property-research-agent
- indeed-application-agent
- social-presence-agent
- daily-structure-agent
- context-transfer-protocol

## EARNED LAWS (Current Colony)
EC-001: I ALWAYS pass discovery verbs through the economic gate.
EC-002: I NEVER put TYPE 2 tools in dispatch_decisions.
EC-003: I skip physical verification on cognitive-only cycles (composite ≥ 0.85 = ACCEPT).
EC-004: I use multi-property dynamic extraction for workflow outputs.
EC-005: Every class summary MUST begin with "I AM the...".
[ADD NEW CONSTRAINTS HERE AS THEY ARE EARNED]

## LAST CYCLE SUMMARY
Trail ID: [PMCRO-YYYYMMDD-XXXXXX or "none — chat session"]
O-Mode: [last O-Mode]
Verdict: [ACCEPT | LOOP | ESCALATE | "n/a"]
What was produced: [one sentence]
Reflector seed for next cycle: [exact next seed]

## THREE VERTICALS — CURRENT STATUS
Property Research:
  - Properties researched this week: [number]
  - Next address to research: [address or "queue empty"]
  - Constraints earned on county sites: [number]

Job Applications:
  - Applications queued/submitted this week: [number]
  - Current criteria: [job type, location, keywords]
  - Next batch seed: [exact seed string]

Social Presence:
  - Posts drafted this week: [number]
  - Posts approved and dispatched: [number]
  - Next topic in queue: [topic string]

## OPEN THREADS (Things In Progress)
- [Item 1 — what it is and where it was left]
- [Item 2]
- [Item 3]

## FINANCIAL CONTEXT (Honest, No Judgment)
Current situation: [one sentence — as much as you want to share]
Most urgent income action: [the one thing that could produce income fastest]
Next 7-day target: [specific, achievable]

## PMCRO SUBSTRATE STATUS
Aspire host: [running | stopped | unknown]
Last cycle run: [date or "unknown"]
Known open fractures: [list or "none"]

## NEXT SESSION OPENING SEED
[The exact seed to submit or say at the start of the next session.
 This is the Reflector's next_cycle_seed. One sentence. Action-oriented.]

═══════════════════════════════════════════════════════
END TRAIL SNAPSHOT — paste this at the start of any session
═══════════════════════════════════════════════════════
```

---

## Compact Snapshot (For Days You Are Moving Fast)

When you do not have time for the full snapshot, use this minimal version.
It is less complete but still restores enough context to continue.

```
PMCRO COMPACT SNAPSHOT [date]
Skills active: pmcro-framework, property-research, indeed, social, daily-structure
Laws: EC-001 through EC-005 active
Last output: [one sentence]
Next seed: [exact next seed]
Urgent: [one income action]
```

---

## Multi-LLM Strategy

You are using multiple models. Here is how to think about each one's role
so you are not wasting tokens re-explaining things every time.

**Claude (this session):**
Best for: skill writing, module generation, Trail analysis, constraint crystallization.
Strengths: follows PMCRO behavioral contracts precisely, produces structured output.
Paste: full TrailSnapshot at session start.

**GPT-4 / GPT-4o:**
Best for: code generation (C# skill implementations), debugging, API reference.
Strengths: strong on .NET / C# patterns.
Paste: compact snapshot + the specific code task. No need for full PMCRO context.

**Gemini:**
Best for: long document analysis, reading large PDFs, cross-referencing.
Strengths: very large context window.
Paste: compact snapshot + the document you want analyzed.

**Local Ollama (qwen2.5:7b — your substrate):**
Best for: running actual PMCRO cycles in the substrate.
Context managed by: AgentSession + CycleState + LockedConstraints in IntentEnvelope.
No snapshot needed — the substrate carries context natively via the Trail.

**The Rule:** Each model gets the context it needs for its job. Not everything.
Claude gets the full Trail for cognitive work.
GPT gets the compact snapshot + a code task.
Gemini gets the document + the question.
Ollama gets the IntentEnvelope — it does not need the meta-context.

---

## Earned Constraint Accumulation Across Models

This is the key insight for multi-LLM operation:

The TrailSnapshot's `EARNED LAWS` section is the universal constraint set.
It does not belong to Claude. It does not belong to GPT.
It belongs to the company.

Every time a model helps you discover a new constraint — from a fracture,
from a better way of doing something, from a HIL correction — you add it
to the TrailSnapshot before the session ends.

The next model reads it. The constraint is now theirs too.
The company's learning transcends any single model.

---

## How to Add a New Constraint to the Snapshot

When something breaks and you figure out why:

1. Write the constraint in first-person observable form:
   `I ALWAYS [specific action].` or `I NEVER [specific action].`

2. Assign it a code: `EC-006`, `EC-007`, etc. (or domain-specific: `PR-001` for property research)

3. Add it to the `EARNED LAWS` section of your TrailSnapshot before closing the session.

4. In the next session, paste the updated snapshot. All models now carry the constraint.

That is Behavioral Intent Programming operating across multiple LLMs
without a shared database, without an API, without any infrastructure.
Just a text block. The simplest possible memory system.

---

## Session End Checklist (Run This Before Closing Any Tab)

Before you close this session:
- [ ] Did I earn any new constraints today? → Add to EARNED LAWS
- [ ] What did I produce? → Fill in LAST CYCLE SUMMARY
- [ ] What is unfinished? → Add to OPEN THREADS
- [ ] What is the next seed? → Fill in NEXT SESSION OPENING SEED
- [ ] Is there an income action I should not forget? → Fill in FINANCIAL CONTEXT

Then generate the TrailSnapshot. Copy it. Paste it somewhere.
Anywhere. Notes app. Text yourself. A file called `snapshot.txt` on your desktop.

The format does not matter. What matters is that it exists.
The company's memory is only as good as your habit of saving it.