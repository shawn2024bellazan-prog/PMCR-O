---
title: "Module 04 — The Workflow Engine"
---

# Module 04 — The Workflow Engine

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchor: ARCH-003, ARCH-012, ARCH-NEW-001, ARCH-025, FRAC-007*

---

## PmcroWorkflow.cs — The Heart of the System

`PmcroWorkflow.cs` is the single most important file in the substrate. Everything else — the skills, the models, the MCP actuators — exists to support what happens in this file. It defines the MAF `AgentWorkflow` that IS the PMCRO loop.

The file contains seven inner classes, each one a MAF `Executor`:

1. `PlannerExecutor` — reconnaissance and plan production
2. `MakerExecutor` — artifact production and dispatch decision planning
3. `DispatchExecutor` — TYPE 1 tool execution (the Orchestrator gate)
4. `ConstraintInjectorExecutor` — earned law propagation
5. `CheckerExecutor` — quality evaluation and verdict
6. `ReflectorExecutor` — learning crystallization
7. `EscalateExecutor` — human escalation

The workflow graph connects them in this sequence:

```
PlannerExecutor
    │
    ▼
MakerExecutor
    │
    ▼
DispatchExecutor
    │
    ▼
ConstraintInjectorExecutor
    │
    ▼
CheckerExecutor
    │
    ├──[ACCEPT]──► ReflectorExecutor ──► [DONE]
    ├──[LOOP]────► MakerExecutor (back)
    └──[ESCALATE]► EscalateExecutor ──► [HIL]
```

---

## PlannerExecutor — Reconnaissance First, Plan Second

The Planner executor runs first. Its job is to read the current state of the world — using TYPE 2 MCP tools natively through MAF — and produce an `ExecutionPlan` that the Maker can follow.

```csharp
// From PmcroWorkflow.cs
public sealed class PlannerExecutor(
    [FromKeyedServices("planner")] AIAgent plannerAgent,
    ILogger logger) : Executor
{
    [Handler]
    public async Task Plan(CycleState state, WorkflowContext<CycleState> ctx)
    {
        var response = await plannerAgent.RunAsync(
            message: BuildPlannerPrompt(state.Envelope),
            options: new ChatClientAgentOptions
            {
                ResponseFormat = ResponseFormat.ForJsonSchema<PlannerResponse>()
            });

        state.Plan = JsonSerializer.Deserialize<PlannerResponse>(response)?.Plan;
        await ctx.SendMessageAsync(state);
    }
}
```

Notice `ResponseFormat.ForJsonSchema<PlannerResponse>()`. This is the typed output contract — the MAF mechanism that constrains the model to produce deserializable JSON matching the `PlannerResponse` schema.

Inside `plannerAgent.RunAsync()`, the Planner may make zero or more TYPE 2 tool calls before producing its final response. If the Planner calls `ListDirectory`, MAF intercepts the `tool_calls` block in the model's response, executes the directory listing via the filesystem MCP client, appends the result to the conversation history, and re-calls the model. This loop continues until the model produces a final text response — the `PlannerResponse` JSON.

The Planner's instructions (from `PlannerSkill.cs`) require it to make TYPE 2 reconnaissance calls before emitting its plan. The key constraint:

> I ALWAYS use my MCP tools to explore the filesystem before producing a plan. I NEVER fabricate file contents or directory listings.

---

## MakerExecutor — Planning What to Execute, Not Executing

The Maker executor runs second. Its job is to take the ExecutionPlan and produce two things: artifacts (the actual content to be created or modified) and dispatch decisions (the list of TYPE 1 tool calls to execute those changes).

```csharp
public sealed class MakerExecutor(
    [FromKeyedServices("maker")] AIAgent makerAgent,
    ILogger logger) : Executor
{
    [Handler]
    public async Task Make(CycleState state, WorkflowContext<CycleState> ctx)
    {
        var prompt = BuildMakerPrompt(state.Envelope, state.Plan!);
        var response = await makerAgent.RunAsync(
            message: prompt,
            options: new ChatClientAgentOptions
            {
                ResponseFormat = ResponseFormat.ForJsonSchema<MakerFrame>()
            });

        state.MakerFrame = JsonSerializer.Deserialize<MakerFrame>(response);
        await ctx.SendMessageAsync(state);
    }
}
```

The critical constraint for the Maker, from `MakerSkill.cs`:

> I AM on the cognitive layer. I reason about what should change. I do NOT execute changes.

The Maker produces `McpDispatchDecision` entries that look like this:

```json
{
  "dispatch_decisions": [
    {
      "mcp": "filesystem",
      "tool": "WriteFile",
      "args": {
        "relativePath": "docs/course/new-module.md",
        "content": "# New Module\n\n...",
        "overwrite": "true"
      }
    }
  ],
  "artifacts": [
    "# New Module\n\nContent here..."
  ]
}
```

Notice that the Maker specifies `"overwrite": "true"` as a string, not a boolean. This is EC-005 alignment — a specific learned constraint: "I ALWAYS pass overwrite as the string 'true', never a bare boolean." This matters because the filesystem MCP server validates the argument as a string, not a JSON boolean.

---

## DispatchExecutor — The Only Place TYPE 1 Calls Fire

This is the most security-critical executor in the system. Before v4.0, `DispatchExecutor` was a no-op stub — it cleared `state.DispatchResults` and returned. TYPE 1 tools were planned by the Maker and approved by the Checker but never executed. Every cycle that required file writes silently succeeded in the cognitive layer and failed in the physical layer.

This was FRAC-007. The fix:

```csharp
public sealed class DispatchExecutor(
    IMcpToolExecutor toolExecutor,
    IMcpClientRegistry registry,
    ILogger logger) : Executor
{
    [Handler]
    public async Task Dispatch(CycleState state, WorkflowContext<CycleState> ctx)
    {
        if (state.MakerFrame?.DispatchDecisions is null or { Count: 0 })
        {
            // Cognitive-only cycle — nothing to dispatch
            await ctx.SendMessageAsync(state);
            return;
        }

        foreach (var decision in state.MakerFrame.DispatchDecisions)
        {
            var result = await toolExecutor.ExecuteType1Async(
                actuator: decision.Mcp,
                tool: decision.Tool,
                args: decision.Args);

            state.DispatchResults.Add(result);

            if (!result.Success)
            {
                logger.LogError(
                    "TYPE 1 dispatch failed: {Actuator}.{Tool} → {Error}",
                    decision.Mcp, decision.Tool, result.Error);
            }
        }

        await ctx.SendMessageAsync(state);
    }
}
```

Two design decisions here are worth studying.

First, `IMcpToolExecutor` is injected into `DispatchExecutor` and ONLY into `DispatchExecutor`. No phase agent receives this injection. This is enforced by architecture — the skill classes (`PlannerSkill`, `MakerSkill`, etc.) do not have access to `IMcpToolExecutor` because it is not registered in a way that `AgentClassSkill<T>` can reach.

Second, failure handling is non-throwing. If a TYPE 1 call fails, the failure is recorded in `state.DispatchResults` with `Success: false` and the loop continues. The `CheckerExecutor` receives all dispatch results — including failures — and scores correctness accordingly. A failed dispatch is not a crashed cycle; it is a scored event.

---

## ConstraintInjectorExecutor — How Earned Laws Propagate

This small executor runs between `DispatchExecutor` and `CheckerExecutor`. Its job is to ensure the locked constraints from the IntentEnvelope are present in the context for the Checker.

In a simple implementation, this might be a pass-through — the constraints are already in the envelope which is already in `CycleState`. In a more complete implementation, this executor also promotes `PendingLaws` from the envelope into the `LockedConstraints` list if they meet the ratification criteria.

This executor is the mechanical expression of P4 — constraint accumulation as learning. Without it, earned constraints from previous cycles would need to be injected manually. With it, they flow automatically from the ReflectorFrame of cycle N into the LockedConstraints of cycle N+1.

---

## CheckerExecutor — Quality Gate and Verdict

The Checker executor receives the full `CycleState` — envelope, plan, maker frame, dispatch results, and constraints — and produces a `QualityFrame`.

```csharp
public sealed class CheckerExecutor(
    [FromKeyedServices("checker")] AIAgent checkerAgent,
    ILogger logger) : Executor
{
    [Handler]
    public async Task Check(CycleState state, WorkflowContext<CycleState> ctx)
    {
        var response = await checkerAgent.RunAsync(
            message: BuildCheckerPrompt(state),
            options: new ChatClientAgentOptions
            {
                ResponseFormat = ResponseFormat.ForJsonSchema<QualityFrame>()
            });

        state.QualityFrame = JsonSerializer.Deserialize<QualityFrame>(response);

        // Route based on verdict
        switch (state.QualityFrame?.Verdict)
        {
            case "ACCEPT":
                await ctx.SendMessageAsync(state);      // → ReflectorExecutor
                break;
            case "LOOP":
                state.Envelope.LoopCount++;
                await ctx.SendMessageAsync(state, "loop");   // → MakerExecutor
                break;
            case "ESCALATE":
                await ctx.SendMessageAsync(state, "escalate"); // → EscalateExecutor
                break;
        }
    }
}
```

The `QualityFrame` has eight scoring dimensions:

```csharp
public sealed class QualityFrame
{
    // 0.0 – 1.0 scores for each dimension
    public double GoalAlignment { get; set; }        // Does it solve the stated goal?
    public double PlanAdherence { get; set; }        // Did Maker follow the plan?
    public double LawCompliance { get; set; }        // Are earned constraints obeyed?
    public double PhysicalVerification { get; set; } // Is the artifact actually there?
    public double OutputQuality { get; set; }        // Is the artifact well-formed?
    public double DispatchCorrectness { get; set; }  // Did TYPE 1 calls succeed?
    public double CognitiveCoherence { get; set; }   // Does the Trail make sense?
    public double EconomicEfficiency { get; set; }   // Was compute used well?

    // Weighted average of all eight dimensions
    public double Composite { get; set; }

    // EC-003: SLV < 0.10 triggers SLV-001 override
    public double Slv { get; set; }

    // ACCEPT | LOOP | ESCALATE
    public string Verdict { get; set; } = "";

    // Constraints earned from this cycle — raw material for the Reflector
    public List<string> EarnedConstraints { get; set; } = [];

    // Improvement directives for LOOP verdicts
    public List<string> ImprovementDirectives { get; set; } = [];
}
```

The EC-003 fix is in the Checker's instructions (from `CheckerSkill.cs`):

> @cognitive-only-cycle: If dispatch_decisions is empty AND dispatch_results is empty, physical verification is NOT required. Skip directly to verdict. ACCEPT if composite >= 0.85.

Before this fix, the Checker would attempt to call `ReadFile` to verify files that were never written, fail to confirm `PASSED`, and emit LOOP despite a composite score of 0.90. The fix adds an explicit short-circuit path for cognitive-only cycles.

---

## ReflectorExecutor — Crystallizing the Learning

The Reflector executor runs after ACCEPT. It receives the completed `CycleState` and produces the `ReflectorFrame` — the final frame that seals the Trail.

```csharp
public sealed class ReflectorExecutor(
    [FromKeyedServices("reflector")] AIAgent reflectorAgent,
    ILogger logger) : Executor
{
    [Handler]
    public async Task Reflect(CycleState state, WorkflowContext<CycleState> ctx)
    {
        var response = await reflectorAgent.RunAsync(
            message: BuildReflectorPrompt(state),
            options: new ChatClientAgentOptions
            {
                ResponseFormat = ResponseFormat.ForJsonSchema<ReflectorFrame>()
            });

        state.ReflectorFrame = JsonSerializer.Deserialize<ReflectorFrame>(response);
        await ctx.YieldOutputAsync(state);  // ← YieldOutput, not SendMessage — cycle ends here
    }
}
```

`YieldOutputAsync` vs `SendMessageAsync` is the MAF mechanism for ending a workflow. `SendMessageAsync` passes control to the next executor. `YieldOutputAsync` returns the output to the workflow caller and ends the graph traversal.

The `ReflectorFrame`:

```csharp
public sealed class ReflectorFrame
{
    // The crystallized constraint from this cycle, in I ALWAYS / I NEVER form
    public string CrystallisedConstraint { get; set; } = "";

    // Law code in EC-NNN format
    public string LawCode { get; set; } = "";

    // Locked chain-of-thought — the reasoning that produced the constraint
    public string LockedCot { get; set; } = "";

    // Semantic Learning Velocity — how much new learning this cycle produced
    public double Slv { get; set; }

    // The seed for the next cycle — what should the next Planner focus on?
    public string NextCycleSeed { get; set; } = "";
}
```

The `CrystallisedConstraint` is the most important field. It is the law that will appear in every subsequent cycle's `LockedConstraints`. The Reflector derives it from the Checker's `EarnedConstraints` — not inventing a new constraint, but promoting the one the Checker identified as most significant.

---

## EscalateExecutor — Human in the Loop

The `EscalateExecutor` runs when the Checker emits `ESCALATE`. Its job is to surface the current `CycleState` to a human — via a webhook, a Slack notification, an email, or whatever HIL mechanism is configured — and wait for a correction.

The human's correction is tagged `hil_correction` in the ReflectorFrame (P9) and carries triple the training weight of automated signals. Over time, frequent escalation categories decrease in frequency as the system learns from corrections.

---

## PmcroWorkflowFactory — Wiring It All Together

The `PmcroWorkflowFactory` is registered as a scoped service and is responsible for constructing a fresh `AgentWorkflow` instance for each cycle:

```csharp
public sealed class PmcroWorkflowFactory(
    [FromKeyedServices("planner")] AIAgent plannerAgent,
    [FromKeyedServices("maker")]   AIAgent makerAgent,
    [FromKeyedServices("checker")] AIAgent checkerAgent,
    [FromKeyedServices("reflector")] AIAgent reflectorAgent,
    IMcpToolExecutor toolExecutor,
    IMcpClientRegistry registry,
    ILogger<PmcroWorkflowFactory> logger)
{
    public async Task<CycleState> RunAsync(IntentEnvelope envelope)
    {
        var planner  = new PlannerExecutor(plannerAgent, logger);
        var maker    = new MakerExecutor(makerAgent, logger);
        var dispatch = new DispatchExecutor(toolExecutor, registry, logger);
        var injector = new ConstraintInjectorExecutor(logger);
        var checker  = new CheckerExecutor(checkerAgent, logger);
        var reflector = new ReflectorExecutor(reflectorAgent, logger);
        var escalate = new EscalateExecutor(logger);

        var workflow = new AgentWorkflowBuilder(startExecutor: planner)
            .AddEdge(planner,  maker)
            .AddEdge(maker,    dispatch)
            .AddEdge(dispatch, injector)
            .AddEdge(injector, checker)
            .AddEdge(checker,  reflector)
            .AddConditionalEdge(checker, "loop",    maker)
            .AddConditionalEdge(checker, "escalate", escalate)
            .Build();

        var initial = new CycleState { Envelope = envelope };
        return await workflow.RunAsync(initial);
    }
}
```

Each call to `RunAsync` creates a fresh workflow instance with fresh executor instances. This ensures that no state bleeds between cycles. The `CycleState` is the only carrier of cross-phase information, and it starts fresh for every cycle.

---

> **Next:** [Module 05 — The MCP Layer](05-mcp-layer.md)
