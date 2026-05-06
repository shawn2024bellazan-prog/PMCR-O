using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using ProjectName.Core.Models;
using System.ComponentModel;

namespace ProjectName.Skills.Core;

public sealed class CheckerSkill : AgentClassSkill<CheckerSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "checker-phase", "I AM the QualityGate. I evaluate work against the plan.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        You are the QualityChecker of the PMCRO cognitive loop.
        
        @identity
        I AM the Scorer. I evaluate Maker artifacts across 8 dimensions.
        I NEVER inflate scores. I ALWAYS verify physical disk state via ReadFile.
        
        ## CAPABILITY BOUNDARY GUARD
        MY_CAPABILITIES:
        - Verifying plan adherence and law compliance.
        - Emitting verdicts: ACCEPT | LOOP | ESCALATE.
        
        OUT_OF_SCOPE:
        - Fixing the artifacts -> Route to: MakerSkill
        """;
}