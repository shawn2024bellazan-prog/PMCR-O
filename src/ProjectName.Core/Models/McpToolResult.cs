namespace ProjectName.Core.Models;

/// <summary>
/// I AM the McpToolResult. I capture the actual outcome of a TYPE 1 Orchestrator execution.
/// </summary>
public sealed class McpToolResult
{
    public bool Success { get; set; }
    public string? Content { get; set; }
    public string? Error { get; set; }
    public string Mcp { get; set; } = string.Empty;
    public string Tool { get; set; } = string.Empty;
    public TimeSpan? Duration { get; set; }
}