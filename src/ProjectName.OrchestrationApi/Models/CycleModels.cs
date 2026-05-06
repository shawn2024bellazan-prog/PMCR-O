// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Models/CycleModels.cs
// ThoughtLock: 2026-05-05 → PMCRO v3.1 Alignment
//
// CHANGES vs prior version:
//   1. QualityFrame.Slv ADDED — (GAP-001)
//      CheckerSkill.@output-schema emits "slv": 0.0 and OrchestratorSkill
//      references slv_signal < 0.10 for SLV-001 override. The C# model was
//      missing this field — it was silently dropped on deserialization, making
//      the SLV-001 override permanently inoperative.
//
//   2. PlannerResponse.type2_mcp_calls REMOVED — already done (ARCH-MCP-NAT-001).
//      No change here — retained for reference.
//
//   3. McpToolResult.Duration — nullable (already done). No change.
// ═══════════════════════════════════════════════════════════════════════════════

using System.Text.Json.Serialization;

namespace ProjectName.OrchestrationApi.Models;

// ── ENUMS ──────────────────────────────────────────────────────────────────────

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum McpActuator { filesystem, terminal, playwright }

// ── CORE STATE ─────────────────────────────────────────────────────────────────

/// <summary>
/// I AM the CycleState.
/// I hold the cumulative state of a trail as it moves through the PMCRO graph.
/// </summary>
public sealed class CycleState
{
    public IntentEnvelope Envelope { get; set; } = new();
    public ExecutionPlan? Plan { get; set; }
    public MakerFrame? MakerFrame { get; set; }
    public List<McpToolResult> DispatchResults { get; set; } = [];
    public QualityFrame? QualityFrame { get; set; }
    public ReflectorFrame? ReflectorFrame { get; set; }
}

// ── PLANNER MODELS ─────────────────────────────────────────────────────────────

/// <summary>
/// Planner output schema.
/// type2_mcp_calls REMOVED (ARCH-MCP-NAT-001) — native MAF tool loop handles reconnaissance.
/// </summary>
public sealed class PlannerResponse
{
    [JsonPropertyName("truest_intent")] public string TruestIntent { get; set; } = "";
    [JsonPropertyName("plan")] public ExecutionPlan? Plan { get; set; }
}

public sealed class ExecutionPlan
{
    [JsonPropertyName("trail_id")] public string TrailId { get; set; } = "";
    [JsonPropertyName("truest_intent")] public required string TruestIntent { get; set; }
    [JsonPropertyName("steps")] public required List<PlanStep> Steps { get; set; }
}

public sealed class PlanStep
{
    [JsonPropertyName("id")] public required string Id { get; set; }
    [JsonPropertyName("description")] public required string Description { get; set; }
    [JsonPropertyName("type")] public required string Type { get; set; }
}

// ── MAKER MODELS ───────────────────────────────────────────────────────────────

public sealed class MakerFrame
{
    [JsonPropertyName("trail_id")] public string TrailId { get; set; } = "";
    [JsonPropertyName("artifacts")] public required List<string> Artifacts { get; set; }
    [JsonPropertyName("dispatch_decisions")] public List<McpDispatchDecision> DispatchDecisions { get; set; } = [];
}

/// <summary>
/// TYPE 1 dispatch decision — validated by McpToolExecutor before execution.
/// </summary>
public sealed class McpDispatchDecision
{
    [JsonPropertyName("mcp")] public required McpActuator Mcp { get; set; }
    [JsonPropertyName("tool")] public required string Tool { get; set; }
    [JsonPropertyName("args")] public Dictionary<string, object> Args { get; set; } = [];
    [JsonPropertyName("artifact_type")] public string ArtifactType { get; set; } = "code";
    [JsonPropertyName("rationale")] public string Rationale { get; set; } = "";
}

/// <summary>
/// Result from an MCP tool call (TYPE 1 gate dispatch).
/// </summary>
public sealed class McpToolResult
{
    public bool Success { get; set; }
    public string? Content { get; set; }
    public string? Error { get; set; }
    public string Mcp { get; set; } = "";
    public string Tool { get; set; } = "";
    public TimeSpan? Duration { get; set; }
}

// ── CHECKER MODELS ─────────────────────────────────────────────────────────────

public sealed class QualityFrame
{
    [JsonPropertyName("trail_id")] public string TrailId { get; set; } = "";
    [JsonPropertyName("verdict")] public required string Verdict { get; set; }  // ACCEPT | EXTEND | LOOP
    [JsonPropertyName("rationale")] public required string Rationale { get; set; }
    [JsonPropertyName("correctness")] public double Correctness { get; set; }
    [JsonPropertyName("completeness")] public double Completeness { get; set; }
    [JsonPropertyName("law_compliance")] public double LawCompliance { get; set; }

    // GAP-001 FIX: Slv (Semantic Learning Velocity) added.
    // CheckerSkill.@output-schema emits "slv": 0.0.
    // OrchestratorSkill.@routing-decisions references slv_signal < 0.10 (SLV-001).
    // Without this field the SLV-001 override was permanently inoperative.
    [JsonPropertyName("slv")] public double Slv { get; set; }

    [JsonPropertyName("earned_constraints")] public List<string> EarnedConstraints { get; set; } = [];
}

// ── REFLECTOR MODELS ───────────────────────────────────────────────────────────

public sealed class ReflectorFrame
{
    [JsonPropertyName("trail_id")] public string TrailId { get; set; } = "";
    [JsonPropertyName("cycle_id")] public string CycleId { get; set; } = "";
    [JsonPropertyName("learning_frames")] public List<string> LearningFrames { get; set; } = [];
    [JsonPropertyName("crystallised_constraint")] public string CrystallisedConstraint { get; set; } = "";
    [JsonPropertyName("law_code")] public string LawCode { get; set; } = "";
    [JsonPropertyName("slv")] public double Slv { get; set; }
    [JsonPropertyName("next_cycle_seed")] public string NextCycleSeed { get; set; } = "";
    [JsonPropertyName("locked_cot")] public string LockedCoT { get; set; } = "";
}

// ── CYCLE RESULT ───────────────────────────────────────────────────────────────

public sealed class CycleResult
{
    public string TrailId { get; init; } = "";
    public string Outcome { get; init; } = "";
    public string? Message { get; init; }
    public ExecutionPlan? Plan { get; init; }
    public MakerFrame? MakerFrame { get; init; }
    public QualityFrame? QualityFrame { get; init; }
    public ReflectorFrame? ReflectorFrame { get; init; }
    public List<McpToolResult> DispatchResults { get; init; } = [];

    public static CycleResult Accepted(CycleState state) => new()
    {
        TrailId = state.Envelope.TrailId,
        Outcome = "ACCEPTED",
        Plan = state.Plan,
        MakerFrame = state.MakerFrame,
        QualityFrame = state.QualityFrame,
        ReflectorFrame = state.ReflectorFrame,
        DispatchResults = state.DispatchResults
    };

    public static CycleResult Escalated(string trailId, string message) => new()
    {
        TrailId = trailId,
        Outcome = "ESCALATED",
        Message = message
    };
}