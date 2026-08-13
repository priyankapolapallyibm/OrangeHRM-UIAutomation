@RecruitmentManagement @US-172 @US-176
Feature: Recruitment Management
  As an HR Administrator
  I want to manage job vacancies and candidate pipelines through the UI
  So that hiring processes are tracked end-to-end

  Background:
    Given I am logged in as "Admin" with password "admin123"
    And I navigate to the Recruitment page

  @Smoke @VacancyList
  Scenario: Recruitment page loads with vacancy list
    Then the recruitment page should be visible

  @AddVacancy @Smoke
  Scenario: Successfully create a new job vacancy
    When I click Add Vacancy
    And I fill the vacancy form with:
      | Field       | Value                      |
      | Title       | BDD Senior QA Engineer     |
      | Department  | Quality Assurance          |
      | Description | BDD automation test vacancy|
      | Positions   | 2                          |
    And I submit the vacancy form
    Then the vacancy "BDD Senior QA Engineer" should appear in the list with status "OPEN"

  @AddVacancy @Validation
  Scenario: Create vacancy fails when title is missing
    When I click Add Vacancy
    And I submit the vacancy form without filling required fields
    Then I should see a validation error for the form

  @AddCandidate @Smoke
  Scenario: Successfully add a candidate to a vacancy
    Given a vacancy "BDD Senior QA Engineer" exists and is open
    When I click Add Candidate
    And I fill the candidate form with:
      | Field     | Value                         |
      | FirstName | Deepa                         |
      | LastName  | Nair                          |
      | Email     | deepa.nair.bdd@example.test   |
      | Phone     | +91-9012345678                |
    And I submit the candidate form
    Then candidate "Deepa Nair" should appear with status "APPLIED"

  @PipelineAdvance
  Scenario Outline: Advance candidate through hiring pipeline stages
    Given candidate "Deepa Nair" is in the recruitment pipeline
    When I advance candidate "Deepa Nair" to status "<Status>"
    Then candidate "Deepa Nair" should show status "<Status>"

    Examples:
      | Status      |
      | SHORTLISTED |
      | INTERVIEWED |
      | OFFERED     |
      | HIRED       |

  @CloseVacancy
  Scenario: Close a filled vacancy
    Given vacancy "BDD Senior QA Engineer" has been filled
    When I close the vacancy "BDD Senior QA Engineer"
    Then the vacancy "BDD Senior QA Engineer" should show status "CLOSED"
