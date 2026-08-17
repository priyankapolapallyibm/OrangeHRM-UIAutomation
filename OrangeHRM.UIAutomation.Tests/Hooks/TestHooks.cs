using TechTalk.SpecFlow;
using OrangeHRM.UIAutomation.Tests.Drivers;
using OrangeHRM.UIAutomation.Tests.Config;
using OrangeHRM.UIAutomation.Tests.Utils;
using BoDi;

namespace OrangeHRM.UIAutomation.Tests.Hooks;

/// <summary>
/// SpecFlow Hooks — manage browser lifecycle and test reporting.
///
/// Optimization 1: BeforeFeature / AfterFeature login caching
///   - One browser logs in once per feature file and saves Playwright
///     browser storage state (cookies + localStorage) to a temp file.
///   - BeforeScenario loads that state instead of re-logging in,
///     saving ~30s per scenario in CI.
///   - Auth feature is excluded (it tests login itself, needs fresh state).
/// </summary>
[Binding]
public class TestHooks
{
    private readonly IObjectContainer _container;
    private readonly ScenarioContext _scenarioContext;
    private readonly FeatureContext _featureContext;
    private readonly TestSettings _settings;
    private BrowserDriver? _browserDriver;

    // Key used to pass storage-state path through FeatureContext
    private const string StorageStateKey = "LoginStorageStatePath";

    public TestHooks(IObjectContainer container, ScenarioContext scenarioContext, FeatureContext featureContext)
    {
        _container = container;
        _scenarioContext = scenarioContext;
        _featureContext = featureContext;
        _settings = ConfigurationLoader.GetTestSettings();
    }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        ReportManager.InitializeReport();
        Console.WriteLine("=== PeopleFlow UI Automation Suite Starting ===");
        Console.WriteLine($"Target:   {ConfigurationLoader.GetTestSettings().BaseUrl}");
        Console.WriteLine($"Browser:  {ConfigurationLoader.GetTestSettings().Browser}");
        Console.WriteLine($"Headless: {ConfigurationLoader.GetTestSettings().Headless}");
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        ReportManager.FlushReport();
        Console.WriteLine("=== PeopleFlow UI Automation Suite Complete ===");
    }

    // ── BeforeFeature: login once, save storage state ─────────────────────
    // Skipped for Authentication feature (it tests login flows itself).
    [BeforeFeature(Order = 1)]
    public static async Task BeforeFeature(FeatureContext featureContext)
    {
        var featureTags = featureContext.FeatureInfo.Tags;

        // Auth feature must run without cached state — each scenario tests login
        if (featureTags.Contains("Authentication"))
        {
            Console.WriteLine($"[Feature] Authentication — skipping pre-login (tests login flows)");
            return;
        }

        var settings = ConfigurationLoader.GetTestSettings();
        var users    = ConfigurationLoader.GetTestUsers();

        // Spin up a throw-away browser just to log in and save state
        var loginDriver = new BrowserDriver();
        await loginDriver.InitializeAsync();

        try
        {
            var loginPage = new Pages.LoginPage(loginDriver.Page, settings.BaseUrl);
            await loginPage.NavigateToLogin();
            await loginPage.Login(users.Admin.Username, users.Admin.Password);

            if (!await loginPage.IsLoggedIn())
                throw new InvalidOperationException($"Pre-feature login failed for {users.Admin.Username}");

            // Save browser storage state (cookies + localStorage) to a temp file
            var statePath = Path.Combine(Path.GetTempPath(),
                $"pf-auth-{featureContext.FeatureInfo.Title.Replace(" ", "_")}-{Guid.NewGuid():N}.json");
            await loginDriver.Page.Context.StorageStateAsync(new() { Path = statePath });

            featureContext[StorageStateKey] = statePath;
            Console.WriteLine($"[Feature] Logged in once for '{featureContext.FeatureInfo.Title}' — state cached");
        }
        finally
        {
            await loginDriver.DisposeAsync();
        }
    }

    [AfterFeature]
    public static void AfterFeature(FeatureContext featureContext)
    {
        // Clean up temp storage state file
        if (featureContext.TryGetValue(StorageStateKey, out string? path) && path != null && File.Exists(path))
        {
            File.Delete(path);
            Console.WriteLine($"[Feature] Cleaned up storage state: {path}");
        }
    }

    // ── BeforeScenario: create browser, restore login state if available ──
    [BeforeScenario(Order = 1)]
    public async Task BeforeScenario()
    {
        string? storageStatePath = null;
        _featureContext.TryGetValue(StorageStateKey, out storageStatePath);

        _browserDriver = new BrowserDriver();
        await _browserDriver.InitializeAsync(storageStatePath);
        _container.RegisterInstanceAs(_browserDriver);

        var featureName = _scenarioContext.ScenarioInfo.Tags.FirstOrDefault(t => t.StartsWith("US-")) ?? "General";
        ReportManager.CreateTest(_scenarioContext.ScenarioInfo.Title, featureName);

        Console.WriteLine($"[START] {_scenarioContext.ScenarioInfo.Title}");
        if (storageStatePath != null)
            Console.WriteLine($"  (restored login state — skipped re-login)");
    }

    [AfterScenario(Order = 1)]
    public async Task AfterScenario()
    {
        var status = _scenarioContext.ScenarioExecutionStatus;
        var title  = _scenarioContext.ScenarioInfo.Title;

        if (_browserDriver != null)
        {
            if (status == ScenarioExecutionStatus.TestError && _settings.ScreenshotOnFailure)
            {
                try
                {
                    var screenshotBytes = await _browserDriver.TakeScreenshotAsync();
                    var screenshotDir   = Path.Combine(AppContext.BaseDirectory, _settings.ScreenshotsPath);
                    Directory.CreateDirectory(screenshotDir);

                    var safeName = string.Concat(title.Split(Path.GetInvalidFileNameChars()))
                                        .Replace(" ", "_").Take(80).ToArray();
                    var fileName = $"FAIL_{new string(safeName)}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                    var filePath = Path.Combine(screenshotDir, fileName);

                    await File.WriteAllBytesAsync(filePath, screenshotBytes);
                    ReportManager.AddScreenshot(filePath, "Failure Screenshot");
                    Console.WriteLine($"[SCREENSHOT] {filePath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[WARNING] Screenshot failed: {ex.Message}");
                }
            }

            await _browserDriver.DisposeAsync();
        }

        if (status == ScenarioExecutionStatus.OK)
        {
            ReportManager.LogPass($"Scenario PASSED: {title}");
            Console.WriteLine($"[PASS] {title}");
        }
        else if (status == ScenarioExecutionStatus.TestError)
        {
            var error = _scenarioContext.TestError?.Message ?? "Unknown error";
            ReportManager.LogFail($"Scenario FAILED: {title}", error);
            Console.WriteLine($"[FAIL] {title} — {error}");
        }
        else
        {
            ReportManager.LogSkip($"Scenario SKIPPED: {title}");
            Console.WriteLine($"[SKIP] {title}");
        }
    }
}
