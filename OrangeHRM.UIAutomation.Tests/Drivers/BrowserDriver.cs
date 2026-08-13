using Microsoft.Playwright;
using OrangeHRM.UIAutomation.Tests.Config;

namespace OrangeHRM.UIAutomation.Tests.Drivers;

/// <summary>
/// Manages Playwright browser lifecycle — one browser per test run,
/// one page per scenario (thread-safe via ScenarioContext injection).
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

    public async Task InitializeAsync()
    {
        _playwright = await Playwright.CreateAsync();

        var launchOptions = new BrowserTypeLaunchOptions
        {
            Headless = _settings.Headless,
            SlowMo = _settings.SlowMo,
            Args = new[] { "--no-sandbox", "--disable-dev-shm-usage" }
        };

        _browser = _settings.Browser.ToLower() switch
        {
            "firefox" => await _playwright.Firefox.LaunchAsync(launchOptions),
            "webkit"  => await _playwright.Webkit.LaunchAsync(launchOptions),
            _         => await _playwright.Chromium.LaunchAsync(launchOptions)
        };

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 },
            RecordVideoDir = null
        });

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
        if (_page != null) await _page.CloseAsync();
        if (_context != null) await _context.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
        _playwright?.Dispose();
    }
}
