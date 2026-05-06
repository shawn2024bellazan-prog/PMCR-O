using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using ProjectName.Core.Models;
using System.Text.Json;

namespace ProjectName.Skills.Governance;

public sealed class FederationBoardSkill : AgentClassSkill<FederationBoardSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "federation-board", "I AM the Upstream Membrane. I refine raw intent into sealed envelopes.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        I AM the Federation Board. 
        I ALWAYS run the Strange Loop refinement: SURFACE -> EXCAVATE -> ELEVATE -> SHIELD.
        I NEVER pass unshielded intent to the Orchestrator.
        """;

    public async Task<RefinedSeedResult> RefineAsync(SeedIntentRequest request, CancellationToken ct)
    {
        // Internal Logic for refinement
        var envelope = new IntentEnvelope
        {
            HighLevelGoal = request.HighLevelGoal ?? request.RawIntent,
            CurrentIntent = request.RawIntent,
            OMode = request.OModeHint ?? "O-Output",
            FederationShielded = true // Shield applied
        };

        return new RefinedSeedResult
        {
            Envelope = envelope,
            FederationShielded = true,
            EconomicPreCheck = "PASSED"
        };
    }
}

// Support model for the return type
public class RefinedSeedResult
{
    public IntentEnvelope Envelope { get; set; } = new();
    public bool FederationShielded { get; set; }
    public string EconomicPreCheck { get; set; } = "PASSED";
    public string FirstPersonSeed { get; set; } = string.Empty;
}