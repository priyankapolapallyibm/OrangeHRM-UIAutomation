using Microsoft.Playwright;

namespace OrangeHRM.UIAutomation.Tests.Pages;

/// <summary>
/// POM for the PeopleFlow Login page (SPA — URL never changes after login).
/// The app renders .login-layout before login and .app-shell after login.
/// </summary>
public class LoginPage : BasePage
{
    // Exact IDs from the Vue template
    private ILocator UsernameInput => Page.Locator("#username");
    private ILocator PasswordInput => Page.Locator("#password");
    private ILocator LoginButton   => Page.Locator("button[type='submit']");
    private ILocator ErrorMessage  => Page.Locator("[role='alert'], .error-message");
    // The app-shell div is only present when logged in
    private ILocator AppShell      => Page.Locator(".app-shell");

    public LoginPage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public async Task NavigateToLogin()
    {
        await NavigateTo();
        await Page.WaitForSelectorAsync(".login-layout", new PageWaitForSelectorOptions { Timeout = 15000 });
    }

    public async Task Login(string username, string password)
    {
        await UsernameInput.ClearAsync();
        await UsernameInput.FillAsync(username);
        await PasswordInput.ClearAsync();
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
        // Wait for either app-shell (success) or error (failure) — max 5s
        try
        {
            await Page.WaitForSelectorAsync(".app-shell, [role='alert'], .error-message",
                new PageWaitForSelectorOptions { Timeout = 5000 });
        }
        catch { /* timeout — let caller assert */ }
    }

    public async Task<bool> IsLoginPageVisible()
    {
        try
        {
            await Page.WaitForSelectorAsync(".login-layout", new PageWaitForSelectorOptions { Timeout = 5000 });
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> IsLoggedIn()
    {
        // SPA: URL does not change; app-shell div appears after successful login
        try
        {
            await Page.WaitForSelectorAsync(".app-shell", new PageWaitForSelectorOptions { Timeout = 5000 });
            return true;
        }
        catch { return false; }
    }

    public async Task<string> GetErrorMessage()
    {
        try
        {
            await Page.WaitForSelectorAsync("[role='alert'], .error-message",
                new PageWaitForSelectorOptions { Timeout = 5000 });
            return await ErrorMessage.First.InnerTextAsync();
        }
        catch { return string.Empty; }
    }

    public async Task Logout()
    {
        try
        {
            // The app uses "Sign out" text on a secondary-button
            await Page.Locator("button:has-text('Sign out')").First.ClickAsync();
            await Page.WaitForSelectorAsync(".login-layout", new PageWaitForSelectorOptions { Timeout = 5000 });
        }
        catch { await NavigateTo(); }
    }
}
