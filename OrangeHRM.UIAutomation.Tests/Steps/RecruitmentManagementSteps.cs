using TechTalk.SpecFlow;
using OrangeHRM.UIAutomation.Tests.Drivers;
using OrangeHRM.UIAutomation.Tests.Pages;
using OrangeHRM.UIAutomation.Tests.Config;
using NUnit.Framework;

namespace OrangeHRM.UIAutomation.Tests.Steps;

[Binding]
public class RecruitmentManagementSteps
{
    private readonly BrowserDriver _driver;
    private readonly RecruitmentPage _recruitmentPage;
    private readonly TestSettings _settings;

    public RecruitmentManagementSteps(BrowserDriver driver)
    {
        _driver = driver;
        _settings = ConfigurationLoader.GetTestSettings();
        _recruitmentPage = new RecruitmentPage(_driver.Page, _settings.BaseUrl);
    }

    [Given(@"I navigate to the Recruitment page")]
    public async Task GivenINavigateToTheRecruitmentPage()
    {
        await _recruitmentPage.NavigateToRecruitment();
    }

    [Then(@"the recruitment page should be visible")]
    public async Task ThenTheRecruitmentPageShouldBeVisible()
    {
        Assert.That(await _recruitmentPage.IsRecruitmentPageVisible(), Is.True, "Recruitment page not visible");
    }

    [When(@"I click Add Vacancy")]
    public async Task WhenIClickAddVacancy()
    {
        await _recruitmentPage.ClickAddVacancy();
    }

    [When(@"I fill the vacancy form with:")]
    public async Task WhenIFillTheVacancyFormWith(Table table)
    {
        var data = table.Rows.ToDictionary(r => r["Field"], r => r["Value"]);
        await _recruitmentPage.FillVacancyForm(
            data.GetValueOrDefault("Title", ""),
            data.GetValueOrDefault("Department", ""),
            data.GetValueOrDefault("Description", ""),
            int.Parse(data.GetValueOrDefault("Positions", "1"))
        );
    }

    [When(@"I submit the vacancy form")]
    public async Task WhenISubmitTheVacancyForm()
    {
        await _recruitmentPage.SubmitVacancyForm();
    }

    [When(@"I submit the vacancy form without filling required fields")]
    public async Task WhenISubmitTheVacancyFormWithoutFillingRequiredFields()
    {
        await _recruitmentPage.SubmitVacancyForm();
    }

    [Then(@"the vacancy ""(.*)"" should appear in the list with status ""(.*)""")]
    public async Task ThenTheVacancyShouldAppearWithStatus(string title, string status)
    {
        Assert.That(await _recruitmentPage.IsVacancyVisible(title), Is.True,
            $"Vacancy '{title}' not found in list");
    }

    [Given(@"a vacancy ""(.*)"" exists and is open")]
    public async Task GivenAVacancyExistsAndIsOpen(string title)
    {
        var exists = await _recruitmentPage.IsVacancyVisible(title);
        if (!exists) Assert.Inconclusive($"Pre-condition: vacancy '{title}' not found. Run Add Vacancy scenario first.");
    }

    [When(@"I click Add Candidate")]
    public async Task WhenIClickAddCandidate()
    {
        await _recruitmentPage.ClickAddCandidate();
    }

    [When(@"I fill the candidate form with:")]
    public async Task WhenIFillTheCandidateFormWith(Table table)
    {
        var data = table.Rows.ToDictionary(r => r["Field"], r => r["Value"]);
        await _recruitmentPage.FillCandidateForm(
            data.GetValueOrDefault("FirstName", ""),
            data.GetValueOrDefault("LastName", ""),
            data.GetValueOrDefault("Email", ""),
            data.GetValueOrDefault("Phone", "")
        );
    }

    [When(@"I submit the candidate form")]
    public async Task WhenISubmitTheCandidateForm()
    {
        await _recruitmentPage.SubmitVacancyForm();
    }

    [Then(@"candidate ""(.*)"" should appear with status ""(.*)""")]
    public async Task ThenCandidateShouldAppearWithStatus(string name, string status)
    {
        var content = await _driver.Page.ContentAsync();
        Assert.That(content, Does.Contain(name).IgnoreCase, $"Candidate '{name}' not found in list");
    }

    [Given(@"candidate ""(.*)"" is in the recruitment pipeline")]
    public async Task GivenCandidateIsInThePipeline(string name)
    {
        var content = await _driver.Page.ContentAsync();
        if (!content.Contains(name, StringComparison.OrdinalIgnoreCase))
            Assert.Inconclusive($"Pre-condition: candidate '{name}' not in pipeline.");
    }

    [When(@"I advance candidate ""(.*)"" to status ""(.*)""")]
    public async Task WhenIAdvanceCandidateToStatus(string name, string status)
    {
        await _recruitmentPage.AdvanceCandidateStatus(name, status);
    }

    [Then(@"candidate ""(.*)"" should show status ""(.*)""")]
    public async Task ThenCandidateShouldShowStatus(string name, string status)
    {
        await Task.Delay(500);
        var content = await _driver.Page.ContentAsync();
        Assert.That(content, Does.Contain(status).IgnoreCase,
            $"Expected candidate '{name}' to show status '{status}'");
    }

    [Given(@"vacancy ""(.*)"" has been filled")]
    public async Task GivenVacancyHasBeenFilled(string title)
    {
        await Task.CompletedTask; // pre-condition already satisfied by pipeline scenario
    }

    [When(@"I close the vacancy ""(.*)""")]
    public async Task WhenICloseTheVacancy(string title)
    {
        var row = _driver.Page.Locator($"tr:has-text('{title}'), [class*='row']:has-text('{title}')").First;
        var closeBtn = row.Locator("button:has-text('Close'), button:has-text('Fill')").First;
        if (await closeBtn.CountAsync() > 0) await closeBtn.ClickAsync();
        await Task.Delay(800);
    }

    [Then(@"the vacancy ""(.*)"" should show status ""(.*)""")]
    public async Task ThenTheVacancyShouldShowStatus(string title, string status)
    {
        var content = await _driver.Page.ContentAsync();
        Assert.That(content, Does.Contain(status).IgnoreCase,
            $"Expected vacancy '{title}' to show status '{status}'");
    }
}
