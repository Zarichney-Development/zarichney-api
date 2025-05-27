# Module/Directory: Unit Tests

**Last Updated:** 2025-05-27

**Parent:** [`Zarichney.Models.Tests`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Container directory for all unit tests of Zarichney.Models, organized by model namespace/category.
* **Key Responsibilities:** 
  - Organizing unit tests in logical subdirectories that mirror the Zarichney.Models project structure
  - Providing isolated unit tests for individual model classes
  - Ensuring consistent test patterns across all model categories
* **Why it exists:** To maintain a clear, navigable test structure that matches the production code organization.
* **Submodules:**
  * **Auth Tests:** [`Auth/`](./Auth/README.md) - Tests for authentication-related models
  * **Configuration Tests:** [`Configuration/`](./Configuration/README.md) - Tests for configuration models  
  * **Cookbook Tests:** [`Cookbook/`](./Cookbook/README.md) - Tests for cookbook domain models

## 2. Architecture & Key Concepts

* **High-Level Design:** Each subdirectory contains XUnit test classes following the pattern `{ModelName}Tests.cs`, with tests organized by functionality (serialization, validation, logic).
* **Core Logic Flow:** Tests are completely independent and stateless, using fresh test data for each test method via AutoFixture.
* **Key Data Structures:** Test classes mirror the model structure from Zarichney.Models
* **State Management:** No state management - pure unit tests
* **Test Organization Pattern:**
  ```mermaid
  graph TD
      A[Unit/] --> B[Auth/]
      A --> C[Configuration/]
      A --> D[Cookbook/]
      B --> E[AuthResultTests.cs]
      C --> F[ServerConfigTests.cs]
      D --> G[RecipeTests.cs]
      
      E --> H[JSON Serialization]
      E --> I[Factory Methods]
      E --> J[Constructor Logic]
      
      F --> K[Validation Tests]
      F --> L[Required Fields]
      
      G --> M[JSON Property Names]
      G --> N[String Length Validation]
  ```

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):** N/A - Internal test organization directory
* **Critical Assumptions:**
  * **External Systems/Config:** No external dependencies - pure unit tests
  * **Data Integrity:** All test data generated via AutoFixture provides valid inputs unless explicitly testing validation failures
  * **Implicit Constraints:** Test classes follow standard XUnit conventions and FluentAssertions patterns

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Directory Structure:** 
  - Subdirectories match Zarichney.Models namespace structure exactly
  - Test files named `{ModelName}Tests.cs`
  - Each test class focuses on one model class
* **Technology Choices:** 
  - XUnit [Fact] and [Theory] attributes
  - FluentAssertions for all assertions (.Should() syntax)
  - AutoFixture via GetRandom.Create<T>() for test data
* **Testing Patterns:**
  - Serialization: JSON round-trip testing (serialize → deserialize → compare)
  - Validation: Use Validator.TryValidateObject with ValidationContext
  - Logic: Direct method calls with assertions on results
  - Theory tests for multiple validation scenarios

## 5. How to Work With This Code

* **Setup:** No special setup required
* **Testing:**
  * **Location:** Test files in subdirectories of this Unit/ folder
  * **How to Run:** 
    ```bash
    # Run all unit tests
    dotnet test Zarichney.Models.Tests --filter "Category=Unit"
    
    # Run specific model tests
    dotnet test --filter "ClassName~AuthResultTests"
    ```
  * **Testing Strategy:** 
    - Add new test classes when new models are added to Zarichney.Models
    - Maintain 3 categories: serialization, validation, constructor/method logic
    - Use Theory tests for validation edge cases
* **Common Pitfalls / Gotchas:** 
  - Ensure JSON property names in tests match actual JsonPropertyName attributes
  - Validation attributes only trigger during explicit validation, not during object construction
  - Some validation (like [Url]) may be more permissive than expected

## 6. Dependencies

* **Internal Code Dependencies:**
  * [`../../Zarichney.Models`](../../Zarichney.Models/README.md) - Models being tested
  * [`../../Zarichney.TestingFramework`](../../Zarichney.TestingFramework/README.md) - GetRandom helper
* **External Library Dependencies:** Inherited from parent test project
* **Dependents (Impact of Changes):** Parent test project depends on this organization structure

## 7. Rationale & Key Historical Context

* **Why namespace-based organization:** Maintains clear separation of concerns and makes it easy to locate tests for specific model types
* **Why flat test class structure:** Each model gets exactly one test class to avoid confusion and maintain simplicity

## 8. Known Issues & TODOs

* Consider adding integration tests if models begin having cross-dependencies
* May need additional subdirectories as the Zarichney.Models project grows

---