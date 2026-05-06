---
title: "ADR Log"
---

# ADR Log

*Author: Shawn Bellazan · ThoughtLock: 2026-05-05*

---

## ADR-001 — Federation Board as Mandatory Upstream Gate

**Status:** Accepted  
**Date:** 2026-05-05  
**Author:** Shawn Bellazan  
**Law Anchor:** ARCH-016, ARCH-027

### Context

Raw human intent is structurally unreliable. Voice-to-text artifacts, emotional noise, scope creep, and potentially malicious framing all arrive at the system boundary. Allowing raw intent to reach the Orchestrator means the cognitive loop runs on garbage. Every downstream phase inherits that noise.

### Decision

The Federation Board is mandatory. Raw intent never reaches the Orchestrator. Every intent passes through the Strange Loop refinement (SURFACE → EXCAVATE → ELEVATE → SHIELD) before entering the cognitive loop. The OrchestrationApi controller enforces this at the boundary: `federation_shielded: false` is rejected.

### Consequences

**Positive:** Every downstream agent operates on a verified, well-formed intent. The economic gate runs once, before the loop, not distributed across phases.  
**Negative:** Every intent incurs the Federation Board refinement cost, even simple intents that appear unambiguous. EC-001 mitigates this by ensuring discovery verbs pass the economic gate without deep refinement.  
**Neutral:** The Federation Board must be updated when new O-Mode types are added.

---

## ADR-002 — Orchestrator-Only TYPE 1 MCP Dispatch (ARCH-NEW-001)

**Status:** Accepted  
**Date:** 2026-05-05  
**Author:** Shawn Bellazan  
**Law Anchor:** ARCH-NEW-001, EC-002

### Context

Fracture FRAC-007: The `DispatchExecutor` in `PmcroWorkflow.cs` was a no-op stub. The Maker produced `McpDispatchDecision` objects correctly but the Orchestrator never executed them. TYPE 1 tools (WriteFile, RunCommand) were planned and approved but never fired. The Checker's physical verification always failed because no file was ever written.

Additionally, multiple early fractures (FRAC-MAKER-TYPE1-001) showed phase agents attempting to call TYPE 1 tools directly, bypassing the Orchestrator gate.

### Decision

TYPE 1 tools (world-changing, side-effecting) are called by one and only one node: the Orchestrator's `DispatchExecutor`. `IMcpToolExecutor` is not injected into phase agents. Phase agents return `McpDispatchDecision` and the Orchestrator executes it. TYPE 2 tools (read, compile, score, analyse) are called natively by phase agents during their own MicroWorkflows.

### Consequences

**Positive:** One accountable point for all world-changing actions. The Trail records exactly which Orchestrator cycle, which Maker decision, and which tool execution produced any file system or shell change.  
**Negative:** Cognitive services cannot directly execute their own plans. They must express intent as a `McpDispatchDecision` and wait for the Orchestrator.  
**Neutral:** `IMcpToolExecutor` is a DI service registered only in `OrchestratorService`. Phase agents' DI containers do not include it.

---

## ADR-003 — Cognitive Trail as Primary Data Structure and Commercial Product

**Status:** Accepted  
**Date:** 2026-05-05  
**Author:** Shawn Bellazan  
**Law Anchor:** ARCH-003

### Context

Agent frameworks typically produce ephemeral outputs — responses, function results, tool call chains. These outputs are not persistent, not attributed, not transferable. The system has no memory of its own operation beyond the current context window.

### Decision

Every PMCRO cycle produces a Cognitive Trail — a typed, immutable, agent-attributed record of the full execution from Federation Board to Reflector. The Trail is the primary data structure. It is also the commercial product: transferable, licensable expertise encoded as a constraint set and behavioral record.

### Consequences

**Positive:** Full audit trail for every autonomous action. The Trail enables debugging, compliance review, and the HIL Flywheel (corrections become TrailFrames become constraints). The Trail enables commercial transfer of domain expertise.  
**Negative:** Trail persistence requires EF Core + PostgreSQL. The substrate has a storage dependency that purely stateless agents do not have.  
**Neutral:** Trail size grows with cycle count. Archival strategy for long-running trails is a future concern.

---

## ADR-004 — DocFX 2.78.5 as Documentation Pipeline

**Status:** Accepted  
**Date:** 2026-05-05  
**Author:** Shawn Bellazan  
**Law Anchor:** EC-005, DFX-001 through DFX-010

### Context

The PMCRO substrate uses XML doc comments with I AM identity declarations on every public type. These declarations need to be rendered into searchable, navigable documentation that reflects the actual source — not aspirational state. Manual documentation diverges. Generated documentation cannot diverge.

### Decision

DocFX 2.78.5 is the documentation pipeline. XML comments generate `api/*.yml`. Conceptual docs live in `docs/**/*.md`. The DocFX build is the CL-007 acceptance criterion — a cycle that produces or modifies source code does not ACCEPT until `docfx metadata` and `docfx build` both exit 0.

### Consequences

**Positive:** Documentation and source are structurally coupled. A public type without `<summary>` causes a DocFX warning. The build enforces documentation discipline without requiring manual review.  
**Negative:** The source must compile before `docfx metadata` can run. DocFX build is a dependency on `dotnet build`.  
**Neutral:** `api/*.yml` and `_site/` are generated outputs. Both are excluded from source control.
