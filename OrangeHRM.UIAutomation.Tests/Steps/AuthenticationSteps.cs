using TechTalk.SpecFlow;
using OrangeHRM.UIAutomation.Tests.Drivers;
using OrangeHRM.UIAutomation.Tests.Pages;
using OrangeHRM.UIAutomation.Tests.Config;
using NUnit.Framework;

namespace OrangeHRM.UIAutomation.Tests.Steps;

[Binding]
public class AuthenticationSteps
{
    private readonly BrowserDriver _driver;
    private readonly LoginPage _loginPage;
    private readonly TestSettings _settings;

    public AuthenticationSteps(BrowserDriver driver)
    {
        _driver = driver;
        _settings = ConfigurationLoader.GetTestSettings();
        _loginPage = new LoginPage(_driver.Page, _settings.BaseUrl);
    }

    [Given(@"I am on the PeopleFlow login page")]
    public async Task GivenIAmOnThePeopleFlowLoginPage()
    {
        await _loginPage.NavigateToLogin();
        Assert.That(await _loginPage.IsLoginPageVisible(), Is.True, "Login page is not visible");
    }

    [Given(@"I am logged in as ""(.*)"" with password ""(.*)""")]
    public async Task GivenIAmLoggedInAs(string username, string password)
    {
if (string.IsNullOrWhiteSpace(_driver.Page.Url) || _driver.Page.Url == "about:blank")
    await _driver.Page.GotoAsync(_settings.BaseUrl);

// If storage state was restored by BeforeFeature, the browser is already
// authenticated. Keep this step as a semantic assertion only.
if (await _loginPage.IsLoggedIn())
{
    Console.WriteLine($"  [AUTH] Already logged in via cached storage state — skipping Login()");
    return;
}

if (!await _loginPage.IsLoginPageVisible())
    await _loginPage.NavigateToLogin();

// Retry login with exponential backoff: 3s, 5s, 7s, 10s (up to 4 attempts)
int maxRetries = 4;
int[] delays = { 3000, 5000, 7000, 10000 }; // Exponential backoff in ms
 
for (int attempt = 1; attempt <= maxRetries; attempt++)
{
    try
    {
        Console.WriteLine($"  [AUTH] Login attempt {attempt}/{maxRetries} for user: {username}");
        await _loginPage.Login(username, password);
        if (await _loginPage.IsLoggedIn())
        {
            Console.WriteLine($"  ✓ [AUTH] Login succeeded for {username} (attempt {attempt}/{maxRetries})");
            return;
        }
        else
        {
            var error = await _loginPage.GetErrorMessage();
            Console.WriteLine($"  ✗ [AUTH] Login failed for {username} (attempt {attempt}): {error}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  ✗ [AUTH] Login exception (attempt {attempt}): {ex.Message}");
    }

    // If not the last attempt, wait before retrying
    if (attempt < maxRetries)
    {
        int delay = delays[attempt - 1];
        Console.WriteLine($"  ⏳ [AUTH] Waiting {delay}ms before retry attempt {attempt + 1}...");
        await Task.Delay(delay);
    }
}

Assert.That(false, Is.True, $"Login failed for user: {username} after {maxRetries} attempts with exponential backoff (3s, 5s, 7s, 10s)");
    }

    [When(@"I enter username ""(.*)"" and password ""(.*)""")]
    public async Task WhenIEnterUsernameAndPassword(string username, string password)
    {
        await _loginPage.Login(username, password);
    }

    [When(@"I click the login button")]
    public async Task WhenIClickTheLoginButton()
    {
        // Login already clicked in the Login method; this step is a semantic anchor
        await Task.CompletedTask;
    }

    [When(@"I click logout")]
    public async Task WhenIClickLogout()
    {
        await _loginPage.Logout();
    }

    [Then(@"I should be redirected to the dashboard")]
    public async Task ThenIShouldBeRedirectedToTheDashboard()
    {
        Assert.That(await _loginPage.IsLoggedIn(), Is.True, "Expected to be on dashboard after login");
    }

    [Then(@"the navigation menu should be visible")]
    public async Task ThenTheNavigationMenuShouldBeVisible()
    {
        var navVisible = await _driver.Page.Locator("nav, [class*='nav'], [class*='sidebar']").CountAsync() > 0;
        Assert.That(navVisible, Is.True, "Navigation menu is not visible after login");
    }

    [Then(@"I should see an error message")]
    public async Task ThenIShouldSeeAnErrorMessage()
    {
        var error = await _loginPage.GetErrorMessage();
        Assert.That(error, Is.Not.Empty, "Expected an error message but none was displayed");
    }

    [Then(@"I should remain on the login page")]
    public async Task ThenIShouldRemainOnTheLoginPage()
    {
        Assert.That(await _loginPage.IsLoginPageVisible(), Is.True, "Expected to remain on login page");
    }

    [Then(@"I should see a validation error")]
    public async Task ThenIShouldSeeAValidationError()
    {
        var errorVisible = await _driver.Page.Locator("[class*='error']:visible, [class*='invalid']:visible, [required]:invalid").CountAsync() > 0;
        Assert.That(errorVisible, Is.True, "Expected validation error but none shown");
    }

    [Then(@"I should be redirected to the login page")]
    public async Task ThenIShouldBeRedirectedToTheLoginPage()
    {
        Assert.That(await _loginPage.IsLoginPageVisible(), Is.True, "Expected login page after logout");
    }
}
