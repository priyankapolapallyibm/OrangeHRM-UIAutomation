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
        // CI app takes longer to serve — wait up to 30s
        await Page.WaitForSelectorAsync(".login-layout", new PageWaitForSelectorOptions { Timeout = 30000 });
    }

    public async Task Login(string username, string password)
    {
        await UsernameInput.ClearAsync();
        await UsernameInput.FillAsync(username);
        await PasswordInput.ClearAsync();
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
        // Wait for either app-shell (success) or error (failure) — 30s for CI
        try
        {
            await Page.WaitForSelectorAsync(".app-shell, [role='alert'], .error-message",
                new PageWaitForSelectorOptions { Timeout = 30000 });
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
        // Also confirm nav buttons are rendered (app-shell may flash briefly)
        try
        {
            await Page.WaitForSelectorAsync(".app-shell", new PageWaitForSelectorOptions { Timeout = 30000 });
            // Wait for at least one nav button to be rendered inside app-shell
            await Page.WaitForSelectorAsync(".app-shell nav button", new PageWaitForSelectorOptions { Timeout = 15000 });
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
            var signOutButton = Page.Locator("button:has-text('Sign out')").First;
            
            // Wait up to 2s for button to be available
            if (await signOutButton.CountAsync() > 0)
            {
                await signOutButton.ClickAsync();
                // Wait for login layout or just verify we're off dashboard
                try
                {
                    await Page.WaitForSelectorAsync(".login-layout, .login-form", 
                        new PageWaitForSelectorOptions { Timeout = 5000 });
                }
                catch { /* If login page doesn't appear, logout might have worked anyway */ }
            }
            else
            {
                // Sign out button not found — session might already be expired
                // Try to navigate to login directly
                await NavigateTo();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LOGOUT WARNING] Logout attempt failed: {ex.Message} — attempting fallback navigation");
            try
            {
                await NavigateTo();
            }
            catch { /* Last resort failed, but we'll continue anyway */ }
        }
    }
}
