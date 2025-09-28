# Module/Directory: /Unit/Controllers

**Last Updated:** 2025-01-20

> **Parent:** [../README.md](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Unit test suite for all ASP.NET Core controllers in the zarichney-api application, providing comprehensive validation of HTTP endpoint behavior, request/response handling, and controller-specific logic.
* **Key Responsibilities:**
  - Unit testing of all controller endpoints and HTTP action methods
  - Validation of request model binding and validation logic
  - Testing of controller dependency injection and service integration
  - Verification of response formatting and status code handling
  - Testing of authorization and authentication integration at controller level
* **Why it exists:** To ensure all API endpoints function correctly in isolation, providing confidence in HTTP interface contracts and controller-specific business logic before integration testing.

### Child Test Modules:
* **AiController:** [./AiController/README.md](./AiController/README.md) - AI service integration endpoints testing
* **ApiController:** [./ApiController/README.md](./ApiController/README.md) - Core API functionality testing
* **AuthController:** [./AuthController/README.md](./AuthController/README.md) - Authentication and authorization endpoints
* **CookbookController:** [./CookbookController/README.md](./CookbookController/README.md) - Cookbook order and recipe management endpoint testing
* **CookbookControllers:** [./CookbookControllers/README.md](./CookbookControllers/README.md) - Recipe management endpoint testing
* **PaymentController:** [./PaymentController/README.md](./PaymentController/README.md) - Payment processing endpoint testing
* **PublicController:** [./PublicController/README.md](./PublicController/README.md) - Public API endpoints testing

## 2. Architecture & Key Concepts

* **High-Level Design:** Controller unit tests follow the AAA (Arrange-Act-Assert) pattern with extensive use of mock services and dependency injection containers. Tests focus on controller-specific logic while mocking all service dependencies.
* **Core Logic Flow:**
  1. **Setup Phase:** Mock all controller dependencies (services, repositories, configuration)
  2. **Execution Phase:** Create controller instance with mocked dependencies and execute HTTP action
  3. **Verification Phase:** Assert response types, status codes, and controller-specific business logic
* **Key Data Structures:**
  - `ApiErrorResult` - Standardized error response format tested across controllers
  - `ApiResponse<T>` - Generic API response wrapper for consistent response structure
  - Controller-specific DTOs and request/response models
* **State Management:** Controllers are stateless by design; tests verify statelessness and proper dependency usage
* **Testing Patterns:**
  ```mermaid
  graph TD
      A[Test Setup] --> B{Mock Dependencies}
      B --> C[Create Controller Instance]
      C --> D[Execute Action Method]
      D --> E{Assert Response}
      E --> F[Verify Status Code]
      E --> G[Verify Response Data]
      E --> H[Verify Service Calls]
  ```

## 3. Interface Contract & Assumptions

* **Key Public Testing Interfaces:**
  * `ApiErrorResultTests` - Tests for standardized error response handling:
    * **Purpose:** Verify consistent error response formatting across all controllers
    * **Critical Preconditions:** Valid exception types and error codes must be provided
    * **Critical Postconditions:** Proper HTTP status codes and error message formatting guaranteed
    * **Non-Obvious Error Handling:** Tests edge cases like null exceptions and malformed error data
  * `ApiErrorResultBuilderTests` - Tests for error response construction:
    * **Purpose:** Validate error response builder pattern and fluent interface
    * **Critical Preconditions:** Builder pattern properly initialized with required error context
    * **Critical Postconditions:** Consistent error response structure regardless of error type
    * **Non-Obvious Error Handling:** Tests builder resilience with incomplete error information

* **Critical Assumptions:**
  * **Service Mocking:** All service dependencies are properly mocked to ensure true unit testing isolation
  * **HTTP Context:** ASP.NET Core HTTP context is properly simulated for request/response testing
  * **Dependency Injection:** Controller constructors properly receive mocked dependencies
  * **Authentication:** Authentication context is properly mocked for authorization testing
  * **Model Binding:** Request model binding behaves consistently with ASP.NET Core conventions

## 4. Local Conventions & Constraints

* **Configuration:** Tests use `TestConfiguration` with mock `IConfiguration` objects and test-specific `appsettings.Test.json` values
* **Directory Structure:**
  - Each controller has its own subdirectory with dedicated README and test files
  - Shared test utilities in parent `Unit/` directory
  - Response models tested in `Responses/` subdirectory
* **Technology Choices:**
  - xUnit for test framework with `[Theory]` and `[Fact]` attributes
  - Moq for service mocking and dependency isolation
  - FluentAssertions for readable test assertions
  - Microsoft.AspNetCore.Mvc.Testing for controller testing infrastructure
* **Performance Notes:** Controller tests should execute rapidly (< 100ms each) due to full mocking
* **Security Notes:** Tests verify authorization attributes and security headers without requiring actual authentication

## 5. How to Work With This Code

* **Setup:**
  - Ensure all controller dependencies are properly registered in test DI container
  - Configure test HTTP context and controller context for realistic test environment
  - Set up mock authentication and authorization contexts

* **Module-Specific Testing Strategy:**
  - **Unit Testing Focus:** Test controller logic in complete isolation using mocked dependencies
  - **Response Contract Testing:** Verify all endpoints return expected response types and status codes
  - **Error Handling Testing:** Test exception handling and error response formatting
  - **Authorization Testing:** Verify role-based and policy-based authorization without actual authentication

* **Key Test Scenarios:**
  - **Happy Path:** Valid requests with expected responses and status codes
  - **Validation Failures:** Invalid model binding and request validation scenarios
  - **Service Failures:** Service exceptions properly handled and converted to appropriate HTTP responses
  - **Authorization:** Access control verification for protected endpoints
  - **Edge Cases:** Null parameters, empty collections, and boundary conditions

* **Test Data Considerations:**
  - Use `TestDataBuilder` pattern for creating consistent test objects
  - Mock service responses with realistic but predictable data
  - Test with various user roles and authentication states
  - Include boundary value testing for numeric and string parameters

* **Testing Commands:**
  ```bash
  # Run all controller tests
  dotnet test --filter "Category=Unit&FullyQualifiedName~Controllers"

  # Run specific controller tests
  dotnet test --filter "FullyQualifiedName~ApiControllerTests"

  # Run with coverage
  dotnet test --collect:"XPlat Code Coverage" --filter "Category=Unit&FullyQualifiedName~Controllers"
  ```

## 6. Dependencies

* **Internal Code Dependencies:**
  * [`../Services/`](../Services/README.md) - Service layer components being tested through dependency injection
  * [`../Framework/`](../Framework/README.md) - Test framework utilities, builders, and mock factories
  * [`../../Code/Zarichney.Server/Controllers/`](../../../Zarichney.Server/Controllers/README.md) - Actual controller implementations under test

* **External Library Dependencies:**
  * `Microsoft.AspNetCore.Mvc.Testing` - Controller testing infrastructure
  * `Moq` - Service mocking and dependency isolation
  * `xUnit` - Test framework and test runner
  * `FluentAssertions` - Readable test assertions and error messages
  * `Microsoft.AspNetCore.Authorization.Policy` - Authorization testing utilities

* **Dependents (Impact of Changes):**
  * [`../Integration/`](../Integration/README.md) - Integration tests that build upon controller contracts
  * [`../../Framework/TestData/`](../Framework/TestData/README.md) - Test data builders used by controller tests

## 7. Rationale & Key Historical Context

Controller unit tests were implemented with complete service mocking to ensure true isolation and fast test execution. The decision to use extensive mocking rather than integration testing at this level allows for precise testing of controller-specific logic while maintaining predictable test execution times. The `ApiErrorResult` standardization emerged from the need for consistent error handling across all API endpoints, ensuring clients receive uniform error responses regardless of the underlying failure cause.

## 8. Known Issues & TODOs

* Consider implementing shared controller test base class to reduce test setup duplication across controller test modules
* Add performance benchmarking for controller action execution to detect performance regressions
* Enhance authorization testing to cover more complex policy-based scenarios and custom authorization handlers
* Implement contract testing to ensure controller responses remain compatible with API documentation and client expectations
