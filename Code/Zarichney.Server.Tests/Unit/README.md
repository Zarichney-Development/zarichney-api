# README: /Unit Tests Directory

**Version:** 1.1
**Last Updated:** 2025-09-27
**Parent:** `../README.md`

## 1. Purpose & Responsibility

This directory, `/Unit/`, contains all **unit tests** for the `zarichney-api/Zarichney.Server` project. The primary purpose of these tests is to verify the correctness of individual software units—such as classes, methods, services, command/query handlers, and utility functions—in **complete isolation** from their dependencies.

Unit tests are foundational to our testing strategy, aiming to:
* Provide fast feedback to developers on code changes.
* Pinpoint defects accurately within specific components.
* Prevent regressions in business logic and core functionalities.
* Facilitate safer code refactoring by ensuring individual units still behave as expected after changes.
* Achieve comprehensive code coverage for non-trivial logic through continuous testing excellence.

All unit tests **must** adhere to the detailed guidelines and standards outlined in **`../../../Docs/Standards/UnitTestCaseDevelopment.md`**.

## 2. Unit Testing Approach & Key Concepts

Our unit testing strategy emphasizes rigor, clarity, and maintainability:

* **Complete Isolation:** The System Under Test (SUT) is tested without involving external dependencies like databases, network services, file systems, or live configuration sources. All such dependencies **must be mocked** using Moq.
* **Speed:** Unit tests must execute very quickly (typically in milliseconds) to ensure rapid feedback cycles.
* **Test Behavior, Not Implementation:** Tests should focus on the observable outcomes and external contract of a unit, rather than its internal implementation details. This makes tests more resilient to refactoring.
* **AAA Pattern (Arrange-Act-Assert):** All test methods must clearly follow this structure.
* **Core Tooling:**
    * **xUnit:** Test framework (`[Fact]`, `[Theory]`).
    * **Moq:** Mocking library for creating test doubles of dependencies.
    * **FluentAssertions:** Assertion library for expressive and readable verifications.
    * **AutoFixture:** For generating anonymous test data, especially with attributes like `[AutoData]` and `[Frozen]` for parameters and mock setup.
* **Coverage Goal:** As stated in the `../TechnicalDesignDocument.md` (Section 8), we aim for comprehensive unit test coverage of all non-trivial logic through continuous testing excellence.

## 3. Directory Structure & Organization

The directory structure within `/Unit/` is designed to **mirror the project structure of the `Zarichney.Server` application**. This convention makes it easy to locate unit tests corresponding to a specific component or module. For example:
* Unit tests for `Zarichney.Server/Services/Auth/AuthService.cs` would typically be found in `Zarichney.Server.Tests/Unit/Services/Auth/AuthServiceTests.cs`.
* Unit tests for controllers might be in `Zarichney.Server.Tests/Unit/Controllers/[ControllerName]/[ControllerName]UnitTests.cs`.

**Test File Granularity (as per TDD Section 8):**
* For complex SUT methods or classes that warrant numerous test cases, it's encouraged to create a dedicated test file focusing on that specific method or a cohesive set of related methods (e.g., `MyService_ComplexMethodBehaviorTests.cs`).
* If a SUT class or method has only a few test cases, they can be grouped within a more general test file for that SUT (e.g., `MyServiceTests.cs`). The expectation is that if test cases for a particular function grow, they may be migrated to their own dedicated file for better organization.

Key subdirectories within `/Unit/` typically include (but are not limited to):
* `./Config/` (Tests for configuration models, middleware, etc.)
* `./Controllers/` (Tests for controller logic, often focused on parameter mapping, service delegation, and action result creation, with services mocked)
* `./Cookbook/` (Tests for components within the Cookbook domain)
* `./Framework/` (Unit tests for components *within* the `../Framework/` directory itself, e.g., `Attributes/`, `Helpers/`)
* `./Middleware/` (If testing middleware in isolation)
* `./Services/` (Tests for business logic within various services)
* `./Startup/` (Tests for dependency injection wiring or startup validation logic)

Each significant subdirectory under `/Unit/` should have its own `README.md` detailing its specific focus if it deviates from general unit testing patterns or contains a large, distinct group of tests.

## 4. Key Standards & Development Guide

The definitive guide for writing unit tests in this project is:
* **`../../../Docs/Standards/UnitTestCaseDevelopment.md`**

This document provides detailed instructions on:
* Designing production code for testability.
* Setting up unit tests (AAA, naming, structure).
* Advanced mocking with Moq.
* Writing expressive assertions with FluentAssertions.
* Effective test data management with AutoFixture.
* Testing specific scenarios like asynchronous code and exception handling.
* Common pitfalls and how to avoid them.

All developers (human and AI) writing unit tests **must** adhere to this guide.
Supporting standards include:
* `../../../Docs/Standards/TestingStandards.md` (Overarching principles and tooling).
* `../../../Docs/Standards/CodingStandards.md` (Essential for writing testable SUTs).
* `../TechnicalDesignDocument.md` (Provides context on the overall testing strategy and framework components that might be unit-tested here).

## 5. How to Work With This Code

### Writing New Unit Tests

1.  **Identify the SUT:** Determine the specific class or method in `Zarichney.Server` that needs testing.
2.  **Create Test File:** Following the directory structure and naming conventions, create or locate the appropriate test file in `/Unit/`.
3.  **Design for Testability:** Ensure the SUT adheres to principles in `../../../Docs/Standards/CodingStandards.md` (e.g., uses DI, avoids statics for dependencies). Refactor the SUT if necessary.
4.  **Follow `UnitTestCaseDevelopment.md`:** Apply the AAA pattern, mock all dependencies using Moq, use AutoFixture for data, and write clear assertions with FluentAssertions.
5.  **Categorize:** Add `[Trait("Category", "Unit")]`.

### Running Unit Tests

* **Run all unit tests:**
  ```bash
  dotnet test --filter "Category=Unit"
  ```
* **Run specific unit tests:** Use more specific filters, for example:
  ```bash
  dotnet test --filter "FullyQualifiedName~MyNamespace.MyClassTests"
  dotnet test --filter "DisplayName~MyMethod_MyScenario_MyExpectedOutcome"
  ```
* Ensure all unit tests pass locally before committing code, as per the workflow defined in `../../../Docs/Standards/TestingStandards.md`.

## 6. Dependencies

### Internal Dependencies

* **`Zarichney.Server` Project:** This is the primary dependency, as its components are the Systems Under Test (SUTs).
* **`../Framework/Helpers/GetRandom.cs`:** May be used for simple data generation needs, though AutoFixture attributes (`[AutoData]`, `[Frozen]`) are often preferred for parameter-level data.
* **This directory also contains unit tests for components within `../Framework/` itself** (e.g., `../Unit/Framework/Attributes/`, `../Unit/Framework/Helpers/`).

### Key External Libraries

* **`xUnit.net`**: The test framework.
* **`Moq`**: The mocking library.
* **`FluentAssertions`**: The assertion library.
* **`AutoFixture`** (including `AutoFixture.Xunit2` and `AutoFixture.AutoMoq`): For test data generation and integration with Moq.

## 7. Rationale & Key Historical Context

Unit tests form the largest and most critical layer of our automated testing pyramid. Their speed and precision are essential for:
* **Rapid Feedback:** Allowing developers to quickly verify changes and catch regressions locally.
* **Design Guidance:** Writing unit tests often drives better, more decoupled designs in the production code.
* **Documentation:** Well-written unit tests can serve as executable documentation for the behavior of individual components.
* **Confidence for Refactoring:** A strong unit test suite allows for confident refactoring of production code, knowing that breakages in core logic will be caught.

The emphasis on strict isolation and mocking ensures that failures in unit tests point directly to issues within the SUT itself, not its dependencies.

## 8. Known Issues & TODOs

* **Coverage Gaps:** While the goal is comprehensive coverage, there may be existing areas in `Zarichney.Server` with lower unit test coverage. These should be identified and addressed progressively through continuous testing excellence, especially for critical business logic. (Tracked via tasks referencing the `../../../Docs/Templates/GHTestCoverageTask.md` template).
* **Complex SUTs:** Some older or more complex components in `Zarichney.Server` might still be challenging to unit test effectively. Ongoing refactoring for testability, guided by `../../../Docs/Standards/CodingStandards.md`, is encouraged.
* **Review AI-Generated Tests:** As AI tools assist in test generation, ensure their output is rigorously reviewed against the `../../../Docs/Standards/UnitTestCaseDevelopment.md` for quality, correctness, and adherence to best practices.

---
