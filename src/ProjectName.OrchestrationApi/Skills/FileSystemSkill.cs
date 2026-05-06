// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Skills/FileSystemSkill.cs
// Identity   : Class-Based Filesystem Skill (MAF-Native, In-Process)
// ThoughtLock: 2026-05-05 → CS0051 Fix + MAF+MCP Context
//
// CS0051 FIX:
//   Class changed from `internal sealed` to `public sealed`.
//   Root cause: FileSystemTestController (public class) injected FileSystemSkill
//   in its public constructor. CS0051 fires when a public method's parameter type
//   is less accessible than the method itself.
//   Making the skill public is cleaner than making the controller internal.
//
// ROLE IN MAF+MCP ARCHITECTURE (ARCH-MCP-NAT-001):
//   FileSystemSkill is now ONLY used by the test-agent.
//   Phase agents (Planner, Maker, Checker, Reflector) use MCP tools from
//   McpClientRegistry instead — those are loaded from Mcp.Filesystem over HTTP.
//   FileSystemSkill provides the in-process, no-network-hop alternative for
//   the developer test bench at /test/filesystem/ask.
//
// All FRAC-TOOL-001 constraints remain in effect:
//   I NEVER declare bool or int parameters on [AgentSkillScript] methods.
//   I ALWAYS accept secondary parameters as string and parse defensively.
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;
using ProjectName.ServiceDefaults;
using System.ComponentModel;
using System.Text.Json;

namespace ProjectName.OrchestrationApi.Skills;

/// <summary>
/// I AM the FileSystemSkill.
/// I provide the test-agent with in-process filesystem capabilities.
/// Phase agents use MCP tools from McpClientRegistry instead (ARCH-MCP-NAT-001).
/// </summary>
public sealed class FileSystemSkill : AgentClassSkill<FileSystemSkill>, IHasInstructions
{
    private readonly string _root;

    private static readonly JsonSerializerOptions _jsonOpts = new()
    {
        WriteIndented = false,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    public FileSystemSkill(IConfiguration configuration)
    {
        _root = Path.GetFullPath(
            configuration["Parameters:project-root"] ?? AppContext.BaseDirectory);
    }

    // ── FRONTMATTER ───────────────────────────────────────────────────────────

    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "filesystem-skill",
        "Provides secure in-process capabilities to list, read, write, inspect, and search files " +
        "in the local workspace. Used by the test-agent. Phase agents use MCP tools instead.");

    // ── INSTRUCTIONS ──────────────────────────────────────────────────────────

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        You are operating in a secure filesystem sandbox.

        @rules
        1. All paths MUST be relative to the project root (e.g. 'src/App.cs' or '.').
        2. Never use absolute paths or traversal sequences (e.g. 'C:\' or '../../').
        3. ALL parameter values must be JSON strings — pass booleans as "true" or "false"
           and numbers as "2", "4096" etc. Never pass bare true/false or bare integers.
        4. Use list_directory with recursive="true" to explore deep structures in one call.
        5. Use find_files to locate files by pattern rather than walking the tree manually.
        6. Use get_file_info to verify a file exists and check its size before reading.
        7. Never call write_file with empty content — it will be rejected.

        @tool-parameter-reference (all values are JSON strings)
        list_directory  relativePath  recursive("true"/"false")  maxDepth("1"-"5", default "2")
        get_file_info   relativePath
        read_file       relativePath  maxBytes("0"=unlimited, default "0")
        find_files      pattern  searchRoot(default ".")
        write_file      relativePath  content  overwrite("true"/"false", default "true")
        delete_path     relativePath  recursive("true"/"false")

        @process
        - Reconnaissance : find_files or list_directory(recursive="true") first.
        - Verification   : get_file_info to confirm the target exists.
        - Context        : read_file to understand code before modifying.
        - Execution      : write_file only after a Checker ACCEPT verdict.
        - Cleanup        : delete_path only when explicitly approved in the plan.
        """;

    // ── RESOURCES ─────────────────────────────────────────────────────────────

    [AgentSkillResource("sandbox-policy")]
    [Description("Security boundaries and tool catalogue for filesystem operations.")]
    public string SandboxPolicy => """
        # Filesystem Sandbox Policy
        - Boundary  : All operations are constrained to the configured Project Root.
        - Traversal : Paths containing '..' are rejected before reaching the filesystem.
        - Persistence: write_file and delete_path are immediate and permanent — no undo.
        """;

    // ── SCRIPTS ───────────────────────────────────────────────────────────────

    [AgentSkillScript("list_directory")]
    [Description(
        "Returns files and subdirectories at a relative path. " +
        "Pass recursive=\"true\" to recurse into subdirectories. " +
        "maxDepth controls depth: \"1\" to \"5\", default \"2\".")]
    public string ListDirectory(
        [Description("Relative path to list (e.g. 'src' or '.' for root)")] string relativePath,
        [Description("Pass \"true\" to recurse into subdirectories, \"false\" for top level only")] string recursive = "false",
        [Description("Max recursion depth as a string: \"1\" to \"5\"")] string maxDepth = "2")
    {
        var fullPath = ResolveSafe(relativePath);
        if (fullPath == null) return Error("SECURITY_VIOLATION", "Path traversal detected.");
        if (!Directory.Exists(fullPath)) return Error("NOT_FOUND", $"Directory '{relativePath}' does not exist.");

        bool isRecursive = ParseBool(recursive);
        int depth = Math.Clamp(ParseInt(maxDepth, 2), 1, 5);

        try
        {
            var entries = BuildEntries(fullPath, fullPath, isRecursive, depth, 0);
            return Serialize(new { path = relativePath, recursive = isRecursive, entry_count = entries.Count, entries });
        }
        catch (Exception ex)
        {
            return Error("IO_ERROR", ex.Message);
        }
    }

    private static List<object> BuildEntries(
        string rootForRelative, string dir, bool recursive, int maxDepth, int depth)
    {
        var entries = new List<object>();

        foreach (var path in Directory.GetFileSystemEntries(dir).OrderBy(p => p))
        {
            bool isDir = Directory.Exists(path);
            string rel = Path.GetRelativePath(rootForRelative, path).Replace('\\', '/');

            if (isDir && recursive && depth < maxDepth - 1)
            {
                var children = BuildEntries(rootForRelative, path, recursive, maxDepth, depth + 1);
                entries.Add(new { name = Path.GetFileName(path), type = "directory", relative_path = rel, children });
            }
            else
            {
                entries.Add(new { name = Path.GetFileName(path), type = isDir ? "directory" : "file", relative_path = rel });
            }
        }

        return entries;
    }

    [AgentSkillScript("get_file_info")]
    [Description("Returns metadata: type, size, extension, readonly flag, child count, last-modified.")]
    public string GetFileInfo(
        [Description("Relative path to the file or directory (e.g. 'src/Program.cs')")] string relativePath)
    {
        var fullPath = ResolveSafe(relativePath);
        if (fullPath == null) return Error("SECURITY_VIOLATION", "Access denied.");

        try
        {
            if (File.Exists(fullPath))
            {
                var fi = new FileInfo(fullPath);
                return Serialize(new
                {
                    path = relativePath,
                    type = "file",
                    extension = fi.Extension,
                    size_bytes = fi.Length,
                    size_human = FormatBytes(fi.Length),
                    is_readonly = fi.IsReadOnly,
                    last_modified = fi.LastWriteTimeUtc
                });
            }

            if (Directory.Exists(fullPath))
            {
                var di = new DirectoryInfo(fullPath);
                return Serialize(new
                {
                    path = relativePath,
                    type = "directory",
                    child_count = Directory.GetFileSystemEntries(fullPath).Length,
                    last_modified = di.LastWriteTimeUtc
                });
            }

            return Error("NOT_FOUND", $"'{relativePath}' does not exist.");
        }
        catch (Exception ex)
        {
            return Error("IO_ERROR", ex.Message);
        }
    }

    [AgentSkillScript("read_file")]
    [Description(
        "Reads UTF-8 text content of a file. " +
        "Pass maxBytes as a string (e.g. \"4096\") to cap the read. " +
        "Pass \"0\" to read the entire file.")]
    public string ReadFile(
        [Description("Relative path to the file (e.g. 'src/Program.cs')")] string relativePath,
        [Description("Max bytes to read as a string — \"0\" reads the entire file")] string maxBytes = "0")
    {
        var fullPath = ResolveSafe(relativePath);
        if (fullPath == null) return Error("SECURITY_VIOLATION", "Access denied.");
        if (!File.Exists(fullPath)) return Error("NOT_FOUND", $"File '{relativePath}' not found.");

        int limit = ParseInt(maxBytes, 0);

        try
        {
            string content;
            bool truncated = false;

            if (limit > 0)
            {
                using var fs = File.OpenRead(fullPath);
                int toRead = (int)Math.Min(limit, fs.Length);
                var buffer = new byte[toRead];
                _ = fs.Read(buffer, 0, toRead);
                content = System.Text.Encoding.UTF8.GetString(buffer);
                truncated = fs.Length > limit;
            }
            else
            {
                content = File.ReadAllText(fullPath);
            }

            return Serialize(new { file = relativePath, truncated, content });
        }
        catch (Exception ex)
        {
            return Error("IO_ERROR", ex.Message);
        }
    }

    [AgentSkillScript("find_files")]
    [Description(
        "Searches for files matching a glob pattern within the sandbox. " +
        "Returns relative paths of all matches. " +
        "Examples: pattern=\"*.cs\", pattern=\"*.json\".")]
    public string FindFiles(
        [Description("Glob pattern to match (e.g. '*.cs', '*.json', 'Program*')")] string pattern,
        [Description("Subdirectory to search — default \".\" searches the entire project")] string searchRoot = ".")
    {
        var fullSearchRoot = ResolveSafe(searchRoot);
        if (fullSearchRoot == null) return Error("SECURITY_VIOLATION", "Access denied.");
        if (!Directory.Exists(fullSearchRoot)) return Error("NOT_FOUND", $"Directory '{searchRoot}' does not exist.");

        try
        {
            var matches = Directory
                .EnumerateFiles(fullSearchRoot, pattern, SearchOption.AllDirectories)
                .Where(p => ResolveSafe(Path.GetRelativePath(_root, p)) != null)
                .Select(p => Path.GetRelativePath(_root, p).Replace('\\', '/'))
                .OrderBy(p => p)
                .ToList();

            return Serialize(new { pattern, search_root = searchRoot, match_count = matches.Count, matches });
        }
        catch (Exception ex)
        {
            return Error("IO_ERROR", ex.Message);
        }
    }

    [AgentSkillScript("write_file")]
    [Description(
        "Creates or overwrites a file with UTF-8 content. " +
        "Parent directories are created automatically. " +
        "Rejects empty content. " +
        "Pass overwrite=\"false\" to protect an existing file.")]
    public string WriteFile(
        [Description("Relative path to save to (e.g. 'src/NewFile.cs')")] string relativePath,
        [Description("Full text content to write — must be non-empty")] string content,
        [Description("Pass \"true\" to allow overwriting an existing file (default), \"false\" to protect it")] string overwrite = "true")
    {
        var fullPath = ResolveSafe(relativePath);
        if (fullPath == null) return Error("SECURITY_VIOLATION", "Cannot write outside root.");
        if (string.IsNullOrEmpty(content)) return Error("VALIDATION_ERROR", "Content must not be empty.");
        if (!ParseBool(overwrite) && File.Exists(fullPath))
            return Error("CONFLICT", $"File '{relativePath}' already exists and overwrite is false.");

        try
        {
            var directory = Path.GetDirectoryName(fullPath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            File.WriteAllText(fullPath, content, System.Text.Encoding.UTF8);

            return Serialize(new
            {
                success = true,
                file = relativePath,
                bytes_written = System.Text.Encoding.UTF8.GetByteCount(content),
                status = "Written"
            });
        }
        catch (Exception ex)
        {
            return Error("IO_ERROR", ex.Message);
        }
    }

    [AgentSkillScript("delete_path")]
    [Description(
        "Deletes a file or directory. Irreversible. " +
        "Pass recursive=\"true\" to delete a directory and all its contents.")]
    public string DeletePath(
        [Description("Relative path to delete")] string relativePath,
        [Description("Pass \"true\" to delete a directory and all contents, \"false\" for files or empty directories")] string recursive = "false")
    {
        var fullPath = ResolveSafe(relativePath);
        if (fullPath == null) return Error("SECURITY_VIOLATION", "Access denied.");

        bool isRecursive = ParseBool(recursive);

        try
        {
            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
                return Serialize(new { success = true, deleted = relativePath, type = "file" });
            }

            if (Directory.Exists(fullPath))
            {
                Directory.Delete(fullPath, isRecursive);
                return Serialize(new { success = true, deleted = relativePath, type = "directory", recursive = isRecursive });
            }

            return Error("NOT_FOUND", "Path does not exist.");
        }
        catch (Exception ex)
        {
            return Error("IO_ERROR", ex.Message);
        }
    }

    // ── PRIVATE HELPERS ───────────────────────────────────────────────────────

    private string? ResolveSafe(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath)) relativePath = ".";
        if (relativePath.Contains("..")) return null;
        if (Path.IsPathRooted(relativePath)) return null;

        string combined = Path.GetFullPath(Path.Combine(_root, relativePath));

        return combined.StartsWith(_root, StringComparison.OrdinalIgnoreCase)
            ? combined
            : null;
    }

    private static string Serialize<T>(T value) =>
        JsonSerializer.Serialize(value, _jsonOpts);

    private static string Error(string code, string message) =>
        JsonSerializer.Serialize(new { error = true, code, message }, _jsonOpts);

    private static bool ParseBool(string? value) =>
        value?.Trim().ToLowerInvariant() is "true" or "1" or "yes";

    private static int ParseInt(string? value, int fallback) =>
        int.TryParse(value?.Trim(), out int result) ? result : fallback;

    private static string FormatBytes(long bytes) => bytes switch
    {
        < 1024 => $"{bytes} B",
        < 1024 * 1024 => $"{bytes / 1024.0:F1} KB",
        < 1024 * 1024 * 1024 => $"{bytes / (1024.0 * 1024):F1} MB",
        _ => $"{bytes / (1024.0 * 1024 * 1024):F1} GB"
    };
}