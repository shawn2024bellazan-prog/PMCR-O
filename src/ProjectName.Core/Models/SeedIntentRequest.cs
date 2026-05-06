namespace ProjectName.Core.Models;

/// <summary>
/// I AM the SeedIntentRequest.
/// I carry the raw, unrefined human input from the API boundary to the Federation Board.
/// </summary>
public sealed class SeedIntentRequest
{
    public string RawIntent { get; set; } = string.Empty;
    public string? HighLevelGoal { get; set; }
    public string? OModeHint { get; set; }
    public string? MasterContext { get; set; }
}