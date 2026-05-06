// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — SERVICE DEFAULTS
// File       : IHasInstructions.cs
// Identity   : Compile-time contract for skill instruction exposure
// Law Anchor : SKILL-001
// ThoughtLock: 2026-05-05
// ═══════════════════════════════════════════════════════════════════════════════
//
// WHY THIS EXISTS:
//   AddMAFAgent<TSkill>() previously called (skill as dynamic).GetInstructions()
//   which compiles fine but throws RuntimeBinderException at startup if any
//   AgentClassSkill<T> forgets to implement the method (OrchestratorSkill was
//   the first victim). This interface moves the contract to compile time —
//   the missing method is a build error, not a runtime crash.
// ═══════════════════════════════════════════════════════════════════════════════

namespace ProjectName.ServiceDefaults;

/// <summary>
/// Contract for AgentClassSkill&lt;T&gt; subclasses that expose their
/// system instructions for injection into ChatOptions.Instructions.
/// Implement this on every skill registered via AddMAFAgent&lt;T&gt;.
/// </summary>
public interface IHasInstructions
{
    /// <summary>
    /// Returns the full system-prompt text for the agent.
    /// Typically delegates to the protected Instructions property on AgentClassSkill&lt;T&gt;.
    /// </summary>
    string GetInstructions();
}