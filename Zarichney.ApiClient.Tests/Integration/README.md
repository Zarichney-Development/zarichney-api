# Module/Directory: Zarichney.ApiClient.Tests/Integration

**Last Updated:** 2025-05-26

**(Parent Directory's README)**
> **Parent:** [`Zarichney.ApiClient.Tests`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Integration tests for the `Zarichney.ApiClient` library that test client communication with live API servers.
* **Key Responsibilities:**
  - Testing actual HTTP communication between clients and servers
  - Validating client behavior with real API responses
  - Testing error handling with live server error conditions
  - Verifying client configuration works in real-world scenarios
* **Why it exists:** To ensure the API client library works correctly when communicating with actual API servers, complementing the isolated unit tests.

## 2. Architecture & Key Concepts

* **High-Level Design:** XUnit integration tests that create real HTTP clients and communicate with live servers
* **Core Logic Flow:** 
  1. Configure API clients with real HTTP dependencies
  2. Make actual HTTP requests to running API server
  3. Verify responses and error handling behavior
  4. Test client resilience and error scenarios
* **Key Data Structures:** 
  - Real `HttpClient` instances configured for local API server
  - Live API request/response objects
* **State Management:** Tests may require server state setup/cleanup

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):** N/A - Integration tests only
* **Critical Assumptions:**
    * **External Systems/Config:** Assumes API server is running and accessible at configured URL
    * **Data Integrity:** Assumes server returns responses matching expected contract
    * **Implicit Constraints:** Tests may be slower due to network communication and server dependencies

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Directory Structure:** Organized by API area (e.g., `PublicApiClientTests.cs`)
* **Technology Choices:** 
  - Real HTTP clients with actual network communication
  - Microsoft.AspNetCore.Mvc.Testing for test server scenarios (when available)
* **Testing Strategy:** Real HTTP communication with live or test servers

## 5. How to Work With This Code

* **Setup:** 
  - Ensure API server is running locally (typically `dotnet run --project api-server`)
  - Verify server is accessible at expected URL (default: http://localhost:5000)
* **Testing:**
    * **Location:** This directory contains integration tests only
    * **How to Run:** `dotnet test --filter "Category=Integration"` or `dotnet test Zarichney.ApiClient.Tests`
    * **Testing Strategy:** 
      - Use real HTTP clients configured for local server
      - Test actual API communication and response handling
      - Verify error handling with real server error responses
      - Currently many tests are skipped pending proper test infrastructure

## 6. Dependencies

* **Internal Code Dependencies:** 
    * [`Zarichney.ApiClient`](../../Zarichney.ApiClient/README.md) - Library under test
* **External Library Dependencies:** Same as parent test project
* **External System Dependencies:** Requires running API server instance

## 7. Rationale & Key Historical Context

* **Real Communication:** Integration tests provide confidence that clients work with actual HTTP infrastructure
* **Server Dependency:** Currently require manual server startup - will be improved once test infrastructure is available
* **Limited Scope:** Initially focused on basic connectivity - will expand once `Zarichney.TestingFramework` provides test server infrastructure

## 8. Known Issues & TODOs

* **Manual Server Requirement:** Tests currently require manually starting API server - should automate with test infrastructure (GH-14)
* **Skipped Tests:** Many tests are marked as skipped pending proper test server setup
* **Limited Coverage:** Current tests focus on basic connectivity - expand to cover:
  - Authentication scenarios
  - Error handling with various HTTP status codes
  - Different API endpoints and request types
  - Network timeout and resilience scenarios
* **Test Infrastructure:** Once `Zarichney.TestingFramework` is available, update tests to use proper test server infrastructure

---