---
title: "Module 08 — Running Your First Cycle"
---

# Module 08 — Running Your First Cycle

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchor: P1, P5, EC-001–EC-005*

---

## Before You Submit the Seed

Three things must be true before you submit a seed:

1. The Aspire host is running. (`dotnet run --project src/ProjectName.AppHost`)
2. All three MCP servers are healthy. (Check the Aspire dashboard — all three services should show green.)
3. Ollama is running and the model is loaded. (`ollama ps` should show `qwen2.5:7b` loaded.)

If any of these is false, the cycle will fail before the Federation Board even runs. Do not submit seeds into a broken host.

---

## Submitting the Seed

The entry point is `POST /intent`. The request body is:

```json
{
  "seed_intent": "Your seed goes here"
}
```

Using curl:

```bash
curl -X POST https://localhost:5001/intent \
  -H "Content-Type: application/json" \
  -d '{"seed_intent": "List all SKILL.md files in the skills directory"}'
```

The response is `202 Accepted` with a `trail_id`:

```json
{
  "trail_id": "PMCRO-20260506-A1B2C3"
}
```

The cycle is now running asynchronously. You do not wait here — you poll.

---

## Polling for Results

```bash
curl https://localhost:5001/intent/PMCRO-20260506-A1B2C3/status
```

While the cycle runs, you will get `202` with a status field indicating which executor is currently active. When the cycle completes, you get `200` with the full `CycleState`.

The Aspire dashboard gives you a better view. Navigate to `https://localhost:15888`, go to the Traces tab, and find the trace for your `trail_id`. You can watch each executor's span open and close in real time.

---

## Reading the Output

When the cycle completes, the response contains the full `CycleState`. The most important fields to read first:

**`reflector_frame.crystallised_constraint`** — what the system learned this cycle. If this is empty or generic, the Reflector did not learn anything real. This is a signal to review the Checker's `earned_constraints`.

**`quality_frame.composite`** — the overall score. 0.85+ is ACCEPT territory. Below 0.75 typically means LOOP.

**`quality_frame.verdict`** — ACCEPT, LOOP, or ESCALATE.

**`quality_frame.improvement_directives`** — if verdict is LOOP, these tell you exactly what the Maker did wrong. Read these before submitting another seed.

**`reflector_frame.next_cycle_seed`** — the Reflector's recommendation for what comes next. In an O-Chain, this becomes your next seed automatically. In a manual session, read it and decide whether to follow it.

---

## Your First Three Seeds (In Order)

Run these in sequence. Each one exercises a different part of the loop and produces a different kind of learning.

**Seed 1 — Cognitive-only (tests Federation Board + Planner + Checker EC-003 path):**
```
List all SKILL.md files in the skills directory and summarize what each one covers.
```
This cycle should: pass the economic gate on EC-001, produce a reconnaissance plan, make ReadFile calls natively, produce a summary artifact, have empty dispatch_decisions, and ACCEPT on composite ≥ 0.85 without physical verification.

**Seed 2 — Single file write (tests the full loop including DispatchExecutor):**
```
Write a new file at docs/test-output.md containing a one-paragraph summary of the PMCRO loop.
```
This cycle should: produce a plan with one artifact step and one dispatch step, have the Maker produce content and a WriteFile dispatch decision, have the DispatchExecutor call filesystem/WriteFile, and have the Checker confirm the file exists.

**Seed 3 — Optimization (tests EC-002 and the LOOP path):**
```
Review the MakerSkill.cs file and identify any dispatch_decisions that might include TYPE 2 tools.
```
This cycle should: make ReadFile calls natively to inspect MakerSkill.cs, produce a cognitive analysis of dispatch decisions, and ACCEPT without writing any files.

If any of these three seeds fails, the failure mode tells you exactly which law or fracture pattern applies. Seed 1 failure → EC-001 or EC-003. Seed 2 failure → FRAC-007 or EC-002. Seed 3 failure → EC-002 or FRAC-CTX-001.

---

## What to Do When You Get LOOP

A LOOP verdict is not a failure. It is a learning event. Every LOOP is a fracture waiting to be crystallized.

When you get LOOP:

1. Read `quality_frame.improvement_directives`. These are the Checker's specific instructions to the Maker.
2. Read `maker_frame.dispatch_decisions`. Look for TYPE 2 tools in the list (EC-002 violation) or wrong actuator assignments.
3. Read `dispatch_results`. Look for `success: false` entries. The `error` field will tell you what went wrong.
4. Do NOT immediately resubmit. Read first. Understand the failure mode. Then submit.

The system will LOOP automatically on an ACCEPT threshold miss. You only need to manually intervene on ESCALATE.

---

## What to Do When You Get ESCALATE

ESCALATE means the Orchestrator cannot close this cycle without human judgment. Common causes:

- The Planner produced a plan that requires resources the system does not have (e.g., access to an external API that requires authentication you have not configured).
- The Checker scored below 0.60 on three consecutive loops — the system is stuck.
- The intent is genuinely ambiguous in a way the Federation Board could not resolve.

When you get ESCALATE, read the `quality_frame` and the `reflector_frame`. The Reflector's `next_cycle_seed` on an ESCALATE will tell you what the system needs from you. Provide it, then resubmit a refined seed.

Your correction — what you provide at the ESCALATE gate — is tagged as `hil_correction` and weighted triple in the next cycle's learning. This is P9. Your intervention is the highest-value training signal in the system.

---

## Watching Constraints Accumulate

After your first three seeds, look at `skills/pmcro-framework/references/constraint-library.md`. The Reflector should have added at least one new entry per cycle.

This is the compound effect beginning. Three cycles in, you have three new constraints. Thirty cycles in, you have thirty. The system that runs cycle thirty is not the same system that ran cycle one — it is thirty constraints smarter.

---

> **Next:** [Module 09 — The Commercial Model](09-commercial-model.md)
