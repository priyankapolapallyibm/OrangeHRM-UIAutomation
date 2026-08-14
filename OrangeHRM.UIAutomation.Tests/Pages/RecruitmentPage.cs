using Microsoft.Playwright;

namespace OrangeHRM.UIAutomation.Tests.Pages;

/// <summary>
/// POM for Recruitment Management page (.module-page, h1 "Recruitment").
/// Has two tabs via .segmented buttons: "Vacancies" and "Candidates".
/// Vacancy form: inline, labels "Job title", "Department", "Positions", "Description", button "Add vacancy".
/// Candidate form: labels "Vacancy" (select), "First name", "Last name", "Email", "Phone", button "Add candidate".
/// Candidate pipeline: status select per row (APPLIED→SHORTLISTED→INTERVIEW→OFFERED→HIRED/REJECTED).
/// </summary>
public class RecruitmentPage : BasePage
{
    // Tab buttons
    private ILocator VacanciesTab    => Page.Locator(".segmented button:has-text('Vacancies')");
    private ILocator CandidatesTab   => Page.Locator(".segmented button:has-text('Candidates')");
    private ILocator ErrorAlert      => Page.Locator("[role='alert'], .error-message");

    // Vacancy form (always visible on Vacancies tab)
    private ILocator VacancyTitleInput       => Page.Locator("label:has-text('Job title') input").First;
    private ILocator VacancyDepartmentInput  => Page.Locator("label:has-text('Department') input").First;
    private ILocator VacancyPositionsInput   => Page.Locator("label:has-text('Positions') input").First;
    private ILocator VacancyDescriptionInput => Page.Locator("label:has-text('Description') textarea").First;
    private ILocator AddVacancyButton        => Page.Locator("button:has-text('Add vacancy')").First;

    // Candidate form (visible on Candidates tab)
    private ILocator CandidateVacancySelect  => Page.Locator("label:has-text('Vacancy') select").First;
    private ILocator CandidateFirstName      => Page.Locator("label:has-text('First name') input").First;
    private ILocator CandidateLastName       => Page.Locator("label:has-text('Last name') input").First;
    private ILocator CandidateEmail          => Page.Locator("label:has-text('Email') input").First;
    private ILocator CandidatePhone          => Page.Locator("label:has-text('Phone') input").First;
    private ILocator AddCandidateButton      => Page.Locator("button:has-text('Add candidate')").First;

    public RecruitmentPage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public async Task NavigateToRecruitment()
    {
        await DismissModalIfOpen();
        await Page.Locator("nav button:has-text('Recruitment')").First.ClickAsync();
        await Page.WaitForSelectorAsync("h1:has-text('Recruitment')", new PageWaitForSelectorOptions { Timeout = 10000 });
    }

    public async Task<bool> IsRecruitmentPageVisible()
    {
        try
        {
            await Page.WaitForSelectorAsync("h1:has-text('Recruitment')", new PageWaitForSelectorOptions { Timeout = 5000 });
            return true;
        }
        catch { return false; }
    }

    public async Task SwitchToVacanciesTab()
    {
        await VacanciesTab.ClickAsync();
        await Task.Delay(400);
    }

    public async Task SwitchToCandidatesTab()
    {
        await CandidatesTab.ClickAsync();
        await Task.Delay(400);
    }

    public async Task FillVacancyForm(string title, string department, string description, int positions = 1)
    {
        await VacancyTitleInput.ClearAsync();
        await VacancyTitleInput.FillAsync(title);
        await VacancyDepartmentInput.ClearAsync();
        await VacancyDepartmentInput.FillAsync(department);
        await VacancyPositionsInput.ClearAsync();
        await VacancyPositionsInput.FillAsync(positions.ToString());
        await VacancyDescriptionInput.ClearAsync();
        await VacancyDescriptionInput.FillAsync(description);
    }

    public async Task SubmitVacancyForm()
    {
        await AddVacancyButton.ClickAsync();
        await Task.Delay(1500);
    }

    public async Task<bool> IsVacancyVisible(string title)
    {
        await Task.Delay(400);
        return await Page.Locator($".record-card h2:has-text('{title}')").CountAsync() > 0;
    }

    public async Task CloseVacancy(string title)
    {
        var card = Page.Locator($".record-card:has-text('{title}')").First;
        var btn = card.Locator("button:has-text('Close vacancy')").First;
        if (await btn.CountAsync() > 0)
            await btn.ClickAsync();
        await Task.Delay(1000);
    }

    public async Task<string> GetVacancyStatus(string title)
    {
        var card = Page.Locator($".record-card:has-text('{title}')").First;
        if (await card.CountAsync() > 0)
        {
            var badge = card.Locator(".record-status").First;
            if (await badge.CountAsync() > 0)
                return await badge.InnerTextAsync();
        }
        return string.Empty;
    }

    public async Task FillCandidateForm(string firstName, string lastName, string email, string phone = "")
    {
        await CandidateFirstName.ClearAsync();
        await CandidateFirstName.FillAsync(firstName);
        await CandidateLastName.ClearAsync();
        await CandidateLastName.FillAsync(lastName);
        await CandidateEmail.ClearAsync();
        await CandidateEmail.FillAsync(email);
        if (!string.IsNullOrEmpty(phone))
        {
            await CandidatePhone.ClearAsync();
            await CandidatePhone.FillAsync(phone);
        }
    }

    public async Task SubmitCandidateForm()
    {
        await AddCandidateButton.ClickAsync();
        await Task.Delay(1500);
    }

    public async Task<bool> IsCandidateVisible(string name)
    {
        await Task.Delay(400);
        return await Page.Locator($"table tbody tr td:has-text('{name}')").CountAsync() > 0;
    }

    public async Task AdvanceCandidateStatus(string candidateName, string newStatus)
    {
        var row = Page.Locator($"table tbody tr:has-text('{candidateName}')").First;
        if (await row.CountAsync() > 0)
        {
            var statusSelect = row.Locator("select").First;
            if (await statusSelect.CountAsync() > 0)
                await statusSelect.SelectOptionAsync(new SelectOptionValue { Value = newStatus });
        }
        await Task.Delay(1000);
    }

    public async Task<string> GetCandidateStatus(string candidateName)
    {
        var row = Page.Locator($"table tbody tr:has-text('{candidateName}')").First;
        if (await row.CountAsync() > 0)
        {
            var statusSelect = row.Locator("select").First;
            if (await statusSelect.CountAsync() > 0)
                return await statusSelect.InputValueAsync();
        }
        return string.Empty;
    }

    public async Task<bool> IsErrorDisplayed()
    {
        return await ErrorAlert.CountAsync() > 0;
    }
}
