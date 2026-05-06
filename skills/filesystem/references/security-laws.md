# FileSystem MCP — Security Laws

## FRAC-FS-TRAVERSAL-001: Path Boundary Enforcement

**Rule:** Every path passed to a tool is resolved via `Path.GetFullPath(Path.Combine(FileSystemRoot, relativePath))`.
The resolved absolute path MUST start with the `FileSystemRoot`. If it does not, the tool returns:
```json
{ "success": false, "error": "Path traversal rejected." }
```

**What this blocks:**
- `../../etc/passwd`
- `..\..\..\Windows\System32`
- Absolute paths like `C:\secrets`

**What this allows:**
- `""` or `"."` → resolves to `FileSystemRoot`
- `"src/ProjectName.Core"` → resolves to `FileSystemRoot\src\ProjectName.Core`
- `"src/X.cs"` → resolves to `FileSystemRoot\src\X.cs`

## Deny List

The following file patterns are always rejected regardless of path validity:

| Pattern | Reason |
|---------|--------|
| `*.env` | Environment secrets |
| `appsettings.Production.json` | Production connection strings |
| `*.pfx`, `*.p12` | Certificate private keys |
| Files containing `secret` in name | General secret hygiene |
| Files containing `password` in name | Credential hygiene |
| Files containing `credential` in name | Credential hygiene |

## Max Payload Limits

| Operation | Default Limit |
|-----------|--------------|
| Single file read (`ReadFile`) | 10 MB |
| Directory listing (`ListDirectory`) | No limit (returns filenames only) |

## ARCH-NEW-001: TYPE 1 / TYPE 2 Split

Phase agents (Planner, Maker) may call TYPE 2 tools directly without human approval.
TYPE 1 tools are dispatched exclusively by the Orchestrator via `McpDispatchDecision`.
The Maker NEVER calls `WriteFile` or `DeletePath` directly — it returns a `McpDispatchDecision`.