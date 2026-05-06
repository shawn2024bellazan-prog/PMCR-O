using System.Text.Json.Serialization;

namespace ProjectName.Core.Models;

/// <summary>
/// I AM the PlannerResponse. I wrap the truest intent and the ExecutionPlan for MAF output extraction.
/// </summary>
public sealed class PlannerResponse
{
    [JsonPropertyName("truest_intent")]
    public string TruestIntent { get; set; } = string.Empty;

    [JsonPropertyName("plan")]
    public ExecutionPlan? Plan { get; set; }
}