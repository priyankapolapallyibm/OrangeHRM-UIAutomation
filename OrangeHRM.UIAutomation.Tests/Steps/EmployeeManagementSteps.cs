using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Assist;
using OrangeHRM.UIAutomation.Tests.Drivers;
using OrangeHRM.UIAutomation.Tests.Pages;
using OrangeHRM.UIAutomation.Tests.Config;
using NUnit.Framework;

namespace OrangeHRM.UIAutomation.Tests.Steps;

[Binding]
public class EmployeeManagementSteps
{
    private readonly BrowserDriver _driver;
    private readonly EmployeePage _employeePage;
    private readonly TestSettings _settings;

    public EmployeeManagementSteps(BrowserDriver driver)
    {
        _driver = driver;
        _settings = ConfigurationLoader.GetTestSettings();
        _employeePage = new EmployeePage(_driver.Page, _settings.BaseUrl);
    }

    [Given(@"I navigate to the Employees page")]
    public async Task GivenINavigateToTheEmployeesPage()
    {
        await _employeePage.NavigateToEmployees();
    }

    [Then(@"the employee list should be visible")]
    public async Task ThenTheEmployeeListShouldBeVisible()
    {
        Assert.That(await _employeePage.IsEmployeePageVisible(), Is.True, "Employee page not visible");
    }

    [Then(@"the list should contain at least one employee")]
    public async Task ThenTheListShouldContainAtLeastOneEmployee()
    {
        var count = await _employeePage.GetEmployeeCount();
        Assert.That(count, Is.GreaterThan(0), "Employee list should contain at least one record");
    }

    [When(@"I search for employee ""(.*)""")]
    public async Task WhenISearchForEmployee(string searchTerm)
    {
        await _employeePage.SearchEmployee(searchTerm);
    }

    [Then(@"the employee list should show results containing ""(.*)""")]
    public async Task ThenTheEmployeeListShouldShowResultsContaining(string name)
    {
        Assert.That(await _employeePage.IsEmployeeInList(name), Is.True,
            $"Expected employee matching '{name}' in list");
    }

    [Then(@"the employee list should show no results")]
    public async Task ThenTheEmployeeListShouldShowNoResults()
    {
        var count = await _employeePage.GetEmployeeCount();
        Assert.That(count, Is.EqualTo(0), "Expected no results for this search");
    }

    [When(@"I click Add Employee")]
    public async Task WhenIClickAddEmployee()
    {
        await _employeePage.ClickAddEmployee();
    }

    [When(@"I fill the employee form with:")]
    public async Task WhenIFillTheEmployeeFormWith(Table table)
    {
        var data = table.Rows.ToDictionary(r => r["Field"], r => r["Value"]);
        await _employeePage.FillEmployeeForm(
            data.GetValueOrDefault("EmployeeNumber", ""),
            data.GetValueOrDefault("FirstName", ""),
            data.GetValueOrDefault("LastName", ""),
            data.GetValueOrDefault("Email", ""),
            data.GetValueOrDefault("JobTitle", ""),
            data.GetValueOrDefault("Department", ""),
            data.GetValueOrDefault("Status", "ACTIVE")
        );
    }

    [When(@"I submit the employee form")]
    public async Task WhenISubmitTheEmployeeForm()
    {
        await _employeePage.SubmitEmployeeForm();
    }

    [When(@"I submit the employee form without filling required fields")]
    public async Task WhenISubmitTheEmployeeFormWithoutFillingRequiredFields()
    {
        await _employeePage.SubmitEmployeeForm();
    }

    [Then(@"the employee ""(.*)"" should appear in the list")]
    public async Task ThenTheEmployeeShouldAppearInTheList(string name)
    {
        Assert.That(await _employeePage.IsEmployeeInList(name), Is.True,
            $"Employee '{name}' not found in list after creation");
    }

    [Then(@"I should see a validation error for the form")]
    public async Task ThenIShouldSeeAValidationErrorForTheForm()
    {
        var error = await _employeePage.GetFormValidationError();
        var errorVisible = await _driver.Page.Locator("[class*='error']:visible, [class*='invalid']:visible").CountAsync() > 0;
        Assert.That(error.Length > 0 || errorVisible, Is.True, "Expected form validation error");
    }

    [Then(@"I should see a duplicate error")]
    public async Task ThenIShouldSeeADuplicateError()
    {
        var errorVisible = await _driver.Page.Locator("[class*='error']:visible, [role='alert']:visible, [class*='conflict']:visible").CountAsync() > 0;
        Assert.That(errorVisible, Is.True, "Expected duplicate employee number error");
    }

    [Given(@"employee ""(.*)"" exists in the system")]
    public async Task GivenEmployeeExistsInTheSystem(string name)
    {
        var exists = await _employeePage.IsEmployeeInList(name);
        if (!exists)
            Assert.Inconclusive($"Pre-condition failed: employee '{name}' not in list. Run 'Add Employee' scenario first.");
    }

    [When(@"I click Edit on employee ""(.*)""")]
    public async Task WhenIClickEditOnEmployee(string name)
    {
        await _employeePage.ClickEditEmployee(name);
    }

    [When(@"I update the job title to ""(.*)""")]
    public async Task WhenIUpdateTheJobTitleTo(string jobTitle)
    {
        await _employeePage.UpdateEmployeeField("job title", jobTitle);
    }

    [Then(@"employee ""(.*)"" should show job title ""(.*)""")]
    public async Task ThenEmployeeShouldShowJobTitle(string name, string jobTitle)
    {
        var content = await _driver.Page.ContentAsync();
        Assert.That(content, Does.Contain(jobTitle).IgnoreCase,
            $"Expected job title '{jobTitle}' for employee '{name}'");
    }

    [When(@"I delete employee ""(.*)""")]
    public async Task WhenIDeleteEmployee(string name)
    {
        await _employeePage.ClickDeleteEmployee(name);
    }

    [Then(@"employee ""(.*)"" should not appear in the list")]
    public async Task ThenEmployeeShouldNotAppearInTheList(string name)
    {
        Assert.That(await _employeePage.IsEmployeeInList(name), Is.False,
            $"Employee '{name}' should have been deleted but still appears");
    }
}
