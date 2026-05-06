// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.FILESYSTEM
// File       : Resources/FileSystemResources.cs
// Identity   : Filesystem State Provider
// Law Anchor : ARCH-NEW-001, FRAC-FS-TRAVERSAL-001
// ThoughtLock: 2026-05-01
// ═══════════════════════════════════════════════════════════════════════════════

using ModelContextProtocol.Server;
using ProjectName.Mcp.FileSystem.Configuration;
using System.ComponentModel;
using System.Text.Json;

namespace ProjectName.Mcp.FileSystem.Resources;

/// <summary>
/// I AM the FileSystemResources Provider.
/// I expose sandbox configuration and actuator capabilities to the cognitive federation.
/// </summary>
[McpServerResourceType]
public sealed class FileSystemResources(FileSystemConfig config)
{
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    /// <summary>
    /// Returns the live operational health and active boundary of the filesystem sandbox.
    /// </summary>
    [McpServerResource(
        UriTemplate = "filesystem://status",
        Name = "FileSystemStatus",
        Title = "FileSystem Status",
        MimeType = "application/json")]
    [Description("Live operational health and active sandbox boundary path.")]
    public string GetStatus() =>
        JsonSerializer.Serialize(new
        {
            status = "OPERATIONAL",
            sandboxRoot = config.FileSystemRoot,
            boundaryEnforcement = "ACTIVE",
            timestamp = DateTime.UtcNow.ToString("O")
        }, _json);

    /// <summary>
    /// Returns the capability manifest for this actuator, explicitly distinguishing
    /// TYPE 1 and TYPE 2 tools.
    /// </summary>
    [McpServerResource(
        UriTemplate = "filesystem://capabilities",
        Name = "FileSystemCapabilities",
        Title = "FileSystem Capabilities",
        MimeType = "application/json")]
    [Description("Actuator capability manifest defining TYPE 1 and TYPE 2 operations.")]
    public string GetCapabilities() =>
        JsonSerializer.Serialize(new
        {
            actuator = "ProjectName.Mcp.FileSystem",
            architecture = "ARCH-NEW-001",
            type2_read_tools = new[]
            {
                "ReadFile",
                "ListDirectory",
                "GetFileInfo"
            },
            type1_write_tools = new[]
            {
                "WriteFile",
                "DeletePath"
            },
            security = new[]
            {
                "Path Traversal Prevention (FRAC-FS-TRAVERSAL-001)"
            }
        }, _json);
}