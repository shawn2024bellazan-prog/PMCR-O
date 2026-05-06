using System.Text.Json.Serialization;

namespace ProjectName.Core.Models;

/// <summary>
/// I AM a PlanStep. I represent a single atomic action within an ExecutionPlan.
/// </summary>
public sealed class PlanStep
{
    [JsonPropertyName("id")]
    public required string Id { get; set; }

    [JsonPropertyName("description")]
    public required string Description { get; set; }

    [JsonPropertyName("type")]
    public required string Type { get; set; }
}