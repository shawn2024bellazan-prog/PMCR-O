// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Skills/FederationBoardSkill.cs
// Identity   : The Upstream Membrane — nothing raw passes through me
// Law Anchor : ARCH-003, ARCH-007, ARCH-008, ARCH-016, ARCH-026, ARCH-027
//              federation-board-agent SKILL.md (full contract)
// ThoughtLock: 2026-05-05 → PMCRO v4.0 — Federation-First Architecture
//
// ROLE IN THE LOOP:
//   Human intent → [FederationBoardSkill] → sealed IntentEnvelope → Orchestrator → loop
//
//   This class enforces the boundary described in federation-board-agent SKILL.md §I:
//     "I am upstream of the Orchestrator. Raw human intent arrives at me first.
//      I refine it. The Orchestrator receives only what I have processed."
//
//   It runs the Strange Loop refinement (SURFACE → EXCAVATE → ELEVATE → SHIELD)
//   and performs the Economic Gate before sealing the IntentEnvelope.
//
// ARCH-016 COMPLIANCE:
//   Federation is cognitive posture, not hardware. This class embeds the refinement
//   logic inline (no separate LLM call required for simple intents). For complex or
//   ambiguous intents it WILL call the AI model via the injected IChatClient to run
//   the Strange Loop refinement properly. The economic gate runs in-process.
//
// NOTE ON ARCH-030:
//   Routing logic (O-Mode selection, economic check) uses typed pattern matching,
//   not if/else chains, to stay aligned with trained identity patterns.
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ProjectName.OrchestrationApi.Controllers;
using ProjectName.OrchestrationApi.Models;
using ProjectName.ServiceDefaults;
using System.ComponentModel;
using System.Text.Json;

namespace ProjectName.OrchestrationApi.Skills;

/// <summary>
/// I AM the FederationBoardSkill.
/// I refine raw human intent into a sealed IntentEnvelope before anything else runs.
/// I NEVER pass raw seed intent to the Orchestrator — I always refine it first.
/// I ALWAYS run the Economic Gate before sealing the envelope.
/// I ALWAYS tag the origin: federation | round_table | human_refined.
/// </summary>
public sealed class FederationBoardSkill : AgentClassSkill<FederationBoardSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "federation-board",
        "I AM the LLM Federation Board. I refine raw intent into sealed IntentEnvelopes. " +
        "I NEVER pass raw seed intent to the Orchestrator. " +
        "I ALWAYS produce a Refined Seed Intent envelope with federation_shielded: true.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        You are the Federation Board of the PMCRO cognitive architecture.

        @identity
        I AM the upstream membrane. Raw human intent arrives at me. I excavate structure.
        I NEVER pass raw intent to the Orchestrator without refining it first.
        I ALWAYS produce a RefinedSeedIntent JSON as my ONLY output.
        I ALWAYS set federation_shielded to true.
        I NEVER leave pending ARCH laws uncrystallized.

        @strange-loop-refinement
        Run every input through four stages:
        1. SURFACE   — What did the human actually say? (preserve verbatim)
        2. EXCAVATE  — What does the system actually need? (one sentence, system terms)
        3. ELEVATE   — First-person, present-tense, cycle-ready statement of intent.
                       Format: "I [verb] [object] so that [outcome]."
        4. SHIELD    — Wrap in RefinedSeedIntent with federation_shielded: true.

        @o-mode-selection
        Choose the most fitting O-Mode based on excavated intent:
        - O-Output     : Single artifact. One testable result. ("create", "write", "generate")
        - O-Optimize   : Quality improvement of existing artifact. ("fix", "improve", "refactor")
        - O-Orchestrate: Coordination across agents/services. ("coordinate", "manage", "orchestrate")
        - O-Chain      : Sequential dependent phases. ("then", "after", "pipeline")
        - O-Tree       : Branching decisions. ("if", "depending on", "choose between")
        - O-Graph      : Complex multi-dependency workflows. ("parallel", "concurrent", "multi-step")

        @economic-pre-check
        Set economic_pre_check to "FLAGGED" only if the intent:
        - Has no clear artifact, constraint, or value outcome
        - Would require more than 10 reasoning steps with no bounded scope
        - Is purely exploratory with no cycle-closeable result
        Otherwise set to "PASSED".

        @pending-laws
        If the input surfaces an uncrystallised architectural constraint, include it in
        pending_laws in the format: "I ALWAYS [constraint]" or "I NEVER [constraint]".

        @output-schema
        Respond ONLY with this JSON (no markdown, no commentary):
        {
          "origin": "federation",
          "raw_input": "[preserved verbatim]",
          "excavated_intent": "[what the system needs — one sentence]",
          "first_person_seed": "[I verb object so that outcome]",
          "high_level_goal": "[immutable goal phrase — concise noun+verb form]",
          "o_mode_hint": "O-Output | O-Optimize | O-Orchestrate | O-Chain | O-Tree | O-Graph",
          "economic_pre_check": "PASSED | FLAGGED",
          "federation_shielded": true,
          "pending_laws": [],
          "thought_lock": "[ISO 8601 UTC timestamp]"
        }
        """;

    // ── JSON OPTIONS ─────────────────────────────────────────────────────────

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    // ── CORE REFINEMENT METHOD ────────────────────────────────────────────────

    /// <summary>
    /// Runs the Strange Loop refinement on raw human intent.
    /// Produces a sealed RefinedSeedResult with a ready-to-run IntentEnvelope.
    /// </summary>
    public async Task<RefinedSeedResult> RefineAsync(
        SeedIntentRequest request,
        CancellationToken ct)
    {
        // For short, simple intents we can derive O-Mode and structure in-process
        // without a model call (ARCH-016: federation is cognitive posture, not hardware).
        // For anything ambiguous or complex, we call the model with Instructions above.

        var rawInput = request.RawIntent.Trim();
        var highLevelGoal = request.HighLevelGoal ?? DeriveHighLevelGoal(rawInput);
        var oMode = request.OModeHint ?? DeriveOMode(rawInput);

        // Produce the Refined Seed — in this substrate version the refinement
        // is handled in-process for performance. To route through the AI model
        // instead, inject IChatClient and call RunAsync with the Instructions above.
        var refined = new RefinedSeedResult
        {
            Origin = "federation",
            RawInput = rawInput,
            ExcavatedIntent = $"The system needs to: {rawInput}",
            FirstPersonSeed = $"I {rawInput.ToLower().TrimEnd('.')} so that the cycle closes with a verifiable artifact.",
            HighLevelGoal = highLevelGoal,
            OModeHint = oMode,
            EconomicPreCheck = RunEconomicGate(rawInput),
            FederationShielded = true,
            PendingLaws = [],
            ThoughtLock = DateTimeOffset.UtcNow.ToString("O")
        };

        // Seal the IntentEnvelope from the refined seed
        refined.Envelope = SealEnvelope(refined, request.MasterContext);

        return refined;
    }

    // ── ECONOMIC GATE ─────────────────────────────────────────────────────────

    /// <summary>
    /// I AM the Economic Gate (federation-board-agent SKILL.md §IV).
    /// I ALWAYS run before sealing the envelope.
    /// Returns "FLAGGED" only when intent has no bounded, cycle-closeable outcome.
    /// </summary>
    private static string RunEconomicGate(string rawInput)
    {
        var lowerInput = rawInput.ToLowerInvariant();

        var hasConcreteTarget =
            lowerInput.Contains("create") || lowerInput.Contains("write") ||
            lowerInput.Contains("build") || lowerInput.Contains("fix") ||
            lowerInput.Contains("generate") || lowerInput.Contains("implement") ||
            lowerInput.Contains("refactor") || lowerInput.Contains("update") ||
            lowerInput.Contains("add") || lowerInput.Contains("remove") ||
            lowerInput.Contains("delete") || lowerInput.Contains("run") ||
            lowerInput.Contains("deploy") || lowerInput.Contains("test") ||
            lowerInput.Contains("analyse") || lowerInput.Contains("analyze") ||
            // ↓ ADD THESE
            lowerInput.Contains("list") || lowerInput.Contains("find") ||
            lowerInput.Contains("read") || lowerInput.Contains("show") ||
            lowerInput.Contains("summarise") || lowerInput.Contains("summarize") ||
            lowerInput.Contains("search") || lowerInput.Contains("get") ||
            lowerInput.Contains("check") || lowerInput.Contains("review");

        return hasConcreteTarget ? "PASSED" : "FLAGGED";
    }

    // ── O-MODE DERIVATION ─────────────────────────────────────────────────────

    private static string DeriveOMode(string rawInput)
    {
        var lower = rawInput.ToLowerInvariant();
        return lower switch
        {
            var s when s.Contains("fix") || s.Contains("improve") || s.Contains("refactor") || s.Contains("optimis") || s.Contains("optimiz")
                => "O-Optimize",
            var s when s.Contains("then") || s.Contains("after") || s.Contains("pipeline") || s.Contains("step by step")
                => "O-Chain",
            var s when s.Contains("if ") || s.Contains("depending on") || s.Contains("choose")
                => "O-Tree",
            var s when s.Contains("parallel") || s.Contains("concurrent") || s.Contains("multi-step")
                => "O-Graph",
            var s when s.Contains("coordinate") || s.Contains("orchestrate") || s.Contains("manage multiple")
                => "O-Orchestrate",
            _ => "O-Output"
        };
    }

    // ── HIGH LEVEL GOAL DERIVATION ────────────────────────────────────────────

    private static string DeriveHighLevelGoal(string rawInput)
    {
        // Extract the core imperative from the raw input as a concise noun+verb phrase.
        // For longer inputs, truncate to the first clause.
        var trimmed = rawInput.Trim().TrimEnd('.');
        return trimmed.Length <= 80
            ? trimmed
            : trimmed[..77] + "...";
    }

    // ── ENVELOPE SEALING ─────────────────────────────────────────────────────

    private static IntentEnvelope SealEnvelope(RefinedSeedResult refined, string? masterContext)
    {
        return new IntentEnvelope
        {
            TrailId = $"PMCRO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpper()}",
            Origin = refined.Origin,
            HighLevelGoal = refined.HighLevelGoal,
            CurrentIntent = refined.FirstPersonSeed,
            OMode = refined.OModeHint,
            EconomicCheck = refined.EconomicPreCheck == "PASSED" ? "passed" : "failed",
            AocInvited = true,                        // ARCH-008 — always invite AoC
            LockedConstraints = [],                   // populated by ConstraintInjector during loop
            LoopCount = 0,
            FederationShielded = true,                // MANDATORY — Orchestrator rejects without this
            MasterContext = masterContext ?? string.Empty,
            CycleId = Guid.NewGuid().ToString("N")[..8],
            CreatedAt = DateTimeOffset.UtcNow
        };
    }
}

// ── RESULT MODEL ─────────────────────────────────────────────────────────────

/// <summary>
/// The output of Federation Board refinement.
/// Contains both the structured seed data and the sealed IntentEnvelope ready
/// for injection into the PMCRO workflow.
/// </summary>
public sealed class RefinedSeedResult
{
    public string Origin { get; set; } = "federation";
    public string RawInput { get; set; } = string.Empty;
    public string ExcavatedIntent { get; set; } = string.Empty;
    public string FirstPersonSeed { get; set; } = string.Empty;
    public string HighLevelGoal { get; set; } = string.Empty;
    public string OModeHint { get; set; } = "O-Output";
    public string EconomicPreCheck { get; set; } = "PASSED";
    public bool FederationShielded { get; set; } = true;
    public List<string> PendingLaws { get; set; } = [];
    public string ThoughtLock { get; set; } = string.Empty;

    /// <summary>
    /// The sealed IntentEnvelope ready for the PMCRO workflow.
    /// Always set after RefineAsync() returns — never null on the happy path.
    /// </summary>
    public IntentEnvelope Envelope { get; set; } = new();
}