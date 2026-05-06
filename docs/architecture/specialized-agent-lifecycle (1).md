---
title: "Specialized Agent Lifecycle Reference"
---

# Specialized Agent Lifecycle Reference

*ThoughtLock: 2026-05-06 · Reference file for pmcro-framework skill*

---

## Spawn Protocol

```
1. Orchestrator confirms domain has no existing skill (EC-007)
2. Round Table runs in GENERATIVE mode
3. Reflector produces skill skeleton
4. Orchestrator executes TYPE 1 file write → skills/[slug]/SKILL.md
5. Checker validates:
   - I AM declaration present (ARCH-SA-001)
   - CORE layer populated
   - At least one EARNED constraint present
   - Tool manifest present
6. Skill registered in library
7. Loop proceeds driven by new Specialized Agent
```

---

## Versioning Rules

- Every ACCEPT cycle = new version. No threshold. No gate.
- Version tag format: `"MAJOR.0.0"` where MAJOR increments per cycle
- Prior versions archived as ReflectorFrame in Trail
- Current SKILL.md always = latest version
- Suppressed constraints annotated, never deleted

---

## CORE Extension Protocol

CORE is modified only when the Planner declares a scope extension in a Round Table refinement session. The declaration must be explicit:

> "Hey Orchestrator — v6 covers current CORE. **This cycle we also need [capability]. Extending CORE scope.**"

The word "Extending" is the signal. Without it, the Planner is operating within existing CORE and the new capability belongs in the cycle's execution plan only — not in the skill file.

---

## Identity Injection Map

| Field | Source | Applied At |
|---|---|---|
| `identity.user_name` | License agreement | Activation |
| `identity.user_email` | License agreement | Activation |
| `identity.branch_prefix` | License agreement | Activation |
| `identity.commit_signature` | License agreement | Activation |
| `identity.org_slug` | License agreement | Activation |

Fields injected at activation. Never stored in seller's SKILL.md. Never transmitted back to seller.

---

## Fork Merge Policy

| Scenario | Policy |
|---|---|
| Buyer wants to contribute constraint to seller's CORE | Trail merge proposal required. Seller reviews. |
| Seller wants to push CORE update to buyer's fork | Governed by license agreement. Buyer can reject. |
| Buyer EARNED constraint becomes CORE candidate | Buyer initiates proposal. Neither party merges unilaterally. |
| Constraint suppressed in buyer's fork | Stays suppressed. Never auto-reverted. |

---

## Laws Anchored Here

| ID | Statement |
|---|---|
| ARCH-SA-001 | I ALWAYS include an I AM declaration in every Specialized Agent SKILL.md. |
| ARCH-SA-002 | I NEVER modify the EARNED layer directly. Only the Reflector appends to EARNED. |
| ARCH-SA-003 | I NEVER generalise a buyer-context EARNED constraint into CORE without a Trail merge agreement. |
| EC-007 | I ALWAYS check the skill library before the Round Table speaks. |
