// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.PLAYWRIGHT
// File       : Resources/PlaywrightResources.cs
// Identity   : Browser State Provider — Pillar Two
// Law Anchor : ARCH-013, PW-LAW-005, SUB-LAW-004
// ThoughtLock: 2026-05-05
// ═══════════════════════════════════════════════════════════════════════════════

using ModelContextProtocol.Server;
using ProjectName.Mcp.Playwright.Configuration;
using System.ComponentModel;
using System.Text.Json;

namespace ProjectName.Mcp.Playwright.Resources;

/// <summary>
/// I AM the PlaywrightResources Provider — Pillar Two of the Playwright MCP.
/// I expose browser session state and configuration to the cognitive federation.
/// </summary>
[McpServerResourceType]
public sealed class PlaywrightResources(PlaywrightSessionManager session, PlaywrightConfig config)
{
    private static readonly JsonSerializerOptions _json = new() { WriteIndented = true };

    /// <summary>
    /// Returns the current operational status of the Playwright browser session.
    /// </summary>
    [McpServerResource(
        UriTemplate = "playwright://session/status",
        Name = "PlaywrightSessionStatus",
        Title = "Playwright Session Status",
        MimeType = "application/json")]
    [Description("Returns whether the browser is running, the current page URL, and configured timeouts.")]
    public string GetSessionStatus() =>
        JsonSerializer.Serialize(new
        {
            browserReady = session.IsReady,
            activeUrl = session.ActiveUrl,
            headless = config.Headless,
            defaultTimeoutMs = config.DefaultTimeoutMs,
            waitForElementTimeoutMs = config.WaitForElementTimeoutMs,
            screenshotDir = config.ScreenshotDir,
            laws = new[]
            {
                "PW-LAW-001: HTTP/HTTPS only — no file:// navigation.",
                "PW-LAW-002: No credentials or PII stored between cycles.",
                "PW-LAW-003: WaitForElement max 30 000 ms enforcement.",
                "PW-LAW-005: PlaywrightSessionManager owns the browser lifecycle."
            }
        }, _json);

    /// <summary>
    /// Returns the tool reference table for all Playwright tools including the Micro-Workflow.
    /// </summary>
    [McpServerResource(
        UriTemplate = "playwright://tools/reference",
        Name = "PlaywrightToolReference",
        Title = "Playwright Tool Reference",
        MimeType = "application/json")]
    [Description("Quick reference for all Playwright tools and argument names.")]
    public string GetToolReference() =>
        JsonSerializer.Serialize(new
        {
            type_classification = "ALL_TYPE2",
            note = "All Playwright tools are TYPE 2. Any phase agent may call them directly.",
            tools = new[]
            {
                new { name = "NavigateTo", args = new[] { "url" }, returns = "Final URL + status" },
                new { name = "GetPageUrl", args = Array.Empty<string>(), returns = "Current URL" },
                new { name = "GetPageTitle", args = Array.Empty<string>(), returns = "Page Title" },
                new { name = "GetPageContent", args = Array.Empty<string>(), returns = "Full HTML" },
                new { name = "GetInnerText", args = new[] { "selector" }, returns = "Text content" },
                new { name = "ClickElement", args = new[] { "selector" }, returns = "Success confirmation" },
                new { name = "FillInput", args = new[] { "selector", "value" }, returns = "Success confirmation" },
                new { name = "WaitForElement", args = new[] { "selector", "timeoutMs" }, returns = "Visibility confirmation" },
                new { name = "TakeScreenshot", args = new[] { "filename" }, returns = "File path" },
                new { name = "EvaluateJavaScript", args = new[] { "expression" }, returns = "Serialized result" },
                
                // MICRO-WORKFLOW (PW-LAW-003)
                new { name = "ExecuteBrowserResearch", args = new[] { "objective", "startUrl", "maxSteps" }, returns = "Summarized research answer" },
            }
        }, _json);
}