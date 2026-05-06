---
title: "Agent Registration"
---

# Agent Registration

*Author: Shawn Bellazan · ThoughtLock: 2026-05-05 · Law Anchor: SKILL-001, ARCH-MCP-NAT-001*

---

## How Phase Agents Are Wired

Each phase agent in the PMCRO loop — Planner, Maker, Checker, Reflector — is registered as a keyed singleton `AIAgent` in the .NET dependency injection container. The key is the phase name: `"planner"`, `"maker"`, `"checker"`, `"reflector"`.

The Executor classes in `PmcroWorkflow.cs` receive their agent via `[FromKeyedServices("planner")]` on the constructor parameter. This is standard .NET keyed DI — each Executor gets exactly the agent it needs, with no ambiguity.

```csharp
// PlannerExecutor constructor (from PmcroWorkflow.cs)
public sealed class PlannerExecutor(
    [FromKeyedServices("planner")] AIAgent agent,
    ILogger<PlannerExecutor> logger)
    : Executor<CycleState, CycleState>("PlannerExecutor")
```

---

## The AddMAFAgent Pattern

In `Program.cs`, each agent is registered using the `AddMAFAgent<TSkill>()` extension method from `AIExtensions.cs`. This method:

1. Resolves the skill instance from the DI container
2. Calls `skill.GetInstructions()` — enforced at compile time by `IHasInstructions`
3. Builds `ChatClientAgentOptions` with the skill instructions and the appropriate MCP tools
4. Registers a keyed `AIAgent` singleton

The critical distinction is which tool set each agent receives. The Planner and Checker receive `AllType2Tools` — read-only tools from all three actuators. The Maker receives `AllTools` — the full set including TYPE 1 tools, because it needs to know their signatures to produce valid `McpDispatchDecision` entries. The Reflector receives no tools at all — it operates purely on the data passed to it.

```csharp
// From Program.cs — Planner gets TYPE 2 tools only
builder.Services.AddKeyedSingleton<AIAgent>("planner", (sp, _) =>
{
    var registry = sp.GetRequiredService<McpClientRegistryHolder>();
    var skill = sp.GetRequiredService<PlannerSkill>();
    var chatClient = sp.GetRequiredService<IChatClient>();

    var tools = registry.Registry?.AllType2Tools ?? [];
    var options = new ChatClientAgentOptions(chatClient)
    {
        Instructions = skill.GetInstructions(),
        Tools = [.. tools]
    };
    return new ChatClientAgent(options);
});
```

---

## The IHasInstructions Contract

Every `AgentClassSkill<T>` registered via `AddMAFAgent<TSkill>()` must implement `IHasInstructions`. This interface has one method: `string GetInstructions()`.

The reason this interface exists is to move a runtime crash to a compile-time error. Before this interface was introduced, `AddMAFAgent<T>()` called `(skill as dynamic).GetInstructions()`. That compiled fine but threw a `RuntimeBinderException` at startup if any skill forgot to implement the method. `OrchestratorSkill` was the first casualty.

With `IHasInstructions`, the method is required by the compiler. A missing `GetInstructions()` implementation is a build error, not a 3am production crash.

```csharp
// ServiceDefaults/IHasInstructions.cs
public interface IHasInstructions
{
    string GetInstructions();
}

// Skills/PlannerSkill.cs — implements the contract
public sealed class PlannerSkill : AgentClassSkill<PlannerSkill>, IHasInstructions
{
    public string GetInstructions() => Instructions;
    protected override string Instructions => "...";
}
```

---

## AgentSkillResource — Injecting Context into Agents

MAF's `[AgentSkillResource]` attribute marks a property on an `AgentClassSkill<T>` as a named resource that gets injected into the agent's context at runtime. Resources are not part of the system prompt — they are delivered to the model as structured context it can reference by name.

`OrchestratorSkill` uses this heavily. The earned laws, the MCP tool catalogue, the phase agent name blocklist, the O-Mode reference, and the escalation protocol are all `[AgentSkillResource]` properties. The model can ask for `earned-laws` or `mcp-tool-catalogue` by name and receive the current values.

```csharp
[AgentSkillResource("earned-laws")]
[Description("Earned constraints crystallised from live trail fractures.")]
public static string EarnedLaws => """
    EC-001 — I ALWAYS treat discovery verbs as economically justified.
    EC-002 — I NEVER put TYPE 2 reads in dispatch_decisions.
    EC-003 — I ALWAYS skip physical verification on cognitive-only cycles.
    """;
```

This pattern means the earned law registry lives in code, is versioned with the codebase, and is automatically available to every agent that uses `OrchestratorSkill` as a context provider — without needing to stuff the laws into the system prompt.

---

## The McpClientRegistryHolder Pattern

Because `McpClientRegistry.CreateAsync()` is asynchronous — it makes network calls to each MCP server at startup — it cannot be registered directly as a `IHostedService` constructor parameter. Instead, `McpClientRegistryHolder` acts as a mutable container that `McpRegistryStartupService` populates once startup completes.

```csharp
// McpClientRegistryHolder — a thread-safe container
public sealed class McpClientRegistryHolder
{
    public McpClientRegistry? Registry { get; set; }
}

// McpRegistryStartupService — IHostedService that populates the holder
protected override async Task ExecuteAsync(CancellationToken stoppingToken)
{
    var registry = await McpClientRegistry.CreateAsync(configuration, logger, stoppingToken);
    holder.Registry = registry;
}
```

Agent registrations resolve `McpClientRegistryHolder` and access `holder.Registry?.AllType2Tools`. If the registry is not yet populated when an agent is first used, the agent falls back to an empty tool list. In practice, the hosted service completes before any HTTP requests arrive, so this fallback is defensive rather than operational.
