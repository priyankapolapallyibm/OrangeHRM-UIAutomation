using Microsoft.Playwright;
using OrangeHRM.UIAutomation.Tests.Config;

namespace OrangeHRM.UIAutomation.Tests.Drivers;

/// <summary>
/// Manages Playwright browser lifecycle — one browser per test run,
/// one page per scenario (thread-safe via ScenarioContext injection).
///
/// Optimization 1: accepts an optional storageStatePath so BeforeScenario
/// can restore an already-authenticated session (set in BeforeFeature),
/// skipping re-login for every scenario and saving ~30s per test in CI.
/// </summary>
public class BrowserDriver : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private readonly TestSettings _settings;

    public BrowserDriver()
    {
        _settings = ConfigurationLoader.GetTestSettings();
    }

    public IPage Page => _page ?? throw new InvalidOperationException("Page not initialized. Call InitializeAsync first.");

    /// <summary>Context exposed so BeforeFeature can call StorageStateAsync on it.</summary>
    public IBrowserContext Context => _context ?? throw new InvalidOperationException("Context not initialized.");

    /// <param name="storageStatePath">
    ///   Optional path to a Playwright storage-state JSON file produced by
    ///   <c>IBrowserContext.StorageStateAsync</c>. When provided the context
    ///   starts with an already-authenticated session — no login needed.
    /// </param>
    public async Task InitializeAsync(string? storageStatePath = null)
    {
        _playwright = await Playwright.CreateAsync();
        var browserName = _settings.Browser.ToLowerInvariant();

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = _settings.Headless,
            SlowMo   = _settings.SlowMo
        };

        if (browserName == "chromium")
            launchOptions.Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" };

        _browser = browserName switch
        {
            "firefox" => await _playwright.Firefox.LaunchAsync(launchOptions),
            "webkit"  => await _playwright.Webkit.LaunchAsync(launchOptions),
            _         => await _playwright.Chromium.LaunchAsync(launchOptions)
        };

        var contextOptions = new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 },
            RecordVideoDir = null
        };

        // Restore authentication state if a storage-state file was provided
        if (storageStatePath != null && File.Exists(storageStatePath))
            contextOptions.StorageStatePath = storageStatePath;

        _context = await _browser.NewContextAsync(contextOptions);
        _context.SetDefaultTimeout(_settings.DefaultTimeout);
        _page = await _context.NewPageAsync();
    }

    public async Task<byte[]> TakeScreenshotAsync()
    {
        if (_page == null) return Array.Empty<byte>();
        return await _page.ScreenshotAsync(new PageScreenshotOptions { FullPage = true });
    }

    public async ValueTask DisposeAsync()
    {
        if (_page    != null) await _page.CloseAsync();
        if (_context != null) await _context.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }
}
