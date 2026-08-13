@Authentication @US-130
Feature: Authentication
  As a PeopleFlow user
  I want to securely log in and out of the application
  So that my data and actions are protected

  Background:
    Given I am on the PeopleFlow login page

  @Smoke @Login-Valid
  Scenario: Successful login with valid admin credentials
    When I enter username "Admin" and password "admin123"
    And I click the login button
    Then I should be redirected to the dashboard
    And the navigation menu should be visible

  @Login-Invalid
  Scenario: Login fails with incorrect password
    When I enter username "Admin" and password "WrongPassword123"
    And I click the login button
    Then I should see an error message
    And I should remain on the login page

  @Login-Invalid
  Scenario: Login fails with empty credentials
    When I enter username "" and password ""
    And I click the login button
    Then I should see a validation error

  @Login-Invalid
  Scenario: Login fails with unknown username
    When I enter username "unknown.user@test.com" and password "admin123"
    And I click the login button
    Then I should see an error message

  @Logout
  Scenario: Successful logout after login
    Given I am logged in as "Admin" with password "admin123"
    When I click logout
    Then I should be redirected to the login page
