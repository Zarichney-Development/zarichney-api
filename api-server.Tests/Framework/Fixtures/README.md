# Module/Directory: /api-server.Tests/Framework/Fixtures

**Last Updated:** 2025-04-21

> **Parent:** [`/api-server.Tests/Framework/`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Contains xUnit fixtures responsible for setting up and managing the test environment for integration tests.
* **Key Responsibilities:**
  * Bootstrapping the `api-server` application in-memory using `WebApplicationFactory` (`CustomWebApplicationFactory`).
  * Managing the lifecycle of a dedicated PostgreSQL test database container (`DatabaseFixture`).
  * Providing shared, configured API client instances (`ApiClientFixture`).
  * Configuring the test application's services, including mocking external dependencies and setting up test authentication.
  * Providing mechanisms for database cleanup between tests (via Respawn in `DatabaseFixture`).
* **Why it exists:** To provide reusable, isolated, and configurable test environments for running integration tests against the API, ensuring consistency and managing shared resources like the database container and application host.

## 2. Architecture & Key Concepts (Current & Future State)

### Current State (Pre-Refactor)

* **`CustomWebApplicationFactory`**: Inherits `WebApplicationFactory<Program>`. Configures services, mocks, test auth, and configuration loading. Intended to provide the application host. Often used via `IClassFixture` in base test classes.
* **`DatabaseFixture`**: Manages PostgreSQL Testcontainer via `Testcontainers.PostgreSql` and `IAsyncLifetime`. Provides connection string and `ResetDatabaseAsync` using Respawn. Intended for use via `ICollectionFixture` in a dedicated database collection.
* **`ApiClientFixture`**: Creates its *own internal* `CustomWebApplicationFactory` instance. Reads test user config via `TestConfigurationHelper`, attempts login, and provides shared `IZarichneyAPI` clients. Used via `ICollectionFixture` in a separate integration collection.
* **Issues:** This setup leads to multiple `CustomWebApplicationFactory` instances, inconsistent configuration between the `ApiClientFixture`'s factory and the test's factory, and inefficient resource usage. Fixture management is spread across `IClassFixture` in base classes and multiple `ICollectionFixture` definitions.

### Desired Future State (Consolidated Approach - TDD v1.5)

* **Single Collection:** A single `[Collection("Integration")]` definition will manage *all three* fixtures (`CustomWebApplicationFactory`, `DatabaseFixture`, `ApiClientFixture`) using `ICollectionFixture<>`. This ensures only one instance of each fixture is created and shared across all tests in the collection.
* **`CustomWebApplicationFactory` (Shared):**
  * Will implement the refined configuration loading strategy (supporting user secrets, env vars, `appsettings.Testing.json`) as defined in TDD v1.5.
  * Will implement the prioritized database selection logic (Config -> Testcontainers -> InMemory).
  * Will be provided as a shared instance by the `"Integration"` collection.
* **`DatabaseFixture` (Shared):**
  * Will remain largely the same but **must** include logic to automatically apply EF Core migrations after the container starts successfully (`InitializeAsync`).
  * Will be provided as a shared instance by the `"Integration"` collection.
* **`ApiClientFixture` (Shared & Refactored):**
  * Will **receive** the shared `CustomWebApplicationFactory` instance via constructor injection.
  * Will **not** create its own factory instance.
  * Will use the injected factory to create its `HttpClient` and resolve the final `IConfiguration` to get `TestUser` credentials.
  * Will be provided as a shared instance by the `"Integration"` collection.
* **Base Classes (`IntegrationTestBase`, `DatabaseIntegrationTestBase`):**
  * Will be simplified by removing `IClassFixture<>` declarations.
  * Will continue to accept shared fixture instances via constructor injection (provided by the collection).
* **Test Classes:**
  * All integration tests will belong to the `[Collection("Integration")]`.
  * Tests requiring database access will inherit `DatabaseIntegrationTestBase`.

## 3. Interface Contract & Assumptions (Future State)

* **`CustomWebApplicationFactory`**:
  * Provides a single, shared, configured instance of the `api-server` application via `ICollectionFixture`.
  * Correctly loads configuration according to TDD v1.5.
  * Correctly configures `UserDbContext` based on the prioritized strategy.
  * Registers mocks and test authentication.
* **`DatabaseFixture`**:
  * Provides a single, shared PostgreSQL container instance via `ICollectionFixture`.
  * Applies migrations successfully during `InitializeAsync`. Throws/skips if container or migrations fail.
  * Provides `ResetDatabaseAsync` for test cleanup.
  * Provides `ConnectionString`.
* **`ApiClientFixture`**:
  * Provides shared, initialized `UnauthenticatedClient` and `AuthenticatedClient` instances via `ICollectionFixture`.
  * Uses the shared `CustomWebApplicationFactory` for client creation and configuration access.
  * Handles test user login based on configuration resolved from the shared factory.
* **Critical Assumptions:**
  * Docker Desktop (or equivalent) is running for tests requiring the `DatabaseFixture`.
  * The configurations specified in TDD v1.5 are adhered to.
  * User secrets are configured correctly for local development scenarios.
  * `TestUser` credentials exist in a loadable configuration source (`appsettings.Testing.json`, user secrets, or env vars).

## 4. Local Conventions & Constraints (Future State)

* **Fixture Usage:** All three fixtures (`CustomWebApplicationFactory`, `DatabaseFixture`, `ApiClientFixture`) are managed and shared via `ICollectionFixture` within a single `[Collection("Integration")]`.
* **Base Classes:** `IntegrationTestBase` and `DatabaseIntegrationTestBase` provide common functionality and receive shared fixtures via constructor injection (without implementing `IClassFixture<>`).
* **Mocking:** Relies on mock factories located in [`/api-server.Tests/Framework/Mocks/Factories/`](../Mocks/Factories/README.md).
* **Configuration:** The shared `CustomWebApplicationFactory` defines the primary configuration loading strategy for all integration tests.

## 5. How to Work With This Code (Future State)

* **Setup:** Ensure Docker is running if database tests are included. Ensure user secrets are set up for local development testing against a real DB.
* **Testing:** Inherit from `IntegrationTestBase` or `DatabaseIntegrationTestBase`. Add `[Collection("Integration")]` to the test class. Use the injected fixtures or helper properties from the base class. Call `await ResetDatabaseAsync()` in database tests before mutating actions or seeding data.
* **Common Pitfalls / Gotchas:**
  * Forgetting `await ResetDatabaseAsync()` can cause state leakage.
  * Ensure test classes have the correct `[Collection("Integration")]` attribute.
  * Ensure tests needing the DB inherit `DatabaseIntegrationTestBase`.
  * Remember that configuration loading depends on the environment (`Development` default, override via `ASPNETCORE_ENVIRONMENT`/`DOTNET_ENVIRONMENT`).

## 6. Dependencies

* **Internal Code Dependencies:**
  * [`/api-server.Tests/Framework/Helpers/`](../Helpers/README.md)
  * [`/api-server.Tests/Framework/Mocks/Factories/`](../Mocks/Factories/README.md)
  * `api-server/Program.cs`
* **External Library Dependencies:**
  * `Microsoft.AspNetCore.Mvc.Testing`
  * `Testcontainers.PostgreSql`
  * `Respawn`
  * `Xunit`
  * `Moq`, `Refit`, `FluentAssertions`, `AutoFixture` (used by dependents)
* **Dependents (Impact of Changes):**
  * [`/api-server.Tests/Integration/`](../Integration/README.md) - All integration tests rely on these fixtures.

## 7. Rationale & Key Historical Context

* The consolidated fixture strategy using a single `ICollectionFixture` definition provides the most efficient and consistent way to manage shared resources like the application host, database container, and API clients across the entire integration test suite.
* The refined configuration and database handling logic allows tests to run correctly and predictably across different environments (local dev with secrets, CI with Testcontainers).

## 8. Known Issues & TODOs (Refactoring Plan)

* **TODO:** Define the single `[Collection("Integration")]` class providing `CustomWebApplicationFactory`, `DatabaseFixture`, and `ApiClientFixture` via `ICollectionFixture<>`.
* **TODO:** Remove the old collection definition classes (`IntegrationTestCollection.cs`, `DatabaseIntegrationTestCollection.cs`).
* **TODO:** Refactor `ApiClientFixture` to remove internal `CustomWebApplicationFactory` creation. Inject the shared `CustomWebApplicationFactory` via the constructor. Use the injected factory to create `HttpClient` and resolve `IConfiguration`. Remove `DisposeAsync` logic related to the internal factory.
* **TODO:** Refactor `CustomWebApplicationFactory.ConfigureWebHost` to implement the configuration loading order specified in TDD v1.5 (Default Env -> JSON files -> User Secrets (Dev) -> Env Vars). Remove the `AddInMemoryCollection` override for the connection string.
* **TODO:** Refactor `CustomWebApplicationFactory.ConfigureTestServices` to implement the prioritized database selection logic (Config -> `DatabaseFixture` -> InMemory) and configure `UserDbContext` accordingly.
* **TODO:** Implement automatic EF Core migration application within `DatabaseFixture.InitializeAsync` after the container starts. Ensure proper error handling if migrations fail.
* **TODO:** Remove `IClassFixture<>` interface declarations from `IntegrationTestBase` and `DatabaseIntegrationTestBase`. Ensure constructors still accept the fixtures.
* **TODO:** Add `[Collection("Integration")]` to all integration test classes.
* **TODO:** Ensure all test classes requiring database access inherit from `DatabaseIntegrationTestBase` (e.g., fix `LoginEndpointTests.cs`).
* **TODO:** Verify `TestUser` credentials are still loaded correctly from configuration and used successfully by the refactored `ApiClientFixture`.

