---
title: Cross-Platform and Host Portability
---

# Cross-Platform and Host Portability

**Requirement:** `AS-ARCH-PORTABILITY-001`  
**Status:** Proposed architecture requirement — not yet a runtime conformance claim.

## Intent

AgentSystem applications and reusable agent capabilities should be deployable across compatible operating systems and hosting environments without rewriting agent definitions or transferring governance authority to a host or interaction channel.

**Design principle:** define the application and agent capability once; adapt the host through explicit, validated contracts.

## Portability is multidimensional

| Dimension | Meaning | Proof required |
|---|---|---|
| OS portability | Runs on supported operating systems and architectures | Build and runtime tests per OS/architecture |
| Hosting portability | Deploys to compatible local, container, VM, and cloud hosts | Deployment and health checks per substrate |
| Runtime portability | Agent/workflow behavior is preserved by compatible runtimes | Runtime contract conformance suite |
| Channel portability | The same agent is exposed through independently governed channels | CLI, web, IDE, messaging, API/MCP equivalence tests |
| Infrastructure portability | External dependencies are accessed through replaceable adapters | Database, storage, secrets, model, telemetry adapter tests |

A container image running on multiple hosts proves neither runtime portability nor channel portability by itself.

## Architectural boundaries

1. **Agent identity and definition** are independent of host, channel, and deployment manifest.
2. **Capabilities and skills** are declared and versioned; package presence does not grant execution authority.
3. **Orchestration and runtime** own lifecycle, phase transitions, checkpoint/resume, cancellation, and evidence semantics.
4. **Interaction channels** adapt input/output and session context; they do not redefine the underlying agent.
5. **Host adapters** encapsulate OS, filesystem, process, network, secrets, storage, model provider, and telemetry differences.
6. **Authentication and authorization** remain host-enforced and separate from discovery, registration, activation, and execution.
7. **Project composition** selects agents, capabilities, and channels without changing canonical identity or global/project workspace boundaries.

## Host capability declaration

A compatible host should publish a versioned capability declaration containing:

- host identity/version and supported contract versions;
- OS, architecture, and deployment substrate;
- runtime and isolation guarantees;
- model/tool provider adapters;
- filesystem/storage scopes and network egress policy;
- secret and identity provider integration;
- checkpoint persistence/resume support;
- telemetry/audit capabilities;
- available channels and explicit limitations.

The application package declares required and optional capabilities. Compatibility is evaluated before activation. A host declaration is not an authority grant; security-sensitive capabilities require conformance evidence.

## .NET deployment model

For .NET applications, portability is established per target:

- framework-dependent deployment requires a compatible installed runtime;
- self-contained deployment bundles the runtime for a specific OS/architecture;
- Native AOT is optional and subject to trimming/AOT compatibility;
- containers package an OS-specific userspace and architecture and are not universal binaries.

One source tree may produce several deployment artifacts. Do not claim a single binary runs everywhere without testing the exact matrix.

## Governance invariants

This requirement does not replace the Agent Directory, marketplace, PMCR-O control plane, plugin lifecycle, or runtime services. It does not authorize host-driven activation, wider permissions, or migration of workspace boundaries. Host adapters are execution infrastructure, not a new source of agent identity or governance authority.

## Acceptance criteria

1. Versioned contracts exist for package requirements, host capabilities, runtime, channels, and evidence.
2. Host capability negotiation rejects required-but-unsupported capabilities.
3. The same unchanged agent package runs on at least two compatible host substrates.
4. Agent identity, authorization decisions, lifecycle state, and evidence semantics remain consistent.
5. Conformance tests cover OS, hosting, runtime, channel, infrastructure, security, and evidence dimensions independently.
6. Results are retained against exact package, runtime, and host versions and independently checked.

Until these criteria are demonstrated, AgentSystem portability remains a target architecture requirement rather than a verified property of every current deployment.
