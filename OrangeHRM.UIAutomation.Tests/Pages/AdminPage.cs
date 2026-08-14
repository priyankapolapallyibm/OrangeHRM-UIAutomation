using Microsoft.Playwright;

namespace OrangeHRM.UIAutomation.Tests.Pages;

/// <summary>
/// POM for Admin / User Management page.
/// The admin nav button is in the sidebar: button text "Admin"
/// The page shows h1 "User Accounts" and an inline form + table.
/// </summary>
public class AdminPage : BasePage
{
    // Inline create-account form (label-wrapped inputs)
    private ILocator UsernameInput    => Page.Locator("label:has-text('Username') input").First;
    private ILocator DisplayNameInput => Page.Locator("label:has-text('Display name') input").First;
    private ILocator PasswordInput    => Page.Locator("label:has-text('Temporary password') input").First;
    private ILocator RoleSelect       => Page.Locator("label:has-text('Role') select").First;
    private ILocator CreateButton     => Page.Locator("button:has-text('Create account')").First;
    private ILocator ErrorAlert       => Page.Locator("[role='alert'], .error-message");
    private ILocator AccountTable     => Page.Locator("table tbody");

    public AdminPage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public async Task NavigateToAdmin()
    {
        await DismissModalIfOpen();
        // Sidebar nav button with exact text "Admin"
        await Page.Locator("nav button:has-text('Admin')").First.ClickAsync();
        // Wait for the "User Accounts" heading to confirm page loaded
        await Page.WaitForSelectorAsync("h1:has-text('User Accounts')", new PageWaitForSelectorOptions { Timeout = 10000 });
    }

    public async Task<bool> IsAdminPageVisible()
    {
        try
        {
            await Page.WaitForSelectorAsync("h1:has-text('User Accounts')", new PageWaitForSelectorOptions { Timeout = 5000 });
            return true;
        }
        catch { return false; }
    }

    public async Task FillUserForm(string username, string password, string displayName, string role = "EMPLOYEE")
    {
        await UsernameInput.ClearAsync();
        await UsernameInput.FillAsync(username);
        await DisplayNameInput.ClearAsync();
        await DisplayNameInput.FillAsync(displayName);
        await PasswordInput.ClearAsync();
        await PasswordInput.FillAsync(password);
        await RoleSelect.SelectOptionAsync(new SelectOptionValue { Value = role });
    }

    public async Task SubmitUserForm()
    {
        await CreateButton.ClickAsync();
        // Wait for table reload or error
        await Task.Delay(1500);
    }

    public async Task<bool> IsUserInList(string username)
    {
        await Task.Delay(500);
        return await Page.Locator($"table tbody tr td:has-text('{username}')").CountAsync() > 0;
    }

    public async Task<bool> IsPasswordHashExposed()
    {
        var content = await Page.ContentAsync();
        return content.Contains("passwordHash", StringComparison.OrdinalIgnoreCase);
    }

    public async Task DisableUser(string username)
    {
        // Find the row containing the username, click its Disable button
        var row = Page.Locator($"table tbody tr:has-text('{username}')").First;
        if (await row.CountAsync() > 0)
        {
            var btn = row.Locator("button:has-text('Disable'), button:has-text('Enable')").First;
            if (await btn.CountAsync() > 0)
                await btn.ClickAsync();
        }
        await Task.Delay(1000);
    }

    public async Task<string> GetUserStatus(string username)
    {
        var row = Page.Locator($"table tbody tr:has-text('{username}')").First;
        if (await row.CountAsync() > 0)
        {
            // Status cell has span.record-status with text ACTIVE or DISABLED
            var badge = row.Locator(".record-status").First;
            if (await badge.CountAsync() > 0)
                return await badge.InnerTextAsync();
        }
        return string.Empty;
    }

    public async Task<bool> IsSelfDisableButtonDisabled(string username)
    {
        // The app disables the button via :disabled="account.id === currentUserId"
        var row = Page.Locator($"table tbody tr:has-text('{username}')").First;
        if (await row.CountAsync() > 0)
        {
            var btn = row.Locator("button:has-text('Disable'), button:has-text('Enable')").First;
            if (await btn.CountAsync() > 0)
            {
                var disabled = await btn.GetAttributeAsync("disabled");
                return disabled != null;
            }
        }
        return false;
    }

    public async Task<bool> IsErrorDisplayed(string errorText)
    {
        if (await ErrorAlert.CountAsync() > 0)
        {
            var text = await ErrorAlert.First.InnerTextAsync();
            return text.Contains(errorText, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}
