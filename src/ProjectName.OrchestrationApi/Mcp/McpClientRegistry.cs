using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using System.Collections.Concurrent;

namespace ProjectName.OrchestrationApi.Mcp;

/// <summary>
/// I AM the McpClientRegistry.
/// I establish native connections to MCP Actuators and discover their tools.
/// </summary>
public sealed class McpClientRegistry : IAsyncDisposable
{
    private readonly ConcurrentDictionary<string, McpClient> _clients = new();
    public List<AITool> AllDiscoveredTools { get; private set; } = [];

    public List<AITool> Type2Tools => AllDiscoveredTools
        .Where(t => !IsType1(t.Name))
        .ToList();

    public async Task InitialiseAsync(
        string filesystemUrl,
        string terminalUrl,
        string playwrightUrl,
        CancellationToken ct = default)
    {
        await ConnectActuator("filesystem", filesystemUrl, ct);
        await ConnectActuator("terminal", terminalUrl, ct);
        await ConnectActuator("playwright", playwrightUrl, ct);

        var allTools = new List<AITool>();
        foreach (var entry in _clients)
        {
            var client = entry.Value;
            var tools = await client.ListToolsAsync(cancellationToken: ct);

            foreach (var toolDef in tools)
            {
                var tool = AIFunctionFactory.Create(
                    async (Dictionary<string, object?> args) =>
                        await client.CallToolAsync(toolDef.Name, args,
                            cancellationToken: CancellationToken.None),
                    toolDef.Name,
                    toolDef.Description
                );
                allTools.Add(tool);
            }
        }
        AllDiscoveredTools = allTools;
    }

    private async Task ConnectActuator(string name, string url, CancellationToken ct)
    {
        // SDK 1.x: HttpClientTransport handles both SSE and Streamable HTTP automatically.
        // No separate McpClientFactory — CreateAsync is a static method on McpClient itself.
        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri(url)
        });

        var client = await McpClient.CreateAsync(transport, cancellationToken: ct);
        _clients[name] = client;
    }

    public async Task<string> CallToolAsync(
        string actuator, string toolName,
        Dictionary<string, object?> args, CancellationToken ct)
    {
        if (!_clients.TryGetValue(actuator.ToLowerInvariant(), out var client))
            throw new ArgumentException($"Actuator {actuator} not connected.");

        var result = await client.CallToolAsync(toolName, args, cancellationToken: ct);
        return System.Text.Json.JsonSerializer.Serialize(result);
    }

    private static bool IsType1(string toolName)
    {
        string[] type1Verbs = ["Write", "Delete", "RunCommand", "Click", "Fill", "Submit"];
        return type1Verbs.Any(v => toolName.Contains(v, StringComparison.OrdinalIgnoreCase));
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var client in _clients.Values)
            await client.DisposeAsync();
    }
}