// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.PLAYWRIGHT
// File       : Executors/PlaywrightMicroWorkflow.cs
// Identity   : Browser Research Loop — MAF WorkflowBuilder Implementation
// Law Anchor : PW-LAW-003, PW-LAW-005, ARCH-013, ARCH-NEW-001
// ThoughtLock: 2026-05-05
// ───────────────────────────────────────────────────────────────────────────────

using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.Logging;
using ProjectName.Mcp.Playwright.Tools;

namespace ProjectName.Mcp.Playwright.Executors;

// ── Shared message types ──────────────────────────────────────────────────────

public sealed record BrowserResearchInput(
    string Objective,
    string StartUrl,
    int MaxSteps = 5);

public sealed record BrowserResearchState(
    string Objective,
    string CurrentUrl,
    string PageSummary,
    List<string> StepLog,
    int StepsRemaining,
    bool ResearchComplete,
    string FinalAnswer);

public sealed record BrowserResearchResult(
    string Objective,
    string Answer,
    string FinalUrl,
    List<string> StepLog,
    bool Succeeded);

// ── Executor 1 — OBSERVE ─────────────────────────────────────────────────────

public sealed class ObserveExecutor(PlaywrightTools tools, ILogger<ObserveExecutor> logger)
    : Executor<BrowserResearchState, BrowserResearchState>("ObserveExecutor")
{
    public override async ValueTask<BrowserResearchState> HandleAsync(
        BrowserResearchState state,
        IWorkflowContext context,
        CancellationToken ct)
    {
        logger.LogInformation("[PW-Workflow:Observe] Navigating to {Url}", state.CurrentUrl);

        var navResult = await tools.NavigateToAsync(state.CurrentUrl);
        var titleResult = await tools.GetPageTitleAsync();
        var contentResult = await tools.GetPageContentAsync();

        var rawHtml = contentResult.StartsWith("SUCCESS:") ? contentResult[8..] : string.Empty;
        var truncated = rawHtml.Length > 8_000 ? rawHtml[..8_000] + "\n[…truncated…]" : rawHtml;
        var title = titleResult.StartsWith("SUCCESS:") ? titleResult[8..].Trim() : "(unknown)";

        var stepEntry = $"OBSERVE: {state.CurrentUrl} → {title} ({navResult})";
        logger.LogDebug("[PW-Workflow:Observe] {Step}", stepEntry);

        return state with
        {
            PageSummary = truncated,
            StepLog = [.. state.StepLog, stepEntry],
        };
    }
}

// ── Executor 2 — PLAN ────────────────────────────────────────────────────────

public sealed record BrowserAction(
    string Kind,         // "EXTRACT" | "CLICK" | "DONE" | "GIVE_UP"
    string? Selector,    // for CLICK
    string? Answer);     // for EXTRACT/DONE

public sealed class PlanExecutor(ILogger<PlanExecutor> logger)
    : Executor<BrowserResearchState, (BrowserResearchState State, BrowserAction Action)>("PlanExecutor")
{
    public override ValueTask<(BrowserResearchState State, BrowserAction Action)> HandleAsync(
        BrowserResearchState state,
        IWorkflowContext context,
        CancellationToken ct)
    {
        if (state.StepsRemaining <= 0)
        {
            logger.LogWarning("[PW-Workflow:Plan] Max steps exhausted — GIVE_UP");
            return ValueTask.FromResult((state, new BrowserAction("GIVE_UP", null,
                $"Research objective not fully answered after exhausting steps. Partial context: {state.PageSummary[..Math.Min(500, state.PageSummary.Length)]}")));
        }

        var objectiveWords = state.Objective
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 4)
            .Take(5)
            .ToList();

        var pageText = System.Text.RegularExpressions.Regex.Replace(state.PageSummary, "<[^>]+>", " ");
        var matchCount = objectiveWords.Count(w => pageText.Contains(w, StringComparison.OrdinalIgnoreCase));

        if (matchCount >= Math.Max(1, objectiveWords.Count / 2))
        {
            var visibleText = pageText
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Where(l => l.Trim().Length > 20)
                .Take(20)
                .Select(l => l.Trim());

            logger.LogInformation("[PW-Workflow:Plan] Objective matched — EXTRACT");
            return ValueTask.FromResult((state, new BrowserAction("EXTRACT", null, string.Join(" ", visibleText))));
        }

        logger.LogInformation("[PW-Workflow:Plan] Objective not matched — DONE with partial");
        return ValueTask.FromResult((state, new BrowserAction("DONE", null,
            $"Could not fully resolve objective on {state.CurrentUrl}. Available context: {pageText[..Math.Min(600, pageText.Length)]}")));
    }
}

// ── Executor 3 — ACT ─────────────────────────────────────────────────────────

public sealed class ActExecutor(PlaywrightTools tools, ILogger<ActExecutor> logger)
    : Executor<(BrowserResearchState State, BrowserAction Action), BrowserResearchState>("ActExecutor")
{
    public override async ValueTask<BrowserResearchState> HandleAsync(
        (BrowserResearchState State, BrowserAction Action) input,
        IWorkflowContext context,
        CancellationToken ct)
    {
        var (state, action) = input;
        string stepEntry;

        switch (action.Kind)
        {
            case "CLICK" when action.Selector is not null:
                var clickResult = await tools.ClickElementAsync(action.Selector);
                var urlAfter = await tools.GetPageUrlAsync();
                var newUrl = urlAfter.StartsWith("SUCCESS:") ? urlAfter[8..].Trim() : state.CurrentUrl;
                stepEntry = $"ACT/CLICK: {action.Selector} → {clickResult}";
                logger.LogDebug("[PW-Workflow:Act] {Step}", stepEntry);
                return state with
                {
                    CurrentUrl = newUrl,
                    StepsRemaining = state.StepsRemaining - 1,
                    StepLog = [.. state.StepLog, stepEntry],
                };

            case "EXTRACT":
            case "DONE":
            case "GIVE_UP":
            default:
                stepEntry = $"ACT/{action.Kind}: answer captured ({action.Answer?.Length ?? 0} chars)";
                logger.LogDebug("[PW-Workflow:Act] {Step}", stepEntry);
                return state with
                {
                    ResearchComplete = action.Kind is "EXTRACT" or "DONE",
                    FinalAnswer = action.Answer ?? string.Empty,
                    StepsRemaining = state.StepsRemaining - 1,
                    StepLog = [.. state.StepLog, stepEntry],
                };
        }
    }
}

// ── Executor 4 — CHECK ───────────────────────────────────────────────────────

public sealed class CheckExecutor(ILogger<CheckExecutor> logger)
    : Executor<BrowserResearchState, BrowserResearchState>("CheckExecutor")
{
    public override ValueTask<BrowserResearchState> HandleAsync(
        BrowserResearchState state,
        IWorkflowContext context,
        CancellationToken ct)
    {
        if (state.ResearchComplete && state.FinalAnswer.Trim().Length < 20)
        {
            logger.LogWarning("[PW-Workflow:Check] Answer too short — forcing another iteration");
            return ValueTask.FromResult(state with { ResearchComplete = false });
        }

        if (state.StepsRemaining <= 0 && !state.ResearchComplete)
        {
            logger.LogWarning("[PW-Workflow:Check] Steps exhausted — accepting partial answer");
            return ValueTask.FromResult(state with { ResearchComplete = true });
        }

        logger.LogInformation("[PW-Workflow:Check] State valid — ResearchComplete={Done}", state.ResearchComplete);
        return ValueTask.FromResult(state);
    }
}

// ── Executor 5 — REFLECT ─────────────────────────────────────────────────────

public sealed class ReflectExecutor(ILogger<ReflectExecutor> logger)
    : Executor<BrowserResearchState, BrowserResearchResult>("ReflectExecutor")
{
    public override ValueTask<BrowserResearchResult> HandleAsync(
        BrowserResearchState state,
        IWorkflowContext context,
        CancellationToken ct)
    {
        logger.LogInformation(
            "[PW-Workflow:Reflect] ResearchComplete={Done} StepsLeft={Steps}",
            state.ResearchComplete, state.StepsRemaining);

        return ValueTask.FromResult(new BrowserResearchResult(
            Objective: state.Objective,
            Answer: state.FinalAnswer.Length > 0
                ? state.FinalAnswer
                : $"Research inconclusive after visiting {state.CurrentUrl}.",
            FinalUrl: state.CurrentUrl,
            StepLog: state.StepLog,
            Succeeded: state.FinalAnswer.Trim().Length >= 20));
    }
}

// ── Workflow factory ──────────────────────────────────────────────────────────

public static class PlaywrightMicroWorkflow
{
    public static async Task<BrowserResearchResult> RunAsync(
        BrowserResearchInput input,
        PlaywrightTools tools,
        ILoggerFactory loggerFactory,
        CancellationToken ct = default)
    {
        var observe = new ObserveExecutor(tools, loggerFactory.CreateLogger<ObserveExecutor>());
        var plan = new PlanExecutor(loggerFactory.CreateLogger<PlanExecutor>());
        var act = new ActExecutor(tools, loggerFactory.CreateLogger<ActExecutor>());
        var check = new CheckExecutor(loggerFactory.CreateLogger<CheckExecutor>());
        var reflect = new ReflectExecutor(loggerFactory.CreateLogger<ReflectExecutor>());

        WorkflowBuilder builder = new(observe);
        builder.AddEdge(observe, plan);
        builder.AddEdge(plan, act);
        builder.AddEdge(act, check);
        builder.AddEdge(check, reflect);
        builder.AddEdge(reflect, observe);

        var workflow = builder.Build();

        var initialState = new BrowserResearchState(
            Objective: input.Objective,
            CurrentUrl: input.StartUrl,
            PageSummary: string.Empty,
            StepLog: [],
            StepsRemaining: input.MaxSteps,
            ResearchComplete: false,
            FinalAnswer: string.Empty);

        var checkpointManager = CheckpointManager.CreateInMemory();
        var runId = Guid.NewGuid().ToString();

        // ──────────────────────────────────────────────────────────────────────
        // FIX: Using purely positional arguments mapped strictly to:
        // (Workflow, State, CheckpointManager, string, CancellationToken)
        // ──────────────────────────────────────────────────────────────────────
        var run = await InProcessExecution.RunStreamingAsync(
            workflow,
            initialState,
            checkpointManager,
            runId,
            ct);

        BrowserResearchResult? finalResult = null;

        await foreach (var evt in run.WatchStreamAsync())
        {
            var evtName = evt.GetType().Name;

            if (evtName.Contains("WorkflowOutputEvent"))
            {
                dynamic d = evt;
                try { finalResult ??= d.Result as BrowserResearchResult; } catch { }
                try { finalResult ??= d.Value as BrowserResearchResult; } catch { }
                try { finalResult ??= d.Output as BrowserResearchResult; } catch { }
                try { finalResult ??= d.Data as BrowserResearchResult; } catch { }
                break;
            }

            if (evtName.Contains("WorkflowErrorEvent"))
            {
                dynamic d = evt;
                string err = "Unknown";
                try { err = d.Error?.ToString() ?? err; } catch { }
                try { if (err == "Unknown") err = d.Exception?.ToString() ?? err; } catch { }

                return new BrowserResearchResult(
                    Objective: input.Objective,
                    Answer: $"Workflow error: {err}",
                    FinalUrl: input.StartUrl,
                    StepLog: [],
                    Succeeded: false);
            }
        }

        return finalResult ?? new BrowserResearchResult(
            Objective: input.Objective,
            Answer: "Workflow completed without emitting a result.",
            FinalUrl: input.StartUrl,
            StepLog: [],
            Succeeded: false);
    }
}