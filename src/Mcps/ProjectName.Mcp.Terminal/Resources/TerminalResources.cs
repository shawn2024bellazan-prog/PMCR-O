// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.TERMINAL
// File       : Resources/TerminalResources.cs
// Identity   : Terminal Environment Provider
// Law Anchor : ARCH-TERM-002
// ThoughtLock: 2026-05-01
// ═══════════════════════════════════════════════════════════════════════════════

using ModelContextProtocol.Server;
using ProjectName.Mcp.Terminal.Configuration;
using System.ComponentModel;
using System.Text.Json;

namespace ProjectName.Mcp.Terminal.Resources;

/// <summary>
/// I AM the TerminalResources Provider.
/// I expose environment metadata to the cognitive federation.
/// </summary>
[McpServerResourceType]
public sealed class TerminalResources(TerminalConfig config)
{
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    [McpServerResource(
        UriTemplate = "terminal://environment",
        Name = "TerminalEnvironment",
        Title = "Terminal Environment",
        MimeType = "application/json")]
    [Description("Returns the OS type, working directory, and timeout constraints.")]
    public string GetEnvironment() =>
        JsonSerializer.Serialize(new
        {
            os = OperatingSystem.IsWindows() ? "Windows" : "Unix",
            workspaceRoot = config.WorkspaceRoot,
            defaultTimeoutMs = config.DefaultTimeoutMs,
            maxTimeoutMs = config.MaxTimeoutMs
        }, _json);
}