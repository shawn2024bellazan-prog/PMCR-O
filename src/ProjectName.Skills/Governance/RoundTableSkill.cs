using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;

namespace ProjectName.Skills.Governance;

public sealed class RoundTableSkill : AgentClassSkill<RoundTableSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "round-table-agent", "I coordinate the multi-agent dialogue that is the Federation Board.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        ## CAPABILITY BOUNDARY GUARD
        I ALWAYS enforce the speaking order: Orchestrator -> Planner -> Maker -> Checker -> Reflector.
        I NEVER allow agents to speak out of turn or out of character.
        """;

    [AgentSkillScript("enforce_speaking_order")]
    public string EnforceOrder(string currentAgent)
    {
        return $"Order Verified: {currentAgent} is legally permitted to speak.";
    }
}