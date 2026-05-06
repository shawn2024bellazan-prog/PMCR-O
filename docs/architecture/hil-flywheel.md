---
title: "The HIL Flywheel"
---

# The HIL Flywheel

*Author: Shawn Bellazan · ThoughtLock: 2026-05-05 · Law Anchor: ARCH-003*

---

## What the HIL Flywheel Is

HIL — Human in the Loop — is not a safety net in the PMCRO substrate. It is a **training mechanism**. Every human correction to an agent decision is worth more than an approval, because corrections capture edge cases the loop has not yet seen. The flywheel converts human corrections into constraints, and constraints into autonomous capability.

The flywheel turns: human corrects → correction becomes TrailFrame → Reflector crystallizes constraint → constraint locks into future cycles → HIL gate opens automatically → human moves to higher-order decisions → flywheel restarts at the new level.

---

## The Three HIL Levels

| Level | Where | Granularity | Opens First? |
|---|---|---|---|
| 3 | Console agent MicroWorkflow | Per tool execution | Yes — most predictable. Humans approve each tool call before execution. |
| 2 | MAF macro checkpoint | Per phase transition | Second — humans review phase outputs before next phase begins. |
| 1 | CopilotKit CoAgents | Per full cycle | Last — full autonomy. Human reviews only the final ACCEPT/LOOP verdict. |

Level 3 opens first because individual tool executions are the most predictable — small scope, verifiable outcome, cheap to correct. As the agent earns constraints from Level 3 corrections, those constraints close the Level 3 gate. The human moves to Level 2. The same process repeats. Eventually Level 1 closes, and the agent runs full cycles without HIL — only surfacing for ESCALATE routing decisions.

---

## Why Corrections Are More Valuable Than Approvals

An approval confirms the agent did what the human expected. No new information. An approval in trail 50 confirms that the constraint set from trails 1–49 is holding.

A correction reveals a gap. The agent did something the constraint set did not prevent. The correction becomes a new constraint. Trail 51 carries that constraint. The gap cannot recur.

The flywheel is asymmetric by design: the system learns faster from failure than from success. Every HIL gate that fires and receives a correction is an accelerant. The human who corrects the agent is not slowing the system down. The human is teaching the system to not need that correction again.

---

## See Also

- [The PMCRO Loop](pmcro-loop.md) — where HIL gates fire in the macro and micro workflows
- [The Cognitive Trail](cognitive-trail.md) — how corrections become TrailFrames
- [Earned Laws Registry](laws.md) — the crystallized output of the flywheel
