# Module/Directory: /api-server.Tests/TestData

**Last Updated:** 2025-04-18

> **Parent:** [`api-server.Tests`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Contains test data models, builders, and utilities for creating consistent test objects.
* **Key Responsibilities:**
    * Defining test-specific data models (`Recipe.cs`, etc.)
    * Providing builder classes for complex test data creation (`Builders/`)
    * Supporting the creation of valid test data for unit and integration tests
* **Child Components:**
    * [`Builders/`](./Builders/README.md): Builder classes for constructing test data objects

## 2. Architecture & Key Concepts

* **Test Models:** Simple POCOs representing core domain objects needed for testing
* **Builders:** Classes following the Builder pattern to facilitate creating complex test objects
* **Data Integration:** Works with the random data generation utilities from [`Helpers/`](../Helpers/README.md)
* **Data Isolation:** Test data is isolated from production data models, allowing for simpler test-specific implementations

## 3. Interface Contract & Assumptions

* **Test Models:** Provide simplified versions of domain objects for testing purposes
* **Builders:** Follow the fluent interface pattern (`WithProperty(value)`) and inherit from `BaseBuilder<TBuilder, TEntity>`
* **Critical Assumptions:**
    * Test data models match the structure expected by the API endpoints under test
    * Builders create valid data objects by default, with options to create invalid data for negative testing

## 4. Local Conventions & Constraints

* **Model Naming:** Test data models use the same names as their production counterparts
* **Builder Naming:** Builders are named `[EntityName]Builder` (e.g., `RecipeBuilder`)
* **Builder Methods:** Follow the `With[PropertyName]` naming convention (e.g., `WithTitle`)
* **Random Data:** Use the `GetRandom` helper for generating random property values

## 5. How to Work With This Code

* **Creating Test Data:**
  ```csharp
  // Using a builder with specific values
  var recipe = new RecipeBuilder()
      .WithTitle("Test Recipe")
      .WithDescription("A test recipe")
      .WithIngredients(["Ingredient 1", "Ingredient 2"])
      .Build();
      
  // Using a builder with random values
  var randomRecipe = RecipeBuilder.CreateRandom().Build();
  ```

* **Adding New Test Models:**
  1. Create a new POCO class in this directory
  2. Implement required properties (matching production model where needed)
  3. Consider implementing a corresponding builder in the `Builders/` directory

* **Common Pitfalls:**
  * Test models may diverge from production models over time; review periodically
  * Default values in builders should create valid objects that pass validation

## 6. Dependencies

* **Internal Code Dependencies:**
  * [`api-server.Tests/Helpers/GetRandom.cs`](../Helpers/README.md): For random data generation
  * Production models in the main `api-server` project (for structural reference)
* **External Library Dependencies:**
  * `AutoFixture` (indirectly via `GetRandom`)
* **Dependents:**
  * Unit and integration tests throughout the test project
  * Mock factory setup code that needs to create test data

## 7. Rationale & Key Historical Context

* Dedicated test data models provide isolation from changes to production models and allow for simplified implementations focused on testing needs
* The Builder pattern was chosen to improve test readability and maintainability, especially for complex objects

## 8. Known Issues & TODOs

* Implement builders for additional entities as needed (Customer, Order, etc.)
* Consider creating a test data factory that can generate related sets of objects (e.g., a customer with orders and recipes)