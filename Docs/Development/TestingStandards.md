# Automation Testing Standards

**Version:** 1.1
**Date:** 2025-04-15

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
* **Integration Host:** `CustomWebApplicationFactory<Program>` (in `api-server.Tests/Fixtures/`)
* **Integration API Client:** Refit (`IZarichneyAPI` generated via `Scripts/GenerateApiClient.ps1` into `api-server.Tests/Client/`)
* **Integration Database:** Testcontainers (PostgreSQL) via `DatabaseFixture`
* **Database Cleanup:** Respawn (within `DatabaseFixture`)
* **Code Coverage:** Coverlet

## 4. Test Project Structure & Naming

* **Test Project:** All tests **must** reside in the `api-server.Tests` project.
* **Folder Structure:** Strictly adhere to the structure defined in the TDD:
    * `Client/` (Generated Refit client)
    * `Configuration/`
    * `Unit/` (Mirrors `api-server` structure)
    * `Integration/` (Mirrors `api-server` structure)
    * `Fixtures/` (`CustomWebApplicationFactory`, `DatabaseFixture`)
    * `Helpers/` (`AuthTestHelper`, etc.)
    * `Mocks/Factories/`
    * `TestData/Builders/`
* **Class Naming:** `[SystemUnderTest]Tests.cs` (e.g., `OrderServiceTests.cs`, `PaymentApiTests.cs`).
* **Method Naming:** `[MethodUnderTest]_[Scenario]_[ExpectedOutcome]` (e.g., `CreateOrder_ValidInput_ReturnsCreatedResult`, `Login_IncorrectPassword_ReturnsBadRequest`). Names must be descriptive.

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
* **Framework:** Use `IClassFixture<CustomWebApplicationFactory>`, `ICollectionFixture<DatabaseFixture>`. Derive tests from a base class (e.g., `IntegrationTestBase`) providing access to the factory, fixtures, and the generated Refit client (`IZarichneyAPI`).
* **API Interaction:** All interactions with the `api-server` **must** go through the generated `IZarichneyAPI`. Direct service calls are forbidden in integration tests.
* **Database (`DatabaseFixture`):**
    * Tests requiring DB interaction **must** use `ICollectionFixture<DatabaseFixture>`.
    * **Mandatory:** Call `await _dbFixture.ResetDatabaseAsync();` at the *beginning* of each test requiring a clean database state.
* **External APIs (Stripe, OpenAI, etc.):**
    * **Must be mocked.** Mocks are configured in `CustomWebApplicationFactory` via `Mocks/Factories/`.
    * Retrieve mocks in tests using `_factory.Services.GetRequiredService<Mock<IExternalService>>()`.
    * Configure `Setup()` and `Verify()` on these mocks as needed per test.
    * **Live external API calls are strictly forbidden.**
* **Authentication (`TestAuthHandler`):** Use helper methods (e.g., `_authHelper.CreateAuthenticatedClient(userId, roles)`) to obtain an `HttpClient` (and subsequently a Refit client) configured with simulated user claims/roles for testing authorization.
* **Assertions (FluentAssertions):** Assert on API response status codes, DTO content (`Should().BeEquivalentTo()`), and expected side effects (e.g., database state changes verified via a separate DbContext instance obtained from the factory's services *after* the API call). Use `.Because("...")`.

## 8. Test Data Standards

* **Tools:** AutoFixture and Custom Test Data Builders (`TestData/Builders/`).
* **AutoFixture:** Use for anonymous data, simple DTOs, primitive test parameters (`[AutoData]`). Configure customizations as needed.
* **Builders:** **TODO:** Implement builders for core domain models/complex DTOs. Use when specific, controlled object states are required. Leverage AutoFixture within builders.

## 9. Developer Workflow & CI/CD Integration

* **Requirement:** New/updated tests **must** be included in the *same Pull Request* as the code changes they cover.
* **Local Testing (Mandatory Pre-PR):**
    1.  Run `Scripts/GenerateApiClient.ps1` if API contracts changed.
    2.  Run the specific tests added/modified.
    3.  Run **all unit tests** (`dotnet test --filter "Category=Unit"`).
    4.  Run relevant integration tests (e.g., `dotnet test --filter "Category=Integration&Category=Database"`).
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
