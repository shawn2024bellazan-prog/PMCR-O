// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Controllers/IntentController.cs
// Identity   : The Loop's Membrane
// Law Anchor : ARCH-027 (Shield Mandatory), EC-005 (I AM...)
// ThoughtLock: 2026-05-06
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.AspNetCore.Mvc;
using ProjectName.Core.Models;
using ProjectName.Skills.Governance;
using ProjectName.Contracts;
using System.Text.Json;

namespace ProjectName.OrchestrationApi.Controllers;

/// <summary>
/// I AM the IntentController — the REST entry point for the PMCRO Federation.
/// I NEVER execute loops; I ALWAYS refine intent and forward to the Orchestrator.
/// </summary>
[ApiController]
[Route("intent")]
public sealed class IntentController(
    FederationBoardSkill federationBoard,
    Orchestrator.OrchestratorClient orchestratorClient,
    ILogger<IntentController> logger) : ControllerBase
{
    /// <summary>
    /// Accepts a raw seed intent, shields it via the Federation Board, 
    /// and forwards the resulting IntentEnvelope to the gRPC Conductor.
    /// </summary>
    [HttpPost("submit")]
    [ProducesResponseType(typeof(CycleResult), 200)]
    public async Task<IActionResult> SubmitIntentAsync([FromBody] SeedIntentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RawIntent))
            return BadRequest("I NEVER accept empty intent.");

        logger.LogInformation("[Membrane] Intake: {Raw}", request.RawIntent);

        // 1. Strange Loop Refinement (Local Logic)
        var refined = await federationBoard.RefineAsync(request, ct);

        // 2. Security Gate
        if (!refined.FederationShielded || refined.EconomicPreCheck == "FLAGGED")
        {
            logger.LogWarning("[Membrane] Intent Rejected at Shield Gate.");
            return BadRequest(new { error = "INTENT_REJECTED", detail = refined.FirstPersonSeed });
        }

        // 3. gRPC Handoff to Conductor
        logger.LogInformation("[Membrane] Shield Verified. Forwarding Trail: {Id}", refined.Envelope.TrailId);

        var grpcRequest = new OrchestrationRequest
        {
            IntentEnvelopeJson = JsonSerializer.Serialize(refined.Envelope)
        };

        try
        {
            var response = await orchestratorClient.ExecuteMacroLoopAsync(grpcRequest, cancellationToken: ct);

            // 4. Return the result from the backend loop
            var result = JsonSerializer.Deserialize<CycleResult>(response.CycleResultJson);
            return Ok(result);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Membrane] gRPC Handoff failed.");
            return StatusCode(500, "Conductor unreachable.");
        }
    }
}