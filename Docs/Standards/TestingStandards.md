# Overarching Automation Testing Standards

**Version:** 1.5
**Last Updated:** 2025-05-22

## 1. Introduction

* **Purpose:** To define the foundational philosophy, mandatory tools, project-wide conventions, and general workflow for all automated testing activities within the `api-server` solution. This document serves as the primary guide for both human and AI developers, ensuring code quality, stability, and maintainability. It links to detailed guides for specific types of testing.
* **Scope:** Applies to all **unit** and **integration** tests within the `api-server.Tests` project. Performance, security, and manual testing are out of scope for this document but may be addressed elsewhere. Detailed "how-to" instructions for unit and integration testing are provided in separate, linked documents.
* **Mandate:** Adherence is **mandatory** for all development tasks (features, fixes, refactoring), especially those performed by AI coders. Tests are a critical part of the definition-of-done. All testing activities must align with the `api-server.Tests/TechnicalDesignDocument.md`.

## 2. Core Testing Philosophy

* **Test Behavior, Not Implementation:** Focus tests on verifying *observable outcomes* and *external behavior*. Avoid testing internal implementation details to ensure tests are **resilient** to refactoring.
* **Arrange-Act-Assert (AAA):** Structure all test methods clearly using the AAA pattern.
* **Isolation:**
    * **Unit Tests:** Test components in complete isolation, mocking *all* external dependencies. Detailed guidance in `Docs/Standards/UnitTestCaseDevelopment.md`.
    * **Integration Tests:** Ensure complete isolation between test runs, especially for shared resources. Detailed guidance in `Docs/Standards/IntegrationTestCaseDevelopment.md`.
* **Readability & Maintainability:** Test code is production code. Write clear, concise, well-named tests. Use helpers, builders, and fixtures effectively to reduce duplication. Refactoring is encouraged to eliminate redundancies.
* **Determinism:** Tests **must** be deterministic and repeatable. Flaky tests are bugs and **must** be fixed immediately or temporarily skipped with a linked issue (`[Fact(Skip = "Reason + Issue Link")]`).

## 3. Required Tooling

* **Test Framework:** xUnit
* **Assertion Library:** FluentAssertions (*Mandatory*)
* **Mocking Library:** Moq (*Mandatory*)
* **Test Data:** AutoFixture, Custom Test Data Builders
* **Integration Host:** `CustomWebApplicationFactory<Program>` (in `api-server.Tests/Framework/Fixtures/`)
* **Integration API Client:** Refit (`IZarichneyAPI` generated via `Scripts/generate-api-client.ps1` into `api-server.Tests/Framework/Client/`)
* **Integration Database:** Testcontainers (PostgreSQL) via `DatabaseFixture`
* **External HTTP Service Virtualization:** WireMock.Net (as per `api-server.Tests/TechnicalDesignDocument.md` roadmap)
* **Database Cleanup:** Respawn (within `DatabaseFixture`)
* **Code Coverage:** Coverlet
* **Contract Testing (Future Consideration):** PactNet (as per `api-server.Tests/TechnicalDesignDocument.md` roadmap)

## 4. Test Project Structure & Naming

* **Test Project:** All tests **must** reside in the `api-server.Tests` project.
* **Folder Structure:** Strictly adhere to the structure defined in the `api-server.Tests/TechnicalDesignDocument.md`. Key directories include:
    * `Framework/Client/` (Generated Refit client)
    * `Framework/Configuration/`
    * `Framework/Attributes/`
    * `Framework/Fixtures/` (`CustomWebApplicationFactory`, `DatabaseFixture`, `ApiClientFixture`)
    * `Framework/Helpers/` (`AuthTestHelper`, etc.)
    * `Framework/Mocks/Factories/`
    * `Framework/Mocks/Virtualization/` (For WireMock.Net configurations)
    * `Framework/TestData/AutoFixtureCustomizations/` (New location for advanced AutoFixture setups)
    * `Unit/` (Mirrors `api-server` structure)
    * `Integration/` (Mirrors `api-server` structure)
    * `TestData/Builders/`
* **Class Naming:** `[SystemUnderTest]Tests.cs` (e.g., `OrderServiceTests.cs`, `PaymentControllerTests.cs`).
* **Method Naming:** `[MethodName]_[Scenario]_[ExpectedOutcome]` (e.g., `CreateOrder_ValidInput_ReturnsCreatedResult`, `Login_IncorrectPassword_ReturnsBadRequest`). Names must be descriptive.

## 5. Test Categorization (Traits)

* All test methods **must** use `[Trait("Category", "...")]`.
* **Mandatory Base Traits:** `"Unit"`, `"Integration"`.
* **Mandatory Dependency Traits (Apply *all* relevant, as defined in `api-server.Tests/TechnicalDesignDocument.md` and `TestCategories.cs`):**
    * `"Database"` (For integration tests using `DatabaseFixture`)
    * `"ExternalHttp:[ServiceName]"` (e.g., `"ExternalHttp:Stripe"`, `"ExternalHttp:OpenAI"`) for tests relying on virtualized HTTP services.
    * *(Add others as needed, ensuring alignment with `TestCategories.cs`)*
* **Mutability Traits:** `"ReadOnly"`, `"DataMutating"` (as defined in `api-server.Tests/TechnicalDesignDocument.md`).
* **Purpose:** Enables granular test execution filtering for development efficiency and CI optimization.

## 6. Unit Test Standards

* **Objective:** To verify the correctness of individual, isolated software units (classes, methods).
* **Coverage Goal:** Strive for >=90% coverage of non-trivial logic in services, handlers, utilities, etc.
* **Key Principles:**
    * **Isolation:** Mock *all* dependencies using Moq.
    * **Assertions:** Use FluentAssertions with `.Because("...")`.
    * **Data:** Leverage AutoFixture and custom Builders.
* **Detailed Guidance:** For comprehensive instructions on writing unit tests, including SUT design for testability, advanced mocking, assertion patterns, and AutoFixture usage, refer to:
    * **`Docs/Standards/UnitTestCaseDevelopment.md`**

## 7. Integration Test Standards

* **Objective:** To verify interactions between components and with out-of-process dependencies like databases and virtualized external services.
* **Scope:** Cover all public API endpoints, focusing on key success/error paths, authorization, and component interactions.
* **Key Framework Components:**
    * `CustomWebApplicationFactory`, `DatabaseFixture`, `ApiClientFixture` (via shared collection fixture).
    * API interaction **must** use the generated Refit client interfaces (e.g., `IAuthApi`, `ICookbookApi`, `IPublicApi`).
    * External HTTP APIs **must be virtualized** using WireMock.Net. Live external API calls are strictly forbidden.
    * Authentication simulated via `TestAuthHandler`.
* **API Response Handling Standards:**
    * All Refit client methods return `IApiResponse<T>` wrapper types for proper HTTP response inspection.
    * **Mandatory Pattern:** Always extract the response content into a descriptively named variable before assertions:
        ```csharp
        // ✅ CORRECT - Extract content first
        var loginResponse = await client.Login(request);
        var authResult = loginResponse.Content;

        // Assert on the extracted content
        authResult.Success.Should().BeTrue();
        authResult.Email.Should().NotBeEmpty();

        // ✅ CORRECT - For collections, extract and name appropriately
        var statusResponse = await client.StatusAll();
        var serviceStatuses = statusResponse.Content;

        serviceStatuses.Should().NotBeEmpty();
        serviceStatuses.Should().HaveCountGreaterThan(3);
        ```
    * **Avoid:** Repetitive `.Content` access in assertions for better readability:
        ```csharp
        // ❌ INCORRECT - Repetitive .Content access
        loginResponse.Content.Success.Should().BeTrue();
        loginResponse.Content.Email.Should().NotBeEmpty();
        loginResponse.Content.UserId.Should().NotBeEmpty();
        ```
    * **HTTP Status Verification:** When testing specific HTTP status codes, use the response wrapper:
        ```csharp
        var response = await client.GetNonExistentResource();
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ```
* **Dependency Skipping:** Use the `[DependencyFact]` attribute for tests requiring specific dependencies (external features, infrastructure, or configurations). These attributes leverage the `IntegrationTestBase` and framework helpers to automatically skip tests if prerequisites are unmet.
    * **Implementation:** The `DependencyFact` attribute can be used in several ways:
        1. **With ExternalServices enum**: `[DependencyFact(ExternalServices.LLM, ExternalServices.GitHubAccess)]` - This directly checks if the specified features are available using the `IStatusService`. It also automatically maps each ExternalServices to the appropriate dependency trait for filtering and reporting purposes.
        2. **With InfrastructureDependency enum** (preferred for infrastructure dependencies): `[DependencyFact(InfrastructureDependency.Database)]` - This directly checks for infrastructure-level dependencies like Database or Docker availability using dedicated factory properties.
        3. **With a combination of both**: `[DependencyFact(ExternalServices.LLM, InfrastructureDependency.Database)]` - For tests requiring both API features and infrastructure dependencies.
        4. **With string-based trait dependencies** (legacy): `[DependencyFact]` combined with `[Trait(TestCategories.Dependency, TestCategories.ExternalOpenAI)]` - This uses the `ConfigurationStatusHelper` and is maintained for backward compatibility.
    * **Enum-to-Trait Mapping:** For both the ExternalServices and InfrastructureDependency approaches, there are built-in mappings to appropriate `TestCategories.Dependency` trait values (e.g., `ExternalServices.LLM` maps to `TestCategories.ExternalOpenAI`, `InfrastructureDependency.Database` maps to `TestCategories.Database`). This mapping ensures that tests can still be filtered by dependency traits when using the enum-based approaches.
    * **Configuration:** Tests using any approach will be properly skipped when the required dependencies are unavailable, with detailed skip reasons that include which features or infrastructure are unavailable and what configurations are missing.
    * **CI/CD Integration:** In CI environments, all dependencies should be properly configured or mocked/virtualized to ensure comprehensive test coverage, while local development environments may skip tests based on available dependencies.
* **Detailed Guidance:** For comprehensive instructions on writing integration tests, including `WebApplicationFactory` customization, `Testcontainers` usage, `WireMock.Net` setup, data management with `Respawn` and `AutoFixture`, and `TestAuthHandler` patterns, refer to:
    * **`Docs/Standards/IntegrationTestCaseDevelopment.md`**

## 8. Test Data Standards

* **Tools:** AutoFixture and Custom Test Data Builders (`TestData/Builders/`).
* **Usage Principles:**
    * Use **Builders** for core domain models or complex DTOs requiring specific, controlled states.
    * Use **AutoFixture** for anonymous data, simple DTOs, and populating non-critical properties.
* **Detailed Guidance:** Refer to `Docs/Standards/UnitTestCaseDevelopment.md` and `Docs/Standards/IntegrationTestCaseDevelopment.md` for specific AutoFixture customization and builder patterns.
* **Clarity:** Test data setup should be clear and maintainable within the Arrange block.

## 9. Developer Workflow & CI/CD Integration

* **Requirement:** New/updated tests **must** be included in the *same Pull Request* as the code changes they cover.
* **Local Testing (Mandatory Pre-PR):**
    1.  Run `Scripts/generate-api-client.ps1` (or `.sh`) if API contracts changed.
    2.  Run the specific tests added/modified.
    3.  Run **all unit tests** (`dotnet test --filter "Category=Unit"`).
    4.  Run relevant integration tests (e.g., `dotnet test --filter "Category=Integration&Feature=Auth"`).
    5.  Ensure **all** locally run tests pass.
* **CI/CD (GitHub Actions):**
    * Workflow runs on Pull Requests and merges to `main`.
    * Workflow **must** include: Build, Unit Tests, Integration Tests (potentially parallelized by category), Coverage Report generation/publishing.
    * PRs **must** pass all CI checks to be mergeable. CI failures must be fixed.

## 10. Quality & Maintenance

* **Avoid Brittle Tests:** Test behavior, not implementation details.
* **Avoid Trivial Tests:** Don't test basic framework/language features. Focus on logic, boundaries, and integrations.
* **Performance:** Write efficient tests. Avoid unnecessary delays. Optimize fixture setup.
* **Fix Flaky Tests:** Treat flaky tests as high-priority bugs. Fix immediately or temporarily skip with a tracking issue.
* **Refactoring:** Keep test code clean (DRY principle via helpers/fixtures). Delete obsolete tests. Add tests for bugs found post-release.
* **Code Review:** Test code is subject to the same review standards as production code. Reviewers **must** check adherence to these standards.
* **Comment TODOs:** Use `// TODO:` comments for areas needing improvement or refactoring only if issue is out-of-scope from the current assignment. Make mentions of any newly introduced TODOs in the output report.

## 11. AI Agent Consumption of These Standards

* **Purpose:** To enable AI coding agents to effectively ingest, interpret, and adhere to these testing standards when generating or maintaining test cases.
* **Documentation Structure:**
    * **Foundation:** These standards (and linked documents) use Markdown for readability and machine parsability. Consistent use of headings, lists, and fenced code blocks is crucial.
    * **Metadata Tagging (Future):** Consideration will be given to implementing explicit metadata tags (e.g., HTML-style comments or YAML frontmatter) to provide context for AI agents, allowing for efficient categorization and retrieval of relevant standards.
    * **Structured Data Snippets (Future):** For complex rules or configuration examples, embedding well-formed JSON or YAML snippets may be adopted.
* **Input for AI Coders:**
    * This `OverarchingTestingStandards.md` document.
    * The relevant specific guide: `Docs/Standards/UnitTestCaseDevelopment.md` OR `Docs/Standards/IntegrationTestCaseDevelopment.md`.
    * The source file(s) of the System Under Test (SUT).
    * The OpenAPI specification (`swagger.json`) for API-related tasks.
    * Any existing tests for the SUT as examples of patterns to follow or refactor.
* **Expected AI Behavior:**
    * Utilize OpenAPI/Swagger specifications for identifying testable API endpoints, generating basic positive/negative test cases, and understanding authentication requirements.
    * Leverage JSON Schemas (from OpenAPI) for generating valid/invalid test data payloads and formulating response body assertions.
    * Adhere strictly to naming conventions, test categorization (Traits), and required tooling.
    * Implement the AAA pattern consistently.
    * Properly mock dependencies for unit tests and use the specified framework (Refit client, `CustomWebApplicationFactory`, WireMock.Net) for integration tests.
* **Continuous Learning & Versioning:**
    * **Git-based Version Control:** All standards documents are versioned in Git. Commit messages will describe changes to standards.
    * **Semantic Versioning:** These documents will follow Semantic Versioning (e.g., v1.5.0).
    * **Changelog (Future):** A machine-readable `CHANGELOG.md` may be introduced to detail changes between versions, facilitating differential ingestion by AI agents.
    * **Feedback Loop:** AI-generated tests flagged during code review for non-adherence will serve as feedback to refine prompts or identify ambiguities in these standards.

## 12. Document References

* **Detailed Unit Testing Guide:** `Docs/Standards/UnitTestCaseDevelopment.md` (To be created)
* **Detailed Integration Testing Guide:** `Docs/Standards/IntegrationTestCaseDevelopment.md` (To be created)
* **Testing Framework Blueprint:** `api-server.Tests/TechnicalDesignDocument.md`
* **Development Workflows:** See files in `Docs/Development/` (e.g., `StandardWorkflow.md`, `TestCovergeWorkflow.md`).
* **Code Standards:** `Docs/Standards/CodingStandards.md`
* **Task Management Standards:** `Docs/Standards/TaskManagementStandards.md`
