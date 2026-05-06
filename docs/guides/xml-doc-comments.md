---
title: "XML Doc Comment Standard"
---

# XML Doc Comment Standard

*Author: Shawn Bellazan · ThoughtLock: 2026-05-05 · Law Anchor: EC-005, DFX-004*

---

## The Identity Declaration Pattern

Every public type in the PMCRO substrate opens its `<summary>` with the identity declaration. This is not documentation convention. It is Level 2 identity injection — the same mechanism that `FileSystemSkillLoader` uses to give agents their self-knowledge.

A class that declares "I AM the PlannerGrpcService" enforces accountability the same way a SKILL.md does. DocFX renders it into the API reference. Developers reading the reference see not just what the type does, but what it is.

---

## Type-Level Pattern

```csharp
/// <summary>
/// I AM the PlannerGrpcService — the gRPC surface of the PLAN phase.
/// I NEVER allow a BLOCKED plan to proceed to the Maker.
/// I ALWAYS run the PlannerMicroWorkflow with checkpoint support.
/// </summary>
/// <remarks>
/// Author: Shawn Bellazan | ThoughtLock: 2026-05-05
/// Law Anchors: ARCH-003, ARCH-012, EC-001
/// </remarks>
public sealed class PlannerGrpcService : PlannerAgent.PlannerAgentBase
```

**Required elements:**
- `I AM the [TypeName] — [one-phrase role description].`
- At least one `I NEVER` statement (the hard boundary).
- At least one `I ALWAYS` statement (the guaranteed behavior).
- `<remarks>` with author, ThoughtLock date, and Law Anchors for all ARCH/EC laws that govern this type.

---

## Method-Level Pattern

```csharp
/// <summary>
/// Receives an <see cref="IntentEnvelope"/> and runs the PlannerMicroWorkflow.
/// Returns an <see cref="ExecutionPlan"/> sealed with the phase's self-assessment.
/// </summary>
/// <param name="envelope">
/// The sealed IntentEnvelope. Must have <c>federation_shielded: true</c>.
/// </param>
/// <param name="ct">Propagates cancellation to the MAF checkpoint layer.</param>
/// <returns>
/// The completed <see cref="ExecutionPlan"/> with truest intent, steps,
/// and the TYPE 2 resource readings embedded.
/// </returns>
/// <exception cref="InvalidOperationException">
/// Thrown when <c>federation_shielded</c> is <c>false</c> — raw intent is rejected.
/// </exception>
/// <remarks>
/// Law Anchors: ARCH-003, ARCH-012, EC-001, EC-002
/// </remarks>
public async Task<ExecutionPlan> PlanAsync(IntentEnvelope envelope, CancellationToken ct = default)
```

**Required elements for non-trivial methods:**
- `<param>` for every parameter, with constraint context where applicable.
- `<returns>` describing what the return type contains, not just its type name.
- `<exception>` for every exception the method can throw.
- `<remarks>` with Law Anchors for every EC or ARCH law this method enforces.

---

## Interface Pattern

```csharp
/// <summary>
/// I AM the IMcpToolExecutor — the sole interface through which TYPE 1 MCP tools
/// are executed in the PMCRO substrate.
/// I NEVER expose methods that phase agents can call — only the Orchestrator wires me.
/// </summary>
/// <remarks>
/// Law Anchor: ARCH-NEW-001
/// This interface is not registered in phase agent DI containers.
/// It is registered only in OrchestratorService.
/// </remarks>
public interface IMcpToolExecutor
```

---

## What Triggers a DFX-004 Violation

A `DFX-004` violation occurs when `docfx metadata` emits a warning for a public member without `<summary>`. The build does not fail on warnings by default — but the CL-007 acceptance criterion treats warning-free as the standard.

Run:

```bash
docfx metadata docfx.json 2>&1 | grep "missing"
```

Any line containing "missing" with a type name is a DFX-004 violation. Add the identity declaration pattern to that type before committing.

---

## What Must Not Appear in XML Comments

| Pattern | Why |
|---|---|
| `TODO` or `TBD` | DFX-006: no placeholder content in committed docs |
| `/// See documentation` | Unhelpful — state what the member does |
| `/// Gets or sets X` | Auto-generated filler — state the constraint |
| Second-person address | The comments are written as the type, not about the type |

---

## See Also

- [DocFX Build (CL-007)](docfx-build.md) — how the comments are validated
- [Architecture → Earned Laws Registry](../architecture/laws.md) — EC-005 (documentation law)
