using TechTalk.SpecFlow;
using OrangeHRM.UIAutomation.Tests.Drivers;
using OrangeHRM.UIAutomation.Tests.Pages;
using OrangeHRM.UIAutomation.Tests.Config;
using NUnit.Framework;

namespace OrangeHRM.UIAutomation.Tests.Steps;

[Binding]
public class AdminManagementSteps
{
    private readonly BrowserDriver _driver;
    private readonly AdminPage _adminPage;
    private readonly TestSettings _settings;

    public AdminManagementSteps(BrowserDriver driver)
    {
        _driver = driver;
        _settings = ConfigurationLoader.GetTestSettings();
        _adminPage = new AdminPage(_driver.Page, _settings.BaseUrl);
    }

    [Given(@"I navigate to the Admin page")]
    public async Task GivenINavigateToTheAdminPage()
    {
        await _adminPage.NavigateToAdmin();
    }

    [Then(@"the admin page should be visible")]
    public async Task ThenTheAdminPageShouldBeVisible()
    {
        Assert.That(await _adminPage.IsAdminPageVisible(), Is.True, "Admin page not visible");
    }

    [Then(@"the user list should be visible")]
    public async Task ThenTheUserListShouldBeVisible()
    {
        var listVisible = await _driver.Page.Locator("table, [class*='user-list'], [class*='list']").CountAsync() > 0;
        Assert.That(listVisible, Is.True, "User list not visible on admin page");
    }

    [Then(@"no password hashes should be exposed in the UI")]
    public async Task ThenNoPasswordHashesShouldBeExposedInTheUI()
    {
        Assert.That(await _adminPage.IsPasswordHashExposed(), Is.False,
            "passwordHash field should not be visible in the UI");
    }

    [When(@"I click Add User")]
    public async Task WhenIClickAddUser()
    {
        await _adminPage.ClickAddUser();
    }

    [When(@"I fill the user form with:")]
    public async Task WhenIFillTheUserFormWith(Table table)
    {
        var data = table.Rows.ToDictionary(r => r["Field"], r => r["Value"]);
        await _adminPage.FillUserForm(
            data.GetValueOrDefault("Username", ""),
            data.GetValueOrDefault("Password", ""),
            data.GetValueOrDefault("DisplayName", ""),
            data.GetValueOrDefault("Role", "EMPLOYEE")
        );
    }

    [When(@"I submit the user form")]
    public async Task WhenISubmitTheUserForm()
    {
        await _adminPage.SubmitUserForm();
    }

    [Then(@"user ""(.*)"" should appear in the user list")]
    public async Task ThenUserShouldAppearInTheUserList(string username)
    {
        Assert.That(await _adminPage.IsUserInList(username), Is.True,
            $"User '{username}' not found in user list after creation");
    }

    [Then(@"I should see a password validation error")]
    public async Task ThenIShouldSeeAPasswordValidationError()
    {
        var errorVisible = await _driver.Page.Locator("[class*='error']:visible, [role='alert']:visible").CountAsync() > 0;
        Assert.That(errorVisible, Is.True, "Expected password validation error");
    }

    [Given(@"user ""(.*)"" already exists")]
    public async Task GivenUserAlreadyExists(string username)
    {
        var exists = await _adminPage.IsUserInList(username);
        if (!exists) Assert.Inconclusive($"Pre-condition: user '{username}' not found. Run create user scenario first.");
    }

    [Then(@"I should see a duplicate username error")]
    public async Task ThenIShouldSeeADuplicateUsernameError()
    {
        var errorVisible = await _driver.Page.Locator("[class*='error']:visible, [role='alert']:visible, [class*='conflict']:visible").CountAsync() > 0;
        Assert.That(errorVisible, Is.True, "Expected duplicate username error");
    }

    [Given(@"user ""(.*)"" is active in the system")]
    public async Task GivenUserIsActiveInTheSystem(string username)
    {
        var exists = await _adminPage.IsUserInList(username);
        if (!exists) Assert.Inconclusive($"Pre-condition: user '{username}' not found.");
    }

    [When(@"I disable user ""(.*)""")]
    public async Task WhenIDisableUser(string username)
    {
        await _adminPage.DisableUser(username);
    }

    [Then(@"user ""(.*)"" should show status ""(.*)""")]
    public async Task ThenUserShouldShowStatus(string username, string status)
    {
        var userStatus = await _adminPage.GetUserStatus(username);
        Assert.That(userStatus, Does.Contain(status).IgnoreCase,
            $"Expected user '{username}' to have status '{status}' but got '{userStatus}'");
    }

    [When(@"I attempt to disable my own account")]
    public async Task WhenIAttemptToDisableMyOwnAccount()
    {
        await _adminPage.DisableUser("Admin");
    }

    [Then(@"I should see an error preventing self-deactivation")]
    public async Task ThenIShouldSeeAnErrorPreventingSelfDeactivation()
    {
        var errorShown = await _adminPage.IsErrorDisplayed("cannot") || await _adminPage.IsErrorDisplayed("own") || await _adminPage.IsErrorDisplayed("self");
        Assert.That(errorShown, Is.True, "Expected self-deactivation protection error");
    }
}
