using System.Text.Json.Serialization;

namespace ProjectName.Core.Models;

/// <summary>
/// I AM the MakerFrame. I carry generated artifacts and planned TYPE 1 dispatch decisions.
/// </summary>
public sealed class MakerFrame
{
    [JsonPropertyName("trail_id")]
    public string TrailId { get; set; } = string.Empty;

    [JsonPropertyName("artifacts")]
    public required List<string> Artifacts { get; set; }

    [JsonPropertyName("dispatch_decisions")]
    public List<McpDispatchDecision> DispatchDecisions { get; set; } = [];
}