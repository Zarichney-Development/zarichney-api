# Module/Directory: /api-server.Tests/Helpers

**Last Updated:** 2025-04-17

> **Parent:** [`/api-server.Tests/README.md`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** A collection of utility and helper classes that support the testing infrastructure for the Zarichney API.
* **Key Responsibilities:**
    * Providing authentication test helpers (`AuthTestHelper`, `TestAuthHandler`)
    * Managing test categories and traits (`TestCategories`)
    * Supporting conditional test execution (`DockerAvailableFactAttribute`)
    * Generating random test data (`GetRandom`)
    * Checking configuration status across the application (`ConfigurationStatusHelper`)
    * Supporting environment-specific testing (`TestEnvironmentHelper`)
* **Why it exists:** To centralize common testing utilities, reduce code duplication, and provide a consistent approach to handling cross-cutting testing concerns.

## 2. Architecture & Key Concepts

* **Authentication Helpers:** `AuthTestHelper` generates test tokens for authentication, while `TestAuthHandler` provides an authentication scheme for tests.
* **Test Configuration:** `TestEnvironmentHelper` manages environment-specific test configurations.
* **Test Infrastructure:** `DockerAvailableFactAttribute` enables conditional execution of tests based on Docker availability.
* **Data Generation:** `GetRandom` provides methods for generating random test data.
* **Configuration Status:** `ConfigurationStatusHelper` provides methods to check the status of configuration items by querying the `/api/status/config` endpoint.

## 3. Interface Contract & Assumptions

* **ConfigurationStatusHelper:**
    * `Task<List<ConfigurationItemStatus>> GetConfigurationStatusAsync(CustomWebApplicationFactory factory)`:
        * **Purpose:** Fetches the status of all configuration items from the `/api/status/config` endpoint.
        * **Critical Preconditions:** `factory` must be a valid `CustomWebApplicationFactory` instance.
        * **Critical Postconditions:** Returns a list of configuration item statuses or throws an exception if the request fails.
        * **Non-Obvious Error Handling:** Throws `InvalidOperationException` if response deserialization fails.
    * `bool IsConfigurationAvailable(List<ConfigurationItemStatus> statuses, string configName)`:
        * **Purpose:** Checks if a specific configuration item is available (configured).
        * **Critical Preconditions:** `statuses` must not be null, `configName` must not be null or whitespace.
        * **Critical Postconditions:** Returns `true` only if a matching item with "Configured" status is found.
        * **Non-Obvious Error Handling:** Returns `false` if the item is not found or not "Configured".
* **AuthTestHelper:**
    * `GenerateTestToken(string userId, string[] roles)`: Generates a test token for authentication.
    * `ValidateTestToken(string token)`: Validates a test token.
* **TestCategories:**
    * Constants for consistent test categorization using traits.
* **Critical Assumptions:**
    * `ConfigurationStatusHelper` assumes the `/api/status/config` endpoint is working correctly and returning valid data.
    * `AuthTestHelper` assumes the token format is compatible with `TestAuthHandler`.
    * Testing utilities assume their dependencies (e.g., `CustomWebApplicationFactory`) are properly initialized.

## 4. Local Conventions & Constraints

* **Naming:** Helper classes follow the naming convention of `[Purpose]Helper` or `[Purpose]Handler`.
* **Common Patterns:** Static helper methods are used for utility functions, while instance methods are used for stateful helpers.
* **Exception Handling:** Helpers should validate inputs and throw appropriate exceptions for invalid parameters.
* **Test Categories:** All tests should use the categories defined in `TestCategories` for consistent filtering.

## 5. How to Work With This Code

* **Setup:** Most helper classes don't require special setup, as they are instantiated as needed by tests.
* **Testing:**
    * Unit tests for helper classes are located in `api-server.Tests/Unit/Helpers/`.
    * Integration tests that use the helpers are spread across the integration test directories.
* **Common Pitfalls / Gotchas:**
    * `ConfigurationStatusHelper` performs case-sensitive comparisons for both config names and status values.
    * `DockerAvailableFactAttribute` can lead to tests being silently skipped if Docker is not available.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/api-server.Tests/Fixtures`](../Fixtures/README.md) - For `CustomWebApplicationFactory`
    * [`/api-server/Services/Status`](../../api-server/Services/Status/README.md) - For `ConfigurationItemStatus` model
* **External Library Dependencies:**
    * `Xunit` - For test attributes and assertions
    * `FluentAssertions` - For more expressive assertions
    * `Microsoft.AspNetCore.Mvc.Testing` - For `WebApplicationFactory`
* **Dependents (Impact of Changes):**
    * Any test class that uses these helpers - Changes to these helpers can affect many tests
    * Integration tests that need to check if required configurations are available

## 7. Rationale & Key Historical Context

* **ConfigurationStatusHelper:** Created to support conditional test execution based on available configurations, enabling tests to be skipped when dependencies (like databases or API keys) are not configured rather than failing.
* **DockerAvailableFactAttribute:** Added to handle tests that require Docker, allowing them to be skipped gracefully when Docker is not available.
* **TestCategories:** Standardized to ensure consistent categorization and filtering of tests.

## 8. Known Issues & TODOs

* Consider adding caching to `ConfigurationStatusHelper.GetConfigurationStatusAsync()` to reduce redundant API calls during a test run.
* Evaluate adding a more comprehensive configuration validation system that can automatically skip tests when required configurations are missing.
* Consider creating a framework for conditional test execution based on multiple configuration dependencies.