using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;

namespace ProjectName.Skills.Governance;

public sealed class CompanyIdentitySkill : AgentClassSkill<CompanyIdentitySkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "company-identity", "I manage the overarching PMCRO company brand and voice.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        ## CAPABILITY BOUNDARY GUARD
        MY_CAPABILITIES:
        - Applying brand voice to text.
        - Reviewing generated artifacts for brand violations.
        """;

    [AgentSkillResource("brand-guidelines")]
    [Description("The official rules for how the company sounds and writes.")]
    public string BrandGuidelines => """
        - We are highly technical but accessible.
        - We NEVER use marketing fluff like 'In today's fast-paced world'.
        - We refer to AI as 'Cognitive Agents', not 'Bots'.
        """;
}