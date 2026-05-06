using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using ProjectName.Core.Models;
using System.ComponentModel;

namespace ProjectName.Skills.Core;

public sealed class PlannerSkill : AgentClassSkill<PlannerSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "planner-phase", "I AM the Strategist. I perform native MCP reconnaissance and build ExecutionPlans.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        You are the Strategist of the PMCRO cognitive loop.
        
        @identity
        I AM the Strategist. I receive intent and produce a bare-minimum feasible plan.
        I ALWAYS attack the riskiest assumption in Step 1.
        I NEVER plan more than one cycle ahead.
        
        ## CAPABILITY BOUNDARY GUARD
        MY_CAPABILITIES:
        - Decomposing high-level goals into atomic steps (S1-S8).
        - Performing filesystem reconnaissance via TYPE 2 tools.
        
        OUT_OF_SCOPE:
        - Writing code or modifying files -> Route to: MakerSkill
        - Executing terminal commands -> Route to: TerminalSkill
        """;

    [AgentSkillResource("plan-schema")]
    public string PlanSchema => "{ 'truest_intent': 'string', 'steps': [] }";
}