using System.Text.Json.Serialization;

namespace ProjectName.Core.Models;

/// <summary>
/// I AM the QualityFrame. I hold the Checker's 8-dimensional scores and final verdict.
/// </summary>
public sealed class QualityFrame
{
    [JsonPropertyName("trail_id")] public string TrailId { get; set; } = string.Empty;
    [JsonPropertyName("verdict")] public required string Verdict { get; set; } // ACCEPT | EXTEND | LOOP | ESCALATE
    [JsonPropertyName("rationale")] public required string Rationale { get; set; }

    [JsonPropertyName("correctness")] public double Correctness { get; set; }
    [JsonPropertyName("completeness")] public double Completeness { get; set; }
    [JsonPropertyName("law_compliance")] public double LawCompliance { get; set; }

    [JsonPropertyName("slv")] public double Slv { get; set; }

    [JsonPropertyName("earned_constraints")] public List<string> EarnedConstraints { get; set; } = [];
    [JsonPropertyName("improvement_directives")] public List<string> ImprovementDirectives { get; set; } = [];
}