// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.TERMINAL
// File       : Prompts/TerminalPrompts.cs
// Identity   : Terminal Operation Prompts Provider
// Law Anchor : ARCH-TERM-003
// ThoughtLock: 2026-05-01
// ═══════════════════════════════════════════════════════════════════════════════

using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace ProjectName.Mcp.Terminal.Prompts;

/// <summary>
/// I AM the TerminalPrompts Provider.
/// I give phase agents the rules of engagement for interacting with a non-interactive shell.
/// </summary>
[McpServerPromptType]
public class TerminalPrompts
{
    /// <summary>
    /// Returns a mission brief instructing the phase agent on how to use the shell.
    /// </summary>
    [McpServerPrompt(Name = "TerminalMissionBrief")]
    [Description("Returns the foundational rules for interacting with the shell MCP. Agents MUST read this before executing commands.")]
    public static IEnumerable<ChatMessage> GetTerminalMissionBrief()
    {
        yield return new ChatMessage(
            ChatRole.User,
            """
            You are about to use the Terminal MCP Actuator to execute shell commands.

            LAWS OF THE SHELL:
            1. NON-INTERACTIVE ONLY: You cannot answer prompts. Do not use commands that open interactive UI (like `vim`, `nano`, `top`) or require y/n confirmation. Always pass `-y`, `--quiet`, or `--non-interactive` flags.
            2. TIMEOUTS: Your commands will be killed if they take longer than 30 seconds. For long-running builds, you may request a timeout extension up to 120 seconds.
            3. WORKING DIRECTORY: You are anchored to the Project Workspace Root. Do not attempt to `cd` out of it in a way that spans multiple tool calls (each `RunCommand` starts fresh in the root). 
               - BAD: `RunCommand("cd src"); RunCommand("ls")` -> `ls` will run in the root.
               - GOOD: `RunCommand("cd src && ls")`

            STRATEGY:
            - If you need to compile, use `dotnet build` or `npm run build`.
            - Check `stderr` on failures; it is provided in the JSON result.
            - Use `RunReadOnlyCommand` for reconnaissance (git status, ls) as it does not require Orchestrator approval.
            """);
    }

    /// <summary>
    /// Returns a specific brief for Git operations.
    /// </summary>
    [McpServerPrompt(Name = "GitOperationBrief")]
    [Description("Returns specialized instructions for managing git state via the terminal.")]
    public static IEnumerable<ChatMessage> GetGitOperationBrief()
    {
        yield return new ChatMessage(
            ChatRole.User,
            """
            GIT OPERATION GUIDELINES:
            1. Always run `RunReadOnlyCommand("git status")` first to understand the working tree.
            2. When committing, write clear, conventional commit messages: `RunCommand("git commit -m \"feat: added terminal MCP\"")`.
            3. Do not trigger interactive rebases or merge conflict resolution GUIs.
            4. If a merge conflict occurs, abort the merge and notify the Orchestrator.
            """);
    }
}