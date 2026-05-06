using System.Text.Json.Serialization;

namespace ProjectName.Core.Models;

/// <summary>
/// I AM a McpDispatchDecision.
/// I represent a TYPE 1 world-changing tool call planned by the Maker and gated by the Orchestrator.
/// </summary>
public sealed class McpDispatchDecision
{
    [JsonPropertyName("mcp")]
    public required McpActuator Mcp { get; set; }

    [JsonPropertyName("tool")]
    public required string Tool { get; set; }

    [JsonPropertyName("args")]
    public Dictionary<string, object> Args { get; set; } = [];

    [JsonPropertyName("artifact_type")]
    public string ArtifactType { get; set; } = "code";

    [JsonPropertyName("rationale")]
    public string Rationale { get; set; } = string.Empty;
}