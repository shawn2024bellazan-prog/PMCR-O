// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Controllers/IntentController.cs
// Identity   : The Loop's Membrane — raw intent enters here, nothing else does
// Law Anchor : ARCH-027, EC-004, EC-005
// ThoughtLock: 2026-05-06 → Unified Membrane v1.0
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.AspNetCore.Mvc;
using Microsoft.Agents.AI.Workflows;
using ProjectName.OrchestrationApi.Models;
using ProjectName.OrchestrationApi.Skills;
using ProjectName.OrchestrationApi.Workflows;
using System.Diagnostics;

namespace ProjectName.OrchestrationApi.Controllers;

/// <summary>
/// I AM the IntentController.
/// I serve as the sole membrane through which raw human intent enters the PMCRO substrate.
/// </summary>
/// <remarks>
/// <para><b>ARCH-027:</b> Enforces the mandatory Federation Shield gate.</para>
/// <para><b>EC-004:</b> Implements robust multi-property result extraction for MAF workflows.</para>
/// <para><b>EC-005:</b> Adheres to the Semantic Documentation Law.</para>
/// </remarks>
[ApiController]
[Route("intent")]
public sealed class IntentController(
    FederationBoardSkill federationBoard,
    PmcroWorkflowFactory workflowFactory,
    ILogger<IntentController> logger) : ControllerBase
{
    /// <summary>
    /// Accepts raw human intent, refines it via the Federation Board, and executes the PMCRO loop.
    /// </summary>
    /// <param name="request">The seed intent from the human user.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>A <see cref="CycleResult"/> representing the accepted or escalated outcome.</returns>
    [HttpPost("submit")]
    [ProducesResponseType(typeof(CycleResult), 200)]
    [ProducesResponseType(typeof(CycleResult), 202)]
    public async Task<IActionResult> SubmitIntentAsync([FromBody] SeedIntentRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.RawIntent))
            return BadRequest(new { error = "raw_intent is required. I NEVER accept empty intent." });

        var timer = Stopwatch.StartNew();
        logger.LogInformation("[Intent] Membrane Intake: {Raw}", request.RawIntent);

        var refined = await federationBoard.RefineAsync(request, ct);

        if (refined.EconomicPreCheck == "FLAGGED")
        {
            return BadRequest(new { reason = "ROUND_TABLE_NOT_ECONOMICALLY_JUSTIFIED", refined = refined.FirstPersonSeed });
        }

        return await RunWorkflowAsync(refined.Envelope, timer, ct);
    }

    /// <summary>
    /// Internal execution engine for the cognitive workflow.
    /// </summary>
    private async Task<IActionResult> RunWorkflowAsync(
    IntentEnvelope envelope,
    Stopwatch timer,
    CancellationToken ct)
    {
        logger.LogInformation("[Intent] Starting Trail: {TrailId} | O-Mode: {OMode}",
            envelope.TrailId, envelope.OMode);

        try
        {
            var workflow = workflowFactory.Build();
            var initialState = new CycleState { Envelope = envelope };

            // Execute the workflow as a stream
            var run = await InProcessExecution.RunStreamingAsync(workflow, initialState, cancellationToken: ct);

            CycleResult? finalResult = null;

            // Exhaust the stream to capture the terminal result
            await foreach (var @event in run.WatchStreamAsync())
            {
                // Peeking into the event to find the CycleResult
                if (@event is WorkflowOutputEvent outputEvt && outputEvt.Data is CycleResult res)
                {
                    finalResult = res;
                }
                // Robust check using dynamic in case the SDK wraps the result differently
                else if (@event.GetType().Name.Contains("WorkflowOutputEvent"))
                {
                    dynamic d = @event;
                    try { finalResult ??= d.Data as CycleResult; } catch { }
                    try { finalResult ??= d.Result as CycleResult; } catch { }
                    try { finalResult ??= d.Value as CycleResult; } catch { }
                }
            }

            timer.Stop();

            if (finalResult == null)
            {
                logger.LogError("[Intent-Error] Workflow finished but CycleResult was not captured from the stream. Trail: {Trail}", envelope.TrailId);
                return StatusCode(500, new { error = "Workflow accepted but failed to yield a result to the membrane. Check physical disk for success." });
            }

            logger.LogInformation(
                "[Intent] Trail {TrailId} complete. Outcome: {Outcome} in {Duration}ms",
                envelope.TrailId, finalResult.Outcome, timer.ElapsedMilliseconds);

            return finalResult.Outcome == "ACCEPTED" ? Ok(finalResult) : Accepted(finalResult);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "[Intent-Error] Fatal failure in Trail {TrailId}", envelope.TrailId);
            return StatusCode(500, new { error = ex.Message });
        }
    }
}