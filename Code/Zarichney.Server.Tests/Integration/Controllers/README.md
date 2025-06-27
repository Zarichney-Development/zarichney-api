# Module/Directory: /Zarichney.Server.Tests/Integration/Controllers

**Last Updated:** 2025-04-21

> **Parent:** [`Integration`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Contains integration tests specifically targeting the API controllers defined in the main `Zarichney.Server` project.
* **Key Responsibilities:**
    * Verifying correct request routing to controller actions.
    * Testing model binding and validation for request DTOs.
    * Validating response status codes and DTOs against the expected API contract.
    * Ensuring authentication and authorization rules are correctly applied at the controller level.
    * Testing controller interaction with underlying services (using mocks where appropriate for external dependencies, but allowing interaction with real services/repositories for full integration).
    * Validating error handling and exception-to-response mapping.
* **Why it exists:** To ensure the API layer functions correctly, handles requests as expected, enforces security rules, and integrates properly with the rest of the application stack from the perspective of an API consumer.
* **Submodule Structure:** Tests are organized into subdirectories mirroring the controller structure in the main `Zarichney.Server` project (e.g., `./AuthController/`, `./CookbookController/`).

## 2. Architecture & Key Concepts

* **Test Execution:** Tests run within the context provided by the shared `CustomWebApplicationFactory`, hosted in-memory.
* **Fixtures:** Relies on the shared fixtures (`CustomWebApplicationFactory`, `DatabaseFixture`, `ApiClientFixture`) provided by the single `[Collection("Integration")]` definition via `ICollectionFixture<>`.
* **Base Classes:** Tests typically inherit from `IntegrationTestBase` or `DatabaseIntegrationTestBase` (if database interaction is required via the controller's actions). These base classes provide access to the shared fixtures.
* **API Interaction:** Tests **must** interact with the controllers *only* through API calls made using the shared, generated Refit clients provided by `ApiClientFixture` (accessed via specific interface properties like `_apiClientFixture.UnauthenticatedPublicApi`). Direct instantiation or invocation of controller classes or their service dependencies is generally forbidden in these integration tests.
* **Database State:** For tests involving controllers that modify or depend on database state, `DatabaseIntegrationTestBase` should be used, and `ResetDatabaseAsync()` must be called to ensure test isolation.

## 3. Interface Contract & Assumptions

* **Tests as API Clients:** These tests act as clients to the API endpoints exposed by the controllers.
* **Validation Scope:** They validate the external contract of the API controllers – routes, HTTP methods, request/response models, status codes, and authentication/authorization behavior.
* **Critical Assumptions:**
    * Assumes the shared `CustomWebApplicationFactory` correctly sets up the application environment, including routing, middleware, service registration (and mocks for external services).
    * Assumes the shared `DatabaseFixture` provides a correctly migrated database when required.
    * Assumes the shared `ApiClientFixture` provides correctly configured clients.
    * Assumes the generated Refit clients accurately reflect the API contract being tested.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Directory Structure:** Maintain subdirectories named after the corresponding controller (e.g., `AuthController/` contains `LoginEndpointTests.cs`).
* **Test Structure:** Follow AAA pattern. Use descriptive method names (`[MethodName]_[Scenario]_[ExpectedOutcome]`).
* **Assertions:** Use `FluentAssertions`. Focus on asserting API response details (status code, headers, body content using `Should().BeEquivalentTo()` for DTOs) and verifying expected side effects (e.g., mock interactions, database state changes *if necessary*, though verifying via subsequent API calls is preferred). Include `.Because("...")`.
* **Collection:** All tests **MUST** belong to `[Collection("Integration")]`.
* **Base Class:** Inherit `IntegrationTestBase` or `DatabaseIntegrationTestBase`.
* **API Client Usage:** **MUST** use `ApiClient` or `AuthenticatedApiClient` from the base class.
* **Database Reset:** Call `await ResetDatabaseAsync()` at the start of tests involving data mutation or specific state requirements.
* **Traits:** Apply `Category=Integration`, relevant `Feature`, `Dependency`, and `Mutability` traits. Use `[DependencyFact]` for tests relying on configurations checked by `IntegrationTestBase`.

## 5. How to Work With This Code

* **Adding Tests for a New Controller:**
    1. Create a new subdirectory named after the controller (e.g., `MyNewController/`).
    2. Create test class(es) within the subdirectory (e.g., `MyNewControllerTests.cs`).
    3. Add `[Collection("Integration")]` to the class.
    4. Inherit from `IntegrationTestBase` or `DatabaseIntegrationTestBase`.
    5. Add necessary `[Trait]` attributes.
    6. Write test methods using `[DependencyFact]` or `[Fact]`, interacting via `ApiClient`/`AuthenticatedApiClient`.
* **Adding Tests for an Existing Controller:** Add new test methods to the appropriate existing test class(es) within the controller's subdirectory.
* **Running Tests:** Use `dotnet test` with filters targeting `Category=Integration` and optionally the specific `Feature` trait related to the controller.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/Zarichney.Server.Tests/Framework/Fixtures/`](../../Framework/Fixtures/README.md) - Relies on all shared fixtures.
    * [`/Zarichney.Server.Tests/Framework/Client/`](../../Framework/Client/README.md) - Uses the generated Refit client.
    * [`/Zarichney.Server.Tests/Framework/Helpers/`](../../Framework/Helpers/README.md) - Uses base classes and potentially other helpers.
    * [`/Zarichney.Server.Tests/TestData/`](../../TestData/README.md) - May use test data builders.
    * `Zarichney.Server/Controllers/` - Tests target these components indirectly via API calls.
* **External Library Dependencies:** `Xunit`, `FluentAssertions`.
* **Dependents (Impact of Changes):** Changes to controller API contracts will require updates to these tests and potentially regeneration of the Refit client.

## 7. Rationale & Key Historical Context

* Testing controllers via API integration tests provides high confidence that the primary interface of the application behaves correctly, including routing, model binding, authorization, and interaction with underlying services. This is crucial for validating the user-facing contract of the API.

## 8. Known Issues & TODOs

* Ensure comprehensive test coverage for all actions, scenarios (success/failure), and authorization paths within each controller.
* Specific TODOs related to individual controllers or endpoints should be tracked within the relevant test files or controller-specific READMEs if created.

