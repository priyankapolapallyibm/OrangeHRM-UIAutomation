using Microsoft.Playwright;

namespace OrangeHRM.UIAutomation.Tests.Pages;

/// <summary>
/// POM for Leave Management page.
/// US: Submit Leave Request, View Leave List, Approve/Reject Leave.
/// </summary>
public class LeavePage : BasePage
{
    private ILocator NewLeaveButton  => Page.Locator("button:has-text('New'), button:has-text('Request'), button:has-text('Add')").First;
    private ILocator LeaveTypeSelect => Page.Locator("select[name*='leaveType' i], select[id*='type' i], select").First;
    private ILocator StartDateInput  => Page.Locator("input[name*='start' i], input[placeholder*='start' i], input[type='date']").First;
    private ILocator EndDateInput    => Page.Locator("input[name*='end' i], input[placeholder*='end' i], input[type='date']").Last;
    private ILocator ReasonInput     => Page.Locator("textarea, input[name*='reason' i], input[placeholder*='reason' i]").First;
    private ILocator SubmitButton    => Page.Locator("button[type='submit']:visible, button:has-text('Submit'):visible, button:has-text('Save'):visible").Last;

    public LeavePage(IPage page, string baseUrl) : base(page, baseUrl) { }

    public async Task NavigateToLeave()
    {
        await ClickNavTab("Leave");
        await Task.Delay(1000);
    }

    public async Task<bool> IsLeavePageVisible()
    {
        await Task.Delay(500);
        return await Page.Locator("[class*='leave'], button:has-text('New'), button:has-text('Request')").CountAsync() > 0;
    }

    public async Task ClickNewLeaveRequest()
    {
        await NewLeaveButton.ClickAsync();
        await Task.Delay(600);
    }

    public async Task FillLeaveForm(int employeeId, string leaveType, string startDate, string endDate, string reason)
    {
        // Employee ID
        var empInput = Page.Locator("input[placeholder*='employee' i], input[name*='employee' i], input[type='number']").First;
        if (await empInput.CountAsync() > 0)
        {
            await empInput.ClearAsync();
            await empInput.FillAsync(employeeId.ToString());
        }

        // Leave type
        if (await LeaveTypeSelect.CountAsync() > 0)
            await LeaveTypeSelect.SelectOptionAsync(new SelectOptionValue { Value = leaveType });

        // Dates
        await StartDateInput.FillAsync(startDate);
        await EndDateInput.FillAsync(endDate);

        // Reason
        if (await ReasonInput.CountAsync() > 0)
        {
            await ReasonInput.ClearAsync();
            await ReasonInput.FillAsync(reason);
        }
    }

    public async Task SubmitLeaveForm()
    {
        await SubmitButton.ClickAsync();
        await Task.Delay(1500);
    }

    public async Task<bool> IsLeaveRequestVisible(string leaveType)
    {
        await Task.Delay(500);
        return await Page.Locator($"text={leaveType}").CountAsync() > 0;
    }

    public async Task<string> GetLeaveStatus(string leaveType)
    {
        var row = Page.Locator($"tr:has-text('{leaveType}'), [class*='row']:has-text('{leaveType}')").First;
        if (await row.CountAsync() > 0)
        {
            var statusBadge = row.Locator("[class*='badge'], [class*='status'], td").Last;
            return await statusBadge.InnerTextAsync();
        }
        return string.Empty;
    }

    public async Task ApproveLeaveRequest(string leaveType)
    {
        var row = Page.Locator($"tr:has-text('{leaveType}'), [class*='row']:has-text('{leaveType}')").First;
        var approveBtn = row.Locator("button:has-text('Approve'), [class*='approve']").First;
        if (await approveBtn.CountAsync() > 0)
            await approveBtn.ClickAsync();
        await Task.Delay(1000);
    }

    public async Task<bool> IsOverlapErrorDisplayed()
    {
        var err = Page.Locator("[class*='error']:visible, [role='alert']:visible");
        if (await err.CountAsync() > 0)
        {
            var text = await err.First.InnerTextAsync();
            return text.Contains("overlap", StringComparison.OrdinalIgnoreCase)
                || text.Contains("conflict", StringComparison.OrdinalIgnoreCase)
                || text.Contains("existing", StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }
}
