// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ORCHESTRATION API
// File   : Program.cs
// Identity: PMCRO Membrane & Cognitive Lobe Host
// Law Anchor : ARCH-012, ARCH-NEW-001, EC-005, MAF-001
// ThoughtLock: 2026-05-06
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Http.Resilience;
using ProjectName.Contracts;
using ProjectName.OrchestrationApi.Mcp;
using ProjectName.OrchestrationApi.Workflows;
using ProjectName.ServiceDefaults;
using ProjectName.Skills.Core;
using ProjectName.Skills.Governance;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// ── 0. INFRASTRUCTURE ─────────────────────────────────────────────────────────
builder.AddServiceDefaults();
builder.AddAIDefaults();

builder.Services.AddControllers();
builder.Services.AddHttpClient();

// ── 1. MCP ACTUATOR REGISTRY ──────────────────────────────────────────────────
// ── 1. MCP ACTUATOR REGISTRY ──────────────────────────────────────────────────
builder.Services.AddSingleton<McpClientRegistry>();
builder.Services.AddSingleton<IMcpToolExecutor, McpToolExecutor>();


// ── 2. PHASE SKILLS ──────────────────────────────────────────────────────────
builder.Services.AddSingleton<FederationBoardSkill>();
builder.Services.AddSingleton<PlannerSkill>();
builder.Services.AddSingleton<MakerSkill>();
builder.Services.AddSingleton<CheckerSkill>();
builder.Services.AddSingleton<ReflectorSkill>();

// ── 3. PHASE AGENTS (Keyed Registrations) ─────────────────────────────────────

builder.Services.AddKeyedSingleton<AIAgent>("planner", (sp, _) =>
{
    var skill = sp.GetRequiredService<PlannerSkill>();
    var client = sp.GetRequiredService<IChatClient>();

    // Use AsAIAgent extension to handle read-only property assignment
    return client.AsAIAgent(
        name: "Planner",
        instructions: skill.GetInstructions()
    );
});

builder.Services.AddKeyedSingleton<AIAgent>("maker", (sp, _) =>
{
    var skill = sp.GetRequiredService<MakerSkill>();
    var client = sp.GetRequiredService<IChatClient>();

    return client.AsAIAgent(
        name: "Maker",
        instructions: skill.GetInstructions()
    );
});

builder.Services.AddKeyedSingleton<AIAgent>("checker", (sp, _) =>
{
    var skill = sp.GetRequiredService<CheckerSkill>();
    var client = sp.GetRequiredService<IChatClient>();

    return client.AsAIAgent(
        name: "Checker",
        instructions: skill.GetInstructions()
    );
});

builder.Services.AddKeyedSingleton<AIAgent>("reflector", (sp, _) =>
{
    var skill = sp.GetRequiredService<ReflectorSkill>();
    var client = sp.GetRequiredService<IChatClient>();

    return client.AsAIAgent(
        name: "Reflector",
        instructions: skill.GetInstructions()
    );
});

// ── 4. WORKFLOW FACTORY ───────────────────────────────────────────────────────
builder.Services.AddScoped<PmcroWorkflowFactory>();
// ── 4. WORKFLOW EXECUTORS + FACTORY ───────────────────────────────────────────
builder.Services.AddScoped<PlannerExecutor>();
builder.Services.AddScoped<MakerExecutor>();
builder.Services.AddScoped<DispatchExecutor>();
builder.Services.AddScoped<CheckerExecutor>();
builder.Services.AddScoped<ReflectorExecutor>();
// ── 5. gRPC CLIENT REGISTRATION ──────────────────────────────────────────────
// ── 5. gRPC CLIENT REGISTRATION ──────────────────────────────────────────────
static void RegisterResilientGrpcClient<TClient>(WebApplicationBuilder b, string serviceName)
    where TClient : class
{
    b.Services
        .AddGrpcClient<TClient>(options =>
            options.Address = new Uri($"https://{serviceName}")) // <-- CHANGED TO https
        .AddServiceDiscovery()
        .AddStandardResilienceHandler()
        .Configure(static options =>
        {
            options.TotalRequestTimeout.Timeout = TimeSpan.FromMinutes(10);
            options.AttemptTimeout.Timeout = TimeSpan.FromMinutes(9);
            options.CircuitBreaker.SamplingDuration = TimeSpan.FromMinutes(20);
            options.Retry.MaxRetryAttempts = 1;
        });
}

// Register the Orchestrator (Note: Use the exact name registered in AppHost.cs)
RegisterResilientGrpcClient<Orchestrator.OrchestratorClient>(builder, "orchestrator-service"); 

// ── 6. API EXPLORATION ────────────────────────────────────────────────────────
builder.Services.AddOpenApi("v1", options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Info.Title = "ProjectName Membrane";
        document.Info.Version = "v1";
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
        options.WithTitle("PMCRO Membrane Explorer").WithTheme(ScalarTheme.Moon);
    });
    app.MapGet("/", () => Results.Redirect("/docs/v1")).ExcludeFromDescription();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();