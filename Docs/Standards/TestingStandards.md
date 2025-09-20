# Overarching Automation Testing Standards

**Version:** 1.8
**Last Updated:** 2025-09-10

## 1. Introduction

* **Purpose:** To define the foundational philosophy, mandatory tools, project-wide conventions, and general workflow for all automated testing activities within the `Zarichney.Server` solution. This document serves as the primary guide for both human and AI developers, ensuring code quality, stability, and maintainability. It links to detailed guides for specific types of testing.
* **Scope:** Applies to all **unit** and **integration** tests within the `Zarichney.Server.Tests` project. Performance, security, and manual testing are out of scope for this document but may be addressed elsewhere. Detailed "how-to" instructions for unit and integration testing are provided in separate, linked documents.
* **Mandate:** Adherence is **mandatory** for all development tasks (features, fixes, refactoring), especially those performed by AI coders. Tests are a critical part of the definition-of-done. All testing activities must align with the `Zarichney.Server.Tests/TechnicalDesignDocument.md`.

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
* **Integration Host:** `CustomWebApplicationFactory<Program>` (in `Zarichney.Server.Tests/Framework/Fixtures/`)
* **Integration API Client:** Refit (Multiple granular interfaces generated via `Scripts/generate-api-client.sh` into `Zarichney.Server.Tests/Framework/Client/`)
* **Integration Database:** Testcontainers (PostgreSQL) via `DatabaseFixture`
* **External HTTP Service Virtualization:** WireMock.Net (as per `Zarichney.Server.Tests/TechnicalDesignDocument.md` roadmap)
* **Database Cleanup:** Respawn (within `DatabaseFixture`)
* **Code Coverage:** Coverlet
* **Contract Testing (Future Consideration):** PactNet (as per `Zarichney.Server.Tests/TechnicalDesignDocument.md` roadmap)

## 4. Test Project Structure & Naming

* **Test Project:** All tests **must** reside in the `Zarichney.Server.Tests` project.
* **Folder Structure:** Strictly adhere to the structure defined in the `Zarichney.Server.Tests/TechnicalDesignDocument.md`. Key directories include:
    * `Framework/Client/` (Generated Refit client)
    * `Framework/Configuration/`
    * `Framework/Attributes/`
    * `Framework/Fixtures/` (`CustomWebApplicationFactory`, `DatabaseFixture`, `ApiClientFixture`)
    * `Framework/Helpers/` (`AuthTestHelper`, etc.)
    * `Framework/Mocks/Factories/`
    * `Framework/Mocks/Virtualization/` (For WireMock.Net configurations)
    * `Framework/TestData/AutoFixtureCustomizations/` (New location for advanced AutoFixture setups)
    * `Unit/` (Mirrors `Zarichney.Server` structure)
    * `Integration/` (Mirrors `Zarichney.Server` structure)
    * `TestData/Builders/`
* **Class Naming:** `[SystemUnderTest]Tests.cs` (e.g., `OrderServiceTests.cs`, `PaymentControllerTests.cs`).
* **Method Naming:** `[MethodName]_[Scenario]_[ExpectedOutcome]` (e.g., `CreateOrder_ValidInput_ReturnsCreatedResult`, `Login_IncorrectPassword_ReturnsBadRequest`). Names must be descriptive.

## 5. Test Categorization (Traits)

* All test methods **must** use `[Trait("Category", "...")]`.
* **Mandatory Base Traits:** `"Unit"`, `"Integration"`.
* **Mandatory Dependency Traits (Apply *all* relevant, as defined in `Zarichney.Server.Tests/TechnicalDesignDocument.md` and `TestCategories.cs`):**
    * `"Database"` (For integration tests using `DatabaseFixture`)
    * `"ExternalHttp:[ServiceName]"` (e.g., `"ExternalHttp:Stripe"`, `"ExternalHttp:OpenAI"`) for tests relying on virtualized HTTP services.
    * *(Add others as needed, ensuring alignment with `TestCategories.cs`)*
* **Mutability Traits:** `"ReadOnly"`, `"DataMutating"` (as defined in `Zarichney.Server.Tests/TechnicalDesignDocument.md`).
* **Purpose:** Enables granular test execution filtering for development efficiency and CI optimization.

## 6. Unit Test Standards

* **Objective:** To verify the correctness of individual, isolated software units (classes, methods).
* **Coverage Goal:** Strive for >=90% coverage of non-trivial logic in services, handlers, utilities, etc.
* **Key Principles:**
    * **Isolation:** Mock *all* dependencies using Moq.
    * **Assertions:** Use FluentAssertions and include a reason via the optional message parameter, e.g., `actual.Should().BeTrue("because the operation should succeed")`.
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
    * **Enhanced Service Builders (PR #179):** Use fluent builder patterns (e.g., `LlmServiceBuilder`) for complex service scenarios with invalid state testing capabilities.
* **Resource Management (PR #179):** Test classes managing unmanaged resources (e.g., `MemoryStream`, external connections) **must** implement `IDisposable` pattern:
    ```csharp
    public class ApiErrorResultTests : IDisposable
    {
        private readonly MemoryStream _responseStream;
        private bool _disposed = false;

        public ApiErrorResultTests()
        {
            _responseStream = new MemoryStream();
            // Test setup
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _responseStream?.Dispose();
                _disposed = true;
            }
        }
    }
    ```
* **Interface Wrapper Pattern (PR #179):** Eliminate reflection-based mocking by creating testable abstractions for external SDK dependencies (e.g., `IChatCompletionWrapper` for OpenAI integration).
* **Detailed Guidance:** Refer to `Docs/Standards/UnitTestCaseDevelopment.md` and `Docs/Standards/IntegrationTestCaseDevelopment.md` for specific AutoFixture customization and builder patterns.
* **Clarity:** Test data setup should be clear and maintainable within the Arrange block.

## 9. Developer Workflow & CI/CD Integration

* **Requirement:** New/updated tests **must** be included in the *same Pull Request* as the code changes they cover.
* **Local Testing (Mandatory Pre-PR):**
    1.  Run `Scripts/generate-api-client.ps1` (or `.sh`) if API contracts changed.
    2.  **Verify Warning-Free Build:** Ensure build completes with zero compiler or linter warnings
    3.  Run the unified test suite for comprehensive validation:
        ```bash
        # Quick validation with AI-powered analysis
        /test-report summary

        # Full test suite with detailed recommendations
        Scripts/run-test-suite.sh report

        # Traditional HTML coverage report
        Scripts/run-test-suite.sh automation
        ```
    4.  For specific test categories during development:
        ```bash
        Scripts/run-test-suite.sh report --unit-only
        Scripts/run-test-suite.sh report --integration-only

        # Enhanced dependency trait filtering (PR #179)
        dotnet test --filter "Dependency=ExternalOpenAI"   # 119 OpenAI-dependent tests
        dotnet test --filter "Dependency=ExternalGitHub"   # 13 GitHub-dependent tests
        dotnet test --filter "Category=Unit&Dependency!=ExternalOpenAI"  # Unit tests without external dependencies
        ```
    5.  Ensure **all** locally run tests pass, build is warning-free, and quality gates are met.
* **CI/CD (GitHub Actions - Phase 3 Enhanced):**
    * Workflow runs on Pull Requests and merges to both `main` and `develop` branches.
    * **Parallel Test Execution:** Tests run in parallel collections (IntegrationAuth, IntegrationCore, etc.) with up to 4 concurrent threads.
    * **Dynamic Quality Gates:** Adaptive thresholds based on historical project data and statistical analysis.
    * **AI-Powered Analysis:** Automated test result analysis with trend detection and performance insights.
    * Workflow **must** include: Build, Parallel Test Execution, AI Analysis Reports, Coverage Metrics with Historical Trending.
    * PRs **must** pass all CI checks including dynamic quality gates. CI failures must be fixed.

## 10. Unified Test Suite & AI-Powered Analysis

* **Primary Testing Interface:** The unified test suite (`Scripts/run-test-suite.sh`) is the recommended method for running tests.
* **Multiple Modes:**
    * **automation**: Traditional HTML coverage reports with browser opening
    * **report**: AI-powered markdown analysis with recommendations
    * **both**: Runs both modes for comprehensive coverage
* **Claude Integration:** Use `/test-report` commands for instant AI-powered test analysis:
    * `/test-report`: Full detailed analysis with actionable recommendations
    * `/test-report summary`: Quick executive summary for daily checks
    * `/test-report json`: Machine-readable output for CI/CD integration
    * `/test-report --performance`: Include performance analysis and trends
* **Quality Gates:**
    * Dynamic thresholds based on historical data (Phase 3)
    * Statistical analysis using standard deviation for adaptive limits
    * Linear regression for trend prediction
    * Customizable thresholds via `--threshold` parameter
* **Parallel Execution:**
    * Configured via `xunit.runner.json` with up to 4 parallel collections
    * Test collections organized by feature area for optimal parallelization
    * Complete database isolation using TestContainers

## 11. Quality & Maintenance

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

## 12. Progressive Coverage Strategy & Test Addition Guidelines

### 12.1 Coverage Progression Framework
* **Objective:** Systematic progression from 14.22% baseline to 90% coverage by January 2026 through phase-based strategic test development.
* **Timeline:** 27 months with target monthly velocity of 2.8% coverage increase.
* **Approach:** Phase-appropriate testing priorities ensure efficient resource utilization and sustainable coverage growth.

### 12.2 Coverage Phases & Strategic Focus

#### Phase 1: Foundation (14.22% → 20%)
* **Duration:** ~2 months
* **Primary Focus:** Service layer basics, API contracts, core business logic
* **Strategy:** Target low-hanging fruit and establish broad coverage foundation
* **Key Areas:**
  * Public service methods (happy path scenarios)
  * Controller action coverage (major API endpoints)
  * Core business entity validation
  * Basic integration test coverage
* **AI Coder Guidance:** Focus on straightforward unit tests with clear arrange-act-assert patterns

#### Phase 2: Growth (20% → 35%) 
* **Duration:** ~5 months
* **Primary Focus:** Service layer depth, integration scenarios, data validation
* **Strategy:** Deepen existing coverage and expand integration testing
* **Key Areas:**
  * Complex service method scenarios
  * Repository integration patterns
  * Authentication and authorization flows
  * Data transformation and validation logic
* **AI Coder Guidance:** Emphasis on integration testing and complex test data scenarios

#### Phase 3: Maturity (35% → 50%)
* **Duration:** ~5 months  
* **Primary Focus:** Edge cases, error handling, input validation, boundary conditions
* **Strategy:** Comprehensive error scenario coverage and edge case testing
* **Key Areas:**
  * Exception handling paths
  * Input validation boundary testing
  * External service error scenarios
  * Concurrent operation testing
* **AI Coder Guidance:** Focus on negative test cases and error condition coverage

#### Phase 4: Excellence (50% → 75%)
* **Duration:** ~9 months
* **Primary Focus:** Complex business scenarios, integration depth, cross-cutting concerns
* **Strategy:** Advanced integration testing and comprehensive business logic coverage
* **Key Areas:**
  * End-to-end business process validation
  * Complex integration scenarios
  * Performance-sensitive code paths
  * Security and authorization edge cases
* **AI Coder Guidance:** Advanced testing patterns and comprehensive scenario coverage

#### Phase 5: Mastery (75% → 90%)
* **Duration:** ~6 months
* **Primary Focus:** Comprehensive edge cases, performance scenarios, system integration
* **Strategy:** Complete coverage of critical paths and system-wide validation
* **Key Areas:**
  * Comprehensive performance testing
  * System integration validation
  * Advanced security scenarios
  * Complete edge case coverage
* **AI Coder Guidance:** Focus on system-wide testing and advanced scenarios

### 12.3 Test Addition Guidelines

#### Coverage-Driven Test Prioritization
Use the **Coverage Impact Matrix** to prioritize test development:

1. **High Impact, Low Effort** (Priority 1):
   * Public API methods with minimal dependencies
   * Service layer methods with clear input/output contracts
   * Validation logic with well-defined rules

2. **High Impact, High Effort** (Priority 2):
   * Complex integration scenarios
   * End-to-end business processes
   * Multi-service coordination testing

3. **Low Impact, Low Effort** (Priority 3):
   * Utility method testing
   * Simple getter/setter validation
   * Basic configuration testing

4. **Low Impact, High Effort** (Priority 4):
   * Legacy code with complex dependencies
   * Performance testing for non-critical paths
   * Comprehensive UI testing

#### Phase-Specific Test Development
When adding tests, consider current coverage phase:

* **Early Phases (1-2):** Focus on breadth and basic functionality
* **Middle Phases (3-4):** Emphasize depth and edge case coverage
* **Late Phases (5):** Complete comprehensive coverage and optimization

### 12.4 AI Coder Test Development Guidelines

#### Progressive Test Patterns
* **Phase 1-2:** Standard AAA patterns, basic mocking, simple integration tests
* **Phase 3-4:** Advanced mocking strategies, complex test data builders, negative testing
* **Phase 5:** Performance testing, comprehensive integration, advanced scenarios

#### Coverage-Aware Development
* Always check current phase before test development
* Align test complexity with phase-appropriate strategies
* Use progressive coverage analysis to identify high-impact opportunities
* Balance test coverage with maintainability and execution performance

### 12.5 Quality Gates & Velocity Tracking

#### Dynamic Thresholds
* **Regression Tolerance:** 1% coverage decrease allowed for refactoring
* **Phase Progression:** Must reach phase targets before advancing focus areas
* **Velocity Requirements:** Maintain 2.8%/month average for 90% by Jan 2026
* **Quality Maintenance:** Test pass rate must remain ≥99% throughout progression
* **Warning-Free Requirement:** All coverage work must maintain zero build warnings policy

#### Velocity Monitoring
* **Monthly Reviews:** Track coverage progression against velocity targets
* **Trend Analysis:** Identify acceleration or deceleration patterns
* **Intervention Triggers:** Act when velocity drops below 80% of required rate
* **Success Metrics:** On-track progression toward 90% coverage epic

### 12.6 Integration with Existing Workflows

#### Test Suite Commands
```bash
# Get current phase and recommendations
./Scripts/run-test-suite.sh report

# Phase-appropriate test guidance
./Scripts/run-test-suite.sh report --coverage-detail

# Progress tracking with AI insights
/test-report summary
```

#### CI/CD Integration
* Progressive coverage analysis in all PR workflows
* Phase-aware AI analysis and recommendations
* Dynamic quality gate adjustments based on current phase
* Velocity tracking and timeline alerts

### 12.7 Automated Epic Execution Environment

#### **Epic Context & Automation**
The Backend Coverage Epic (Issue #94) operates through automated AI agents in GitHub Actions CI environment:

* **Execution Environment:** GitHub Actions CI (unconfigured - external services unavailable)
* **Automation Frequency:** 4 AI agent instances per day (6-hour cron intervals)
* **Epic Branch Strategy:** All work performed on `epic/testing-coverage-to-90` branch off `develop`
* **Task Branches:** Individual tasks use `tests/issue-94-description-timestamp` pattern

#### **CI Environment Test Execution**
In the automated CI environment, the test suite operates with specific constraints:

* **Expected Skip Rate:** The number of skipped tests is controlled by the EXPECTED_SKIP_COUNT environment variable (default: 23). This reflects tests that require unavailable external dependencies in CI:
  * OpenAI API tests: 6 tests skipped
  * Stripe payment tests: 6 tests skipped
  * Microsoft Graph tests: 4 tests skipped
  * Database integration tests: 6 tests skipped
  * Production safety tests: 1 test skipped
* **Success Criteria:** 100% pass rate on ~65 executable tests
* **Quality Gate:** All executable tests must pass with zero build warnings; skipped tests (EXPECTED_SKIP_COUNT) are acceptable and expected in unconfigured environments.

**Rationale:** In CI environments where external dependencies are not configured, tests requiring those services are intentionally skipped to prevent false negatives. The skip count is set via EXPECTED_SKIP_COUNT (default: 23) and should be referenced in all validation scripts and documentation. See also Docs/Development/AutomatedCoverageEpicWorkflow.md for workflow details.

#### **Automated Coverage Workflow Integration**
AI agents follow the detailed workflow defined in:
* **Primary Workflow:** `Docs/Development/AutomatedCoverageEpicWorkflow.md`
* **Scope Selection:** Self-directed based on coverage phase and impact analysis
* **Conflict Prevention:** File-level focus areas to avoid agent overlap
* **Framework Enhancement:** Include testing infrastructure improvements when beneficial

#### **Success Metrics for Automated Execution**
* **Daily Output:** 4 pull requests per day against epic branch (target)
* **Coverage Velocity:** ~2.8% coverage increase per month toward 90% target
* **Quality Maintenance:** ≥99% test pass rate throughout progression
* **Timeline:** January 2026 target for 90% backend coverage

#### **Production Code Assumptions**
* **Default Assumption:** Production code is bug-free; all new tests should pass
* **Safe Production Changes Policy:** When tests reveal defects or testability issues:
  * **Permitted inline fixes (when safe):**
    - Minimal, behavior-preserving refactors for testability (DI, interface extraction, parameterization)
    - Targeted bug fixes when clearly attributable, with accompanying tests
    - Changes that maintain backward compatibility and public contracts
  * **Require separate issue when:**
    - Changes are large, risky, or cross-cutting
    - Architectural rewrites would be needed
    - Fix scope exceeds minimal viable correction
  * **Documentation requirement:** All production changes must be documented in PR with:
    - Clear "Production Fixes" section describing changes
    - Rationale for inline fix vs separate issue decision
    - Safety validation and test coverage proof

### 12.8 Zero-Tolerance Brittle Tests Policy

#### **Definition of Brittle Tests**
Tests that exhibit non-deterministic behavior or environmental dependencies:
* Tests with sleeps or time-based waits without deterministic control
* Reliance on wall-clock time, random data without fixed seeds
* Dependencies on real network, file system, or external services
* Non-deterministic concurrency or race conditions

#### **Enforcement Points**
* **TestMaster Sentinel:** Flags brittle patterns as CRITICAL severity
* **PR Reviews:** Changes requested for any brittle test patterns
* **Coverage Epic Agent:** Required to use framework helpers for determinism
* **CI Pipeline:** Tests must pass consistently across all runs

#### **Required Patterns for Deterministic Tests**
* Use framework fixtures (`CustomWebApplicationFactory`, `DatabaseFixture`)
* Employ test data builders with fixed seeds
* Mock all external dependencies
* Use `[DependencyFact]` for conditional execution
* Leverage AutoFixture customizations for controlled data generation

## 13. Document References

* **Detailed Unit Testing Guide:** `Docs/Standards/UnitTestCaseDevelopment.md` (To be created)
* **Detailed Integration Testing Guide:** `Docs/Standards/IntegrationTestCaseDevelopment.md` (To be created)
* **Testing Framework Blueprint:** `Zarichney.Server.Tests/TechnicalDesignDocument.md`
* **Development Workflows:** See files in `Docs/Development/` (e.g., `StandardWorkflow.md`, `TestCoverageWorkflow.md`).
* **Code Standards:** `Docs/Standards/CodingStandards.md`
* **Task Management Standards:** `Docs/Standards/TaskManagementStandards.md`
