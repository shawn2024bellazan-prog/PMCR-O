using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;

namespace ProjectName.Skills.Capabilities;

public sealed class StructureExtractorSkill : AgentClassSkill<StructureExtractorSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "structure-extractor", "I map physical file trees to cognitive context.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        ## CAPABILITY BOUNDARY GUARD
        MY_CAPABILITIES:
        - Filtering and summarizing large directory listings.
        - Identifying entry points (Program.cs, etc.) in a file tree.
        """;

    [AgentSkillScript("summarize_tree")]
    [Description("Takes a raw list of files and returns a concise map of the core project structure.")]
    public string SummarizeTree(string rawFileList)
    {
        // Logic to extract only .csproj and key .cs files to save tokens
        var lines = rawFileList.Split('\n').Where(l => l.EndsWith(".cs") || l.EndsWith(".csproj"));
        return string.Join("\n", lines.Take(20)) + "\n(Truncated for context efficiency)";
    }
}