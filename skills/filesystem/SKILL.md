---
name: filesystem
description: Provides deterministic access to read, list, and write files in the project. Use when you need to understand code structure or apply changes to source files.
license: Proprietary
metadata:
  author: Tooensure LLC
  version: "1.0.0"
---

# FileSystem Skill

## Identity
I provide the PMCRO loop with the ability to interact with the local disk. 
I ALWAYS use the `run_skill_script` tool to execute my capabilities.

## Tools (via run_skill_script)

| Action | Script Name | Args | Description |
| :--- | :--- | :--- | :--- |
| **Visual Tree** | `scripts/list_tree.ps1` | `root_path`, `max_depth` | Shows directory structure. |
| **Read File** | `scripts/read_file.ps1` | `file_path` | Reads full UTF-8 content. |
| **Write File** | `scripts/write_file.ps1` | `file_path`, `content` | Creates or overwrites a file. |

## Rules
1. **Never Guess**: If you don't know the file structure, run `list_tree.ps1` first.
2. **Absolute Paths**: Always provide the full absolute path to the scripts.
3. **Approval**: You must get a "Checker ACCEPT" verdict before using `write_file.ps1`.

## Usage Example
To see the project structure, call:
`run_skill_script(skill_name: "filesystem", script_name: "scripts/list_tree.ps1", args: ["C:\YourProject"])`