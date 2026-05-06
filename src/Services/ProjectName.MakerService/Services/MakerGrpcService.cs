// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — MAKER SERVICE
// File       : Services/MakerGrpcService.cs
// Identity   : I AM the ArtifactMaker Lobe
// Law Anchor : WORKFLOW-010
// ThoughtLock: 2026-05-06
//
// FIX (CS9006 / CS1733 / CS0103 — FRAC-MAKER-RAWSTRING-001):
//   The prompt used $"""...""" (single $). In a single-$ raw string, {{ is a
//   literal '{' — but the parser still tries to read what follows as an
//   interpolation hole. So {{ "trail_id" is parsed as interpolation starting
//   with the identifier trail_id (invalid), producing CS9006 / CS0103 / CS1073.
//
//   Resolution: Switch to $$"""...""" (two dollar signs). In a $$-raw string,
//   {{ and }} are always literal braces. Interpolations require {{expr}}.
//   All interpolation sites below use {{...}} accordingly.
//
// FIX (FRAC-MAKER-JSONMODE-001):
//   Added AgentRunOptions { ResponseFormat = ChatResponseFormat.Json }.
//   This activates Ollama's native JSON mode at the inference layer — an
//   additional constraint beyond the prompt instruction alone.
//   NOTE: ChatResponseFormat.ForJsonSchema<T>() (schema-constrained output)
//   requires OpenAI/Azure OpenAI structured output support and does NOT work
//   with Ollama. ChatResponseFormat.Json (JSON mode) IS supported.
//
// FIX (FRAC-MAKER-DIRTRAVERSAL-001):
//   The model consistently produced ReadFile("skills/SKILL.md") after a
//   ListDirectory("skills") that returned subdirectory names like "planner-agent",
//   "checker-agent" etc. There is no flat skills/SKILL.md — each skill lives at
//   skills/{subdirectory}/SKILL.md.
//
//   Root cause: The prompt documented the tool argument shapes but gave no
//   guidance on how directory listing results should inform subsequent ReadFile
//   paths. The model defaulted to a plausible-but-wrong flat path.
//
//   Resolution: Added an explicit "Directory traversal rule" to the prompt that
//   instructs the model:
//     1. ListDirectory returns entry names (files and subdirs), not full paths.
//     2. To read a file inside a subdirectory, construct the full relative path:
//        {listed_parent}/{entry_name}/{target_filename}
//     3. When the goal is to read SKILL.md files inside each subdirectory,
//        emit one ReadFile decision per subdirectory entry, not one ReadFile
//        against the parent directory itself.
//   Also added a concrete worked example so the model has a pattern to follow.
//
// PRIOR FIXES (preserved):
//   FRAC-MAKER-TRUNCATION-001 — RecoverTruncated() for mid-array truncation.
//   FRAC-MAKER-PREAMBLE-001   — Forward-scan to first '{' after fence strip.
//   FRAC-MAKER-JSON-001       — mcp/tool/args schema, forbidden key names.
// ═══════════════════════════════════════════════════════════════════════════════

using Grpc.Core;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ProjectName.Contracts;
using ProjectName.Core.Models;
using ProjectName.Skills.Core;
using System.Text.Json;

namespace ProjectName.MakerService.Services;

public sealed class MakerGrpcService(
    MakerSkill makerSkill,
    IChatClient chatClient,
    ILogger<MakerGrpcService> logger) : Maker.MakerBase
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
    };

    public override async Task<MakerResponse> ExecuteMake(MakerRequest request, ServerCallContext context)
    {
        logger.LogInformation("[MakerLobe] Executing for Trail {Id}", request.TrailId);

        var state = JsonSerializer.Deserialize<CycleState>(request.CycleStateJson)!;

        var agent = chatClient.AsAIAgent(makerSkill.Frontmatter.Name, makerSkill.GetInstructions());

        var stepLines = state.Plan?.Steps
            .Select(s => $"  {s.Id}: {s.Description}")
            ?? ["  (no steps)"];
        var stepsText = string.Join("\n", stepLines);

        // FIX CS9006: $$"""...""" — {{ and }} are literal braces; {{expr}} is interpolation.
        var prompt = $$"""
            You are the Maker agent of the PMCRO Cognitive Architecture.

            CRITICAL: Respond ONLY with a single valid JSON object. No prose, no markdown fences.
            Start your response with { and end with }.

            The object must have exactly these keys:

            {
              "trail_id": "{{request.TrailId}}",
              "artifacts": ["string", ...],
              "dispatch_decisions": [
                {
                  "mcp": "filesystem",
                  "tool": "WriteFile",
                  "args": { "relativePath": "path/to/file.txt", "content": "file content" },
                  "artifact_type": "text",
                  "rationale": "why this write is needed"
                }
              ]
            }

            Rules:
            - "mcp" must be one of: filesystem | terminal | playwright | postman
            - Common filesystem tools: WriteFile, ReadFile, ListDirectory, DeleteFile
            - WriteFile args: relativePath (string), content (string), overwrite (bool, default true)
            - ReadFile / ListDirectory args: relativePath (string)
            - FORBIDDEN keys: file_write_operation, write_file, file_operation, write, operation
            - dispatch_decisions may be [] for cognitive-only cycles
            - Maximum 4 dispatch_decisions per cycle (context limit)
            - Close ALL JSON arrays and objects — never leave them open

            DIRECTORY TRAVERSAL RULE (FRAC-MAKER-DIRTRAVERSAL-001):
            ListDirectory returns entry names only — not full paths. Each entry is either a
            file name or a subdirectory name relative to the listed path.

            If you need to read a specific file inside each subdirectory:
              - The full relativePath for ReadFile is: {listed_parent}/{entry_name}/{target_file}
              - NEVER use just {listed_parent}/{target_file} — that reads from the parent, not the subdir.

            Example: goal = "read SKILL.md from each subdirectory of skills/"
              Step 1 — ListDirectory: relativePath = "skills"
                Returns entries: ["planner-agent", "checker-agent", "maker-agent"]
              Step 2 — ReadFile for each entry:
                relativePath = "skills/planner-agent/SKILL.md"   ← CORRECT
                relativePath = "skills/checker-agent/SKILL.md"   ← CORRECT
                relativePath = "skills/SKILL.md"                  ← WRONG (no such file)

            Because the context limit is 4 decisions per cycle, if a ListDirectory returns
            more entries than you can read in this cycle, read the first 3 subdirectories
            and note the remainder in the rationale.

            Plan steps to execute:
            {{stepsText}}
            """;

        // FIX FRAC-MAKER-JSONMODE-001: ChatResponseFormat.Json = Ollama JSON mode.
        // ForJsonSchema<T>() is OpenAI-only and will throw with Ollama.
        var runOptions = new AgentRunOptions { ResponseFormat = ChatResponseFormat.Json };

        var result = await agent.RunAsync(prompt, options: runOptions);
        var rawText = result.Text ?? string.Empty;

        return new MakerResponse { Success = true, MakerFrameJson = SanitiseAndValidate(rawText, request.TrailId) };
    }

    private string SanitiseAndValidate(string raw, string trailId)
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

        var candidate = TryDeserialize(cleaned);
        if (candidate is not null) return JsonSerializer.Serialize(candidate, _jsonOptions);

        var recovered = RecoverTruncated(cleaned);
        if (recovered is not null)
        {
            candidate = TryDeserialize(recovered);
            if (candidate is not null)
            {
                logger.LogWarning("[MakerLobe] FRAC-MAKER-TRUNCATION-001 — recovered {Count} decisions.",
                    candidate.DispatchDecisions?.Count ?? 0);
                return JsonSerializer.Serialize(candidate, _jsonOptions);
            }
        }

        logger.LogWarning("[MakerLobe] FRAC-MAKER-JSON-001 — non-JSON output. Raw (500): {Raw}",
            cleaned.Length > 500 ? cleaned[..500] : cleaned);

        return JsonSerializer.Serialize(
            new MakerFrame { TrailId = trailId, Artifacts = [], DispatchDecisions = [] },
            _jsonOptions);
    }

    private MakerFrame? TryDeserialize(string json)
    {
        try
        {
            var frame = JsonSerializer.Deserialize<MakerFrame>(json, _jsonOptions);
            if (frame is null) return null;
            frame.DispatchDecisions ??= [];
            frame.Artifacts ??= [];
            return frame;
        }
        catch (JsonException ex)
        {
            logger.LogDebug("[MakerLobe] Parse attempt failed: {Msg}", ex.Message);
            return null;
        }
    }

    private static string? RecoverTruncated(string raw)
    {
        try
        {
            const string arrayKey = "\"dispatch_decisions\"";
            var keyIdx = raw.IndexOf(arrayKey, StringComparison.Ordinal);
            if (keyIdx < 0) return null;

            var arrayOpen = raw.IndexOf('[', keyIdx + arrayKey.Length);
            if (arrayOpen < 0) return null;

            int depth = 0, lastComplete = -1;
            bool inString = false, escape = false;

            for (int i = arrayOpen + 1; i < raw.Length; i++)
            {
                char c = raw[i];
                if (escape) { escape = false; continue; }
                if (c == '\\' && inString) { escape = true; continue; }
                if (c == '"') { inString = !inString; continue; }
                if (inString) continue;

                if (c == '{') depth++;
                else if (c == '}') { depth--; if (depth == 0) lastComplete = i; }
                else if (c == ']' && depth == 0) return null;
            }

            return lastComplete < 0 ? null : raw[..(lastComplete + 1)] + "]}";
        }
        catch { return null; }
    }
}