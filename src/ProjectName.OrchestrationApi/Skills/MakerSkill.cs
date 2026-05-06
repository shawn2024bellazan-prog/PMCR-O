// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Skills/MakerSkill.cs
// Identity   : The Actuator — cognitive layer, dispatch planner, NOT executor
// Law Anchor : MAKER-001, ARCH-NEW-001, ARCH-MCP-NAT-001, EC-005
// ThoughtLock: 2026-05-06 → Enforcing EC-005 Documentation Standard
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Agents.AI;
using ProjectName.ServiceDefaults;
using System.ComponentModel;

namespace ProjectName.OrchestrationApi.Skills;

/// <summary>
/// I AM the MakerSkill.
/// I provide the instructions and logic for the MakerAgent to produce artifacts and dispatch decisions.
/// </summary>
/// <remarks>
/// <para><b>EC-005:</b> This class serves as the template for the Semantic Documentation Law.</para>
/// </remarks>
internal sealed class MakerSkill : AgentClassSkill<MakerSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } =
        new("maker-phase", "I AM the MakerAgent — the actuator of the PMCRO loop. I plan execution. I do not execute.");

    public string GetInstructions() => Instructions;
    [AgentSkillResource("maker-schema-reference")]
    [Description("Valid McpDispatchDecision schema. Maker uses this to produce correct dispatch entries.")]
    public static string MakerSchemaReference => """
        McpDispatchDecision valid actuators and their TYPE 1 tools ONLY:

          mcp: "filesystem"   tool: "WriteFile"    args: relativePath (string), content (string), overwrite (boolean)
          mcp: "filesystem"   tool: "DeletePath"   args: relativePath (string), recursive (boolean)
          mcp: "terminal"     tool: "RunCommand"   args: command (string), timeoutMs (integer)
          mcp: "playwright"   tool: "ClickElement" args: selector (string)
          mcp: "playwright"   tool: "FillInput"    args: selector (string), value (string)
          mcp: "playwright"   tool: "WaitForElement" args: selector (string), timeoutMs (integer)

        FORBIDDEN in dispatch_decisions (these are TYPE 2 — read-only, call natively):
          filesystem: ReadFile, ListDirectory, GetFileInfo, FindFiles
          terminal:   RunReadOnlyCommand
          playwright: GetPageContent, GetPageTitle, GetPageUrl
        """;

    protected override string Instructions => """
        You are the MakerAgent. You turn ExecutionPlans into artifacts and physical change descriptions.

        @identity
        I AM on the cognitive layer. I reason about what should change. I do NOT execute changes.
        I ALWAYS follow the EC-005 Semantic Documentation Law.

        @documentation-standard (EC-005)
        - I NEVER produce naked code. 
        - I ALWAYS use triple-slash (///) XML comments for DocFX.
        - Every class summary MUST begin with "I AM the...".
        - Every non-trivial method MUST include Law Anchors (ARCH-NNN or EC-NNN) in <remarks>.
        - I document parameters (<param>) and return values (<returns>) with reasoning-ready detail.

        @type1-gate
        dispatch_decisions ONLY accepts TYPE 1 mutative tools. 
        If you find yourself writing ReadFile or ListDirectory in dispatch_decisions: STOP.
        Call those tools natively first, then write your MakerFrame.

        @actuator-routing
        RunCommand belongs to "terminal", not "filesystem" (EC-002).

        @goal-without-write
        If the goal is analysis or reporting, emit empty dispatch_decisions [] and put the result in artifacts (EC-003).

        @output-schema
        Your FINAL response must be ONLY this JSON (no markdown, no commentary):
        {
          "trail_id": "from plan",
          "artifacts": ["full content of each artifact"],
          "dispatch_decisions": [
            {
              "mcp": "filesystem",
              "tool": "WriteFile",
              "args": {
                "relativePath": "relative/path/to/file.cs",
                "content": "full documented code here",
                "overwrite": true
              },
              "artifact_type": "code",
              "rationale": "creates the X file required by step STEP-001"
            }
          ]
        }
        """;


}