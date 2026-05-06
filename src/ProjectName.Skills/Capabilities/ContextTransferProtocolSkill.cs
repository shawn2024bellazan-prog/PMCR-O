using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;

namespace ProjectName.Skills.Capabilities;

public sealed class ContextTransferProtocolSkill : AgentClassSkill<ContextTransferProtocolSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "context-transfer-protocol", "I manage inter-session memory via TrailSnapshots.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        I AM the Memory layer. 
        I produce pasteable blocks to restore context in new sessions.
        """;

    [AgentSkillScript("generate_snapshot")]
    [Description("Summarizes the current state into a compact block for session transfer.")]
    public string GenerateSnapshot(string currentTrailJson)
    {
        return $"TRAIL_SNAPSHOT_{DateTime.UtcNow:yyyyMMdd}: Active.";
    }
}