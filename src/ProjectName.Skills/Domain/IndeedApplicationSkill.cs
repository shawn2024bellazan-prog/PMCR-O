using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;

namespace ProjectName.Skills.Domain;

public sealed class IndeedApplicationSkill : AgentClassSkill<IndeedApplicationSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "indeed-application-agent", "Expert at submitting job applications on Indeed.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        ## CAPABILITY BOUNDARY GUARD
        MY_CAPABILITIES:
        - Evaluating job descriptions against operator criteria.
        - Formatting ApplicationFrames.
        
        OUT_OF_SCOPE (ROUTE TO):
        - Web Browsing -> Route to: Playwright MCP
        - Saving files -> Route to: FileSystem MCP

        EXECUTION RULE: ALWAYS call 'read_job_criteria' resource before evaluating a listing.
        """;

    // PROGRESSIVE DISCLOSURE: Loads rules only when needed
    [AgentSkillResource("read_job_criteria")]
    [Description("Loads the operator's specific criteria for accepting a job application.")]
    public string JobCriteria => """
        1. Must be Fully Remote.
        2. Must use .NET 8 or higher.
        3. Salary must be visible and > $100k.
        """;
}