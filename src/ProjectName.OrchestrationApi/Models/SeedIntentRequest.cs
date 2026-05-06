// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Models/SeedIntentRequest.cs (or local to IntentController)
// Identity   : The Initial Intent Carrier
// Law Anchor : P1 (Seed vs True Intent)
// ThoughtLock: 2026-05-06 → Adhering to EC-005 Documentation Standards
// ═══════════════════════════════════════════════════════════════════════════════

namespace ProjectName.OrchestrationApi.Models;

/// <summary>
/// I AM the SeedIntentRequest.
/// I carry the raw, unrefined human input from the API boundary to the Federation Board.
/// </summary>
/// <remarks>
/// <para><b>Primitive 1:</b> Represents the "Messy" input that requires refinement.</para>
/// <para><b>Primitive 11:</b> Carries the <see cref="MasterContext"/> for identity injection.</para>
/// </remarks>
public sealed class SeedIntentRequest
{
    /// <summary>
    /// The verbatim input from the human (Speech-to-text, raw text, or malformed commands).
    /// I NEVER discard this data as it represents the "Seed" of the cycle.
    /// </summary>
    public string RawIntent { get; set; } = string.Empty;

    /// <summary>
    /// An optional override for the High-Level Goal. 
    /// If null, the Federation Board will derive the goal from the <see cref="RawIntent"/>.
    /// </summary>
    public string? HighLevelGoal { get; set; }

    /// <summary>
    /// A classification hint for the Federation Board to determine the cycle's O-Mode.
    /// </summary>
    /// <example>O-Output, O-Optimize, O-Orchestrate</example>
    public string? OModeHint { get; set; }

    /// <summary>
    /// Cumulative context from previous trails or external identity data.
    /// Used for "Dependency Injection" of agent expertise (Primitive 11).
    /// </summary>
    public string? MasterContext { get; set; }
}