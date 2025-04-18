# Module/Directory: /api-server.Tests/Framework/Mocks

**Last Updated:** 2025-04-18

> **Parent:** [`api-server.Tests`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Contains mocks, mock factories, and mock data for simulating external dependencies in tests.
* **Key Responsibilities:**
    * Providing standardized mock implementations of external services
    * Centralizing mock creation logic through factory classes
    * Ensuring consistent behavior of mocked dependencies across tests
* **Child Components:**
    * [`Factories/`](./Factories/README.md): Factory classes for creating configured mock objects

## 2. Architecture & Key Concepts

* **Mock Factories:** Classes that create pre-configured mock objects for external service interfaces
* **Base Factory Pattern:** Most factories inherit from `BaseMockFactory<T>`, providing a consistent approach
* **Default Setups:** Factories apply default behaviors to mock objects, which can be overridden in specific tests
* **Usage in Tests:** Mocks are registered in `CustomWebApplicationFactory` and retrieved in tests via dependency injection

## 3. Interface Contract & Assumptions

* **Factory Methods:** Each factory provides a static `CreateMock()` method returning a configured `Mock<T>` instance
* **Default Behavior:** Unless specified otherwise, mocks simulate successful operations by default
* **Critical Assumptions:**
    * Assumes the interfaces being mocked accurately reflect the real service contracts
    * Assumes tests will override default mock behavior when testing specific scenarios

## 4. Local Conventions & Constraints

* **Factory Naming:** Mock factories follow the pattern `Mock[ServiceName]Factory`
* **Implementation:** Factories implement `SetupDefaultMock()` to configure standard mock behavior
* **Registration:** All factory-created mocks must be registered in `CustomWebApplicationFactory`

## 5. How to Work With This Code

* **Using Existing Mocks:**
  ```csharp
  // In integration tests
  var mockService = _factory.Services.GetRequiredService<Mock<IExternalService>>();
  
  // Configure mock for specific test scenario
  mockService.Setup(s => s.GetDataAsync(It.IsAny<string>()))
      .ReturnsAsync(testData);
      
  // Verify mock interactions after the test
  mockService.Verify(s => s.GetDataAsync(It.IsAny<string>()), Times.Once);
  ```

* **Creating a New Mock Factory:**
  1. Create a class inheriting from `BaseMockFactory<T>`
  2. Implement the `SetupDefaultMock()` method
  3. Add a static `CreateMock()` method
  4. Register in `CustomWebApplicationFactory.RegisterMockExternalServices()`

* **Common Pitfalls:**
  * Forgetting to register new mocks in the factory
  * Conflicting mock setups between default configuration and test-specific setup
  * Overly strict mock verifications making tests brittle

## 6. Dependencies

* **Internal Code Dependencies:**
  * [`api-server.Tests/Framework/Fixtures/CustomWebApplicationFactory.cs`](Framework/Fixtures/README.md): Consumes these factories
  * External service interfaces from the main `api-server` project
* **External Library Dependencies:**
  * `Moq`: The core mocking library
* **Dependents:**
  * All integration tests that rely on mocked external services

## 7. Rationale & Key Historical Context

* Centralized mock factories promote consistency and reduce duplication in test code
* The approach ensures external dependencies are properly isolated in tests

## 8. Known Issues & TODOs

* Consider adding specialized mock data models for common external service responses
* Evaluate adding more sophisticated default behaviors for complex services