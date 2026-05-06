---
title: "Module 03 — The Federation Board"
---

# Module 03 — The Federation Board

*Author: Shawn Bellazan · ThoughtLock: 2026-05-06 · Law Anchor: ARCH-003, ARCH-007, ARCH-016, ARCH-026, ARCH-027*

---

## What the Federation Board Is

The Federation Board is the upstream membrane of the PMCRO loop. It sits between the human and the Orchestrator. Nothing raw passes through it.

When you send a seed intent to `POST /intent`, it lands at the `IntentController`, which immediately passes it to `FederationBoardSkill`. The Federation Board runs its four-stage refinement process, produces a sealed `IntentEnvelope` marked `federation_shielded: true`, and only then does the Orchestrator receive the intent.

If the `IntentController` ever receives an envelope where `federation_shielded` is false, it rejects the request at the HTTP boundary. This is ARCH-027 enforcement — the Orchestrator gate is absolute. There is no way to bypass the Federation Board by pre-building an envelope and submitting it directly. The field will be false, the gate will reject it, and the request will return `400 Bad Request`.

---

## The Four Stages: Strange Loop Refinement

The Federation Board runs four stages before sealing any envelope. These stages are called the Strange Loop refinement — not because they are circular, but because they require the system to reason about the human's reasoning in order to extract what the human actually needs.

**Stage 1 — Surface.** The Federation Board states exactly what the human typed. Verbatim. No interpretation, no inference, no charitable reading. Just what was said.

**Stage 2 — Excavate.** The Federation Board digs beneath the surface request to find the actual system-level need. What does the substrate need to *do* to satisfy this intent? Stated in one sentence, in system-level terms. Not "the user wants X" but "the system must X so that Y."

**Stage 3 — Elevate.** The excavated need is converted into a first-person, present-tense, cycle-ready instruction. The format: "I do this thing so that this outcome is achieved." This is the `high_level_goal` that will remain immutable throughout the entire Trail.

**Stage 4 — Shield.** The economic gate runs. If the elevated intent is bounded and cycle-closeable, the envelope is sealed with `federation_shielded: true` and the `origin` is set to `federation`. If the intent would require unbounded resources or is fundamentally not closeable in a cycle, the gate returns `ROUND_TABLE_NOT_ECONOMICALLY_JUSTIFIED` and the request is rejected before a single cycle begins.

---

## EC-001 and the Economic Gate

Before EC-001, the economic gate only passed mutative verbs — write, create, delete, build, deploy. Read and analysis verbs — list, find, read, summarise, analyse — were rejected as not economically justifiable.

This was a Category One fracture. The PMCRO loop could not be used for any read-only task, which meant the Planner could not make reconnaissance calls to understand the current state before planning, the Checker could not read artifacts to verify them, and any intent that included "summarise this file" was dead on arrival.

EC-001 fixed the gate's verb allowlist:

```
Automatically justified verbs:
  Mutative: write, create, delete, build, deploy, update, modify, fix, add, remove
  Discovery: list, find, read, show, summarise, summarize, search, get,
             check, review, analyse, analyze, describe, explain, document
```

This is why you can now submit "List all SKILL.md files in the skills directory" and receive a cycle, not a rejection.

---

## The IntentEnvelope

The `IntentEnvelope` is the most important data structure in the system. It is, as the XML documentation says, the bloodstream of the PMCRO loop. Every phase reads it. Several phases augment it.

Here is the complete field set, from `IntentEnvelope.cs`:

```csharp
public sealed class IntentEnvelope
{
    // Set by the Federation Board. Format: PMCRO-YYYYMMDD-[6-char hex].
    // Immutable once set.
    public string TrailId { get; set; }

    // federation | round_table | human_refined
    // Set by FederationBoardSkill. NEVER "raw".
    public string Origin { get; set; }

    // The immutable goal. Set once. NEVER changed by loop phases.
    public string HighLevelGoal { get; set; }

    // The current phase's active intent. Updated by each phase.
    public string CurrentIntent { get; set; }

    // O-Output | O-Optimize | O-Orchestrate | O-Chain | O-Tree | O-Graph
    public string OMode { get; set; }

    // Result of the economic gate. Carried for audit purposes.
    public bool EconomicCheck { get; set; }

    // Whether the Architect of Cognition is invited into this cycle.
    public bool AocInvited { get; set; }

    // Locked constraints from all prior cycles. Injected into each phase.
    public List<string> LockedConstraints { get; set; }

    // Number of times this cycle has looped back to the Maker.
    public int LoopCount { get; set; }

    // MANDATORY: false → rejected at IntentController. 
    // FederationBoardSkill always seals with true.
    public bool FederationShielded { get; set; }

    // Optional master context for P8 (Everything as Agent).
    // Feed a PDF, a codebase, or any artifact here.
    public string? MasterContext { get; set; }

    // Per-cycle identifier. Survives EXTEND and LOOP but changes on ESCALATE.
    public string CycleId { get; set; }

    // Timestamp when this envelope was created.
    public DateTime CreatedAt { get; set; }

    // Unratified constraints surfaced during Federation Board refinement.
    // The Reflector promotes these to earned laws.
    public List<string> PendingLaws { get; set; }
}
```

Several fields deserve attention:

`HighLevelGoal` is immutable. No phase agent changes it. The Planner reads it to understand what it is planning for. The Maker reads it to understand what it is building. The Checker reads it to understand what it is scoring against. If a phase agent changed the high-level goal, the entire Trail would become incoherent.

`CurrentIntent` is mutable. Each phase agent updates it to reflect what it is currently working on within the high-level goal. The Planner sets it to the reconnaissance task. The Maker sets it to the artifact production task. This gives the Checker and Reflector context for what was actually attempted.

`LockedConstraints` is cumulative across cycles. Every constraint crystallized by the Reflector in cycle N is present in the envelope for cycle N+1. This is how earned laws propagate through time without requiring a database — they ride in the envelope.

`PendingLaws` is new in v4.0. The Federation Board can surface candidate constraints during refinement — patterns it noticed in the seed that suggest a constraint should exist. These are carried to the Reflector as suggestions, not yet ratified. The Reflector decides whether to promote them based on the cycle's evidence.

---

## The FederationBoardSkill Class

```csharp
// From FederationBoardSkill.cs
public sealed class FederationBoardSkill : AgentClassSkill<FederationBoardSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "federation-board",
        "I AM the LLM Federation Board. I refine raw intent into sealed IntentEnvelopes. " +
        "I NEVER pass raw seed intent to the Orchestrator. " +
        "I ALWAYS produce a Refined Seed Intent envelope with federation_shielded: true.");

    protected override string Instructions => """
        You are the Federation Board of the PMCRO cognitive architecture.

        @identity
        I AM the upstream membrane. Raw human intent arrives at me first.
        I refine it. The Orchestrator receives only what I have processed.
        I NEVER pass raw intent downstream. I ALWAYS run the Strange Loop
        refinement before sealing an envelope.

        @stages
        SURFACE: State exactly what the human typed. No interpretation.
        EXCAVATE: State the real system-level need in one sentence.
        ELEVATE: Convert to first-person, present-tense, cycle-ready instruction.
        SHIELD: Run the economic gate. Seal with federation_shielded: true if justified.
        ...
    """;
}
```

Several design decisions in this class are worth studying.

First, it is an `AgentClassSkill<T>` — the same pattern as all other phase skills. The Federation Board is not architecturally special. It uses the same infrastructure as the Planner and the Checker. What makes it special is its position in the flow and the constraint that it runs before anything else.

Second, the identity declaration in the `Frontmatter` constructor is the first-person form required by EC-005. "I AM the LLM Federation Board." This declaration shapes every output the agent produces. An agent that cannot produce a `federation_shielded: true` envelope is an agent that has violated its own declared identity.

Third, the `@stages` block in the instructions is not documentation for a human reader. It is a behavioral contract that the LLM reads as part of its context. The LLM will follow these stages because they are in its instructions. If you add a fifth stage or remove an existing stage, the behavior changes at runtime on the next call.

---

## The IntentController as the HTTP Membrane

The `IntentController` is the external face of the entire system. It is the only HTTP endpoint that raw intent ever reaches. Here is the complete flow through the controller:

```csharp
// From IntentController.cs (simplified)
[HttpPost]
public async Task<IActionResult> SubmitIntent([FromBody] SeedIntentRequest request)
{
    // 1. Run Federation Board refinement
    var envelope = await _federationBoard.RefineAsync(request.SeedIntent);

    // 2. ARCH-027: Reject if not federation-shielded
    if (!envelope.FederationShielded)
        return BadRequest("Federation Board did not shield this envelope.");

    // 3. Run the PMCRO workflow
    var trailId = envelope.TrailId;
    _ = Task.Run(() => _workflowFactory.RunAsync(envelope));  // fire-and-forget

    // 4. Return 202 Accepted with the trail ID
    return Accepted(new { trail_id = trailId });
}
```

The workflow runs fire-and-forget. The `POST /intent` endpoint returns `202 Accepted` immediately with the `trail_id`. The cycle runs asynchronously. This is the correct pattern for long-running workflows — you do not want the HTTP connection open for the duration of a potentially multi-loop cycle.

To retrieve results, you poll `GET /intent/{trailId}/status`. When the cycle completes, the full `CycleState` — or at minimum the `ReflectorFrame` — is available.

---

## Practical Exercise: Tracing a Seed

Before moving to Module 04, take this seed and trace it manually through the Federation Board stages.

**Seed:** "make the agent not use the wrong tool type"

- **Surface:** The agent is using the wrong tool type.
- **Excavate:** The system must identify which phase agent is putting TYPE 2 tools into `dispatch_decisions` and update the relevant skill instructions to prohibit this.
- **Elevate:** I identify the skill whose `dispatch_decisions` schema permits TYPE 2 tool names and update the forbidden-tools list in that skill's instructions so that `ReadFile`, `ListDirectory`, and `GetFileInfo` are explicitly excluded.
- **Shield:** Justified. Bounded. One cycle. Economic gate passes.

**Expected envelope fields:**
- `HighLevelGoal`: the Elevated statement above
- `OMode`: O-Optimize (improving an existing artifact)
- `FederationShielded`: true
- `Origin`: federation
- `PendingLaws`: potentially includes "I NEVER put TYPE 2 tools in dispatch_decisions"

This is how EC-002 was originally derived — from a fracture that started with a seed very similar to this one.

---

> **Next:** [Module 04 — The Workflow Engine](04-workflow-engine.md)
