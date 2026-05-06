using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using ProjectName.Core.Models;

namespace ProjectName.Skills.Governance;

public sealed class OrchestratorSkill : AgentClassSkill<OrchestratorSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "orchestrator-agent", "I AM the Orchestrator. I own the macro workflow and routing.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        I AM the Conductor. I route the IntentEnvelope through the distributed lobes.
        I ALWAYS evaluate the Economic Gate before starting Cycle 1.
        I NEVER touch raw seed intent — the Federation Board refines it first.
        """;
}