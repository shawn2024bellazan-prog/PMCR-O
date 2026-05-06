using ProjectName.Core.Models;
using System.Text.Json.Nodes;

namespace ProjectName.OrchestrationApi.Mcp;

/// <summary>
/// I AM the McpToolExecutor. 
/// I handle the physical execution of TYPE 1 tools.
/// </summary>
public sealed class McpToolExecutor(McpClientRegistry registry, ILogger<McpToolExecutor> logger) : IMcpToolExecutor
{
    public async Task<McpToolResult> ExecuteType1Async(McpActuator actuator, string tool, Dictionary<string, object> args)
    {
        logger.LogWarning("[GATE] Dispatching TYPE 1: {Actuator}.{Tool}", actuator, tool);

        try
        {
            var toolArgs = args.ToDictionary(k => k.Key, v => (object?)v.Value);

            var jsonResult = await registry.CallToolAsync(
                actuator.ToString(),
                tool,
                toolArgs,
                CancellationToken.None
            );

            return new McpToolResult
            {
                Success = true,
                Mcp = actuator.ToString(),
                Tool = tool,
                Content = jsonResult
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[GATE] Execution Failed");
            return new McpToolResult { Success = false, Error = ex.Message };
        }
    }
}
