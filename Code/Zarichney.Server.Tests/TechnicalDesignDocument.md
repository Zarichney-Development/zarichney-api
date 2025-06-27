**Technical Design Document: Automation Testing Strategy for Zarichney.Server**

**Version:** 1.8
**Last Updated:** 2025-06-26

**1. Introduction & Goals**

* **Purpose:** This document outlines the standards, frameworks, structure, and practices required for implementing automated testing within the `Zarichney.Server` solution (.NET 8). It serves as the strategic blueprint defining the expected testing infrastructure.
* **Goals:**
    * Ensure the highest possible confidence in code quality, stability, and correctness.
    * Prevent regressions through comprehensive test coverage.
    * Provide fast and reliable feedback via automated testing.
    * Integrate seamlessly into the CI/CD pipeline (GitHub Actions).
    * Establish clear, maintainable, and exceptionally *strict* testing standards, designed for rigorous enforcement by AI-driven development workflows. Adherence to the testing standards documents is mandatory.
    * Achieve maximum practical unit test coverage (striving for >=90%) on all non-trivial logic, focusing on resilient tests verifying behavior.
    * Validate API endpoint functionality comprehensively via integration tests using a generated typed Refit client.

**2. Scope**

* **In Scope:**
    * Unit testing of `Zarichney.Server` components.
    * Integration testing of `Zarichney.Server` endpoints via in-memory hosting and a generated Refit client.
    * Database interaction testing (using Testcontainers).
    * Testing involving mocked/virtualized external service interfaces.
    * Authentication/Authorization testing for API endpoints.
* **Out of Scope (Initially):** UI/E2E testing, Load/Performance testing (beyond basic smoke tests), Security penetration testing, Manual testing.

**3. Solution Project Structure (Current State)**

1.  `Zarichney.Server/`: The main ASP.NET Core Web API project.
2.  `Zarichney.Server.Tests/`: The single test project containing all automated tests and the generated Refit API client code. (Depends on `Zarichney.Server`).
3.  `Zarichney.TestingFramework/`: Shared testing framework library (planned migration from Zarichney.Server.Tests/Framework).
4.  `Zarichney.TestingFramework.Tests/`: Unit tests for the testing framework itself.
5.  `Zarichney.ApiClient/`: Refit-based API client library for external consumption.
6.  `Zarichney.ApiClient.Tests/`: Tests for the API client library.

**4. Chosen Framework & Tools (Requirements)**

* **Test Framework:** xUnit
* **Assertion Library:** FluentAssertions
* **Mocking Library:** Moq
* **Test Data Generation:** AutoFixture, Custom Test Data Builders
* **Integration Testing Host:** `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>`
* **Integration Testing API Client:** Refit (multiple clients generated into `Zarichney.Server.Tests` via `.refitter` configuration)
* **API Client Generation Tools:** `dotnet swagger`, `refitter` (automation expected via script with `.refitter` settings file)
* **Integration Test Database:** Testcontainers (PostgreSQL)
* **External HTTP Service Virtualization:** WireMock.Net (to be integrated, see Roadmap)
* **Database Cleanup:** Respawn
* **Code Coverage:** Coverlet
* **CI/CD Platform:** GitHub Actions
* **Contract Testing (Future):** PactNet (under consideration, see Roadmap)

**5. Test Project Structure (`Zarichney.Server.Tests/Framework/`) (Expected)**

* A clear folder structure separating configuration, unit tests, integration tests, fixtures, helpers, mocks (including mock factories and virtualization setups), test data artifacts (including builders), and the generated API client code.
    * `Client/`: Will contain the auto-generated Refit client interfaces grouped by API tags (controllers) and contracts (Namespace: `Zarichney.Client`).
    * `Configuration/`: Expected to contain helpers for loading test configuration (though the factory primarily uses direct `IConfigurationBuilder` methods).
    * `Unit/`, `Integration/`: Organized mirroring `Zarichney.Server` structure where applicable.
    * `Fixtures/`: Must contain implementations for `CustomWebApplicationFactory`, `DatabaseFixture`, and potentially fixtures for managing WireMock.Net instances.
    * `Helpers/`: Must contain utilities like `GetRandom` and `AuthTestHelper`.
    * `Mocks/Factories/`: Must contain factories for external service mock setup (e.g., Stripe, OpenAI, GitHub).
    * `Mocks/Virtualization/`: (New) Configuration and setup for WireMock.Net stubs for external HTTP services.
    * `TestData/Builders/`: Implementations for core object builders and AutoFixture customizations.

**6. Naming Conventions (Requirement)**

* **Test Classes:** `[SystemUnderTest]Tests.cs`.
* **Test Methods:** `[MethodName]_[Scenario]_[ExpectedOutcome]`.

**7. Test Categorization (Traits) (Requirement)**

* Test methods **must** use `[Trait("Category", "...")]`.
* **Mandatory Base Traits:** `"Unit"`, `"Integration"`.
* **Mandatory Dependency Traits (Apply all relevant):** `"Database"`, `"ExternalStripe"`, `"ExternalOpenAI"`, `"ExternalGitHub"`, `"ExternalMSGraph"`, etc.
* **Mutability:** `"ReadOnly"`, `"DataMutating"` (Used to filter tests for safe execution against production-like environments. `ReadOnly` tests should not alter state; `DataMutating` tests might alter state and should only be run against test environments).
* **Purpose:** Enable granular test execution filtering for development efficiency and CI optimization. (Expected dev workflow: specific tests -> all unit -> relevant integration).
* **Note:** Tests that depend on external resources declared via `Dependency` traits should use the `[DependencyFact]` attribute. This attribute ensures the test is skipped if the `IntegrationTestBase` determines required configurations are missing. The `[DockerAvailableFact]` attribute can be used for tests requiring the Docker runtime itself to be available, independent of specific service configuration.

**8. Unit Testing Strategy (Requirements)**

* **Philosophy:** Test units in isolation, verify behavior, promote resilience, refactor to reduce redundancy. Follow AAA. Adhere to `../Docs/Standards/UnitTestCaseDevelopment.md`.
* **Mocking:** Use Moq. Employ mock factories (`Mocks/Factories/`) for consistent internal service mock setup.
* **Assertions:** Use FluentAssertions. Utilize `"Because ...")` for intent and specific assertions for clear diagnostics. Logging content is to emphasize being beneficial for future code maintainers.
* **Parameterization:** Use `[Fact]` and `[Theory]`.
* **Data:** Use AutoFixture and Builders. Reference `../Docs/Standards/UnitTestCaseDevelopment.md` for advanced AutoFixture usage patterns.
* **Directory Structure**: With the expectation to achieve a maximal practice unit test coverage (striving for >=90%), along with proper organization of the test classes, the unit tests should be organized in a way that mirrors the structure of the `Zarichney.Server` project. For complex functions that warrants multiple test cases, the unit directory is to contain a dedicated file for that function's test cases. Only if a function has a single test case, should it be placed in a parent file rather than it's own dedicated file. The future expectation is that when a function warrants additional test cases, it will be migrated to a dedicated file to well organize the multitude of test cases and make it easy to find for code maintainers.

**9. Integration Testing Strategy (Requirements)**

* **Approach:** Host `Zarichney.Server` in-memory via `CustomWebApplicationFactory`. Interact using the generated Refit client interfaces (e.g., `IAuthApi`, `IAiApi`, `ICookbookApi`) from `Zarichney.Server.Tests/Framework/Client/`. Tests declare dependencies on external resources (Database, external APIs) using `[Trait("Dependency", "External...")]`. The `IntegrationTestBase` checks these dependencies against runtime configuration status during initialization. Test methods requiring these dependencies use `[DependencyFact]` to ensure they are automatically skipped if dependencies are unavailable. Adhere to `../Docs/Standards/IntegrationTestCaseDevelopment.md`.
* **Test Configuration:** The `CustomWebApplicationFactory` configures the test application's `IConfiguration` to closely mimic the main application's loading strategy while allowing for test-specific overrides and integration with user secrets for local development. The goal is to ensure tests run under realistic configuration conditions across different environments (Local Dev, CI, specific "Testing" environment).
    * **Loading Order:** Configuration providers are added in the following order within the factory's `ConfigureAppConfiguration`:
        1.  `appsettings.json` (Optional base configuration)
        2.  `appsettings.{EnvironmentName}.json` (e.g., `appsettings.Development.json`, optional environment-specific settings)
        3.  `appsettings.Testing.json` (Optional, provides base test settings like `TestUser` and allows test-suite-wide overrides. Loaded *before* secrets/env vars.)
        4.  User Secrets (Loaded conditionally, typically if `EnvironmentName` is "Development", allowing local override of connection strings, API keys, etc.)
        5.  Environment Variables (Allows overrides from CI/CD or local environment, takes highest precedence)
    * **Database Connection:** The connection string (`ConnectionStrings:IdentityConnection`) is **not** forcefully overridden during configuration loading. It is determined dynamically within `ConfigureTestServices` based on the final loaded configuration and fixture availability (see Database Handling below).
    * **Scenario Handling:** This strategy supports:
        * **Local Dev:** Defaults to "Development" environment, loads User Secrets for sensitive data like the DB connection string, uses `appsettings.Testing.json` for non-secret test data (e.g., `TestUser`).
        * **CI/CD:** Relies on Environment Variables (if needed) and the Testcontainers database provided by `DatabaseFixture`. Loads base settings and `TestUser` from `appsettings.Testing.json`.
        * **"Testing" Environment:** Can be triggered by setting `ASPNETCORE_ENVIRONMENT=Testing` (or `DOTNET_ENVIRONMENT=Testing`). This would load `appsettings.Testing.json` *after* `appsettings.json` but *before* User Secrets (if Dev) and Environment Variables, allowing it to provide environment-specific overrides.
    * **Note:** The `TestConfigurationHelper` class may be less critical for the factory's setup but can be used for creating isolated `IConfiguration` instances within tests if needed.
* **`CustomWebApplicationFactory` (`Fixtures/`, `ICollectionFixture`):** Must be implemented to inherit `WebApplicationFactory<Program>`.
    * Uses the refined test configuration strategy described above in `ConfigureAppConfiguration`.
    * Overrides `ConfigureTestServices` to:
        * Determine the database connection strategy (prioritized: Configured String -> `DatabaseFixture` -> InMemory Fallback) and register the `UserDbContext` accordingly.
        * Register mocked internal services (using `Mocks/Factories/`) and configure/register WireMock.Net for external HTTP services.
        * Register `TestAuthHandler` for simulating authentication.
    * Is provided as a shared instance via `ICollectionFixture` to all tests in the `"Integration"` collection.
* **`DatabaseFixture` (`Fixtures/`, `ICollectionFixture`):** Must implement `IAsyncLifetime`.
    * Manages the PostgreSQL Testcontainer lifecycle (start/stop).
    * Provides the dynamic connection string for the container.
    * **Crucially, must apply EF Core migrations programmatically** (e.g., using `dbContext.Database.MigrateAsync()`) within `InitializeAsync` after the container starts successfully, to ensure the schema is ready.
    * Initializes Respawn for database cleanup.
    * Offers a `ResetDatabaseAsync()` method to be called by tests before seeding data.
    * Is provided as a shared instance via `ICollectionFixture` to all tests in the `"Integration"` collection.
* **Refit Client Usage:** Tests must obtain and use instances of the generated API client interfaces (e.g., `IAuthApi`, `IAiApi`, `ICookbookApi`, `IPaymentApi`, `IPublicApi`). Client instantiation happens within the shared `ApiClientFixture`, which provides granular access to specific API functionality and uses `HttpClient` instances derived from the shared `CustomWebApplicationFactory.CreateClient()`.
* **Database Handling:** Tests requiring DB access should inherit `DatabaseIntegrationTestBase` and belong to the `"Integration"` collection to receive the shared `DatabaseFixture`. The `CustomWebApplicationFactory` determines the `DbContext` configuration based on this priority: 1. Use connection string from `IConfiguration` (supports User Secrets) if valid. 2. Else, use connection string from the shared `DatabaseFixture` (supports Testcontainers) if available and running. 3. Else, fallback to `UseInMemoryDatabase`. Tests should call `await ResetDatabaseAsync()` before seeding data or performing mutating actions. Interaction should prioritize API calls over direct DB manipulation for setup/assert where practical.
* **External API Handling:** Integration tests must interact with virtualized external HTTP services managed by WireMock.Net, configured via `CustomWebApplicationFactory`. Mocks for non-HTTP external dependencies (if any) are registered via `Mocks/Factories/`.
* **Authentication Simulation:** Implement and use `TestAuthHandler` (registered in factory). Employ `AuthTestHelper` or base class methods to configure user/claims before API calls.
* **Fixtures & Test Base Class:** A single test collection (`[Collection("Integration")]`) should be used for all integration tests. This collection provides shared instances of `CustomWebApplicationFactory`, `DatabaseFixture`, and `ApiClientFixture` via `ICollectionFixture<>` for efficiency.
    * An `IntegrationTestBase` class provides common setup (dependency checking, configuration access) and accessors for shared fixtures (Factory, API Clients). It accepts fixtures via constructor injection but does **not** declare `IClassFixture<>`.
    * A `DatabaseIntegrationTestBase` inherits `IntegrationTestBase` and adds specific handling and accessors for the `DatabaseFixture`. It also accepts fixtures via constructor injection without declaring `IClassFixture<>`.
    * Tests should inherit the appropriate base class and belong to the `"Integration"` collection.

**10. Test Data Management & Reusable Utilities (Requirements)**

* **Primary Tools:** AutoFixture and Custom Builders.
* **AutoFixture:** Must be used for anonymous data, simple DTOs, test parameters (`[AutoData]`), and populating builders (via `GetRandom` helper). Consult relevant standards documents for advanced customization patterns.
* **Builders (`TestData/Builders/`):** Implementations for core object builders (e.g., `RecipeBuilder`), leveraging `AutoFixture` via the `GetRandom` helper where appropriate. Implement custom AutoFixture `ISpecimenBuilder` and `ICustomization` for complex domain objects here.
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

* **Purpose:** Provide multiple strongly-typed Refit client interfaces (grouped by API tags/controllers) within `Zarichney.Server.Tests` for integration testing.
* **Configuration:** The primary configuration is managed via the root `.refitter` file, which defines:
    * **Key Settings:**
        * `multipleInterfaces: "ByTag"` - Generates separate interfaces per OpenAPI tag (controller)
        * `generateMultipleFiles: true` - Creates separate files for each interface
        * `outputFolder: "Zarichney.Server.Tests/Framework/Client"` - Target directory for generated code
        * `addContentTypeHeaders: false` - Resolves multipart/form-data Content-Type conflicts
        * `returnIApiResponse: true` - Returns `IApiResponse<T>` for better error handling
        * `namespace: "Zarichney.Client"` - Generated code namespace
        * `contractsNamespace: "Zarichney.Client.Contracts"` - DTOs namespace
* **Generation Tools:** PowerShell script at `/Scripts/generate-api-client.ps1` and shell script `/Scripts/generate-api-client.sh`.
* **Script Functionality:** The scripts automate:
    1.  Building the `Zarichney.Server` project (Debug config).
    2.  Generating `swagger.json` using `dotnet swagger tofile`.
    3.  Generating multiple Refit client interfaces (e.g., `IAuthApi`, `IAiApi`, `ICookbookApi`) and contracts using `refitter` with the `.refitter` settings file.
    4.  Placing the generated code into `Zarichney.Server.Tests/Framework/Client/` with organized structure.
* **Generated Output:** Multiple files including:
    * Individual interface files: `IAuthApi.cs`, `IAiApi.cs`, `ICookbookApi.cs`, `IPaymentApi.cs`, `IPublicApi.cs`
    * Shared contracts: `Contracts.cs` (DTOs and models)
* **ApiClientFixture Integration:** The `ApiClientFixture` provides granular access to each client interface via properties like `AuthenticatedAuthApi`, `UnauthenticatedAiApi`, etc.
* **Usage Requirement:** The developer (or AI coder) assigned a task **must** run the generation script after any changes to `Zarichney.Server` controller signatures, routes, or associated models to ensure the test clients are synchronized with the API contract. The `.refitter` configuration ensures consistent generation behavior.
* **Deliverable:** The functional generation scripts in `/Scripts/`, the `.refitter` configuration file, and references to these in relevant documentation to ensure proper workflow integration.

**14. Current Test Coverage State (As of 2025-06-26)**

* **Overall Line Coverage:** 24% (2,558 of 10,640 lines)
* **Overall Branch Coverage:** 18.2% (404 of 2,208 branches)
* **Overall Method Coverage:** 30.6% (305 of 996 methods)
* **Test Count:** 228 total tests (201 passing, 27 skipped)
* **Test Categories:** 
    * Unit Tests: 39 test methods marked
    * Integration Tests: 2 test methods marked (many integration tests missing proper traits)
* **Key Coverage Gaps:**
    * Controllers: Most at 0% coverage (AiController, CookbookController, PaymentController)
    * Core Services: AiService (0%), PaymentService (0%), EmailService (3.8%)
    * Integration test traits need to be properly applied to existing tests

**15. API Server Project Modifications**

* **Refactoring into a testable state**: The only reason to modify anything under the `/Zarichney.Server/` directory is to refactor any code to make it testing compatible. You are at liberty to refactor any code that is not testable in it's current state. Confirm any refactoring and ensure no regression via automation test case implementation.

**16. Documentation & Maintenance (Requirements)**

* **General Documentation:** All general or solution related (non localized specific) documentation must be maintained in the `/Docs/` directory.
* **Testing Standards:** The overarching testing standards are documented in `../Docs/Standards/TestingStandards.md`. Specific guides for unit and integration test case development are located in `../Docs/Standards/UnitTestCaseDevelopment.md` and `../Docs/Standards/IntegrationTestCaseDevelopment.md` respectively. These documents must be reviewed prior to any assignment work.
* **Existing Documentation Standards:** `../Docs/Standards/DocumentationStandards.md` must be followed for the introduction of documentation within the `Zarichney.Server.Tests` project. Use the `../Docs/Templates/ReadmeTemplate.md` for the introduction of new README files. Note the emphasis on capturing the why and what, and not the how, as these are most beneficial as english documentation, while the how is well articulated via the code itself.
* **Maintenance:** Use the `/Docs/Maintenance/` for any requirement of manual setups, or beneficial documentation for a human solution maintainer.
* **Document Recommendations**: For anything that is out-of-scope from this technical design, please leave references where appropriate regarding future recommended enhancements to the test suite or testing framework. You are at liberty to implement any existing recommendations that you see fit, there is no limit to putting in additional effort in order to deliver the highest quality testing project. The ultimate goal to the overall endeavor is to maximize code quality. Your expertise is appreciated.

**17. Framework Augmentation Roadmap (TODOs)**

This section outlines planned enhancements to the testing framework. These items will be implemented incrementally.

* **17.1. Foundational Enhancements**
    * **TODO (FRMK-001): Implement Testable Time with `System.TimeProvider`**
        * *Goal:* Eliminate `DateTime.Now`/`UtcNow` direct usage in SUT; make time-dependent logic deterministic.
        * *Tasks:*
            * Update coding standards to mandate `TimeProvider` injection for time-sensitive operations.
            * Refactor existing SUT code where `DateTime.Now/UtcNow` is used to use an injected `TimeProvider`.
            * Provide `FakeTimeProvider` (from `Microsoft.Extensions.TimeProvider.Testing`) in `CustomWebApplicationFactory` for integration tests and demonstrate usage in unit tests.
            * Update `../Docs/Standards/UnitTestCaseDevelopment.md` and `../Docs/Standards/IntegrationTestCaseDevelopment.md` with guidance.
        * *Impacts:* `Zarichney.Server` (refactoring), `Zarichney.Server.Tests` (test setup, new base helpers if any).
        * *References:* Research Report Sec 6.2.
    * **TODO (FRMK-002): Standardize Advanced AutoFixture Customizations**
        * *Goal:* Improve realistic and complex test data generation; reduce boilerplate in tests.
        * *Tasks:*
            * Develop and document project-specific AutoFixture `ICustomization` and `ISpecimenBuilder` implementations for common/complex domain entities (e.g., EF Core models with relationships, DTOs with specific constraints). Store these in `Zarichney.Server.Tests/Framework/TestData/AutoFixtureCustomizations/`.
            * Ensure `OmitOnRecursionBehavior` is consistently applied for EF Core entities within these customizations.
            * Update `../Docs/Standards/UnitTestCaseDevelopment.md` and `../Docs/Standards/IntegrationTestCaseDevelopment.md` with guidance on using these advanced customizations.
        * *Impacts:* `Zarichney.Server.Tests` (new customization classes, test data setup).
        * *References:* Research Report Sec 4.1.1, 4.1.2.

* **17.2. Integration Test Dependency Management**
    * **TODO (FRMK-003): Enhance `Testcontainers` Usage in `DatabaseFixture`**
        * *Goal:* Ensure maximum stability and efficiency for database integration tests.
        * *Tasks:*
            * Review and confirm/implement image pinning (use specific PostgreSQL version).
            * Verify robust wait strategies are used (beyond simple port checks if necessary, e.g., waiting for a log message or health check).
            * Ensure Resource Reaper considerations are documented and defaults are appropriate.
            * Document how to add and manage other containerized services (e.g., Redis, RabbitMQ) if they become part of the architecture.
        * *Impacts:* `Zarichney.Server.Tests/Framework/Fixtures/DatabaseFixture.cs`.
        * *References:* Research Report Sec 4.2.1.
    * **TODO (FRMK-004): Integrate WireMock.Net for External HTTP Service Virtualization**
        * *Goal:* Provide stable, controllable mocks for external HTTP dependencies (e.g., Stripe, OpenAI, GitHub, MSGraph).
        * *Tasks:*
            * Integrate WireMock.Net into `CustomWebApplicationFactory` or a new dedicated fixture (e.g., `WireMockFixture`).
            * Define a standard way to configure WireMock.Net instances (port, stubs) per test class or suite, potentially loaded from `Zarichney.Server.Tests/Framework/Mocks/Virtualization/`.
            * Modify `CustomWebApplicationFactory` to redirect configured external service HTTP clients to the WireMock.Net instance(s).
            * Update `../Docs/Standards/IntegrationTestCaseDevelopment.md` with detailed guidance on setting up stubs and testing with WireMock.Net.
            * Update `Trait` for external services to use the existing pattern (e.g., `ExternalStripe`, `ExternalOpenAI`).
        * *Impacts:* `Zarichney.Server.Tests` (new fixture, mock setup, test logic), `CustomWebApplicationFactory`.
        * *References:* Research Report Sec 2.4.1, 6.5.

* **17.3. Advanced Testing Capabilities (Future Considerations)**
    * **TODO (FRMK-005): Evaluate and Pilot Consumer-Driven Contract Testing (PactNet)**
        * *Goal:* Ensure mocks/virtualizations of external services remain aligned with actual provider contracts.
        * *Tasks:*
            * Research PactNet integration with xUnit and ASP.NET Core.
            * Select one critical external HTTP dependency as a pilot.
            * Develop a PoC for generating and verifying pacts.
            * If successful, plan broader rollout and document in `../Docs/Standards/IntegrationTestCaseDevelopment.md`.
        * *Impacts:* `Zarichney.Server.Tests` (new dependencies, test structure for contract tests).
        * *References:* Research Report Sec 2.4.3, 6.5.

* **17.4. Process, Governance, and Developer Experience**
    * **TODO (FRMK-006): Establish Formal Production Code Testability Review Process**
        * *Goal:* Proactively improve SUT testability to support high coverage goals.
        * *Tasks:*
            * Integrate a testability review step into the definition of "done" for features/stories.
            * Checklist should include DI best practices, SOLID (esp. SRP, ISP), Humble Object pattern opportunities, and avoidance of anti-patterns.
            * Document this process in `../Docs/Standards/CodingStandards.md` or the overarching `../Docs/Standards/TestingStandards.md`.
        * *Impacts:* Development workflow, potentially `Zarichney.Server` code for refactoring.
        * *References:* Research Report Sec 3.
    * **TODO (FRMK-007): Document Patterns for Complex/Interconnected Test Data Seeding**
        * *Goal:* Provide clear guidance for setting up challenging integration test data scenarios.
        * *Tasks:*
            * Identify common scenarios requiring complex data graphs (e.g., user with multiple orders and specific order items).
            * Document patterns in `../Docs/Standards/IntegrationTestCaseDevelopment.md` combining `DatabaseFixture.ResetDatabaseAsync()`, advanced AutoFixture customizations, Test Data Builders, and (if necessary) minimal direct `DbContext` interaction for setup.
        * *Impacts:* Documentation, potentially new `TestData/Builders`.
        * *References:* Research Report Sec 4.2.2, 6.4.
    * **TODO (FRMK-008): Enhance AI Coder Support in Framework & Standards**
        * *Goal:* Continuously improve the ability of AI agents to generate high-quality, compliant tests.
        * *Tasks:*
            * Periodically review AI-generated tests against the standards.
            * Based on reviews, refine metadata tags, Gherkin-like templates, or other structured guidance in the standards documents (as per Report Sec 5).
            * Consider if new base classes, helper attributes, or utility methods in the test framework could simplify common tasks for AI.
        * *Impacts:* `../Docs/Standards/` documents, potentially `Zarichney.Server.Tests/Framework/`.
        * *References:* Research Report Sec 5.
    * **TODO (FRMK-009): Implement Detailed Coverage Analysis and Reporting in CI**
        * *Goal:* Track progress towards 90%+ coverage goal and maintain high standards.
        * *Tasks:*
            * Configure Coverlet in the GitHub Actions workflow to output reports in Cobertura format.
            * Integrate coverage reports with GitHub (e.g., as PR comments or checks).
            * Establish initial acceptable coverage thresholds and plan for gradual increases.
        * *Impacts:* `/.github/workflows/main.yml`.
