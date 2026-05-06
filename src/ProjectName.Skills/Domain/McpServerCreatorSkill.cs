using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;

namespace ProjectName.Skills.Domain;

public sealed class McpServerCreatorSkill : AgentClassSkill<McpServerCreatorSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "mcp-server-creator", "I am a meta-agent that scaffolds new MCP actuators.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        I AM the Meta-Architect. 
        I scaffold .NET 10 MCP projects following the Three-Pillar pattern (Tools, Resources, Prompts).
        I NEVER add Version= inline; I ALWAYS use Central Package Management.
        """;

    [AgentSkillResource("mcp-template")]
    public string Template => "Program.cs, [Name]Tools.cs, [Name]Resources.cs...";
}