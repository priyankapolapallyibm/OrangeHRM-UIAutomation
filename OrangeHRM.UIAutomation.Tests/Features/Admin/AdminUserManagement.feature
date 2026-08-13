@AdminManagement @US-185 @US-189
Feature: Admin User Management
  As a System Administrator
  I want to manage user accounts through the Admin UI
  So that access control is properly enforced

  Background:
    Given I am logged in as "Admin" with password "admin123"
    And I navigate to the Admin page

  @Smoke @UserList
  Scenario: Admin page loads with user account list
    Then the admin page should be visible
    And the user list should be visible
    And no password hashes should be exposed in the UI

  @CreateUser @Smoke
  Scenario: Successfully create a new employee user account
    When I click Add User
    And I fill the user form with:
      | Field       | Value                    |
      | Username    | bdd.test.user.auto       |
      | Password    | Welcome@BDD123           |
      | DisplayName | BDD Test User            |
      | Role        | EMPLOYEE                 |
    And I submit the user form
    Then user "bdd.test.user.auto" should appear in the user list

  @CreateUser @Validation
  Scenario: Create user fails when password is too short
    When I click Add User
    And I fill the user form with:
      | Field       | Value        |
      | Username    | short.pw.bdd |
      | Password    | short        |
      | DisplayName | Short PW     |
      | Role        | EMPLOYEE     |
    And I submit the user form
    Then I should see a password validation error

  @CreateUser @Duplicate
  Scenario: Create user fails with duplicate username
    Given user "bdd.test.user.auto" already exists
    When I click Add User
    And I fill the user form with:
      | Field       | Value              |
      | Username    | bdd.test.user.auto |
      | Password    | Welcome@BDD123     |
      | DisplayName | Duplicate BDD      |
      | Role        | EMPLOYEE           |
    And I submit the user form
    Then I should see a duplicate username error

  @DisableUser
  Scenario: Disable a user account
    Given user "bdd.test.user.auto" is active in the system
    When I disable user "bdd.test.user.auto"
    Then user "bdd.test.user.auto" should show status "Inactive"

  @SelfProtection
  Scenario: Admin cannot deactivate their own account
    When I attempt to disable my own account
    Then I should see an error preventing self-deactivation
