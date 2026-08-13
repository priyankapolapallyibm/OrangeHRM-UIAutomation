using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Config;

namespace OrangeHRM.UIAutomation.Tests.Utils;

/// <summary>
/// Manages ExtentReports HTML reporting.
/// Produces Reports/index.html with scenario-level detail.
/// </summary>
public static class ReportManager
{
    private static ExtentReports? _extent;
    private static ExtentTest? _currentTest;
    private static readonly object _lock = new();

    public static void InitializeReport()
    {
        var reportsDir = Path.Combine(AppContext.BaseDirectory, "Reports");
        Directory.CreateDirectory(reportsDir);

        var htmlReporter = new ExtentSparkReporter(Path.Combine(reportsDir, "index.html"));
        htmlReporter.Config.Theme = Theme.Dark;
        htmlReporter.Config.DocumentTitle = "PeopleFlow UI Automation Report";
        htmlReporter.Config.ReportName = "PeopleFlow HR Platform — UI Test Results";
        htmlReporter.Config.TimeStampFormat = "MMM dd, yyyy HH:mm:ss";

        _extent = new ExtentReports();
        _extent.AttachReporter(htmlReporter);
        _extent.AddSystemInfo("Application", "PeopleFlow HR Platform");
        _extent.AddSystemInfo("Framework", "Playwright + SpecFlow BDD + C#");
        _extent.AddSystemInfo("Pattern", "Page Object Model (POM)");
        _extent.AddSystemInfo("Environment", "Local — http://localhost:5173");
        _extent.AddSystemInfo("OS", Environment.OSVersion.ToString());
        _extent.AddSystemInfo(".NET Version", Environment.Version.ToString());
        _extent.AddSystemInfo("Run Date", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
    }

    public static void CreateTest(string testName, string category = "")
    {
        lock (_lock)
        {
            _currentTest = _extent?.CreateTest(testName);
            if (!string.IsNullOrEmpty(category))
                _currentTest?.AssignCategory(category);
        }
    }

    public static void LogPass(string message)
    {
        lock (_lock) { _currentTest?.Pass(message); }
    }

    public static void LogFail(string message, string? details = null)
    {
        lock (_lock)
        {
            _currentTest?.Fail(message);
            if (!string.IsNullOrEmpty(details))
                _currentTest?.Fail($"<pre>{System.Net.WebUtility.HtmlEncode(details)}</pre>");
        }
    }

    public static void LogSkip(string message)
    {
        lock (_lock) { _currentTest?.Skip(message); }
    }

    public static void LogInfo(string message)
    {
        lock (_lock) { _currentTest?.Info(message); }
    }

    public static void AddScreenshot(string filePath, string title = "Screenshot")
    {
        lock (_lock)
        {
            if (File.Exists(filePath))
                _currentTest?.AddScreenCaptureFromPath(filePath, title);
        }
    }

    public static void FlushReport()
    {
        _extent?.Flush();
    }
}
