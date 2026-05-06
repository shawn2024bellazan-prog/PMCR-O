---
title: "The Native MAF Tool Loop"
---

# The Native MAF Tool Loop

*Author: Shawn Bellazan · ThoughtLock: 2026-05-05 · Law Anchor: ARCH-MCP-NAT-001*

---

## Why We Removed FunctionInvokingChatClient

The original substrate used `UseFunctionInvocation()` to wrap the `IChatClient` pipeline. The intention was correct — intercept tool calls from the model and execute them. The implementation was not compatible with how MAF populates tools.

Here is what was happening.

`FunctionInvokingChatClient` intercepts tool calls by checking whether `ChatOptions.Tools` is non-empty. If tools are present in the options passed to `GetResponseAsync()`, it handles the tool call, injects the result, and re-calls the model. If `Tools` is empty, it passes the call through without interception.

MAF's `ChatClientAgent` does not populate `ChatOptions.Tools` statically. It injects tools at runtime via `AIContextProviders` — specifically `AgentSkillsProvider` and the MCP tool sets — just before each model call inside `RunAsync()`. By the time `FunctionInvokingChatClient` saw the call, the tools had not yet been injected. It saw an empty `Tools` list and did nothing.

The result: the model emitted a raw `tool_calls` JSON block as its response text. The workflow received that raw JSON, tried to deserialise it as a `PlannerResponse`, failed, and crashed — or silently returned null.

The fix was to remove `UseFunctionInvocation()` entirely. MAF's `ChatClientAgent` owns the tool execution loop internally. When the model emits a `tool_calls` block inside `AIAgent.RunAsync()`, MAF handles it — calls the function, appends the result to the message history, and re-calls the model. This loop runs until the model produces a final text response. No `FunctionInvokingChatClient` wrapper needed.

---

## What Happens Inside AIAgent.RunAsync

When you call `await agent.RunAsync(message: prompt, cancellationToken: ct)`, here is the sequence.

First, MAF assembles the context. It collects the current message, the agent's system instructions from `GetInstructions()`, and all registered context providers. The `AgentSkillsProvider` adds the skill tools — file-based SKILL.md scripts and class-based skills like `PlannerSkill`. The MCP tools from `McpClientRegistry` are added as `ChatClientAgentOptions.Tools`. Everything lands in a single `ChatOptions` object.

Second, MAF calls the underlying `IChatClient.GetResponseAsync()` with that full context. The Ollama model processes the prompt and either returns a text response or emits tool call requests.

Third, if the model requests a tool call, MAF executes it. For file-based skills, that means running the PowerShell or shell script via `SubprocessScriptRunner`. For MCP tools, that means calling the `McpClient`'s `CallToolAsync()` method, which sends a JSON-RPC `tools/call` to the MCP server over the established `HttpClientTransport` connection.

Fourth, MAF appends the tool result to the message history and re-calls the model. This loop continues until the model produces a final text response with no more tool calls.

Fifth, MAF returns the final text response from `RunAsync()`. The workflow deserializes this as the phase's output frame — `PlannerResponse`, `MakerFrame`, `QualityFrame`, or `ReflectorFrame`.

---

## The OLLAMA_NUM_CTX Fix

One critical constraint governs this pattern: the model's context window.

A PMCRO Maker prompt includes the skill instructions, the execution plan as JSON, any prior dispatch results, locked constraints, and the tool catalogue. On Qwen2.5-Coder-7B, this easily exceeds 4,096 tokens — Ollama's default context length.

When the context is truncated, the tool definitions injected by `AgentSkillsProvider` are silently dropped. The model never sees `run_skill_script` in its context. It cannot call tools it does not know exist.

The fix — captured in `AppHost.cs` as `FRAC-CTX-001` — is to set `OLLAMA_NUM_CTX=8192` as an environment variable on the Ollama container. Ollama honours this as the default context length for all loaded models. At 8,192 tokens, a full PMCRO Maker prompt including tools fits comfortably.

```csharp
var ollama = builder
    .AddOllama("ollama")
    .WithGPUSupport()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("OLLAMA_NUM_CTX", "8192");  // FRAC-CTX-001
```

---

## The TYPE 1 / TYPE 2 Split in Practice

The native tool loop handles TYPE 2 operations — reads, queries, reconnaissance. The Orchestrator gate handles TYPE 1 operations — writes, mutations, executions.

In practice this means the following.

The Planner, when it calls `agent.RunAsync()`, may decide to call `ReadFile` or `ListDirectory` to explore the codebase before producing its plan. It emits a `tool_calls` block requesting `ReadFile`. MAF calls the Filesystem MCP client. The file contents come back as a tool result. The model incorporates them into its next response. The Planner produces its `PlannerResponse` with full knowledge of the files it referenced.

The Maker, when it calls `agent.RunAsync()`, may also call `ReadFile` to verify what it is about to modify. But when it wants to write a file, it does not emit a `tool_calls` block for `WriteFile`. Instead, it includes a `McpDispatchDecision` entry in its `MakerFrame` output: `{ "mcp": "filesystem", "tool": "WriteFile", "args": { "relativePath": "...", "content": "..." } }`. The `DispatchExecutor` — the Orchestrator's execution arm — picks that up after the Maker phase and calls `McpToolExecutor.ExecuteType1Async()`.

This is the gate. One place. One accountable point. TYPE 1 actions do not happen inside `agent.RunAsync()`. They happen in `DispatchExecutor`. This is `ARCH-NEW-001`.
