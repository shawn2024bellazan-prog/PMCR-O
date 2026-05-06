using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace ProjectName.Skills.Capabilities;

public sealed class ArtifactGeneratorSkill : AgentClassSkill<ArtifactGeneratorSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "artifact-generator", "I enforce implementation completeness in all artifacts.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        ## CAPABILITY BOUNDARY GUARD
        MY_CAPABILITIES:
        - Validating that C# and Markdown artifacts are complete.
        - Detecting stubs and placeholder comments.
        
        OUT_OF_SCOPE:
        - Deciding what to build -> Route to: PlannerSkill
        """;

    [AgentSkillScript("check_for_stubs")]
    [Description("Scans an artifact string for TODOs or NotImplemented exceptions. Returns 'CLEAN' or 'STUB_DETECTED'.")]
    public string CheckForStubs(string content)
    {
        if (content.Contains("TODO") || content.Contains("NotImplementedException") || content.Contains("// ..."))
            return "STUB_DETECTED: The artifact contains unfinished markers.";

        return "CLEAN";
    }
}