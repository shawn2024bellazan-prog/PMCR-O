// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — SERVICE DEFAULTS
// File       : AIExtensions.cs
// Identity   : Unified AI & Agent Registration Conductor
// Fix        : Removed UseFunctionInvocation() from the IChatClient pipeline.
//
//   ROOT CAUSE OF DEAD TOOL LOOP:
//   MAF's AgentClassSkill / AgentSkillsProvider inject tools at runtime via
//   AIContextProviders — they are NOT pre-populated in ChatOptions.Tools.
//   FunctionInvokingChatClient (from UseFunctionInvocation()) only intercepts
//   tool calls when Tools are present on the ChatOptions it receives. Since
//   MAF populates tools via context providers just before each model call
//   inside ChatClientAgent.RunAsync(), the FunctionInvokingChatClient wrapper
//   sees an empty Tools list on the first call and never enters its loop.
//   Result: the raw load_skill tool call JSON is returned as the response text
//   instead of being executed.
//
//   THE CORRECT PATTERN:
//   Register a plain IChatClient (no FunctionInvokingChatClient wrapper).
//   MAF's ChatClientAgent owns the tool execution loop — it calls the model,
//   detects tool call responses, invokes the skill scripts, and re-calls the
//   model with results. This loop is internal to AIAgent.RunAsync() and does
//   not require FunctionInvokingChatClient in the pipeline.
//
//   UseFunctionInvocation() is only needed when YOU manage the tool loop
//   outside of MAF — e.g. calling IChatClient.GetResponseAsync() directly
//   with AIFunctionFactory tools in ChatOptions.Tools.
// ThoughtLock: 2026-05-05
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OllamaSharp;

namespace ProjectName.ServiceDefaults;

public static class AIExtensions
{
    private const string OllamaConnectionName = "ollama-chat";

    /// <summary>
    /// Registers the Ollama IChatClient for use by MAF agent factories.
    /// No FunctionInvokingChatClient wrapper — MAF's ChatClientAgent owns the tool loop.
    /// </summary>
    public static IHostApplicationBuilder AddAIDefaults(this IHostApplicationBuilder builder)
    {
        // Step 1: Register IOllamaApiClient via the Aspire toolkit.
        //         This is the one registration the toolkit reliably makes across
        //         all versions. IOllamaApiClient is registered as an unkeyed singleton.
        builder.AddOllamaApiClient(OllamaConnectionName);

        // Step 2: Promote IOllamaApiClient to the unkeyed IChatClient.
        //         OllamaApiClient implements IChatClient directly — the cast is safe.
        //         We use Microsoft.Extensions.AI's AddChatClient(factory) here, NOT
        //         the toolkit's chained .AddChatClient(), to avoid all toolkit
        //         overload ambiguity (CS1929, keyed-service mismatches).
        //         No .UseFunctionInvocation() — MAF handles the tool loop internally.
        builder.Services.AddSingleton<IChatClient>(sp =>
            (IChatClient)sp.GetRequiredService<IOllamaApiClient>());

        // Step 3: Configure the underlying HttpClient for long-running inference.
        //         Infinite timeout prevents Ollama responses from being killed by
        //         the default 100-second HttpClient timeout.
        builder.Services.AddHttpClient(OllamaConnectionName, client =>
        {
            client.Timeout = Timeout.InfiniteTimeSpan;
            client.DefaultRequestHeaders.Add("User-Agent", "PMCRO-Substrate");
        });

        return builder;
    }
}