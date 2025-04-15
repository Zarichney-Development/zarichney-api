**Technical Design Document: Automation Testing Strategy for api-server**

**1. Introduction & Goals**

* **Purpose:** This document outlines the standards, frameworks, structure, and practices required for implementing automated testing within the `api-server` solution (.NET 8). It serves as the strategic blueprint defining the expected testing infrastructure.
* **Goals:**
    * Ensure the highest possible confidence in code quality, stability, and correctness.
    * Prevent regressions through comprehensive test coverage.
    * Provide fast and reliable feedback via automated testing.
    * Integrate seamlessly into the CI/CD pipeline (GitHub Actions).
    * Establish clear, maintainable, and exceptionally *strict* testing standards, designed for rigorous enforcement by AI-driven development workflows. Passing tests according to the associated `Testing Standards` document is mandatory.
    * Achieve maximum practical unit test coverage (striving for >=90%) on all non-trivial logic, focusing on resilient tests verifying behavior.
    * Validate API endpoint functionality comprehensively via integration tests using a generated typed Refit client.

**2. Scope**

* **In Scope:**
    * Unit testing of `api-server` components.
    * Integration testing of `api-server` endpoints via in-memory hosting and a generated Refit client.
    * Database interaction testing (using Testcontainers).
    * Testing involving mocked external service interfaces.
    * Authentication/Authorization testing for API endpoints.
* **Out of Scope (Initially):** UI/E2E testing, Load/Performance testing, Security penetration testing, Manual testing.

**3. Solution Project Structure (Expected)**

1.  `api-server/`: The main ASP.NET Core Web API project.
2.  `api-server.Tests/`: The single test project containing all automated tests and the generated Refit API client code. (Depends on `api-server`).

**4. Chosen Framework & Tools (Requirements)**

* **Test Framework:** xUnit
* **Assertion Library:** FluentAssertions
* **Mocking Library:** Moq
* **Test Data Generation:** AutoFixture, Custom Test Data Builders
* **Integration Testing Host:** `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>`
* **Integration Testing API Client:** Refit (code generated into `api-server.Tests`)
* **API Client Generation Tools:** `dotnet swagger`, `refitter` (automation expected via script)
* **Integration Test Database:** Testcontainers (PostgreSQL)
* **Database Cleanup:** Respawn
* **Code Coverage:** Coverlet
* **CI/CD Platform:** GitHub Actions

**5. Test Project Structure (`api-server.Tests/`) (Expected)**

* A clear folder structure separating configuration, unit tests, integration tests, fixtures, helpers, mocks (including mock factories), test data artifacts (including builders), and the generated API client code.
    * `ApiClient/`: Will contain the auto-generated Refit client code (Namespace: `Zarichney.Client`).
    * `Configuration/`: Expected to contain helpers for loading test configuration.
    * `Unit/`, `Integration/`: Organized mirroring `api-server` structure where applicable.
    * `Fixtures/`: Must contain implementations for `CustomWebApplicationFactory` and `DatabaseFixture`.
    * `Helpers/`: Must contain utilities like `GetRandom` and `AuthTestHelper`.
    * `Mocks/Factories/`: Must contain factories for external service mock setup (e.g., Stripe, OpenAI, GitHub).
    * `TestData/Builders/`: **TODO:** Expects implementations for core object builders once models are defined.

**6. Naming Conventions (Requirement)**

* **Test Classes:** `[SystemUnderTest]Tests.cs`.
* **Test Methods:** `[MethodName]_[Scenario]_[ExpectedOutcome]`.

**7. Test Categorization (Traits) (Requirement)**

* Test methods **must** use `[Trait("Category", "...")]`.
* **Mandatory Base Traits:** `"Unit"`, `"Integration"`.
* **Mandatory Dependency Traits (Apply all relevant):** `"Database"`, `"External:Stripe"`, `"External:OpenAI"`, `"External:GitHub"`, `"External:MSGraph"`, etc.
* **Purpose:** Enable granular test execution filtering for development efficiency and CI optimization. (Expected dev workflow: specific tests -> all unit -> relevant integration).

**8. Unit Testing Strategy (Requirements)**

* **Philosophy:** Test units in isolation, verify behavior, promote resilience. Follow AAA.
* **Mocking:** Use Moq. Employ mock factories (`Mocks/Factories/`) for consistent external service mock setup.
* **Assertions:** Use FluentAssertions. Utilize `.Because("...")` for intent and specific assertions for clear diagnostics.
* **Parameterization:** Use `[Fact]` and `[Theory]`.
* **Data:** Use AutoFixture and Builders.

**9. Integration Testing Strategy (Requirements)**

* **Approach:** Host `api-server` in-memory via `CustomWebApplicationFactory`. Interact using the generated Refit client (`Zarichney.Client.IZarichneyClient`) from `api-server.Tests/ApiClient/`.
* **Test Configuration:** Implement `TestConfigurationHelper` (`Configuration/`) to load test `IConfiguration`.
* **`CustomWebApplicationFactory` (`Fixtures/`):** Must be implemented to inherit `WebApplicationFactory<Program>`, use test configuration, override `ConfigureServices` to register test DbContext, register mocked external services (using `Mocks/Factories/`), register `TestAuthHandler`, and provide server `BaseAddress`.
* **`DatabaseFixture` (`Fixtures/`, `ICollectionFixture`):** Must implement `IAsyncLifetime`, manage the PostgreSQL Testcontainer, provide its connection string, initialize Respawn, and offer a `ResetDatabaseAsync()` method.
* **Refit Client Usage:** Tests must obtain and use instances of the generated `IZarichneyClient`. Client instantiation must use an `HttpClient` derived from `CustomWebApplicationFactory.CreateClient()` and be configured appropriately (likely within `IntegrationTestBase` or a dedicated fixture).
* **Database Handling:** Tests requiring DB access must use `DatabaseFixture` and call `ResetDatabaseAsync()`. Interaction should prioritize API calls over direct DB manipulation for setup/assert.
* **External API Handling:** Tests must interact with mocks registered by `CustomWebApplicationFactory` (provided via `Mocks/Factories/`). Mocks retrieved via `_factory.Services.GetRequiredService<Mock<IExternalService>>()` and configured per-test.
* **Authentication Simulation:** Implement and use `TestAuthHandler` (registered in factory). Employ `AuthTestHelper` or base class methods to configure user/claims before API calls.
* **Fixtures & Test Base Class:** Implement standard xUnit fixture usage (`ICollectionFixture<DatabaseFixture>`, `IClassFixture<CustomWebApplicationFactory>`). Provide an `IntegrationTestBase` class for common setup and accessors (client, helpers, fixtures).

**10. Test Data Management (Requirements)**

* **Primary Tools:** AutoFixture and Custom Builders.
* **AutoFixture:** Must be used for anonymous data, simple DTOs, test parameters (`[AutoData]`), and populating builders.
* **Builders (`TestData/Builders/`):** **TODO:** Builders for core/complex objects must be implemented once models are stable.

**11. CI/CD Integration (GitHub Actions) (Requirements)**

* **Workflow:** Must run on PRs to `main` and merges to `main`.
* **Steps:** Must include Checkout, Setup .NET SDK, Restore, Build, Run Unit Tests (filtered), Run Integration Tests (filtered, potentially parallelized by category), Publish Test Results (TRX), Generate & Publish Coverage Report (Cobertura). Consider adding a step to run the client generation script.
* **Deliverables:**
    * Updated `/.github/workflows/main.yml` file implementing the workflow.
    * Documentation in `/Docs/Maintenance/TestingSetup.md` for any manual setup or maintenance procedures.

**12. Reliability & Performance (Requirements)**

* **Flakiness:** Must be actively prevented and fixed immediately. Ensure proper isolation.
* **Performance:** Maximize test suite performance.
    * Enable/Verify xUnit parallel execution.
    * Optimize fixture setup time.
    * Keep integration tests focused.
    * Monitor execution time in CI.

**13. API Client Generation for Tests (Requirement)**

* **Purpose:** Provide a strongly-typed Refit client (`IZarichneyClient`) within `api-server.Tests` for integration testing.
* **Mechanism:** A PowerShell script must be provided at `/Scripts/GenerateApiClient.ps1`.
* **Script Functionality:** The script must automate:
    1.  Building the `api-server` project (Debug config).
    2.  Generating `swagger.json` using `dotnet swagger tofile`.
    3.  Generating the Refit client (`IZarichneyClient` and models) using `refitter`.
    4.  Placing the generated code into `api-server.Tests/ApiClient/` with the namespace `Zarichney.Client`.
* **Deliverable:** The functional `GenerateApiClient.ps1` script in the `/Scripts` directory.
* **Usage Requirement:** The developer (or AI coder) assigned a task **must** run this script after any changes to `api-server` controller signatures, routes, or associated models to ensure the test client is synchronized with the API contract. Relevant documentation should remind users of this step.
