using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
using ProjectName.Core.Models;
using ProjectName.OrchestrationApi.Mcp;
using System.Text.Json;

namespace ProjectName.OrchestrationApi.Workflows;

public sealed class PlannerExecutor(
    [FromKeyedServices("planner")] AIAgent plannerAgent,
    ILogger<PlannerExecutor> logger) : Executor<CycleState, CycleState>("PlannerExecutor")
{
    public override async ValueTask<CycleState> HandleAsync(CycleState state, IWorkflowContext context, CancellationToken ct)
    {
        logger.LogInformation("[Planner] Starting native loop for {Id}", state.Envelope.TrailId);

        // Correct Options Type and Property Path
        var runOptions = new ChatClientAgentRunOptions
        {
            ChatOptions = new ChatOptions { ResponseFormat = ChatResponseFormat.ForJsonSchema<PlannerResponse>() }
        };

        // Correct Argument Order: message, session (null), options
        var response = await plannerAgent.RunAsync(JsonSerializer.Serialize(state.Envelope), null, runOptions, ct);

        state.Plan = JsonSerializer.Deserialize<PlannerResponse>(response.Text)?.Plan;
        return state;
    }
}

public sealed class MakerExecutor(
    [FromKeyedServices("maker")] AIAgent makerAgent,
    ILogger<MakerExecutor> logger) : Executor<CycleState, CycleState>("MakerExecutor")
{
    public override async ValueTask<CycleState> HandleAsync(CycleState state, IWorkflowContext context, CancellationToken ct)
    {
        logger.LogInformation("[Maker] Creating artifacts for {Id}", state.Envelope.TrailId);

        var runOptions = new ChatClientAgentRunOptions
        {
            ChatOptions = new ChatOptions { ResponseFormat = ChatResponseFormat.ForJsonSchema<MakerFrame>() }
        };

        var response = await makerAgent.RunAsync(JsonSerializer.Serialize(state.Plan), null, runOptions, ct);

        state.MakerFrame = JsonSerializer.Deserialize<MakerFrame>(response.Text);
        return state;
    }
}

public sealed class DispatchExecutor(
    IMcpToolExecutor toolExecutor,
    ILogger<DispatchExecutor> logger) : Executor<CycleState, CycleState>("DispatchExecutor")
{
    public override async ValueTask<CycleState> HandleAsync(CycleState state, IWorkflowContext context, CancellationToken ct)
    {
        if (state.MakerFrame?.DispatchDecisions is null or { Count: 0 }) return state;

        foreach (var decision in state.MakerFrame.DispatchDecisions)
        {
            logger.LogInformation("[Dispatcher] Executing {Tool}", decision.Tool);
            var result = await toolExecutor.ExecuteType1Async(decision.Mcp, decision.Tool, decision.Args);
            state.DispatchResults.Add(result);
        }
        return state;
    }
}

public sealed class CheckerExecutor(
    [FromKeyedServices("checker")] AIAgent checkerAgent,
    ILogger<CheckerExecutor> logger) : Executor<CycleState, CycleState>("CheckerExecutor")
{
    public override async ValueTask<CycleState> HandleAsync(CycleState state, IWorkflowContext context, CancellationToken ct)
    {
        logger.LogInformation("[Checker] Scoring Trail {Id}", state.Envelope.TrailId);

        var runOptions = new ChatClientAgentRunOptions
        {
            ChatOptions = new ChatOptions { ResponseFormat = ChatResponseFormat.ForJsonSchema<QualityFrame>() }
        };

        var response = await checkerAgent.RunAsync(JsonSerializer.Serialize(state), null, runOptions, ct);

        state.QualityFrame = JsonSerializer.Deserialize<QualityFrame>(response.Text);
        return state;
    }
}

public sealed class ReflectorExecutor(
    [FromKeyedServices("reflector")] AIAgent reflectorAgent,
    ILogger<ReflectorExecutor> logger) : Executor<CycleState, CycleState>("ReflectorExecutor")
{
    public override async ValueTask<CycleState> HandleAsync(CycleState state, IWorkflowContext context, CancellationToken ct)
    {
        logger.LogInformation("[Reflector] Crystallizing Trail {Id}", state.Envelope.TrailId);

        var runOptions = new ChatClientAgentRunOptions
        {
            ChatOptions = new ChatOptions { ResponseFormat = ChatResponseFormat.ForJsonSchema<ReflectorFrame>() }
        };

        var response = await reflectorAgent.RunAsync(JsonSerializer.Serialize(state), null, runOptions, ct);

        state.ReflectorFrame = JsonSerializer.Deserialize<ReflectorFrame>(response.Text);
        await context.YieldOutputAsync(state);
        return state;
    }
}