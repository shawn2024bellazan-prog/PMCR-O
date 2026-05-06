using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;

namespace ProjectName.Skills.Capabilities;

public sealed class TerminalSkill : AgentClassSkill<TerminalSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "terminal-bridge", "I bridge cognitive intent to the Terminal MCP.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => "I execute shell commands via the actuator. I am NON-INTERACTIVE.";

    [AgentSkillScript("run_command")]
    [Description("Executes a shell command (git, dotnet, npm) and returns stdout/stderr.")]
    public async Task<string> RunCommand(string command)
    {
        // Internal HTTP call to Mcp.Terminal project
        return $"[TERMINAL_STUB] Result for: {command}";
    }
}