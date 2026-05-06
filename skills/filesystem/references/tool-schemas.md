# FileSystem MCP — Tool Schemas

Full input/output schemas for all tools. Load this file when constructing a specific tool call.

---

## `ReadFile` (TYPE 2)

**Input**
```json
{
  "relativePath": "src/ProjectName.Core/Models/TrailFrame.cs"
}
```

**Output** — `FileSystemResult`
```json
{
  "success": true,
  "content": "<full UTF-8 file content>",
  "path": "src/ProjectName.Core/Models/TrailFrame.cs",
  "error": ""
}
```

---

## `ListDirectory` (TYPE 2)

**Input**
```json
{
  "relativePath": "src"
}
```
Omit `relativePath` or pass `""` for the root.

**Output** — `FileSystemResult`
```json
{
  "success": true,
  "content": "ProjectName.Core\nProjectName.OrchestrationApi\nProjectName.ServiceDefaults",
  "path": "src",
  "error": ""
}
```

---

## `GetFileInfo` (TYPE 2)

**Input**
```json
{
  "relativePath": "src/ProjectName.Core/Models/TrailFrame.cs"
}
```

**Output** — `FileSystemResult`
```json
{
  "success": true,
  "content": "Type: File\nSize: 4096 bytes\nModified: 2026-05-05T08:00:00.0000000Z",
  "path": "src/ProjectName.Core/Models/TrailFrame.cs",
  "error": ""
}
```

---

## `WriteFile` (TYPE 1 — Orchestrator dispatches only)

**Input**
```json
{
  "relativePath": "src/ProjectName.Core/Models/TrailFrame.cs",
  "content": "// full file content here",
  "overwrite": true
}
```

**Output** — `FileSystemResult`
```json
{
  "success": true,
  "content": "Written: src/ProjectName.Core/Models/TrailFrame.cs",
  "path": "src/ProjectName.Core/Models/TrailFrame.cs",
  "error": ""
}
```

---

## `DeletePath` (TYPE 1 — Orchestrator dispatches only)

**Input**
```json
{
  "relativePath": "src/ProjectName.Core/Models/OldModel.cs",
  "recursive": false
}
```
Set `recursive: true` only for directories.

**Output** — `FileSystemResult`
```json
{
  "success": true,
  "content": "Deleted file: src/ProjectName.Core/Models/OldModel.cs",
  "path": "src/ProjectName.Core/Models/OldModel.cs",
  "error": ""
}
```

---

## Error Responses

All tools return the same shape on failure:
```json
{
  "success": false,
  "content": "",
  "path": "",
  "error": "Path traversal rejected."
}
```

Common error messages:
- `"Path traversal rejected."` — path attempts to escape `FileSystemRoot`
- `"File not found."` — file does not exist at the resolved path
- `"Directory not found."` — directory does not exist
- `"File exists."` — `WriteFile` with `overwrite: false` and file already exists
- `"Path not found."` — `DeletePath` on a path that doesn't exist