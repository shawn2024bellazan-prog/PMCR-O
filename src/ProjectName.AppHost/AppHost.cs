var builder = DistributedApplication.CreateBuilder(args);

var projectRoot = builder.Configuration["Parameters:project-root"]
                  ?? Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", ".."));

var postgres = builder.AddPostgres("postgres")
    .WithDataVolume()
    .AddDatabase("pmcro-substrate");

var ollama = builder.AddOllama("ollama")
    .WithGPUSupport()
    .WithDataVolume()
    .WithEnvironment("OLLAMA_NUM_CTX", "8192")
    .WithLifetime(ContainerLifetime.Persistent);

var chatModel = ollama.AddModel("ollama-chat", "qwen3:8b");

var mcpFilesystem = builder.AddProject<Projects.ProjectName_Mcp_Filesystem>("mcp-filesystem")
    .WithEnvironment("Parameters__project-root", projectRoot);

var mcpTerminal = builder.AddProject<Projects.ProjectName_Mcp_Terminal>("mcp-terminal")
    .WithEnvironment("Parameters__project-root", projectRoot);

var mcpPlaywright = builder.AddProject<Projects.ProjectName_Mcp_Playwright>("mcp-playwright")
    .WithEnvironment("Parameters__project-root", projectRoot);

var plannerService = builder.AddProject<Projects.ProjectName_PlannerService>("planner-service")
    .WithReference(chatModel)
    .WithReference(mcpFilesystem);

// FIX: Added .WithReference(mcpFilesystem) so Aspire injects the mcp-filesystem
// endpoint env var (services__mcp-filesystem__http__0) into the maker-service
// process. MakerService/Program.cs reads this key and pre-configures the named
// "mcp-filesystem" HttpClient, bypassing hostname-based DNS resolution entirely.
// Without this reference the env var is never injected and every TYPE 1 MCP
// WriteFile dispatch from the Maker degrades to a no-op MakerFrame with
// SocketException 11001.
var makerService = builder.AddProject<Projects.ProjectName_MakerService>("maker-service")
    .WithReference(chatModel)
    .WithReference(mcpFilesystem);   // ← FIX

// FIX: Added .WithReference(mcpFilesystem) so the checker-service gets the
// Aspire-injected endpoint for its TYPE 2 ReadFile verification calls.
// Without this the "mcp-filesystem" named HttpClient has no BaseAddress and
// every verification attempt fails with SocketException 11001 after 4 Polly
// retries (~18 s wasted per cycle), after which the LLM Checker responds in
// plain English prose — causing the Orchestrator to crash with
//   JsonException: 'G' is an invalid start of a value.
var checkerService = builder.AddProject<Projects.ProjectName_CheckerService>("checker-service")
    .WithReference(chatModel)
    .WithReference(mcpFilesystem);   // ← FIX

var reflectorService = builder.AddProject<Projects.ProjectName_ReflectorService>("reflector-service")
    .WithReference(chatModel);

var orchestratorService = builder.AddProject<Projects.ProjectName_OrchestratorService>("orchestrator-service")
    .WithReference(postgres)
    .WithReference(plannerService)
    .WithReference(makerService)
    .WithReference(checkerService)
    .WithReference(reflectorService)
    .WithReference(mcpFilesystem)
    .WithReference(mcpTerminal)
    .WithReference(mcpPlaywright);

builder.AddProject<Projects.ProjectName_OrchestrationApi>("orchestration-api")
    .WithReference(orchestratorService)
    .WithEnvironment("Parameters__project-root", projectRoot)
    .WithExternalHttpEndpoints();

builder.Build().Run();