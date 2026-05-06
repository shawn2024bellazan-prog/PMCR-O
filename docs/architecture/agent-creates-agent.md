---
title: "The Agent-Creates-Agent Pattern"
---

# The Agent-Creates-Agent Pattern

*Author: Shawn Bellazan · ThoughtLock: 2026-05-05 · Law Anchor: ARCH-NEW-001, ARCH-003*

---

## What It Is

A PMCRO cycle whose artifact type is `CONSOLE_AGENT` does not produce a document, a code file, or an analysis. It produces a new sovereign agent — a new member of the company, permanently registered, capable of running its own MicroWorkflows and receiving its own IntentEnvelopes.

The loop creates itself. This is not metaphor. The Maker produces the scaffolding for a new Console project with its own SKILL.md, its own identity declaration, its own MicroWorkflow. The Orchestrator executes the file writes. The Checker validates the compile and identity declaration. The Reflector seeds the next cycle to register the new agent in AppHost.

---

## The Full Cycle Shape

```
Intent:      "Create ProjectManagementAgent"

Planner:     - Designs responsibility boundary
             - Defines MicroWorkflow steps
             - Drafts SKILL.md I AM declaration
             - Validates no existing agent has this responsibility

Maker:       - Scaffolds sovereign Console project from template
             - Calls Mcp.Roslyn (TYPE 2) to validate compile
             - Returns McpDispatchDecision:
               { mcp: "filesystem", artifact_type: "CONSOLE_AGENT", files: [...] }

Orchestrator: - Executes TYPE 1 dispatch → project files written to disk
              - One accountable point for the world change

Checker:     - Mcp.Roslyn compile ✅
             - SKILL.md present with I AM declaration ✅
             - Sovereign Console template structure ✅
             - Mcp.Ml composite score ✅
             - Verdict: ACCEPT

Reflector:   - Locks constraint from this cycle
             - Seeds next cycle: "register ProjectManagementAgent in AppHost"
             - Result: new sovereign Console agent permanently part of the company
```

---

## Why This Matters

The autonomous company grows itself. Each new agent adds capability to the company without human scaffolding. The human states intent at the seed level. The loop produces the agent. The Reflector seeds the integration cycle. The company expands by running cycles, not by writing code.

The constraint set that governed the creation of the first agent governs the creation of every subsequent agent. The same laws that crystallized from early fractures prevent the same fractures in new agent creation. The company's growth is constrained by its own earned wisdom.

---

## Requirements for Every Created Agent

| Requirement | Law | Verification |
|---|---|---|
| SKILL.md present | SUB-LAW-001 | Checker validates file exists |
| I AM declaration in SKILL.md | EC-005 | Checker validates content matches pattern |
| No package Version= attributes | SUB-LAW-002 | Checker validates csproj |
| Proto files in Contracts only | SUB-LAW-003 | Checker validates project references |
| Solo mode executable | ARCH-Sovereign | Maker scaffolds with `--solo` entry point |
| `--serve` mode for gRPC | ARCH-Sovereign | Maker scaffolds with `--serve` entry point |

---

## See Also

- [The PMCRO Loop](pmcro-loop.md) — the cycle shape this pattern executes
- [MCP Type Classification](mcp-types.md) — TYPE 1 file writes are Orchestrator-only
- [What PMCRO Is](what-pmcro-is.md) — Primitive 10: Everything as Agent
