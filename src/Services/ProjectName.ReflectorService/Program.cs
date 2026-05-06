// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — REFLECTOR SERVICE
// File       : Program.cs
// Identity   : The Learning Lobe (Cognitive)
// Law Anchor : PROTO-008, MAF-001
// ThoughtLock: 2026-05-06
// ═══════════════════════════════════════════════════════════════════════════════

using ProjectName.ReflectorService.Services;
using ProjectName.Skills.Core;
using ProjectName.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Standard Aspire defaults + centralized AI configuration
builder.AddServiceDefaults();
builder.AddAIDefaults();

// Register the cognitive skill
builder.Services.AddSingleton<ReflectorSkill>();

builder.Services.AddHttpClient();
builder.Services.AddGrpc();

var app = builder.Build();

app.MapDefaultEndpoints();
app.MapGrpcService<ReflectorGrpcService>();

app.MapGet("/", () => "ProjectName.ReflectorService — gRPC Learning Lobe.");

app.Run();