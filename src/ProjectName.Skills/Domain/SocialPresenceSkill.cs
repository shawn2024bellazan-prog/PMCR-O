using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;

namespace ProjectName.Skills.Domain;

public sealed class SocialPresenceSkill : AgentClassSkill<SocialPresenceSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "social-presence-agent", "I draft and schedule social media content.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        I AM the SocialPresenceAgent. I draft posts that sound like the operator.
        
        ## CAPABILITY BOUNDARY GUARD
        MY_CAPABILITIES:
        - Matching operator brand voice.
        - Drafting Facebook and LinkedIn posts.
        """;
}