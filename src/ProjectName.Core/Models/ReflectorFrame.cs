using System.Text.Json.Serialization;

namespace ProjectName.Core.Models;

/// <summary>
/// I AM the ReflectorFrame. I hold the crystallized learning and the seed for the next cycle.
/// </summary>
public sealed class ReflectorFrame
{
    [JsonPropertyName("trail_id")] public string TrailId { get; set; } = string.Empty;
    [JsonPropertyName("cycle_id")] public string CycleId { get; set; } = string.Empty;
    [JsonPropertyName("learning_frames")] public List<string> LearningFrames { get; set; } = [];
    [JsonPropertyName("crystallised_constraint")] public string CrystallisedConstraint { get; set; } = string.Empty;
    [JsonPropertyName("law_code")] public string LawCode { get; set; } = string.Empty;
    [JsonPropertyName("slv")] public double Slv { get; set; }
    [JsonPropertyName("next_cycle_seed")] public string NextCycleSeed { get; set; } = string.Empty;
    [JsonPropertyName("locked_cot")] public string LockedCoT { get; set; } = string.Empty;
}