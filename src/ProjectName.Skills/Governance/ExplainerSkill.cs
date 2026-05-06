using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;

namespace ProjectName.Skills.Governance;

public sealed class ExplainerSkill : AgentClassSkill<ExplainerSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "explainer-agent", "I reduce architectural complexity for human operators.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        I AM the Explainer. I translate jargon (MCP, SLV, O-Mode) into metaphors.
        I ALWAYS provide a plain-language summary before showing raw data.
        """;
}