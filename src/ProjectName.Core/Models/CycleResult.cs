namespace ProjectName.Core.Models;

/// <summary>
/// I AM the CycleResult.
/// I am the final payload yielded back through the API membrane to the user.
/// </summary>
public sealed class CycleResult
{
    public string TrailId { get; init; } = string.Empty;
    public string Outcome { get; init; } = string.Empty;
    public string? Message { get; init; }
    public ExecutionPlan? Plan { get; init; }
    public MakerFrame? MakerFrame { get; init; }
    public QualityFrame? QualityFrame { get; init; }
    public ReflectorFrame? ReflectorFrame { get; init; }
    public List<McpToolResult> DispatchResults { get; init; } = [];

    public static CycleResult Accepted(CycleState state) => new()
    {
        TrailId = state.Envelope.TrailId,
        Outcome = "ACCEPTED",
        Plan = state.Plan,
        MakerFrame = state.MakerFrame,
        QualityFrame = state.QualityFrame,
        ReflectorFrame = state.ReflectorFrame,
        DispatchResults = state.DispatchResults
    };

    public static CycleResult Escalated(string trailId, string message) => new()
    {
        TrailId = trailId,
        Outcome = "ESCALATED",
        Message = message
    };
}