// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Mcp/McpRegistryStartupService.cs
// Identity   : MCP Registry Bootstrapper (IHostedService)
// Law Anchor : ARCH-MCP-NAT-001
// ThoughtLock: 2026-05-05 → MAF+MCP
// ═══════════════════════════════════════════════════════════════════════════════
//
// WHY A HOSTED SERVICE:
//   The MCP clients connect over HTTP at startup time. We cannot await async
//   work inside DI registration lambdas in Program.cs without blocking.
//   The IHostedService pattern is the correct .NET way to run async startup
//   work before the application begins handling requests.
//
//   This service runs StartAsync() before Kestrel opens its port
//   (because we register it before builder.Build()). It blocks HTTP traffic
//   until all MCP clients are ready — matching WaitFor() semantics in AppHost.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ProjectName.OrchestrationApi.Mcp;

/// <summary>
/// I AM the McpRegistryStartupService.
/// I initialise the MCP client connections before the OrchestrationApi
/// accepts its first HTTP request, then register the registry in the DI container
/// so agent factories can pull tools from it at construction time.
/// </summary>
public sealed class McpRegistryStartupService(
    IConfiguration configuration,
    ILogger<McpClientRegistry> logger,
    McpClientRegistryHolder holder) : IHostedService
{
    public async Task StartAsync(CancellationToken ct)
    {
        var registry = await McpClientRegistry.CreateAsync(configuration, logger, ct);
        holder.Registry = registry;
    }

    public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
}

/// <summary>
/// Mutable holder registered as singleton — bridging the IHostedService startup
/// result into the DI container without requiring async DI factory support.
/// </summary>
public sealed class McpClientRegistryHolder
{
    private IMcpClientRegistry? _registry;

    public IMcpClientRegistry Registry
    {
        get => _registry ?? throw new InvalidOperationException(
            "McpClientRegistry has not been initialised yet. " +
            "Ensure McpRegistryStartupService has completed StartAsync() before resolving agents.");
        set => _registry = value;
    }
}