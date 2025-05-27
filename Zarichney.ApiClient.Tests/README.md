# Module/Directory: Zarichney.ApiClient.Tests

**Last Updated:** 2025-05-26

**(Parent Directory's README)**
> **Parent:** [`zarichney-api (root)`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Test project for the `Zarichney.ApiClient` library, ensuring the API client interfaces and configuration work correctly.
* **Key Responsibilities:**
  - Unit testing of dependency injection configuration and client setup
  - Integration testing of API client communication with live servers
  - Validation of client interface behavior and error handling
  - Testing API client configuration options and customization
* **Why it exists:** To provide confidence that the `Zarichney.ApiClient` library functions correctly as a standalone component and can be safely consumed by other projects.
* **Submodules:**
    * **Submodule:** [`Unit/`](./Unit/README.md) - Unit tests for client configuration and setup
    * **Submodule:** [`Integration/`](./Integration/README.md) - Integration tests for client-server communication

## 2. Architecture & Key Concepts

* **High-Level Design:** XUnit test project with standard Unit/Integration test separation. Tests focus on verifying the `Zarichney.ApiClient` library functionality rather than server endpoints.
* **Core Logic Flow:** 
  1. Unit tests verify dependency injection setup and client configuration
  2. Integration tests validate client communication with live API servers
  3. Tests use FluentAssertions for readable assertions and Moq for mocking where needed
* **Key Data Structures:** 
  - Test fixtures for API client setup
  - Mock HTTP responses for unit testing
  - Test data builders for request/response objects
* **State Management:** Stateless tests with isolated service provider instances per test
* **Diagram(s):**
    ```mermaid
    graph TD
        A[Zarichney.ApiClient.Tests] --> B[Unit Tests]
        A --> C[Integration Tests]
        B --> D[DI Configuration Tests]
        B --> E[Client Setup Tests]
        C --> F[API Communication Tests]
        C --> G[Error Handling Tests]
        H[Zarichney.ApiClient] --> A
        I[Test Server/Live API] --> C
    ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
  * This is a test project - no public interfaces for external consumption
* **Critical Assumptions:**
    * **External Systems/Config:** Integration tests may require a running instance of the API server
    * **Data Integrity:** Tests assume `Zarichney.ApiClient` library is properly built and available
    * **Implicit Constraints:** Some integration tests are marked as skipped until proper test infrastructure (Zarichney.TestingFramework) is available

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:** No special configuration required - tests use default client settings
* **Directory Structure:** 
  - `Unit/` - Unit tests for internal client logic and configuration
  - `Integration/` - Integration tests requiring live server communication
* **Technology Choices:** 
  - XUnit for test framework
  - FluentAssertions for readable test assertions
  - Moq for mocking HTTP dependencies
  - Refit for HTTP client testing
* **Performance/Resource Notes:** Integration tests may be slower due to HTTP communication
* **Security Notes:** Tests use localhost connections only

## 5. How to Work With This Code

* **Setup:** 
  - Ensure `Zarichney.ApiClient` project is built
  - For integration tests: ensure API server is running locally (when not skipped)
* **Testing:**
    * **Location:** This entire project is for testing
    * **How to Run:** `dotnet test Zarichney.ApiClient.Tests`
    * **Testing Strategy:** 
      - Unit tests focus on dependency injection and client configuration
      - Integration tests validate communication with live servers (currently limited)
      - Mock HTTP responses for isolated unit testing
      - TODO: Expand integration tests once `Zarichney.TestingFramework` is available (GH-14)
* **Common Pitfalls / Gotchas:** 
  - Integration tests require running API server instance
  - Some tests marked as skipped pending proper test infrastructure
  - Refit client setup requires proper base URL configuration

## 6. Dependencies

* **Internal Code Dependencies:** 
    * [`Zarichney.ApiClient`](../Zarichney.ApiClient/README.md) - The library being tested
* **External Library Dependencies:** 
  - `xunit` and `xunit.runner.visualstudio` - Test framework
  - `FluentAssertions` - Readable test assertions
  - `Moq` - Mocking framework
  - `Refit` and `Refit.HttpClientFactory` - HTTP client testing
  - `Microsoft.AspNetCore.Mvc.Testing` - ASP.NET Core testing utilities
* **Dependents (Impact of Changes):** 
  - None - this is a leaf test project

## 7. Rationale & Key Historical Context

* **Standalone Test Project:** Created as separate project to test `Zarichney.ApiClient` as an independent library, separate from `api-server.Tests` which tests server functionality
* **Limited Integration Tests:** Initially focused on unit testing due to lack of `Zarichney.TestingFramework` - will expand once proper test infrastructure is available
* **Migration Strategy:** Fixed broken tests in `api-server.Tests` by updating namespace references rather than migrating all tests, since those tests primarily validate server behavior

## 8. Known Issues & TODOs

* **Limited Integration Testing:** Most integration tests are skipped pending `Zarichney.TestingFramework` (GH-14)
* **Test Coverage:** Currently basic coverage - expand tests for error scenarios and edge cases
* **Server Dependency:** Integration tests require manual server startup - automate with test infrastructure
* **Mock Improvements:** Could benefit from more sophisticated HTTP mocking for edge case testing

---