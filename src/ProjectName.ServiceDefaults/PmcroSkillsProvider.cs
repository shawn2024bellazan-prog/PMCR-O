// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME COGNITIVE SUBSTRATE — SERVICE DEFAULTS
// File       : PmcroSkillsProvider.cs
// Identity   : Agent Skills Directory Resolver
// Law Anchor : ARCH-NEW-001, SKILL-001
// ThoughtLock: 2026-05-05
// ═══════════════════════════════════════════════════════════════════════════════
//
// CHANGES vs prior version:
//   1. Added BuildProvider() factory — returns a fully configured AgentSkillsProvider
//      from the resolved skills directory. Used when registering file-based skill agents.
//   2. Added BuildProviderWithFilter() — for approved-skill-name allowlisting
//      (AgentSkillsProviderBuilder.UseFilter pattern from MAF docs).
//   3. GetSkillsPath() unchanged — resolves from Aspire Parameters:project-root.
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Agents.AI;
using Microsoft.Extensions.Configuration;

namespace ProjectName.ServiceDefaults;

/// <summary>
/// I AM the PmcroSkillsProvider helper.
/// I resolve the physical path for the SKILL.md skills directory and
/// construct AgentSkillsProvider instances for file-based agent registration.
/// </summary>
public static class PmcroSkillsProvider
{
    private const string SkillsSubfolder = "skills";

    /// <summary>
    /// Resolves the absolute path to the skills directory based on Aspire configuration.
    /// Falls back to AppContext.BaseDirectory when project-root param is not set.
    /// </summary>
    public static string GetSkillsPath(IConfiguration configuration)
    {
        var root = configuration["Parameters:project-root"] ?? AppContext.BaseDirectory;
        return Path.Combine(root, SkillsSubfolder);
    }

    /// <summary>
    /// Builds an AgentSkillsProvider from the resolved skills directory.
    /// Discovers all SKILL.md files up to 2 levels deep.
    /// Caching is enabled by default (leave DisableCaching=false in production).
    /// </summary>
    public static AgentSkillsProvider BuildProvider(
        IConfiguration configuration,
        AgentSkillsProviderOptions? options = null)
    {
        var skillsPath = GetSkillsPath(configuration);
        return new AgentSkillsProvider(skillsPath, options: options);
    }

    /// <summary>
    /// Builds an AgentSkillsProvider restricted to an approved set of skill names.
    /// Use this when loading from a shared directory but excluding experimental skills.
    /// </summary>
    public static AgentSkillsProvider BuildProviderWithFilter(
        IConfiguration configuration,
        IReadOnlySet<string> approvedSkillNames,
        AgentSkillsProviderOptions? options = null)
    {
        var skillsPath = GetSkillsPath(configuration);

        return new AgentSkillsProviderBuilder()
            .UseFileSkill(skillsPath, options: options is null ? null : new AgentFileSkillsSourceOptions())
            .UseFilter(skill => approvedSkillNames.Contains(skill.Frontmatter.Name))
            .Build();
    }
}