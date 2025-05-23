# README: /Framework/Fixtures Directory

**Version:** 1.1
**Last Updated:** 2025-05-22
**Parent:** `../README.md`

## 1. Purpose & Responsibility

This directory contains shared xUnit **fixtures** that are fundamental to the integration testing strategy of the `api-server.Tests` project. Fixtures are used by xUnit to manage the lifecycle of expensive or shared resources, ensuring that tests have a consistent and properly configured environment.

The primary responsibilities of the fixtures in this directory are:
* **Test Server Management (`CustomWebApplicationFactory.cs`):** To host the `api-server` application in-memory, manage its configuration for testing, and provide a mechanism for overriding services (e.g., for injecting mocks or test-specific authentication handlers).
* **Database Management (`DatabaseFixture.cs`):** To manage the lifecycle of a PostgreSQL database instance using Testcontainers, including starting/stopping the container, applying EF Core migrations, and providing database cleanup capabilities via Respawn.
* **API Client Provisioning (`ApiClientFixture.cs`):** To create and provide pre-configured instances of the auto-generated Refit client (`IZarichneyAPI.cs`), ensuring tests use a consistent client for API interactions.

These fixtures are crucial for creating efficient, reliable, and maintainable integration tests by abstracting away complex setup and teardown logic.

## 2. Architecture & Key Concepts

* **xUnit Fixture Model:** These fixtures primarily leverage xUnit's `ICollectionFixture<>` interface. This allows a single instance of each fixture to be created and shared across all test classes within a specific test collection (the `"Integration"` collection defined in `../../Integration/IntegrationCollection.cs`). This significantly improves test suite performance by avoiding repeated setup of expensive resources like database containers or the web application host.
* **Asynchronous Initialization/Disposal:** Fixtures managing external resources or requiring asynchronous setup (like `DatabaseFixture` and `CustomWebApplicationFactory` in practice, though `WebApplicationFactory` handles its async aspects internally) often implement `IAsyncLifetime` for `InitializeAsync` and `DisposeAsync` logic.
* **Key Fixtures:**
    * **`CustomWebApplicationFactory.cs`:**
        * Inherits from `Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory<Program>`.
        * Overrides `ConfigureWebHost` to customize the test server's services (`ConfigureTestServices`) and configuration (`ConfigureAppConfiguration`). This is where dependencies are mocked/overridden (e.g., `TestAuthHandler` registration, mock factories from `../Mocks/Factories/`, and future WireMock.Net setup).
        * The detailed test configuration strategy is defined in Section 9 of the `../../TechnicalDesignDocument.md`.
    * **`DatabaseFixture.cs`:**
        * Manages a PostgreSQL `Testcontainer`.
        * Applies EF Core migrations (`_dbContext.Database.MigrateAsync()`) upon initialization to ensure the schema is current.
        * Provides a connection string to the test database.
        * Initializes `Respawner` for database cleaning and exposes `ResetDatabaseAsync()` to be called by tests.
    * **`ApiClientFixture.cs`:**
        * Depends on `CustomWebApplicationFactory` to get an `HttpClient`.
        * Creates and configures instances of the Refit client `IZarichneyAPI`.
* **Consumption:** These fixtures are typically injected into the constructor of base test classes (`IntegrationTestBase`, `DatabaseIntegrationTestBase`), which then expose their functionalities or the resources they manage (e.g., API client, DbContext factory method) to the actual test methods.

## 3. Interface Contract & Assumptions

* **Interface for Test Classes:**
    * Test classes belonging to the `"Integration"` collection receive these fixtures via constructor injection (often handled by their base class).
    * They interact with the fixtures' exposed properties or methods (e.g., `ApiClientFixture.GetApiClient()`, `DatabaseFixture.GetContext()`, `DatabaseFixture.ResetDatabaseAsync()`, `CustomWebApplicationFactory.CreateClient()`).
* **Assumptions:**
    * **Docker Environment:** `DatabaseFixture` fundamentally assumes that a Docker environment is available and correctly configured on the machine running the tests. Tests requiring it can be decorated with `[DockerAvailableFact]`.
    * **`api-server` Configurability:** `CustomWebApplicationFactory` assumes that the `api-server`'s `Program.cs` is structured to allow for in-memory hosting and that its service registration and configuration pipeline can be customized for testing.
    * **Migration Application:** `DatabaseFixture` assumes that EF Core migrations are present and can be applied successfully to the test database.
    * **Client Generation:** `ApiClientFixture` assumes the `IZarichneyAPI.cs` client has been correctly generated and is up-to-date.

## 4. Local Conventions & Constraints

* **Naming:** Fixture classes should clearly indicate their purpose and end with `Fixture` (e.g., `DatabaseFixture`).
* **Scope:** Fixtures in this directory are intended for broad, cross-cutting concerns shared by many integration tests. More specialized, single-test-class fixtures (using `IClassFixture<>`) are generally discouraged in favor of this shared model for performance, unless a resource is truly unique to one test class and very expensive.
* **Resource Management:** Fixtures managing unmanaged resources or requiring asynchronous setup/teardown **must** implement `IAsyncLifetime` correctly.
* **Idempotency:** Fixture setup should be idempotent where possible, though `ICollectionFixture` instances are created only once per collection run. Methods like `DatabaseFixture.ResetDatabaseAsync()` are designed to be called multiple times.
* **Documentation:** New fixtures must be documented in this README, and their usage patterns explained.

## 5. How to Work With This Code

* **Primary Usage (for Test Writers):**
    * Developers writing integration tests typically do not need to instantiate or directly manage these fixtures in their test methods.
    * Instead, they inherit from `IntegrationTestBase` or `DatabaseIntegrationTestBase`. These base classes receive the shared fixtures through constructor injection (due to the `[Collection("Integration")]` attribute) and expose necessary functionalities (e.g., `ApiClient` property, `ResetDatabaseAsync()` method).
    * Example: To get a database context in a test: `await using var context = DbFixture.GetContext();` (where `DbFixture` is a property from `DatabaseIntegrationTestBase`).
    * Example: To reset the database: `await DbFixture.ResetDatabaseAsync();`.
* **Extending or Modifying Fixtures:**
    * Modifications to these core fixtures should be done cautiously, as they impact all integration tests.
    * When adding a new shared resource (e.g., a Testcontainer for Redis, a shared WireMock.Net server instance):
        1.  Create a new class in this directory implementing `IAsyncLifetime` if needed.
        2.  Add it to the `IntegrationCollection` definition (`../../Integration/IntegrationCollection.cs`) as an `ICollectionFixture<>`.
        3.  Update `IntegrationTestBase` to accept and expose the new fixture if it's broadly needed.
        4.  Document the new fixture in this README.

## 6. Dependencies

### Internal Dependencies

* **`api-server` Project:** Required by `CustomWebApplicationFactory` for hosting and by `DatabaseFixture` for `UserDbContext` and migrations.
* **`../Client/IZarichneyAPI.cs`:** Used by `ApiClientFixture`.
* **`../Helpers/`**: Fixtures may use helper classes (e.g., `TestAuthHandler` is configured by `CustomWebApplicationFactory`).
* **`../Mocks/Factories/`**: Used by `CustomWebApplicationFactory` to register mock services.
* **`../../Integration/IntegrationCollection.cs`:** Defines how these fixtures are shared.

### Key External Libraries

* **`Xunit.net` (`xunit.core`, `xunit.abstractions`):** For `ICollectionFixture<>`, `IAsyncLifetime`.
* **`Microsoft.AspNetCore.Mvc.Testing`:** Base for `CustomWebApplicationFactory`.
* **`Testcontainers.PostgreSql`:** Used by `DatabaseFixture`.
* **`Respawn.Postgres`:** Used by `DatabaseFixture` for database cleanup.
* **`Refit`:** Consumed indirectly via `ApiClientFixture`.
* **`Microsoft.EntityFrameworkCore` (and related packages):** Used by `DatabaseFixture` for migrations and `DbContext` operations.

## 7. Rationale & Key Historical Context

The primary rationale for the design of these fixtures, especially their use as `ICollectionFixture`s, is **performance and consistency**. Starting a web application host and a database container are expensive operations. Sharing these resources across the entire integration test suite drastically reduces overall execution time.

* `CustomWebApplicationFactory` centralizes test server configuration and service overriding logic.
* `DatabaseFixture` was created to provide a true, ephemeral PostgreSQL database for testing data persistence and complex queries, moving away from less reliable in-memory database providers. The inclusion of programmatic migration application and Respawn ensures each test can run against a known, clean schema.
* `ApiClientFixture` ensures all tests use a consistently configured HTTP client and Refit interface for interacting with the SUT.

This centralized fixture model is a cornerstone of the integration testing strategy outlined in the `../../TechnicalDesignDocument.md`.

## 8. Known Issues & TODOs

* **Testcontainer Startup Time:** While shared, the initial startup of the PostgreSQL Testcontainer can still take several seconds. This is a known trade-off for database fidelity. (See TDD FRMK-003 for planned Testcontainer enhancements).
* **WireMock.Net Fixture (Planned):** A new fixture will be introduced here to manage the lifecycle of `WireMock.Net` servers for external HTTP service virtualization (as per TDD FRMK-004). This fixture will likely be integrated into the `IntegrationCollection` as well.
* **Configuration Complexity:** The `CustomWebApplicationFactory` has complex configuration logic. This needs to be carefully maintained and well-understood.
* Refer to the "Framework Augmentation Roadmap (TODOs)" in `../../TechnicalDesignDocument.md` for broader framework enhancements that may impact components in this directory.

---
