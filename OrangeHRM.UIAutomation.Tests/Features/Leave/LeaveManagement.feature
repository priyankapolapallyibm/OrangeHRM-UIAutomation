@LeaveManagement @US-159 @US-163
Feature: Leave Management
  As an HR Administrator or Employee
  I want to submit and manage leave requests through the UI
  So that employee absences are properly tracked and approved

  Background:
    Given I am logged in as "Admin" with password "admin123"
    And I navigate to the Leave page

  @Smoke @LeaveList
  Scenario: Leave list page loads with request history
    Then the leave management page should be visible
    And the leave request list should be visible

  @SubmitLeave @Smoke
  Scenario: Successfully submit a new annual leave request
    When I click New Leave Request
    And I fill the leave form with:
      | Field      | Value       |
      | LeaveType  | ANNUAL      |
      | StartDate  | 2036-01-10  |
      | EndDate    | 2036-01-12  |
      | Reason     | BDD automation test - annual leave |
    And I submit the leave form
    Then the leave request should appear in the list with status "PENDING"

  @SubmitLeave @Validation
  Scenario: Leave request fails when end date is before start date
    When I click New Leave Request
    And I fill the leave form with:
      | Field      | Value       |
      | LeaveType  | SICK        |
      | StartDate  | 2036-02-15  |
      | EndDate    | 2036-02-10  |
      | Reason     | Invalid date range test |
    And I submit the leave form
    Then I should see a date validation error

  @SubmitLeave @Overlap
  Scenario: Leave request rejected when dates overlap existing approved leave
    Given a pending leave request exists in the list
    When I click New Leave Request
    And I fill the leave form with:
      | Field      | Value       |
      | LeaveType  | PERSONAL    |
      | StartDate  | 2036-01-11  |
      | EndDate    | 2036-01-13  |
      | Reason     | Overlap test |
    And I submit the leave form
    Then I should see an overlap conflict error

  @ApproveLeave
  Scenario: Manager approves a pending leave request
    Given a pending leave request exists in the list
    When I approve the leave request
    Then the leave status should change to "APPROVED"
