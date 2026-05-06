// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.TERMINAL
// File       : Configuration/TerminalConfig.cs
// Identity   : Terminal Execution Boundary Configuration
// Law Anchor : GOV-001, ARCH-TERM-001, ARCH-TERM-002
// ThoughtLock: 2026-05-01
// ═══════════════════════════════════════════════════════════════════════════════

namespace ProjectName.Mcp.Terminal.Configuration;

/// <summary>
/// I AM the TerminalConfig.
/// I define the strict boundaries for all shell execution within this actuator.
/// </summary>
/// <remarks>
/// <para><b>ARCH-TERM-001 (Timeouts):</b> Every process MUST have a maximum execution time. Agents cannot create infinite loops.</para>
/// <para><b>ARCH-TERM-002 (Working Directory):</b> All processes are forced to start within <see cref="WorkspaceRoot"/>.</para>
/// </remarks>
public sealed class TerminalConfig
{
    /// <summary>
    /// The absolute path that serves as the forced working directory for all shell commands.
    /// </summary>
    public required string WorkspaceRoot { get; init; }

    /// <summary>
    /// The default time (in milliseconds) before a shell command is forcefully killed.
    /// </summary>
    public int DefaultTimeoutMs { get; init; } = 30_000;

    /// <summary>
    /// The absolute maximum time (in milliseconds) allowed for any command, overriding agent requests.
    /// </summary>
    public int MaxTimeoutMs { get; init; } = 120_000;
}