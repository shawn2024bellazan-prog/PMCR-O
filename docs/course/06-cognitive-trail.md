---
title: "Module 06 — The Cognitive Trail"
---

# Module 06 — The Cognitive Trail

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchor: ARCH-003, ARCH-025, P3, P11*

---

## What the Trail Is

The Cognitive Trail is not a log. This distinction is the most important thing to understand about the Trail, and it is worth repeating until it is automatic.

A log records events. It answers "what happened?" A Trail records self-knowledge. It answers "what did the system learn, what did it produce, how confident was it, and what should happen next?" Logs are consumed and discarded. Trails are carried forward. The difference is not a matter of detail or completeness. It is a matter of purpose.

Every cycle produces exactly one Trail, identified by a `trail_id` in the format `PMCRO-YYYYMMDD-[6-char hex]`. The Trail is built incrementally across the cycle as each phase adds its frame to the `CycleState`.

The Trail is the primary product of the substrate. Not the file the Maker wrote. Not the plan the Planner produced. The Trail — the complete, typed, cross-phase record of what was attempted, what succeeded, what was learned, and what comes next.

---

## The Frame Types

A Trail is composed of six typed frames. Each frame is produced by exactly one phase. No frame can be produced by any other phase — the attribution is structural.

### 1. IntentEnvelope (Federation Board)

The `IntentEnvelope` is the first frame. It is produced by the Federation Board before any cycle phase runs and is carried unmodified (except for the `CurrentIntent` field) through the entire cycle. It contains the sealed, refined true intent, the O-Mode classification, the economic gate result, the locked constraints from prior cycles, and the `PendingLaws` surfaced during refinement.

The TrailId in the IntentEnvelope is the identifier of the entire Trail. Every other frame carries this same TrailId.

### 2. ExecutionPlan (Planner)

```csharp
public sealed class ExecutionPlan
{
    public string TrailId { get; set; }
    public required string TruestIntent { get; set; }
    public required List<PlanStep> Steps { get; set; }
}

public sealed class PlanStep
{
    public required string Id { get; set; }
    public required string Description { get; set; }
    public required string Type { get; set; }  // "reconnaissance" | "artifact" | "dispatch"
}
```

The ExecutionPlan is the Planner's structured output. It contains the true intent as the Planner understood it (a refinement of the Federation Board's elevated intent into specific, actionable terms) and a list of typed steps. Reconnaissance steps describe what the Planner read. Artifact steps describe what the Maker should produce. Dispatch steps describe what TYPE 1 tool calls should be made.

### 3. MakerFrame (Maker)

```csharp
public sealed class MakerFrame
{
    public string TrailId { get; set; }
    public List<string> Artifacts { get; set; }           // The actual content produced
    public List<McpDispatchDecision> DispatchDecisions { get; set; }  // TYPE 1 calls to make
    public string Reasoning { get; set; }                 // Why the Maker made these decisions
}

public sealed class McpDispatchDecision
{
    public McpActuator Mcp { get; set; }       // filesystem | terminal | playwright
    public string Tool { get; set; }           // WriteFile | RunCommand | ClickElement | etc.
    public JsonObject Args { get; set; }       // Tool-specific arguments
}
```

The MakerFrame contains the artifacts — the actual content to be written or the actual changes to be made — and the dispatch decisions — the structured list of TYPE 1 tool calls that will write those changes to the world. The Maker does not execute. It produces the plan for execution.

### 4. DispatchResults (Orchestrator Gate)

```csharp
public sealed class McpToolResult
{
    public string TrailId { get; set; }
    public string Actuator { get; set; }       // Which MCP server was called
    public string Tool { get; set; }           // Which tool was called
    public bool Success { get; set; }          // Whether the call succeeded
    public string? Output { get; set; }        // Return value from the tool
    public string? Error { get; set; }         // Error message if Success is false
    public TimeSpan? Duration { get; set; }    // How long the call took
}
```

`DispatchResults` is a list — one entry per TYPE 1 dispatch decision that was executed. For cognitive-only cycles (no dispatch decisions), this list is empty. The Checker reads this list to verify that TYPE 1 calls succeeded, and the `PhysicalVerification` dimension is scored against these results.

### 5. QualityFrame (Checker)

The QualityFrame is the most information-dense frame in the Trail. It contains the eight-dimensional score, the composite, the SLV signal, the verdict, the earned constraints from this cycle, and the improvement directives if the verdict is LOOP.

```csharp
public sealed class QualityFrame
{
    public string TrailId { get; set; }
    public double GoalAlignment { get; set; }
    public double PlanAdherence { get; set; }
    public double LawCompliance { get; set; }
    public double PhysicalVerification { get; set; }
    public double OutputQuality { get; set; }
    public double DispatchCorrectness { get; set; }
    public double CognitiveCoherence { get; set; }
    public double EconomicEfficiency { get; set; }
    public double Composite { get; set; }
    public double Slv { get; set; }
    public string Verdict { get; set; }
    public List<string> EarnedConstraints { get; set; }
    public List<string> ImprovementDirectives { get; set; }
}
```

The `Slv` field — Semantic Learning Velocity — is the one that drives the SLV-001 override. If `Slv < 0.10` on an ACCEPT verdict, the Orchestrator questions the Checker's verdict. A cycle that learned nothing is a cycle that is probably being evaluated incorrectly.

Before the `Slv` field was added to the C# model (in `CycleModels.cs`), the `SLV-001` override was permanently inoperative — the value was emitted by the Checker in JSON but silently dropped on deserialization because the target property did not exist.

### 6. ReflectorFrame (Reflector)

```csharp
public sealed class ReflectorFrame
{
    public string TrailId { get; set; }
    public string CrystallisedConstraint { get; set; }   // The new earned law
    public string LawCode { get; set; }                   // EC-NNN
    public string LockedCot { get; set; }                 // The locked chain-of-thought
    public double Slv { get; set; }                       // Final SLV for this cycle
    public string NextCycleSeed { get; set; }             // What the next cycle should do
}
```

The `CrystallisedConstraint` is the most important output of the entire cycle in terms of long-term system improvement. It is a first-person, specific, observable behavioral rule derived from the Checker's `EarnedConstraints`. It will appear in the `LockedConstraints` of every subsequent IntentEnvelope.

The `NextCycleSeed` is the Reflector's recommendation for what the next cycle should tackle. It is written in the same seed-intent format that a human would type — first-person present tense, action-oriented. The Orchestrator uses this as the starting point for the next cycle if the current cycle was part of a multi-cycle O-Chain or O-Graph.

---

## How the Trail Builds Across Multiple Cycles

A single Trail covers a single cycle. But in an O-Chain or O-Graph mission, multiple cycles contribute to the same mission. The mechanism for chaining is the `NextCycleSeed` from the previous cycle's `ReflectorFrame`, which becomes the new seed intent fed back to the Federation Board.

```
Cycle 1
  Trail: PMCRO-20260506-A1B2C3
  ReflectorFrame.NextCycleSeed: "write the unit tests for the module I just scaffolded"

↓ Federation Board receives this seed ↓

Cycle 2
  Trail: PMCRO-20260506-D4E5F6
  IntentEnvelope.LockedConstraints includes all constraints from Cycle 1
  ReflectorFrame.NextCycleSeed: "run the tests and fix any failures"
```

The `LockedConstraints` accumulate across chained cycles because the Reflector appends the new `CrystallisedConstraint` to the `LockedConstraints` list before seeding the next Federation Board invocation. By cycle fifty, an agent working in a specific domain has fifty earned constraints in its IntentEnvelope — all derived from real events in that domain.

---

## The Trail as Transferable Expertise (P11)

This is where the Trail becomes commercially interesting. Consider two scenarios.

**Scenario A — Building from Scratch.** A new agent instance starts with EC-001 through EC-005 (the Colony Laws) and nothing else. It runs cycles. It makes mistakes. It earns constraints. By cycle twenty, it has twenty constraints. By cycle fifty, it has fifty. It becomes an expert through experience.

**Scenario B — Trail Licensing.** You have run fifty cycles on a specific domain — say, generating DocFX documentation from .NET C# source code. Your agent is an expert. It knows which patterns produce good DocFX output, which tool calls fail silently, which output schemas need to be tighter. That expertise is encoded in fifty earned constraints.

You package those fifty constraints — the full `ReflectorFrame` history — as a licensable Trail. A buyer purchases the Trail. They load it into a new agent instance with their own identity injected (Identity Injection). Their agent starts cycle one with fifty constraints already in its IntentEnvelope. It does not need to rediscover what you already discovered.

The `master_context` field in the IntentEnvelope is the injection mechanism for Trail-carried constraints. When a licensed Trail is loaded, its constraints populate `LockedConstraints` in the buyer's IntentEnvelope.

Your Trail continues to improve from your own cycles. The buyer's Trail evolves from their cycles on their fork. The seller earns from the initial license and from any royalty arrangement on Trail improvements.

---

## Reading a Trail in the Aspire Dashboard

When a cycle runs, the Aspire dashboard provides structured telemetry for every executor step. You can watch the Trail build in real-time:

1. Open the Aspire dashboard (`https://localhost:15888`)
2. Navigate to the `projectname-orchestrationapi` service
3. In the Traces view, find the trace for your `trail_id`
4. Expand the trace to see each executor's span: `PlannerExecutor`, `MakerExecutor`, `DispatchExecutor`, etc.
5. Each span contains the duration, the input state, and the output state

The `McpToolResult.Duration` field — which is nullable but populated on successful calls — appears in the Dispatcher span. This is useful for identifying slow TYPE 1 calls that might be causing timeout issues.

---

> **Next:** [Module 07 — Writing Skills](07-writing-skills.md)
