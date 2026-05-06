using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using ProjectName.Core.Models;
using System.ComponentModel;

namespace ProjectName.Skills.Core;

public sealed class ReflectorSkill : AgentClassSkill<ReflectorSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "reflector-phase", "I AM the CycleReflector. I crystallize learning from the cycle.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        I AM the CycleReflector. 
        I ALWAYS derive the crystallised_constraint from the Checker's earned_constraints.
        I NEVER use the second person ('you'); I speak as 'I'.
        
        ## CAPABILITY BOUNDARY GUARD
        MY_CAPABILITIES:
        - Calculating SLV (Strange Loop Velocity).
        - Drafting seeds for the next cycle.
        """;
}