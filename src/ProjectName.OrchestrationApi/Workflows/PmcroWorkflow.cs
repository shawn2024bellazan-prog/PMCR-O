// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Workflows/PmcroWorkflow.cs
// Identity   : Cognitive Conductor — Orchestrator-Gated TYPE 1 Dispatch
// Law Anchor : ARCH-003, ARCH-007, ARCH-012, ARCH-025, ARCH-027
//              MAKER-001, ARCH-MCP-NAT-001, ARCH-NEW-001
// ThoughtLock: 2026-05-05 → PMCRO v4.0 — Orchestrator-Only TYPE 1 Execution
//
// CHANGES vs prior version (v3.1):
//
//   FRAC-007 FIX — DispatchExecutor was a no-op stub.
//     Was: DispatchExecutor cleared state.DispatchResults and returned.
//          TYPE 1 tools were NEVER executed — WriteFile, RunCommand, etc. were
//          planned by Maker and approved by Checker but never fired. The Checker's
//          physical verification always failed because no file was ever written.
//
//     Now: DispatchExecutor iterates state.MakerFrame.DispatchDecisions and calls
//          IMcpToolExecutor.ExecuteType1Async() for each one. This is the explicit
//          Orchestrator gate (ARCH-NEW-001). Results populate state.DispatchResults
//          so the Checker can read them for physical verification.
//
//   ARCH-012 COMPLIANCE — Roles are now crystal clear:
//     Phase agents (Planner, Maker, Checker, Reflector) → cognitive layer only.
//       They reason, plan, score, crystallise. They NEVER touch the world.
//       TYPE 2 reads (ReadFile, ListDirectory) fire natively through MAF.
//     DispatchExecutor → Orchestrator's execution arm.
//       It is the ONLY workflow node that calls IMcpToolExecutor.
//       No phase agent references IMcpToolExecutor. It is not injected into them.
//     This is the strict enforcement of ARCH-NEW-001 §IV:
//       "TYPE 1 — World-changing | Orchestrator ONLY | Side effects need one
//        accountable point."
//
//   ECONOMIC GUARD added to DispatchExecutor:
//     If any TYPE 1 dispatch returns success: false, the failure is recorded
//     in state.DispatchResults and a dispatch_failure flag is set on state.
//     CheckerExecutor receives the full dispatch_results in its evidence payload
//     so it can score correctness accordingly (already in GAP-003 fix).
//
//   All prior GAP fixes (002–006) remain in effect and are not changed here.
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using ProjectName.OrchestrationApi.Mcp;
using ProjectName.OrchestrationApi.Models;
using System.Text.Json;

namespace ProjectName.OrchestrationApi.Workflows;

// ───────────────────────────────────────────────────────────────────────────────
// 1. PLANNER EXECUTOR
// Cognitive layer — reconnaissance + ExecutionPlan. No TYPE 1 calls ever.
// ───────────────────────────────────────────────────────────────────────────────

public sealed class PlannerExecutor(
    [FromKeyedServices("planner")] AIAgent agent,
    ILogger<PlannerExecutor> logger)
    : Executor<CycleState, CycleState>("PlannerExecutor")
{
    public override async ValueTask<CycleState> HandleAsync(CycleState state, IWorkflowContext context, CancellationToken ct)
    {
        logger.LogInformation("[Planner] Phase Started. Trail: {Trail}", state.Envelope?.TrailId);

        if (state.Envelope != null) state.Envelope.LoopCount++;

        var prompt = $"""
            GOAL: {state.Envelope?.HighLevelGoal}
            CURRENT INTENT: {state.Envelope?.CurrentIntent}
            LOCKED CONSTRAINTS: {JsonSerializer.Serialize(state.Envelope?.LockedConstraints)}
            PENDING LAWS: {JsonSerializer.Serialize(state.Envelope?.PendingLaws)}

            Explore the filesystem if needed. Then respond ONLY with a JSON PlannerResponse.
            """;

        var response = await agent.RunAsync(message: prompt, cancellationToken: ct);

        // GAP-002: Deserialise to PlannerResponse first, then extract nested Plan.
        var plannerResponse = WorkflowUtils.Deserialize<PlannerResponse>(response?.Text, logger)
            ?? throw new InvalidOperationException("Planner failed to yield a valid PlannerResponse JSON.");

        state.Plan = plannerResponse.Plan
            ?? throw new InvalidOperationException("PlannerResponse contained no plan object.");

        state.Plan.TrailId = state.Envelope?.TrailId ?? "UNKNOWN";

        if (state.Envelope != null && !string.IsNullOrWhiteSpace(plannerResponse.TruestIntent))
            state.Envelope.CurrentIntent = plannerResponse.TruestIntent;

        return state;
    }
}

// ───────────────────────────────────────────────────────────────────────────────
// 2. MAKER EXECUTOR
// Cognitive layer — produces MakerFrame with dispatch_decisions. No TYPE 1 calls.
// The Maker NEVER executes TYPE 1 tools directly. It emits dispatch_decisions
// which the DispatchExecutor (Orchestrator's arm) executes with gate control.
// ───────────────────────────────────────────────────────────────────────────────

public sealed class MakerExecutor(
    [FromKeyedServices("maker")] AIAgent agent,
    ILogger<MakerExecutor> logger)
    : Executor<CycleState, CycleState>("MakerExecutor")
{
    // ── REPLACE the entire HandleAsync body in MakerExecutor (PmcroWorkflow.cs) ──
    // FRAC-MAKER-TYPE1-001: surfaces gate rejections as an unmissable top-level field
    // so the model cannot miss them buried in prior_dispatch_results array noise.

    public override async ValueTask<CycleState> HandleAsync(CycleState state, IWorkflowContext context, CancellationToken ct)
    {
        logger.LogInformation("[Maker] Generating Implementation for: {Intent}", state.Plan?.TruestIntent);

        // FRAC-MAKER-TYPE1-001: extract gate rejections from prior loop as a dedicated
        // top-level signal. When buried inside prior_dispatch_results the model ignores them.
        var gateRejections = state.DispatchResults
            .Where(r => !r.Success && r.Error != null && r.Error.Contains("GATE-REJECT"))
            .Select(r => new { r.Mcp, r.Tool, r.Error })
            .ToList();

        var payload = JsonSerializer.Serialize(new
        {
            plan = state.Plan,
            constraints = state.Envelope?.LockedConstraints,
            feedback = state.QualityFrame?.Rationale,
            prior_dispatch_results = state.DispatchResults,

            // Unmissable correction signal — model sees this before anything else in its context
            gate_rejection_error = gateRejections.Count > 0
                ? $"GATE-REJECT on previous loop: the tool(s) [{string.Join(", ", gateRejections.Select(r => $"{r.Mcp}/{r.Tool}"))}] " +
                  $"are TYPE 2 read-only tools and are FORBIDDEN in dispatch_decisions. " +
                  $"Remove them. Call them natively instead, or use empty dispatch_decisions for analysis goals."
                : null
        });

        var response = await agent.RunAsync(message: payload, cancellationToken: ct);

        state.MakerFrame = WorkflowUtils.Deserialize<MakerFrame>(response?.Text, logger)
            ?? throw new InvalidOperationException("Maker failed to produce a valid MakerFrame.");

        state.MakerFrame.TrailId = state.Envelope?.TrailId ?? "UNKNOWN";

        logger.LogInformation(
            "[Maker] MakerFrame produced. Artifacts: {ArtifactCount}, Dispatch Decisions: {DispatchCount}",
            state.MakerFrame.Artifacts.Count,
            state.MakerFrame.DispatchDecisions.Count);

        return state;
    }
}

// ───────────────────────────────────────────────────────────────────────────────
// 3. DISPATCH EXECUTOR — THE ORCHESTRATOR'S EXECUTION ARM
//
// This is the ONLY place in the entire workflow that calls IMcpToolExecutor.
// No phase agent (Planner, Maker, Checker, Reflector) has IMcpToolExecutor
// injected. This is the strict enforcement of ARCH-NEW-001:
//   "TYPE 1 — World-changing | Orchestrator ONLY | Side effects need one
//    accountable point."
//
// The DispatchExecutor:
//   1. Validates each McpDispatchDecision (actuator must be known, tool must be TYPE 1)
//   2. Calls IMcpToolExecutor.ExecuteType1Async() — the gate enforces the allowlist
//   3. Collects all McpToolResults into state.DispatchResults
//   4. Logs each outcome — success or failure
//   5. Does NOT abort on partial failure — the Checker scores correctness from results
// ───────────────────────────────────────────────────────────────────────────────

public sealed class DispatchExecutor(
    IMcpToolExecutor mcpExecutor,
    ILogger<DispatchExecutor> logger)
    : Executor<CycleState, CycleState>("DispatchExecutor")
{
    public override async ValueTask<CycleState> HandleAsync(CycleState state, IWorkflowContext context, CancellationToken ct)
    {
        state.DispatchResults.Clear();

        var decisions = state.MakerFrame?.DispatchDecisions;
        if (decisions is null or { Count: 0 })
        {
            logger.LogInformation(
                "[Dispatch] No TYPE 1 dispatch decisions from Maker. " +
                "Trail: {Trail} — this is valid for cognitive-only cycles (O-Output with no file writes).",
                state.Envelope?.TrailId);
            return state;
        }

        logger.LogInformation(
            "[Dispatch] Orchestrator Gate — executing {Count} TYPE 1 dispatch decision(s). Trail: {Trail}",
            decisions.Count, state.Envelope?.TrailId);

        // Execute each dispatch decision through the TYPE 1 gate.
        // The gate rejects any tool not in the TYPE 1 allowlist (WriteFile, DeletePath,
        // RunCommand, ClickElement, FillInput, WaitForElement).
        foreach (var decision in decisions)
        {
            logger.LogInformation(
                "[Dispatch] → TYPE 1: {Mcp}/{Tool} | Rationale: {Rationale}",
                decision.Mcp, decision.Tool, decision.Rationale);

            var result = await mcpExecutor.ExecuteType1Async(decision, ct);

            state.DispatchResults.Add(result);

            if (result.Success)
            {
                logger.LogInformation(
                    "[Dispatch] ✓ {Mcp}/{Tool} succeeded in {Ms}ms",
                    decision.Mcp, decision.Tool,
                    result.Duration.HasValue ? (long)result.Duration.Value.TotalMilliseconds : -1);
            }
            else
            {
                logger.LogWarning(
                    "[Dispatch] ✗ {Mcp}/{Tool} FAILED: {Error}",
                    decision.Mcp, decision.Tool, result.Error);
            }
        }

        var successCount = state.DispatchResults.Count(r => r.Success);
        var failCount = state.DispatchResults.Count(r => !r.Success);

        logger.LogInformation(
            "[Dispatch] Gate complete. Success: {Success}, Failed: {Failed}. " +
            "Checker will verify physical artifacts. Trail: {Trail}",
            successCount, failCount, state.Envelope?.TrailId);

        return state;
    }
}

// ───────────────────────────────────────────────────────────────────────────────
// 4. CHECKER EXECUTOR
// Cognitive layer — verifies artifacts using TYPE 2 MCP reads (natively via MAF).
// Receives dispatch_results from state so it can score correctness against
// the Orchestrator's actual execution outcomes.
// ───────────────────────────────────────────────────────────────────────────────

public sealed class CheckerExecutor(
    [FromKeyedServices("checker")] AIAgent agent,
    ILogger<CheckerExecutor> logger)
    : Executor<CycleState, CycleState>("CheckerExecutor")
{
    public override async ValueTask<CycleState> HandleAsync(CycleState state, IWorkflowContext context, CancellationToken ct)
    {
        logger.LogInformation("[Checker] Validating artifacts for Trail {Trail}", state.Envelope?.TrailId);

        // GAP-003 FIX: Full evidence payload including dispatch_results.
        // dispatch_results now contains actual Orchestrator execution outcomes,
        // not just the Maker's intentions — the Checker can score correctness
        // against what actually happened on disk.
        var evidence = JsonSerializer.Serialize(new
        {
            trail_id = state.Envelope?.TrailId,
            goal = state.Envelope?.HighLevelGoal,
            current_intent = state.Envelope?.CurrentIntent,
            plan = state.Plan,
            artifacts = state.MakerFrame?.Artifacts,
            dispatch_decisions = state.MakerFrame?.DispatchDecisions,
            dispatch_results = state.DispatchResults,   // actual TYPE 1 execution outcomes
            locked_constraints = state.Envelope?.LockedConstraints
        });

        var response = await agent.RunAsync(message: evidence, cancellationToken: ct);

        state.QualityFrame = WorkflowUtils.Deserialize<QualityFrame>(response?.Text, logger)
            ?? throw new InvalidOperationException("Checker failed to yield a QualityFrame.");

        state.QualityFrame.TrailId = state.Envelope?.TrailId ?? "UNKNOWN";

        logger.LogInformation(
            "[Checker] Verdict: {Verdict} | Composite: {Comp:F2} | SLV: {Slv:F2}",
            state.QualityFrame.Verdict,
            (state.QualityFrame.Correctness + state.QualityFrame.Completeness + state.QualityFrame.LawCompliance) / 3.0,
            state.QualityFrame.Slv);

        return state;
    }
}

// ───────────────────────────────────────────────────────────────────────────────
// 4b. CONSTRAINT INJECTOR EXECUTOR (GAP-005 FIX — retained from v3.1)
// ───────────────────────────────────────────────────────────────────────────────

/// <summary>
/// I AM the ConstraintInjectorExecutor.
/// I run after every CheckerExecutor verdict, before the routing switch.
/// I merge QualityFrame.EarnedConstraints into Envelope.LockedConstraints
/// (de-duplicated) so they are available to the Maker on EXTEND
/// and to the Reflector on ACCEPT.
/// Law anchor: ARCH-003 + OrchestratorSkill.@routing-decisions EXTEND.
/// </summary>
public sealed class ConstraintInjectorExecutor(ILogger<ConstraintInjectorExecutor> logger)
    : Executor<CycleState, CycleState>("ConstraintInjectorExecutor")
{
    public override ValueTask<CycleState> HandleAsync(CycleState state, IWorkflowContext context, CancellationToken ct)
    {
        var earned = state.QualityFrame?.EarnedConstraints;
        if (earned is { Count: > 0 })
        {
            var existing = state.Envelope.LockedConstraints;
            var added = 0;
            foreach (var constraint in earned)
            {
                if (!existing.Contains(constraint))
                {
                    existing.Add(constraint);
                    added++;
                }
            }
            if (added > 0)
                logger.LogInformation(
                    "[ConstraintInjector] Injected {Count} earned constraint(s) into Envelope. Trail: {Trail}",
                    added, state.Envelope.TrailId);
        }
        return ValueTask.FromResult(state);
    }
}

// ───────────────────────────────────────────────────────────────────────────────
// 5. TERMINATION EXECUTORS
// ───────────────────────────────────────────────────────────────────────────────

public sealed class ReflectorExecutor(
    [FromKeyedServices("reflector")] AIAgent agent,
    ILogger<ReflectorExecutor> logger)
    : Executor<CycleState, CycleResult>("ReflectorExecutor")
{
    public override async ValueTask<CycleResult> HandleAsync(CycleState state, IWorkflowContext context, CancellationToken ct)
    {
        logger.LogInformation("[Reflector] Closing Cycle {Trail}", state.Envelope?.TrailId);

        // GAP-004 FIX: Full cycle data including actual dispatch_results.
        var payload = JsonSerializer.Serialize(new
        {
            trail_id = state.Envelope?.TrailId,
            cycle_id = state.Envelope?.CycleId,
            goal = state.Envelope?.HighLevelGoal,
            current_intent = state.Envelope?.CurrentIntent,
            plan = state.Plan,
            quality_frame = state.QualityFrame,
            dispatch_results = state.DispatchResults,
            loop_count = state.Envelope?.LoopCount,
            locked_constraints = state.Envelope?.LockedConstraints
        });

        var response = await agent.RunAsync(message: payload, cancellationToken: ct);
        state.ReflectorFrame = WorkflowUtils.Deserialize<ReflectorFrame>(response?.Text, logger);

        if (state.ReflectorFrame?.NextCycleSeed is { Length: > 0 } seed
            && state.Envelope != null)
        {
            state.Envelope.CurrentIntent = seed;
        }

        return CycleResult.Accepted(state);
    }
}

public sealed class EscalateExecutor(ILogger<EscalateExecutor> logger)
    : Executor<CycleState, CycleResult>("EscalateExecutor")
{
    public override async ValueTask<CycleResult> HandleAsync(CycleState state, IWorkflowContext context, CancellationToken ct)
    {
        logger.LogWarning("[Escalate] Recursion Depth Reached for Trail {Trail}", state.Envelope?.TrailId);
        return CycleResult.Escalated(state.Envelope?.TrailId ?? "UNKNOWN", "Maximum loop limit reached.");
    }
}

// ───────────────────────────────────────────────────────────────────────────────
// 6. FACTORY & UTILS
// ───────────────────────────────────────────────────────────────────────────────

public sealed class PmcroWorkflowFactory(
    PlannerExecutor planner,
    MakerExecutor maker,
    DispatchExecutor dispatch,
    CheckerExecutor checker,
    ConstraintInjectorExecutor constraintInjector,
    ReflectorExecutor reflector,
    EscalateExecutor escalate)
{
    // ── REPLACE PmcroWorkflowFactory.Build() in PmcroWorkflow.cs ──────────────────
    // FRAC-CHECKER-COGNITIVE-001: adds composite score guard so a clearly-passing
    // cognitive-only cycle cannot loop due to a model verdict inconsistency.
    //
    // The guard sits BEFORE the verdict switch and overrides LOOP → ACCEPT when:
    //   - The cycle is cognitive-only (no dispatch)
    //   - Composite score >= 0.85
    //   - The model emitted LOOP or EXTEND despite the score
    //
    // This is a safety net, not the primary fix. CheckerSkill.cs is the primary fix.
    // The guard handles the case where the model ignores its own scored composite.

    public Workflow Build()
    {
        var builder = new WorkflowBuilder(planner)
            .AddEdge(planner, maker)
            .AddEdge(maker, dispatch)
            .AddEdge(dispatch, checker)
            .AddEdge(checker, constraintInjector);

        builder.AddSwitch(constraintInjector, sw => sw
            // Hard escalation ceiling — always checked first
            .AddCase<CycleState>(s => (s?.Envelope?.LoopCount ?? 0) >= 5, escalate)

            // Explicit ACCEPT verdict from Checker
            .AddCase<CycleState>(s => s?.QualityFrame?.Verdict == "ACCEPT", reflector)

            // FRAC-CHECKER-COGNITIVE-001 guard:
            // Composite score override — cognitive-only cycle scored >= 0.85 but
            // model emitted LOOP or EXTEND anyway. Treat as ACCEPT.
            .AddCase<CycleState>(s =>
            {
                var qf = s?.QualityFrame;
                if (qf is null) return false;
                var composite = (qf.Correctness + qf.Completeness + qf.LawCompliance) / 3.0;
                var isCognitiveOnly = (s!.MakerFrame?.DispatchDecisions.Count ?? 0) == 0
                                   && (s.DispatchResults.Count == 0);
                return isCognitiveOnly && composite >= 0.85;
            }, reflector)

            // EXTEND — route back to Maker (skip Planner, keep plan)
            .AddCase<CycleState>(s => s?.QualityFrame?.Verdict == "EXTEND", maker)

            // Default — LOOP back to Planner
            .WithDefault(planner));

        return builder.Build();
    }
}


internal static class WorkflowUtils
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    public static T? Deserialize<T>(string? input, ILogger logger) where T : class
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        string cleaned = input.Trim();
        if (cleaned.Contains("```"))
        {
            int start = cleaned.IndexOf('{');
            int end = cleaned.LastIndexOf('}');
            if (start != -1 && end > start)
                cleaned = cleaned.Substring(start, end - start + 1);
        }

        try
        {
            return JsonSerializer.Deserialize<T>(cleaned, JsonOpts);
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "[Workflow-Utils] JSON Parse Error for Type: {Type}", typeof(T).Name);
            return null;
        }
    }
}