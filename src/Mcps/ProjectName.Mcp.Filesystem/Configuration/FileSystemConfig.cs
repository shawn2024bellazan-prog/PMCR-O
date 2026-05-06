// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.FILESYSTEM
// File       : Configuration/FileSystemConfig.cs
// Identity   : Sandbox Boundary Configuration
// Law Anchor : GOV-001, FRAC-FS-TRAVERSAL-001
// ThoughtLock: 2026-05-01
// ═══════════════════════════════════════════════════════════════════════════════

namespace ProjectName.Mcp.FileSystem.Configuration;

/// <summary>
/// I AM the FileSystemConfig.
/// I define the single root boundary that constrains all file system tool operations.
/// </summary>
/// <remarks>
/// <para>
/// <b>FRAC-FS-TRAVERSAL-001:</b> This path serves as the sandbox root.
/// Implementation uses <see cref="System.IO.Path.GetFullPath(string)"/> to normalize input
/// and blocks all paths attempting to escape this boundary.
/// </para>
/// </remarks>
public sealed class FileSystemConfig
{
    /// <summary>
    /// The absolute path that serves as the root boundary for all <c>FileSystemTools</c> operations.
    /// </summary>
    public required string FileSystemRoot { get; init; }
}