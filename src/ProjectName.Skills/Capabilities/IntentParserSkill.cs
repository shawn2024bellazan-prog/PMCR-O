using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;
using System.Text.Json;

namespace ProjectName.Skills.Capabilities;

public sealed class IntentParserSkill : AgentClassSkill<IntentParserSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "intent-parser",
        "I validate and parse raw intent into a structured schema.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        ## CAPABILITY BOUNDARY GUARD
        MY_CAPABILITIES:
        - Parsing raw strings into structured IntentEnvelopes.
        - Validating JSON schemas.
        
        OUT_OF_SCOPE:
        - Planning actions -> Route to: PlannerSkill
        - Running MCP tools -> Route to: Orchestrator
        """;

    // DETERMINISTIC SCRIPT: The LLM calls this instead of guessing
    [AgentSkillScript("validate_intent_schema")]
    [Description("Validates that a JSON string matches the IntentEnvelope schema.")]
    public string ValidateSchema(string jsonInput)
    {
        try
        {
            var parsed = JsonSerializer.Deserialize<JsonDocument>(jsonInput);
            return parsed?.RootElement.TryGetProperty("federation_shielded", out _) == true
                ? "VALID: Schema matches IntentEnvelope."
                : "INVALID: Missing federation_shielded property.";
        }
        catch (Exception ex)
        {
            return $"INVALID: Malformed JSON. {ex.Message}";
        }
    }
}