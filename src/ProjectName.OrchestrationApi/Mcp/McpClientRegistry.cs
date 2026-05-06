// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Mcp/McpClientRegistry.cs
// Identity   : Native MCP Client Pool — Startup Bootstrapper
// Law Anchor : ARCH-NEW-001, ARCH-MCP-NAT-001, FRAC-CTX-001
// ThoughtLock: 2026-05-05 → MAF+MCP
// ═══════════════════════════════════════════════════════════════════════════════
//
// HOW THIS WORKS:
//   1. At application startup, CreateAsync() is called once.
//   2. For each MCP actuator (filesystem, terminal, playwright), we create an
//      McpClient using HttpClientTransport pointing at the Aspire service URL.
//   3. ListToolsAsync() fetches the tool manifest from each MCP server.
//   4. The tools are stored as IReadOnlyList<AITool> and injected into agent
//      registrations via ChatClientAgentOptions.Tools.
//   5. MAF's ChatClientAgent loop then owns all tool invocation — when the
//      model emits a tool_calls block, MAF calls the MCP client, returns the
//      result, and re-calls the model. Zero manual dispatch for TYPE 2.
//
// SDK API (ModelContextProtocol 1.x — verified against official docs):
//   Transport : HttpClientTransport + HttpClientTransportOptions  (ModelContextProtocol.Client)
//   Factory   : McpClient.CreateAsync(transport, ...)             (static on McpClient)
//   Client    : McpClient (concrete class — IMcpClient was removed in 1.x)
//
//   DOES NOT EXIST in 1.x (all prior errors were caused by these):
//     SseClientTransport, SseClientTransportOptions  → CS0246
//     ModelContextProtocol.Protocol.Transport        → CS0234
//     McpClientFactory                               → CS0103
//     IMcpClient                                     → CS0246
//
// RESILIENCE:
//   Each actuator connect is attempted independently. If one fails (e.g. playwright
//   not running), the others still load. The registry returns an empty list for
//   failed actuators, and agents that depend on them degrade gracefully rather than
//   crashing the entire OrchestrationApi.
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.AI;
using ModelContextProtocol.Client; // HttpClientTransport, HttpClientTransportOptions, McpClient, McpClientTool

namespace ProjectName.OrchestrationApi.Mcp;

/// <summary>
/// I AM the McpClientRegistry.
/// I connect to all MCP actuators at startup, enumerate their tools, and hold
/// live <see cref="McpClient"/> instances for the lifetime of the application.
/// </summary>
public sealed class McpClientRegistry : IMcpClientRegistry
{
    private readonly List<McpClient> _clients = [];
    private IReadOnlyList<AITool> _filesystemTools = [];
    private IReadOnlyList<AITool> _terminalTools = [];
    private IReadOnlyList<AITool> _playwrightTools = [];

    public IReadOnlyList<AITool> FilesystemTools => _filesystemTools;
    public IReadOnlyList<AITool> TerminalTools => _terminalTools;
    public IReadOnlyList<AITool> PlaywrightTools => _playwrightTools;

    // TYPE 2 = ReadFile, ListDirectory, GetFileInfo, RunReadOnlyCommand, NavigateTo, GetPageContent, TakeScreenshot
    // This allowlist matches ARCH-NEW-001 TYPE 2 definitions.
    private static readonly HashSet<string> Type2ToolNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "ReadFile", "ListDirectory", "GetFileInfo",         // filesystem TYPE 2
        "RunReadOnlyCommand",                                // terminal TYPE 2
        "NavigateTo", "GetPageContent", "TakeScreenshot"    // playwright TYPE 2
    };

    public IReadOnlyList<AITool> AllType2Tools =>
        _filesystemTools.Concat(_terminalTools).Concat(_playwrightTools)
            .Where(t => Type2ToolNames.Contains(t.Name))
            .ToList();

    public IReadOnlyList<AITool> AllTools =>
        _filesystemTools.Concat(_terminalTools).Concat(_playwrightTools).ToList();

    private McpClientRegistry() { }

    /// <summary>
    /// Factory — connects to all MCP actuators, enumerates tools, returns a ready registry.
    /// Call once at startup via McpRegistryStartupService (IHostedService).
    /// </summary>
    public static async Task<McpClientRegistry> CreateAsync(
        IConfiguration configuration,
        ILogger<McpClientRegistry> logger,
        CancellationToken ct = default)
    {
        var registry = new McpClientRegistry();

        // Resolve base URLs from Aspire service discovery / appsettings.
        // Aspire injects: services__projectname-mcp-filesystem__http__0
        // IConfiguration surfaces as: services:projectname-mcp-filesystem:http:0
        var filesystemUrl = ResolveServiceUrl(configuration, "projectname-mcp-filesystem",
            fallback: "http://localhost:5010");
        var terminalUrl = ResolveServiceUrl(configuration, "projectname-mcp-terminal",
            fallback: "http://localhost:5011");
        var playwrightUrl = ResolveServiceUrl(configuration, "projectname-mcp-playwright",
            fallback: "http://localhost:5012");

        registry._filesystemTools = await registry.LoadToolsAsync(
            "filesystem", filesystemUrl, logger, ct);

        registry._terminalTools = await registry.LoadToolsAsync(
            "terminal", terminalUrl, logger, ct);

        registry._playwrightTools = await registry.LoadToolsAsync(
            "playwright", playwrightUrl, logger, ct);

        logger.LogInformation(
            "[McpClientRegistry] Loaded: filesystem={Fs} terminal={Term} playwright={Pw} tools",
            registry._filesystemTools.Count,
            registry._terminalTools.Count,
            registry._playwrightTools.Count);

        return registry;
    }

    private async Task<IReadOnlyList<AITool>> LoadToolsAsync(
        string name, string baseUrl, ILogger logger, CancellationToken ct)
    {
        try
        {
            logger.LogInformation("[McpClientRegistry] Connecting to {Name} at {Url}", name, baseUrl);

            // HttpClientTransport is the correct HTTP client transport in ModelContextProtocol 1.x.
            // McpClient.CreateAsync is the static factory (McpClientFactory was removed in 1.x).
            var transport = new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri($"{baseUrl}/mcp"),
            });

            var client = await McpClient.CreateAsync(transport, cancellationToken: ct);
            _clients.Add(client);

            var tools = await client.ListToolsAsync(cancellationToken: ct);
            var aiTools = tools.Cast<AITool>().ToList();

            // AITool exposes .Name and .Description directly — no .Metadata wrapper.
            logger.LogInformation(
                "[McpClientRegistry] {Name}: {Count} tools — [{Names}]",
                name, aiTools.Count, string.Join(", ", aiTools.Select(t => t.Name)));

            return aiTools;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "[McpClientRegistry] Failed to connect to {Name} MCP at {Url}. " +
                "Tools for this actuator will be unavailable. " +
                "Agents will degrade gracefully (TYPE 2 calls to {Name} will return errors).",
                name, baseUrl, name);
            return [];
        }
    }

    /// <summary>
    /// Resolves the Aspire service URL from configuration.
    /// Aspire injects: services__projectname-mcp-filesystem__http__0 as env var.
    /// IConfiguration maps __ → :, so the key is: services:projectname-mcp-filesystem:http:0
    /// </summary>
    private static string ResolveServiceUrl(
        IConfiguration configuration, string serviceName, string fallback)
    {
        var value = configuration[$"services:{serviceName}:http:0"]
                 ?? configuration[$"services:{serviceName}:https:0"]
                 ?? fallback;

        return value.TrimEnd('/');
    }

    public async ValueTask DisposeAsync()
    {
        foreach (var client in _clients)
        {
            await client.DisposeAsync();
        }
        _clients.Clear();
    }
}