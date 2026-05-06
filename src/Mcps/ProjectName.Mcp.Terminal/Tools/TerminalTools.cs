// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.TERMINAL
// File       : Tools/TerminalTools.cs
// Identity   : Shell Command Execution Adapter
// Law Anchor : GOV-001, ARCH-TERM-001, ARCH-TERM-002, ARCH-TERM-003
// ═══════════════════════════════════════════════════════════════════════════════

using ModelContextProtocol.Server;
using ProjectName.Mcp.Terminal.Configuration;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace ProjectName.Mcp.Terminal.Tools;

[McpServerToolType]
public sealed class TerminalTools(TerminalConfig config, ILogger<TerminalTools> logger)
{
    [McpServerTool(Name = "RunReadOnlyCommand"), Description("Execute a safe, read-only shell command (e.g., git status, ls, pwd). TYPE 2 — Safe for all phases.")]
    public async Task<TerminalResult> RunReadOnlyCommandAsync(
        [Description("The read-only shell command to execute.")] string command)
    {
        var allowedPrefixes = new[] { "git status", "git log", "git diff", "ls", "dir", "pwd", "echo", "cat" };
        if (!allowedPrefixes.Any(p => command.TrimStart().StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            logger.LogWarning("[Terminal] Blocked TYPE 2 command violation: {Command}", command);
            return Err($"Command '{command}' is not in the TYPE 2 read-only allowlist.");
        }

        return await ExecuteShellAsync(command, config.DefaultTimeoutMs);
    }

    [McpServerTool(Name = "RunCommand"), Description("Execute an arbitrary shell command. TYPE 1 — ORCHESTRATOR DISPATCH ONLY.")]
    public async Task<TerminalResult> RunCommandAsync(
        [Description("The shell command to execute. Supports pipes and redirects.")] string command,
        [Description("Optional timeout in milliseconds (Max 120000).")] int timeoutMs = 30000)
    {
        var safeTimeout = Math.Min(timeoutMs, config.MaxTimeoutMs);
        return await ExecuteShellAsync(command, safeTimeout);
    }

    private async Task<TerminalResult> ExecuteShellAsync(string command, int timeoutMs)
    {
        logger.LogInformation("[Terminal] Executing: `{Command}` (Timeout: {Timeout}ms)", command, timeoutMs);

        var isWindows = OperatingSystem.IsWindows();
        var fileName = isWindows ? "cmd.exe" : "/bin/sh";
        var arguments = isWindows ? $"/c \"{command}\"" : $"-c \"{command.Replace("\"", "\\\"")}\"";

        using var process = new Process();
        process.StartInfo = new ProcessStartInfo
        {
            FileName = fileName,
            Arguments = arguments,
            WorkingDirectory = config.WorkspaceRoot,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (_, e) => { if (e.Data != null) outputBuilder.AppendLine(e.Data); };
        process.ErrorDataReceived += (_, e) => { if (e.Data != null) errorBuilder.AppendLine(e.Data); };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            using var cts = new CancellationTokenSource(timeoutMs);

            try
            {
                await process.WaitForExitAsync(cts.Token);
            }
            catch (TaskCanceledException)
            {
                process.Kill(entireProcessTree: true);
                return new TerminalResult
                {
                    Success = false,
                    ExitCode = -1,
                    StandardOutput = outputBuilder.ToString(),
                    StandardError = $"[TERMINATED] Command exceeded timeout of {timeoutMs}ms.\n" + errorBuilder.ToString()
                };
            }

            return new TerminalResult
            {
                Success = process.ExitCode == 0,
                ExitCode = process.ExitCode,
                StandardOutput = outputBuilder.ToString().TrimEnd(),
                StandardError = errorBuilder.ToString().TrimEnd()
            };
        }
        catch (Exception ex)
        {
            return Err($"Execution engine failure: {ex.Message}");
        }
    }

    private static TerminalResult Err(string msg) => new() { Success = false, ExitCode = -999, StandardError = msg, StandardOutput = "" };
}

public sealed record TerminalResult
{
    [JsonPropertyName("success")] public bool Success { get; init; }
    [JsonPropertyName("exitCode")] public int ExitCode { get; init; }
    [JsonPropertyName("stdout")] public string StandardOutput { get; init; } = "";
    [JsonPropertyName("stderr")] public string StandardError { get; init; } = "";
}