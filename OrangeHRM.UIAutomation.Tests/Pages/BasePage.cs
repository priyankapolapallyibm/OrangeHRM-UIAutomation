using Microsoft.Playwright;

namespace OrangeHRM.UIAutomation.Tests.Pages;

/// <summary>
/// Base page object — all pages inherit from this.
/// Provides common utilities: navigation, wait helpers, screenshot.
/// </summary>
public abstract class BasePage
{
    protected readonly IPage Page;
    protected readonly string BaseUrl;

    protected BasePage(IPage page, string baseUrl)
    {
        Page = page;
        BaseUrl = baseUrl;
    }

    protected async Task NavigateTo(string path = "")
    {
        // Use DOMContentLoaded — NetworkIdle never fires reliably for Vite SPA in CI
        await Page.GotoAsync($"{BaseUrl}{path}", new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
    }

    protected async Task ClickNavTab(string tabName)
    {
        // Close any open modal first
        await DismissModalIfOpen();
        await Page.EvaluateAsync($@"
            const btns = [...document.querySelectorAll('nav button, nav a, header button')];
            const b = btns.find(el => el.textContent.trim().toLowerCase().includes('{tabName.ToLower()}'));
            if (b) b.click();
        ");
        await Task.Delay(500);
    }

    protected async Task DismissModalIfOpen()
    {
        try
        {
            var backdrop = Page.Locator(".modal-backdrop, [class*='backdrop'], [class*='overlay']");
            if (await backdrop.CountAsync() > 0)
            {
                await Page.Keyboard.PressAsync("Escape");
                await Task.Delay(400);
            }
        }
        catch { /* ignore */ }
    }

    protected async Task FillInputByPlaceholder(string placeholder, string value)
    {
        var input = Page.Locator($"input[placeholder*='{placeholder}' i]");
        await input.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible });
        await input.ClearAsync();
        await input.FillAsync(value);
    }

    protected async Task FillInputByLabel(string label, string value)
    {
        var input = Page.Locator($"label:has-text('{label}') + * input, label:has-text('{label}') ~ * input").First;
        if (await input.CountAsync() > 0)
        {
            await input.ClearAsync();
            await input.FillAsync(value);
        }
    }

    protected async Task SelectDropdownByLabel(string label, string value)
    {
        var select = Page.Locator($"label:has-text('{label}') + select, label:has-text('{label}') ~ select").First;
        if (await select.CountAsync() > 0)
            await select.SelectOptionAsync(new SelectOptionValue { Label = value });
    }

    protected async Task ClickButton(string text)
    {
        await Page.Locator($"button:has-text('{text}')").First.ClickAsync();
        await Task.Delay(300);
    }

    protected async Task<string> GetToastMessage()
    {
        var toast = Page.Locator("[class*='toast'], [class*='alert'], [class*='notification'], [role='alert']").First;
        if (await toast.CountAsync() > 0)
            return await toast.InnerTextAsync();
        return string.Empty;
    }

    protected async Task WaitForTableLoad()
    {
        await Page.WaitForSelectorAsync("table tbody tr, [class*='table'] [class*='row'], [class*='list'] [class*='item']",
            new PageWaitForSelectorOptions { Timeout = 10000 });
    }
}
