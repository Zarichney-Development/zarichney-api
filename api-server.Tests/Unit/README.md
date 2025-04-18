# Module/Directory: /api-server.Tests/Unit

**Last Updated:** 2025-04-18

> **Parent:** [`api-server.Tests`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Contains unit tests that verify the behavior of individual classes or components within the `api-server` project in isolation.
* **Key Responsibilities:**
    * Testing the logic within specific services, handlers, repositories, utilities, etc.
    * Validating algorithms, conditional logic, state transitions, and error handling within a single unit.
    * Ensuring components adhere to their intended contracts when interacting with dependencies (via mocks).
* **Why it exists:** To provide fast, granular feedback on the correctness of individual code units, isolate failures, and facilitate refactoring by ensuring low-level logic remains correct.
* **Submodule Structure:** Tests are organized in subdirectories mirroring the structure of the `api-server` project (e.g., `./Cookbook/Recipes/RecipeServiceTests.cs`, `./Services/Status/StatusServiceTests.cs`, `./Helpers/ConfigurationStatusHelperTests.cs`).

## 2. Architecture & Key Concepts

* **Isolation:** The fundamental principle is testing units in isolation. All external dependencies (other services, repositories, `ILogger`, `IConfiguration`, `HttpClient`, `DbContext`, external APIs) **must** be mocked.
* **Mocking Framework:** Uses `Moq` as the primary mocking library.
* **Mock Factories:** May leverage mock factories from [`/api-server.Tests/Mocks/Factories/`](../Mocks/Factories/README.md) for consistent setup of mocks representing external service interfaces (like `ILlmService`), although direct `Mock<T>` instantiation is also common.
* **Assertion Library:** Uses `FluentAssertions` for writing expressive and readable assertions.
* **Test Data:** Uses a combination of direct instantiation, Test Data Builders from [`/api-server.Tests/TestData/Builders/`](../TestData/Builders/README.md), and potentially `AutoFixture` (via helpers like `GetRandom`) for creating input data and mock return values.
* **Structure:** Follows the Arrange-Act-Assert (AAA) pattern clearly within each test method.

## 3. Interface Contract & Assumptions

* **Tests as Consumers:** Unit tests act as the first consumer of a class's public (and sometimes internal) interface.
* **Mock Interactions:** Tests define the expected interactions with mocked dependencies using `mock.Setup(...)` and potentially verify critical interactions using `mock.Verify(...)`.
* **Critical Assumptions:**
    * Assumes mocks accurately represent the essential contract of the real dependencies for the scenario under test.
    * Assumes the test data provided covers the relevant logic paths and edge cases.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Test Structure:** Adhere strictly to the AAA pattern. Keep tests focused on verifying a single logical concern. Test class names end with `Tests`. Test method names follow `[MethodName]_[Scenario]_[ExpectedOutcome]`. Directory structure mirrors `api-server`.
* **Assertions:** Use specific `FluentAssertions` (e.g., `Should().Be()`, `Should().Throw<TException>()`, `Should().BeEquivalentTo()`). Include `.Because("...")` clauses for clarity.
* **Traits:** Must use `[Trait("Category", "Unit")]` and relevant `Component`, `Feature` traits. Use Dependency traits (e.g., `[Trait("Dependency", "External:OpenAI")]`) if the *unit under test* interacts with a *mock* of that dependency.
* **Mocking:** Mock *all* external dependencies. Use constructor injection in the class under test to facilitate mocking.

## 5. How to Work With This Code

* **Setup:** No external setup (like Docker) is required.
* **Running Tests:** Use `dotnet test --filter "Category=Unit"`. Filter further by `Feature`, `Component`, or specific class name as needed.
* **Adding Tests:**
    1.  Locate or create the corresponding test file in the mirrored directory structure (e.g., for `api-server/Services/MyService.cs`, use `api-server.Tests/Unit/Services/MyServiceTests.cs`).
    2.  Add `[Trait]` attributes to the class.
    3.  In the test class constructor or setup method (Arrange phase):
        * Create mocks for all dependencies of the class under test using `new Mock<IDependency>()`.
        * Set up default or specific behaviors on mocks using `Setup()`.
        * Instantiate the class under test, injecting the `.Object` property of the mocks.
        * Prepare input data using builders or direct instantiation.
    4.  Write test methods using `[Fact]` or `[Theory]`.
    5.  Follow the AAA pattern:
        * **Arrange:** Additional setup specific to the test case (if not covered in constructor).
        * **Act:** Call the method under test.
        * **Assert:** Use `FluentAssertions` to verify the result, state changes, or mock interactions (`Verify()`).
* **Common Pitfalls / Gotchas:**
    * **Incomplete Mocking:** Forgetting to mock a dependency, leading to `NullReferenceException` or unexpected behavior.
    * **Testing Implementation Details:** Writing tests that rely too heavily on the internal workings of a class, making them brittle to refactoring. Focus on testing the observable behavior and outputs.
    * **Over-verification:** Using `mock.Verify()` excessively for non-critical interactions can make tests verbose and fragile. Verify only essential collaborations.

## 6. Dependencies

* **Internal Code Dependencies:**
    * The specific `api-server` class being tested.
    * Interfaces of dependencies being mocked (from `api-server`).
    * [`/api-server.Tests/TestData/Builders/`](../TestData/Builders/README.md) - Used for data setup.
    * [`/api-server.Tests/Mocks/Factories/`](../Mocks/Factories/README.md) - Optionally used for mock creation.
* **External Library Dependencies:**
    * `Xunit` - Test framework.
    * `FluentAssertions` - Assertion library.
    * `Moq` - Mocking library.
    * Potentially `AutoFixture` (if used for test data).
* **Dependents (Impact of Changes):** CI/CD workflows consume the results. Refactoring the code under test will likely require corresponding updates to these unit tests.

## 7. Rationale & Key Historical Context

* Unit tests provide the fastest feedback loop during development and are essential for verifying the correctness of individual components and preventing regressions at a granular level. They form the base of the test pyramid.

## 8. Known Issues & TODOs

* Need to achieve target code coverage (>=90%) across all non-trivial components in `api-server`.
* Ensure consistent application of mocking strategies and AAA pattern across all unit tests.