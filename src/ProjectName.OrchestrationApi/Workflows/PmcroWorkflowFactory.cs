using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.DependencyInjection;

namespace ProjectName.OrchestrationApi.Workflows;

/// <summary>
/// I AM the PmcroWorkflowFactory.
/// I construct a MAF Workflow for a single PMCRO cycle:
/// Planner → Maker → Dispatch → Checker → Reflector.
/// </summary>
public sealed class PmcroWorkflowFactory(IServiceProvider sp)
{
    public Workflow Create()
    {
        var planner = sp.GetRequiredService<PlannerExecutor>();
        var maker = sp.GetRequiredService<MakerExecutor>();
        var dispatch = sp.GetRequiredService<DispatchExecutor>();
        var checker = sp.GetRequiredService<CheckerExecutor>();
        var reflector = sp.GetRequiredService<ReflectorExecutor>();

        // WorkflowBuilder is non-generic. First arg to ctor is the start executor.
        // AddEdge defines the directed sequence. Build() returns a plain Workflow.
        return new WorkflowBuilder(planner)
            .AddEdge(planner, maker)
            .AddEdge(maker, dispatch)
            .AddEdge(dispatch, checker)
            .AddEdge(checker, reflector)
            .Build();
    }
}