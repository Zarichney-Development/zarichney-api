# Effective Unit Test Case Development

**Version:** 1.0
**Last Updated:** 2025-05-22

## 1. Introduction

* **Purpose:** This document provides detailed, actionable guidance on how to write effective, isolated, and maintainable unit tests for the `api-server` project. It expands upon the general principles outlined in the overarching `Docs/Standards/TestingStandards.md`.
* **Scope:** This guide applies specifically to unit testing individual components (classes, methods, services, handlers, etc.) within the `api-server` solution. All unit tests reside within the `api-server.Tests` project, typically under the `/Unit` directory.
* **Prerequisites:** Developers (human and AI) utilizing this guide **must** be familiar with:
    * `Docs/Standards/TestingStandards.md` (the overarching testing philosophy and tooling).
    * `Docs/Standards/CodingStandards.md` (for principles on writing testable production code).
    * The `api-server.Tests/TechnicalDesignDocument.md` for understanding the test framework's architecture.
* **Goal:** To enable the creation of a comprehensive suite of unit tests that are fast, reliable, provide precise feedback, and contribute to achieving >=90% code coverage for non-trivial business logic.

## 2. Core Principles of Unit Testing (Recap & Elaboration)

Unit tests are the foundation of the testing pyramid. They verify the smallest pieces of testable software in isolation.

* **Definition:** A unit test examines the behavior of a single, small, and isolated unit of work (typically a method or a class).
* **Speed:** Unit tests **must** execute extremely quickly (milliseconds). A slow unit test suite hinders rapid feedback.
* **Complete Isolation:**
    * The System Under Test (SUT) **must** be completely isolated from its external dependencies.
    * This means **no actual interaction** with databases, file systems, networks, external APIs, or out-of-process configuration sources.
    * All such dependencies **must** be replaced with test doubles (primarily mocks).
* **Test Behavior, Not Implementation Details:** Focus on *what* the unit does (its observable output or state changes based on inputs), not *how* it internally achieves it. This makes tests resilient to refactoring.
* **Arrange-Act-Assert (AAA) Structure:** Every test method **must** clearly follow this pattern:
    1.  **Arrange:** Set up preconditions, initialize the SUT, and configure mocks/stubs.
    2.  **Act:** Execute the method or property being tested on the SUT.
    3.  **Assert:** Verify that the outcome (return value, state change, or interaction with a mock) is as expected.
* **Readability and Maintainability:** Test code is production code and requires the same level of quality. Write clear, concise, and well-named tests.
* **Determinism (No Flakiness):** Tests **must** produce the same result every time they are run, regardless of environment or order. Flaky tests undermine trust and **must** be fixed or removed immediately.

## 3. Designing Production Code for Unit Testability

Effective unit testing begins with writing testable production code. Adherence to `Docs/Standards/CodingStandards.md` is paramount.

* **Criticality of Testable Design:** Code **must** be designed with testability as a primary consideration from the outset.
* **Dependency Injection (DI) Best Practices:**
    * **Constructor Injection:** **MUST** be used for all dependencies. This makes dependencies explicit and replaceable in tests.
    * **Inject Interfaces:** Depend on abstractions (interfaces) rather than concrete implementations. This allows test doubles (mocks) to be easily substituted for real dependencies.
    * **Avoid Service Locator:** Directly resolving services from `IServiceProvider` within application logic hides dependencies and makes unit testing significantly harder.
* **SOLID Principles for Testability:**
    * **Single Responsibility Principle (SRP):** Leads to smaller, focused classes that are easier to isolate and test thoroughly.
    * **Interface Segregation Principle (ISP):** Promotes lean interfaces, meaning mocks are simpler as they only need to implement methods relevant to the SUT's interaction.
    * **Liskov Substitution Principle (LSP):** Ensures that test doubles based on an interface behave predictably according to the contract.
    * **Dependency Inversion Principle (DIP):** Reinforces depending on abstractions, crucial for mocking.
* **The Humble Object Pattern:**
    * Apply this pattern to separate complex, testable business logic from components that are inherently difficult to unit test (e.g., API controllers directly handling HTTP concerns, classes making direct, complex use of EF Core `DbContext`).
    * Move core logic into services, command/query handlers (e.g., using MediatR), or other focused classes. These can then be unit-tested in isolation.
    * Keep boundary objects (controllers, direct data access wrappers) "humble," with minimal logic beyond validation, delegation, and mapping.
* **Avoiding Static Cling:**
    * Avoid static methods, properties, or classes for dependencies or stateful logic. These are difficult to replace with test doubles. Prefer instance-based designs managed by DI.
    * Stateless static utility functions with no external dependencies are acceptable but should not contain complex logic that itself requires varied test scenarios.
* **Embracing Immutability and Pure Functions:**
    * Immutable objects and pure functions (output depends only on input, no side effects) are inherently easier to test due to their predictability and lack of hidden state.
* **Refactoring for Testability:** If existing code is difficult to unit test, it **must** be refactored to improve its testability as part of the task.

## 4. Setting Up Unit Tests

* **Project Structure:** Unit tests should mirror the structure of the `api-server` project under the `api-server.Tests/Unit/` directory. For complex SUT methods warranting multiple test cases, create a dedicated test file (e.g., `MyService_ComplexMethodTests.cs`). Otherwise, group tests for a class in a single test file (`MyServiceTests.cs`).
* **Test Class Naming:** `[SystemUnderTest]Tests.cs` (e.g., `RecipeServiceTests.cs`).
* **Test Method Naming:** `[MethodName]_[Scenario]_[ExpectedOutcome]` (e.g., `CreateRecipe_WithValidInput_ReturnsSuccessResult`). Names must be descriptive and unambiguous.
* **xUnit Attributes:**
    * `[Fact]`: For a single test case.
    * `[Theory]`: For parameterized tests that run the same logic with multiple different inputs.
* **Test Categories:** All unit tests **must** be marked with `[Trait("Category", "Unit")]`.

## 5. Mocking Dependencies with Moq

Moq is the mandatory mocking library for creating test doubles of dependencies.

* **Why Mock?** To isolate the SUT, control dependency behavior for specific test scenarios, and ensure tests are fast and deterministic.
* **Mocking Interfaces:** This is the primary and preferred approach.
    ```csharp
    // Example: Mocking IRecipeRepository
    var mockRecipeRepository = new Mock<IRecipeRepository>();
    ```
* **Basic Mock Setup:**
    * **Methods returning values:**
        ```csharp
        mockRecipeRepository
            .Setup(repo => repo.GetRecipeByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(expectedRecipe) // or .Returns(syncExpectedRecipe)
            .Verifiable(); // Optional: if you intend to verify this specific call
        ```
    * **Methods returning `Task` (void async methods):**
        ```csharp
        mockEmailService
            .Setup(service => service.SendWelcomeEmailAsync(It.IsAny<string>()))
            .Returns(Task.CompletedTask); // or .ReturnsAsync(Task.CompletedTask) - style varies
        ```
    * **Setting up properties:**
        ```csharp
        var mockConfig = new Mock<IMyConfig>();
        mockConfig.SetupGet(cfg => cfg.IsEnabled).Returns(true);
        ```
    * **Throwing exceptions:**
        ```csharp
        mockRecipeRepository
            .Setup(repo => repo.GetRecipeByIdAsync(nonExistentId))
            .ThrowsAsync(new RecipeNotFoundException("Recipe not found"));
        ```
* **Argument Matching:**
    * `It.IsAny<T>()`: Matches any value of type T. Use when the specific value isn't critical to the test's logic.
    * `It.Is<T>(predicate)`: Matches based on a condition. Example: `It.Is<User>(u => u.Email == "test@example.com")`.
    * Specific values: `mock.Setup(d => d.Method("specificValue"))`.
* **Verifying Interactions (`Verify`) - Use Sparingly:**
    * Verification confirms that a specific method was called on a mock, often with particular arguments or a certain number of times.
        ```csharp
        mockEmailService.Verify(
            service => service.SendPasswordResetEmailAsync(user.Email),
            Times.Once(),
            "A password reset email should have been sent once."
        );
        ```
    * **Caution:** Overuse of `Verify` can lead to tests that are tightly coupled to implementation details. Prefer verifying the SUT's state or return value. Use `Verify` for critical side-effects that are not otherwise observable (e.g., calling an external void method that *must* occur).
* **Strict vs. Loose Mocks:** Default to Moq's loose mocks. Strict mocks (`MockBehavior.Strict`) can make tests more brittle.
* **`Mock.Of<T>()`:** For simple stubs that only need to return default values or simple configured values without complex setup:
    ```csharp
    var logger = Mock.Of<ILogger<MyService>>(); // Provides a mock that does nothing.
    var simpleDependency = Mock.Of<IDataProvider>(dp => dp.GetData() == "testData");
    ```

## 6. Writing Assertions with FluentAssertions

FluentAssertions is mandatory for its expressive, readable syntax and detailed failure messages.

* **Clarity and Readability:** Assertions should clearly state the expected outcome.
* **`.Because("...")`:** **Mandatory for all assertions.** Explain the *intent* or *reasoning* behind the assertion. This is invaluable for future maintainers (human or AI).
    ```csharp
    result.Should().BeTrue().Because("the operation was expected to succeed under these conditions");
    ```
* **Common Assertion Patterns:**
    * **Equality and Equivalence:**
        * `actual.Should().Be(expected).Because(...)` (for simple types or reference equality).
        * `actual.Should().BeEquivalentTo(expectedDto, options => options.ExcludingMissingMembers()).Because(...)` (for comparing complex objects structurally. Use `options` to customize, e.g., `Excluding(x => x.Id)`).
    * **Collections:**
        * `collection.Should().HaveCount(expectedCount).Because(...)`
        * `collection.Should().ContainSingle().Because(...)`
        * `collection.Should().ContainEquivalentOf(expectedItem).Because(...)`
        * `collection.Should().BeEmpty().Because(...)` / `NotBeEmpty()`
        * `collection.Should().Contain(item => item.Property == value).Because(...)` (for asserting presence based on a condition).
        * `collection.Should().OnlyContain(item => item.IsActive).Because(...)`
    * **Exceptions:**
        ```csharp
        Action act = () => sut.ProcessRequest(invalidInput);
        act.Should().Throw<ArgumentNullException>()
           .WithMessage("*parameterName*") // Wildcards can be used
           .And.ParamName.Should().Be("parameterName").Because("...");

        act.Should().ThrowExactly<CustomDomainException>()
           .Where(ex => ex.ErrorCode == "ERR123")
           .Because("a specific domain error was expected");
        ```
    * **Boolean values:** `actual.Should().BeTrue().Because(...)` / `BeFalse().Because(...)`.
    * **Nulls:** `actual.Should().BeNull().Because(...)` / `NotBeNull().Because(...)`.
    * **Strings:** `actual.Should().StartWith("prefix").And.EndWith("suffix").And.Contain("substring").Because(...)`; `actual.Should().BeNullOrEmpty().Because(...)`.
    * **Numbers:** `actual.Should().BePositive().Because(...)`; `actual.Should().BeInRange(min, max).Because(...)`.
    * **Dates/Times (SUT should use `TimeProvider`):**
        * `actualDateTime.Should().Be(expectedDateTime).Because(...)` (if `FakeTimeProvider` provides exact time).
        * `actualDateTime.Should().BeCloseTo(expected, TimeSpan.FromMilliseconds(100)).Because(...)` (if some minor variance is acceptable/unavoidable).
* **Avoid `.Should().BeTrue()` or `.Should().BeFalse()` when a more specific and expressive assertion is available.** For example, instead of `collection.Any(x => x.IsValid).Should().BeTrue()`, use `collection.Should().Contain(x => x.IsValid)`.

## 7. Test Data Management with AutoFixture

AutoFixture helps create anonymous, yet structurally valid, test data, reducing manual setup and making tests less brittle.

* **Purpose:** To automate the generation of test data, especially for the "Arrange" phase, focusing the test on its specific logic rather than boilerplate data creation.
* **Basic Usage (within a test method):**
    ```csharp
    var fixture = new Fixture();

    // Create a simple SUT (dependencies would ideally be mocked and injected if complex)
    // For simple SUTs with no deps, or if deps are also fixture-generated (less common for strict unit tests):
    var sut = fixture.Create<MyServiceWithoutDependencies>();

    // Create DTOs or input models
    var request = fixture.Create<MyRequestDto>();
    request.SpecificProperty = "ControlledValueForThisTest"; // Customize after creation

    // Create primitive types
    var someId = fixture.Create<Guid>();
    var someNumber = fixture.Create<int>();
    ```
* **xUnit Integration (`AutoFixture.Xunit2`):** This is the preferred way to use AutoFixture with xUnit.
    * **`[AutoData]`:** Automatically supplies all test method parameters with AutoFixture-generated instances.
        ```csharp
        [Theory, AutoData]
        public void MyMethod_WithAutoGeneratedData_BehavesAsExpected(
            MyRequestDto request,
            MyService sut) // If MyService has a parameterless ctor or resolvable deps by AutoFixture
        {
            // Act
            var result = sut.Process(request);
            // Assert
            result.Should().NotBeNull();
        }
        ```
    * **`[Frozen]`:** Crucial for unit tests. Creates a single instance of a type (often a mock) and "freezes" it, so AutoFixture reuses that same instance for any subsequent requests for that type within the same test method's resolution graph (e.g., when injecting it into the SUT).
        ```csharp
        [Theory, AutoData]
        public void ProcessOrder_WithValidUser_CallsUserService(
            OrderRequest orderRequest,
            [Frozen] Mock<IUserService> mockUserService, // Frozen mock
            OrderProcessor sut) // AutoFixture injects mockUserService.Object into sut
        {
            // Arrange
            mockUserService.Setup(s => s.IsValidUser(orderRequest.UserId)).Returns(true);

            // Act
            sut.ProcessOrder(orderRequest);

            // Assert
            mockUserService.Verify(s => s.IsValidUser(orderRequest.UserId), Times.Once());
        }
        ```
    * **`[InlineAutoData(explicitArg1, ...)]`:** Allows providing some explicit values for a `[Theory]` while AutoFixture generates the remaining parameters.
        ```csharp
        [Theory]
        [InlineAutoData(0)]
        [InlineAutoData(-1)]
        public void UpdateStock_InvalidQuantity_ThrowsException(
            int invalidQuantity, // Explicitly provided
            Product product,     // Auto-generated by AutoFixture
            StockService sut)    // Auto-generated (dependencies mocked if Frozen)
        {
            // Arrange
            product.Quantity = invalidQuantity;
            Action act = () => sut.UpdateStock(product);

            // Act & Assert
            act.Should().Throw<ArgumentOutOfRangeException>()
               .Because("quantity cannot be zero or negative");
        }
        ```
* **Customizing AutoFixture for Unit Tests:**
    * **Injecting specific values:** `fixture.Inject("SpecificStringValue");`
    * **Customizing properties:** `fixture.Customize<MyRequestDto>(composer => composer.With(dto => dto.Status, "Active"));`
    * **Omitting properties (e.g., database-generated IDs):** `fixture.Customize<MyEntity>(composer => composer.Without(e => e.Id));`
    * **Handling Recursion (if complex object graphs are involved, less common in pure unit tests):**
        ```csharp
        fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => fixture.Behaviors.Remove(b));
        fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        ```
    * For project-wide AutoFixture customizations (e.g., default behaviors, custom specimen builders for domain types), these should be defined in `api-server.Tests/Framework/TestData/AutoFixtureCustomizations/` and can be composed into custom `AutoData` attributes if needed. However, for unit tests, direct `Fixture` instance customization or simple `[Frozen]` usage is often sufficient.

## 8. Testing Specific Scenarios

* **Asynchronous Code:**
    * Unit test methods involving asynchronous SUT methods **must** be `async Task`.
        ```csharp
        [Fact]
        public async Task MyAsyncMethod_WhenCondition_ReturnsExpected()
        {
            // Arrange
            var mockDep = new Mock<IDependency>();
            mockDep.Setup(d => d.GetDataAsync()).ReturnsAsync("test data");
            var sut = new MyAsyncService(mockDep.Object);

            // Act
            var result = await sut.MyAsyncMethod();

            // Assert
            result.Should().Be("test data").Because("...");
        }
        ```
    * Always `await` calls to the SUT's asynchronous methods.
    * **Never** use `.Result` or `.Wait()` on tasks in test code; this can cause deadlocks.
    * Ensure mocks of async methods are set up with `ReturnsAsync(...)` or `ThrowsAsync(...)`.
* **Exception Handling:**
    * Test that your SUT throws the expected exceptions under specific (e.g., invalid input, dependency failure) conditions.
    * Use `Action act = () => sut.MethodUnderTest(...); act.Should().Throw<TException>() ...;`
    * Verify exception messages, inner exceptions, or custom properties on the exception if they are part of the contract.
* **Business Logic and Conditional Paths:**
    * Use `[Theory]` with `[InlineAutoData]` or `[MemberData]` to efficiently test various input combinations that trigger different logic paths.
    * Explicitly test edge cases (e.g., min/max values, empty collections, null inputs if not guarded).
    * Test invalid inputs to ensure proper validation and error handling.
* **Void Methods (Methods with Side Effects on Dependencies):**
    * If a method doesn't return a value but is expected to cause a side effect by calling a method on one of its dependencies, you verify that interaction on the mock.
        ```csharp
        // SUT method: public void ProcessAndNotify(Data data) { _notifier.Notify(data.Message); }
        [Theory, AutoData]
        public void ProcessAndNotify_WhenCalled_NotifiesWithMessage(
            Data inputData,
            [Frozen] Mock<INotifier> mockNotifier,
            ProcessorService sut)
        {
            // Act
            sut.ProcessAndNotify(inputData);

            // Assert
            mockNotifier.Verify(n => n.Notify(inputData.Message), Times.Once(),
                "The notifier should have been called with the correct message.");
        }
        ```

## 9. Common Pitfalls in Unit Testing (And How to Avoid Them)

* **Over-Mocking:**
    * **Symptom:** Tests are brittle and break even with minor internal refactoring of the SUT that doesn't change its external behavior. Mock setups are overly complex.
    * **Solution:** Mock only the *direct dependencies* of the SUT. Focus on testing the SUT's observable behavior (return values, state changes if applicable) rather than verifying every single internal call it makes to its mocks. If a mock's setup is complex, the SUT might be violating SRP or its dependencies are too chatty.
* **Testing Implementation Details instead of Behavior:**
    * **Symptom:** Tests assert on private state or the sequence of internal operations.
    * **Solution:** Test only through the SUT's public contract/API. If complex private logic needs testing, consider if it should be extracted into its own class with a public interface, then test that new class.
* **Brittle Assertions:**
    * **Symptom:** Tests fail due to trivial, unrelated changes (e.g., comparing entire complex objects when only a few properties matter, or exact string matches for long messages).
    * **Solution:** Use `BeEquivalentTo` with options to exclude irrelevant properties or compare specific properties. For strings, use `Contain`, `StartWith`, `EndWith`, or regex matching. For `DateTime`, use `BeCloseTo` or control time with `TimeProvider`.
* **Slow Unit Tests:**
    * **Symptom:** The unit test suite execution time becomes a bottleneck.
    * **Solution:** Ensure strict isolation – no actual I/O. Mocks should respond instantaneously. Avoid `Task.Delay` or lengthy computations within test logic.
* **Neglecting Edge Cases and Negative Paths:**
    * **Symptom:** Code has high line coverage, but bugs related to error handling or unusual inputs still occur.
    * **Solution:** Deliberately design test cases for invalid inputs, null arguments (where appropriate), exceptions thrown by dependencies, empty collections, boundary values (min/max), etc.
* **Excessively Complex Test Setup (Long "Arrange" Blocks):**
    * **Symptom:** The `Arrange` section of a test is very long and difficult to understand, making it hard to see what is being tested.
    * **Solution:**
        * Leverage AutoFixture more effectively (`[AutoData]`, `[Frozen]`, `[InlineAutoData]`).
        * For genuinely complex but common object setups, consider creating private helper methods within the test class or simple Test Data Builders (though full builders are often more for integration tests).
        * If the SUT requires many dependencies or complex setup, it might be an indicator that the SUT itself has too many responsibilities (violating SRP) and should be refactored.
* **Testing Configuration Logic:**
    * **Symptom:** Unit tests try to validate `IConfiguration` loading or binding.
    * **Solution:** Unit tests should receive configuration via an options interface (e.g., `IOptions<MyConfig>`) or a direct custom interface for the config object. The mock for this interface then provides the specific configuration values needed for the test. Validating the configuration loading mechanism itself is an integration concern (often covered by application startup tests).

## 10. Unit Test Checklist (Quick Reference)

Before committing unit tests, quickly verify:

1.  **Focus:** Does the test target a single unit of behavior/logic?
2.  **Isolation:** Are ALL external dependencies (DB, network, file system, other services, `DateTime.Now`, config files) properly mocked or stubbed? No live calls?
3.  **AAA:** Is the Arrange-Act-Assert pattern clearly followed?
4.  **Naming:** Is the test class and method name clear, descriptive, and following `[SUT]Tests` and `[Method]_[Scenario]_[ExpectedOutcome]`?
5.  **Assertions:** Are assertions specific, using FluentAssertions, and include a `.Because("...")` clause explaining the intent?
6.  **Data:** Is test data managed effectively (e.g., AutoFixture for anonymous data, explicit values only for scenario-specific inputs)? Are there minimal hardcoded complex objects?
7.  **Speed & Determinism:** Is the test fast and consistently produces the same result?
8.  **Coverage:** Does the test (along with others for the SUT) cover relevant positive paths, negative paths, and edge cases?
9.  **Resilience:** Does the test verify behavior rather than internal implementation details, making it resilient to SUT refactoring?
10. **Readability:** Is the test code easy to understand and maintain?
11. **Category:** Is the test marked with `[Trait("Category", "Unit")]`?

---
