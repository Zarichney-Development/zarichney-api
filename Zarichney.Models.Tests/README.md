# Module/Directory: Zarichney.Models.Tests

**Last Updated:** 2025-05-27

**Parent:** [`Solution Root`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** XUnit test project providing comprehensive unit test coverage for all data models in the Zarichney.Models project.
* **Key Responsibilities:** 
  - Testing JSON serialization/deserialization behavior with System.Text.Json
  - Validating model validation logic using System.ComponentModel.DataAnnotations
  - Testing constructor logic and static factory methods
  - Ensuring model behavior consistency across different scenarios
* **Why it exists:** To ensure data integrity, serialization compatibility, and validation reliability for all shared model classes used throughout the zarichney-api solution.
* **Submodules:**
  * **Unit Tests:** [`Unit/`](./Unit/README.md) - Organized unit tests by model category

## 2. Architecture & Key Concepts

* **High-Level Design:** Test project follows standard XUnit patterns with FluentAssertions for readable assertions. Tests are organized into logical subdirectories mirroring the Zarichney.Models project structure (Auth/, Configuration/, Cookbook/).
* **Core Logic Flow:** 
  1. Test data generated using AutoFixture via Zarichney.TestingFramework
  2. Models tested for serialization round-trips (serialize → deserialize → validate equality)
  3. Validation tested using Validator.TryValidateObject with ValidationContext
  4. Constructor and method logic verified with specific test scenarios
* **Key Data Structures:** Test classes for AuthResult, ServerConfig, Recipe, and future model additions
* **State Management:** Stateless unit tests with fresh test data generation per test method
* **Test Categories Covered:**
  ```mermaid
  graph TD
      A[Model Tests] --> B[Serialization Tests]
      A --> C[Validation Tests] 
      A --> D[Constructor/Method Tests]
      B --> E[JSON Property Names]
      B --> F[Round-trip Integrity]
      C --> G[Required Fields]
      C --> H[String Length Limits]
      C --> I[URL Validation]
      D --> J[Factory Methods]
      D --> K[Business Logic]
  ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):** N/A - This is a test project with no public interfaces
* **Critical Assumptions:**
  * **External Systems/Config:** None - pure unit tests with no external dependencies
  * **Data Integrity:** Models under test follow standard .NET conventions for validation attributes
  * **Implicit Constraints:** Tests assume System.Text.Json serialization patterns and DataAnnotations validation behavior

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:** No runtime configuration - test project only
* **Directory Structure:** 
  - `Unit/` - Contains all unit tests organized by model namespace
  - Test files named `{ModelName}Tests.cs`
  - Subdirectories mirror Zarichney.Models structure
* **Technology Choices:** 
  - XUnit for test framework
  - FluentAssertions for readable test assertions
  - AutoFixture (via Zarichney.TestingFramework) for test data generation
  - System.ComponentModel.DataAnnotations for validation testing
* **Testing Patterns:** 
  - Theory tests with InlineData for multiple validation scenarios
  - Fact tests for specific business logic verification
  - JSON serialization round-trip testing pattern

## 5. How to Work With This Code

* **Setup:** No special setup required beyond standard .NET 8 SDK
* **Testing:**
  * **Location:** All tests in this project (`Zarichney.Models.Tests/`)
  * **How to Run:** 
    ```bash
    dotnet test Zarichney.Models.Tests
    # Or run specific test categories
    dotnet test --filter "Category=Unit"
    ```
  * **Testing Strategy:** Focus on edge cases for validation, ensure JSON property names match API contracts, verify factory method behavior. Refer to [`../Zarichney.Standards/Testing/TestingStandards.md`](../Zarichney.Standards/Testing/TestingStandards.md)
* **Common Pitfalls / Gotchas:** 
  - URL validation using [Url] attribute is more permissive than expected (accepts FTP, etc.)
  - JSON property names must exactly match expected API contract naming
  - Required validation only triggers on explicit validation calls, not constructor usage

## 6. Dependencies

* **Internal Code Dependencies:**
  * [`Zarichney.Models`](../Zarichney.Models/README.md) - Models being tested
  * [`Zarichney.TestingFramework`](../Zarichney.TestingFramework/README.md) - GetRandom helper for test data generation
* **External Library Dependencies:**
  - `xunit` - Core testing framework
  - `FluentAssertions` - Readable test assertions
  - `System.ComponentModel.Annotations` - Validation attribute testing
  - `System.Text.Json` - JSON serialization testing
* **Dependents (Impact of Changes):** 
  - No dependents - this is a leaf test project

## 7. Rationale & Key Historical Context

* **Why separate test project:** Follows .NET testing best practices of separating test code from production code while maintaining clear project boundaries
* **Why comprehensive model testing:** Data models are foundational to API contracts and data integrity; thorough testing prevents runtime serialization/validation issues
* **Why AutoFixture:** Reduces test maintenance overhead while providing realistic test data generation

## 8. Known Issues & TODOs

* Consider adding performance tests for large model serialization if models grow significantly
* May need to add custom JsonConverter tests if complex serialization logic is added to models
* Future: Add property-based testing using FsCheck if model complexity increases

---