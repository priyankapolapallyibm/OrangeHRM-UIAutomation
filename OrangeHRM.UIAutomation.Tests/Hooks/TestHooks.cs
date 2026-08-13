using TechTalk.SpecFlow;
using OrangeHRM.UIAutomation.Tests.Drivers;
using OrangeHRM.UIAutomation.Tests.Config;
using OrangeHRM.UIAutomation.Tests.Utils;
using BoDi;

namespace OrangeHRM.UIAutomation.Tests.Hooks;

/// <summary>
/// SpecFlow Hooks — manage browser lifecycle and test reporting.
/// Runs Before/After each Scenario.
/// </summary>
[Binding]
public class TestHooks
{
    private readonly IObjectContainer _container;
    private readonly ScenarioContext _scenarioContext;
    private readonly TestSettings _settings;
    private BrowserDriver? _browserDriver;

    public TestHooks(IObjectContainer container, ScenarioContext scenarioContext)
    {
        _container = container;
        _scenarioContext = scenarioContext;
        _settings = ConfigurationLoader.GetTestSettings();
    }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        ReportManager.InitializeReport();
        Console.WriteLine("=== PeopleFlow UI Automation Suite Starting ===");
        Console.WriteLine($"Target: {ConfigurationLoader.GetTestSettings().BaseUrl}");
        Console.WriteLine($"Browser: {ConfigurationLoader.GetTestSettings().Browser}");
        Console.WriteLine($"Headless: {ConfigurationLoader.GetTestSettings().Headless}");
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        ReportManager.FlushReport();
        Console.WriteLine("=== PeopleFlow UI Automation Suite Complete ===");
        Console.WriteLine($"Report: {ConfigurationLoader.GetTestSettings().ReportsPath}/index.html");
    }

    [BeforeScenario(Order = 1)]
    public async Task BeforeScenario()
    {
        _browserDriver = new BrowserDriver();
        await _browserDriver.InitializeAsync();
        _container.RegisterInstanceAs(_browserDriver);

        var featureName = _scenarioContext.ScenarioInfo.Tags.FirstOrDefault(t => t.StartsWith("US-")) ?? "General";
        ReportManager.CreateTest(_scenarioContext.ScenarioInfo.Title, featureName);

        Console.WriteLine($"[START] {_scenarioContext.ScenarioInfo.Title}");
    }

    [AfterScenario(Order = 1)]
    public async Task AfterScenario()
    {
        var status = _scenarioContext.ScenarioExecutionStatus;
        var title = _scenarioContext.ScenarioInfo.Title;

        if (_browserDriver != null)
        {
            // Take screenshot on failure
            if (status == ScenarioExecutionStatus.TestError && _settings.ScreenshotOnFailure)
            {
                try
                {
                    var screenshotBytes = await _browserDriver.TakeScreenshotAsync();
                    var screenshotDir = Path.Combine(AppContext.BaseDirectory, _settings.ScreenshotsPath);
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

        // Log result
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
