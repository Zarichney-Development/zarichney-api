# Module/Directory: /api-server.Tests/Mocks/Factories

**Last Updated:** 2025-04-18

> **Parent:** [`api-server.Tests/Mocks/`](../Mocks/README.md) (Assuming a parent Mocks README exists or will be created)
> If `/api-server.Tests/Mocks/README.md` doesn't exist, the parent link should be:
> **Parent:** [`api-server.Tests`](../../README.md)

## 1. Purpose & Responsibility

* **What it is:** A collection of factory classes responsible for creating configured `Moq.Mock<T>` instances for external service interfaces (e.g., `IStripeService`, `ILlmService`, `IGitHubService`).
* **Key Responsibilities:**
    * Providing static `CreateMock()` methods to easily generate mock objects for specific external services.
    * Setting up *default* behaviors on the mocks (e.g., returning successful responses, empty lists) to simplify test setup for common scenarios.
    * Ensuring consistency in how external services are mocked across different tests.
* **Why it exists:** To centralize the creation and default configuration of mock objects, reducing boilerplate code in tests and promoting standard mock setups.

## 2. Architecture & Key Concepts

* **Base Class:** Most factories inherit from `BaseMockFactory<T>`, which provides a common structure.
* **Individual Factories:** Each factory (e.g., `MockStripeServiceFactory`, `MockOpenAIServiceFactory`) targets a specific external service interface.
* **`CreateMock()` Method:** The primary static method used to get a mock instance with default setups applied.
* **`SetupDefaultMock()` Method:** An abstract/virtual method implemented by each specific factory to define the default `mock.Setup(...)` calls for that service.
* **Usage:**
    * The `CustomWebApplicationFactory` uses these factories to register default mocks in the test application's service container.
    * Individual tests can retrieve these pre-configured mocks from the factory's service provider (`_factory.Services.GetRequiredService<Mock<IExternalService>>()`) and further customize their behavior (`Setup()`, `Verify()`) as needed for specific test scenarios.

## 3. Interface Contract & Assumptions

* **`MockXServiceFactory.CreateMock()`**:
    * **Purpose:** Returns a `Mock<IXService>` instance.
    * **Critical Postconditions:** The returned mock object has default behaviors pre-configured via `SetupDefaultMock`. These defaults usually represent successful, non-exceptional paths.
* **Critical Assumptions:**
    * Assumes the interfaces being mocked (`IStripeService`, `ILlmService`, etc.) accurately reflect the actual service contracts used by the main application.
    * Assumes the default mock behaviors are suitable for the *majority* of test cases; specific tests needing different behavior must override the setup.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Naming:** Factories are named `Mock[ServiceName]ServiceFactory`.
* **Structure:** Follows the `BaseMockFactory<T>` pattern.
* **Default Behavior:** Default setups should generally mock successful operations and avoid throwing exceptions unless specifically testing failure paths.

## 5. How to Work With This Code

* **Adding a New Mock Factory:**
    1.  Create a new class `MockMyNewServiceFactory` inheriting `BaseMockFactory<IMyNewService>`.
    2.  Implement the `SetupDefaultMock(Mock<IMyNewService> mock)` method to define default behaviors.
    3.  Add a static `CreateMock()` method.
    4.  Register the factory's output in `CustomWebApplicationFactory`.
* **Testing:** These factories are typically not unit tested directly. Their correctness is implicitly tested by the integration tests that rely on the mocks they produce.
* **Common Pitfalls / Gotchas:** Forgetting to register a new mock factory in `CustomWebApplicationFactory` will mean the real service (or no service) is used in integration tests, likely causing failures. Ensure default mock setups don't conflict with specific setups needed in individual tests.

## 6. Dependencies

* **Internal Code Dependencies:**
    * Interfaces defined in `api-server` (e.g., `api-server/Services/Payment/IStripeService.cs`, `api-server/Services/AI/ILlmService.cs`).
* **External Library Dependencies:**
    * `Moq` - The core mocking library.
* **Dependents (Impact of Changes):**
    * [`/api-server.Tests/Fixtures/CustomWebApplicationFactory.cs`](../Fixtures/README.md) - Consumes these factories for registration.
    * [`/api-server.Tests/Integration/`](../Integration/README.md) - Integration tests rely on the mocks provided by these factories. Changes to default behavior can affect test outcomes.

## 7. Rationale & Key Historical Context

* Centralizes mock creation logic, promoting DRY (Don't Repeat Yourself) and consistency in testing. Simplifies the setup required in `CustomWebApplicationFactory` and individual tests.

## 8. Known Issues & TODOs

* None currently identified.