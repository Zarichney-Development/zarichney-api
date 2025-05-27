# Module/Directory: Zarichney.ApiClient.Tests/Unit

**Last Updated:** 2025-05-26

**(Parent Directory's README)**
> **Parent:** [`Zarichney.ApiClient.Tests`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Unit tests for the `Zarichney.ApiClient` library that test components in isolation without external dependencies.
* **Key Responsibilities:**
  - Testing dependency injection configuration and setup
  - Validating client interface registration and resolution
  - Testing configuration options and customization
  - Mocking HTTP responses to test client behavior in isolation
* **Why it exists:** To provide fast, isolated tests that verify the core functionality of the API client library without requiring external services.

## 2. Architecture & Key Concepts

* **High-Level Design:** Standard XUnit unit tests organized by functionality area (Configuration, Clients, etc.)
* **Core Logic Flow:** 
  1. Create isolated service provider instances
  2. Configure API clients using library methods
  3. Assert proper registration and configuration
  4. Mock HTTP responses for behavior testing
* **Key Data Structures:** 
  - `ServiceCollection` instances for DI testing
  - Mock HTTP message handlers for client behavior testing
* **State Management:** Stateless - each test creates fresh service provider instances

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):** N/A - Unit tests only
* **Critical Assumptions:**
    * **External Systems/Config:** No external dependencies - all HTTP calls mocked
    * **Data Integrity:** Tests assume `Zarichney.ApiClient` library compiles and loads correctly
    * **Implicit Constraints:** Tests focus on library behavior, not HTTP protocol implementation

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Directory Structure:** Organized by functional area (e.g., `Configuration/`)
* **Technology Choices:** 
  - XUnit for test framework
  - FluentAssertions for assertions
  - Moq for mocking HTTP dependencies
* **Testing Strategy:** Fast, isolated tests with no network calls

## 5. How to Work With This Code

* **Testing:**
    * **Location:** This directory contains unit tests only
    * **How to Run:** `dotnet test --filter "Category=Unit"` or `dotnet test Zarichney.ApiClient.Tests`
    * **Testing Strategy:** 
      - Mock all external HTTP dependencies
      - Focus on library configuration and setup logic
      - Test error conditions and edge cases
      - Verify proper dependency injection registration

## 6. Dependencies

* **Internal Code Dependencies:** 
    * [`Zarichney.ApiClient`](../../Zarichney.ApiClient/README.md) - Library under test
* **External Library Dependencies:** Same as parent test project

## 7. Rationale & Key Historical Context

* **Isolation Focus:** Unit tests prioritize fast execution and isolation over real-world scenarios
* **Configuration Testing:** Emphasis on DI setup since this is the primary integration point for consuming applications

## 8. Known Issues & TODOs

* **Coverage Expansion:** Add more unit tests for:
  - Error handling scenarios
  - Custom Refit settings configuration
  - HTTP client configuration options
  - Client interface method behavior with mocked responses
* **Mock Sophistication:** Could benefit from more realistic HTTP response mocking
* **Edge Cases:** Test boundary conditions and error scenarios more thoroughly

---