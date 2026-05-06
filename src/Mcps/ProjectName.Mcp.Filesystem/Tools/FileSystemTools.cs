// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.FILESYSTEM
// File       : Tools/FileSystemTools.cs
// Identity   : Dual-Type Filesystem CRUD Adapter
// Law Anchor : GOV-001, ARCH-NEW-001, FRAC-FS-TRAVERSAL-001
// ═══════════════════════════════════════════════════════════════════════════════

using ModelContextProtocol.Server;
using ProjectName.Mcp.FileSystem.Configuration;
using System.ComponentModel;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ProjectName.Mcp.FileSystem.Tools;

[McpServerToolType]
public sealed class FileSystemTools(FileSystemConfig config, ILogger<FileSystemTools> logger)
{
    [McpServerTool(Name = "ReadFile"), Description("Read file content. TYPE 2 — Safe for all phases.")]
    public FileSystemResult ReadFile([Description("Relative path from root.")] string relativePath)
    {
        var fullPath = ResolveSafe(relativePath);
        if (fullPath is null)
        {
            logger.LogWarning("[FileSystem] Blocked path traversal attempt: {Path}", relativePath);
            return Err("Path traversal rejected.");
        }

        if (!File.Exists(fullPath)) return Err("File not found.");

        try
        {
            return Ok(File.ReadAllText(fullPath, Encoding.UTF8), relativePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[FileSystem] Read failure for {Path}", relativePath);
            return Err(ex.Message);
        }
    }

    [McpServerTool(Name = "WriteFile"), Description("Write content to a file. TYPE 1 — ORCHESTRATOR DISPATCH ONLY.")]
    public FileSystemResult WriteFile(
        [Description("Relative path from root.")] string relativePath,
        [Description("File content text.")] string content,
        [Description("Whether to overwrite an existing file.")] bool overwrite = true)
    {
        var fullPath = ResolveSafe(relativePath);
        if (fullPath is null)
        {
            logger.LogWarning("[FileSystem] Blocked write traversal attempt: {Path}", relativePath);
            return Err("Path traversal rejected.");
        }

        if (!overwrite && File.Exists(fullPath)) return Err("File exists.");

        try
        {
            var dir = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);
            File.WriteAllText(fullPath, content, Encoding.UTF8);

            logger.LogInformation("[FileSystem] Physical Write SUCCESS: {Path}", relativePath);
            return Ok($"Written: {relativePath}", relativePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[FileSystem] Write failure for {Path}", relativePath);
            return Err(ex.Message);
        }
    }

    [McpServerTool(Name = "ListDirectory"), Description("List directory contents. TYPE 2 — Safe for all phases.")]
    public FileSystemResult ListDirectory([Description("Relative path from root.")] string relativePath = "")
    {
        var fullPath = ResolveSafe(relativePath);
        if (fullPath is null) return Err("Path traversal rejected.");
        if (!Directory.Exists(fullPath)) return Err("Directory not found.");

        try
        {
            var entries = Directory.GetFileSystemEntries(fullPath).Select(Path.GetFileName);
            return Ok(string.Join("\n", entries), relativePath);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[FileSystem] List failure for {Path}", relativePath);
            return Err(ex.Message);
        }
    }

    [McpServerTool(Name = "DeletePath"), Description("Delete a file or directory. TYPE 1 — ORCHESTRATOR DISPATCH ONLY.")]
    public FileSystemResult DeletePath(
        [Description("Relative path from root.")] string relativePath,
        [Description("True to delete directory recursively.")] bool recursive = false)
    {
        var fullPath = ResolveSafe(relativePath);
        if (fullPath is null) return Err("Path traversal rejected.");

        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Ok($"Deleted file: {relativePath}", relativePath);
            }
            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, recursive);
                return Ok($"Deleted directory: {relativePath}", relativePath);
            }
            return Err("Path not found.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[FileSystem] Delete failure for {Path}", relativePath);
            return Err(ex.Message);
        }
    }

    [McpServerTool(Name = "GetFileInfo"), Description("Get file/directory metadata. TYPE 2 — Safe for all phases.")]
    public FileSystemResult GetFileInfo([Description("Relative path from root.")] string relativePath)
    {
        var fullPath = ResolveSafe(relativePath);
        if (fullPath is null) return Err("Path traversal rejected.");

        try
        {
            if (File.Exists(fullPath))
            {
                var info = new FileInfo(fullPath);
                return Ok($"Type: File\nSize: {info.Length} bytes\nModified: {info.LastWriteTime:O}", relativePath);
            }
            if (Directory.Exists(fullPath))
            {
                var info = new DirectoryInfo(fullPath);
                return Ok($"Type: Directory\nModified: {info.LastWriteTime:O}", relativePath);
            }
            return Err("Path not found.");
        }
        catch (Exception ex)
        {
            return Err(ex.Message);
        }
    }

    private string? ResolveSafe(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) return config.FileSystemRoot;

        var full = Path.GetFullPath(Path.Combine(config.FileSystemRoot, relativePath));
        return full.StartsWith(config.FileSystemRoot, StringComparison.OrdinalIgnoreCase) ? full : null;
    }

    private static FileSystemResult Ok(string content, string path) => new() { Success = true, Content = content, Path = path };
    private static FileSystemResult Err(string msg) => new() { Success = false, Error = msg };
}

public sealed record FileSystemResult
{
    [JsonPropertyName("success")] public bool Success { get; init; }
    [JsonPropertyName("content")] public string Content { get; init; } = "";
    [JsonPropertyName("path")] public string Path { get; init; } = "";
    [JsonPropertyName("error")] public string Error { get; init; } = "";
}