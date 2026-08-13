using Microsoft.Playwright;

namespace OrangeHRM.UIAutomation.Tests.Pages;

/// <summary>
/// POM for the Login page.
/// US: Authentication — login with valid/invalid credentials.
/// </summary>
public class LoginPage : BasePage
{
    // Locators
    private ILocator UsernameInput => Page.Locator("input[type='text'], input[placeholder*='username' i], input[name*='username' i]").First;
    private ILocator PasswordInput => Page.Locator("input[type='password']").First;
    private ILocator LoginButton   => Page.Locator("button[type='submit'], button:has-text('Login'), button:has-text('Sign In')").First;
    private ILocator ErrorMessage  => Page.Locator("[class*='error'], [class*='alert'], [role='alert']").First;
    private ILocator WelcomeText   => Page.Locator("h1, h2, [class*='dashboard'], [class*='welcome']").First;

    public LoginPage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public async Task NavigateToLogin()
    {
        await NavigateTo();
        await Page.WaitForSelectorAsync("input[type='password']", new PageWaitForSelectorOptions { Timeout = 15000 });
    }

    public async Task Login(string username, string password)
    {
        await UsernameInput.ClearAsync();
        await UsernameInput.FillAsync(username);
        await PasswordInput.ClearAsync();
        await PasswordInput.FillAsync(password);
        await LoginButton.ClickAsync();
        await Task.Delay(1500);
    }

    public async Task<bool> IsLoginPageVisible()
    {
        try
        {
            await Page.WaitForSelectorAsync("input[type='password']", new PageWaitForSelectorOptions { Timeout = 5000 });
            return true;
        }
        catch { return false; }
    }

    public async Task<bool> IsLoggedIn()
    {
        await Task.Delay(1000);
        // Check URL changed or dashboard element visible
        var url = Page.Url;
        if (url.Contains("dashboard") || url.Contains("home")) return true;

        var nav = Page.Locator("nav button, [class*='nav']");
        return await nav.CountAsync() > 0;
    }

    public async Task<string> GetErrorMessage()
    {
        try
        {
            await Page.WaitForSelectorAsync("[class*='error'], [class*='alert'], [role='alert']",
                new PageWaitForSelectorOptions { Timeout = 5000 });
            return await ErrorMessage.InnerTextAsync();
        }
        catch { return string.Empty; }
    }

    public async Task Logout()
    {
        try
        {
            await Page.Locator("button:has-text('Logout'), a:has-text('Logout'), [class*='logout']").First.ClickAsync();
            await Task.Delay(1000);
        }
        catch { await NavigateTo(); }
    }
}
