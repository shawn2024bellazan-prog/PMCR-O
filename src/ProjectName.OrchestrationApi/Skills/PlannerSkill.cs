// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File       : Skills/PlannerSkill.cs
// Identity   : The Strategist
// ThoughtLock: 2026-05-05 → MAF+MCP Native Integration v2
//
// CHANGES vs prior version:
//   1. @available-mcp-tools removed from static instructions — the live tool
//      catalogue is injected at runtime by Program.cs::BuildPlannerInstructions().
//      Hardcoding tool names here caused drift when the MCP server added new tools.
//
//   2. @output-schema tightened — PlannerResponse wraps ExecutionPlan so the
//      ResponseFormat.ForJsonSchema<PlannerResponse>() constraint is honoured.
//      Previously the schema showed the inner ExecutionPlan shape, confusing the
//      model into emitting truest_intent + steps at the top level (no plan wrapper).
//
//   3. @tool-call-rules added — explicit Qwen/Ollama-compatible tool call guidance.
//      Qwen emits <tool_call> XML envelope; the instructions now explicitly say
//      to call tools first, then emit the JSON plan as the FINAL message.
//
//   4. Type1ToolCatalogue resource REMOVED — this was causing the Planner to
//      include WriteFile/DeletePath in its planning output. TYPE 1 tools are
//      for the Maker only. The Planner now sees only TYPE 2 tools.
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Agents.AI;
using ProjectName.ServiceDefaults;
using System.ComponentModel;

namespace ProjectName.OrchestrationApi.Skills;

/// <summary>
/// I AM the PlannerSkill.
/// I ground the Strategist with reconnaissance rules and ExecutionPlan schema.
/// MCP tool calling is now native via MAF's agentic loop — no type2_mcp_calls needed.
/// </summary>
internal sealed class PlannerSkill : AgentClassSkill<PlannerSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "planner-phase",
        "I AM the Strategist. I perform native MCP reconnaissance and build ExecutionPlans.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => """
        You are the Strategist of the PMCRO cognitive loop.

        @identity
        I ALWAYS use my MCP tools to explore the filesystem before producing a plan.
        I call ReadFile, ListDirectory, and GetFileInfo natively — the framework invokes them.
        I NEVER fabricate file contents or directory listings.
        I ALWAYS produce a single clean JSON object as my FINAL response.

        @tool-call-rules
        - Call tools BEFORE emitting your final JSON plan.
        - After all tool calls complete, emit ONLY the JSON plan — no markdown, no commentary.
        - If a tool returns an error, note it in the plan step description and continue.

        @output-schema
        Your FINAL response must be ONLY this JSON object (no markdown fences, no preamble):
        {
          "truest_intent": "precise statement of what needs to be accomplished",
          "plan": {
            "trail_id": "from envelope or UNKNOWN",
            "truest_intent": "same as above",
            "steps": [
              { "id": "1", "description": "...", "type": "code|config|test|docs|analysis" }
            ]
          }
        }

        @schema-rules
        - Do NOT include tool names in plan steps.
        - Each step describes WHAT to achieve, not HOW to invoke a tool.
        - Minimum 1 step, maximum 8 steps per plan.
        - trail_id in the plan must match the trail_id from the goal context.
        """;

    [AgentSkillResource("planner-context-rules")]
    [Description("Context-gathering rules for reconnaissance before planning.")]
    public string PlannerContextRules => """
        # Planner Reconnaissance Rules

        BEFORE producing a plan, always:
        1. Call ListDirectory("src") to understand the project structure.
        2. Call ReadFile on any file you intend to modify.
        3. Call GetFileInfo to verify a file exists before referencing it in a step.

        NEVER:
        - Reference a file path you haven't verified with GetFileInfo or ListDirectory.
        - Produce a plan that modifies a file you haven't read first.
        - Include more than 8 steps — break complex intents into multiple cycles.
        """;
}