// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATOR SERVICE
// File       : Services/OrchestratorGrpcService.cs
// Identity   : I AM the OrchestratorGrpcService — the Conductor.
// Law Anchor : ARCH-NEW-001 (TYPE 1 Dispatch Gate)
// ThoughtLock: 2026-05-06
//
// FIX (Bug 4 — FRAC-ORCHESTRATOR-DISPATCH-001):
//   ExecuteType1Async previously called httpClientFactory.CreateClient() (unnamed,
//   no BaseAddress) and posted to the hardcoded URIs:
//     "http://mcp-filesystem/mcp"
//     "http://mcp-terminal/mcp"
//     "http://mcp-playwright/mcp"
//   Although the OrchestratorService already has WithReference(mcpFilesystem/
//   mcpTerminal/mcpPlaywright) in AppHost.cs, the ResolvingHttpDelegatingHandler
//   requires those env vars to be present AND the client must be named to carry
//   the handler correctly in some Aspire configurations.
//
//   Resolution:
//     Register named HttpClients ("mcp-filesystem", "mcp-terminal", "mcp-playwright")
//     in OrchestratorService/Program.cs (not shown here) with BaseAddresses read
//     from the Aspire config keys. ExecuteType1Async now resolves the correct named
//     client per actuator and uses the relative path "/mcp".
//
// FIX (Bug 3 — FRAC-ORCHESTRATOR-JSON-001):
//   The Orchestrator crashed with:
//     JsonException: 'G' is an invalid start of a value. Path: $ | LineNumber: 0
//   at line 65 (JsonSerializer.Deserialize<QualityFrame>) because the Checker
//   returned plain English prose ("GOOD — artifacts look correct...") instead
//   of a JSON QualityFrame. This was a downstream consequence of the Checker's
//   mcp-filesystem connection failure.
//
//   Resolution:
//     Add a TryParsePhaseJson<T> helper that catches JsonException on every phase
//     deserialization, logs the raw string, and throws an InvalidOperationException
//     with an actionable message. The loop failure catch already handles this and
//     returns a CycleResult.Escalated, so the Orchestrator never hard-crashes.
//     Once the Checker fix (FRAC-CHECKER-JSON-001) is live, this guard becomes a
//     last-resort safety net rather than a frequent code path.
//
// FIX (FRAC-ORCHESTRATOR-SSE-001):
//   ExecuteType1Async stored the raw HTTP response body directly as McpToolResult.Content.
//   With Stateless = true on MCP servers, the response body is SSE format:
//     event: message
//     data: {"result":{"content":[...]}, ...}
//
//   This raw SSE string was forwarded as-is to the Checker via CycleState.DispatchResults,
//   which embedded it verbatim in the LLM prompt. The Checker LLM had to interpret SSE
//   envelopes, degrading evidence quality.
//
//   Resolution:
//     Added UnwrapSse() — same approach as CheckerGrpcService. The McpToolResult.Content
//     field now contains the unwrapped JSON-RPC response payload, not the SSE envelope.
//     McpToolResult.Error also unwraps for consistency (error responses are also SSE).
//     The success check now runs on the unwrapped JSON to avoid false negatives from
//     the isError field being buried inside the SSE data payload.
// ═══════════════════════════════════════════════════════════════════════════════

using Grpc.Core;
using ProjectName.Contracts;
using ProjectName.Core.Models;
using System.Diagnostics;
using System.Text.Json;

namespace ProjectName.OrchestratorService.Services;

/// <summary>
/// I AM the OrchestratorGrpcService — the Conductor of the PMCRO loop.
/// I ALWAYS coordinate the flow between cognitive lobes via gRPC.
/// I ALWAYS enforce the TYPE 1 Dispatch Gate.
/// </summary>
public sealed class OrchestratorGrpcService(
    Planner.PlannerClient planner,
    Maker.MakerClient maker,
    Checker.CheckerClient checker,
    Reflector.ReflectorClient reflector,
    IHttpClientFactory httpClientFactory,
    ILogger<OrchestratorGrpcService> logger) : Orchestrator.OrchestratorBase
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    public override async Task<OrchestrationResponse> ExecuteMacroLoop(OrchestrationRequest request, ServerCallContext context)
    {
        // FIX: Wrap the initial envelope deserialise in TryParsePhaseJson so that a
        // malformed IntentEnvelope produces an actionable error rather than a raw
        // JsonException with no context.
        var envelope = TryParsePhaseJson<IntentEnvelope>(
            request.IntentEnvelopeJson,
            "IntentEnvelope",
            "OrchestrationRequest.IntentEnvelopeJson")
            ?? throw new InvalidOperationException("IntentEnvelope deserialized as null.");

        var state = new CycleState { Envelope = envelope };

        logger.LogInformation("[Conductor] Starting Trail {Id}", envelope.TrailId);

        try
        {
            // 1. PLAN
            var pReq = new PlannerRequest { TrailId = envelope.TrailId, CycleStateJson = JsonSerializer.Serialize(state) };
            var pRes = await planner.ExecutePlanAsync(pReq);
            state.Plan = TryParsePhaseJson<ExecutionPlan>(pRes.PlanFrameJson, "ExecutionPlan", "PlannerResponse.PlanFrameJson");

            // 2. MAKE
            var mReq = new MakerRequest { TrailId = envelope.TrailId, CycleStateJson = JsonSerializer.Serialize(state) };
            var mRes = await maker.ExecuteMakeAsync(mReq);
            state.MakerFrame = TryParsePhaseJson<MakerFrame>(mRes.MakerFrameJson, "MakerFrame", "MakerResponse.MakerFrameJson");

            // 2.5 DISPATCH GATE (TYPE 1 EXECUTION — ARCH-NEW-001)
            if (state.MakerFrame?.DispatchDecisions is { Count: > 0 })
            {
                logger.LogInformation("[Conductor] Executing {Count} TYPE 1 Dispatch Decisions", state.MakerFrame.DispatchDecisions.Count);
                foreach (var decision in state.MakerFrame.DispatchDecisions)
                {
                    var result = await ExecuteType1Async(decision, context.CancellationToken);
                    state.DispatchResults.Add(result);
                }
            }

            // 3. CHECK
            var cReq = new CheckerRequest { TrailId = envelope.TrailId, CycleStateJson = JsonSerializer.Serialize(state) };
            var cRes = await checker.ExecuteCheckAsync(cReq);
            state.QualityFrame = TryParsePhaseJson<QualityFrame>(cRes.QualityFrameJson, "QualityFrame", "CheckerResponse.QualityFrameJson");

            // 4. REFLECT
            var rReq = new ReflectorRequest { TrailId = envelope.TrailId, CycleStateJson = JsonSerializer.Serialize(state) };
            var rRes = await reflector.ExecuteReflectAsync(rReq);
            state.ReflectorFrame = TryParsePhaseJson<ReflectorFrame>(rRes.ReflectorFrameJson, "ReflectorFrame", "ReflectorResponse.ReflectorFrameJson");

            return new OrchestrationResponse
            {
                CycleResultJson = JsonSerializer.Serialize(CycleResult.Accepted(state))
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Conductor] Loop failed for Trail {Id}", envelope.TrailId);
            return new OrchestrationResponse
            {
                CycleResultJson = JsonSerializer.Serialize(CycleResult.Escalated(envelope.TrailId, ex.Message))
            };
        }
    }

    /// <summary>
    /// Deserialises <paramref name="json"/> as <typeparamref name="T"/>.
    /// On failure, logs the raw string (first 500 chars) and rethrows as an
    /// <see cref="InvalidOperationException"/> with a clear actionable message
    /// so the outer catch can log it against the correct Trail ID.
    /// </summary>
    private T? TryParsePhaseJson<T>(string json, string typeName, string fieldName)
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (JsonException ex)
        {
            var preview = json.Length > 500 ? json[..500] : json;
            logger.LogError(
                ex,
                "[Conductor] Failed to deserialise {Type} from {Field}. " +
                "Raw value (first 500 chars): {Raw}",
                typeName, fieldName, preview);

            throw new InvalidOperationException(
                $"Phase response for {typeName} (field: {fieldName}) was not valid JSON. " +
                $"First 100 chars: '{(json.Length > 100 ? json[..100] : json)}'. " +
                $"See logs for full raw value. Inner: {ex.Message}", ex);
        }
    }

    private async Task<McpToolResult> ExecuteType1Async(McpDispatchDecision decision, CancellationToken ct)
    {
        var sw = Stopwatch.StartNew();

        // FIX: Resolve the named HttpClient for each actuator. Each client has its
        // BaseAddress set from the Aspire config key in OrchestratorService/Program.cs,
        // bypassing hostname-based DNS resolution entirely.
        // Named clients registered: "mcp-filesystem", "mcp-terminal", "mcp-playwright"
        string? clientName = decision.Mcp switch
        {
            McpActuator.filesystem => "mcp-filesystem",
            McpActuator.terminal => "mcp-terminal",
            McpActuator.playwright => "mcp-playwright",
            _ => null
        };

        if (clientName is null)
        {
            return new McpToolResult
            {
                Success = false,
                Error = $"Unknown Actuator: {decision.Mcp}",
                Mcp = decision.Mcp.ToString(),
                Tool = decision.Tool
            };
        }

        var payload = new
        {
            jsonrpc = "2.0",
            id = Guid.NewGuid().ToString(),
            method = "tools/call",
            @params = new { name = decision.Tool, arguments = decision.Args }
        };

        try
        {
            // FIX: Use named client (BaseAddress already set) + relative path "/mcp"
            var client = httpClientFactory.CreateClient(clientName);
            var response = await client.PostAsJsonAsync("/mcp", payload, cancellationToken: ct);
            var raw = await response.Content.ReadAsStringAsync(ct);
            sw.Stop();

            // FIX (FRAC-ORCHESTRATOR-SSE-001): Streamable HTTP (Stateless = true) returns
            // SSE format. Unwrap to plain JSON before storing in McpToolResult so downstream
            // consumers (Checker LLM prompt, logs) receive clean JSON-RPC payloads.
            var json = UnwrapSse(raw);

            bool success = response.IsSuccessStatusCode && !json.Contains("\"isError\":true");

            return new McpToolResult
            {
                Mcp = decision.Mcp.ToString(),
                Tool = decision.Tool,
                Success = success,
                Content = success ? json : null,
                Error = success ? null : json,
                Duration = sw.Elapsed
            };
        }
        catch (Exception ex)
        {
            sw.Stop();
            return new McpToolResult
            {
                Mcp = decision.Mcp.ToString(),
                Tool = decision.Tool,
                Success = false,
                Error = ex.Message,
                Duration = sw.Elapsed
            };
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
}