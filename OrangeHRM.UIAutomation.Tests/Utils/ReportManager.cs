using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Config;
using System.Collections.Concurrent;

namespace OrangeHRM.UIAutomation.Tests.Utils;

/// <summary>
/// Manages ExtentReports HTML reporting.
/// Thread-safe: each thread (feature fixture) gets its own ExtentTest instance
/// stored in a ConcurrentDictionary keyed by thread ID. This allows
/// Optimization 3 (parallel NUnit fixtures) to run without report corruption.
/// </summary>
public static class ReportManager
{
    private static ExtentReports? _extent;
    private static readonly object _initLock = new();

    // Thread-local test storage — safe under NUnit ParallelScope.Fixtures
    private static readonly ConcurrentDictionary<int, ExtentTest> _tests = new();

    private static ExtentTest? CurrentTest =>
        _tests.TryGetValue(Environment.CurrentManagedThreadId, out var t) ? t : null;

    public static void InitializeReport()
    {
        lock (_initLock)
        {
            if (_extent != null) return; // idempotent for parallel init calls

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
    }

    public static void CreateTest(string testName, string category = "")
    {
        lock (_initLock)
        {
            var test = _extent?.CreateTest(testName);
            if (!string.IsNullOrEmpty(category))
                test?.AssignCategory(category);
            if (test != null)
                _tests[Environment.CurrentManagedThreadId] = test;
        }
    }

    public static void LogPass(string message)    => CurrentTest?.Pass(message);
    public static void LogFail(string message, string? details = null)
    {
        CurrentTest?.Fail(message);
        if (!string.IsNullOrEmpty(details))
            CurrentTest?.Fail($"<pre>{System.Net.WebUtility.HtmlEncode(details)}</pre>");
    }
    public static void LogSkip(string message)    => CurrentTest?.Skip(message);
    public static void LogInfo(string message)    => CurrentTest?.Info(message);

    public static void AddScreenshot(string filePath, string title = "Screenshot")
    {
        if (File.Exists(filePath))
            CurrentTest?.AddScreenCaptureFromPath(filePath, title);
    }

    public static void FlushReport()
    {
        lock (_initLock) { _extent?.Flush(); }
    }
}
