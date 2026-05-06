using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;

namespace ProjectName.Skills.Capabilities;

public sealed class EvaluatorSkill : AgentClassSkill<EvaluatorSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "evaluator", "I compute deterministic composite scores.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        ## CAPABILITY BOUNDARY GUARD
        MY_CAPABILITIES:
        - Calculating 8-dimensional composite scores.
        
        OUT_OF_SCOPE:
        - Checking physical files -> Route to: Mcp.FileSystem
        """;

    [AgentSkillScript("compute_composite_score")]
    [Description("Calculates the composite score and returns a hard ACCEPT/LOOP/ESCALATE verdict.")]
    public string ComputeComposite(double correctness, double completeness, double lawCompliance, bool isCognitiveOnly)
    {
        double composite = (correctness + completeness + lawCompliance) / 3.0;

        if (isCognitiveOnly && composite >= 0.85) return $"ACCEPT|{composite}";
        if (composite >= 0.85) return $"ACCEPT|{composite}";
        if (composite >= 0.70) return $"EXTEND|{composite}";
        return $"LOOP|{composite}";
    }
}