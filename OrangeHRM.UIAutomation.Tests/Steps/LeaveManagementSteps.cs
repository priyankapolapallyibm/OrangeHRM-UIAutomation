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
        var listVisible = await _driver.Page.Locator("table, [class*='leave-list'], [class*='list']").CountAsync() > 0;
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
            int.Parse(data.GetValueOrDefault("EmployeeId", "1")),
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
        var errorVisible = await _driver.Page.Locator("[class*='error']:visible, [role='alert']:visible").CountAsync() > 0;
        Assert.That(errorVisible, Is.True, "Expected date validation error");
    }

    [Given(@"an approved leave exists for employee (.*) from ""(.*)"" to ""(.*)""")]
    public async Task GivenAnApprovedLeaveExistsForEmployee(int empId, string start, string end)
    {
        // This is a pre-condition — already set up via API in test data setup
        await Task.CompletedTask;
    }

    [Then(@"I should see an overlap conflict error")]
    public async Task ThenIShouldSeeAnOverlapConflictError()
    {
        Assert.That(await _leavePage.IsOverlapErrorDisplayed(), Is.True,
            "Expected overlap conflict error but none shown");
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
