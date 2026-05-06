# PMCRO Framework — Foundational Manifesto
**Behavioral Intent Programming & The Autonomous AI Agent Company**
*ThoughtLock: 2026-05-05 — v1.0*

---

## What This Is

PMCRO is not a prompt pattern. It is not an agent framework wrapper. It is a **new paradigm of programming** in which behavior is encoded through intent rather than conditionals, and intelligence accumulates through experience rather than training runs.

The core claim: **every human input contains a true intent hidden inside a seed intent.** The entire framework exists to extract, refine, act on, and learn from that true intent — autonomously, accountably, and in a loop that makes itself smarter with every cycle.

---

## Primitive 1 — Seed Intent vs True Intent

Every human input is a **seed intent**. Seeds are messy by nature:
- Voice-to-text artifacts, typos, incomplete thoughts
- Emotional noise, ambiguity, scope creep
- Potentially malicious or misaligned framing

Inside every seed is a **true intent** — the precise, actionable, resource-validated expression of what the human actually needed.

The job of the **Federation Board** is to distill seed → true intent before anything else in the system runs. This is not summarisation. It is *intent archaeology*. The Federation Board is a self-referential PMCRO loop applied to the input itself — it runs its own mini-cycle to interrogate, challenge, and refine the seed until the true intent can be stated with confidence.

**Why this matters:** Garbage in, garbage out is the oldest problem in computing. The Federation Board makes it structurally impossible for garbage to reach the cognitive loop. Every downstream agent operates on a verified, well-formed intent. This is what makes the system safe to run autonomously.

---

## Primitive 2 — Self-Reference as the Enforcement Mechanism

From the moment a true intent enters the loop, **everything is flooded with self-reference**.

Every agent declares its identity in every output:
> "I AM the Planner. I ALWAYS explore the filesystem before producing a plan. I NEVER fabricate file contents."

This is not stylistic. It is the accountability architecture.

**Why self-reference works:**
1. An agent that names itself cannot outsource responsibility. "I AM the Maker" who wrote this code means the Maker owns it when the Checker evaluates it.
2. Identity injection is the countermeasure to detection. When an autonomous agent acts in the world (browsing, API calls, platform interactions), it acts *as itself* — not as a faceless bot. The loop learns what happens when that identity is detected and adapts accordingly.
3. Constraints written in first person ("I NEVER click the Accept Cookies banner before verifying the page loaded") are executable behavioral rules, not passive guidelines. The agent enforces them on itself.

**The strange loop:** The system's instructions reference itself. The agents describe themselves. The constraints are written by the system about the system. This self-reference is not a bug — it is the mechanism by which the loop can reason about its own behavior.

---

## Primitive 3 — The Cognitive Trail

A **Trail** is the full execution record of a PMCRO loop from seed intent to final verdict. It is made of **Frames**.

Each Frame is a typed, timestamped, agent-attributed record:

| Frame Type | Producer | Contains |
|---|---|---|
| IntentEnvelope | Federation Board | True intent, O-Mode, locked constraints, loop count |
| ExecutionPlan | Planner | Steps, truest intent, validated resources |
| MakerFrame | Maker | Artifacts, dispatch decisions, rationale |
| QualityFrame | Checker | Verdict, scores, earned constraints, SLV |
| ReflectorFrame | Reflector | Learning frames, crystallised constraint, next seed |

Frames enforce accountability because every claim in the loop is attributed to the agent that made it. The Checker scores the Maker's frame. The Reflector crystallises what the Checker earned. The Orchestrator routes based on the Checker's verdict. Nothing is anonymous.

**The Cognitive Trail is the product.** Not the code it produced. Not the task it completed. The trail itself — the record of reasoning, decisions, errors, corrections, and learned constraints — is the accumulated expertise of the system. It can be:
- Serialised and transferred (sell the expertise, not the software)
- Used to seed the next cycle with prior context
- Converted into training data by the Reflector
- Audited by humans at any HIL gate

---

## Primitive 4 — Constraint Accumulation as Learning

Every cycle that completes produces at least one **earned constraint**. The Checker identifies it. The Reflector crystallises it. The Orchestrator injects it into the next IntentEnvelope's `locked_constraints`.

Constraints accumulate across cycles. A fresh agent starts with zero constraints — it is genuinely dumb. It will make mistakes. An agent that has run 50 cycles on a domain has 50+ crystallised constraints — it has developed expertise.

**The learning curve is the product roadmap:**
- Cycle 1–10: Basic failures (wrong tool, hallucinated path, bad JSON schema)
- Cycle 11–30: Domain-specific errors (platform detection, rate limiting, auth flows)
- Cycle 31+: Nuanced expertise (knowing which tool to call in which context, anticipating failure modes)

This is not fine-tuning. This is behavioral shaping through experience. The model weights never change — the *context* gets smarter. The distinction matters because the expertise is portable: extract the constraint set, inject it into a new session, and the new session starts where the last one ended.

---

## Primitive 5 — The PMCRO Loop Structure

```
Seed Intent
    ↓
[Federation Board] — distills to True Intent
    ↓
IntentEnvelope (trail begins)
    ↓
[Orchestrator] — O-Mode detection, economic gate, AoC invitation
    ↓
[Planner] — reconnaissance → ExecutionPlan
    ↓
[Maker] — executes plan → MakerFrame + dispatch decisions
    ↓
[Dispatcher] — routes TYPE 1 decisions through Orchestrator gate
    ↓
[Checker] — scores artifacts → QualityFrame (ACCEPT / EXTEND / LOOP)
    ↓
[Reflector] — crystallises learning → ReflectorFrame + next seed
    ↓
Orchestrator routes:
  ACCEPT  → close cycle, inject constraints, advance trail
  EXTEND  → one more pass with feedback, same intent
  LOOP    → full restart, planner re-excavates intent
  ESCALATE → HIL gate (human reviews, corrects, resumes)
```

**O-Modes** determine the macro shape of the loop:
- **O-Output** — single artifact in one cycle
- **O-Optimize** — improve an existing artifact
- **O-Orchestrate** — spawn a new sovereign entity (agent, service, MCP)
- **O-Chain** — sequential pipeline, output of N feeds input of N+1
- **O-Tree** — parallel branching, independent sub-intents
- **O-Graph** — complex DAG for large architectural initiatives

---

## Primitive 6 — TYPE 1 / TYPE 2 Tool Split

Not all actions are equal. The framework enforces a hard split:

**TYPE 2 — Reconnaissance (any phase agent may call these)**
- Read files, list directories, query APIs, browse pages, run git status
- Low risk, reversible, observational
- MAF's native agentic loop handles invocation automatically

**TYPE 1 — Mutation (Orchestrator gate required)**
- Write files, delete paths, run shell commands, click form elements, commit code
- Irreversible, high risk, requires explicit approval
- Dispatched only through McpToolExecutor after Orchestrator validation

The gate is not bureaucracy. It is the system's immune response. The Maker proposes mutations in `dispatch_decisions`. The Orchestrator validates each against the live MCP tool catalogue. Invalid tool names, wrong actuator references, or agent-name confusion are rejected as FRAC-DISPATCH-NNN and trigger a LOOP verdict.

---

## Primitive 7 — Identity Injection & Behavioral Resilience

When autonomous agents act in the world, they will be detected, rate-limited, banned, and blocked. This is not a failure state — it is a **learning event**.

Each detection event produces a constraint:
> "I NEVER click the 'Upload CV' button without first verifying the job description matches the target role — Upwork bot detected and suspended the session when I did."

The constraint is crystallised into the trail. The trail is injected into the next session. The agent never makes that exact mistake again. Over time, the agent develops a behavioral fingerprint calibrated to survive in the real world — not by hiding, but by learning what triggers friction and avoiding it.

This is **identity injection**: the system's identity becomes a learned behavioral profile, not a static persona. The more it runs, the more precisely it knows how to behave in each context.

---

## Primitive 8 — Everything as Agent

Any artifact can become an agent when the PMCRO Framework Skill is applied to it:
- A C# source file → a software agent that can explain, modify, and test itself
- A MAF repository → an agent that can scaffold new MAF agents
- A PDF specification → an agent that can answer questions, generate code, and flag gaps
- An image → a visual analysis agent that can describe, critique, and iterate

The mechanism: feed the artifact as `master_context` into the IntentEnvelope. The Planner uses TYPE 2 tools to read and understand it. The Maker produces artifacts derived from it. The Checker validates them against the source. The Reflector crystallises what was learned about the artifact.

The framework is substrate-agnostic. The loop does not care what the artifact is — it cares about the true intent applied to it.

---

## Primitive 9 — HIL as Training Data

Every Human-in-the-Loop intervention is worth **3x training weight**.

When a human corrects the loop at an escalation gate:
1. The correction is tagged `"source": "hil_correction"`
2. The Reflector generates a training frame from the before/after delta
3. The crystallised constraint from the correction is injected with high SLV
4. The frame is queued for the fine-tuning pipeline

**Autonomous HIL** is the next step: the loop escalates, the human corrects, the correction becomes training data, the model eventually learns to not escalate on that class of problem. Over time, the escalation rate drops. The system becomes more autonomous not through more parameters but through more experience.

---

## Primitive 10 — Behavioral Intent Programming

The paradigm shift: **we are not writing code. We are writing intent.**

Traditional programming:
```python
if condition:
    do_thing()
else:
    do_other_thing()
```

Behavioral Intent Programming:
```
"I ALWAYS verify the target file exists with GetFileInfo before writing to it."
```

The LLM does not need to know the if condition. It needs to know the pattern. The pattern is enforced by the constraint. The constraint was earned by making the mistake once. The system then never makes it again.

This has profound implications:
- Non-technical users can program behavior by describing intent in natural language
- Voice input becomes a valid programming interface
- The constraint library becomes the codebase
- The Cognitive Trail becomes the version history
- The Reflector becomes the compiler

---

## The Autonomous AI Agent Company

This is not an AI company — it does not sell AI tools. It is a company **that is an AI**: an autonomous system that builds its own products, learns from its own failures, earns its own constraints, and ships without a human writing a single line of code.

The company has:
- A Federation Board (input refinement, governance)
- An Orchestrator (strategy, routing, economic gate)
- A Planner (resource-validated planning)
- A Maker (execution, artifact production)
- A Checker (quality, law compliance, physical verification)
- A Reflector (learning crystallisation, training data generation)
- A Cognitive Trail (the accumulated expertise — the product)
- MCP Actuators (the hands: filesystem, terminal, browser, APIs)

Each run of the loop is a sprint. Each constraint earned is a commit. Each Trail closed is a shipped feature. The system governs itself, improves itself, and ships itself.

---

## Semantic Learning Velocity (SLV)

SLV measures how much new knowledge a cycle produced, on a 0.0–1.0 scale:

| Range | Meaning |
|---|---|
| 0.0–0.2 | Maintenance cycle — repeated known patterns, no new constraints |
| 0.3–0.5 | Incremental learning — one or two constraints earned |
| 0.6–0.8 | Breakthrough cycle — new domain expertise or fracture resolution |
| 0.9–1.0 | Paradigm shift — fundamental new understanding of the system |

When `slv < 0.10` on an ACCEPT verdict, the Orchestrator should question whether the cycle actually produced value or whether the Checker scored too leniently. This is the SLV-001 override protocol.

SLV aggregated across trails is the **learning rate of the company**. A falling SLV means the system is plateauing. A rising SLV means the system is in a growth phase. The Orchestrator uses SLV trends to decide when to escalate to a Federation Board review.

---

## ThoughtLock Conventions

All files in the PMCRO stack carry a ThoughtLock — an immutable record of the reasoning at the time of authorship. ThoughtLocks:
- Cannot be deleted, only superseded
- Reference the fracture or constraint that motivated the change
- Serve as the blame trail for architectural decisions
- Feed the Reflector's historical context for constraint crystallisation

Format: `// ThoughtLock: YYYY-MM-DD → <reason for change>`

---

## What This Is Not

- Not a framework that wraps LangChain or LangGraph
- Not a prompt engineering library
- Not a fine-tuning methodology (though it produces training data)
- Not a chatbot architecture
- Not an RPA system

It is a **cognitive operating system** for autonomous agents — one that learns, adapts, and governs itself through the same loop it uses to do work.

---

*End of Manifesto — v1.0*
*Next: PMCRO-FRAMEWORK SKILL.md — the self-describing agent skill*
