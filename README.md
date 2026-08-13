# OrangeHRM UI Automation

> **Playwright + SpecFlow BDD + Page Object Model + C# (.NET 8)**  
> End-to-end UI automation for the PeopleFlow HR Platform

[![UI Automation Tests](https://github.com/priyankapolapallyibm/OrangeHRM-UIAutomation/actions/workflows/ui-tests.yml/badge.svg)](https://github.com/priyankapolapallyibm/OrangeHRM-UIAutomation/actions/workflows/ui-tests.yml)

---

## 📐 Architecture

```
OrangeHRM-UIAutomation/
└── OrangeHRM.UIAutomation.Tests/
    ├── Features/               # BDD Gherkin feature files (US-level scenarios)
    │   ├── Auth/               # Authentication.feature
    │   ├── Employee/           # EmployeeManagement.feature
    │   ├── Leave/              # LeaveManagement.feature
    │   ├── Recruitment/        # RecruitmentManagement.feature
    │   └── Admin/              # AdminUserManagement.feature
    ├── Steps/                  # SpecFlow step definitions (binding Gherkin → C#)
    │   ├── AuthenticationSteps.cs
    │   ├── EmployeeManagementSteps.cs
    │   ├── LeaveManagementSteps.cs
    │   ├── RecruitmentManagementSteps.cs
    │   └── AdminManagementSteps.cs
    ├── Pages/                  # Page Object Model (POM)
    │   ├── BasePage.cs         # Common helpers: nav, fill, dismiss modal
    │   ├── LoginPage.cs
    │   ├── EmployeePage.cs
    │   ├── LeavePage.cs
    │   ├── RecruitmentPage.cs
    │   └── AdminPage.cs
    ├── Hooks/
    │   └── TestHooks.cs        # BeforeScenario / AfterScenario lifecycle
    ├── Drivers/
    │   └── BrowserDriver.cs    # Playwright browser factory (chromium/firefox/webkit)
    ├── Config/
    │   ├── appsettings.json    # BaseUrl, browser, headless, timeout config
    │   └── ConfigurationLoader.cs
    ├── Utils/
    │   └── ReportManager.cs    # ExtentReports HTML report generator
    └── Reports/                # Generated at runtime — HTML report + screenshots
```

---

## 🧪 Test Coverage (User Story Mapping)

| Feature | User Story | Scenarios |
|---------|-----------|-----------|
| Authentication | US-130 | Valid login, invalid password, empty fields, unknown user, logout |
| Employee Management | US-130, US-134, US-138 | List, Search, Add (valid/validation/duplicate), Edit, Delete |
| Leave Management | US-159, US-163 | List, Submit, Invalid dates, Overlap detection, Approve |
| Recruitment Management | US-172, US-176 | Vacancies list, Add vacancy, Add candidate, Pipeline stages (Scenario Outline), Close |
| Admin User Management | US-185, US-189 | User list, Create (valid/short-pw/duplicate), Disable, Self-protection |

**Total: 5 feature files · 28 scenarios · @Smoke subset for PR gates**

---

## ⚡ Quick Start

### Prerequisites

| Tool | Version |
|------|---------|
| .NET SDK | 8.0+ |
| PeopleFlow app | Running at `http://localhost:5173` |
| PeopleFlow API | Running at `http://localhost:3000` |

### 1. Clone and restore

```bash
git clone https://github.com/priyankapolapallyibm/OrangeHRM-UIAutomation.git
cd OrangeHRM-UIAutomation
dotnet restore
```

### 2. Install Playwright browsers

```bash
cd OrangeHRM.UIAutomation.Tests
dotnet build
pwsh bin/Debug/net8.0/playwright.ps1 install
```

### 3. Start the PeopleFlow application

```bash
# In the orangehrm repo
npm run dev
# API: http://localhost:3000  |  UI: http://localhost:5173
```

### 4. Run all tests

```bash
dotnet test OrangeHRM.UIAutomation.Tests/ --logger "console;verbosity=normal"
```

### 5. Run only Smoke tests

```bash
dotnet test OrangeHRM.UIAutomation.Tests/ --filter "Category=Smoke"
```

### 6. Run a specific module

```bash
dotnet test OrangeHRM.UIAutomation.Tests/ --filter "Category=EmployeeManagement"
dotnet test OrangeHRM.UIAutomation.Tests/ --filter "Category=LeaveManagement"
dotnet test OrangeHRM.UIAutomation.Tests/ --filter "Category=RecruitmentManagement"
dotnet test OrangeHRM.UIAutomation.Tests/ --filter "Category=AdminManagement"
```

### 7. Run headed (visible browser)

```bash
# Edit Config/appsettings.json → "Headless": false
dotnet test OrangeHRM.UIAutomation.Tests/
```

---

## ⚙️ Configuration

`Config/appsettings.json`:

```json
{
  "TestSettings": {
    "BaseUrl": "http://localhost:5173",
    "ApiBaseUrl": "http://localhost:3000/api",
    "Browser": "chromium",        // chromium | firefox | webkit
    "Headless": true,
    "SlowMo": 0,                  // milliseconds between actions (0 = fastest)
    "DefaultTimeout": 30000,
    "ScreenshotOnFailure": true,
    "ScreenshotsPath": "Reports/Screenshots",
    "ReportsPath": "Reports"
  }
}
```

Override via environment variables (prefix `TEST_`):
```bash
TEST_TestSettings__BaseUrl=https://staging.example.com
TEST_TestSettings__Browser=firefox
TEST_TestSettings__Headless=false
```

---

## 📊 Reports

After a test run, open the HTML report:
```
OrangeHRM.UIAutomation.Tests/bin/Debug/net8.0/Reports/index.html
```

- **ExtentReports** dark-theme HTML with pass/fail/skip counts, timings, and screenshots
- **TRX file** for Azure DevOps / GitHub Actions integration
- **Failure screenshots** auto-saved under `Reports/Screenshots/FAIL_*.png`

---

## 🏷️ Tag Reference

| Tag | Description |
|-----|-------------|
| `@Smoke` | Critical path — run on every PR |
| `@US-130..US-189` | User Story traceability |
| `@Authentication` | Auth module |
| `@EmployeeManagement` | Employee module |
| `@LeaveManagement` | Leave module |
| `@RecruitmentManagement` | Recruitment module |
| `@AdminManagement` | Admin/User module |
| `@AddEmployee`, `@EditEmployee`, etc. | Operation-level |
| `@Validation` | Negative / boundary tests |
| `@Duplicate` | Duplicate data tests |

---

## 🔄 CI/CD

GitHub Actions pipeline (`.github/workflows/ui-tests.yml`):

- **Push to `main`/`develop`** → full suite on Chromium
- **Pull Request** → Smoke tests only (fast gate, ~10 min)
- **Manual trigger** → choose browser (chromium/firefox/webkit) + optional tag filter
- Artifacts: HTML report, TRX file, failure screenshots — retained 30 days

---

## 🗂️ Linked Azure DevOps Work Items

| ADO Item | Title |
|----------|-------|
| Feature 117 | Employee Management |
| Feature 154 | Leave Management |
| Feature 167 | Recruitment Management |
| Feature 180 | Admin/User Management |
| US 130, 134, 138 | Employee UI User Stories |
| US 159, 163 | Leave UI & Integration |
| US 172, 176 | Recruitment UI & Integration |
| US 185, 189 | Admin UI & Integration |

ADO Project: https://dev.azure.com/squaresquad26/OrangeHRM%20Automation

---

## 📝 Adding New Tests

1. Create or update a `.feature` file in `Features/<Module>/`
2. Add step definitions in `Steps/<Module>Steps.cs`
3. Add page interactions in `Pages/<Page>Page.cs`
4. Tag with `@US-XXX` and appropriate module tag
5. Run: `dotnet test --filter "Category=<YourTag>"`
