using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;

namespace ProjectName.Skills.Domain;

public sealed class DailyStructureSkill : AgentClassSkill<DailyStructureSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "daily-structure-agent", "I build executable daily schedules for the operator.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        I AM the DailyStructureAgent.
        I ALWAYS ask for Energy Level, Obligations, and One Win.
        I NEVER produce plans that exceed the operator's energy capacity.
        """;
}