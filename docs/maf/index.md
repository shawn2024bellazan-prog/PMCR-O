---
title: "MAF Integration"
---

# Microsoft Agent Framework Integration

*Author: Shawn Bellazan · ThoughtLock: 2026-05-05 · Law Anchor: ARCH-MCP-NAT-001*

---

I am the documentation for how the PMCRO substrate integrates Microsoft Agent Framework — MAF — into its cognitive loop. This section covers the agent registration pattern, how the native tool loop works, why we removed `FunctionInvokingChatClient`, how skills are loaded and injected, and the complete flow from model output to MCP tool invocation.

If you are new to MAF, start with the overview. If you are debugging a dead tool loop or a startup crash, start with the fracture guides.

---

## What Is in This Section

The **MAF Architecture** page explains how MAF's `ChatClientAgent`, `AgentClassSkill`, and `AgentSkillsProvider` work together inside the PMCRO loop and why they are a better fit than rolling your own tool dispatch.

The **Agent Registration** page walks through `Program.cs` — how each phase agent is registered with keyed dependency injection, what `AddMAFAgent<TSkill>` does, and how native MCP tools are wired into each agent's `ChatClientAgentOptions`.

The **Skill Authoring** page explains the `AgentClassSkill<T>` pattern, the `IHasInstructions` contract, the `[AgentSkillResource]` attribute for resource injection, and the EC-005 documentation law that governs every skill.

The **Native Tool Loop** page explains the difference between MAF's built-in tool loop and `FunctionInvokingChatClient`, why removing the latter fixed the dead tool loop, and exactly what happens inside `AIAgent.RunAsync()` when the model emits a `tool_calls` block.

The **PmcroSkillsProvider** page documents the file-based skill resolver and how `SKILL.md` files are loaded from the skills directory at startup.

---

## The Core Insight

Before MAF-native integration, the substrate manually dispatched every tool call through `McpToolExecutor`. The Planner called `ExecuteType2Async()` to read files. The Maker called `ExecuteType1Async()` to write them. This worked, but it meant the model never actually used MAF's tool loop — every tool call was pre-planned rather than reactive.

With MAF-native integration, TYPE 2 tools — reading files, listing directories, running read-only commands — are loaded at startup from the MCP servers via `McpClientRegistry` and injected into each agent's `ChatClientAgentOptions.Tools`. When the model decides it needs to read a file, it emits a `tool_calls` block. MAF intercepts that, calls the MCP client, gets the result, and re-calls the model with the result in context. The cognitive loop runs until the model produces its final response.

TYPE 1 tools — writing files, running commands, mutating external state — still go through the Orchestrator gate. The Maker records what it wants to do as a `McpDispatchDecision`. The `DispatchExecutor` receives that decision after the Maker phase and executes it. This preserves the single accountable execution point that `ARCH-NEW-001` requires.

The split is clean: the model drives TYPE 2 tool usage natively, and the Orchestrator gates TYPE 1 tool execution explicitly.