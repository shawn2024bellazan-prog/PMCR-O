using Microsoft.Extensions.Configuration;

namespace ProjectName.Mcp.Playwright.Configuration;

/// <summary>
/// I AM the PlaywrightConfig.
/// I surface environment-driven Playwright settings and sandbox workspace paths.
///
/// ARCH-013: No browser or filesystem access escapes this configuration boundary.
///
/// Configuration keys (all optional — defaults shown):
///   Parameters:project-root        → workspace root for .pmcro/ directory (default: CWD)
///   PLAYWRIGHT_HEADLESS             → "false" to run headed (default: headless)
///   Playwright:DefaultTimeoutMs     → default page timeout in ms (default: 10000)
///   Playwright:WaitForElementTimeoutMs → WaitForElement cap in ms (default + max: 30000)
///
/// PW-LAW-003: WaitForElementTimeoutMs is ALWAYS capped at 30 000 ms regardless of config.
/// </summary>
public sealed class PlaywrightConfig
{
    /// <summary>Absolute path to the project workspace root.</summary>
    public string WorkspaceRoot { get; }

    /// <summary>Absolute path to the screenshot sandbox directory (.pmcro/screenshots/).</summary>
    public string ScreenshotDir { get; }

    /// <summary>Whether Chromium launches headless. Controlled by PLAYWRIGHT_HEADLESS env var.</summary>
    public bool Headless { get; }

    /// <summary>Default Playwright page timeout in milliseconds.</summary>
    public int DefaultTimeoutMs { get; }

    /// <summary>
    /// Maximum timeout for WaitForElement operations.
    /// PW-LAW-003: Always capped at 30 000 ms — never exceeds this regardless of config value.
    /// </summary>
    public int WaitForElementTimeoutMs { get; }

    public PlaywrightConfig(IConfiguration config)
    {
        WorkspaceRoot = config["Parameters:project-root"]
            ?? Directory.GetCurrentDirectory();

        ScreenshotDir = Path.Combine(WorkspaceRoot, ".pmcro", "screenshots");
        Directory.CreateDirectory(ScreenshotDir);

        Headless = !string.Equals(
            config["PLAYWRIGHT_HEADLESS"], "false",
            StringComparison.OrdinalIgnoreCase);

        DefaultTimeoutMs = int.TryParse(config["Playwright:DefaultTimeoutMs"], out var dt)
            ? dt
            : 10_000;

        // PW-LAW-003: cap enforced here at the source — callers do not re-validate.
        WaitForElementTimeoutMs = Math.Min(
            int.TryParse(config["Playwright:WaitForElementTimeoutMs"], out var wt) ? wt : 30_000,
            30_000);
    }

    /// <summary>
    /// Resolves a screenshot filename to a safe absolute path inside the sandbox.
    /// Generates a timestamped fallback name when <paramref name="filename"/> is null/empty.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown on path traversal attempt (e.g. "../../../etc/passwd").
    /// </exception>
    public string ResolveScreenshotPath(string? filename)
    {
        // Strip any directory components — only the bare filename is honoured.
        var safe = Path.GetFileName(filename);
        if (string.IsNullOrWhiteSpace(safe))
            safe = $"screenshot_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";

        var full = Path.GetFullPath(Path.Combine(ScreenshotDir, safe));

        // Ensure the resolved path stays inside the sandbox.
        if (!full.StartsWith(ScreenshotDir, StringComparison.OrdinalIgnoreCase))
            throw new UnauthorizedAccessException(
                $"THREAT DETECTED: Screenshot path traversal — '{filename}' escapes sandbox.");

        return full;
    }
}