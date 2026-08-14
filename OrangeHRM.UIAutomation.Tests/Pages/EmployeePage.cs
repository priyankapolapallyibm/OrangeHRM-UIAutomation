using Microsoft.Playwright;

namespace OrangeHRM.UIAutomation.Tests.Pages;

/// <summary>
/// POM for Employee Management page (.employees-page).
/// The form opens in a modal (.modal-backdrop > .employee-form) via "Add employee" button.
/// Labels wrap inputs: label:has-text('First name') input, etc.
/// Delete uses window.confirm — must accept the dialog.
/// </summary>
public class EmployeePage : BasePage
{
    // Header controls
    private ILocator AddEmployeeButton => Page.Locator("button:has-text('Add employee')").First;
    private ILocator SearchInput       => Page.Locator("#employee-search");
    private ILocator SearchButton      => Page.Locator("button:has-text('Search')").First;

    // Modal form (inside .employee-form)
    private ILocator EmployeeForm       => Page.Locator(".employee-form");
    private ILocator EmpNumberInput     => EmployeeForm.Locator("label:has-text('Employee number') input");
    private ILocator EmailInput         => EmployeeForm.Locator("label:has-text('Email') input");
    private ILocator FirstNameInput     => EmployeeForm.Locator("label:has-text('First name') input");
    private ILocator LastNameInput      => EmployeeForm.Locator("label:has-text('Last name') input");
    private ILocator JobTitleInput      => EmployeeForm.Locator("label:has-text('Job title') input");
    private ILocator DepartmentInput    => EmployeeForm.Locator("label:has-text('Department') input");
    private ILocator StatusSelect       => EmployeeForm.Locator("label:has-text('Employment status') select");
    private ILocator SaveButton         => EmployeeForm.Locator("button:has-text('Save employee')");
    private ILocator CancelButton       => EmployeeForm.Locator("button:has-text('Cancel')");

    // Page-level error
    private ILocator ErrorMessage => Page.Locator("[role='alert'], .error-message");

    public EmployeePage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public async Task NavigateToEmployees()
    {
        await DismissModalIfOpen();
        await Page.Locator("nav button:has-text('Employees')").First.ClickAsync();
        await Page.WaitForSelectorAsync("h1:has-text('Employees')", new PageWaitForSelectorOptions { Timeout = 10000 });
    }

    public async Task<bool> IsEmployeePageVisible()
    {
        try
        {
            await Page.WaitForSelectorAsync("h1:has-text('Employees')", new PageWaitForSelectorOptions { Timeout = 5000 });
            return true;
        }
        catch { return false; }
    }

    public async Task ClickAddEmployee()
    {
        await AddEmployeeButton.ClickAsync();
        await Page.WaitForSelectorAsync(".employee-form", new PageWaitForSelectorOptions { Timeout = 5000 });
    }

    public async Task FillEmployeeForm(string empNumber, string firstName, string lastName,
        string email, string jobTitle, string department, string status = "ACTIVE")
    {
        await EmpNumberInput.ClearAsync();
        await EmpNumberInput.FillAsync(empNumber);
        await EmailInput.ClearAsync();
        await EmailInput.FillAsync(email);
        await FirstNameInput.ClearAsync();
        await FirstNameInput.FillAsync(firstName);
        await LastNameInput.ClearAsync();
        await LastNameInput.FillAsync(lastName);
        await JobTitleInput.ClearAsync();
        await JobTitleInput.FillAsync(jobTitle);
        await DepartmentInput.ClearAsync();
        await DepartmentInput.FillAsync(department);
        await StatusSelect.SelectOptionAsync(new SelectOptionValue { Value = status });
    }

    public async Task SubmitEmployeeForm()
    {
        await SaveButton.ClickAsync();
        // Wait for modal to close (success) or error to appear
        try
        {
            await Page.WaitForSelectorAsync(".employee-form",
                new PageWaitForSelectorOptions { State = WaitForSelectorState.Detached, Timeout = 5000 });
        }
        catch { /* form might stay open on validation error */ }
        await Task.Delay(500);
    }

    public async Task<bool> IsEmployeeInList(string name)
    {
        await Task.Delay(500);
        return await Page.Locator($"table tbody tr td:has-text('{name}')").CountAsync() > 0;
    }

    public async Task SearchEmployee(string searchTerm)
    {
        await SearchInput.ClearAsync();
        await SearchInput.FillAsync(searchTerm);
        await SearchButton.ClickAsync();
        await Task.Delay(800);
    }

    public async Task<int> GetEmployeeCount()
    {
        await Task.Delay(300);
        return await Page.Locator("table tbody tr").CountAsync();
    }

    public async Task<bool> IsNoResultsMessageVisible()
    {
        return await Page.Locator("text=No employees match your search").CountAsync() > 0;
    }

    public async Task ClickEditEmployee(string name)
    {
        var row = Page.Locator($"table tbody tr:has-text('{name}')").First;
        await row.Locator("button:has-text('Edit')").First.ClickAsync();
        await Page.WaitForSelectorAsync(".employee-form", new PageWaitForSelectorOptions { Timeout = 5000 });
    }

    public async Task UpdateJobTitle(string jobTitle)
    {
        await JobTitleInput.ClearAsync();
        await JobTitleInput.FillAsync(jobTitle);
    }

    public async Task ClickDeleteEmployee(string name)
    {
        var row = Page.Locator($"table tbody tr:has-text('{name}')").First;
        // Handle window.confirm dialog — accept automatically
        Page.Dialog += async (_, dialog) => await dialog.AcceptAsync();
        await row.Locator("button:has-text('Delete')").First.ClickAsync();
        await Task.Delay(1500);
    }

    public async Task<bool> IsErrorVisible()
    {
        return await ErrorMessage.CountAsync() > 0;
    }
}
