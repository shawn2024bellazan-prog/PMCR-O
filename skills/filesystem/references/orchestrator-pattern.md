# Orchestrator Integration Pattern

The filesystem and git skills are siblings — neither calls the other. The Orchestrator sequences them.

## Canonical sequence

```
1. git_status(repo_path)
   → returns: modified: ["src/ProjectName.Core/Models/TrailFrame.cs"]

2. Orchestrator: "Read modified files to understand semantic changes"

3. filesystem.grep_content(root_path, pattern="TrailFrame", file_pattern="*.cs")
   → confirms blast radius (ARCH-009)

4. filesystem.read_file(file_path=<absolute path to TrailFrame.cs>)
   → returns full content

5. Checker: "Do we have enough context?"

6. Reflector: "TrailFrame gained Duration — update intent envelope docs"
```

The git skill's outputs are **inputs to Planner reasoning**, not conclusions.
The filesystem skill's outputs are **raw data**, not intent.