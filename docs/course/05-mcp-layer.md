---
title: "Module 05 — The MCP Layer"
---

# Module 05 — The MCP Layer

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchor: ARCH-NEW-001, ARCH-MCP-NAT-001, EC-002*

---

## What MCP Is and Why It Matters

MCP stands for Model Context Protocol. It is an open protocol for connecting AI models to external tools and data sources via a standardized server/client interface. In this substrate, MCP is the actuator layer — the mechanism by which the PMCRO loop touches the real world.

Every tool that changes or reads the file system, executes terminal commands, or controls a browser is exposed through an MCP server. The substrate runs three MCP servers as Aspire-registered processes:

- `ProjectName.Mcp.Filesystem` — file system operations
- `ProjectName.Mcp.Terminal` — terminal command execution
- `ProjectName.Mcp.Playwright` — browser automation

Each server exposes its tools via the MCP SSE (Server-Sent Events) protocol. The `McpClientRegistry` in the OrchestrationApi maintains live connections to all three servers and provides them to agents and the dispatch gate.

---

## The TYPE 1 / TYPE 2 Split in Practice

The TYPE 1 / TYPE 2 split is not a label on tools. It is an architectural guarantee built into how tools are registered, routed, and executed.

### TYPE 2 — Read-Only, Native, Any Phase

TYPE 2 tools are registered directly on phase agents via their `McpClients` option. When the Planner is registered:

```csharp
builder.Services.AddKeyedSingleton("planner", (sp, _) =>
    sp.GetRequiredService<IChatClient>()
      .AsAIAgent(
          skill: PlannerSkill.Instance,
          options: new ChatClientAgentOptions
          {
              McpClients = [
                  registry.GetClient(McpActuator.filesystem),  // ReadFile, ListDir, etc.
                  registry.GetClient(McpActuator.terminal)     // RunReadOnlyCommand
              ]
          }));
```

The filesystem client exposed to the Planner is filtered — it only exposes TYPE 2 tools. `WriteFile` and `DeletePath` are not in the tool list the Planner sees. The filtering happens at the `McpClientRegistry` level, where separate client configurations exist for read-only and read-write modes.

### TYPE 1 — Mutative, Orchestrator Gate, DispatchExecutor Only

TYPE 1 tools are never registered on phase agents. They exist only as entries in the `McpDispatchDecision` list that the Maker produces, and they are executed only by `DispatchExecutor` via `IMcpToolExecutor.ExecuteType1Async()`.

```csharp
// From McpToolExecutor.cs
public async Task<McpToolResult> ExecuteType1Async(
    McpActuator actuator,
    string tool,
    JsonObject args)
{
    var client = httpClientFactory.CreateClient(McpServiceNames[actuator]);
    // Route to the correct MCP server and execute
    // ...
}
```

The `McpServiceNames` dictionary maps `McpActuator` enum values to Aspire service names:

```csharp
private static readonly Dictionary<McpActuator, string> McpServiceNames = new()
{
    [McpActuator.filesystem] = "projectname-mcp-filesystem",
    [McpActuator.terminal]   = "projectname-mcp-terminal",
    [McpActuator.playwright] = "projectname-mcp-playwright",
};
```

---

## The Filesystem Actuator

`ProjectName.Mcp.Filesystem` exposes the substrate's workspace to the cognitive loop. It is a .NET 10 MCP SSE server registered in Aspire with a configured `WorkspaceRoot` that determines which directory it can read from and write to.

### TYPE 2 Tools (Read-Only)

| Tool | Arguments | Returns |
|------|-----------|---------|
| `ReadFile` | `relativePath: string` | File content as string |
| `ListDirectory` | `relativePath: string` | Array of file/directory names |
| `GetFileInfo` | `relativePath: string` | Size, modified date, exists flag |
| `FindFiles` | `pattern: string, directory: string` | Array of matching paths |

### TYPE 1 Tools (Mutative)

| Tool | Arguments | Effect |
|------|-----------|--------|
| `WriteFile` | `relativePath: string, content: string, overwrite: "true"/"false"` | Creates or overwrites a file |
| `DeletePath` | `relativePath: string, recursive: "true"/"false"` | Deletes file or directory |

**Critical note on `overwrite`:** The overwrite argument is validated as a string, not a JSON boolean. Always pass `"true"` or `"false"` as string values. Passing `true` (boolean) causes argument validation to fail silently. This is EC-005 alignment — a specific learned constraint from a real fracture.

### MCP Prompts and Resources

The filesystem server also exposes two MCP capabilities beyond tools:

**Prompts** — pre-built prompt templates for common filesystem tasks. The Planner can reference `FileSystemPrompts.ReadAndSummarizeFile` to get a well-structured prompt for reading and summarizing a specific file.

**Resources** — static data exposed as MCP resources. `FileSystemResources` exposes the current workspace root path and the list of paths that are excluded from read access (security configuration).

---

## The Terminal Actuator

`ProjectName.Mcp.Terminal` executes shell commands in the substrate's workspace context. It is the most powerful — and therefore most carefully gated — actuator.

### TYPE 2 Tools

| Tool | Arguments | Returns |
|------|-----------|---------|
| `RunReadOnlyCommand` | `command: string, timeoutMs: int` | stdout + stderr, exit code |

### TYPE 1 Tools

| Tool | Arguments | Effect |
|------|-----------|--------|
| `RunCommand` | `command: string, timeoutMs: int` | Executes command, returns stdout + stderr, exit code |

The distinction between `RunReadOnlyCommand` and `RunCommand` is semantic, not technical — both execute shell commands. The semantic distinction matters because it determines routing. A command like `cat file.txt` or `ls -la` is a TYPE 2 intent and belongs in the Planner's native tool calls. A command like `dotnet run` or `git commit` is a TYPE 1 intent and belongs in the Maker's dispatch decisions.

The Maker's instructions (EC-002 aligned) explicitly state:

> I ALWAYS put RunCommand on actuator "terminal", never "filesystem". RunCommand is TYPE 1 — it belongs in dispatch_decisions, not in native tool calls.

### Security Boundaries

The terminal actuator runs commands with the same privileges as the Aspire host process. In development, this is typically your user account. In production, this should be a service account with the minimum privileges needed for the substrate's tasks.

The `TerminalConfig` class exposes a `AllowedCommands` list for allowlisting. In development this is typically empty (all commands allowed). In production, this list should be populated with only the commands the substrate needs.

---

## The Playwright Actuator

`ProjectName.Mcp.Playwright` is the browser automation actuator. It runs Playwright in SSE server mode, exposing browser control as MCP tools. It is the substrate's eyes and hands on the web — enabling the behavior patterns described in P7 (Identity Injection and Behavioral Resilience).

### TYPE 2 Tools

| Tool | Arguments | Returns |
|------|-----------|---------|
| `GetPageContent` | — | Current page HTML |
| `GetPageTitle` | — | Current page `<title>` |
| `GetPageUrl` | — | Current page URL |

### TYPE 1 Tools

| Tool | Arguments | Effect |
|------|-----------|--------|
| `ClickElement` | `selector: string` | Clicks the matched element |
| `FillInput` | `selector: string, value: string` | Types into the matched input |
| `WaitForElement` | `selector: string, timeoutMs: int` | Waits until element is visible |

### The PlaywrightMicroWorkflow

The Playwright actuator includes something the other two do not: a `PlaywrightMicroWorkflow`. This is a mini PMCRO loop built into the browser actuator itself, implementing a Plan/Act/Observe/Reflect pattern for multi-step browser interactions.

```csharp
// From PlaywrightMicroWorkflow.cs
public class PlaywrightMicroWorkflow
{
    // Executors within the browser actuator's own loop
    private readonly PlanExecutor  _plan;
    private readonly ActExecutor   _act;
    private readonly ObserveExecutor _observe;
    private readonly CheckExecutor  _check;
    private readonly ReflectExecutor _reflect;
}
```

This is P8 (Everything as Agent) applied to the browser actuator. The browser actuator is not a dumb tool wrapper. It is its own cognitive loop — it plans its interaction, acts, observes the result, checks whether the action succeeded, and reflects on what it learned about the page's behavior. Constraints learned by the Playwright micro-workflow propagate up to the main PMCRO loop via the `McpToolResult` field that `DispatchExecutor` reads.

---

## McpClientRegistry — The Live Connection Pool

The `McpClientRegistry` maintains live SSE connections to all three actuators. Connections are established at startup via `McpRegistryStartupService` and kept alive for the duration of the Aspire host session.

```csharp
// From McpClientRegistry.cs
public interface IMcpClientRegistry
{
    IMcpClient GetClient(McpActuator actuator);
    IReadOnlyList<IMcpClient> GetAllClients();
    IReadOnlyList<IMcpClient> GetType2Clients();  // For phase agent registration
}
```

The `GetType2Clients()` method returns only the read-only-filtered versions of each actuator's MCP client. This is what gets injected into phase agent `McpClients` options. Phase agents never receive a client from `GetClient(McpActuator.filesystem)` directly — they receive a filtered view.

---

## Common MCP Fracture Patterns

Having operated this substrate through multiple cycles, three MCP fracture patterns recur with enough frequency to document.

**Pattern 1 — TYPE 2 tools in dispatch_decisions (EC-002).**
The Maker puts `ReadFile` or `ListDirectory` in its dispatch decisions. The `DispatchExecutor` attempts to call these as TYPE 1 tools. They fail because the dispatch executor routes through `IMcpToolExecutor`, which calls TYPE 1 endpoints. The call returns a schema validation error. The Checker scores `DispatchCorrectness` at 0.0. The cycle loops.

Diagnosis: look at `MakerFrame.DispatchDecisions` in the `CycleState`. If you see TYPE 2 tool names there, the Maker's skill instructions need to be updated with a stronger explicit FORBIDDEN list.

**Pattern 2 — Wrong actuator for a tool (EC-002).**
The Maker puts `RunCommand` on the `filesystem` actuator instead of the `terminal` actuator. The filesystem server does not expose `RunCommand`. The call fails. The Checker scores failure.

Diagnosis: look at the `Mcp` field in each `McpDispatchDecision`. If `RunCommand` has `mcp: "filesystem"`, it should be `mcp: "terminal"`.

**Pattern 3 — Context window truncation causing tool hallucination (FRAC-CTX-001).**
The Planner or Maker produces a tool call that references a tool that does not exist. The model is hallucinating tool names because its actual tool definitions were truncated from the context window by Ollama's 4,096-token limit.

Diagnosis: check `OLLAMA_NUM_CTX` in `AppHost.cs`. It should be set to at least `8192`. The symptom is a `tool_calls` block referencing tool names like `run_skill_script` that do not match any registered tool.

---

> **Next:** [Module 06 — The Cognitive Trail](06-cognitive-trail.md)
