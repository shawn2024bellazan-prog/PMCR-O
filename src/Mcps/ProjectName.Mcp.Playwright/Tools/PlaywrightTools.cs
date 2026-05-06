// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.PLAYWRIGHT
// File       : Tools/PlaywrightTools.cs
// Identity   : Browser Automation Actuator — Pillar One
// Law Anchor : ARCH-013, PW-LAW-001, PW-LAW-002, PW-LAW-003, PW-LAW-005
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using ProjectName.Mcp.Playwright.Configuration;
using ProjectName.Mcp.Playwright.Executors;
using System.ComponentModel;

namespace ProjectName.Mcp.Playwright.Tools;

[McpServerToolType]
public sealed class PlaywrightTools(
    PlaywrightSessionManager session,
    PlaywrightConfig config,
    ILoggerFactory loggerFactory)
{
    private readonly ILogger<PlaywrightTools> logger = loggerFactory.CreateLogger<PlaywrightTools>();

    [McpServerTool(Name = "NavigateTo")]
    [Description("Navigate to a URL. TYPE 2 — Safe for all phases. HTTP/HTTPS only.")]
    public async Task<string> NavigateToAsync([Description("Full URL")] string url)
    {
        if (!url.StartsWith("http://") && !url.StartsWith("https://"))
            return "ERROR: Only http:// and https:// URLs are permitted.";

        try
        {
            using var _ = await session.AcquireLockAsync(CancellationToken.None);
            var page = await session.GetPageAsync();
            var response = await page.GotoAsync(url, new() { Timeout = config.DefaultTimeoutMs });
            return $"SUCCESS: Navigated to {page.Url} (HTTP {response?.Status ?? 0})";
        }
        catch (Exception ex) { return $"ERROR: {ex.Message}"; }
    }

    [McpServerTool(Name = "GetPageUrl")]
    [Description("Returns the current URL of the active page.")]
    public async Task<string> GetPageUrlAsync()
    {
        try { var page = await session.GetPageAsync(); return $"SUCCESS: {page.Url}"; }
        catch (Exception ex) { return $"ERROR: {ex.Message}"; }
    }

    [McpServerTool(Name = "GetPageTitle")]
    [Description("Returns the <title> of the current page.")]
    public async Task<string> GetPageTitleAsync()
    {
        try { var page = await session.GetPageAsync(); return $"SUCCESS: {await page.TitleAsync()}"; }
        catch (Exception ex) { return $"ERROR: {ex.Message}"; }
    }

    [McpServerTool(Name = "GetPageContent")]
    [Description("Returns the full outer HTML of the current page.")]
    public async Task<string> GetPageContentAsync()
    {
        try { var page = await session.GetPageAsync(); return $"SUCCESS:\n{await page.ContentAsync()}"; }
        catch (Exception ex) { return $"ERROR: {ex.Message}"; }
    }

    [McpServerTool(Name = "ClickElement")]
    [Description("Clicks an element by CSS selector.")]
    public async Task<string> ClickElementAsync([Description("CSS Selector")] string selector)
    {
        try
        {
            using var _ = await session.AcquireLockAsync(CancellationToken.None);
            var page = await session.GetPageAsync();
            await page.ClickAsync(selector, new() { Timeout = config.DefaultTimeoutMs });
            return $"SUCCESS: Clicked {selector}";
        }
        catch (Exception ex) { return $"ERROR: {ex.Message}"; }
    }

    [McpServerTool(Name = "FillInput")]
    [Description("Fills an input by CSS selector.")]
    public async Task<string> FillInputAsync([Description("Selector")] string selector, [Description("Value")] string value)
    {
        try
        {
            using var _ = await session.AcquireLockAsync(CancellationToken.None);
            var page = await session.GetPageAsync();
            await page.FillAsync(selector, value, new() { Timeout = config.DefaultTimeoutMs });
            return $"SUCCESS: Filled {selector}";
        }
        catch (Exception ex) { return $"ERROR: {ex.Message}"; }
    }

    [McpServerTool(Name = "WaitForElement")]
    [Description("Waits until a CSS selector is visible.")]
    public async Task<string> WaitForElementAsync([Description("Selector")] string selector, [Description("Timeout Ms")] int timeoutMs = 10000)
    {
        var safeTimeout = Math.Min(timeoutMs, config.WaitForElementTimeoutMs);
        try { var page = await session.GetPageAsync(); await page.WaitForSelectorAsync(selector, new() { Timeout = safeTimeout }); return $"SUCCESS: {selector} is visible"; }
        catch (Exception ex) { return $"ERROR: {ex.Message}"; }
    }

    [McpServerTool(Name = "TakeScreenshot")]
    [Description("Captures a full-page screenshot.")]
    public async Task<string> TakeScreenshotAsync([Description("Filename")] string? filename = null)
    {
        var path = config.ResolveScreenshotPath(filename);
        try
        {
            using var _ = await session.AcquireLockAsync(CancellationToken.None);
            var page = await session.GetPageAsync();
            await page.ScreenshotAsync(new() { Path = path, FullPage = true });
            return $"SUCCESS: Screenshot saved to {path}";
        }
        catch (Exception ex) { return $"ERROR: {ex.Message}"; }
    }

    [McpServerTool(Name = "ExecuteBrowserResearch")]
    [Description("Runs multi-step browser research workflow.")]
    public async Task<string> ExecuteBrowserResearchAsync(string objective, string startUrl, int maxSteps = 5)
    {
        var result = await PlaywrightMicroWorkflow.RunAsync(new(objective, startUrl, maxSteps), this, loggerFactory);
        return result.Succeeded ? $"SUCCESS:\n{result.Answer}" : $"ERROR: {result.Answer}";
    }
}