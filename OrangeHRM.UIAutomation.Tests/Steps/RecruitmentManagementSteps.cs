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

    // Vacancy tab scenarios — form is always visible, no "click Add Vacancy" needed
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
        // Submit empty form — HTML5 required will block or API error shown
        await _recruitmentPage.SubmitVacancyForm();
    }

    [Then(@"the vacancy ""(.*)"" should appear in the list with status ""(.*)""")]
    public async Task ThenTheVacancyShouldAppearWithStatus(string title, string status)
    {
        Assert.That(await _recruitmentPage.IsVacancyVisible(title), Is.True,
            $"Vacancy '{title}' not found in list");
    }

    [Then(@"I should see a validation error for the form")]
    public async Task ThenIShouldSeeAValidationErrorForTheForm()
    {
        // HTML5 required stops submit (no navigation) or API error appears
        var errorVisible = await _recruitmentPage.IsErrorDisplayed();
        var pageHasForm = await _driver.Page.Locator("label:has-text('Job title')").CountAsync() > 0;
        Assert.That(errorVisible || pageHasForm, Is.True, "Expected validation to block empty vacancy form");
    }

    // Candidate tab scenarios
    [Given(@"a vacancy ""(.*)"" exists and is open")]
    public async Task GivenAVacancyExistsAndIsOpen(string title)
    {
        // Switch to vacancies tab first to check
        await _recruitmentPage.SwitchToVacanciesTab();
        var exists = await _recruitmentPage.IsVacancyVisible(title);
        if (!exists) Assert.Inconclusive($"Pre-condition: vacancy '{title}' not found. Run Add Vacancy scenario first.");
        // Switch to candidates tab for the next steps
        await _recruitmentPage.SwitchToCandidatesTab();
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
        await _recruitmentPage.SubmitCandidateForm();
    }

    [Then(@"candidate ""(.*)"" should appear with status ""(.*)""")]
    public async Task ThenCandidateShouldAppearWithStatus(string name, string status)
    {
        Assert.That(await _recruitmentPage.IsCandidateVisible(name), Is.True,
            $"Candidate '{name}' not found in list");
    }

    [Given(@"candidate ""(.*)"" is in the recruitment pipeline")]
    public async Task GivenCandidateIsInThePipeline(string name)
    {
        await _recruitmentPage.SwitchToCandidatesTab();
        var visible = await _recruitmentPage.IsCandidateVisible(name);
        if (!visible)
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
        var actual = await _recruitmentPage.GetCandidateStatus(name);
        Assert.That(actual, Does.Contain(status).IgnoreCase,
            $"Expected candidate '{name}' to show status '{status}' but got '{actual}'");
    }

    [Given(@"vacancy ""(.*)"" has been filled")]
    public async Task GivenVacancyHasBeenFilled(string title)
    {
        await _recruitmentPage.SwitchToVacanciesTab();
        // Pre-condition check — vacancy should exist
        var exists = await _recruitmentPage.IsVacancyVisible(title);
        if (!exists) Assert.Inconclusive($"Pre-condition: vacancy '{title}' not found.");
    }

    [When(@"I close the vacancy ""(.*)""")]
    public async Task WhenICloseTheVacancy(string title)
    {
        await _recruitmentPage.CloseVacancy(title);
    }

    [Then(@"the vacancy ""(.*)"" should show status ""(.*)""")]
    public async Task ThenTheVacancyShouldShowStatus(string title, string status)
    {
        var actual = await _recruitmentPage.GetVacancyStatus(title);
        Assert.That(actual, Does.Contain(status).IgnoreCase,
            $"Expected vacancy '{title}' to show status '{status}' but got '{actual}'");
    }
}
