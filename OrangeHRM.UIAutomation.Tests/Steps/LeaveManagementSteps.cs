using TechTalk.SpecFlow;
using OrangeHRM.UIAutomation.Tests.Drivers;
using OrangeHRM.UIAutomation.Tests.Pages;
using OrangeHRM.UIAutomation.Tests.Config;
using NUnit.Framework;

namespace OrangeHRM.UIAutomation.Tests.Steps;

[Binding]
public class LeaveManagementSteps
{
    private readonly BrowserDriver _driver;
    private readonly LeavePage _leavePage;
    private readonly TestSettings _settings;

    public LeaveManagementSteps(BrowserDriver driver)
    {
        _driver = driver;
        _settings = ConfigurationLoader.GetTestSettings();
        _leavePage = new LeavePage(_driver.Page, _settings.BaseUrl);
    }

    [Given(@"I navigate to the Leave page")]
    public async Task GivenINavigateToTheLeavePage()
    {
        await _leavePage.NavigateToLeave();
    }

    [Then(@"the leave management page should be visible")]
    public async Task ThenTheLeaveManagementPageShouldBeVisible()
    {
        Assert.That(await _leavePage.IsLeavePageVisible(), Is.True, "Leave management page not visible");
    }

    [Then(@"the leave request list should be visible")]
    public async Task ThenTheLeaveRequestListShouldBeVisible()
    {
        // Check for table OR empty state message
        var hasTable = await _driver.Page.Locator("table").CountAsync() > 0;
        var hasEmptyState = await _driver.Page.Locator("text=No leave requests yet").CountAsync() > 0;
        var hasListContainer = await _driver.Page.Locator("[class*='leave-list'], [class*='list'], [role='region']").CountAsync() > 0;
        
        var listVisible = hasTable || hasEmptyState || hasListContainer;
        Assert.That(listVisible, Is.True, "Leave request list not visible");
    }

    [When(@"I click New Leave Request")]
    public async Task WhenIClickNewLeaveRequest()
    {
        await _leavePage.ClickNewLeaveRequest();
    }

    [When(@"I fill the leave form with:")]
    public async Task WhenIFillTheLeaveFormWith(Table table)
    {
        var data = table.Rows.ToDictionary(r => r["Field"], r => r["Value"]);
        await _leavePage.FillLeaveForm(
            data.GetValueOrDefault("LeaveType", "ANNUAL"),
            data.GetValueOrDefault("StartDate", ""),
            data.GetValueOrDefault("EndDate", ""),
            data.GetValueOrDefault("Reason", "")
        );
    }

    [When(@"I submit the leave form")]
    public async Task WhenISubmitTheLeaveForm()
    {
        await _leavePage.SubmitLeaveForm();
    }

    [Then(@"the leave request should appear in the list with status ""(.*)""")]
    public async Task ThenTheLeaveRequestShouldAppearWithStatus(string status)
    {
        var content = await _driver.Page.ContentAsync();
        Assert.That(content, Does.Contain(status).IgnoreCase,
            $"Expected leave request with status '{status}' to be visible");
    }

    [Then(@"I should see a date validation error")]
    public async Task ThenIShouldSeeADateValidationError()
    {
        var errorVisible = await _leavePage.IsErrorDisplayed();
        Assert.That(errorVisible, Is.True, "Expected date validation error");
    }

    [Then(@"I should see an overlap conflict error")]
    public async Task ThenIShouldSeeAnOverlapConflictError()
    {
        var errorVisible = await _leavePage.IsErrorDisplayed();
        Assert.That(errorVisible, Is.True, "Expected overlap conflict error but none shown");
    }

    [Given(@"a pending leave request exists in the list")]
    public async Task GivenAPendingLeaveRequestExistsInTheList()
    {
        var hasPending = await _driver.Page.Locator("text=PENDING").CountAsync() > 0;
        if (!hasPending)
            Assert.Inconclusive("Pre-condition failed: no PENDING leave request exists. Run submit scenario first.");
    }

    [When(@"I approve the leave request")]
    public async Task WhenIApproveTheLeaveRequest()
    {
        await _leavePage.ApproveLeaveRequest("ANNUAL");
    }

    [Then(@"the leave status should change to ""(.*)""")]
    public async Task ThenTheLeaveStatusShouldChangeTo(string status)
    {
        var content = await _driver.Page.ContentAsync();
        Assert.That(content, Does.Contain(status).IgnoreCase,
            $"Expected leave status to be '{status}'");
    }
}
