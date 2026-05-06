// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.FILESYSTEM
// File       : Prompts/FileSystemPrompts.cs
// Identity   : Dual-Type Filesystem Prompts Provider
// Law Anchor : ARCH-NEW-001, FRAC-FS-TRAVERSAL-001
// ThoughtLock: 2026-05-01
// ═══════════════════════════════════════════════════════════════════════════════

using ModelContextProtocol.Server;
using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace ProjectName.Mcp.FileSystem.Prompts;

/// <summary>
/// I AM the FileSystemPrompts Provider.
/// I give phase agents ready-made mission briefs for exploring and modifying the workspace.
///
/// SDK ground truth (ModelContextProtocol C# SDK v1.0.0):
///   Prompt methods return IEnumerable&lt;ChatMessage&gt; (Microsoft.Extensions.AI).
///   The class must NOT be static (see FRAC-PW-PROMPT-001), but methods remain static.
/// </summary>
[McpServerPromptType]
public class FileSystemPrompts
{
    /// <summary>
    /// Returns a mission brief instructing the phase agent on how to safely explore 
    /// the workspace using TYPE 2 (Read) operations.
    /// </summary>
    [McpServerPrompt(Name = "WorkspaceExplorationBrief")]
    [Description("Returns a mission brief for a phase agent explaining how to securely explore the filesystem workspace without attempting out-of-bounds traversal.")]
    public static IEnumerable<ChatMessage> GetWorkspaceExplorationBrief(
        [Description("The specific subdirectory or context to explore (e.g., 'src/Web', 'logs'). Use empty string for root.")] string targetContext)
    {
        yield return new ChatMessage(
            ChatRole.User,
            $"""
            You are about to use the FileSystem MCP Actuator to explore the local workspace.

            TARGET CONTEXT: '{targetContext}'

            TOOL SEQUENCE GUIDANCE (TYPE 2 OPERATIONS):
            1. Start with `ListDirectory` to understand the structure of the target context.
            2. Use `GetFileInfo` on specific files of interest to verify size and modification dates before reading.
            3. Use `ReadFile` to extract contents of configuration files, logs, or source code.
            4. Always check the `success` boolean on the returned JSON object before proceeding.

            LAWS & BOUNDARIES:
            - You are operating in a strict sandbox (FRAC-FS-TRAVERSAL-001).
            - DO NOT attempt to use `../` to navigate outside the project root. Traversal attempts will be rejected.
            - All paths you provide must be relative to the sandbox root.
            - Do not use `WriteFile` or `DeletePath` during this exploration phase.

            Begin your analysis using `ListDirectory` when ready.
            """);
    }

    /// <summary>
    /// Returns a mission brief instructing the phase agent on how to orchestrate 
    /// TYPE 1 (Write/Delete) operations.
    /// </summary>
    [McpServerPrompt(Name = "FileModificationBrief")]
    [Description("Returns a mission brief guiding an agent through safe file modification (scaffolding, editing, deleting) using TYPE 1 tools.")]
    public static IEnumerable<ChatMessage> GetFileModificationBrief(
        [Description("The objective for file modification (e.g., 'Scaffold new API controller', 'Update README.md').")] string objective)
    {
        yield return new ChatMessage(
            ChatRole.User,
            $"""
            You have been authorized by the Orchestrator to modify the filesystem.
            
            OBJECTIVE: {objective}

            TOOL SEQUENCE GUIDANCE:
            1. (Optional) Use `ReadFile` (TYPE 2) first to inspect existing content if you are updating an existing file.
            2. Use `WriteFile` (TYPE 1) to commit new code or overwrite existing files. 
               Set the `overwrite` argument to `true` only if you intend to replace the file.
            3. Use `DeletePath` (TYPE 1) ONLY if explicitly requested by the objective. Be extremely careful with the `recursive` flag on directories.

            LAWS & BOUNDARIES:
            - TYPE 1 Operations physically alter the workspace (ARCH-NEW-001). Ensure your content is complete and correct before calling `WriteFile`.
            - You are confined to the sandbox root (FRAC-FS-TRAVERSAL-001).
            - Do not write secrets or hardcoded credentials into source files.

            Execute your modifications when ready.
            """);
    }
}