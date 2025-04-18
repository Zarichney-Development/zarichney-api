**Technical Design Document: Automation Testing Strategy for api-server**

**Version:** 1.4
**Last Updated:** 2025-04-18

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
    * `Client/`: Will contain the auto-generated Refit client code (Namespace: `Zarichney.Client`).
    * `Configuration/`: Expected to contain helpers for loading test configuration (though the factory primarily uses direct `IConfigurationBuilder` methods).
    * `Unit/`, `Integration/`: Organized mirroring `api-server` structure where applicable.
    * `Fixtures/`: Must contain implementations for `CustomWebApplicationFactory` and `DatabaseFixture`.
    * `Helpers/`: Must contain utilities like `GetRandom` and `AuthTestHelper`.
    * `Mocks/Factories/`: Must contain factories for external service mock setup (e.g., Stripe, OpenAI, GitHub).
    * `TestData/Builders/`: Implementations for core object builders once models are defined.

**6. Naming Conventions (Requirement)**

* **Test Classes:** `[SystemUnderTest]Tests.cs`.
* **Test Methods:** `[MethodName]_[Scenario]_[ExpectedOutcome]`.

**7. Test Categorization (Traits) (Requirement)**

* Test methods **must** use `[Trait("Category", "...")]`.
* **Mandatory Base Traits:** `"Unit"`, `"Integration"`.
* **Mandatory Dependency Traits (Apply all relevant):** `"Database"`, `"External:Stripe"`, `"External:OpenAI"`, `"External:GitHub"`, `"External:MSGraph"`, etc.
* **Mutability:** `"ReadOnly"`, `"DataMutating"` (Used to filter tests for safe execution against production-like environments. `ReadOnly` tests should not alter state; `DataMutating` tests might alter state and should only be run against test environments).
* **Purpose:** Enable granular test execution filtering for development efficiency and CI optimization. (Expected dev workflow: specific tests -> all unit -> relevant integration).
* **Note:** Tests that depend on external resources declared via `Dependency` traits should use the `[DependencyFact]` attribute. This attribute ensures the test is skipped if the `IntegrationTestBase` determines required configurations are missing. The `[DockerAvailableFact]` attribute can be used for tests requiring the Docker runtime itself to be available, independent of specific service configuration.

**8. Unit Testing Strategy (Requirements)**

* **Philosophy:** Test units in isolation, verify behavior, promote resilience, refactor to reduce redundancy. Follow AAA.
* **Mocking:** Use Moq. Employ mock factories (`Mocks/Factories/`) for consistent external service mock setup.
* **Assertions:** Use FluentAssertions. Utilize `"Because ...")` for intent and specific assertions for clear diagnostics. Logging content is to emphasize being beneficial for future code maintainers.
* **Parameterization:** Use `[Fact]` and `[Theory]`.
* **Data:** Use AutoFixture and Builders.
* **Directory Structure**: With the expectation to achieve a maximal practice unit test coverage (striving for >=90%), along with proper organization of the test classes, the unit tests should be organized in a way that mirrors the structure of the `api-server` project. For complex functions that warrants multiple test cases, the unit directory is to contain a dedicated file for that function's test cases. Only if a function has a single test case, should it be placed in a parent file rather than it's own dedicated file. The future expectation is that when a function warrants additional test cases, it will be migrated to a dedicated file to well organize the multitude of test cases and make it easy to find for code maintainers.

**9. Integration Testing Strategy (Requirements)**

* **Approach:** Host `api-server` in-memory via `CustomWebApplicationFactory`. Interact using the generated Refit client (`Zarichney.Client.IZarichneyAPI`) from `api-server.Tests/Client/`. Tests declare dependencies on external resources (Database, external APIs) using `[Trait("Dependency", "...")]`. The `IntegrationTestBase` checks these dependencies against runtime configuration status during initialization. Test methods requiring these dependencies use `[DependencyFact]` to ensure they are automatically skipped if dependencies are unavailable.
* **Test Configuration:** The `CustomWebApplicationFactory` configures the test application's `IConfiguration` to closely mimic the main application's loading strategy while allowing for test-specific overrides. The goal is to ensure tests run under realistic configuration conditions.
    * **Loading Order:** Configuration providers are added in the following order within the factory's `ConfigureAppConfiguration`:
        1.  `appsettings.json` (Required base configuration)
        2.  `appsettings.{EnvironmentName}.json` (e.g., `appsettings.Testing.json`, optional environment-specific settings)
        3.  `appsettings.Testing.json` (Optional, primary mechanism for test-suite-wide overrides of settings from previous files)
        4.  User Secrets (Loaded conditionally, typically only if `EnvironmentName` is "Development", for local testing)
        5.  Environment Variables (Allows overrides from CI/CD or local environment)
        6.  Database Connection String Override (An `InMemoryCollection` is added *last* to force `ConnectionStrings:IdentityConnection` to use the test database provided by `DatabaseFixture`)
    * **Scenario Handling:** This strategy supports various scenarios:
        * **No Config:** If only `appsettings.json` is present (or even absent if allowed by code), tests relying on missing external dependencies (not configured via *any* provider) will be skipped by the `[DependencyFact]` mechanism. Basic application startup should still succeed if `appsettings.json` provides minimal defaults.
        * **Local Dev:** Loads `appsettings.json`, `appsettings.Development.json` (if present), User Secrets, `appsettings.Testing.json` (for specific test overrides), and allows environment variables. This covers the typical local development workflow.
        * **Test Overrides:** Specific settings can be overridden for the entire test suite using `appsettings.Testing.json`, or for specific runs using Environment Variables (e.g., in CI).
        * **Prod Check (Future):** This scenario is supported by relying on Environment Variables to mimic production configuration. Tests can then be filtered using the `Mutability` trait to run only `ReadOnly` tests against this configuration.
    * **Note:** The `TestConfigurationHelper` class in `Configuration/` is less critical for the factory's primary setup but may still be useful for creating specific, isolated `IConfiguration` instances within individual tests if needed.
* **`CustomWebApplicationFactory` (`Fixtures/`):** Must be implemented to inherit `WebApplicationFactory<Program>`, use the test configuration strategy described above, override `ConfigureServices` to register test DbContext (using the overridden connection string), register mocked external services (using `Mocks/Factories/`), register `TestAuthHandler`, and provide server `BaseAddress`.
* **`DatabaseFixture` (`Fixtures/`, `ICollectionFixture`):** Must implement `IAsyncLifetime`, manage the PostgreSQL Testcontainer, provide its connection string, initialize Respawn, and offer a `ResetDatabaseAsync()` method.
* **Refit Client Usage:** Tests must obtain and use instances of the generated `IZarichneyAPI`. Client instantiation must use an `HttpClient` derived from `CustomWebApplicationFactory.CreateClient()` and be configured appropriately (likely within `IntegrationTestBase` or a dedicated fixture).
* **Database Handling:** Tests requiring DB access must use `DatabaseFixture` and call `ResetDatabaseAsync()`. Interaction should prioritize API calls over direct DB manipulation for setup/assert.
* **External API Handling:** Tests must interact with mocks registered by `CustomWebApplicationFactory` (provided via `Mocks/Factories/`). Mocks retrieved via `_factory.Services.GetRequiredService<Mock<IExternalService>>()` and configured per-test.
* **Authentication Simulation:** Implement and use `TestAuthHandler` (registered in factory). Employ `AuthTestHelper` or base class methods to configure user/claims before API calls.
* **Fixtures & Test Base Class:** Implement standard xUnit fixture usage (`ICollectionFixture<DatabaseFixture>`, `IClassFixture<CustomWebApplicationFactory>`). Provide an `IntegrationTestBase` class for common setup, accessors (client, helpers, fixtures), and automated dependency checking/skipping logic.

**10. Test Data Management & Reusable Utilities (Requirements)**

* **Primary Tools:** AutoFixture and Custom Builders.
* **AutoFixture:** Must be used for anonymous data, simple DTOs, test parameters (`[AutoData]`), and populating builders (via `GetRandom` helper).
* **Builders (`TestData/Builders/`):** Implementations for core object builders (e.g., `RecipeBuilder`), leveraging `AutoFixture` via the `GetRandom` helper where appropriate.
* **Reusable Utilities:** Implement `GetRandom` and `AuthTestHelper` in `Helpers/` for generating random data and simulating authentication. These are examples of reusable utilities that can be used across multiple tests. You are at liberty to identify redundancy in your tests and create reusable utilities to help reduce redundancy. The goal is to minimize redundancy across the test suite, and to encourage future code maintainers to refactor in order to effectively reduce code duplication.

**11. CI/CD Integration (GitHub Actions) (Requirements)**

* **Workflow:** Must run on PRs to `main` and merges to `main`.
* **Steps:** Must include Checkout, Setup .NET SDK, Restore, Build, Run Unit Tests (filtered), Run Integration Tests (filtered, potentially parallelized by category), Publish Test Results (TRX), Generate & Publish Coverage Report (Cobertura). Consider adding a step to run the client generation script.
* **Production Check Scenario:** A potential future step in a deployment workflow could involve running tests against a production-like environment. This step **must** filter tests to exclude those marked with `[Trait("Mutability", "DataMutating")]` (e.g., using `--filter "Category=Integration&Mutability!=DataMutating"`) to prevent unintended state changes.
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

* **Purpose:** Provide a strongly-typed Refit client (`IZarichneyAPI`) within `api-server.Tests` for integration testing.
* **Mechanism:** A PowerShell script must be provided at `/Scripts/GenerateApiClient.ps1`.
* **Script Functionality:** The script must automate:
    1.  Building the `api-server` project (Debug config).
    2.  Generating `swagger.json` using `dotnet swagger tofile`.
    3.  Generating the Refit client (`IZarichneyAPI` and models) using `refitter`.
    4.  Placing the generated code into `api-server.Tests/Client/` with the namespace `Zarichney.Client`.
* **Usage Requirement:** The developer (or AI coder) assigned a task **must** run this script after any changes to `api-server` controller signatures, routes, or associated models to ensure the test client is synchronized with the API contract. Relevant documentation should remind users of this step.
* **Deliverable:** The functional `GenerateApiClient.ps1` script in the `/Scripts` directory, and references to this script in the relevant endpoint and standards documentation, to ensure future maintenance and usage of endpoint changes (important: this is part of the expected workflow - when making endpoint changes, this script must be run in order to detect whether the change broke any tests!! So this needs to be well reflected in documentation in order for code maintainers not to miss this).

**14. API Server Project Modifications**
* **Refactoring into a testable state**: The only reason to modify anything under the `/api-server/` directory is to refactor any code to make it testing compatible. You are at liberty to refactor any code that is not testable in it's current state. Confirm any refactoring and ensure no regression via automation test case implementation.

**15. Documentation & Maintenance (Requirements)**
* **General Documentation:** All general or solution related (non localized specific) documentation must be maintained in the `/Docs/` directory.
* **Testing Standards:** The `TestingStandards.md` document must be maintained in `/Docs/Development/`. This will be the ongoing reference for future code maintainers. The expectation is that this is read and reviewed prior to any assignment work to ensure consistent changes of test suite changes.
* **Existing Documentation Standards:** `/Docs/Development/DocumentationStandards.md` must be followed for the introduction of documentation within the `api-server.Tests` project. Use the `/Docs/Development/README_template.md` for the introduction of new README files. Note the emphasis on capturing the why and what, and not the how, as these are most beneficial as english documentation, while the how is well articulated via the code itself.
* **Maintenance:** Use the `/Docs/Maintenance/` for any requirement of manual setups, or beneficial documentation for a human solution maintainer.
* **Document Recommendations**: For anything that is out-of-scope from this technical design, please leave references where appropriate regarding future recommended enhancements to the test suite or testing framework. You are at liberty to implement any existing recommendations that you see fit, there is no limit to putting in additional effort in order to deliver the highest quality testing project. The ultimate goal to the overall endeavor is to maximize code quality. Your expertise is appreciated.