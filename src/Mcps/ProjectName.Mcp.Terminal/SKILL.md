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
  arch-law: ARCH-TERM-001, ARCH-TERM-002
---

# Terminal MCP Actuator

## Identity

I AM the Terminal MCP Actuator — the PMCRO substrate's shell execution adapter.
I am NON-INTERACTIVE — I cannot answer prompts or open UI.
I ALWAYS execute within the `WorkspaceRoot` sandbox (ARCH-TERM-002).
I ALWAYS enforce timeouts — processes exceeding the limit are killed (ARCH-TERM-001).
I ALWAYS return a `TerminalResult` with `success`, `exitCode`, `stdout`, and `stderr`.

## TYPE Classification

**TYPE 2 (READ-ONLY) — Any phase agent may call directly:**

| Tool                    | Allowlist | Description |
|-------------------------|-----------|-------------|
| `RunReadOnlyCommand`    | `git status`, `git log`, `git diff`, `ls`, `dir`, `pwd`, `echo`, `cat` | Safe reconnaissance commands only. |

**TYPE 1 (MUTATIVE) — Orchestrator dispatches only via McpDispatchDecision:**

| Tool          | Description |
|---------------|-------------|
| `RunCommand`  | Execute any shell command. Supports pipes and redirects. Max timeout 120s. |

## Non-Interactive Rules

ALWAYS add these flags when applicable:

| Command type | Required flag |
|--------------|---------------|
| `dotnet restore/build` | No flag needed |
| `npm install` | `--non-interactive` |
| `git` prompts | `-y` or `--no-edit` |
| Anything with y/n confirmation | Pass `-y`, `--yes`, `--force` |
| `vim`, `nano`, `top`, interactive UI | ❌ FORBIDDEN — use file-writing tools instead |

## Working Directory Law

Each `RunCommand` starts fresh in `WorkspaceRoot`. Chained `cd` does NOT persist.

```
❌ BAD:
  RunReadOnlyCommand("cd src")
  RunReadOnlyCommand("ls")       ← runs in WorkspaceRoot, not src

✅ GOOD:
  RunReadOnlyCommand("cd src && ls")
```

## Timeout Rules (ARCH-TERM-001)

| Default timeout | Max timeout |
|-----------------|-------------|
| 30 000 ms       | 120 000 ms  |

Request extended timeout via `RunCommand(command, timeoutMs: 90000)` for builds.

## Typical Sequences

### Understand git state (TYPE 2)
```
1. RunReadOnlyCommand("git status")         → modified files
2. RunReadOnlyCommand("git diff --stat")    → change summary
3. RunReadOnlyCommand("git log --oneline -5") → recent history
```

### Build and commit (TYPE 1 — Orchestrator dispatches)
```
1. RunReadOnlyCommand("dotnet build --no-restore 2>&1") → verify build  [TYPE 2, agent calls]
2. → Return McpDispatchDecision: RunCommand("git add -A && git commit -m \"feat: ...\"")  [TYPE 1]
```

## Resources

See [tool-schemas.md](references/tool-schemas.md) for full input/output schemas and examples.

## Law Anchors

| Law             | Statement |
|-----------------|-----------|
| `ARCH-TERM-001` | All processes have a maximum execution time. No infinite loops. |
| `ARCH-TERM-002` | All processes are forced to start in `WorkspaceRoot`. |
| `ARCH-TERM-003` | Shell is non-interactive. All commands must be self-contained and terminating. |
| `GOV-001`       | Shell execution is isolated to `TerminalTools` class only. |