# Module/Directory: Zarichney.Standards/Testing

**Version:** 2.0
**Last Updated:** 2025-05-26

> **Parent:** [`Zarichney.Standards`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory contains the official testing standards and development guides for writing comprehensive, maintainable tests within the Zarichney API project.
* **Key Responsibilities:**
    * Defining mandatory testing practices and quality standards.
    * Providing detailed guidance for unit test development and best practices.
    * Establishing standards for integration test development and execution.
    * Ensuring consistent test structure and organization across the codebase.
* **Why it exists:** To maintain high test coverage (>=90% for unit tests) and ensure reliable, maintainable automated testing that supports confident code changes and AI-assisted development.
* **Core Documents within this Directory:**
    * [`TestingStandards.md`](./TestingStandards.md): The overarching testing standards document that defines quality expectations, tooling requirements, and general testing philosophy.
    * [`UnitTestCaseDevelopment.md`](./UnitTestCaseDevelopment.md): Detailed "how-to" guide for writing effective unit tests, including patterns, best practices, and specific techniques.
    * [`IntegrationTestCaseDevelopment.md`](./IntegrationTestCaseDevelopment.md): Detailed "how-to" guide for writing integration tests, including test infrastructure setup and execution strategies.

## 2. Architecture & Key Concepts

* **Testing Framework:** Utilizes xUnit as the primary testing framework with FluentAssertions for expressive test assertions.
* **Test Categories:** Clear separation between Unit tests (fast, isolated) and Integration tests (realistic, with external dependencies).
* **Test Infrastructure:** Comprehensive test infrastructure including fixtures, helpers, and mock factories to support reliable testing.
* **Coverage Goals:** Targets >=90% unit test coverage while maintaining test quality and value.

## 3. Interface Contract & Assumptions

* **Mandatory Compliance:** All new code **MUST** include corresponding tests that follow these standards.
* **Test Categories:** Tests must be properly categorized using `[Trait("Category", "Unit")]` or `[Trait("Category", "Integration")]`.
* **Infrastructure Requirements:** Integration tests assume Docker availability for database testing via Testcontainers.
* **Quality Standards:** Tests must be maintainable, readable, and provide value in detecting regressions.

## 4. Local Conventions & Constraints

* **xUnit Framework:** All tests use xUnit with standard conventions for test organization and execution.
* **Naming Conventions:** Test classes end with `Tests` suffix and follow clear, descriptive naming patterns.
* **AAA Pattern:** Unit tests follow Arrange-Act-Assert pattern for clarity and consistency.
* **Isolation Requirements:** Unit tests must be independent and not rely on external dependencies.

## 5. How to Work With This Documentation

* **Before Writing Tests:** Review [`TestingStandards.md`](./TestingStandards.md) for overall testing philosophy and requirements.
* **Unit Test Development:** Follow the detailed guidance in [`UnitTestCaseDevelopment.md`](./UnitTestCaseDevelopment.md) for writing effective unit tests.
* **Integration Test Development:** Use [`IntegrationTestCaseDevelopment.md`](./IntegrationTestCaseDevelopment.md) for integration test patterns and infrastructure.
* **Test Review:** Reference these standards during code reviews to ensure test quality and coverage.

## 6. Dependencies

* **Parent:** [`Zarichney.Standards`](../README.md) - This directory is part of the overall documentation structure.
* **Related Standards:**
    * [`../Coding/CodingStandards.md`](../Coding/CodingStandards.md): Coding standards that apply to test code.
    * [`../Development/TestingSetup.md`](../Development/TestingSetup.md): Setup guide for testing environment and infrastructure.
* **Test Infrastructure:** 
    * Related to `../../api-server.Tests/TechnicalDesignDocument.md` for detailed test framework architecture.
    * Utilizes test framework components in `../../api-server.Tests/Framework/`.

## 7. Rationale & Key Historical Context

* Testing standards were established to support reliable AI-assisted development where comprehensive tests provide confidence for automated code changes.
* High test coverage targets were set to enable aggressive refactoring and feature development with AI assistance.
* Separate unit and integration test guides were created to address different testing concerns and infrastructure requirements.

## 8. Known Issues & TODOs

* Testing standards require periodic review to incorporate new testing patterns and framework capabilities.
* Additional guidance may be needed for testing specific architectural patterns as they are adopted.
* Performance testing standards are not yet defined and may need to be added for critical paths.