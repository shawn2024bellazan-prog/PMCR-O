// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Models/IntentEnvelope.cs
// Identity   : The Bloodstream — every phase reads and augments this
// Law Anchor : ARCH-025 (I AM the evolving IntentEnvelope), ARCH-027
// ThoughtLock: 2026-05-05 → PMCRO v4.0 — Federation-First Architecture
//
// CHANGES vs prior version:
//   1. FederationShielded default changed from false → false (unchanged) but
//      the field is now VALIDATED at the controller boundary by IntentController.
//      Raw envelopes with federation_shielded: false are rejected at intake.
//      FederationBoardSkill always seals with federation_shielded: true.
//
//   2. PendingLaws field ADDED — carries unratified constraints surfaced during
//      Federation Board refinement. The Reflector promotes these to earned laws.
//
//   3. TrailId generation updated to include a short random suffix to prevent
//      collision when multiple cycles start within the same second.
//
// SCHEMA ALIGNMENT (PMCRO Framework SKILL.md §II):
//   trail_id         ✓
//   origin           ✓
//   high_level_goal  ✓ (immutable across all cycles)
//   current_intent   ✓ (updated by each phase)
//   o_mode           ✓
//   economic_check   ✓
//   aoc_invited      ✓
//   locked_constraints ✓
//   loop_count       ✓
//   federation_shielded ✓ (MANDATORY — Orchestrator rejects if false)
//   master_context   ✓ (extension for identity injection)
//   cycle_id         ✓ (per-cycle identifier, survives EXTEND/LOOP)
//   created_at       ✓
//   pending_laws     ✓ (NEW — carries unratified constraints from Federation Board)
// ═══════════════════════════════════════════════════════════════════════════════

using System.Text.Json.Serialization;

namespace ProjectName.OrchestrationApi.Models;

/// <summary>
/// I AM the IntentEnvelope — the bloodstream of the PMCRO loop.
/// I carry the high-level goal and evolving state for a PMCRO Trail.
/// Every phase reads and augments me. No field is optional once I am sealed.
/// I MUST have federation_shielded: true before entering the Orchestrator.
/// </summary>
public sealed class IntentEnvelope
{
    /// <summary>
    /// Unique trail identifier. Format: PMCRO-YYYYMMDD-[6-char hex].
    /// Immutable once set by the Federation Board.
    /// </summary>
    [JsonPropertyName("trail_id")]
    public string TrailId { get; set; } =
        $"PMCRO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

    /// <summary>
    /// Intent origin tag. One of: federation | round_table | human_refined.
    /// Set by FederationBoardSkill. NEVER "raw".
    /// </summary>
    [JsonPropertyName("origin")]
    public string Origin { get; set; } = "federation";

    /// <summary>
    /// The immutable goal. Set once at Federation Board. NEVER changed by loop phases.
    /// </summary>
    [JsonPropertyName("high_level_goal")]
    public string HighLevelGoal { get; set; } = string.Empty;

    /// <summary>
    /// The current cycle's specific target. Updated by Planner (truest_intent promotion)
    /// and by Reflector (next_cycle_seed promotion) between cycles.
    /// </summary>
    [JsonPropertyName("current_intent")]
    public string CurrentIntent { get; set; } = string.Empty;

    /// <summary>
    /// O-Mode classification. Set by Federation Board. Validated by Orchestrator.
    /// One of: O-Output | O-Optimize | O-Orchestrate | O-Chain | O-Tree | O-Graph.
    /// </summary>
    [JsonPropertyName("o_mode")]
    public string OMode { get; set; } = "O-Output";

    /// <summary>
    /// Economic gate status. Set by Federation Board. "passed" | "failed".
    /// Orchestrator routes INTERRUPT if "failed".
    /// </summary>
    [JsonPropertyName("economic_check")]
    public string EconomicCheck { get; set; } = "passed";

    /// <summary>
    /// ArchitectOfCognition invitation flag. Always true — ARCH-008.
    /// </summary>
    [JsonPropertyName("aoc_invited")]
    public bool AocInvited { get; set; } = true;

    /// <summary>
    /// Constraints locked into this cycle. Populated by ConstraintInjectorExecutor
    /// from QualityFrame.EarnedConstraints after each Checker verdict.
    /// The Orchestrator injects these into each new cycle's envelope on EXTEND/LOOP.
    /// </summary>
    [JsonPropertyName("locked_constraints")]
    public List<string> LockedConstraints { get; set; } = [];

    /// <summary>
    /// Counts the number of times this trail has looped.
    /// Incremented by PlannerExecutor at the start of each cycle.
    /// Orchestrator routes ESCALATE when loop_count > 3.
    /// </summary>
    [JsonPropertyName("loop_count")]
    public int LoopCount { get; set; } = 0;

    /// <summary>
    /// Federation shield. MUST be true before entering the Orchestrator.
    /// FederationBoardSkill always seals with true.
    /// IntentController rejects envelopes with false at the API boundary.
    /// </summary>
    [JsonPropertyName("federation_shielded")]
    public bool FederationShielded { get; set; } = false;

    /// <summary>
    /// Master context from previous sessions or identity injection.
    /// Injected by company-identity-agent at activation time.
    /// Preserved across cycles — never cleared by the loop.
    /// </summary>
    [JsonPropertyName("master_context")]
    public string MasterContext { get; set; } = string.Empty;

    /// <summary>
    /// Per-cycle unique identifier. Survives EXTEND (same cycle continues).
    /// Reset on LOOP (new cycle begins from Planner).
    /// </summary>
    [JsonPropertyName("cycle_id")]
    public string CycleId { get; set; } = Guid.NewGuid().ToString("N")[..8];

    /// <summary>
    /// UTC timestamp of envelope creation by Federation Board.
    /// </summary>
    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Unratified constraints surfaced by the Federation Board during refinement.
    /// The Reflector promotes these to earned laws (EC-NNN) at cycle close.
    /// </summary>
    [JsonPropertyName("pending_laws")]
    public List<string> PendingLaws { get; set; } = [];
}