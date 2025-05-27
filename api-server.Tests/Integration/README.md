# README: /Integration Tests Directory

**Version:** 1.1
**Last Updated:** 2025-05-22
**Parent:** `../README.md`

## 1. Purpose & Responsibility

This directory, `/Integration/`, houses all **integration tests** for the `zarichney-api/api-server` project. The core purpose of these tests is to verify that different components of the application interact correctly to deliver end-to-end functionalities as expected.

Integration tests in this project typically focus on:
* **API Endpoint Contracts:** Validating request/response behavior, status codes, and DTO serialization for all public API endpoints.
* **Component Collaboration:** Ensuring services, repositories, controllers, and middleware function together correctly.
* **Data Persistence:** Verifying interactions with a real database instance (PostgreSQL managed by Testcontainers via `DatabaseFixture`).
* **Authentication and Authorization:** Testing access control mechanisms for secured endpoints.
* **Interactions with External Services:** Validating behavior when interacting with external HTTP services (which will be virtualized using WireMock.Net as per TDD FRMK-004).

These tests provide a high level of confidence that the `api-server` operates as a cohesive system. All integration tests **must** adhere to the detailed guidelines and standards outlined in **`../../../Zarichney.Standards/Standards/IntegrationTestCaseDevelopment.md`**.

## 2. Integration Testing Approach & Key Concepts

Our integration testing strategy is built upon a robust framework designed for realistic and reliable testing:

* **In-Memory Hosting:** The `api-server` is hosted in-memory using `CustomWebApplicationFactory<Program>`, which allows tests to make HTTP requests to the application as if it were deployed, but without actual network overhead for the host itself.
* **Real Database Interaction:** Database operations are tested against a genuine PostgreSQL instance managed by **Testcontainers** via the `DatabaseFixture`. This fixture handles container lifecycle, applies EF Core migrations, and provides database cleanup using **Respawn**.
* **Type-Safe API Clients:** All API interactions are performed using **Refit clients** (multiple granular interfaces), auto-generated from the `api-server`'s OpenAPI specification.
* **Simulated Authentication:** The `TestAuthHandler` and `AuthTestHelper` enable simulation of various authenticated users, roles, and claims.
* **External Service Virtualization (Planned):** External HTTP dependencies will be virtualized using **WireMock.Net** (TDD FRMK-004) to ensure deterministic test outcomes. Until fully implemented, interfaces for these services are mocked via DI using factories from `../Framework/Mocks/Factories/`.
* **Shared Fixtures:** Expensive resources like `CustomWebApplicationFactory`, `DatabaseFixture`, and `ApiClientFixture` are shared across test classes using xUnit's `ICollectionFixture` mechanism within the `"Integration"` collection for optimal performance.
* **Base Test Classes:** `IntegrationTestBase` and `DatabaseIntegrationTestBase` provide common setup and access to shared fixtures.
* **Conditional Test Execution:** The `[DependencyFact]` attribute is used to skip tests if their required configurations or infrastructure (like Docker) are unavailable.
* **Core Tooling:** xUnit, FluentAssertions, AutoFixture (especially for DTOs and complex test data setup).

## 3. Directory Structure & Organization

The structure within `/Integration/` is generally organized by the feature area or API controllers being tested. This helps in locating tests relevant to specific parts of the application.

Key subdirectories typically include:
* **`./Controllers/`**: Contains tests targeting specific API controllers. These are often further subdivided by controller name (e.g., `./Controllers/AuthController/`, `./Controllers/CookbookControllers/`).
* **`./Services/`**: May contain tests that verify the integration of services with other components like the database, especially if these interactions are not fully covered or are too complex to test solely through API endpoints. (e.g., `./Services/Status/` for testing status and feature availability middleware).
* **`./Smoke/`**: Basic "smoke tests" that verify critical application paths and ensure the overall system is operational.
* **`./Performance/`**: Contains tests focused on basic performance checks or boilerplate for future, more detailed performance testing. (Currently very minimal).
* Other directories may be created as needed for specific integration scenarios (e.g., testing specific workflows, background task integrations).

Each significant subdirectory under `/Integration/` should have its own `README.md` detailing its specific focus, key test scenarios covered, and any unique setup or considerations.

## 4. Key Standards & Development Guide

The definitive guide for writing integration tests in this project is:
* **`../../../Zarichney.Standards/Standards/IntegrationTestCaseDevelopment.md`**

This document provides detailed instructions on:
* Core principles of integration testing.
* Utilizing the integration testing framework (fixtures, base classes, API client).
* Managing dependencies (databases with Testcontainers, external services with WireMock.Net).
* Simulating authentication and authorization.
* Effective test data management for integration scenarios.
* Writing robust assertions for API responses and database states.
* Common pitfalls and best practices.

All developers (human and AI) writing integration tests **must** adhere to this guide.
Supporting standards include:
* `../../../Zarichney.Standards/Standards/TestingStandards.md` (Overarching principles and tooling).
* `../TechnicalDesignDocument.md` (Provides the architectural blueprint for the testing framework used by these tests).

## 5. How to Work With This Code

### Writing New Integration Tests

1.  **Identify Scenario:** Determine the specific interaction, API endpoint, or workflow to be tested.
2.  **Create Test File:** Following the directory structure and naming conventions, create or locate the appropriate test file in `/Integration/`.
3.  **Inherit Base Class:** Inherit from `IntegrationTestBase` or `DatabaseIntegrationTestBase` (if database interaction is involved). Ensure the class is part of the `[Collection("Integration")]`.
4.  **Follow `IntegrationTestCaseDevelopment.md`:**
    * Apply the AAA pattern.
    * Use the `ApiClient` (or an authenticated version via `AuthHelper`) for all API calls.
    * If using `DatabaseIntegrationTestBase`, call `await DbFixture.ResetDatabaseAsync();` at the start of tests that require a clean DB state.
    * Set up any necessary data using API calls, `DbFixture.GetContext()`, or Test Data Builders/AutoFixture.
    * Configure mock service behavior (via `Factory.Services.GetRequiredService<Mock<IYourService>>()`) or WireMock.Net stubs (once implemented) for external dependencies.
    * Write clear assertions using FluentAssertions on API responses, database state, or interactions with virtualized services.
5.  **Categorize:** Add `[Trait("Category", "Integration")]` and any relevant dependency traits (e.g., `"Database"`, `"ExternalHttp:[ServiceName]"`). Use `[DependencyFact]` if the test relies on configurations that might not always be present.

### Running Integration Tests

* **Prerequisites:**
    * **Docker:** Must be running if tests use `DatabaseFixture` or other Testcontainer-based resources.
    * **Configuration:** Ensure necessary configurations (e.g., in `../appsettings.Testing.json` or user secrets for local development) are available for tests that might be skipped by `[DependencyFact]`.
* **Run all integration tests:**
  ```bash
  dotnet test --filter "Category=Integration"
  ```
* **Run specific integration tests:** Use more specific filters:
  ```bash
  dotnet test --filter "FullyQualifiedName~MyNamespace.MyIntegrationTestsClass"
  ```
* Ensure all relevant integration tests pass locally before committing code, as per the workflow in `../../../Zarichney.Standards/Standards/TestingStandards.md`. Remember to run `Scripts/generate-api-client.ps1` if API contracts were changed.

## 6. Dependencies

### Internal Dependencies

* **`api-server` Project:** The System Under Test.
* **`../Framework/`**: This entire directory heavily relies on components from the `../Framework/` directory, including all fixtures (`CustomWebApplicationFactory`, `DatabaseFixture`, `ApiClientFixture`), helpers (`AuthTestHelper`), the Refit clients (`Client/` directory), attributes (`DependencyFactAttribute`), and mock infrastructure.
* **`../TestData/`**: May use Test Data Builders or static sample files from this directory.

### Key External Libraries

* **`xUnit.net`**: The test framework.
* **`FluentAssertions`**: The assertion library.
* **`AutoFixture`**: For generating test data, especially DTOs and complex inputs.
* **`Microsoft.AspNetCore.Mvc.Testing`**: For `WebApplicationFactory`.
* **`Testcontainers.PostgreSql`**: For database testing.
* **`Respawn.Postgres`**: For database cleanup.
* **`Refit`**: For the API client.
* **`WireMock.Net`** (Planned): For HTTP service virtualization.
* **`Moq`**: Used when retrieving and configuring mocks from the `CustomWebApplicationFactory`'s service provider.

## 7. Rationale & Key Historical Context

Integration tests are a vital part of the testing strategy, providing assurance that the system's components work together as designed.
* **Realistic Testing:** The use of `CustomWebApplicationFactory` with a real (Testcontainer-managed) database provides a high-fidelity test environment.
* **API Contract Validation:** Testing through the Refit API clients ensures that the API behaves according to its OpenAPI specification.
* **Performance & Reliability:** The shared fixture model (`IntegrationCollection`) was adopted to manage expensive resources efficiently. The planned introduction of WireMock.Net will further enhance reliability by isolating tests from external network dependencies.

These tests bridge the gap between fast, isolated unit tests and potentially slower, more complex end-to-end tests (which are currently out of scope).

## 8. Known Issues & TODOs

* **WireMock.Net Integration (TDD FRMK-004):** Full implementation of HTTP service virtualization using WireMock.Net is a key pending task. Current tests requiring external HTTP services mock the internal service interface instead.
* **Coverage Expansion:** Continuously expand integration test coverage for all critical API endpoints, workflows, and error conditions. (Tracked via tasks referencing the `../../../Zarichney.Standards/Templates/GHTestCoverageTask.md` template).
* **Performance Monitoring:** While the shared fixture model helps, the overall execution time of the integration test suite should be monitored, especially as more tests are added.
* **Complex Scenarios:** Developing robust data setup and verification for very complex multi-step business workflows can be challenging and may require further refinement of Test Data Builder patterns or advanced AutoFixture customizations.
* Refer to the "Framework Augmentation Roadmap (TODOs)" in `../TechnicalDesignDocument.md` for broader framework enhancements.

---
