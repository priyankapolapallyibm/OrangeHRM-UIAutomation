using Microsoft.Playwright;

namespace OrangeHRM.UIAutomation.Tests.Pages;

/// <summary>
/// POM for Admin / User Management page.
/// US: List Users, Create Account, Disable/Enable, Role Management.
/// </summary>
public class AdminPage : BasePage
{
    private ILocator AddUserButton    => Page.Locator("button:has-text('Add User'), button:has-text('New User'), button:has-text('Add')").First;
    private ILocator UsernameInput    => Page.Locator("input[placeholder*='username' i], input[name*='username' i]").First;
    private ILocator PasswordInput    => Page.Locator("input[type='password']").First;
    private ILocator DisplayNameInput => Page.Locator("input[placeholder*='display' i], input[name*='displayName' i], input[placeholder*='name' i]").First;
    private ILocator RoleSelect       => Page.Locator("select[name*='role' i], select[id*='role' i]").First;
    private ILocator SaveButton       => Page.Locator("button[type='submit']:visible, button:has-text('Save'):visible, button:has-text('Create'):visible").Last;
    private ILocator UserTable        => Page.Locator("table tbody, [class*='user-list']").First;

    public AdminPage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public async Task NavigateToAdmin()
    {
        await ClickNavTab("Admin");
        await Task.Delay(1000);
    }

    public async Task<bool> IsAdminPageVisible()
    {
        await Task.Delay(500);
        return await Page.Locator("[class*='admin'], [class*='user-management'], button:has-text('Add User')").CountAsync() > 0;
    }

    public async Task ClickAddUser()
    {
        await AddUserButton.ClickAsync();
        await Task.Delay(600);
    }

    public async Task FillUserForm(string username, string password, string displayName, string role = "EMPLOYEE")
    {
        await UsernameInput.FillAsync(username);
        await PasswordInput.FillAsync(password);
        if (await DisplayNameInput.CountAsync() > 0)
            await DisplayNameInput.FillAsync(displayName);
        if (await RoleSelect.CountAsync() > 0)
            await RoleSelect.SelectOptionAsync(new SelectOptionValue { Value = role });
    }

    public async Task SubmitUserForm()
    {
        await SaveButton.ClickAsync();
        await Task.Delay(1500);
    }

    public async Task<bool> IsUserInList(string username)
    {
        await Task.Delay(500);
        return await Page.Locator($"text={username}").CountAsync() > 0;
    }

    public async Task<bool> IsPasswordHashExposed()
    {
        var content = await Page.ContentAsync();
        return content.Contains("passwordHash", StringComparison.OrdinalIgnoreCase);
    }

    public async Task DisableUser(string username)
    {
        var row = Page.Locator($"tr:has-text('{username}'), [class*='row']:has-text('{username}')").First;
        var disableBtn = row.Locator("button:has-text('Disable'), button:has-text('Deactivate'), input[type='checkbox']").First;
        if (await disableBtn.CountAsync() > 0)
            await disableBtn.ClickAsync();
        await Task.Delay(1000);
    }

    public async Task<string> GetUserStatus(string username)
    {
        var row = Page.Locator($"tr:has-text('{username}'), [class*='row']:has-text('{username}')").First;
        if (await row.CountAsync() > 0)
        {
            var badge = row.Locator("[class*='badge'], [class*='status'], [class*='active'], td").Last;
            return await badge.InnerTextAsync();
        }
        return string.Empty;
    }

    public async Task<bool> IsErrorDisplayed(string errorText)
    {
        var err = Page.Locator("[class*='error']:visible, [role='alert']:visible, [class*='danger']:visible");
        if (await err.CountAsync() > 0)
        {
            var text = await err.First.InnerTextAsync();
            return text.Contains(errorText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}
