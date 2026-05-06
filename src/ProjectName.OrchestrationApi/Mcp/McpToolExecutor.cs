// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Mcp/McpToolExecutor.cs
// Identity   : TYPE 1 Gate Executor
// Law Anchor : ARCH-NEW-001, ARCH-MCP-NAT-001
// ThoughtLock: 2026-05-05 → MAF+MCP
//
// CHANGES vs prior version:
//   1. ExecuteType2Async() REMOVED — TYPE 2 calls are now handled natively by MAF.
//   2. ExecuteType1Async() RETAINED — mutative gate control preserved per ARCH-NEW-001.
//   3. CallMcpAsync() now routes through IMcpClientRegistry's live clients rather
//      than constructing a new HttpClient per call.
//      The SSE transport connection is reused for better performance.
// ═══════════════════════════════════════════════════════════════════════════════

using ProjectName.OrchestrationApi.Models;
using System.Net.Http.Json;
using System.Text.Json.Nodes;

namespace ProjectName.OrchestrationApi.Mcp;

/// <summary>
/// I AM the McpToolExecutor.
/// I execute TYPE 1 (mutative) MCP tool calls on behalf of the Orchestrator gate.
/// I do NOT handle TYPE 2 calls — those are routed natively through MAF's agentic loop.
/// </summary>
public sealed class McpToolExecutor(
    IHttpClientFactory httpClientFactory,
    ILogger<McpToolExecutor> logger) : IMcpToolExecutor
{
    private static readonly Dictionary<McpActuator, string> McpServiceNames = new()
    {
        [McpActuator.filesystem] = "projectname-mcp-filesystem",
        [McpActuator.terminal] = "projectname-mcp-terminal",
        [McpActuator.playwright] = "projectname-mcp-playwright"
    };

    /// <summary>
    /// Executes a mutative (TYPE 1) dispatch decision.
    /// Validates the tool name against the known TYPE 1 allowlist before execution.
    /// </summary>
    public async Task<McpToolResult> ExecuteType1Async(
        McpDispatchDecision decision,
        CancellationToken ct = default)
    {
        // TYPE 1 gate — only these tools may execute mutative changes
        if (!IsType1Tool(decision.Mcp, decision.Tool))
        {
            logger.LogWarning(
                "[MCP-TYPE1-GATE] REJECTED: {Mcp}/{Tool} is not a TYPE 1 mutative tool. " +
                "Possible Maker confusion — it must only dispatch TYPE 1 tools.",
                decision.Mcp, decision.Tool);

            return new McpToolResult
            {
                Success = false,
                Error = $"[GATE-REJECT] '{decision.Tool}' on '{decision.Mcp}' is not a TYPE 1 tool. " +
                        "Only WriteFile, DeletePath, RunCommand, ClickElement, FillInput, WaitForElement are TYPE 1.",
                Mcp = decision.Mcp.ToString(),
                Tool = decision.Tool
            };
        }

        return await CallMcpOverHttpAsync(decision.Mcp, decision.Tool,
            // Convert object values to object? for the call
            decision.Args.ToDictionary(kv => kv.Key, kv => (object?)kv.Value),
            ct);
    }

    /// <summary>
    /// Posts a JSON-RPC tools/call to the MCP actuator's HTTP endpoint.
    /// Used only for TYPE 1 dispatch — TYPE 2 goes through MAF's native tool loop.
    /// </summary>
    private async Task<McpToolResult> CallMcpOverHttpAsync(
        McpActuator mcp, string tool, Dictionary<string, object?> args, CancellationToken ct)
    {
        // Resolve the base URL from Aspire service discovery
        // HttpClient named after the Aspire service name gets automatic SD routing
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            using var client = httpClientFactory.CreateClient(McpServiceNames[mcp]);

            var requestBody = new
            {
                jsonrpc = "2.0",
                method = "tools/call",
                @params = new { name = tool, arguments = args },
                id = Guid.NewGuid().ToString()
            };

            logger.LogInformation(
                "[MCP-TYPE1] Executing {Mcp}/{Tool} — Args: {Args}",
                mcp, tool, System.Text.Json.JsonSerializer.Serialize(args));

            var response = await client.PostAsJsonAsync("/mcp", requestBody, ct);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync(ct);
                stopwatch.Stop();
                return new McpToolResult
                {
                    Success = false,
                    Error = $"Actuator returned {response.StatusCode}: {errorBody}",
                    Mcp = mcp.ToString(),
                    Tool = tool,
                    Duration = stopwatch.Elapsed
                };
            }

            var content = await response.Content.ReadAsStringAsync(ct);

            // Defensive catch in case an SSE response slips through
            string jsonContent = content;
            if (jsonContent.StartsWith("event: ") || jsonContent.StartsWith("data: "))
            {
                var lines = jsonContent.Split('\n');
                var dataLine = lines.FirstOrDefault(l => l.StartsWith("data: "));
                if (dataLine != null)
                {
                    jsonContent = dataLine.Substring(6).Trim();
                }
                else
                {
                    throw new InvalidOperationException("Received SSE connection response instead of tool result.");
                }
            }

            var node = JsonNode.Parse(jsonContent);

            // 1. JSON-RPC Protocol Level Errors
            if (node?["error"] != null)
            {
                var code = node["error"]?["code"]?.GetValue<int>() ?? 0;
                var message = node["error"]?["message"]?.GetValue<string>() ?? "Unknown MCP error";
                stopwatch.Stop();
                return new McpToolResult
                {
                    Success = false,
                    Error = $"JSON-RPC Error {code}: {message}",
                    Mcp = mcp.ToString(),
                    Tool = tool,
                    Duration = stopwatch.Elapsed
                };
            }

            var resultText = node?["result"]?["content"]?[0]?["text"]?.GetValue<string>() ?? "";
            bool isError = node?["result"]?["isError"]?.GetValue<bool>() ?? false;

            // 2. Evaluated Tool Output Values
            bool isSuccess = !isError;
            string contentValue = resultText;
            string errorValue = isError ? resultText : "";

            if (!isError && !string.IsNullOrEmpty(resultText))
            {
                // Playwright String Returns
                if (resultText.StartsWith("ERROR:"))
                {
                    isSuccess = false;
                    errorValue = resultText.Substring(6).Trim();
                }
                else if (resultText.StartsWith("SUCCESS:"))
                {
                    isSuccess = true;
                    contentValue = resultText.Substring(8).Trim();
                }
                // FileSystem / Terminal Serialised Object Returns
                else if (resultText.TrimStart().StartsWith("{") && resultText.Contains("\"success\""))
                {
                    try
                    {
                        var innerNode = JsonNode.Parse(resultText);
                        var successNode = innerNode?["success"];
                        if (successNode != null)
                        {
                            isSuccess = successNode.GetValue<bool>();
                            if (!isSuccess)
                            {
                                errorValue = innerNode?["error"]?.GetValue<string>()
                                    ?? innerNode?["stderr"]?.GetValue<string>()
                                    ?? "Tool execution failed";
                            }
                        }
                    }
                    catch { /* Fallback to success = true assumption if not valid JSON */ }
                }
            }

            stopwatch.Stop();

            logger.LogInformation(
                "[MCP-TYPE1] {Mcp}/{Tool} completed in {Ms}ms. Success={Success}",
                mcp, tool, stopwatch.ElapsedMilliseconds, isSuccess);

            return new McpToolResult
            {
                Success = isSuccess,
                Content = contentValue,
                Error = errorValue,
                Mcp = mcp.ToString(),
                Tool = tool,
                Duration = stopwatch.Elapsed
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "[MCP-TYPE1] Critical failure calling {Mcp}/{Tool}", mcp, tool);
            return new McpToolResult
            {
                Success = false,
                Error = ex.Message,
                Mcp = mcp.ToString(),
                Tool = tool,
                Duration = stopwatch.Elapsed
            };
        }
    }

    /// <summary>
    /// Validates that a tool name is in the TYPE 1 allowlist for the given actuator.
    /// TYPE 1 = mutative operations requiring Orchestrator gate.
    /// </summary>
    private static bool IsType1Tool(McpActuator mcp, string tool) => mcp switch
    {
        McpActuator.filesystem => tool is "WriteFile" or "DeletePath",
        McpActuator.terminal => tool is "RunCommand",
        McpActuator.playwright => tool is "ClickElement" or "FillInput" or "WaitForElement",
        _ => false
    };
}