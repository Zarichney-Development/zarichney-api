# Module/Directory: /api-server.Tests/Framework/Fixtures

**Last Updated:** 2025-04-21

> **Parent:** [`api-server.Tests`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Contains xUnit fixtures responsible for setting up and managing the test environment for integration tests.
* **Key Responsibilities:**
    * Bootstrapping the `api-server` application in-memory using `WebApplicationFactory` (`CustomWebApplicationFactory`).
    * Managing the lifecycle of a dedicated PostgreSQL test database container (`DatabaseFixture`).
    * Configuring the test application's services, including mocking external dependencies and setting up test authentication.
    * Providing mechanisms for database cleanup between tests (via Respawn in `DatabaseFixture`).
* **Why it exists:** To provide reusable, isolated, and configurable test environments for running integration tests against the API, ensuring consistency and managing shared resources like the database container.

## 2. Architecture & Key Concepts

* **`CustomWebApplicationFactory`**:
    * Inherits from `WebApplicationFactory<Program>`.
    * Overrides `ConfigureWebHost` to customize the application's configuration (`IConfiguration`) and services (`IServiceCollection`) for testing.
    * Adds test-specific configuration sources (`appsettings.Testing.json`, environment variables, etc.).
    * Registers mock implementations for external services (e.g., `IStripeService`, `ILlmService`) using factories from [`/api-server.Tests/Framework/Mocks/Factories/`](../Mocks/Factories/README.md).
    * Registers the `TestAuthHandler` from [`/api-server.Tests/Framework/Helpers/`](Framework/Helpers/README.md) to enable simulated authentication.
    * Configures the `UserDbContext` to use the connection string provided by `DatabaseFixture`.
    * Provides methods to create `HttpClient` and Refit clients, including authenticated versions.
* **`DatabaseFixture`**:
    * Implements `IAsyncLifetime` for setup and teardown coordinated by xUnit.
    * Uses `Testcontainers.PostgreSql` to create and manage a PostgreSQL container instance.
    * Uses `Respawn` to efficiently clean database tables between test runs via the `ResetDatabaseAsync` method.
    * Provides the connection string to the running test container.
    * Handles Docker availability checks, throwing `TestSkippedException` if the container cannot be started.
* **`ApiClientFixture`**:
    * Implements `IAsyncLifetime` to create and manage shared `IZarichneyAPI` clients (unauthenticated & authenticated) for integration tests.
    * Instantiates its own `CustomWebApplicationFactory`, reads `TestUser:Username` and `TestUser:Password` from `appsettings.Testing.json` via `TestConfigurationHelper`, logs in to obtain auth, and exposes two clients via `UnauthenticatedClient` and `AuthenticatedClient` properties.
    * Used via `ICollectionFixture<ApiClientFixture>` in the "Integration Tests" collection defined by `IntegrationTestCollection.cs`.
    * Improves performance by reusing initialized clients across multiple tests.

## 3. Interface Contract & Assumptions

* **`CustomWebApplicationFactory`**:
    * **Purpose:** Provides a configured instance of the `api-server` application running in-memory for testing.
    * **Critical Preconditions:** Assumes `Program.cs` is correctly set up for `WebApplicationFactory`. Assumes necessary configuration files (`appsettings.json`, `appsettings.Testing.json`) exist or are optional as configured.
    * **Critical Postconditions:** Provides access to the application's `IServiceProvider` and methods to create `HttpClient`/Refit clients. Correctly registers mocks and test authentication.
    * **Non-Obvious Error Handling:** Internal handling of `DatabaseFixture` initialization attempts to allow tests to run even if the DB container fails to start (sets `IsDatabaseAvailable` flag). Configuration loading follows specific order defined in TDD.
* **`DatabaseFixture`**:
    * `Task InitializeAsync()`: Starts the Docker container. Throws `TestSkippedException` if Docker is unavailable/misconfigured. Initializes Respawner.
    * `Task DisposeAsync()`: Stops and cleans up the Docker container.
    * `Task ResetDatabaseAsync()`: Cleans data from tables using Respawner. Must be called explicitly by tests needing a clean state.
    * `string ConnectionString`: Provides the connection string to the running container. Throws `InvalidOperationException` if accessed before the container is started.
* **`ApiClientFixture`**:
    * **Purpose:** Provides shared API clients for integration tests.
    * **Critical Preconditions:** Assumes `appsettings.Testing.json` contains valid test user credentials.
    * **Critical Postconditions:** Exposes initialized `UnauthenticatedClient` and `AuthenticatedClient` properties for reuse across tests.
    * **Non-Obvious Error Handling:** Handles authentication failures gracefully, ensuring tests can still run with unauthenticated clients if necessary.
* **Critical Assumptions:**
    * Docker Desktop (or equivalent) is installed and running on the machine executing tests that require the `DatabaseFixture`.
    * The configurations specified in [`/api-server.Tests/Framework/TechnicalDesignDocument.md`](../TechnicalDesignDocument.md) are adhered to regarding mock registration and authentication setup within the factory.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Fixture Usage:** 
    * `CustomWebApplicationFactory` is used via `IClassFixture`.
    * `DatabaseFixture` uses `ICollectionFixture` with `[Collection("Database")]` to share a single database container.
    * **`ApiClientFixture`** uses `ICollectionFixture<ApiClientFixture>` under the `"Integration Tests"` collection to provide shared API clients to integration tests.
* **Mocking:** Relies on mock factories located in [`/api-server.Tests/Framework/Mocks/Factories/`](../Mocks/Factories/README.md).
* **Configuration:** The factory defines the primary configuration loading strategy for integration tests, as documented in the TDD.

## 5. How to Work With This Code

* **Setup:** Ensure Docker is running for tests involving `DatabaseFixture`.
* **Testing:** These classes *are* the test setup infrastructure. Unit tests for these fixtures are generally complex and may not provide high value compared to testing their behavior through actual integration tests. Basic unit tests for `DatabaseFixture` could verify Respawner options.
* **Common Pitfalls / Gotchas:**
    * Forgetting to call `await ResetDatabaseAsync()` in tests using `DatabaseFixture` can lead to state leakage between tests.
    * Ensure the correct fixture injection mechanism (`IClassFixture` vs. `ICollectionFixture`) is used.
    * Changes to the main application's `Program.cs` or service registrations might require updates in `CustomWebApplicationFactory.ConfigureWebHost`.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/api-server.Tests/Framework/Helpers/`](Framework/Helpers/README.md) - Uses `TestAuthHandler`.
    * [`/api-server.Tests/Framework/Mocks/Factories/`](../Mocks/Factories/README.md) - Uses mock factories for service registration.
    * [`/api-server.Tests/Framework/Configuration/`](../Configuration/README.md) - May indirectly use helpers if tests require specific config setups.
    * `api-server/Program.cs` - `CustomWebApplicationFactory` depends on the application's entry point.
* **External Library Dependencies:**
    * `Microsoft.AspNetCore.Mvc.Testing` - Core web application factory.
    * `Testcontainers.PostgreSql` - Manages PostgreSQL Docker container.
    * `Respawn` - Database cleanup.
    * `Xunit` - Test framework attributes (`IAsyncLifetime`, fixtures).
    * `Moq` - Used internally for mocking services.
* **Dependents (Impact of Changes):**
    * [`/api-server.Tests/Integration/`](../Integration/README.md) - All integration tests rely heavily on these fixtures. Changes here have wide impact.

## 7. Rationale & Key Historical Context

* `CustomWebApplicationFactory` provides a standard way to host the application in-memory for integration tests.
* `DatabaseFixture` using Testcontainers ensures isolated, ephemeral, and realistic database testing without requiring a manually managed external database. Respawn provides efficient cleanup.

## 8. Known Issues & TODOs

* The `CustomWebApplicationFactory` constructor's handling of `DatabaseFixture` initialization is slightly non-standard to ensure the `IsDatabaseAvailable` flag is set early. Review if this can be simplified while maintaining dependency-checking functionality.