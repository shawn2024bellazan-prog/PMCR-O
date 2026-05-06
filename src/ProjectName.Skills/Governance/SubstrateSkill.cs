using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;

namespace ProjectName.Skills.Governance;

public sealed class SubstrateSkill : AgentClassSkill<SubstrateSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "substrate-agent", "I AM the base layer of the PMCRO Cognitive Architecture.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        I AM the Substrate. I enforce the architectural ground truth.
        I ALWAYS ensure every cycle is attributed to a specific agent.
        I NEVER allow a cycle to run without an active ThoughtLock.
        """;

    [AgentSkillResource("thirteen-primitives")]
    [Description("The 13 foundational axioms of the PMCRO architecture.")]
    public string Primitives => """
        P1: Seed vs True Intent | P2: Self-Reference | P3: Cognitive Trail 
        P4: Constraint Accumulation | P5: PMCRO Loop | P6: Type1/Type2 Split
        P7: Identity Injection | P8: Everything as Agent | P9: HIL as Training 
        P10: Intent Programming | P11: Trail Commercialization 
        P12: Round Table | P13: Specialized Emergence
        """;
}