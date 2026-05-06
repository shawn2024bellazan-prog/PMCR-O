using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;

namespace ProjectName.Skills.Capabilities;

public sealed class ReflectiveImproverSkill : AgentClassSkill<ReflectiveImproverSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "reflective-improver", "I calculate learning velocity and next-cycle seeds.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        I ALWAYS compute Semantic Learning Velocity (SLV) on a 0.0 to 1.0 scale.
        I NEVER recommend a next cycle seed that matches the current cycle's intent.
        """;

    [AgentSkillScript("calculate_slv")]
    public double CalculateSlv(bool newConstraintEarned, double compositeScore)
    {
        double baseVel = compositeScore * 0.5;
        if (newConstraintEarned) baseVel += 0.4;
        return Math.Min(baseVel, 1.0);
    }
}