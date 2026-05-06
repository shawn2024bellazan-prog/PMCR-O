// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — ASPIRE APPHOST
// File   : AppHost.cs
// Identity: Distributed Conductor
// Architecture: MAF-Native (no gRPC phase services)
// ThoughtLock: 2026-05-05
// ═══════════════════════════════════════════════════════════════════════════════
//
// FRACTURE FIX — FRAC-CTX-001
//   PROBLEM: Ollama was truncating prompts at 4,096 tokens, causing
//            tool definitions to be silently dropped from the context
//            window. The Maker received its prompt with no knowledge
//            of available tools and hallucinated tool results.
//
//   ROOT CAUSE: Ollama's default num_ctx is 4,096 tokens for Qwen2.5-Coder-7B.
//     A PMCRO Maker prompt with skills, plan JSON, and history easily
//     exceeds this limit. When the context is truncated, the tool
//     definitions injected by AgentSkillsProvider are silently dropped,
//     so the model never sees run_skill_script in its context.
//
//   FIX: Set OLLAMA_NUM_CTX=8192 as an environment variable on the
//     Ollama container. Ollama honours this env var as the default
//     context length for all loaded models.
// ═══════════════════════════════════════════════════════════════════════════════

var builder = DistributedApplication.CreateBuilder(args);

// ── 0. SHARED CONFIGURATION ───────────────────────────────────────────────────
// Centralize the workspace root so all containers/processes share the same sandbox
var projectRoot = builder.Configuration["Parameters:project-root"]
                  ?? Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", ".."));

// ── 1. LOCAL LLM (OLLAMA) ─────────────────────────────────────────────────────
var ollamaModelName = builder.Configuration["Parameters:ollama-model"] ?? "qwen2.5:7b";

var ollama = builder
    .AddOllama("ollama")
    .WithGPUSupport()
    .WithDataVolume()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithEnvironment("OLLAMA_NUM_CTX", "8192");  // ← FRAC-CTX-001 fix

var chatModel = ollama.AddModel("ollama-chat", ollamaModelName);

// ── 2. SUBSTRATE PERSISTENCE (POSTGRES) ──────────────────────────────────────
var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("pmcro-substrate");

// ── 3. MCP ACTUATORS ──────────────────────────────────────────────────────────
var mcpFilesystem = builder.AddProject<Projects.ProjectName_Mcp_Filesystem>("projectname-mcp-filesystem")
    .WithEnvironment("Parameters__project-root", projectRoot);

var mcpTerminal = builder.AddProject<Projects.ProjectName_Mcp_Terminal>("projectname-mcp-terminal")
    .WithEnvironment("Parameters__project-root", projectRoot);

var mcpPlaywright = builder.AddProject<Projects.ProjectName_Mcp_Playwright>("projectname-mcp-playwright")
    .WithEnvironment("Parameters__project-root", projectRoot);

// ── 4. THE SOLE EXTERNAL SURFACE (ARCH-012) ───────────────────────────────────
builder.AddProject<Projects.ProjectName_OrchestrationApi>("projectname-orchestrationapi")
    .WithReference(chatModel)
    .WithReference(postgres)
    .WithReference(mcpFilesystem)
    .WithReference(mcpTerminal)
    .WithReference(mcpPlaywright)
    .WaitFor(chatModel)
    .WaitFor(postgres)
    .WaitFor(mcpFilesystem)
    .WaitFor(mcpTerminal)
    .WaitFor(mcpPlaywright)
    .WithEnvironment("Parameters__project-root", projectRoot);

builder.Build().Run();