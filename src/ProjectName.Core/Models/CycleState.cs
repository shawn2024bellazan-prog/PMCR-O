namespace ProjectName.Core.Models;

/// <summary>
/// I AM the CycleState.
/// I hold the cumulative state of a trail as it moves through the PMCRO graph.
/// I am passed between gRPC nodes during execution.
/// </summary>
public sealed class CycleState
{
    public IntentEnvelope Envelope { get; set; } = new();
    public ExecutionPlan? Plan { get; set; }
    public MakerFrame? MakerFrame { get; set; }
    public List<McpToolResult> DispatchResults { get; set; } = [];
    public QualityFrame? QualityFrame { get; set; }
    public ReflectorFrame? ReflectorFrame { get; set; }
}