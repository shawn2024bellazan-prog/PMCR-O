---
title: "Adding an Earned Law"
---

# Adding an Earned Law

*Author: Shawn Bellazan · ThoughtLock: 2026-05-05*

---

## Prerequisites

- A fracture has occurred: a cycle failed at a specific point for a known reason.
- The fix has been applied: the code, skill, or configuration is corrected.
- The substrate is running and CL-007 passes: `docfx build` exits 0.

## What the End of This Guide Looks Like

The fracture is permanently recorded, the law is ratified, and the constraint is injected into every future cycle. The same fracture cannot recur.

---

## Steps

### 1. Record the Fracture

Open `skills/pmcro-framework/references/fractures.md`. Add an entry:

```markdown
## FRAC-NNN — [Short symptom description]
**Date:** YYYY-MM-DD
**Trail:** [trail_id]
**Service:** [service or component where fracture occurred]
**Symptom:** [What the cycle did wrong — specific, observable, not "it failed"]
**Root Cause:** [Why — the structural reason, not just the proximate error]
**Fix:** [What was changed and where]
```

The fracture entry is immutable once committed. No fracture is ever deleted. The history of fractures is the history of how the system learned.

### 2. Ratify the Law

Determine the law category:

- **New behavior pattern not covered by any existing law** → assign `EC-NNN` (next available number) and add to `laws.md`.
- **Extends an existing law** → add a note to the existing entry in `laws.md`.

Write the law in first person, present tense:

```markdown
## EC-NNN — [Law Title]
**Fracture:** FRAC-NNN
**Trail:** [trail_id]
**Symptom:** [one-sentence summary of what failed]

**Laws:**
- I ALWAYS [positive behavior].
- I NEVER [negative behavior].
```

### 3. Inject into OrchestratorSkill

Open `src/ProjectName.OrchestrationApi/Skills/OrchestratorSkill.cs`. Add the new law to the `earned-laws` resource block:

```csharp
// EC-NNN — [Law Title]
// Fracture: FRAC-NNN | Trail: [trail_id]
// [One-line summary]
I ALWAYS [behavior].
I NEVER [anti-behavior].
```

### 4. Add to PmcroWorkflow Law Header

Open `src/ProjectName.OrchestrationApi/Workflows/PmcroWorkflow.cs`. Add `EC-NNN` to the Law Anchor header comment.

### 5. Update the Architecture Doc

Open `docs/architecture/laws.md`. Add the full law entry (same content as step 2, without the markdown heading format — use the existing registry format).

### 6. Run CL-007

```powershell
.\07-docfx-build.ps1
```

All three stages must exit 0. If the DocFX build fails after adding the law:
- New `.md` file added without a `toc.yml` entry → add the entry (DFX-005).
- New public type in changed C# without `<summary>` → add the XML doc comment (EC-005, DFX-004).

---

## Verification

Confirm the law is active in the running substrate:

```bash
curl -X POST https://localhost:[port]/api/intent \
  -d '{ "seed_intent": "[an intent that would have triggered the original fracture]" }'
```

The cycle should complete without hitting the fracture. The QualityFrame should show no LOOP verdict caused by the previously unfixed behavior.

---

## Troubleshooting

**The same fracture recurs after law ratification**  
The law was added to `laws.md` but not injected into `OrchestratorSkill.cs`. Verify the `earned-laws` resource block contains the new law text. Restart the OrchestrationApi service.

**DocFX build fails after adding the law doc**  
Missing `toc.yml` entry. Add the new `.md` file to the appropriate `toc.yml` before committing.

---

## See Also

- [Architecture → Earned Laws Registry](../architecture/laws.md) — current active laws
- [Architecture → What PMCRO Is](../architecture/what-pmcro-is.md) — Primitive 4: Constraint Accumulation as Learning
