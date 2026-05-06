# Terminal MCP — Tool Schemas

## RunReadOnlyCommand

```json
{
  "name": "RunReadOnlyCommand",
  "description": "Execute a read-only shell command from the allowlist. Safe for cognitive agents to call directly (TYPE 2).",
  "inputSchema": {
    "type": "object",
    "properties": {
      "command": {
        "type": "string",
        "description": "One of: git status, git log, git diff, git branch, ls, dir, pwd, echo, cat, type. Pipes allowed if both sides are on the allowlist."
      },
      "workingDirectory": {
        "type": "string",
        "description": "Optional relative path within WorkspaceRoot. Defaults to WorkspaceRoot."
      }
    },
    "required": ["command"]
  },
  "returns": "TerminalResult JSON: { success, exitCode, stdout, stderr }"
}
```

## RunCommand (TYPE 1 — Orchestrator dispatch only)

```json
{
  "name": "RunCommand",
  "description": "Execute any shell command in the WorkspaceRoot sandbox. TYPE 1 — only Orchestrator may dispatch.",
  "inputSchema": {
    "type": "object",
    "properties": {
      "command": {
        "type": "string",
        "description": "Shell command to execute. Must be non-interactive and self-terminating."
      },
      "workingDirectory": {
        "type": "string",
        "description": "Optional relative path within WorkspaceRoot."
      },
      "timeoutSeconds": {
        "type": "integer",
        "description": "Max execution time in seconds. Default 30. Max 120.",
        "default": 30,
        "maximum": 120
      }
    },
    "required": ["command"]
  },
  "returns": "TerminalResult JSON: { success, exitCode, stdout, stderr }"
}
```

## TerminalResult Schema

```json
{
  "success": true,
  "exitCode": 0,
  "stdout": "string — captured standard output",
  "stderr": "string — captured standard error (may be non-empty even on success)"
}
```