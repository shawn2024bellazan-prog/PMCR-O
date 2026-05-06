---
title: "Module 02 — MAF to PMCRO Mapping"
---

# Module 02 — MAF to PMCRO Mapping

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchor: ARCH-MCP-NAT-001, ARCH-NEW-001, ARCH-012*

---

## Why This Mapping Matters

Microsoft Agent Framework and PMCRO were not designed together. MAF is an engineering framework — it answers the question "how do I build agents and wire them into workflows in C#?" PMCRO is a cognitive architecture — it answers the question "how do I build a system that learns, governs itself, and produces transferable expertise?"

They describe the same reality from two different angles. And because they do, there is a clean 1:1 mapping between their primitives. Once you see it, you can read MAF documentation and know immediately which PMCRO concept it corresponds to, and vice versa.

This module walks every correspondence, with the actual substrate code as the ground truth.

---

## The Master Mapping Table

| MAF Concept | PMCRO Concept | Substrate Location |
|-------------|---------------|-------------------|
| `AIAgent` | Phase Agent | `Skills/*.cs` |
| `AgentClassSkill<T>` | Agent Identity + Instructions | `Skills/*.cs` |
| `AgentSession` | Cognitive Trail (in-flight) | `CycleState` in `CycleModels.cs` |
| `AgentWorkflow` | PMCRO Loop | `PmcroWorkflow.cs` |
| `Executor` | Phase Executor | Inner classes in `PmcroWorkflow.cs` |
| `WorkflowContext<T>` | CycleState carrier | Edge parameters in `PmcroWorkflow.cs` |
| `Middleware` | Colony Laws enforcement | Injected into `ChatClientAgent` pipeline |
| `IChatClient` pipeline | Cognitive substrate | `AIExtensions.cs` in ServiceDefaults |
| `AgentSkillsProvider` | File-based skill injection | `PmcroSkillsProvider.cs` |
| `[AgentSkillResource]` | Agent resource injection | Attributes on skill properties |
| `ChatClientAgent.RunAsync()` | Native tool loop | Called in each executor |
| `ResponseFormat.ForJsonSchema<T>()` | Typed output contract | Planner, Maker, Checker, Reflector |
| `MCP SSE client` | Actuator connection | `McpClientRegistry.cs` |
| `ToolCallContent` | TYPE 2 tool invocation | MAF-internal, transparent to phases |
| `IMcpToolExecutor` | TYPE 1 gate | `McpToolExecutor.cs` — never in phase agents |

---

## AIAgent → Phase Agent

In MAF, an `AIAgent` is the fundamental unit. It wraps an `IChatClient`, accepts a message, and returns a response. The simplest possible agent:

```csharp
AIAgent agent = new AIProjectClient(new Uri(endpoint), credential)
    .AsAIAgent(model: "gpt-4o-mini", instructions: "You are a helpful assistant.");

string response = await agent.RunAsync("What is the capital of France?");
```

In PMCRO, every phase agent is an `AIAgent`. The Planner is an `AIAgent` whose instructions declare "I AM the Strategist." The Maker is an `AIAgent` whose instructions declare "I AM the MakerAgent." The Checker and Reflector follow the same pattern.

In the substrate, agents are registered in `Program.cs` using keyed dependency injection:

```csharp
// Planner agent — keyed as "planner"
builder.Services.AddKeyedSingleton("planner", (sp, _) =>
    sp.GetRequiredService<IChatClient>()
      .AsAIAgent(
          skill: PlannerSkill.Instance,
          options: new ChatClientAgentOptions
          {
              McpClients = [filesystemClient, terminalClient]  // TYPE 2 tools only
          }));
```

Notice the `McpClients` in the planner's options. This is how TYPE 2 tools become available to phase agents — they are registered as MCP clients on the agent, not on the Orchestrator gate. The Planner can call `ReadFile` directly because the filesystem MCP client is in its tool list. It cannot call `WriteFile` because the filesystem MCP client's TYPE 1 tools are filtered out at the `McpClientRegistry` level.

---

## AgentSession → Cognitive Trail

In MAF, an `AgentSession` maintains conversation state across multiple calls to the same agent. Without a session, each call is stateless. With a session, the agent remembers the conversation history.

```csharp
AgentSession session = await agent.CreateSessionAsync();
await agent.RunAsync("My name is Shawn.", session);
await agent.RunAsync("What is my name?", session);  // → "Your name is Shawn."
```

In PMCRO, the `CycleState` serves this role, but with a crucial extension. Where MAF's `AgentSession` maintains conversational history within one agent's perspective, PMCRO's `CycleState` maintains the full multi-agent Trail — every typed frame from every phase agent in the current cycle.

```csharp
// From CycleModels.cs
public sealed class CycleState
{
    public IntentEnvelope Envelope { get; set; } = new();
    public ExecutionPlan? Plan { get; set; }
    public MakerFrame? MakerFrame { get; set; }
    public List<McpToolResult> DispatchResults { get; set; } = [];
    public QualityFrame? QualityFrame { get; set; }
    public ReflectorFrame? ReflectorFrame { get; set; }
}
```

This is the Trail in its in-flight form. Every executor reads from this state and writes to it. By the time the cycle is complete, `CycleState` contains a complete, cross-phase record of everything that happened.

---

## AgentWorkflow + Executor → PMCRO Loop

This is the most important mapping. The PMCRO loop — the sequence of Planner → Maker → Dispatcher → Checker → Reflector — is a MAF `AgentWorkflow`. The phases are MAF `Executor` classes. The edges between them carry the `CycleState`.

In MAF's tutorial, a simple two-step workflow looks like this:

```csharp
class UpperCase : Executor
{
    [Handler]
    public async Task ToUpperCase(string text, WorkflowContext<string> ctx)
    {
        await ctx.SendMessageAsync(text.ToUpper());
    }
}

var workflow = new AgentWorkflowBuilder(startExecutor: new UpperCase())
    .AddEdge(upper, ReverseText)
    .Build();
```

In the substrate, the PMCRO loop is built with the same `AgentWorkflowBuilder`, but the executors are the PMCRO phases and the state type is `CycleState`:

```csharp
// From PmcroWorkflow.cs (simplified)
var planner  = new PlannerExecutor(plannerAgent, logger);
var maker    = new MakerExecutor(makerAgent, logger);
var dispatch = new DispatchExecutor(toolExecutor, registry, logger);
var checker  = new CheckerExecutor(checkerAgent, logger);
var reflector = new ReflectorExecutor(reflectorAgent, logger);

var workflow = new AgentWorkflowBuilder(startExecutor: planner)
    .AddEdge(planner,  maker)
    .AddEdge(maker,    dispatch)
    .AddEdge(dispatch, checker)
    .AddEdge(checker,  reflector)
    .Build();

CycleState result = await workflow.RunAsync(initialState);
```

The `AddEdge` calls define the routing between phases. The conditional routing — LOOP routes back to Maker, ESCALATE routes to EscalateExecutor — is handled inside each executor by reading the current state and conditionally calling `ctx.SendMessageAsync()` vs `ctx.YieldOutputAsync()`.

---

## Middleware → Colony Laws

In MAF, middleware intercepts every agent action before or after it executes. You register middleware in the `IChatClient` pipeline:

```csharp
chatClient
    .UseLogging()
    .UseFunctionInvocation()   // ← NOT used in this substrate — see ARCH-MCP-NAT-001
    .UseMyCustomMiddleware();
```

In PMCRO, middleware is the Colony Laws enforcement layer. When you need to intercept a tool call before it fires to check whether a constraint is violated, middleware is the mechanism.

**Important note on this substrate:** The original substrate used `UseFunctionInvocation()` to intercept tool calls. This was removed when MAF's native tool loop was adopted (ARCH-MCP-NAT-001). The full explanation is in [The Native MAF Tool Loop](../maf/native-tool-loop.md). The short version: `FunctionInvokingChatClient` intercepts by checking `ChatOptions.Tools` *before* MAF injects them, so it always sees an empty list and does nothing.

The current enforcement mechanism for Colony Laws is through the skill instructions and the `ConstraintInjectorExecutor`, not middleware.

---

## AgentSkillsProvider → PmcroSkillsProvider

MAF's `AgentSkillsProvider` loads skill definitions from the file system and makes them available to agents. In the substrate, `PmcroSkillsProvider` wraps this with PMCRO-specific path resolution:

```csharp
// From PmcroSkillsProvider.cs
public static AgentSkillsProvider BuildProvider(IConfiguration config)
{
    var skillsPath = GetSkillsPath(config);
    return new AgentSkillsProviderBuilder()
        .UseDirectory(skillsPath)
        .Build();
}

private static string GetSkillsPath(IConfiguration config)
{
    var projectRoot = config["Parameters:project-root"]
        ?? AppContext.BaseDirectory;
    return Path.Combine(projectRoot, SkillsSubfolder);
}
```

This is how SKILL.md files in the `skills/` directory become available to agents at runtime. When an agent is registered with `AgentSkillsProvider`, MAF loads all the SKILL.md files in the configured directory and injects them into the agent's context as resources.

---

## ResponseFormat.ForJsonSchema<T>() → Typed Output Contract

Every phase agent in the PMCRO loop is expected to return a specific typed output. The Planner returns a `PlannerResponse`. The Maker returns a `MakerFrame`. The Checker returns a `QualityFrame`. The Reflector returns a `ReflectorFrame`.

MAF enforces this through `ResponseFormat.ForJsonSchema<T>()`:

```csharp
// Inside PlannerExecutor.RunAsync()
var response = await plannerAgent.RunAsync(
    message: JsonSerializer.Serialize(envelope),
    options: new ChatClientAgentOptions
    {
        ResponseFormat = ResponseFormat.ForJsonSchema<PlannerResponse>()
    });

var plan = JsonSerializer.Deserialize<PlannerResponse>(response);
```

The `ResponseFormat.ForJsonSchema<T>()` call instructs the model to produce output that conforms to the JSON schema derived from the C# type. This is how the cognitive layer and the type system are joined — the model is constrained to produce typed, deserializable output.

If the model produces malformed JSON, the `ExecuteType1Async` call in the downstream `DispatchExecutor` will fail gracefully with a logged error rather than crashing the workflow.

---

## MCP SSE Client → Actuator Connection

MAF includes first-class MCP client support. In the substrate, each of the three actuators — filesystem, terminal, playwright — is an MCP server running as a separate Aspire-registered process. The `McpClientRegistry` holds live connections to all three:

```csharp
// From McpClientRegistry.cs (conceptual)
public sealed class McpClientRegistry : IMcpClientRegistry
{
    private readonly Dictionary<McpActuator, IMcpClient> _clients;

    public IMcpClient GetClient(McpActuator actuator)
        => _clients[actuator];
}
```

Phase agents receive the MCP clients in their `ChatClientAgentOptions`. MAF handles the SSE protocol, the tool schema discovery, the tool call serialization, and the response deserialization. From the phase agent's perspective, calling `ReadFile` is no different from calling any other function — it just happens to travel over SSE to a separate process.

---

## The Removed Concept: FunctionInvokingChatClient

This deserves explicit mention because you will encounter it in older documentation, Stack Overflow answers, and early versions of this substrate.

`FunctionInvokingChatClient` was a middleware component that intercepted tool calls from the model and dispatched them. It was the pre-MAF-native way to handle tool use. The problem: it checks `ChatOptions.Tools` to decide whether to intercept, and MAF injects tools at runtime via `AIContextProviders` — *after* `FunctionInvokingChatClient` has already checked. So it always sees an empty `Tools` list and passes everything through without interception.

The result was that tool call responses came back as raw JSON strings embedded in the model's text output. The executor tried to deserialize them as a `PlannerResponse` and failed.

The fix: remove `UseFunctionInvocation()`. MAF's `ChatClientAgent` owns the tool execution loop internally. When the model emits a `tool_calls` block inside `AIAgent.RunAsync()`, MAF handles it — calls the function, appends the result, re-calls the model. No wrapper needed.

This fracture is recorded as FRAC-007 in `skills/pmcro-framework/references/fractures.md`.

---

## The Ollama Context Window Fracture

One more mapping deserves documentation here because it affects every developer who runs this substrate locally with Ollama.

Ollama's default context window for Qwen2.5-Coder-7B is 4,096 tokens. A PMCRO Maker prompt — with the IntentEnvelope, the ExecutionPlan, the earned constraints, the MCP tool definitions, and the output schema — easily exceeds this. When the context is truncated, the tool definitions injected by `AgentSkillsProvider` are silently dropped. The model never sees `run_skill_script` in its context and hallucinates tool results.

The fix in `AppHost.cs`:

```csharp
ollama.WithEnvironment("OLLAMA_NUM_CTX", "8192");
```

This is FRAC-CTX-001. The fracture log explains why `OLLAMA_NUM_CTX=4096` produces silent tool hallucination rather than an explicit error — Ollama truncates silently, so the model simply does not know what tools are available and makes up plausible-looking calls.

---

> **Next:** [Module 03 — The Federation Board](03-federation-board.md)
