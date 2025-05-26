# README: /Framework Directory

**Version:** 1.1
**Last Updated:** 2025-05-22
**Parent:** `../README.md`

## 1. Purpose & Responsibility

This directory, `Framework/`, houses the core, reusable infrastructure, components, and utilities that support and underpin all automated testing (both unit and integration) within the `api-server.Tests` project.

Its primary responsibilities are to:
* Provide a consistent and reliable testing environment.
* Offer shared services and fixtures to reduce boilerplate and improve test maintainability (e.g., `CustomWebApplicationFactory`, `DatabaseFixture`).
* Encapsulate common testing patterns and logic (e.g., authentication simulation via `TestAuthHandler`, conditional test execution via `DependencyFactAttribute`).
* Host the auto-generated API clients used for integration testing (multiple granular interfaces).

The components within this directory are designed to be leveraged by test cases in the `/Unit` and `/Integration` directories, ensuring adherence to the project's testing standards as defined in `../../Docs/Standards/TestingStandards.md` and the detailed guides for unit and integration testing.

### Child Modules / Key Subdirectories:

* **`./Attributes/README.md`**: Custom xUnit attributes for specialized test execution control (e.g., `[DependencyFact]`, `[DockerAvailableFact]`).
* **`./Client/README.md`**: Contains the auto-generated Refit clients (multiple granular interfaces) for type-safe API interactions during integration tests.
* **`./Fixtures/README.md`**: Core xUnit fixtures managing the lifecycle of expensive resources like the test server (`CustomWebApplicationFactory.cs`) and database (`DatabaseFixture.cs`).
* **`./Helpers/README.md`**: Utility classes and extension methods that provide common helper functions for tests (e.g., `AuthTestHelper.cs`, `TestConfigurationHelper.cs`).
* **`./Mocks/README.md`**: Infrastructure for mocking external dependencies, including mock factories (e.g., `MockStripeServiceFactory.cs`) and configurations for service virtualization tools like WireMock.Net (planned).
* **`./TestData/AutoFixtureCustomizations/`** (Planned as per TDD FRMK-002): Will house advanced, reusable AutoFixture customizations (e.g., `ISpecimenBuilder`, `ICustomization`) for complex domain models.
* **`./TestControllers/`**: Contains minimal ASP.NET Core controllers used specifically by the testing framework, for example, to test feature availability middleware or other pipeline components in isolation. (e.g., `FeatureTestController.cs`).

## 2. Architecture & Key Concepts

The framework components are designed to be composable and to integrate seamlessly with xUnit's testing lifecycle.
* **Shared Fixtures:** Core fixtures like `CustomWebApplicationFactory`, `DatabaseFixture`, and `ApiClientFixture` are typically shared across multiple test classes within the `"Integration"` collection using `ICollectionFixture<>` for performance.
* **Base Test Classes:** `IntegrationTestBase` and `DatabaseIntegrationTestBase` provide convenient access to these shared fixtures and common setup logic for integration tests.
* **Dependency Abstraction:** External service interactions are abstracted via interfaces in the main `api-server` project, allowing the `CustomWebApplicationFactory` to inject mock implementations (often from `./Mocks/Factories/`) or configure clients to point to virtualized services (WireMock.Net).
* **Configuration Management:** Test configuration is handled centrally by `CustomWebApplicationFactory`, mimicking production loading patterns while allowing test-specific overrides (see `../TechnicalDesignDocument.md` Section 9). Helpers like `TestConfigurationHelper` can assist if isolated configuration instances are needed.

## 3. Interface Contract & Assumptions

* **For Test Writers:**
    * Fixtures (e.g., `ApiClientFixture`, `DatabaseFixture`) are primarily consumed via constructor injection into test classes that are part of the `"Integration"` collection or through properties exposed by base test classes (`IntegrationTestBase`, `DatabaseIntegrationTestBase`).
    * Helper classes in `./Helpers/` are typically static or designed to be instantiated directly within tests or fixture setup.
    * Custom attributes from `./Attributes/` are applied directly to test methods or classes as per xUnit conventions.
* **Assumptions Made by Framework Components:**
    * `DatabaseFixture` assumes Docker is available and operational for managing Testcontainers. Tests requiring Docker can use `[DockerAvailableFact]`.
    * `DependencyFactAttribute` relies on configuration status provided by the `IStatusService` within the SUT and specific configuration flags to determine if dependencies are met.
    * The Refit client generation script (`../../Scripts/generate-api-client.ps1`) assumes the `api-server` project can be built and its Swagger specification is accessible.

## 4. Local Conventions & Constraints

* **Naming:**
    * Fixture classes should end with `Fixture` (e.g., `DatabaseFixture.cs`).
    * Helper classes should end with `Helper` or be named descriptively for their function (e.g., `AuthTestHelper.cs`, `GetRandom.cs`).
    * Custom xUnit attributes should end with `Attribute` (e.g., `DependencyFactAttribute.cs`).
* **Extensibility:**
    * New shared fixtures should implement `IAsyncLifetime` if they manage unmanaged resources or require asynchronous setup/teardown.
    * New helper methods should be generic and reusable across different test scenarios.
    * Contributions to the framework should be well-documented, including updates to relevant README files.
* **Statelessness:** Framework components, especially shared fixtures, should be designed to be stateless or manage their state in a way that ensures test isolation.

## 5. How to Work With This Code

* **Using Framework Components:**
    * **Integration Tests:** Inherit from `IntegrationTestBase` or `DatabaseIntegrationTestBase` and utilize the exposed properties (`Factory`, `ApiClient`, `DbFixture`, `AuthHelper`, `Output`).
    * **Unit Tests:** May directly instantiate helpers like `GetRandom` or use AutoFixture with customizations defined in `./TestData/AutoFixtureCustomizations/` (once available).
* **Extending the Framework:**
    * **New Mock Factory:** Add a new class to `./Mocks/Factories/` (e.g., `MockNewExternalServiceFactory.cs`) and register it in `CustomWebApplicationFactory.ConfigureTestServices`.
    * **New Custom Attribute:** Add to `./Attributes/`, potentially with a discoverer and executor if complex logic is needed.
    * **New Helper:** Add to `./Helpers/`, ensuring it's well-tested.
* **Testing the Framework Itself:**
    * Unit tests for framework components (e.g., attributes, helpers with logic) are located in `../Unit/Framework/` (e.g., `../Unit/Framework/Attributes/DependencyFactAttributeTests.cs`).
    * Integration tests for fixtures or complex framework interactions can be found within the standard integration test structure if they test observable behavior through an API, or specialized tests might exist if direct testing of fixture behavior is necessary.

## 6. Dependencies

### Internal Dependencies (Consumed by or Consumes)

* **`api-server` project:** `CustomWebApplicationFactory` directly depends on the `api-server`'s `Program.cs` (or `Startup.cs`) for hosting. The Refit client is generated from this project's API.
* **`api-server.Tests/Unit` & `api-server.Tests/Integration`:** These directories are the primary consumers of the components within `/Framework`.

### Key External Libraries Used to Build Framework Components

* **xUnit.net:** For custom attributes (`FactAttribute`, `TheoryAttribute` derivatives) and fixture interfaces (`IAsyncLifetime`, `ICollectionFixture<>`).
* **Testcontainers.PostgreSql:** Used by `DatabaseFixture`.
* **Microsoft.AspNetCore.Mvc.Testing:** Base for `CustomWebApplicationFactory`.
* **Refit:** Core library used for the generated API client.
* **Moq:** Used within mock factories in `./Mocks/Factories/`.
* **AutoFixture:** May be used within helpers or planned for `./TestData/AutoFixtureCustomizations/`.
* **WireMock.Net** (Planned): Will be a key dependency for HTTP service virtualization components.

## 7. Rationale & Key Historical Context

The architecture of this framework, particularly the use of shared `ICollectionFixture` for `CustomWebApplicationFactory` and `DatabaseFixture`, was chosen to optimize the performance of the integration test suite by minimizing the setup and teardown overhead of these expensive resources.

The `DependencyFactAttribute` was introduced to handle conditional test execution gracefully, ensuring that tests are skipped rather than failed when their underlying dependencies (e.g., configured external services, Docker) are not available, which is common in diverse development and CI environments.

## 8. Known Issues & TODOs

* **Framework Augmentation:** This framework is actively being enhanced based on the roadmap in `../TechnicalDesignDocument.md` (Section 16). Key upcoming additions/enhancements include:
    * Full integration of WireMock.Net for external HTTP service virtualization (TDD FRMK-004).
    * Development of advanced AutoFixture customizations (TDD FRMK-002).
    * Further enhancements to Testcontainers usage (TDD FRMK-003).
* Refer to the issues tracker in the repository for any specific bugs or minor improvements planned for framework components.

---
