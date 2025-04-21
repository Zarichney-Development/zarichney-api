# Module/Directory: /api-server.Tests/Integration

**Last Updated:** 2025-04-21

> **Parent:** [`api-server.Tests`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Contains integration tests that verify the behavior of the `api-server` application by interacting with its API endpoints.
* **Key Responsibilities:**
    * Testing the end-to-end flow of requests through controllers, services, repositories, and potentially the database.
    * Validating API contracts (request/response formats, status codes).
    * Verifying component interactions and integration points.
    * Testing authentication and authorization rules for API endpoints.
    * Checking behavior with mocked external services (Stripe, OpenAI, etc.).
* **Why it exists:** To provide confidence that the different parts of the application work together correctly as a whole and that the public API behaves as expected under various conditions.
* **Submodule Structure:** Tests are typically organized in subdirectories mirroring the `api-server/Controllers` structure (e.g., `./Controllers/AuthControllerTests.cs`, `./Controllers/CookbookControllerTests.cs`). Other categories like `./Performance/` or `./Smoke/` may also exist.

## 2. Architecture & Key Concepts

* **Test Host:** Uses `CustomWebApplicationFactory` from [`/api-server.Tests/Framework/Fixtures/`](Framework/Fixtures/README.md) to host the `api-server` in-memory.
* **API Client:** Tests interact with the API exclusively through `IZarichneyAPI` instances provided by `ApiClientFixture` and exposed via `IntegrationTestBase` properties (`ApiClient`, `AuthenticatedApiClient`).
* **ApiClientFixture:** Implements `IAsyncLifetime` to create shared, pre-configured authenticated/unauthenticated Refit clients, reading credentials from `appsettings.Testing.json` and managing login during setup.
* **Collection Fixture:** `IntegrationTestCollection` defines the "Integration Tests" collection, applying `ICollectionFixture<ApiClientFixture>` so all tests in this collection share the same clients.
* **Base Classes:** `IntegrationTestBase` is marked with `[Collection("Integration Tests")]`, injects `ApiClientFixture`, and exposes `ApiClient` and `AuthenticatedApiClient` for direct use in tests.
* **Database Interaction:** Tests requiring database access use `DatabaseFixture` and must call `await ResetDatabaseAsync()` at the start.
* **Mocking:** Mocks for external services are registered in the factory and retrieved via `Factory.Services.GetRequiredService<Mock<IService>>()`.
* **Authentication:** Authenticated calls use the client from `AuthenticatedApiClient`; no per-test client creation is needed.
* **Dependency Skipping:** Uses `[DependencyFact]` and dependency traits (e.g., `[Trait(TestCategories.Dependency, TestCategories.Database)]`) along with logic in `IntegrationTestBase` to automatically skip tests if required configurations (API keys, database connection) are unavailable.

## 3. Interface Contract & Assumptions

* **Tests as Consumers:** Integration tests act as consumers of the `api-server` API, the generated Refit client, and the test fixtures.
* **Critical Assumptions:**
    * Assumes `CustomWebApplicationFactory` and `DatabaseFixture` are correctly configured and functional.
    * Assumes the generated Refit client in [`/api-server.Tests/Framework/Framework/Client/`](../Framework/Client/README.md) is up-to-date with the actual API contract.
    * Assumes the dependency checking mechanism (`IntegrationTestBase`, `ConfigurationStatusHelper`) accurately reflects the availability of necessary configurations.
    * Assumes mocks provided by the factories behave as expected (or are correctly configured within the test).

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Test Structure:** Follow AAA pattern. Use descriptive method names (`[MethodName]_[Scenario]_[ExpectedOutcome]`).
* **Assertions:** Use `FluentAssertions`. Assert on response status codes, DTO contents (`Should().BeEquivalentTo()`), and expected side effects (e.g., database state changes, mock service calls verified). Include `.Because("...")` clauses.
* **Traits:** Must use `[Trait("Category", "Integration")]` and relevant `Feature`, `Dependency`, and `Mutability` traits. Use `[DependencyFact]` for tests with external dependencies.
* **API Interaction:** **MUST** use the Refit client. Direct service calls are forbidden.
* **Database Cleanup:** Call `await ResetDatabaseAsync()` at the start of tests modifying or relying on specific DB state.

## 5. How to Work With This Code

* **Setup:** Ensure Docker is running if database tests are included. Run `Scripts/GenerateApiClient.ps1` if API contracts have changed. Ensure necessary configurations (user secrets, environment variables) are set if not relying on skipping.
* **Running Tests:** Use `dotnet test --filter "Category=Integration"`. Filter further by `Feature`, `Dependency`, or `Mutability` traits as needed (e.g., `dotnet test --filter "Category=Integration&Feature=Auth"`).
* **Adding Tests:**
    1. Inherit from `IntegrationTestBase` (which provides `ApiClient` and `AuthenticatedApiClient`).
    2. Apply appropriate `[Trait]` and `[DependencyFact]` attributes.
    3. Use `ApiClient` or `AuthenticatedApiClient` properties to make API calls.
    4. Call `ResetDatabaseAsync()` if the test modifies or relies on specific DB state.
* **Common Pitfalls / Gotchas:**
    * Outdated Refit client.
    * Forgetting `ResetDatabaseAsync()`.
    * Incorrect trait usage leading to tests not running/skipping appropriately.
    * Tests interfering with each other due to shared state (though fixtures aim to prevent this).
    * Configuration issues (missing secrets/env vars) causing unexpected skips or failures if dependency checking isn't perfect.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/api-server.Tests/Framework/Fixtures/`](Framework/Fixtures/README.md) - Core test environment setup.
    * [`/api-server.Tests/Framework/Framework/Client/`](../Framework/Client/README.md) - Generated API client.
    * [`/api-server.Tests/Framework/Helpers/`](Framework/Helpers/README.md) - Authentication and utility helpers.
    * [`/api-server.Tests/TestData/Builders/`](../TestData/Builders/README.md) - For creating request DTOs.
    * [`/api-server.Tests/Framework/Mocks/Factories/`](../Mocks/Factories/README.md) - Relies on mocks registered by the factory.
* **External Library Dependencies:**
    * `Xunit` - Test framework.
    * `FluentAssertions` - Assertion library.
    * `Refit` - Used by the generated client.
* **Dependents (Impact of Changes):** CI/CD workflows consume the results of these tests.

## 7. Rationale & Key Historical Context

* Integration tests provide the highest level of confidence that the API functions correctly as a whole, bridging the gap between unit tests and real-world usage. Using an in-memory host with Testcontainers provides a balance of realism and test performance/isolation.

## 8. Known Issues & TODOs

* Need to implement comprehensive test coverage for all major API endpoints and scenarios.
* Continuously monitor test flakiness and performance.