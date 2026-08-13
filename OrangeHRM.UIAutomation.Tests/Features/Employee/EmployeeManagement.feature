@EmployeeManagement @US-130 @US-134 @US-138
Feature: Employee Management
  As an HR Administrator
  I want to manage employee records through the UI
  So that accurate employee data is maintained in the system

  Background:
    Given I am logged in as "Admin" with password "admin123"
    And I navigate to the Employees page

  @Smoke @EmployeeList
  Scenario: Employee list loads with existing records
    Then the employee list should be visible
    And the list should contain at least one employee

  @EmployeeSearch
  Scenario: Search for employee by name returns matching results
    When I search for employee "Aarav"
    Then the employee list should show results containing "Aarav"

  @EmployeeSearch
  Scenario: Search with no match shows empty result
    When I search for employee "ZZZNOMATCH99999"
    Then the employee list should show no results

  @AddEmployee @Smoke
  Scenario: Successfully add a new employee
    When I click Add Employee
    And I fill the employee form with:
      | Field            | Value                         |
      | EmployeeNumber   | EMP-BDD-001                   |
      | FirstName        | Bhavana                       |
      | LastName         | Reddy                         |
      | Email            | bhavana.reddy.bdd@example.test|
      | JobTitle         | Test Automation Engineer      |
      | Department       | Quality Assurance             |
    And I submit the employee form
    Then the employee "Bhavana Reddy" should appear in the list

  @AddEmployee @Validation
  Scenario: Add employee fails when required fields are missing
    When I click Add Employee
    And I submit the employee form without filling required fields
    Then I should see a validation error for the form

  @AddEmployee @Duplicate
  Scenario: Add employee fails with duplicate employee number
    When I click Add Employee
    And I fill the employee form with:
      | Field          | Value         |
      | EmployeeNumber | EMP-BDD-001   |
      | FirstName      | Duplicate     |
      | LastName       | Test          |
      | Email          | dup2@example.test |
      | JobTitle       | Tester        |
      | Department     | QA            |
    And I submit the employee form
    Then I should see a duplicate error

  @EditEmployee
  Scenario: Edit an existing employee's job title
    Given employee "Bhavana Reddy" exists in the system
    When I click Edit on employee "Bhavana Reddy"
    And I update the job title to "Senior Test Automation Engineer"
    And I submit the employee form
    Then employee "Bhavana Reddy" should show job title "Senior Test Automation Engineer"

  @DeleteEmployee
  Scenario: Delete an employee removes them from the list
    Given employee "Bhavana Reddy" exists in the system
    When I delete employee "Bhavana Reddy"
    Then employee "Bhavana Reddy" should not appear in the list
