# Effective Integration Test Case Development

**Version:** 1.0
**Last Updated:** 2025-05-22

## 1. Introduction

* **Purpose:** This document provides detailed, actionable guidance on how to write effective integration tests for the `api-server` project. It focuses on verifying interactions between different components of the application, including API endpoints, services, repositories, and direct infrastructure dependencies like databases (via Testcontainers) and virtualized external HTTP services (via WireMock.Net).
* **Scope:** This guide applies specifically to integration testing within the `api-server.Tests` project, typically under the `/Integration` directory. These tests ensure that different parts of the system work together as intended.
* **Prerequisites:** Developers (human and AI) utilizing this guide **must** be thoroughly familiar with:
    * `Docs/Standards/TestingStandards.md` (the overarching testing philosophy and tooling).
    * `Docs/Standards/CodingStandards.md` (for principles on writing testable production code, which also impacts integration points).
    * `api-server.Tests/TechnicalDesignDocument.md` (critically important for understanding the integration testing framework, fixtures, and tools).
* **Goal:** To create a suite of integration tests that provide high confidence in the application's overall behavior, API contracts, data persistence, and interactions with external dependencies, ensuring the system functions correctly as a cohesive unit.

## 2. Core Principles of Integration Testing

Integration tests verify the interfaces and interactions between components or systems.

* **Definition:** An integration test exercises a complete request path through relevant parts of the application, including controllers, services, repositories, and interactions with real (but controlled) infrastructure like databases or virtualized external services.
* **Scope of Test:** Broader than unit tests (which test components in isolation) but generally narrower and more focused than end-to-end (E2E) tests (which might include UI interactions). Integration tests focus on server-side component collaboration.
* **Isolation Between Test Runs:** While components are tested *together*, individual test methods (or test classes) **must** be isolated from each other to prevent interference. This typically means ensuring a clean state (e.g., for the database) before each test.
* **Test Behavior of Integrated Components:** Focus on the observable outcomes of interactions, such as API responses, database state changes, or messages sent to virtualized services.
* **Arrange-Act-Assert (AAA) Structure:** All test methods **must** clearly follow this pattern.
* **Readability and Maintainability:** Test code should be clear, concise, and well-organized.
* **Determinism:** Strive for deterministic tests by controlling the test environment, including database state and external service responses (via virtualization).

## 3. Overview of the Integration Testing Framework

The `api-server.Tests` project utilizes a sophisticated framework for integration testing. Refer to the `api-server.Tests/TechnicalDesignDocument.md` for full architectural details. Key components include:

* **`CustomWebApplicationFactory<Program>`:** Manages an in-memory instance of the `api-server` using `TestServer`. It handles:
    * Test-specific application configuration (see TDD Section 9).
    * Overriding/mocking internal services if necessary for specific test scenarios (though generally, real services are preferred in integration tests unless a dependency is prohibitively complex or slow).
    * Registering test-specific authentication (`TestAuthHandler`).
    * Configuring clients to talk to virtualized external HTTP services (e.g., WireMock.Net, as per TDD FRMK-004).
* **`DatabaseFixture`:** Manages a PostgreSQL database instance using Testcontainers. It is responsible for:
    * Starting and stopping the Dockerized PostgreSQL container.
    * Applying EF Core migrations programmatically to ensure the schema is up-to-date.
    * Providing the dynamic connection string to the test database.
    * Initializing Respawn for efficient database cleanup via its `ResetDatabaseAsync()` method.
* **`ApiClientFixture`:** Provides pre-configured instances of the auto-generated Refit clients (multiple granular interfaces), using `HttpClient` instances obtained from the `CustomWebApplicationFactory`.
* **Shared Test Collection (`[Collection("Integration")]`):** All integration tests belong to this xUnit collection to efficiently share instances of `CustomWebApplicationFactory`, `DatabaseFixture`, and `ApiClientFixture` across multiple test classes, significantly speeding up test execution by avoiding repeated fixture setup.
* **Base Test Classes:**
    * `IntegrationTestBase`: Provides common setup, access to shared fixtures (like `ApiClientFixture` and `CustomWebApplicationFactory`), and dependency checking logic.
    * `DatabaseIntegrationTestBase`: Inherits `IntegrationTestBase` and adds specific access and helper methods for `DatabaseFixture` (like `ResetDatabaseAsync()`).
* **Refit Clients (Multiple Granular Interfaces):** The auto-generated, type-safe HTTP clients used for all API interactions in integration tests. Generated by `Scripts/GenerateApiClient.ps1`.
* **`TestAuthHandler` & `AuthTestHelper`:** Used to simulate authenticated users with specific roles and claims.
* **`WireMock.Net` (Planned Integration):** For virtualizing external HTTP services, enabling deterministic testing of interactions with third-party APIs (TDD FRMK-004).
* **`[DependencyFact]` & `[DockerAvailableFact]` Attributes:** For conditional execution of tests based on the availability of configured dependencies or Docker runtime.

## 4. Setting Up Integration Tests

* **Project Structure:** Integration tests should reside in `api-server.Tests/Integration/` and mirror the `api-server`'s API structure where logical (e.g., `Integration/Controllers/MyControllerTests.cs`).
* **Test Class Naming:** `[ControllerName/FeatureName]IntegrationTests.cs` (e.g., `AuthControllerIntegrationTests.cs`, `RecipeWorkflowIntegrationTests.cs`).
* **Test Method Naming:** `[EndpointName/ActionName]_[Scenario]_[ExpectedOutcome]` (e.g., `Login_WithValidCredentials_ReturnsOkAndToken`).
* **xUnit Attributes:** Use `[Fact]` for single scenario tests. `[Theory]` can be used if multiple, similar API calls with varied data need to be tested against the same integrated setup.
* **Test Categories (Traits):**
    * All integration tests **must** be marked with `[Trait("Category", "Integration")]`.
    * Add relevant dependency traits, e.g., `[Trait("Category", "Database")]` if `DatabaseFixture` is used, or `[Trait("Category", "ExternalHttp:Stripe")]` if the test interacts with a (virtualized) Stripe service. (Refer to `TestCategories.cs`).
* **Inheritance and Fixture Injection:**
    * Test classes **must** belong to the `"Integration"` collection: `[Collection("Integration")]`.
    * Inherit from `IntegrationTestBase` or, if database interaction is needed, `DatabaseIntegrationTestBase`.
    * These base classes provide access to the shared fixtures (`Factory`, `ApiClient`, `DbFixture` etc.) via constructor injection from the collection.
    ```csharp
    [Collection("Integration")]
    public class MyControllerIntegrationTests : DatabaseIntegrationTestBase // Or IntegrationTestBase
    {
        public MyControllerIntegrationTests(IntegrationTestFixture integrationTestFixture)
            : base(integrationTestFixture)
        {
            // Access fixtures: Factory, ApiClient, DbFixture (if DatabaseIntegrationTestBase)
        }

        [Fact]
        [Trait("Category", "Integration")]
        [Trait("Category", "Database")] // If applicable
        public async Task MyEndpoint_WhenCalled_ReturnsExpected()
        {
            // Arrange
            // Use DbFixture.ResetDatabaseAsync() if needed
            // Seed data via ApiClient or DbFixture.GetContext()

            // Act
            var response = await ApiClient.GetMyResourceAsync();

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.OK);
            // ... further assertions
        }
    }
    ```

## 5. Interacting with the API via Refit Client

All interactions with the `api-server` under test **must** go through the granular Refit client interfaces.

* **Accessing the Clients:** Access specific client interfaces via the `ApiClientFixture` properties (e.g., `_apiClientFixture.UnauthenticatedAuthApi`, `_apiClientFixture.AuthenticatedCookbookApi`). Each interface provides access to a specific API area.
* **Making API Calls:**
    ```csharp
    // Example: GET request
    var response = await ApiClient.GetRecipeDetailsAsync(recipeId);

    // Example: POST request with a DTO
    var createRequest = _fixture.Create<CreateRecipeRequestDto>(); // Using AutoFixture
    var createResponse = await ApiClient.CreateRecipeAsync(createRequest);
    ```
* **Handling API Responses:** The Refit client will typically return `IApiResponse<T>` or `IApiResponse` (if configured, or handle exceptions for non-success status codes). Check `response.StatusCode`, `response.IsSuccessStatusCode`, and deserialize `response.Content` or `response.Error.Content`.
* **Client Generation:** Remember to run `Scripts/GenerateApiClient.ps1` (or `.sh`) after any changes to `api-server` controller signatures, routes, or DTOs to keep the client synchronized.

## 6. Managing Dependencies

### 6.1. Database Interactions with `DatabaseFixture` and Testcontainers

Refer to `DatabaseIntegrationTestBase` and `DatabaseFixture`.

* **Purpose:** Provides an isolated, ephemeral PostgreSQL database for each test run (shared across classes in the collection, but state reset per test).
* **Usage:**
    1.  Inherit from `DatabaseIntegrationTestBase`.
    2.  Call `await DbFixture.ResetDatabaseAsync();` at the beginning of test methods that require a clean database slate or perform data mutations. This uses Respawn to efficiently clear data.
    3.  **Seeding Data:**
        * **Preferred:** Seed data by making API calls through the `ApiClient`. This tests the entire stack.
        * **Alternative (for complex setup or verification):** Directly use `await DbFixture.GetContext()` to get a `UserDbContext` instance and interact with the database.
            ```csharp
            // In Arrange phase, after ResetDatabaseAsync()
            await using var context = DbFixture.GetContext();
            var testUser = _fixture.Create<ApplicationUser>(); // Using AutoFixture
            context.Users.Add(testUser);
            await context.SaveChangesAsync();
            ```
    4.  **Verifying Database State:** After an API call, obtain a fresh `DbContext` via `await DbFixture.GetContext()` to assert the expected changes in the database.
* **Testcontainers Best Practices (TDD FRMK-003):** The framework aims to use pinned image versions and robust wait strategies for stability.

### 6.2. External HTTP Services with `WireMock.Net` (Planned)

Based on TDD FRMK-004 and Research Report Sec 2.4.1, 6.5.

* **Purpose:** To replace live calls to external/third-party HTTP APIs (e.g., Stripe, OpenAI) with a controllable mock HTTP server. This ensures tests are deterministic, fast, and do not rely on external system availability or incur costs.
* **Setup (Conceptual - to be implemented via TDD FRMK-004):**
    * `WireMock.Net` server will likely be managed by a fixture, possibly integrated into `CustomWebApplicationFactory`.
    * `CustomWebApplicationFactory` will configure `HttpClientFactory` options for services that call external APIs to redirect their requests to the `WireMock.Net` instance.
* **Defining Stubs/Scenarios (Example):**
    ```csharp
    // Within a test, get the WireMockServer instance (e.g., from a fixture)
    // WireMockServer server = _wiremockFixture.Server; // Conceptual

    server.Reset(); // Clear previous stubs for this test
    server
        .Given(Request.Create().WithPath("/v1/external-payments").UsingPost()
            .WithBody(new JsonMatcher(new { amount = 1000, currency = "usd" }, true)))
        .RespondWith(Response.Create().WithStatusCode(HttpStatusCode.Created)
            .WithHeader("Content-Type", "application/json")
            .WithBodyAsJson(new { id = "txn_123", status = "succeeded" }));
    ```
* **Testing:** Make API calls through your `ApiClient` that trigger interactions with the configured external service. Assert your application's behavior based on the stubbed responses from WireMock.
* **Verification (Optional):**
    ```csharp
    var requests = server.FindLogEntries(
        Request.Create().WithPath("/v1/external-payments").UsingPost()
    );
    requests.Should().HaveCount(1).Because("the payment service should have been called once");
    ```

### 6.3. Simulating Authentication and Authorization

Uses `TestAuthHandler` and `AuthTestHelper`.

* **Mechanism:** `TestAuthHandler` is registered by `CustomWebApplicationFactory` and creates a `ClaimsPrincipal` based on specific HTTP headers or pre-configured test user details.
* **Usage (via `AuthTestHelper` available in base classes):**
    ```csharp
    // Get a client authenticated as a specific user with roles
    var authenticatedClient = await AuthHelper.CreateAuthenticatedApiClientAsync(
        userId: "test-user-id",
        roles: new[] { Roles.Admin, Roles.User } // From your Roles class
    );
    var response = await authenticatedClient.GetAdminResourceAsync();
    response.StatusCode.Should().Be(HttpStatusCode.OK);

    // Get a client authenticated with default test user (from appsettings.Testing.json)
    var defaultUserClient = await AuthHelper.CreateAuthenticatedApiClientAsync();
    var userResponse = await defaultUserClient.GetUserProfileAsync();

    // For unauthenticated requests, use the standard ApiClient
    var publicResponse = await ApiClient.GetPublicInfoAsync();
    ```
* **Testing Various Scenarios:** Easily test access with different user roles, specific claims, missing authentication, or invalid authentication by configuring the `AuthTestHelper` or by directly manipulating headers if `TestAuthHandler` supports it.

### 6.4. Contract Testing with PactNet (Future Consideration)

As per TDD FRMK-005, PactNet may be introduced to ensure that virtualized service contracts (used by WireMock.Net) do not drift from the actual provider contracts. This will be documented further if implemented.

## 7. Test Data Management for Integration Tests

Integration tests often require more complex and realistic data setups than unit tests.

* **AutoFixture (TDD FRMK-002):**
    * Use AutoFixture (via `_fixture` from base classes) to generate DTOs for API requests and to help create entities for database seeding.
        ```csharp
        var createRequest = _fixture.Build<CreateRecipeDto>()
            .With(r => r.Title, "Specific Test Title")
            .Create();
        ```
    * **Advanced Customizations:** For complex domain models (especially EF Core entities), project-specific `ICustomization` and `ISpecimenBuilder` implementations will be developed (see TDD FRMK-002). These will reside in `api-server.Tests/Framework/TestData/AutoFixtureCustomizations/` and help create valid, complex object graphs.
    * Ensure `OmitOnRecursionBehavior` is active for EF Core entities to prevent issues with circular navigation properties.
* **Test Data Builders (`TestData/Builders/`):**
    * For scenarios requiring highly specific, repeatable, or complex data states that are cumbersome to create with AutoFixture alone.
    * Builders can internally use AutoFixture to populate non-critical properties.
    * Example: `new RecipeBuilder(_fixture).WithCuisine("Italian").WithChef("Mario").Build();`
* **Data Isolation Strategies:**
    * **Primary:** `await DbFixture.ResetDatabaseAsync();` before each test method that mutates data or requires a known clean state.
    * **Unique Data:** Generate unique identifiers (e.g., `_fixture.Create<string>()` for names/keys) within tests to prevent conflicts if full reset is not used or if testing concurrency.
    * **Cleanup:** Ensure any resources created outside the database (e.g., files, if applicable) are cleaned up by the test.

## 8. Writing Assertions for Integration Tests

Use FluentAssertions for all assertions. Include `.Because("...")` for clarity.

* **Asserting HTTP Response Details:**
    * **Status Codes:** `response.StatusCode.Should().Be(HttpStatusCode.Created).Because("a new resource was expected");`
    * **Headers:** `response.Headers.Location.Should().NotBeNull().And.BeValidAbsoluteUri().Because("a location header pointing to the new resource is expected");`
    * **Content/Body:**
        * Deserialize JSON content: `var resultDto = await response.Content.ReadFromJsonAsync<MyResourceDto>();`
        * `resultDto.Should().NotBeNull().Because("a valid resource DTO was expected");`
        * `resultDto.Should().BeEquivalentTo(expectedDto, options => options.Excluding(d => d.CreatedAt)).Because("the returned DTO should match the input, except for server-generated fields");`
        * For collections: `resultList.Should().HaveCount(3).And.Contain(item => item.Name == "TestItem").Because("...");`
        * For error responses (e.g., `ProblemDetails` or `ApiErrorResult`):
            ```csharp
            var errorResult = await response.Content.ReadFromJsonAsync<ProblemDetails>(); // Or your custom error DTO
            errorResult.Status.Should().Be((int)HttpStatusCode.BadRequest);
            errorResult.Title.Should().Be("Invalid input provided.");
            ```
* **Asserting Database State:**
    * After an API call that modifies data, fetch the data directly from the database using `DbFixture.GetContext()` to verify the changes.
    ```csharp
    await using var assertContext = DbFixture.GetContext();
    var newEntity = await assertContext.MyEntities.FindAsync(newResourceId);
    newEntity.Should().NotBeNull();
    newEntity.SomeProperty.Should().Be(expectedValue).Because("the API call should have updated this property");
    ```
* **Asserting Interactions with Virtualized Services (WireMock.Net):**
    * Verify that the `WireMock.Net` server received the expected requests from your application.
    ```csharp
    // Conceptual - assuming 'wiremockServer' is accessible
    var loggedRequests = wiremockServer.FindLogEntries(
        Request.Create().WithPath("/external-api/expected-path").UsingPost()
    );
    loggedRequests.Should().HaveCount(1).Because("the external service should have been called once");
    // Further assertions on loggedRequest.RequestMessage.BodyAsJson, etc.
    ```

## 9. Testing Specific Scenarios

* **API Endpoints:** Test all public API endpoints for:
    * Happy paths (valid inputs, expected successful outcomes).
    * Negative paths (invalid inputs, missing required fields, data validation failures leading to 4xx errors).
    * Boundary conditions.
    * Correct error responses (e.g., 401, 403, 404, 409, 500 based on simulated conditions).
* **Business Workflows:** For complex operations spanning multiple API calls or components, design tests that cover the end-to-end flow within the integration scope.
* **Authorization and Authentication:**
    * Test access to protected endpoints with valid tokens/credentials for different user roles.
    * Test access with invalid/expired tokens or missing credentials (expect 401 Unauthorized).
    * Test access by users with insufficient permissions (expect 403 Forbidden).
* **Middleware Behavior:** Test the impact of custom middleware (e.g., error handling, logging, feature flags) if their effects are observable through API responses or verifiable side effects.
* **Idempotency:** For operations that should be idempotent (e.g., some PUT or DELETE operations), verify that repeated calls with the same input yield the same outcome without undesired side effects.

## 10. Common Pitfalls in Integration Testing

* **Flaky Tests due to Uncontrolled State:**
    * **Cause:** Shared database state not being reset, race conditions, reliance on previous test outcomes.
    * **Solution:** Always use `await DbFixture.ResetDatabaseAsync()`. Design tests to be independent. Create all necessary data within the test or use unique data identifiers.
* **Slow Integration Test Suite:**
    * **Cause:** Inefficient fixture setup/teardown, testing too much (unit-level logic) in integration tests, slow Testcontainer startup (mitigated by collection fixtures).
    * **Solution:** Use shared fixtures (`[Collection("Integration")]`). Optimize database seeding. Keep integration tests focused on verifying *interactions* and contracts, not exhaustive business logic variations. Ensure Testcontainers are used efficiently (image pinning, appropriate wait strategies as handled by `DatabaseFixture`).
* **Ignoring or Incorrectly Handling External Dependency Failures:**
    * **Cause:** Tests assume external services are always available or only test "happy path" responses.
    * **Solution:** Use `WireMock.Net` to simulate various states of external HTTP services (errors, timeouts, specific error payloads) and test how your application handles these scenarios.
* **`DateTime` / Timezone Precision Issues:**
    * **Cause:** Discrepancies in `DateTime` precision between .NET, databases, and DTO serialization, especially across time zones.
    * **Solution:** For SUT logic involving current time, ensure it uses an injected `System.TimeProvider` (as per TDD FRMK-001), which can be faked in tests. When asserting `DateTime` values from DB or API responses, use `Should().BeCloseTo(..., TimeSpan.FromSeconds(1))` or compare specific date/time parts if exact match isn't guaranteed or necessary. Prefer `DateTimeOffset` for unambiguous time points.
* **Overly Complex Test Setup and Teardown:**
    * **Cause:** Trying to arrange too many intricate states for a single test.
    * **Solution:** Break down complex scenarios into multiple, more focused integration tests. Use Test Data Builders and AutoFixture customizations effectively. Encapsulate common setup/teardown logic in base test classes or helper methods if truly generic.
* **Test Scope Creep (Blurring with E2E Tests):**
    * **Cause:** Integration tests attempt to cover too broad a scope, potentially including UI elements or very distant third-party systems not directly interacted with by the API.
    * **Solution:** Keep integration tests focused on the interactions between your `api-server`'s components and its directly managed/virtualized infrastructure (database, message queues, virtualized HTTP services).

## 11. Integration Test Checklist (Quick Reference)

Before committing integration tests, quickly verify:

1.  **Focus:** Does the test verify an interaction between components, an API endpoint contract, or a flow?
2.  **Framework Usage:** Does it correctly use `CustomWebApplicationFactory`, `DatabaseFixture` (if needed), `ApiClientFixture`, and inherit appropriate base classes? Is it part of the `"Integration"` collection?
3.  **API Interaction:** Are API calls made via the Refit `ApiClient`?
4.  **Database State:** If stateful, is `ResetDatabaseAsync()` called? Is data seeded appropriately and state verified correctly?
5.  **External HTTP Services:** Are external HTTP dependencies properly virtualized (e.g., with WireMock.Net stubs)? No live external calls?
6.  **Authentication/Authorization:** If testing secured endpoints, are different authentication states and roles considered using `AuthTestHelper`?
7.  **Assertions:** Are assertions clear, using FluentAssertions with `.Because("...")`, and verifying relevant aspects (status, headers, body, DB state)?
8.  **Data Management:** Is test data (DTOs, DB entities) managed effectively (AutoFixture, Builders) and isolated per test?
9.  **Conditional Execution:** Does the test use `[DependencyFact]` if it relies on specific configurations or infrastructure like Docker?
10. **Performance:** Is the test reasonably performant for an integration test? No unnecessary delays?
11. **Resilience & Determinism:** Is the test robust against minor unrelated changes and consistently produces the same outcome?
12. **Traits:** Is it marked with `[Trait("Category", "Integration")]` and all other relevant dependency/mutability traits?

---
