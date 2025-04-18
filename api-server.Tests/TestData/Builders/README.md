# Module/Directory: /api-server.Tests/TestData/Builders

**Last Updated:** 2025-04-18

> **Parent:** [`api-server.Tests/TestData/`](../TestData/README.md) (Assuming a parent TestData README exists or will be created)
> If `/api-server.Tests/TestData/README.md` doesn't exist, the parent link should be:
> **Parent:** [`api-server.Tests`](../../README.md)


## 1. Purpose & Responsibility

* **What it is:** Contains builder classes designed to facilitate the creation of complex test data objects (domain entities, DTOs) in a readable and maintainable way.
* **Key Responsibilities:**
    * Providing a fluent interface (`WithProperty(value)`) for constructing test objects.
    * Encapsulating the logic for creating valid object instances with default values.
    * Allowing easy customization of specific properties for different test scenarios.
    * Potentially integrating with `AutoFixture` (via helpers like `GetRandom`) for generating random data for non-critical properties.
* **Why it exists:** To simplify the Arrange phase of tests, improve test readability, reduce code duplication in test setup, and ensure test data is created consistently according to object constraints.

## 2. Architecture & Key Concepts

* **Base Class:** Builders typically inherit from `BaseBuilder<TBuilder, TEntity>`, which provides the core `Build()` method and fluent interface support (`Self()`).
* **Individual Builders:** Each builder class (e.g., `RecipeBuilder`) is responsible for constructing instances of a specific entity or DTO (`Recipe`).
* **Constructor / Static Factory:** Builders often have a default constructor setting baseline valid properties and potentially a static factory method (e.g., `CreateRandom()`) for creating instances with randomized data.
* **`With...` Methods:** Fluent methods (`WithId`, `WithTitle`, etc.) allow specific properties of the underlying entity to be set.
* **`Build()` Method:** Returns the fully constructed entity instance.
* **Integration with Random Data:** May use helpers like `GetRandom` (which likely uses `AutoFixture` internally) to populate properties with random data.

## 3. Interface Contract & Assumptions

* **`BuilderName()` (Constructor):** Creates a builder instance initialized with baseline valid default values for the target entity.
* `BuilderName.WithProperty(value)`: Sets a specific property on the entity being built. Returns the builder instance for chaining.
* `BuilderName.Build()`: Returns the constructed `TEntity` instance.
* **Critical Assumptions:**
    * Assumes the default values set in the builder constructor result in a valid entity instance according to domain rules.
    * Assumes the target entity class (`TEntity`) has a parameterless constructor or that the builder correctly handles required constructor parameters.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Naming:** Builders are named `[EntityName]Builder`. Fluent methods are named `With[PropertyName]`.
* **Structure:** Follows the `BaseBuilder<TBuilder, TEntity>` pattern.
* **Default Values:** Builders should aim to provide sensible, valid defaults in their constructors.

## 5. How to Work With This Code

* **Using Builders:** Instantiate a builder, chain `With...` methods to customize properties, and call `Build()` to get the final object. Example: `var recipe = new RecipeBuilder().WithTitle("Custom Title").WithServings("6").Build();`
* **Adding a New Builder:**
    1.  Create a new class `MyEntityBuilder` inheriting `BaseBuilder<MyEntityBuilder, MyEntity>`.
    2.  Add a constructor to set default values for `MyEntity`.
    3.  Add `With[PropertyName]` methods for relevant properties of `MyEntity`.
    4.  Consider adding a static `CreateRandom()` method if needed.
* **Testing:** Builders themselves are rarely unit tested directly. Their correctness is validated by the tests that use them to create data.

## 6. Dependencies

* **Internal Code Dependencies:**
    * Domain models and DTOs defined in `api-server` (e.g., `Recipe`).
    * Potentially [`/api-server.Tests/Helpers/`](../Helpers/README.md) if using random data generators like `GetRandom`.
* **External Library Dependencies:**
    * Potentially `AutoFixture` (if used by `GetRandom`).
* **Dependents (Impact of Changes):**
    * All unit and integration tests that use these builders for test data setup. Changes to default values or `With...` methods can affect many tests.

## 7. Rationale & Key Historical Context

* The Builder pattern provides a robust and readable alternative to complex object instantiation in test setup code, especially for objects with many properties or complex validation rules. It promotes the DRY principle in test data creation.

## 8. Known Issues & TODOs

* Need to implement builders for other core entities/DTOs as identified in the TDD (e.g., `User`, `Customer`, `Order`).