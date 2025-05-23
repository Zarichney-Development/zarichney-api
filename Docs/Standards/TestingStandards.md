# Automation Testing Standards

**Version:** 1.4
**Last Updated:** 2025-05-13

## 1. Introduction

* **Purpose:** To define the mandatory standards, practices, and quality expectations for automated testing within the `api-server` solution. These standards ensure code quality, stability, and maintainability.
* **Scope:** Applies to all **unit** and **integration** tests within the `api-server.Tests` project. Performance, security, and manual testing are out of scope.
* **Mandate:** Adherence is **mandatory** for all development tasks (features, fixes, refactoring), especially those performed by AI coders. Tests are a critical part of the definition-of-done.

## 2. Core Testing Philosophy

* **Test Behavior, Not Implementation:** Focus tests on verifying *observable outcomes* and *external behavior*. Avoid testing internal implementation details to ensure tests are **resilient** to refactoring.
* **Arrange-Act-Assert (AAA):** Structure all test methods clearly using the AAA pattern.
* **Isolation:**
  * **Unit Tests:** Test components in complete isolation, mocking *all* external dependencies (database, file system, other services, external APIs).
  * **Integration Tests:** Ensure complete isolation between test runs, especially for shared resources like the database (use `DatabaseFixture` with Respawn).
* **Readability & Maintainability:** Test code is production code. Write clear, concise, well-named tests. Use helpers, builders, and fixtures effectively to reduce duplication. Refactoring is encouraged to eliminate redundancies.
* **Determinism:** Tests **must** be deterministic and repeatable. Flaky tests are bugs and **must** be fixed immediately or temporarily skipped with a linked issue (`[Fact(Skip = "Reason + Issue Link")]`).

## 3. Required Tooling

* **Test Framework:** xUnit
* **Assertion Library:** FluentAssertions (*Mandatory*)
* **Mocking Library:** Moq (*Mandatory*)
* **Test Data:** AutoFixture, Custom Test Data Builders
* **Integration Host:** `CustomWebApplicationFactory<Program>` (in `api-server.Tests/Framework/Fixtures/`)
* **Integration API Client:** Refit (`IZarichneyAPI` generated via `Scripts/GenerateApiClient.ps1` into `api-server.Tests/Framework/Client/`)
* **Integration Database:** Testcontainers (PostgreSQL) via `DatabaseFixture`
* **Database Cleanup:** Respawn (within `DatabaseFixture`)
* **Code Coverage:** Coverlet

## 4. Test Project Structure & Naming

* **Test Project:** All tests **must** reside in the `api-server.Tests` project.
* **Folder Structure:** Strictly adhere to the structure defined in the TDD:
  * `Framework/Client/` (Generated Refit client)
  * `Framework/Configuration/`
  * `Framework/Attributes/`
  * `Framework/Fixtures/` (`CustomWebApplicationFactory`, `DatabaseFixture`, `ApiClientFixture`)
  * `Framework/Helpers/` (`AuthTestHelper`, etc.)
  * `Framework/Mocks/Factories/`
  * `Unit/` (Mirrors `api-server` structure)
  * `Integration/` (Mirrors `api-server` structure)
  * `TestData/Builders/`
* **Class Naming:** `[SystemUnderTest]Tests.cs` (e.g., `OrderServiceTests.cs`, `PaymentControllerTests.cs`).
* **Method Naming:** `[MethodName]_[Scenario]_[ExpectedOutcome]` (e.g., `CreateOrder_ValidInput_ReturnsCreatedResult`, `Login_IncorrectPassword_ReturnsBadRequest`). Names must be descriptive.

## 5. Test Categorization (Traits)

* All test methods **must** use `[Trait("Category", "...")]`.
* **Mandatory Base Traits:** `"Unit"`, `"Integration"`.
* **Mandatory Dependency Traits (Apply *all* relevant):**
  * `"Database"` (For integration tests using `DatabaseFixture`)
  * `"External:Stripe"` (For tests mocking `IStripeService`)
  * `"External:OpenAI"` (For tests mocking `ILlmService`, `ITranscribeService`)
  * `"External:GitHub"` (For tests mocking `IGitHubService`)
  * `"External:MSGraph"` (For tests mocking `IEmailService` via Graph)
  * *(Add others as needed)*
* **Purpose:** Enables granular test execution filtering.

## 6. Unit Test Standards

* **Coverage Goal:** Strive for >=90% coverage of non-trivial logic in services, handlers, utilities, etc.
* **Isolation:** Mock *all* dependencies (services, repositories, configuration objects, `ILogger`, `IHttpClientFactory`, etc.) using Moq. Use `Mock<T>.Of()` for simple mocks, `Setup()` for specific behavior, `Verify()` sparingly for critical interactions. Utilize mock factories (`Mocks/Factories/`) for consistent external service mock setups.
* **Assertions (FluentAssertions):**
  * Use specific assertions (`Should().BeEquivalentTo()`, `Should().ContainSingle()`, `Should().Throw<TException>()`).
  * **Mandatory:** Use `.Because("...")` to explain the assertion's *intent*.
* **Parameterization:** Use `[Fact]` for single cases, `[Theory]` with `[InlineData]`, `[MemberData]`, or `[AutoData]` for multiple inputs.

## 7. Integration Test Standards

* **Scope:** Cover all public API endpoints, focusing on key success/error paths, authorization, and component interactions.
* **Framework:** Use `CustomWebApplicationFactory`, `DatabaseFixture`, and `ApiClientFixture` via the shared `"Integration"` collection fixture. Derive tests from `IntegrationTestBase` or `DatabaseIntegrationTestBase`.
* **API Interaction:** All interactions with the `api-server` **must** go through the generated `IZarichneyAPI` provided by the base classes (`ApiClient` or `AuthenticatedApiClient`). **Direct service instantiation or calls are strictly forbidden in integration tests.**
* **Database (`DatabaseFixture`):**
  * Tests requiring DB interaction **must** inherit `DatabaseIntegrationTestBase` and belong to the `"Integration"` collection.
  * **Mandatory:** Call `await ResetDatabaseAsync();` at the *beginning* of each test requiring a clean database state.
* **External APIs (Stripe, OpenAI, etc.):**
  * **Must be mocked.** Mocks are configured in `CustomWebApplicationFactory` (often via `Mocks/Factories/`).
  * Retrieve mocks in tests using `Factory.Services.GetRequiredService<Mock<IExternalService>>()`.
  * Configure `Setup()` and `Verify()` on these mocks as needed per test.
  * **Live external API calls are strictly forbidden.**
* **Authentication (`TestAuthHandler`):** Use helper methods (e.g., `_authHelper.CreateAuthenticatedClient(userId, roles)` available through the base class) to obtain an `HttpClient` (and subsequently a Refit client) configured with simulated user claims/roles for testing authorization.
* **Assertions (FluentAssertions):** Assert on API response status codes, DTO content (`Should().BeEquivalentTo()`), and expected side effects (e.g., database state changes verified via a separate DbContext instance obtained from the factory's services *after* the API call). Use `.Because("...")`.
* **Dependency Skipping:** Use the `[DependencyFact]` attribute for tests requiring specific dependencies (external features, infrastructure, or configurations). These attributes leverage the `IntegrationTestBase` and framework helpers to automatically skip tests if prerequisites are unmet.
  * **Purpose:** Ensures tests that require external dependencies (databases, APIs, Docker, etc.) are skipped rather than failing when those dependencies are unavailable in the test environment.
<<<<<<< HEAD
  * **Implementation:** The `DependencyFact` attribute can be used in two ways:
    1. **With ExternalServices enum** (preferred): `[DependencyFact(ExternalServices.LLM, ExternalServices.GitHubAccess)]` - This directly checks if the specified features are available using the `IStatusService`. It also automatically maps each ExternalServices to the appropriate dependency trait for filtering and reporting purposes.
    2. **With string-based trait dependencies** (legacy): `[DependencyFact]` combined with `[Trait(TestCategories.Dependency, TestCategories.ExternalOpenAI)]` - This uses the `ConfigurationStatusHelper` and is maintained for backward compatibility.
  * **ExternalServices-to-Trait Mapping:** For the preferred approach using ExternalServices, there is a built-in mapping from each ExternalServices to the appropriate TestCategories.Dependency trait value (e.g., ExternalServices.LLM maps to TestCategories.ExternalOpenAI). This mapping ensures that tests can still be filtered by dependency traits even when using the ExternalServices approach.
  * **Configuration:** Tests using either approach will be properly skipped when the required dependencies are unavailable, with detailed skip reasons that include which features are unavailable and what configurations are missing.
=======
  * **Implementation:** The `DependencyFact` attribute can be used in several ways:
    1. **With ExternalServices enum**: `[DependencyFact(ExternalServices.LLM, ExternalServices.GitHubAccess)]` - This directly checks if the specified features are available using the `IStatusService`. It also automatically maps each ExternalServices to the appropriate dependency trait for filtering and reporting purposes.
    2. **With InfrastructureDependency enum** (preferred for infrastructure dependencies): `[DependencyFact(InfrastructureDependency.Database)]` - This directly checks for infrastructure-level dependencies like Database or Docker availability using dedicated factory properties.
    3. **With a combination of both**: `[DependencyFact(ExternalServices.LLM, InfrastructureDependency.Database)]` - For tests requiring both API features and infrastructure dependencies.
    4. **With string-based trait dependencies** (legacy): `[DependencyFact]` combined with `[Trait(TestCategories.Dependency, TestCategories.ExternalOpenAI)]` - This uses the `ConfigurationStatusHelper` and is maintained for backward compatibility.
  * **Enum-to-Trait Mapping:** For both the ExternalServices and InfrastructureDependency approaches, there are built-in mappings to appropriate TestCategories.Dependency trait values (e.g., ExternalServices.LLM maps to TestCategories.ExternalOpenAI, InfrastructureDependency.Database maps to TestCategories.Database). This mapping ensures that tests can still be filtered by dependency traits when using the enum-based approaches.
  * **Configuration:** Tests using any approach will be properly skipped when the required dependencies are unavailable, with detailed skip reasons that include which features or infrastructure are unavailable and what configurations are missing.
>>>>>>> e4adb1a64dec0488e3a7e8045ca73aaab60c268c
  * **CI/CD Integration:** In CI environments, all dependencies should be properly configured or mocked to ensure comprehensive test coverage, while local development environments may skip tests based on available dependencies.

## 8. Test Data Standards

* **Tools:** AutoFixture and Custom Test Data Builders (`TestData/Builders/`).
* **Usage:**
    * Use **Builders** for core domain models or complex DTOs where specific, controlled object states are required for testing particular logic paths or validation rules.
    * Use **AutoFixture** (typically via the `GetRandom` helper) for anonymous data, simple DTOs, primitive test parameters (`[AutoData]`), or populating non-critical properties within Builders.
* **Clarity:** Test data setup should be clear and maintainable within the Arrange block.

## 9. Developer Workflow & CI/CD Integration

* **Requirement:** New/updated tests **must** be included in the *same Pull Request* as the code changes they cover.
* **Local Testing (Mandatory Pre-PR):**
  1.  Run `Scripts/GenerateApiClient.ps1` if API contracts changed.
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

* **Dependency Skipping:** Use the `[DependencyFact]` attribute for tests requiring specific dependencies (external features, infrastructure, or configurations). These attributes leverage the `IntegrationTestBase` and framework helpers to automatically skip tests if prerequisites are unmet.
  * **Implementation:** The `DependencyFact` attribute can be used in several ways:
    1. **With ExternalServices enum**: `[DependencyFact(ExternalServices.LLM, ExternalServices.GitHubAccess)]` - This directly checks if the specified features are available using the `IStatusService`. It also automatically maps each ExternalServices to the appropriate dependency trait for filtering and reporting purposes.
    2. **With InfrastructureDependency enum** (preferred for infrastructure dependencies): `[DependencyFact(InfrastructureDependency.Database)]` - This directly checks for infrastructure-level dependencies like Database or Docker availability using dedicated factory properties.
    3. **With a combination of both**: `[DependencyFact(ExternalServices.LLM, InfrastructureDependency.Database)]` - For tests requiring both API features and infrastructure dependencies.
    4. **With string-based trait dependencies** (legacy): `[DependencyFact]` combined with `[Trait(TestCategories.Dependency, TestCategories.ExternalOpenAI)]` - This uses the `ConfigurationStatusHelper` and is maintained for backward compatibility.
  * **Enum-to-Trait Mapping:** For both the ExternalServices and InfrastructureDependency approaches, there are built-in mappings to appropriate TestCategories.Dependency trait values (e.g., ExternalServices.LLM maps to TestCategories.ExternalOpenAI, InfrastructureDependency.Database maps to TestCategories.Database). This mapping ensures that tests can still be filtered by dependency traits when using the enum-based approaches.
  * **Configuration:** Tests using any approach will be properly skipped when the required dependencies are unavailable, with detailed skip reasons that include which features or infrastructure are unavailable and what configurations are missing.
