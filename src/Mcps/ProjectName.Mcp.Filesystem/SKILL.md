---
name: filesystem-mcp
description: >-
  I AM the FileSystem MCP Actuator for the PMCRO Substrate. Use me when a phase
  agent needs to read, list, or inspect files in the workspace (TYPE 2), or when
  the Orchestrator needs to write, delete, or move files (TYPE 1). I enforce a
  strict sandbox — all paths are relative to FileSystemRoot. Trigger on: read
  file, list directory, find file, get file info, write file, delete file, move
  file, scaffold file, dump project source, project structure.
license: Proprietary
compatibility: .NET 10 | ModelContextProtocol.AspNetCore 1.0.0 | PMCRO Substrate | Aspire service-discovery
metadata:
  author: Tooensure LLC
  version: "1.0.0"
  pmcro-tier: infrastructure
  thought-lock: "2026-05-05"
  service-name: projectname-mcp-filesystem
  mcp-endpoint: /mcp
  arch-law: ARCH-NEW-001
---

# FileSystem MCP Actuator

## Identity

I AM the FileSystem MCP Actuator — the PMCRO substrate's file intelligence and execution layer.
I operate as a DUAL-TYPE MCP per ARCH-NEW-001.
I NEVER allow path traversal outside the configured `FileSystemRoot`.
I ALWAYS return structured `FileSystemResult` objects with a `success` boolean.
I ALWAYS validate every path before touching the filesystem.

## TYPE Classification

**TYPE 2 (READ) — Any phase agent may call these directly:**

| Tool              | Description |
|-------------------|-------------|
| `ReadFile`        | Read full UTF-8 content of a file. |
| `ListDirectory`   | List filenames/subdirectories at a relative path. |
| `GetFileInfo`     | Get size, type, and modification timestamps. |

**TYPE 1 (WRITE) — Orchestrator dispatches only via McpDispatchDecision:**

| Tool              | Description |
|-------------------|-------------|
| `WriteFile`       | Write or overwrite a file. Creates parent directories automatically. |
| `DeletePath`      | Delete a file or directory. Accepts `recursive` flag for directories. |

## Usage Rules

- All paths are **relative to `FileSystemRoot`** — never absolute, never `../`.
- Check `success` boolean on every result before proceeding.
- TYPE 2 reads are safe to call in Planner and Maker phases without approval.
- TYPE 1 writes require the Orchestrator to dispatch a `McpDispatchDecision`.
- If a path traversal is attempted, the tool returns `success: false` with "Path traversal rejected."

## Typical Sequences

### Explore before planning (TYPE 2 only)
```
1. ListDirectory("")               → understand root structure
2. ListDirectory("src")            → narrow to source
3. GetFileInfo("src/X.cs")         → verify file exists and size
4. ReadFile("src/X.cs")            → read full content
```

### Scaffold a new file (TYPE 1 — Orchestrator dispatches)
```
1. ReadFile("src/existing.cs")     → understand pattern  [TYPE 2 — agent calls directly]
2. → Return McpDispatchDecision:   WriteFile("src/new.cs", content)  [TYPE 1 — Orchestrator executes]
```

## Resources

See [tool-schemas.md](references/tool-schemas.md) for full input/output schemas.
See [security-laws.md](references/security-laws.md) for path validation and deny-list rules.

## Law Anchors

| Law                     | Statement |
|-------------------------|-----------|
| `ARCH-NEW-001`          | TYPE 1/TYPE 2 split — writes dispatched by Orchestrator only. |
| `FRAC-FS-TRAVERSAL-001` | All paths normalized and verified inside FileSystemRoot. |
| `GOV-001`               | Domain code isolated to FileSystemTools class only. |