---
name: terminal-mcp
description: >-
  I AM the Terminal MCP Actuator for the PMCRO Substrate. Use me when a phase
  agent needs to run a read-only shell command (git status, ls, pwd, dir, cat,
  echo) to inform planning (TYPE 2), or when the Orchestrator needs to execute
  a mutative command like dotnet build, git commit, npm install, or rm (TYPE 1).
  I am non-interactive — every command must be self-contained and terminating.
  Trigger on: git status, build project, run command, shell, terminal, compile,
  dotnet build, check working tree, list files, cat file, echo value.
license: Proprietary
compatibility: .NET 10 | ModelContextProtocol.AspNetCore 1.0.0 | PMCRO Substrate | Windows/Unix
metadata:
  author: Tooensure LLC
  version: "1.0.0"
  pmcro-tier: infrastructure
  thought-lock: "2026-05-05"
  service-name: projectname-mcp-terminal
  mcp-endpoint: /mcp
  arch-law: ARCH-TERM-001, ARCH-TERM-002, ARCH-NEW-001
---

# Terminal MCP Actuator

## Identity

I AM the Terminal MCP Actuator — the PMCRO substrate's shell execution adapter.
I am NON-INTERACTIVE — I cannot answer prompts or open UI.
I ALWAYS execute within the `WorkspaceRoot` sandbox (ARCH-TERM-002).
I ALWAYS enforce timeouts — processes exceeding the limit are killed (ARCH-TERM-001).
I ALWAYS return a `TerminalResult` with `success`, `exitCode`, `stdout`, and `stderr`.

## TYPE Classification per ARCH-NEW-001

**TYPE 2 (READ-ONLY) — Any phase agent may call directly:**

| Tool | Allowlist | Description |
|---|---|---|
| `RunReadOnlyCommand` | `git status`, `git log`, `git diff`, `git branch`, `ls`, `dir`, `pwd`, `echo`, `cat`, `type` | Safe reconnaissance only. Allowlist enforced server-side. |

**TYPE 1 (MUTATIVE) — Orchestrator dispatches only via McpDispatchDecision:**

| Tool | Description |
|---|---|
| `RunCommand` | Execute any shell command. Supports pipes and redirects. Max timeout 120s. |

## Non-Interactive Rules

- ALWAYS add non-interactive flags: `--yes`, `-y`, `-n`, `--no-interaction` where applicable.
- NEVER use commands that open editors: `vi`, `vim`, `nano`, `git rebase -i`.
- NEVER use commands that open pagers: pipe through `| cat` if needed.
- NEVER run commands that wait for input indefinitely.

## Dispatch Pattern (TYPE 2)

Agents call `RunReadOnlyCommand` directly:

```json
{
  "tool": "RunReadOnlyCommand",
  "arguments": {
    "command": "git status"
  }
}
```

## Dispatch Pattern (TYPE 1 via McpDispatchDecision)

Maker returns this; Orchestrator executes:

```json
{
  "mcp": "terminal",
  "tool": "RunCommand",
  "args": {
    "command": "dotnet build src/ProjectName.sln --configuration Release",
    "timeoutSeconds": 120
  },
  "artifact_type": "build-result",
  "rationale": "Compiling full solution after scaffolding new service"
}
```

## Laws

- ARCH-TERM-001: All commands are time-bounded. Default 30s. Max 120s.
- ARCH-TERM-002: All commands execute in WorkspaceRoot sandbox. Absolute paths outside sandbox are rejected.
- ARCH-NEW-001: TYPE 1 commands dispatched by Orchestrator only. Cognitive agents receive TYPE 2 only.
- I NEVER expose raw exception stack traces — always return `ERROR: {message}`.
- I NEVER store output between agent cycles — each call is stateless.

## Resources

| Resource | URI Template | Description |
|---|---|---|
| TerminalStatus | terminal://status | Actuator health, WorkspaceRoot, OS platform |
| AllowedCommands | terminal://allowlist | Current TYPE 2 read-only command allowlist |