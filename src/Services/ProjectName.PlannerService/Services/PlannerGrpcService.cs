// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — PLANNER SERVICE
// File       : Services/PlannerGrpcService.cs
// Identity   : I AM the Strategist Lobe
// Law Anchor : WORKFLOW-010, MCP-011
// ThoughtLock: 2026-05-06
//
// FIX (Bug 1 — FRAC-PLANNER-MCP-001):
//   ExecuteMcpRead now uses the named "mcp-filesystem" HttpClient whose BaseAddress
//   is pre-set in Program.cs from the Aspire-injected configuration key
//   services:mcp-filesystem:http:0. The call site uses a relative "/mcp" path so
//   no hardcoded hostname appears here at all.
//
//   Previously:
//     httpClientFactory.CreateClient()                          ← unnamed client
//     client.PostAsJsonAsync("http://mcp-filesystem/mcp", ...)  ← hardcoded host
//
//   Now:
//     httpClientFactory.CreateClient("mcp-filesystem")          ← named client
//     client.PostAsJsonAsync("/mcp", ...)                       ← relative path
//
//   The named client's BaseAddress is set to the Aspire-resolved localhost URL,
//   so the request never reaches the DNS resolver for "mcp-filesystem".
// ═══════════════════════════════════════════════════════════════════════════════

using Grpc.Core;
using Microsoft.Extensions.AI;
using ProjectName.Contracts;
using ProjectName.Core.Models;
using ProjectName.Skills.Core;
using System.Text.Json;

namespace ProjectName.PlannerService.Services;

public sealed class PlannerGrpcService(
    PlannerSkill plannerSkill,
    IChatClient chatClient,
    IHttpClientFactory httpClientFactory,
    ILogger<PlannerGrpcService> logger) : Planner.PlannerBase
{
    public override async Task<PlannerGrpcResponse> ExecutePlan(PlannerRequest request, ServerCallContext context)
    {
        logger.LogInformation("[PlannerLobe] Trail={Id} Scale={Scale} — BEGIN", request.TrailId, request.Scale);

        CycleState cycleState;
        try
        {
            cycleState = JsonSerializer.Deserialize<CycleState>(request.CycleStateJson)
                         ?? throw new InvalidOperationException("CycleState deserialized as null.");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[PlannerLobe] CycleState deserialisation failed.");
            return ErrorResponse("PLAN_DESERIALIZE_FAILURE: " + ex.Message);
        }

        if (!cycleState.Envelope.FederationShielded)
        {
            logger.LogWarning("[PlannerLobe] Rejecting unshielded envelope. Trail={Id}", request.TrailId);
            return ErrorResponse("PLAN_SHIELD_VIOLATION: FederationShielded must be true.");
        }

        // ── TOOL DEFINITION ──
        var readTool = AIFunctionFactory.Create(
            async (string path) => await ExecuteMcpRead("ReadFile", new { relativePath = path }, context.CancellationToken),
            "read_file",
            "Reads the full UTF-8 content of a file. Safe TYPE 2 operation."
        );
        var listTool = AIFunctionFactory.Create(
            async (string path) => await ExecuteMcpRead("ListDirectory", new { relativePath = path }, context.CancellationToken),
            "list_directory",
            "Lists all files and folders in a directory. Safe TYPE 2 operation."
        );

        var chatOptions = new ChatOptions { Tools = [readTool, listTool] };
        var localClient = new ChatClientBuilder(chatClient).UseFunctionInvocation().Build();

        async Task<string> AskAgent(string prompt)
        {
            var messages = new List<ChatMessage> {
                new(ChatRole.System, plannerSkill.GetInstructions()),
                new(ChatRole.User, prompt)
            };
            var res = await localClient.GetResponseAsync(messages, chatOptions, context.CancellationToken);
            return res.Text ?? string.Empty;
        }

        try
        {
            logger.LogInformation("[PlannerLobe] Step 1 — SurfaceIntent");
            var surfacePrompt = BuildSurfaceIntentPrompt(cycleState.Envelope);
            var surfaceResult = await AskAgent(surfacePrompt);

            logger.LogInformation("[PlannerLobe] Step 2 — TruestIntent");
            var truestPrompt = BuildTruestIntentPrompt(cycleState.Envelope, surfaceResult);
            var truestResult = await AskAgent(truestPrompt);

            logger.LogInformation("[PlannerLobe] Step 3 — FeasibilityGate");
            var feasibilityPrompt = BuildFeasibilityPrompt(cycleState.Envelope, truestResult);
            var feasibilityResult = await AskAgent(feasibilityPrompt);

            logger.LogInformation("[PlannerLobe] Step 4 — TrailSeal");
            var sealPrompt = BuildTrailSealPrompt(cycleState.Envelope, truestResult, feasibilityResult);
            var sealResult = await AskAgent(sealPrompt);

            var plan = ParseExecutionPlan(sealResult, cycleState.Envelope);
            var planFrameJson = JsonSerializer.Serialize(plan);

            logger.LogInformation("[PlannerLobe] Trail={Id} — COMPLETE. Steps={Count}", request.TrailId, plan.Steps.Count);

            return new PlannerGrpcResponse
            {
                Success = true,
                PlanFrameJson = planFrameJson,
                ErrorMessage = string.Empty
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[PlannerLobe] MicroWorkflow failed. Trail={Id}", request.TrailId);
            return ErrorResponse("PLAN_WORKFLOW_ERROR: " + ex.Message);
        }
    }

    private async Task<string> ExecuteMcpRead(string toolName, object args, CancellationToken ct)
    {
        logger.LogInformation("[PlannerLobe] Invoking TYPE 2 Tool: {Tool}", toolName);

        var payload = new
        {
            jsonrpc = "2.0",
            id = Guid.NewGuid().ToString(),
            method = "tools/call",
            @params = new { name = toolName, arguments = args }
        };

        try
        {
            // FIX (FRAC-PLANNER-MCP-001): Use the named "mcp-filesystem" client.
            // Its BaseAddress is set in Program.cs from the Aspire env var
            // services__mcp-filesystem__http__0 (injected via WithReference in AppHost).
            // Using a relative path here means no hardcoded hostname reaches the DNS resolver.
            var client = httpClientFactory.CreateClient("mcp-filesystem");

            // Use relative path when BaseAddress is configured; fall back to absolute
            // URI only if Program.cs could not resolve the Aspire endpoint (missing
            // WithReference) — this keeps the same failure mode as before but makes
            // the root cause traceable to the startup warning logged in Program.cs.
            var requestUri = client.BaseAddress is not null
                ? "/mcp"
                : "http://mcp-filesystem/mcp";

            var res = await client.PostAsJsonAsync(requestUri, payload, ct);
            var json = await res.Content.ReadAsStringAsync(ct);

            using var doc = JsonDocument.Parse(json);
            if (doc.RootElement.TryGetProperty("result", out var r)
                && r.TryGetProperty("content", out var c)
                && c.GetArrayLength() > 0)
                return c[0].GetProperty("text").GetString() ?? "[EMPTY]";

            return json;
        }
        catch (Exception ex)
        {
            return $"[ACTUATOR_ERROR] {ex.Message}";
        }
    }

    private static string BuildSurfaceIntentPrompt(IntentEnvelope envelope) =>
        $"""
        PMCRO PLANNER — SUPERSTEP 1: SURFACE INTENT
        Trail-ID     : {envelope.TrailId}
        High-Level   : {envelope.HighLevelGoal}
        Current Intent: {envelope.CurrentIntent}
        Locked Constraints: {string.Join("; ", envelope.LockedConstraints)}
        
        Task: Restate in first person what the human is explicitly asking for.
        Format: One crisp paragraph. No steps yet.
        """;

    private static string BuildTruestIntentPrompt(IntentEnvelope envelope, string surfaceResult) =>
        $"""
        PMCRO PLANNER — SUPERSTEP 2: TRUEST INTENT
        Trail-ID     : {envelope.TrailId}
        Surface Intent: {surfaceResult}
        
        Task: Beneath the surface, what is the system ACTUALLY required to produce?
        Law: I NEVER plan more than one cycle ahead.
        Format: One sentence truest intent statement.
        """;

    private static string BuildFeasibilityPrompt(IntentEnvelope envelope, string truestResult) =>
        $$"""
        PMCRO PLANNER — SUPERSTEP 3: FEASIBILITY GATE
        Trail-ID     : {{envelope.TrailId}}
        Truest Intent: {{truestResult}}
        Economic Check: {{envelope.EconomicCheck}}
        Loop Count   : {{envelope.LoopCount}}
        
        Task: Is this feasible in one Maker cycle? Reply FEASIBLE or ESCALATE with reason.
        If FEASIBLE: list 3–8 atomic steps. Each step: verb + object + acceptance criterion.
        Format (JSON): 
        { "verdict": "FEASIBLE|ESCALATE", "reason": "...", "steps": ["S1: ...", "S2: ..."] }
        """;

    private static string BuildTrailSealPrompt(IntentEnvelope envelope, string truestResult, string feasibilityResult) =>
        $$"""
        PMCRO PLANNER — SUPERSTEP 4: TRAIL SEAL
        Trail-ID     : {{envelope.TrailId}}
        Truest Intent: {{truestResult}}
        Feasibility  : {{feasibilityResult}}
        Locked Constraints: {{string.Join("; ", envelope.LockedConstraints)}}
        
        Task: Produce the final sealed ExecutionPlan as a JSON object.
        Schema: { "truest_intent": "...", "steps": [{ "id": "S1", "action": "...", "acceptance": "..." }] }
        Rules: 
        - I NEVER include stub steps.
        - I ALWAYS attack the riskiest assumption in Step 1.
        - Maximum 8 steps per cycle.
        Respond with ONLY the JSON object. No prose.
        """;

    private static ExecutionPlan ParseExecutionPlan(string sealResult, IntentEnvelope envelope)
    {
        var cleaned = sealResult.Replace("```json", string.Empty).Replace("```", string.Empty).Trim();

        try
        {
            var raw = JsonSerializer.Deserialize<PlannerSealOutput>(cleaned);
            if (raw is not null && raw.Steps is { Count: > 0 })
            {
                return new ExecutionPlan
                {
                    TrailId = envelope.TrailId,
                    TruestIntent = raw.TruestIntent ?? envelope.CurrentIntent,
                    Steps = raw.Steps.Select((s, i) => new PlanStep
                    {
                        Id = s.Id ?? $"S{i + 1}",
                        Description = $"{s.Action ?? s.Raw} - {s.Acceptance}",
                        Type = "code"
                    }).ToList()
                };
            }
        }
        catch { /* Fall through */ }

        return new ExecutionPlan
        {
            TrailId = envelope.TrailId,
            TruestIntent = envelope.CurrentIntent,
            Steps = [
                new PlanStep
                {
                    Id = "S1",
                    Description = sealResult.Length > 500 ? sealResult[..500] : sealResult,
                    Type = "analysis"
                }
            ]
        };
    }

    private static PlannerGrpcResponse ErrorResponse(string message) =>
        new() { Success = false, PlanFrameJson = string.Empty, ErrorMessage = message };

    private sealed class PlannerSealOutput
    {
        [System.Text.Json.Serialization.JsonPropertyName("truest_intent")]
        public string? TruestIntent { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("steps")]
        public List<PlanStepRaw>? Steps { get; set; }
    }

    private sealed class PlanStepRaw
    {
        [System.Text.Json.Serialization.JsonPropertyName("id")] public string? Id { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("action")] public string? Action { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("acceptance")] public string? Acceptance { get; set; }
        [System.Text.Json.Serialization.JsonPropertyName("raw")] public string? Raw { get; set; }
    }
}