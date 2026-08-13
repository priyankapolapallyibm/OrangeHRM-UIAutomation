using Microsoft.Playwright;

namespace OrangeHRM.UIAutomation.Tests.Pages;

/// <summary>
/// POM for Recruitment Management page.
/// US: Manage Vacancies, Candidate Pipeline, Hire workflow.
/// </summary>
public class RecruitmentPage : BasePage
{
    private ILocator AddVacancyButton    => Page.Locator("button:has-text('Add Vacancy'), button:has-text('New Vacancy'), button:has-text('Add')").First;
    private ILocator TitleInput          => Page.Locator("input[placeholder*='title' i], input[name*='title' i]").First;
    private ILocator DepartmentInput     => Page.Locator("input[placeholder*='department' i], input[name*='department' i]").First;
    private ILocator DescriptionInput    => Page.Locator("textarea, input[placeholder*='description' i]").First;
    private ILocator PositionsInput      => Page.Locator("input[type='number'], input[placeholder*='position' i]").First;
    private ILocator SaveButton          => Page.Locator("button[type='submit']:visible, button:has-text('Save'):visible").Last;
    private ILocator AddCandidateButton  => Page.Locator("button:has-text('Add Candidate'), button:has-text('Apply')").First;

    public RecruitmentPage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public async Task NavigateToRecruitment()
    {
        await ClickNavTab("Recruitment");
        await Task.Delay(1000);
    }

    public async Task<bool> IsRecruitmentPageVisible()
    {
        await Task.Delay(500);
        return await Page.Locator("[class*='recruit'], button:has-text('Add Vacancy'), button:has-text('Vacancy')").CountAsync() > 0;
    }

    public async Task ClickAddVacancy()
    {
        await AddVacancyButton.ClickAsync();
        await Task.Delay(600);
    }

    public async Task FillVacancyForm(string title, string department, string description, int positions)
    {
        await TitleInput.FillAsync(title);
        await DepartmentInput.FillAsync(department);
        if (await DescriptionInput.CountAsync() > 0)
            await DescriptionInput.FillAsync(description);
        if (await PositionsInput.CountAsync() > 0)
        {
            await PositionsInput.ClearAsync();
            await PositionsInput.FillAsync(positions.ToString());
        }
    }

    public async Task SubmitVacancyForm()
    {
        await SaveButton.ClickAsync();
        await Task.Delay(1500);
    }

    public async Task<bool> IsVacancyVisible(string title)
    {
        await Task.Delay(500);
        return await Page.Locator($"text={title}").CountAsync() > 0;
    }

    public async Task ClickAddCandidate()
    {
        await AddCandidateButton.ClickAsync();
        await Task.Delay(600);
    }

    public async Task FillCandidateForm(string firstName, string lastName, string email, string phone = "")
    {
        await FillInputByPlaceholder("first name", firstName);
        await FillInputByPlaceholder("last name", lastName);
        await FillInputByPlaceholder("email", email);
        if (!string.IsNullOrEmpty(phone))
            await FillInputByPlaceholder("phone", phone);
    }

    public async Task AdvanceCandidateStatus(string candidateName, string newStatus)
    {
        var row = Page.Locator($"tr:has-text('{candidateName}'), [class*='row']:has-text('{candidateName}')").First;
        var statusSelect = row.Locator("select, button:has-text('Advance'), button:has-text('Move')").First;
        if (await statusSelect.CountAsync() > 0)
        {
            if (await statusSelect.EvaluateAsync<string>("el => el.tagName") == "SELECT")
                await statusSelect.SelectOptionAsync(new SelectOptionValue { Value = newStatus });
            else
                await statusSelect.ClickAsync();
        }
        await Task.Delay(800);
    }

    public async Task<string> GetCandidateStatus(string candidateName)
    {
        var row = Page.Locator($"tr:has-text('{candidateName}'), [class*='row']:has-text('{candidateName}')").First;
        if (await row.CountAsync() > 0)
        {
            var badge = row.Locator("[class*='badge'], [class*='status'], td").Last;
            return await badge.InnerTextAsync();
        }
        return string.Empty;
    }
}
