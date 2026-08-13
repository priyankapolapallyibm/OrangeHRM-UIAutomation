using Microsoft.Playwright;

namespace OrangeHRM.UIAutomation.Tests.Pages;

/// <summary>
/// POM for Employee Management page.
/// US: Employee List/Search, Add Employee, Edit/Delete Employee.
/// </summary>
public class EmployeePage : BasePage
{
    private ILocator AddEmployeeButton   => Page.Locator("button:has-text('Add Employee'), button:has-text('Add')").First;
    private ILocator SearchInput         => Page.Locator("input[placeholder*='search' i], input[type='search']").First;
    private ILocator EmployeeTable       => Page.Locator("table tbody, [class*='employee-list'], [class*='table']").First;
    private ILocator SaveButton          => Page.Locator("button[type='submit']:visible, button:has-text('Save'):visible, button:has-text('Add'):visible").Last;
    private ILocator CancelButton        => Page.Locator("button:has-text('Cancel'), button:has-text('Close')").First;
    private ILocator SuccessToast        => Page.Locator("[class*='success'], [class*='toast'], [role='alert']").First;
    private ILocator ErrorToast          => Page.Locator("[class*='error'], [class*='danger'], [role='alert']").First;

    public EmployeePage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public async Task NavigateToEmployees()
    {
        await ClickNavTab("Employees");
        await Task.Delay(1000);
    }

    public async Task<bool> IsEmployeePageVisible()
    {
        await Task.Delay(500);
        return await Page.Locator("button:has-text('Add Employee'), button:has-text('Add'), [class*='employee']").CountAsync() > 0;
    }

    public async Task ClickAddEmployee()
    {
        await AddEmployeeButton.ClickAsync();
        await Task.Delay(600);
    }

    public async Task FillEmployeeForm(string empNumber, string firstName, string lastName,
        string email, string jobTitle, string department, string status = "ACTIVE")
    {
        await FillInputByPlaceholder("employee number", empNumber);
        await FillInputByPlaceholder("first name", firstName);
        await FillInputByPlaceholder("last name", lastName);
        await FillInputByPlaceholder("email", email);
        await FillInputByPlaceholder("job title", jobTitle);
        await FillInputByPlaceholder("department", department);

        // Status dropdown
        var statusSelect = Page.Locator("select[name*='status' i], select[id*='status' i]").First;
        if (await statusSelect.CountAsync() > 0)
            await statusSelect.SelectOptionAsync(new SelectOptionValue { Value = status });
    }

    public async Task SubmitEmployeeForm()
    {
        await SaveButton.ClickAsync();
        await Task.Delay(1500);
    }

    public async Task<bool> IsEmployeeInList(string name)
    {
        await Task.Delay(500);
        var content = await Page.ContentAsync();
        return content.Contains(name, StringComparison.OrdinalIgnoreCase);
    }

    public async Task SearchEmployee(string searchTerm)
    {
        if (await SearchInput.CountAsync() > 0)
        {
            await SearchInput.ClearAsync();
            await SearchInput.FillAsync(searchTerm);
            await Task.Delay(800);
        }
    }

    public async Task<int> GetEmployeeCount()
    {
        await Task.Delay(500);
        var rows = Page.Locator("table tbody tr, [class*='employee-row'], [class*='list-item']");
        return await rows.CountAsync();
    }

    public async Task ClickEditEmployee(string name)
    {
        var row = Page.Locator($"tr:has-text('{name}'), [class*='row']:has-text('{name}')").First;
        var editBtn = row.Locator("button:has-text('Edit'), button[title*='edit' i], [class*='edit']").First;
        if (await editBtn.CountAsync() > 0)
            await editBtn.ClickAsync();
        else
        {
            // Try clicking the row itself
            await row.ClickAsync();
        }
        await Task.Delay(600);
    }

    public async Task UpdateEmployeeField(string field, string value)
    {
        var input = Page.Locator($"input[placeholder*='{field}' i]:visible, input[name*='{field}' i]:visible").First;
        if (await input.CountAsync() > 0)
        {
            await input.ClearAsync();
            await input.FillAsync(value);
        }
    }

    public async Task ClickDeleteEmployee(string name)
    {
        var row = Page.Locator($"tr:has-text('{name}'), [class*='row']:has-text('{name}')").First;
        var deleteBtn = row.Locator("button:has-text('Delete'), button[title*='delete' i], [class*='delete']").First;
        if (await deleteBtn.CountAsync() > 0)
            await deleteBtn.ClickAsync();
        await Task.Delay(400);
        // Confirm dialog if present
        var confirmBtn = Page.Locator("button:has-text('Confirm'), button:has-text('Yes'), button:has-text('Delete'):visible").First;
        if (await confirmBtn.CountAsync() > 0)
            await confirmBtn.ClickAsync();
        await Task.Delay(1000);
    }

    public async Task<string> GetFormValidationError()
    {
        var err = Page.Locator("[class*='error']:visible, [class*='invalid']:visible, .validation-error:visible").First;
        if (await err.CountAsync() > 0)
            return await err.InnerTextAsync();
        return string.Empty;
    }
}
