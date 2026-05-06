// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File   : Program.cs
// Identity: Substrate Gateway & Workflow Host
// ThoughtLock: 2026-05-06 → PMCRO v4.0 — Unified Fix (Transport + Instruction Builders)
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using ProjectName.OrchestrationApi.Mcp;
using ProjectName.OrchestrationApi.Models;
using ProjectName.OrchestrationApi.Skills;
using ProjectName.OrchestrationApi.Workflows;
using ProjectName.ServiceDefaults;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── 0. INFRASTRUCTURE ─────────────────────────────────────────────────────────
builder.AddServiceDefaults();
builder.AddAIDefaults();

// ── 1. MCP CLIENT REGISTRY (async startup via IHostedService) ─────────────────
builder.Services.AddSingleton<McpClientRegistryHolder>();
builder.Services.AddHostedService<McpRegistryStartupService>();

// ── 2. NAMED HTTP CLIENTS FOR TYPE 1 DISPATCH ─────────────────────────────────
// These are used exclusively by McpToolExecutor for TYPE 1 gate dispatch.
// FRAC-TRANSPORT-001 FIX: MCP Actuators strictly demand both application/json 
// AND text/event-stream Accept headers to avoid 406 Not Acceptable errors.
builder.Services.AddHttpClient("projectname-mcp-filesystem", client =>
{
    client.BaseAddress = new Uri("http://projectname-mcp-filesystem");
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    client.DefaultRequestHeaders.Accept.ParseAdd("text/event-stream");
});

builder.Services.AddHttpClient("projectname-mcp-terminal", client =>
{
    client.BaseAddress = new Uri("http://projectname-mcp-terminal");
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    client.DefaultRequestHeaders.Accept.ParseAdd("text/event-stream");
});

builder.Services.AddHttpClient("projectname-mcp-playwright", client =>
{
    client.BaseAddress = new Uri("http://projectname-mcp-playwright");
    client.DefaultRequestHeaders.Accept.Clear();
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
    client.DefaultRequestHeaders.Accept.ParseAdd("text/event-stream");
});

builder.Services.AddHttpClient();

// ── 3. REGISTER SHARED UTILITY SKILLS ─────────────────────────────────────────
builder.Services.AddSingleton<FederationBoardSkill>();
builder.Services.AddSingleton<FileSystemSkill>();

// ── 4. MCP EXECUTOR — TYPE 1 GATE — INJECTED ONLY INTO DISPATCHEXECUTOR ───────
builder.Services.AddSingleton<IMcpToolExecutor, McpToolExecutor>();

// ── 5. REGISTER PHASE AGENTS WITH NATIVE MCP TOOLS ───────────────────────────
builder.Services.AddKeyedSingleton<AIAgent>("planner", (sp, _) =>
{
    var registry = sp.GetRequiredService<McpClientRegistryHolder>().Registry;
    var chatClient = sp.GetRequiredService<IChatClient>();
    var brain = new PlannerSkill();
    var type2Tools = registry.AllType2Tools;

    var skillsProvider = new AgentSkillsProviderBuilder().UseSkill(brain).Build();

    var opts = new ChatOptions
    {
        Instructions = BuildPlannerInstructions(brain, type2Tools),
        ResponseFormat = ChatResponseFormat.ForJsonSchema<PlannerResponse>(),
        Tools = [.. type2Tools]
    };

    return chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        Name = "planner",
        ChatOptions = opts,
        AIContextProviders = [skillsProvider]
    }, services: sp);
});

builder.Services.AddKeyedSingleton<AIAgent>("maker", (sp, _) =>
{
    var registry = sp.GetRequiredService<McpClientRegistryHolder>().Registry;
    var chatClient = sp.GetRequiredService<IChatClient>();
    var brain = new MakerSkill();
    var allTools = registry.AllTools;

    var skillsProvider = new AgentSkillsProviderBuilder().UseSkill(brain).Build();

    var opts = new ChatOptions
    {
        Instructions = BuildMakerInstructions(brain, allTools),
        ResponseFormat = ChatResponseFormat.ForJsonSchema<MakerFrame>(),
        Tools = [.. allTools]
    };

    return chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        Name = "maker",
        ChatOptions = opts,
        AIContextProviders = [skillsProvider]
    }, services: sp);
});

builder.Services.AddKeyedSingleton<AIAgent>("checker", (sp, _) =>
{
    var registry = sp.GetRequiredService<McpClientRegistryHolder>().Registry;
    var chatClient = sp.GetRequiredService<IChatClient>();
    var brain = new CheckerSkill();
    var type2Tools = registry.AllType2Tools;

    var skillsProvider = new AgentSkillsProviderBuilder()
        .UseSkill(brain)
        .UseSkill(new OrchestratorSkill())
        .Build();

    var opts = new ChatOptions
    {
        Instructions = BuildCheckerInstructions(brain, type2Tools),
        ResponseFormat = ChatResponseFormat.ForJsonSchema<QualityFrame>(),
        Tools = [.. type2Tools]
    };

    return chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        Name = "checker",
        ChatOptions = opts,
        AIContextProviders = [skillsProvider]
    }, services: sp);
});

builder.Services.AddKeyedSingleton<AIAgent>("reflector", (sp, _) =>
{
    var registry = sp.GetRequiredService<McpClientRegistryHolder>().Registry;
    var chatClient = sp.GetRequiredService<IChatClient>();
    var brain = new ReflectorSkill();
    var type2Tools = registry.AllType2Tools;

    var skillsProvider = new AgentSkillsProviderBuilder().UseSkill(brain).Build();

    var opts = new ChatOptions
    {
        Instructions = BuildReflectorInstructions(brain, type2Tools),
        ResponseFormat = ChatResponseFormat.ForJsonSchema<ReflectorFrame>(),
        Tools = [.. type2Tools]
    };

    return chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        Name = "reflector",
        ChatOptions = opts,
        AIContextProviders = [skillsProvider]
    }, services: sp);
});

builder.Services.AddKeyedSingleton<AIAgent>("test-agent", (sp, _) =>
{
    var fsSkill = sp.GetRequiredService<FileSystemSkill>();
    var chatClient = sp.GetRequiredService<IChatClient>();
    var skillsProvider = new AgentSkillsProviderBuilder().UseSkill(fsSkill).Build();

    var instructions = $"""
        You are a filesystem test agent operating in a secure sandbox.
        @mandate
        You MUST use the tools available to you to answer every question.
        @filesystem-rules
        {fsSkill.GetInstructions()}
        """;

    return chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        Name = "test-agent",
        ChatOptions = new ChatOptions { Instructions = instructions },
        AIContextProviders = [skillsProvider]
    }, services: sp);
});

// ── 6. WORKFLOW & EXECUTORS ───────────────────────────────────────────────────
builder.Services.AddSingleton<PmcroWorkflowFactory>();
builder.Services.AddSingleton<PlannerExecutor>();
builder.Services.AddSingleton<MakerExecutor>();
builder.Services.AddSingleton<DispatchExecutor>();
builder.Services.AddSingleton<CheckerExecutor>();
builder.Services.AddSingleton<ConstraintInjectorExecutor>();
builder.Services.AddSingleton<ReflectorExecutor>();
builder.Services.AddSingleton<EscalateExecutor>();

// ── 7. API & DOCS ─────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "ProjectName PMCRO Substrate";
        document.Info.Version = "v1";
        document.Info.Description = "PMCRO Cognitive Architecture — Transport Verified.";
        return Task.CompletedTask;
    });
});

var app = builder.Build();

app.MapDefaultEndpoints();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference("docs", options =>
    {
        options.WithTitle("PMCRO Substrate Explorer").WithTheme(ScalarTheme.Moon);
    });
    app.MapGet("/", () => Results.Redirect("/docs/v1")).ExcludeFromDescription();
}

app.UseHttpsRedirection();
app.MapControllers();
app.Run();

// ── INSTRUCTION BUILDERS ──────────────────────────────────────────────────────

static string BuildToolCatalogue(IReadOnlyList<AITool> tools)
{
    if (tools.Count == 0) return "No MCP tools available.";
    var sb = new System.Text.StringBuilder();
    sb.AppendLine("@available-mcp-tools");
    sb.AppendLine("(All tool parameters accept string values. Use the exact tool name as shown.)");
    foreach (var tool in tools)
        sb.AppendLine($"  - {tool.Name}: {tool.Description}");
    return sb.ToString();
}

static string BuildPlannerInstructions(PlannerSkill brain, IReadOnlyList<AITool> tools) => $"""
    {brain.GetInstructions()}
    @mcp-tools-available
    You have direct access to the following TYPE 2 MCP tools. Call them natively.
    {BuildToolCatalogue(tools)}
    """;

static string BuildMakerInstructions(MakerSkill brain, IReadOnlyList<AITool> tools) => $"""
    {brain.GetInstructions()}

    @mcp-tools-available
    {BuildToolCatalogue(tools)}

    @critical-dispatch-law
    1. TYPE 2 tools (ListDirectory, ReadFile, GetFileInfo, RunReadOnlyCommand) are COGNITIVE AIDS. 
       - You MUST call them NATIVELY (tool_calls) to see the data yourself.
       - NEVER put them in the 'dispatch_decisions' array. 
    2. TYPE 1 tools (WriteFile, DeletePath, RunCommand) are WORLD-CHANGING.
       - These are the ONLY tools allowed in the 'dispatch_decisions' array.
    3. If the goal is 'List files' or 'Analyze code':
       - Call the tools natively.
       - Put your findings/list in the 'artifacts' array as text.
       - Set 'dispatch_decisions' to [].

    I NEVER dispatch a 'List' tool. I ALWAYS call it natively.
    """;

static string BuildCheckerInstructions(CheckerSkill brain, IReadOnlyList<AITool> tools) => $"""
    {brain.GetInstructions()}
    @mcp-tools-available
    {BuildToolCatalogue(tools)}
    """;

static string BuildReflectorInstructions(ReflectorSkill brain, IReadOnlyList<AITool> tools) => $"""
    {brain.GetInstructions()}
    @mcp-tools-available
    {BuildToolCatalogue(tools)}
    """;