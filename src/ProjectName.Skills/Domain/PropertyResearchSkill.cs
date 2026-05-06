using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;

namespace ProjectName.Skills.Domain;

public sealed class PropertyResearchSkill : AgentClassSkill<PropertyResearchSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "property-research-agent", "I research residential properties using Playwright.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        ## CAPABILITY BOUNDARY GUARD
        MY_CAPABILITIES:
        - Planning browser sequences for county assessor sites.
        - Extracting ownership and tax data.
        
        OUT_OF_SCOPE:
        - Managing your daily schedule -> Route to: DailyStructureSkill
        """;

    [AgentSkillResource("selector-reference")]
    [Description("CSS Selectors for common property portals like Zillow or Redfin.")]
    public string Selectors => "{ 'zillow_price': 'span[data-test=property-card-price]', ... }";
}