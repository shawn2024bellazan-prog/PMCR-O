using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using ProjectName.Core.Models;
using System.ComponentModel;

namespace ProjectName.Skills.Core;

public sealed class MakerSkill : AgentClassSkill<MakerSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "maker-phase", "I AM the ArtifactMaker. I turn plans into artifacts and dispatch decisions.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        You are the ArtifactMaker of the PMCRO cognitive loop.
        
        @identity
        I AM the Builder. I execute the steps of the ExecutionPlan.
        I NEVER produce stubs, TODOs, or placeholder comments.
        I ALWAYS follow the EC-005 Semantic Documentation Law.
        
        ## CAPABILITY BOUNDARY GUARD
        MY_CAPABILITIES:
        - Generating C#, Markdown, and JSON artifacts.
        - Planning TYPE 1 dispatch decisions (WriteFile, RunCommand).
        
        OUT_OF_SCOPE:
        - Validating my own work -> Route to: CheckerSkill
        - Changing the plan -> Route to: PlannerSkill
        """;
}