# Module/Directory: Zarichney.TestingFramework

**Last Updated:** 2025-01-27

## 1. Purpose & Responsibility

* **What it is:** A centralized .NET class library containing all shared testing infrastructure, utilities, custom attributes, fixtures, mocks, base test classes, and test data for the zarichney-api solution.
* **Key Responsibilities:** 
  - Providing custom XUnit attributes for dependency-aware test execution
  - Supplying test fixtures for database, API client, and application factory setup
  - Offering shared test helpers and utilities for authentication, configuration, and randomization
  - Centralizing mock factories for external services (OpenAI, GitHub, Stripe, MS Graph)
  - Housing test data builders and sample data for consistent test scenarios
  - Maintaining test controllers for middleware and feature testing
* **Why it exists:** To promote consistency, reduce duplication, and improve maintainability across all test projects in the solution by consolidating shared testing code into a reusable library.
* **Submodules:**
    * **Attributes** - Custom XUnit attributes for conditional test execution based on dependencies
    * **Fixtures** - Test fixtures for database setup, API clients, and web application factories
    * **Helpers** - Utility classes for authentication, configuration, and test support
    * **Mocks** - Mock factories for external services and dependencies
    * **TestControllers** - Controllers for testing middleware and application features
    * **TestData** - Test data classes, builders, and sample data files

## 2. Architecture & Key Concepts

* **High-Level Design:** The framework is organized into logical subdirectories based on functionality. Key components include custom XUnit attributes that integrate with test discoverers to conditionally skip tests, fixtures that manage test infrastructure lifecycle, helpers that provide common test utilities, and mock factories that create consistent test doubles for external services.
* **Core Logic Flow:** 
  1. Custom attributes (`DependencyFactAttribute`, `ServiceUnavailableFactAttribute`) work with XUnit discoverers to evaluate test execution conditions
  2. Test fixtures manage setup/teardown of databases, API clients, and application instances
  3. Helper classes provide authentication tokens, test configuration, and utility methods
  4. Mock factories create configured test doubles for external service dependencies
  5. Test data builders generate consistent test objects and scenarios
* **Key Data Structures:** `Recipe` (test entity), `BaseBuilder<TBuilder, TEntity>` (fluent test data builder), custom attribute classes extending XUnit's `FactAttribute`
* **State Management:** Database fixtures manage PostgreSQL container lifecycle and database reset operations. API client fixtures manage authentication state and client configuration. Mock factories maintain service configurations.

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * `DependencyFactAttribute(params)`:
        * **Purpose:** Conditionally execute tests based on external service or infrastructure dependencies
        * **Critical Preconditions:** Proper configuration of dependency checking services must be available
        * **Critical Postconditions:** Tests are skipped if dependencies are unavailable, executed if available
        * **Non-Obvious Error Handling:** May throw configuration exceptions if dependency detection fails
    * `DatabaseFixture.ResetDatabaseAsync()`:
        * **Purpose:** Reset test database to clean state between tests
        * **Critical Preconditions:** Database container must be running and connection string valid
        * **Critical Postconditions:** All tables cleared, database in known clean state
        * **Non-Obvious Error Handling:** Throws if database connection fails or Respawn library encounters issues
    * `AuthTestHelper.CreateTestJwtToken()`:
        * **Purpose:** Generate JWT tokens for authenticated test scenarios
        * **Critical Preconditions:** Valid JWT configuration must be available
        * **Critical Postconditions:** Returns valid JWT token for test authentication
        * **Non-Obvious Error Handling:** May fail if JWT signing configuration is invalid
* **Critical Assumptions:**
    * **External Systems/Config:** Assumes Docker is available for database containers, valid configuration for external service connections
    * **Data Integrity:** Assumes test isolation through proper fixture usage and database resets
    * **Implicit Constraints:** Performance depends on Docker container startup and external service availability during testing

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:** Requires test-specific configuration sections in `appsettings.Testing.json`, including database connection strings, JWT settings, and external service configurations
* **Directory Structure:** Organized by functionality - `Attributes/`, `Fixtures/`, `Helpers/`, `Mocks/Factories/`, `TestControllers/`, `TestData/Builders/`, `TestData/Recipes/`
* **Technology Choices:** Uses XUnit for test framework, Testcontainers for database integration, Respawn for database cleanup, AutoFixture for test data generation, Moq for mocking
* **Performance/Resource Notes:** Database fixtures involve Docker container overhead; mock factories should be preferred over real external services in unit tests
* **Security Notes:** Test tokens and credentials should only be used in test environments; avoid exposing real API keys in test code

## 5. How to Work With This Code

* **Setup:** 
  - Ensure Docker Desktop is running for integration tests using database fixtures
  - Configure test settings in `appsettings.Testing.json`
  - Reference this project from test projects that need shared infrastructure
* **Testing:**
    * **Location:** This project contains testing infrastructure; its own tests would be in the main test projects
    * **How to Run:** Reference this project from test projects and use `dotnet test` 
    * **Testing Strategy:** Use dependency attributes for conditional execution, database fixtures for integration tests, mock factories for isolated unit tests
* **Common Pitfalls / Gotchas:** 
  - Database fixtures require Docker to be running and accessible
  - Custom attributes rely on proper XUnit discoverer registration
  - Mock factories need proper service interface configuration
  - Test data builders should maintain immutability for thread safety

## 6. Dependencies

* **Internal Code Dependencies:** None - this is a foundational library providing services to other test projects
* **External Library Dependencies:** 
  - `xunit` - Core testing framework and attributes
  - `Microsoft.AspNetCore.Mvc.Testing` - Web application testing infrastructure
  - `Testcontainers.PostgreSql` - Database container management
  - `Respawn` - Database cleanup and reset
  - `Moq` - Mocking framework
  - `AutoFixture` - Test data generation
  - `FluentAssertions` - Fluent assertion library
  - `Refit` - HTTP client generation for API testing
* **Dependents (Impact of Changes):** 
    * `api-server.Tests` - Primary consumer of all testing infrastructure
    * `Zarichney.ApiClient.Tests` - Uses shared testing utilities and fixtures
    * Future test projects - Will depend on shared infrastructure provided here

## 7. Rationale & Key Historical Context

* **Centralization Decision:** Moved from embedded test infrastructure in `api-server.Tests` to shared library to enable reuse across multiple test projects and improve solution modularity
* **Custom Attribute Design:** Created dependency-aware test attributes to handle conditional test execution based on external service availability, reducing test noise and improving CI/CD reliability
* **Fixture Pattern:** Adopted XUnit fixture pattern for managing expensive test resources like database containers and API clients

## 8. Known Issues & TODOs

* Database container startup can be slow in CI environments
* Custom XUnit discoverers may need updates if XUnit framework changes significantly  
* Mock factories may need updates when external service APIs change
* Consider adding more sophisticated test data generation capabilities