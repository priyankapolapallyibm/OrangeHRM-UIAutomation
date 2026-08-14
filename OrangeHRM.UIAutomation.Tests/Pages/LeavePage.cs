using Microsoft.Playwright;

namespace OrangeHRM.UIAutomation.Tests.Pages;

/// <summary>
/// POM for Leave Management page (.module-page, h1 "Leave Management").
/// Form opens inline via "New request" button toggle.
/// Labels: Employee (select), Leave type (select), Start date (date), End date (date), Reason (textarea).
/// </summary>
public class LeavePage : BasePage
{
    private ILocator NewRequestButton => Page.Locator("button:has-text('New request'), button:has-text('Close form')").First;
    private ILocator EmployeeSelect   => Page.Locator("label:has-text('Employee') select").First;
    private ILocator LeaveTypeSelect  => Page.Locator("label:has-text('Leave type') select").First;
    private ILocator StartDateInput   => Page.Locator("label:has-text('Start date') input[type='date']").First;
    private ILocator EndDateInput     => Page.Locator("label:has-text('End date') input[type='date']").First;
    private ILocator ReasonInput      => Page.Locator("label:has-text('Reason') textarea").First;
    private ILocator SubmitButton     => Page.Locator("button:has-text('Submit request')").First;
    private ILocator ErrorAlert       => Page.Locator("[role='alert'], .error-message");

    public LeavePage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public async Task NavigateToLeave()
    {
        await DismissModalIfOpen();
        await Page.Locator("nav button:has-text('Leave')").First.ClickAsync();
        await Page.WaitForSelectorAsync("h1:has-text('Leave Management')", new PageWaitForSelectorOptions { Timeout = 15000 });
    }

    public async Task<bool> IsLeavePageVisible()
    {
        try
        {
            await Page.WaitForSelectorAsync("h1:has-text('Leave Management')", new PageWaitForSelectorOptions { Timeout = 5000 });
            return true;
        }
        catch { return false; }
    }

    public async Task ClickNewLeaveRequest()
    {
        await NewRequestButton.ClickAsync();
        // Wait for form to appear
        await Page.WaitForSelectorAsync("label:has-text('Start date')", new PageWaitForSelectorOptions { Timeout = 5000 });
    }

    public async Task FillLeaveForm(string leaveType, string startDate, string endDate, string reason)
    {
        // Select leave type
        await LeaveTypeSelect.SelectOptionAsync(new SelectOptionValue { Value = leaveType });

        // Fill dates — clear then type the value
        await StartDateInput.FillAsync(startDate);
        await EndDateInput.FillAsync(endDate);

        // Fill reason
        await ReasonInput.ClearAsync();
        await ReasonInput.FillAsync(reason);
    }

    public async Task SubmitLeaveForm()
    {
        await SubmitButton.ClickAsync();
        // Wait for form to close (success) or error to appear
        try
        {
            await Page.WaitForSelectorAsync("label:has-text('Start date')",
                new PageWaitForSelectorOptions { State = WaitForSelectorState.Detached, Timeout = 5000 });
        }
        catch { /* might stay open on error */ }
        await Task.Delay(500);
    }

    public async Task<bool> IsLeaveRequestVisible(string leaveType)
    {
        await Task.Delay(500);
        return await Page.Locator($"table tbody tr td:has-text('{leaveType}')").CountAsync() > 0;
    }

    public async Task<string> GetLeaveStatus(string searchText)
    {
        // Find row containing the search text, return .record-status text
        var row = Page.Locator($"table tbody tr:has-text('{searchText}')").First;
        if (await row.CountAsync() > 0)
        {
            var badge = row.Locator(".record-status").First;
            if (await badge.CountAsync() > 0)
                return await badge.InnerTextAsync();
        }
        return string.Empty;
    }

    public async Task ApproveLeaveRequest(string searchText)
    {
        var row = Page.Locator($"table tbody tr:has-text('{searchText}')").First;
        if (await row.CountAsync() > 0)
        {
            var approveBtn = row.Locator("button:has-text('Approve')").First;
            if (await approveBtn.CountAsync() > 0)
                await approveBtn.ClickAsync();
        }
        await Task.Delay(1000);
    }

    public async Task<bool> HasPendingLeaveRequest()
    {
        return await Page.Locator("table tbody tr .record-status:has-text('PENDING')").CountAsync() > 0;
    }

    public async Task<bool> IsErrorDisplayed()
    {
        return await ErrorAlert.CountAsync() > 0;
    }

    public async Task<string> GetErrorText()
    {
        if (await ErrorAlert.CountAsync() > 0)
            return await ErrorAlert.First.InnerTextAsync();
        return string.Empty;
    }
}
