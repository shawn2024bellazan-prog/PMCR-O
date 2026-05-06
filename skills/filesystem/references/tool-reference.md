# Filesystem Skill — Tool Reference

Full input/output schemas for all tools. Load this file when you need to construct a specific tool call.

---

## `list_tree`

Runs `scripts/list_tree.ps1`.

**input_schema**
```yaml
type: object
properties:
  root_path:
    type: string
    description: Absolute path to the project root.
  max_depth:
    type: integer
    description: Optional. Limit tree depth (default unlimited).
required: [root_path]
```

**output_schema**
```yaml
type: object
properties:
  root: string
  entry_count: integer
  max_depth: integer | null
  tree:
    type: array
    items: string
    description: Each entry prefixed with [DIR] or [FILE].
```

---

## `list_project`

Runs `scripts/list_project.ps1`.

**input_schema**
```yaml
type: object
properties:
  root_path: string
  sub_path:
    type: string
    description: Optional. Restrict to a subdirectory (e.g. "src").
required: [root_path]
```

**output_schema**
```yaml
type: object
properties:
  root: string
  sub_path: string | null
  file_count: integer
  files:
    type: array
    items: string
    description: Relative paths sorted alphabetically.
```

---

## `dump_project_source`

Runs `scripts/dump_project_source.ps1`.

**input_schema**
```yaml
type: object
properties:
  root_path: string
  sub_path: string      # Optional. Scope to subdirectory.
  max_file_kb:
    type: integer
    description: Skip files larger than this KB. Default 500.
required: [root_path]
```

**output_schema**
```yaml
type: object
properties:
  root: string
  sub_path: string | null
  file_count: integer
  skipped_count: integer
  files:
    type: array
    items:
      type: object
      properties:
        relative_path: string
        size_bytes: integer
        last_modified: string  # ISO 8601 UTC
        content: string
```

---

## `read_file`

Runs `scripts/read_file.ps1`.

**input_schema**
```yaml
type: object
properties:
  file_path:
    type: string
    description: Absolute path to the file.
required: [file_path]
```

**output_schema**
```yaml
type: object
properties:
  file_path: string
  file_name: string
  extension: string
  size_bytes: integer
  last_modified: string
  line_count: integer
  content: string
```

---

## `search_files`

Runs `scripts/search_files.ps1`.

**input_schema**
```yaml
type: object
properties:
  root_path: string
  pattern:
    type: string
    description: Wildcard pattern — e.g. "*.cs", "appsettings*", "SKILL.md"
  sub_path: string  # Optional. Restrict to subdirectory.
required: [root_path, pattern]
```

**output_schema**
```yaml
type: object
properties:
  root: string
  pattern: string
  sub_path: string | null
  match_count: integer
  matches:
    type: array
    items: string
    description: Relative paths of matched files.
```

---

## `grep_content`

Runs `scripts/grep_content.ps1`. **Run before any model mutation to confirm blast radius (ARCH-009).**

**input_schema**
```yaml
type: object
properties:
  root_path: string
  pattern:
    type: string
    description: String or regex to search for inside files.
  file_pattern:
    type: string
    description: Optional. Restrict to files matching wildcard (e.g. "*.cs").
  sub_path: string       # Optional.
  case_sensitive: boolean  # Default false.
  max_results:
    type: integer
    description: Cap total matches. Default 200.
required: [root_path, pattern]
```

**output_schema**
```yaml
type: object
properties:
  root: string
  pattern: string
  file_pattern: string | null
  match_count: integer
  truncated: boolean
  matches:
    type: array
    items:
      type: object
      properties:
        file: string
        line_number: integer
        line: string
```

---

## `write_file`

Runs `scripts/write_file.ps1`. **Checker ACCEPT verdict required before invocation.**

**input_schema**
```yaml
type: object
properties:
  file_path:
    type: string
    description: Absolute path. Created if it does not exist.
  content:
    type: string
    description: Full file content. UTF-8.
  overwrite:
    type: boolean
    description: If false and file exists, fail. Default true.
required: [file_path, content]
```

**output_schema**
```yaml
type: object
properties:
  file_path: string
  size_bytes: integer
  written: boolean
  overwritten: boolean
  created_dirs:
    type: array
    items: string
```