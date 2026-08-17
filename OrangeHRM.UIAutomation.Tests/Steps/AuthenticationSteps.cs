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
        // If storage state was restored by BeforeFeature, the browser may already
        // be authenticated — navigate to the app root and check before re-logging in.
        await _loginPage.NavigateToLogin();
        if (await _loginPage.IsLoggedIn())
        {
            Console.WriteLine($"  [AUTH] Already logged in via cached storage state — skipping Login()");
            return;
        }
        await _loginPage.Login(username, password);
        Assert.That(await _loginPage.IsLoggedIn(), Is.True, $"Login failed for user: {username}");
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
