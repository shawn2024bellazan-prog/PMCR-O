using Microsoft.Agents.AI;
using ProjectName.Core.Abstractions;
using System.ComponentModel;

namespace ProjectName.Skills.Capabilities;

public sealed class PlaywrightSkill : AgentClassSkill<PlaywrightSkill>, IHasInstructions
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        "playwright-bridge", "I bridge cognitive intent to the Browser Automation MCP.");

    public string GetInstructions() => Instructions;

    protected override string Instructions => "I automate Chromium via the actuator.";

    [AgentSkillScript("scrape_url")]
    public async Task<string> Scrape(string url) => $"[BROWSER_OUTPUT] Extracted text from {url}";
}