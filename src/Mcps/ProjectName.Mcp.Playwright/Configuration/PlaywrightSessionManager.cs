// ═══════════════════════════════════════════════════════════════════════════════
// PROJECTNAME — MCP.PLAYWRIGHT
// File       : Configuration/PlaywrightSessionManager.cs
// Identity   : Browser Life-cycle Coordinator
// Law Anchor : ARCH-013, PW-LAW-005
// ThoughtLock: 2026-05-01
// ═══════════════════════════════════════════════════════════════════════════════

using Microsoft.Playwright;
using Microsoft.Extensions.Logging;

namespace ProjectName.Mcp.Playwright.Configuration;

/// <summary>
/// I AM the PlaywrightSessionManager.
/// I manage the physical Chromium instance and expose state to the cognitive federation.
/// </summary>
public sealed class PlaywrightSessionManager(ILogger<PlaywrightSessionManager> logger) : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IPage? _activePage;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// Returns true if the Chromium browser process is running.
    /// </summary>
    public bool IsReady => _browser is not null;

    /// <summary>
    /// Returns the URL of the active browser page or a placeholder if none.
    /// </summary>
    public string ActiveUrl => _activePage?.Url ?? "about:blank";

    /// <summary>
    /// Acquires the active browser page, initializing the browser on first call.
    /// </summary>
    public async Task<IPage> GetPageAsync(CancellationToken ct = default)
    {
        if (_browser is null)
        {
            logger.LogInformation("[Playwright] Initializing physical Chromium instance...");
            _playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = new[] { "--disable-blink-features=AutomationControlled" }
            });
            _activePage = await _browser.NewPageAsync();
        }
        return _activePage!;
    }

    /// <summary>
    /// Acquires a thread-safe lock for browser interaction.
    /// </summary>
    public async Task<IDisposable> AcquireLockAsync(CancellationToken ct)
    {
        await _lock.WaitAsync(ct);
        return new LockHandle(_lock);
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null) await _browser.DisposeAsync();
        _playwright?.Dispose();
        _lock.Dispose();
    }

    private sealed class LockHandle(SemaphoreSlim sem) : IDisposable
    {
        public void Dispose() => sem.Release();
    }
}