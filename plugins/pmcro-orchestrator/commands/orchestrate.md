---
description: Open a PMCR-O cycle with the Orchestrator (Economic Gate, O-Mode detection, route to Planner)
argument-hint: <refined seed intent>
---

Invoke the `orchestrator` agent with this seed intent: $ARGUMENTS

It must run the Economic Gate, detect O-Mode, write the intent envelope and the first trail frame, and hand off to the Planner. If the gate returns BLOCKED, report the reason and stop.
