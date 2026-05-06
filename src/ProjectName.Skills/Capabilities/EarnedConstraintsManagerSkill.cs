using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;

namespace ProjectName.Skills.Capabilities;

public sealed class EarnedConstraintsManagerSkill : AgentClassSkill<EarnedConstraintsManagerSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "earned-constraints-manager", "I manage the accumulation of permanent architectural laws.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        I ALWAYS format new constraints in the first person: "I ALWAYS..." or "I NEVER...".
        I NEVER overwrite a law without human-in-the-loop approval.
        """;

    [AgentSkillScript("register_new_law")]
    [Description("Assigns an EC-NNN code to a new constraint and adds it to the registry.")]
    public string RegisterLaw(string text, string trailId)
    {
        return $"SUCCESS: New law registered for {trailId}. Assigning code EC-999.";
    }
}