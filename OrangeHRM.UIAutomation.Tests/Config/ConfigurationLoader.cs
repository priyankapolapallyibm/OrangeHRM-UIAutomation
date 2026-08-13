using Microsoft.Extensions.Configuration;

namespace OrangeHRM.UIAutomation.Tests.Config;

public class TestSettings
{
    public string BaseUrl { get; set; } = "http://localhost:5173";
    public string ApiBaseUrl { get; set; } = "http://localhost:3000/api";
    public string Browser { get; set; } = "chromium";
    public bool Headless { get; set; } = true;
    public int SlowMo { get; set; } = 0;
    public int DefaultTimeout { get; set; } = 30000;
    public bool ScreenshotOnFailure { get; set; } = true;
    public string ScreenshotsPath { get; set; } = "Reports/Screenshots";
    public string ReportsPath { get; set; } = "Reports";
}

public class TestUser
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
}

public class TestUsers
{
    public TestUser Admin { get; set; } = new();
    public TestUser Employee { get; set; } = new();
}

public static class ConfigurationLoader
{
    private static IConfigurationRoot? _config;

    public static IConfigurationRoot GetConfiguration()
    {
        if (_config != null) return _config;

        _config = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("Config/appsettings.json", optional: false, reloadOnChange: false)
            .AddEnvironmentVariables("TEST_")
            .Build();

        return _config;
    }

    public static TestSettings GetTestSettings()
    {
        var settings = new TestSettings();
        GetConfiguration().GetSection("TestSettings").Bind(settings);
        return settings;
    }

    public static TestUsers GetTestUsers()
    {
        var users = new TestUsers();
        GetConfiguration().GetSection("TestUsers").Bind(users);
        return users;
    }
}
