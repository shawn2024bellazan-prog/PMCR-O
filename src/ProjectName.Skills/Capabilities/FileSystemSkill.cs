using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;

namespace ProjectName.Skills.Capabilities;

public sealed class FilesystemSkill : AgentClassSkill<FilesystemSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "filesystem-bridge", "I bridge cognitive intent to the physical FileSystem MCP.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => "I provide scripts to read, list, and write files via the actuator.";

    [AgentSkillScript("read_file")]
    public async Task<string> ReadFile(string path)
    {
        // This will eventually call your ProjectName.Mcp.FileSystem endpoint
        return $"[ACTUATOR_OUTPUT] Content of {path}";
    }
}