using System.Text.Json.Serialization;

namespace ProjectName.Core.Models;

/// <summary>
/// I AM the IntentEnvelope — the bloodstream of the PMCRO loop.
/// I carry the high-level goal and evolving state for a PMCRO Trail.
/// </summary>
public sealed class IntentEnvelope
{
    [JsonPropertyName("trail_id")]
    public string TrailId { get; set; } = $"PMCRO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}";

    [JsonPropertyName("origin")]
    public string Origin { get; set; } = "federation";

    [JsonPropertyName("high_level_goal")]
    public string HighLevelGoal { get; set; } = string.Empty;

    [JsonPropertyName("current_intent")]
    public string CurrentIntent { get; set; } = string.Empty;

    [JsonPropertyName("o_mode")]
    public string OMode { get; set; } = "O-Output";

    [JsonPropertyName("economic_check")]
    public string EconomicCheck { get; set; } = "passed";

    [JsonPropertyName("aoc_invited")]
    public bool AocInvited { get; set; } = true;

    [JsonPropertyName("locked_constraints")]
    public List<string> LockedConstraints { get; set; } = [];

    [JsonPropertyName("loop_count")]
    public int LoopCount { get; set; } = 0;

    [JsonPropertyName("federation_shielded")]
    public bool FederationShielded { get; set; } = false;

    [JsonPropertyName("master_context")]
    public string MasterContext { get; set; } = string.Empty;

    [JsonPropertyName("cycle_id")]
    public string CycleId { get; set; } = Guid.NewGuid().ToString("N")[..8];

    [JsonPropertyName("created_at")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    [JsonPropertyName("pending_laws")]
    public List<string> PendingLaws { get; set; } = [];
}