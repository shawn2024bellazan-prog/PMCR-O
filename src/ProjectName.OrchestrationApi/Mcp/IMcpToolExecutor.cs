using ProjectName.Core.Models;

namespace ProjectName.OrchestrationApi.Mcp;

/// <summary>
/// I AM the IMcpToolExecutor.
/// I define the boundary for TYPE 1 world-changing tool execution.
/// </summary>
public interface IMcpToolExecutor
{
    Task<McpToolResult> ExecuteType1Async(McpActuator actuator, string tool, Dictionary<string, object> args);
}