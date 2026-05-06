// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — REFLECTOR SERVICE
// File       : Services/ReflectorGrpcService.cs
// Identity   : I AM the ReflectorGrpcService — the Learning Lobe.
// Law Anchor : WORKFLOW-010
// ThoughtLock: 2026-05-06
//
// FIX (CS9006 / CS1733 / CS0103 — FRAC-REFLECTOR-RAWSTRING-001):
//   Same root cause as MakerGrpcService. The prompt used $"""...""" (single $),
//   causing the {{ }} around JSON example braces to be misread as interpolation
//   holes by the C# parser. Switched to $$"""..."""; all interpolations use {{...}}.
//
// FIX (FRAC-REFLECTOR-JSONMODE-001):
//   Added AgentRunOptions { ResponseFormat = ChatResponseFormat.Json } to activate
//   Ollama JSON mode at the inference layer, same pattern as MakerGrpcService.
//
// FIX (FRAC-REFLECTOR-HALLUCINATION-001):
//   The Reflector was producing fabricated enterprise-sounding output:
//     crystallised_constraint: "system scalability must exceed 99.95% under peak load"
//     law_code: "ART-321.7(d)"
//     next_cycle_seed: "Validate cross-node communication latency thresholds"
//   None of this had any relation to the actual cycle.
//
//   Root cause: The prompt only supplied the quality verdict, directives, and
//   rationale — no information about what the Maker actually attempted or what
//   the dispatch results were. With minimal grounding, the model filled the
//   fields with plausible-but-fabricated text.
//
//   Resolution:
//     Added a "Cycle evidence" section to the prompt containing:
//       - The truest_intent from the Planner (what was the goal)
//       - A summary of each dispatch decision (tool + path + success/error)
//     This gives the model concrete facts to reflect on. The next_cycle_seed
//     should now describe what the next iteration should actually do (e.g.
//     "Read SKILL.md from each discovered subdirectory") rather than hallucinated
//     infrastructure concerns.
//
//     Also tightened the field-level instructions:
//       - crystallised_constraint: must be derived from the actual cycle, or ""
//       - law_code: PMCRO law code only, or ""
//       - next_cycle_seed: must reference actual artifacts or directories found
//
// PRIOR FIXES (preserved):
//   FRAC-REFLECTOR-PREAMBLE-001  — Forward-scan to first '{'.
//   FRAC-REFLECTOR-CONTEXT-001   — Compact prompt to preserve KV context budget.
//   FRAC-REFLECTOR-JSON-001      — JSON-only prompt + SanitiseAndValidate guard.
// ═══════════════════════════════════════════════════════════════════════════════

using Grpc.Core;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ProjectName.Contracts;
using ProjectName.Core.Models;
using ProjectName.Skills.Core;
using System.Text.Json;

namespace ProjectName.ReflectorService.Services;

public sealed class ReflectorGrpcService(
    ReflectorSkill reflectorSkill,
    IChatClient chatClient,
    ILogger<ReflectorGrpcService> logger) : Reflector.ReflectorBase
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
    };

    public override async Task<ReflectorResponse> ExecuteReflect(ReflectorRequest request, ServerCallContext context)
    {
        logger.LogInformation("[ReflectorLobe] Crystallizing learning for Trail {Id}", request.TrailId);

        var state = JsonSerializer.Deserialize<CycleState>(request.CycleStateJson)!;

        var agent = chatClient.AsAIAgent(reflectorSkill.Frontmatter.Name, reflectorSkill.GetInstructions());

        var topDirectives = (state.QualityFrame?.ImprovementDirectives ?? []).Take(2).ToList();
        var directivesText = topDirectives.Count > 0 ? string.Join("; ", topDirectives) : "none";
        var cycleId = $"cycle-{DateTime.UtcNow:yyyyMMdd-HHmmss}";

        // FIX (FRAC-REFLECTOR-HALLUCINATION-001): Build a grounded cycle evidence summary.
        // This gives the model concrete facts about what actually happened so it doesn't
        // fabricate unrelated constraints or seeds.
        var intent = state.Plan?.TruestIntent ?? state.Envelope?.Intent ?? "unknown";

        var dispatchSummaryLines = (state.DispatchResults ?? []).Select(r =>
        {
            var status = r.Success ? "OK" : $"FAIL: {r.Error?[..Math.Min(80, r.Error?.Length ?? 0)]}";
            return $"  {r.Tool}({r.Mcp}) → {status}";
        }).ToList();
        var dispatchSummary = dispatchSummaryLines.Count > 0
            ? string.Join("\n", dispatchSummaryLines)
            : "  (no dispatch decisions this cycle)";

        // FIX CS9006: $$"""...""" — {{ and }} are literal braces; {{expr}} is interpolation.
        var prompt = $$"""
            You are the Reflector agent of the PMCRO Cognitive Architecture.

            CRITICAL: Respond ONLY with a single valid JSON object. No prose, no markdown, no fences.
            Start your response with { and end with }.

            Emit exactly this structure (all fields required):
            {
              "trail_id": "{{request.TrailId}}",
              "cycle_id": "{{cycleId}}",
              "learning_frames": ["string"],
              "crystallised_constraint": "string or empty",
              "law_code": "string or empty",
              "slv": 0.0,
              "next_cycle_seed": "string",
              "locked_cot": "string"
            }

            Field rules — base ALL values on the cycle evidence below, never invent:
            - trail_id: always "{{request.TrailId}}"
            - cycle_id: always "{{cycleId}}"
            - learning_frames: 1–2 strings about what THIS cycle discovered or failed to do
            - crystallised_constraint: a constraint earned from THIS cycle's outcome, or ""
            - law_code: a PMCRO law code (e.g. FRAC-*, ARCH-*) if applicable, else ""
            - slv: 0.0 for ESCALATE, 0.5 for LOOP, 1.0 for ACCEPT
            - next_cycle_seed: concrete next action based on what was discovered (e.g. "Read
              skills/planner-agent/SKILL.md to extract its summary") — reference actual
              paths or artifacts found in this cycle
            - locked_cot: one sentence explaining why the next_cycle_seed was chosen

            Cycle evidence:
            Goal: {{intent}}
            Verdict: {{state.QualityFrame?.Verdict ?? "UNKNOWN"}}
            Rationale: {{state.QualityFrame?.Rationale ?? "No rationale."}}
            Directives: {{directivesText}}
            Dispatch results:
            {{dispatchSummary}}
            """;

        // FIX FRAC-REFLECTOR-JSONMODE-001: Ollama JSON mode.
        var runOptions = new AgentRunOptions { ResponseFormat = ChatResponseFormat.Json };

        var result = await agent.RunAsync(prompt, options: runOptions);
        var rawText = result.Text ?? string.Empty;

        return new ReflectorResponse { Success = true, ReflectorFrameJson = SanitiseAndValidate(rawText, request.TrailId, cycleId, state) };
    }

    private string SanitiseAndValidate(string raw, string trailId, string cycleId, CycleState state)
    {
        var cleaned = raw.Trim();

        if (cleaned.StartsWith("```"))
        {
            var firstNewline = cleaned.IndexOf('\n');
            var lastFence = cleaned.LastIndexOf("```");
            if (firstNewline > 0 && lastFence > firstNewline)
                cleaned = cleaned[(firstNewline + 1)..lastFence].Trim();
        }

        if (!cleaned.StartsWith('{'))
        {
            var braceIdx = cleaned.IndexOf('{');
            if (braceIdx > 0) cleaned = cleaned[braceIdx..];
        }

        try
        {
            var frame = JsonSerializer.Deserialize<ReflectorFrame>(cleaned, _jsonOptions)
                        ?? throw new JsonException("Deserialised as null.");
            frame.LearningFrames ??= [];
            return JsonSerializer.Serialize(frame, _jsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex,
                "[ReflectorLobe] FRAC-REFLECTOR-JSON-001 — non-JSON output. Raw (500): {Raw}",
                cleaned.Length > 500 ? cleaned[..500] : cleaned);

            return JsonSerializer.Serialize(new ReflectorFrame
            {
                TrailId = trailId,
                CycleId = cycleId,
                LearningFrames = [$"Reflector LLM returned non-JSON. Verdict was: {state.QualityFrame?.Verdict ?? "UNKNOWN"}."],
                CrystallisedConstraint = "Reflector must always return JSON — never prose.",
                LawCode = "FRAC-REFLECTOR-JSON-001",
                Slv = 0.0,
                NextCycleSeed = "Fix Reflector JSON compliance before next cycle.",
                LockedCoT = "Parse guard triggered — raw LLM output was not valid JSON."
            }, _jsonOptions);
        }
    }
}