# Module/Directory: /api-server.Tests/Integration

**Last Updated:** 2025-04-22

> **Parent:** [`/api-server.Tests/Framework/`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Contains integration tests that verify the behavior of the `api-server` application by interacting with its API endpoints using a realistic, in-memory hosted environment.
* **Key Responsibilities:**
  * Testing the end-to-end flow of requests through controllers, services, repositories, and potentially the database.
  * Validating API contracts (request/response formats, status codes).
  * Verifying component interactions and integration points.
  * Testing authentication and authorization rules for API endpoints.
  * Checking behavior with mocked external services (Stripe, OpenAI, etc.).
* **Why it exists:** To provide confidence that the different parts of the application work together correctly as a whole and that the public API behaves as expected under various conditions.
* **Submodule Structure:** Tests are typically organized in subdirectories mirroring the `api-server/Controllers` structure (e.g., `./Controllers/AuthControllerTests.cs`, `./Controllers/CookbookControllerTests.cs`). Other categories like `./Performance/` or `./Smoke/` may also exist.

## 2. Architecture & Key Concepts (Refactoring Planned)

### Current State (Pre-Refactor)

* **Test Host:** Uses `CustomWebApplicationFactory` often instantiated per test class via `IClassFixture` in base classes.
* **API Client:** Uses `ApiClientFixture` which creates its *own* factory instance, shared via `ICollectionFixture` in the `"Integration Tests"` collection.
* **Collections:** Uses two separate collections (`"Integration Tests"` and `"Database Integration Tests"`) with different shared fixture setups.
* **Base Classes:** `IntegrationTestBase` and `DatabaseIntegrationTestBase` declare `IClassFixture<>`, leading to potential redundancy and confusion with collection fixtures.
* **Issues:** This leads to inefficient fixture instantiation (multiple factories) and potential configuration mismatches.

### Desired Future State (Consolidated Approach - TDD v1.5)

* **Single Collection:** All integration tests will belong to a single `[Collection("Integration")]`. This collection definition will provide shared instances of `CustomWebApplicationFactory`, `DatabaseFixture`, and `ApiClientFixture` using `ICollectionFixture<>`.
* **Shared Fixtures:** This ensures only one instance of the application host, database container (if used), and API client fixture is created and shared across all tests in the collection, improving performance and consistency.
* **Test Host (`CustomWebApplicationFactory`):** The single shared factory instance will be configured according to TDD v1.5 (refined config loading, prioritized DB selection).
* **API Client (`ApiClientFixture`):** The single shared instance will use the shared factory for client creation and configuration access.
* **Base Classes:**
  * `IntegrationTestBase`: Serves as the primary base, accepting shared fixtures (`CustomWebApplicationFactory`, `ApiClientFixture`) via constructor injection (but *without* declaring `IClassFixture<>`). Provides common setup (like dependency checking) and accessors.
  * `DatabaseIntegrationTestBase`: Inherits `IntegrationTestBase`, accepts the shared `DatabaseFixture` via constructor injection (without declaring `IClassFixture<>`), and provides database-specific helpers (like `ResetDatabaseAsync`).
* **Database Interaction:** Tests requiring database access will inherit `DatabaseIntegrationTestBase` and use the shared `DatabaseFixture`. Migrations will be applied automatically by the fixture. `ResetDatabaseAsync` must be called before seeding/mutating data.
* **Mocking:** Mocks for external services are registered in the shared factory and retrieved via `Factory.Services.GetRequiredService<Mock<IService>>()`.
* **Authentication:** Authenticated calls use the client from the shared `ApiClientFixture` (`AuthenticatedApiClient` property in base class).
* **Dependency Skipping:** Continues to use `[DependencyFact]` and dependency traits along with logic in `IntegrationTestBase` to automatically skip tests if required configurations are unavailable.

## 3. Interface Contract & Assumptions (Future State)

* **Tests as Consumers:** Integration tests act as consumers of the `api-server` API (via the Refit client), the shared test fixtures, and the base classes.
* **Critical Assumptions:**
  * Assumes the shared `CustomWebApplicationFactory`, `DatabaseFixture`, and `ApiClientFixture` are correctly configured and functional according to TDD v1.5.
  * Assumes the generated Refit client in [`/api-server.Tests/Framework/Client/`](../Client/README.md) is up-to-date with the actual API contract.
  * Assumes the dependency checking mechanism accurately reflects the availability of necessary configurations.
  * Assumes mocks provided by the factories behave as expected (or are correctly configured within the test).

## 4. Local Conventions & Constraints (Future State)

* **Test Structure:** Follow AAA pattern. Use descriptive method names (`[MethodName]_[Scenario]_[ExpectedOutcome]`).
* **Assertions:** Use `FluentAssertions`. Assert on response status codes, DTO contents (`Should().BeEquivalentTo()`), and expected side effects. Include `.Because("...")` clauses.
* **Collection:** All integration tests **MUST** belong to the `[Collection("Integration")]`.
* **Base Class Inheritance:** Tests **MUST** inherit `IntegrationTestBase`. Tests requiring database access **MUST** inherit `DatabaseIntegrationTestBase`.
* **Traits:** Must use `[Trait("Category", "Integration")]` and relevant `Feature`, `Dependency`, and `Mutability` traits. Use `[DependencyFact]` for tests with external dependencies.
* **API Interaction:** **MUST** use the Refit client provided by the base class (`ApiClient` or `AuthenticatedApiClient`). Direct service calls are forbidden.
* **Database Cleanup:** Call `await ResetDatabaseAsync()` at the start of tests modifying or relying on specific DB state.

## 5. How to Work With This Code (Future State)

* **Setup:** Ensure Docker is running if database tests are included. Run `Scripts/GenerateApiClient.ps1` if API contracts have changed. Ensure necessary configurations (user secrets, environment variables) are set for the desired scenario (local dev vs. CI).
* **Running Tests:** Use `dotnet test --filter "Category=Integration"`. Filter further by `Feature`, `Dependency`, or `Mutability` traits as needed.
* **Adding Tests:**
  1.  Create the test class in the appropriate subdirectory (e.g., `./Controllers/MyController/`).
  2.  Add `[Collection("Integration")]`.
  3.  Inherit from `IntegrationTestBase` or `DatabaseIntegrationTestBase` as needed.
  4.  Apply appropriate `[Trait]` and `[DependencyFact]` attributes.
  5.  Use `ApiClient` or `AuthenticatedApiClient` properties from the base class to make API calls.
  6.  If inheriting `DatabaseIntegrationTestBase`, call `ResetDatabaseAsync()` when necessary.
* **Common Pitfalls / Gotchas:**
  * Outdated Refit client.
  * Forgetting `ResetDatabaseAsync()`.
  * Missing `[Collection("Integration")]` attribute.
  * Inheriting the wrong base class (`IntegrationTestBase` when `DatabaseIntegrationTestBase` is needed).
  * Configuration issues causing unexpected skips or failures.

## 6. Dependencies

* **Internal Code Dependencies:**
  * [`/api-server.Tests/Framework/Fixtures/`](../Fixtures/README.md) - Core test environment setup (Factory, DB Fixture, API Client Fixture).
  * [`/api-server.Tests/Framework/Client/`](../Client/README.md) - Generated API client.
  * [`/api-server.Tests/Framework/Helpers/`](../Helpers/README.md) - Authentication and utility helpers.
  * [`/api-server.Tests/TestData/Builders/`](../../TestData/Builders/README.md) - For creating request DTOs.
  * [`/api-server.Tests/Framework/Mocks/Factories/`](../Mocks/Factories/README.md) - Relies on mocks registered by the factory.
* **External Library Dependencies:**
  * `Xunit` - Test framework.
  * `FluentAssertions` - Assertion library.
  * `Refit` - Used by the generated client.
* **Dependents (Impact of Changes):** CI/CD workflows consume the results of these tests.

## 7. Rationale & Key Historical Context

* Integration tests provide the highest level of confidence that the API functions correctly as a whole. Using an in-memory host with Testcontainers provides a balance of realism and test performance/isolation. Consolidating fixture management via a single `ICollectionFixture` definition improves efficiency and maintainability.

## 8. Known Issues & TODOs

* ~~Implement the fixture/collection consolidation described in Section 2 (Future State) and the [`/api-server.Tests/Framework/Fixtures/README.md`](../Fixtures/README.md). This includes:~~ ✅ Completed
  * ~~Defining the single `"Integration"` collection.~~ ✅ Completed
  * ~~Removing old collection definitions.~~ ✅ Completed
  * ~~Refactoring `ApiClientFixture`.~~ ✅ Completed
  * ~~Simplifying base classes (`IntegrationTestBase`, `DatabaseIntegrationTestBase`) by removing `IClassFixture<>`.~~ ✅ Completed
  * ~~Updating all test classes to use `[Collection("Integration")]` and inherit the correct base class.~~ ✅ Completed
* **TODO:** Implement comprehensive test coverage for all major API endpoints and scenarios.
* **TODO:** Continuously monitor test flakiness and performance after refactoring.

