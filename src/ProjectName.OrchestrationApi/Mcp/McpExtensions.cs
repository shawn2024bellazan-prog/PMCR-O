using Microsoft.Extensions.DependencyInjection;

namespace ProjectName.OrchestrationApi.Mcp;

public static class McpExtensions
{
    public static IServiceCollection AddMcpRegistry(this IServiceCollection services)
    {
        services.AddSingleton<McpClientRegistry>();
        return services;
    }
}
