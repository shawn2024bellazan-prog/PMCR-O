// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — CHECKER SERVICE
// File       : Services/CheckerGrpcService.cs
// Identity   : I AM the CheckerGrpcService — the Quality Lobe.
// Law Anchor : EC-003
// ThoughtLock: 2026-05-06
//
// FIX (CS9006 / CS0103 / CS1073 — FRAC-CHECKER-RAWSTRING-001):
//   Same root cause as Maker/Reflector. The prompt used $"""...""" (single $).
//   The schemaExample is pre-serialised C# (no braces in the string literal
//   itself), so the schema line was fine — but "Start your response with {"
//   caused CS9006 because the single { triggered an interpolation parse.
//   Switched to $$"""..."""; all interpolations use {{...}}.
//
// FIX (FRAC-CHECKER-JSONMODE-001):
//   Added ChatResponseFormat.Json to GetResponseAsync ChatOptions.
//   IChatClient.GetResponseAsync accepts ChatOptions which includes ResponseFormat.
//   This activates Ollama's JSON mode at the inference layer.
//
// FIX (FRAC-CHECKER-SSE-001):
//   ExecuteMcpRead called JsonDocument.Parse(json) on the raw HTTP response body,
//   which is SSE format when the MCP server uses Streamable HTTP (Stateless = true):
//     event: message
//     data: {"result":{"content":[...]}, ...}
//
//   JsonDocument.Parse fails immediately with:
//     'e' is an invalid start of a value. LineNumber: 0 | BytePositionInLine: 0.
//   because the body starts with "event:" not "{".
//
//   Resolution:
//     Added UnwrapSse() — a static helper that scans the response body for the
//     first "data: " line, strips the prefix, and returns the JSON payload.
//     If no "data: " line is found (e.g. plain JSON from a future transport),
//     the raw body is returned unchanged so the existing parse path still works.
//
// PRIOR FIXES (preserved):
//   FRAC-CHECKER-TOOLUSE-001 — Pre-fetch files in C#; no tool-use pipeline.
//   FRAC-CHECKER-MCP-001     — Named "mcp-filesystem" client, relative path.
//   FRAC-CHECKER-JSON-001    — JSON-only prompt + SanitiseAndValidate guard.
// ═══════════════════════════════════════════════════════════════════════════════

using Grpc.Core;
using Microsoft.Extensions.AI;
using ProjectName.Contracts;
using ProjectName.Core.Models;
using ProjectName.Skills.Core;
using System.Text.Json;

namespace ProjectName.CheckerService.Services;

public sealed class CheckerGrpcService(
    CheckerSkill checkerSkill,
    IChatClient chatClient,
    IHttpClientFactory httpClientFactory,
    ILogger<CheckerGrpcService> logger) : Checker.CheckerBase
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = false,
        PropertyNameCaseInsensitive = true,
    };

    public override async Task<CheckerResponse> ExecuteCheck(CheckerRequest request, ServerCallContext context)
    {
        logger.LogInformation("[CheckerLobe] Evaluating for Trail {Id}", request.TrailId);

        var state = JsonSerializer.Deserialize<CycleState>(request.CycleStateJson)
                    ?? throw new InvalidOperationException("CycleState deserialized as null.");

        // ── PRE-FETCH FILE EVIDENCE (C# level, no LLM tool-call) ─────────────────
        // FRAC-CHECKER-TOOLUSE-001: Read files directly in C# and embed in the prompt.
        var evidence = new System.Text.StringBuilder();
        evidence.AppendLine("## File Verification Evidence");

        var filePaths = (state.MakerFrame?.DispatchDecisions ?? [])
            .Where(d => d.Mcp == McpActuator.filesystem && d.Args.TryGetValue("relativePath", out _))
            .Select(d => d.Args["relativePath"]?.ToString() ?? string.Empty)
            .Where(p => !string.IsNullOrWhiteSpace(p))
            .Distinct()
            .ToList();

        if (filePaths.Count == 0)
        {
            evidence.AppendLine("No file write decisions to verify.");
        }
        else
        {
            foreach (var path in filePaths)
            {
                var content = await ExecuteMcpRead("ReadFile", new { relativePath = path }, context.CancellationToken);
                var preview = content.Length > 300 ? content[..300] + "...[truncated]" : content;
                evidence.AppendLine($"File: {path}");
                evidence.AppendLine($"Content: {preview}");
                evidence.AppendLine();
            }
        }

        var schemaExample = JsonSerializer.Serialize(new QualityFrame
        {
            TrailId = request.TrailId,
            Verdict = "ACCEPT",
            Rationale = "All artifacts verified and match the plan.",
            Correctness = 1.0,
            Completeness = 1.0,
            LawCompliance = 1.0,
            Slv = 1.0,
        }, _jsonOptions);

        // FIX CS9006: $$"""...""" — {{ and }} are literal braces; {{expr}} is interpolation.
        var prompt = $$"""
            You are the Checker agent of the PMCRO Cognitive Architecture.

            CRITICAL INSTRUCTION: Respond ONLY with a single valid JSON object.
            Do NOT include any prose, explanation, markdown fences, or preamble.
            Start your response with { and end with }.

            Required JSON schema (use exactly these property names):
            {{schemaExample}}

            Field rules:
            - trail_id: always "{{request.TrailId}}"
            - verdict: one of ACCEPT | LOOP | ESCALATE
            - rationale: one sentence explaining the verdict.
            - correctness, completeness, law_compliance, slv: scores 0.0–1.0.
            - earned_constraints: list of strings (may be []).
            - improvement_directives: list of strings (may be []).

            Maker output to evaluate:
            {{JsonSerializer.Serialize(state.MakerFrame, _jsonOptions)}}

            Dispatch results:
            {{JsonSerializer.Serialize(state.DispatchResults, _jsonOptions)}}

            {{evidence}}
            """;

        // FIX FRAC-CHECKER-JSONMODE-001: Activate Ollama JSON mode via ChatOptions.ResponseFormat.
        var chatOptions = new ChatOptions { ResponseFormat = ChatResponseFormat.Json };

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, checkerSkill.GetInstructions()),
            new(ChatRole.User, prompt)
        };

        var result = await chatClient.GetResponseAsync(messages, chatOptions, context.CancellationToken);
        var rawText = result.Text ?? string.Empty;

        return new CheckerResponse { Success = true, QualityFrameJson = SanitiseAndValidate(rawText, request.TrailId) };
    }

    private async Task<string> ExecuteMcpRead(string toolName, object args, CancellationToken ct)
    {
        logger.LogInformation("[CheckerLobe] Pre-fetching file evidence via {Tool}", toolName);

        var payload = new
        {
            jsonrpc = "2.0",
            id = Guid.NewGuid().ToString(),
            method = "tools/call",
            @params = new { name = toolName, arguments = args }
        };

        try
        {
            var client = httpClientFactory.CreateClient("mcp-filesystem");
            var res = await client.PostAsJsonAsync("/mcp", payload, ct);
            var raw = await res.Content.ReadAsStringAsync(ct);

            // FIX (FRAC-CHECKER-SSE-001): Streamable HTTP (Stateless = true) returns SSE format:
            //   event: message
            //   data: {"result":{"content":[{"type":"text","text":"..."}]}, ...}
            //
            // JsonDocument.Parse fails on "event:" with "'e' is an invalid start of a value."
            // UnwrapSse() extracts the JSON payload from the data: line before parsing.
            var json = UnwrapSse(raw);

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("result", out var r)
                && r.TryGetProperty("content", out var c)
                && c.GetArrayLength() > 0)
                return c[0].GetProperty("text").GetString() ?? "[EMPTY]";
            return json;
        }
        catch (Exception ex)
        {
            logger.LogWarning("[CheckerLobe] File pre-fetch failed: {Msg}", ex.Message);
            return $"[ACTUATOR_ERROR] {ex.Message}";
        }
    }

    /// <summary>
    /// Extracts the JSON payload from a Server-Sent Events response body.
    /// SSE format: one or more lines of "event: ...\ndata: {json}\n\n"
    /// Returns the first data: line's value. If no data: line is found,
    /// returns the raw input unchanged (safe fallback for plain-JSON responses).
    /// </summary>
    private static string UnwrapSse(string raw)
    {
        foreach (var line in raw.Split('\n'))
        {
            var trimmed = line.Trim();
            if (trimmed.StartsWith("data: ", StringComparison.Ordinal))
                return trimmed["data: ".Length..].Trim();
        }
        return raw; // not SSE — return as-is
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

        try
        {
            var frame = JsonSerializer.Deserialize<QualityFrame>(cleaned, _jsonOptions)
                        ?? throw new JsonException("Deserialised as null.");
            frame.EarnedConstraints ??= [];
            frame.ImprovementDirectives ??= [];
            return JsonSerializer.Serialize(frame, _jsonOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex,
                "[CheckerLobe] FRAC-CHECKER-JSON-001 — non-JSON output. Raw (500): {Raw}",
                cleaned.Length > 500 ? cleaned[..500] : cleaned);

            return JsonSerializer.Serialize(new QualityFrame
            {
                TrailId = trailId,
                Verdict = "ESCALATE",
                Rationale = "Checker LLM returned non-JSON output. See logs for raw text.",
                Correctness = 0.0,
                Completeness = 0.0,
                LawCompliance = 0.0,
                Slv = 0.0,
                EarnedConstraints = [],
                ImprovementDirectives = ["Checker LLM must return JSON. Check prompt and model."]
            }, _jsonOptions);
        }
    }
}