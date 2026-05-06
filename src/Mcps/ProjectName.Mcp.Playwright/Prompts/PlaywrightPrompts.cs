// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.PLAYWRIGHT
// File       : Prompts/PlaywrightPrompts.cs
// Identity   : Browser Operation Mission Briefs — Pillar Three
// Law Anchor : ARCH-013, PW-LAW-001, PW-LAW-002, PW-LAW-003
// ThoughtLock: 2026-05-05
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.AI;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace ProjectName.Mcp.Playwright.Prompts;

/// <summary>
/// I AM the PlaywrightPrompts Provider — Pillar Three of the Playwright MCP.
/// I give phase agents the rules of engagement for browser automation.
/// Agents MUST read the mission brief before executing browser actions.
/// </summary>
[McpServerPromptType]
public class PlaywrightPrompts
{
    /// <summary>
    /// Returns the foundational rules for using the Playwright MCP Actuator.
    /// Agents must read this before using any Playwright tool.
    /// </summary>
    [McpServerPrompt(Name = "PlaywrightMissionBrief")]
    [Description("Returns the foundational rules and strategy for browser automation via Playwright MCP. Agents MUST read this before using any Playwright tool.")]
    public static IEnumerable<ChatMessage> GetPlaywrightMissionBrief()
    {
        yield return new ChatMessage(
            ChatRole.User,
            """
            You are about to use the Playwright MCP Actuator to automate a browser.

            LAWS OF THE BROWSER (PMCRO PW-LAWS):
            1. HTTP/HTTPS ONLY (PW-LAW-001): You may ONLY navigate to http:// or https:// URLs.
               Never attempt file://, data://, or javascript: URIs. The server will reject them.

            2. NO CREDENTIAL STORAGE (PW-LAW-002): Never ask the browser to remember passwords,
               tokens, or PII. Each PMCRO cycle begins with a clean session boundary.

            3. TIMEOUT CAP (PW-LAW-003): WaitForElement has a hard server-side cap of 30 000 ms.
               Always specify a timeout ≤ 30 000. Never assume the page will load instantly —
               use WaitForElement after NavigateTo for SPAs and dynamic pages.

            4. SINGLE ACTIVE PAGE (PW-LAW-005): There is one shared browser page per container.
               If you need to inspect multiple pages, navigate sequentially, not concurrently.

            STRATEGY:
            - Always read playwright://session/status first to check browser state.
            - After NavigateTo, confirm success by checking GetPageUrl or GetPageTitle.
            - Use GetInnerText with a specific CSS selector rather than GetPageContent
              when you only need a portion of the page — full HTML can be very large.
            - Use TakeScreenshot after a navigation to create an observation artifact.
            - All tools are TYPE 2 — you may call them directly without Orchestrator approval.

            TOOL ORDER FOR WEB RESEARCH:
              1. NavigateTo(url)
              2. WaitForElement("body", 5000)   — confirm DOMContentLoaded
              3. GetPageTitle()                  — confirm you're on the right page
              4. GetInnerText(selector)          — extract the data you need
              5. TakeScreenshot("research.png")  — save observation artifact
            """);
    }

    /// <summary>
    /// Returns a brief for web scraping and content extraction workflows.
    /// </summary>
    [McpServerPrompt(Name = "PlaywrightScrapingBrief")]
    [Description("Returns specialized instructions for web scraping and structured content extraction workflows.")]
    public static IEnumerable<ChatMessage> GetPlaywrightScrapingBrief()
    {
        yield return new ChatMessage(
            ChatRole.User,
            """
            WEB SCRAPING GUIDELINES FOR PLAYWRIGHT MCP:

            1. SELECTOR PRECISION: Always target the most specific CSS selector available.
               Prefer IDs (#target-id) over classes (.item) over tag names (div).
               Use GetInnerText with a precise selector rather than GetPageContent + text parsing.

            2. DYNAMIC CONTENT: For JavaScript-rendered pages (React, Vue, Angular):
               - Use WaitForElement(".your-target-class", 15000) AFTER NavigateTo.
               - Never assume content is present immediately after navigation.

            3. PAGINATION: If content spans pages, use a loop pattern:
               NavigateTo(page1) → extract → NavigateTo(page2) → extract → combine.
               Do not try to open multiple tabs concurrently — this MCP is single-page.

            4. JAVASCRIPT EXTRACTION: For data embedded in JavaScript variables:
               EvaluateJavaScript("window.__INITIAL_STATE__?.products?.length")
               Always cast to String in your expression — the tool returns strings only.

            5. SCREENSHOT EVIDENCE: Always TakeScreenshot after key navigations.
               This creates an audit trail that the Checker and Reflector can verify.
            """);
    }
}