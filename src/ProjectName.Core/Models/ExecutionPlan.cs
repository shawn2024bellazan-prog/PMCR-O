using System.Text.Json.Serialization;

namespace ProjectName.Core.Models;

/// <summary>
/// I AM the ExecutionPlan. I am the cognitive map produced by the Planner.
/// </summary>
public sealed class ExecutionPlan
{
    [JsonPropertyName("trail_id")]
    public string TrailId { get; set; } = string.Empty;

    [JsonPropertyName("truest_intent")]
    public required string TruestIntent { get; set; }

    [JsonPropertyName("steps")]
    public required List<PlanStep> Steps { get; set; }
}