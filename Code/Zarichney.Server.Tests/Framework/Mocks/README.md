# README: /Framework/Mocks Directory

**Version:** 1.1
**Last Updated:** 2025-01-20
**Parent:** `../README.md`

## 1. Purpose & Responsibility

This directory is central to the strategy of isolating the `Zarichney.Server` during integration tests, particularly from external service dependencies. Its primary responsibilities include:

* **Mock Factories (`./Factories/`):** Providing a standardized way to create and register `Moq.Mock<T>` instances for service interfaces defined in the `Zarichney.Server` project. These are primarily consumed by the `CustomWebApplicationFactory` to inject test doubles into the dependency injection container when the test server starts. This allows integration tests to control the behavior of these dependencies. Recent additions include factories for OpenAI AudioClient, authentication services, session management, Swagger documentation, and ASP.NET Identity UserManager components.
* **External HTTP Service Virtualization (Planned - WireMock.Net):** This directory (or a new subdirectory like `./Virtualization/`) will house the configurations, stubs, and potentially helper utilities for `WireMock.Net`. This tool will be used to simulate external HTTP APIs, ensuring tests are deterministic and independent of live third-party services. (Corresponds to TDD FRMK-004).

The overall goal is to enable robust and reliable integration tests by providing fine-grained control over the behavior of dependencies.

### Child Modules / Key Subdirectories:

* **`./Factories/README.md`**: Contains factories that produce `Moq.Mock<T>` instances for various service interfaces (e.g., `MockStripeServiceFactory.cs`, `MockOpenAIServiceFactory.cs`). (This README will need to be created).
* `./Virtualization/` (Planned): Will contain WireMock.Net configurations and stub definitions.

## 2. Architecture & Key Concepts

* **Mock Factories (`./Factories/`):**
    * Each factory (e.g., `MockStripeServiceFactory`) is typically responsible for creating a `Mock<IExternalService>` for a specific interface from the `Zarichney.Server`.
    * They might provide default `Setup()` configurations for common behaviors if applicable, though often the `Mock<T>` is returned without specific setups, allowing individual tests to configure behavior as needed.
    * A `BaseMockFactory.cs` may exist to provide common functionality for these factories.
    * These factories are invoked within `CustomWebApplicationFactory.ConfigureTestServices` to register the `Mock<T>.Object` as a singleton service in the test server's DI container. Integration tests can then retrieve the `Mock<T>` instance itself from the `Factory.Services` provider to customize setups or verify interactions.
    * `AudioClientMockFactory`: Creates `Mock<AudioClient>` instances for OpenAI audio transcription service testing, enabling controlled testing of audio processing workflows without actual OpenAI API calls.
    * `AuthServiceMockFactory`: Provides `Mock<IAuthService>` instances for authentication workflow testing, supporting login, registration, and token management scenarios.
    * `SessionManagerMockFactory`: Creates `Mock<ISessionManager>` instances for session state management testing, enabling controlled session lifecycle testing.
    * `SwaggerMockFactory`: Provides mocking for Swagger/OpenAPI documentation generation components, supporting API documentation testing scenarios.
    * `UserManagerMockFactory`: Creates `Mock<UserManager<ApplicationUser>>` instances for ASP.NET Identity testing, enabling controlled user management testing without database dependencies.
* **HTTP Service Virtualization with `WireMock.Net` (Planned - TDD FRMK-004):**
    * **Concept:** Instead of just mocking an internal interface that *calls* an external HTTP service, WireMock.Net runs an actual lightweight HTTP server during tests. The `HttpClient` instances used by the `Zarichney.Server` to call real external services will be reconfigured (by `CustomWebApplicationFactory`) to target this local WireMock.Net server.
    * **Stubs:** Tests (or global setup) will define "stubs" on the WireMock.Net server, specifying how it should respond to particular incoming HTTP requests (e.g., "if GET /v1/external-resource, respond with 200 OK and this JSON body").
    * **Benefits:** Allows testing of the actual HTTP client logic within the `Zarichney.Server` (retry policies, error handling for HTTP status codes, request/response serialization) against a controlled and predictable fake external API.

## 3. Interface Contract & Assumptions

* **Mock Factories:**
    * **Consumed by:** `CustomWebApplicationFactory.ConfigureTestServices`.
    * **Provided to Tests:** Integration tests can access the `Mock<T>` instances via `Factory.Services.GetRequiredService<Mock<IExternalService>>()` to perform per-test `Setup()` or `Verify()` operations.
    * **Assumption:** The interfaces being mocked are well-defined in the `Zarichney.Server` project.
* **WireMock.Net (Planned):**
    * **Interaction:** The `Zarichney.Server`'s HTTP clients (when running under test conditions) will transparently interact with the WireMock.Net server instead of live external endpoints.
    * **Test Interaction:** Tests may need to configure WireMock.Net stubs before executing API calls if dynamic stubbing is required. Helper methods or a dedicated WireMock fixture might facilitate this.
    * **Assumption:** Network configuration within `CustomWebApplicationFactory` correctly redirects traffic for specified external services to the WireMock.Net instance.

## 4. Local Conventions & Constraints

* **Naming:**
    * Mock factories: `Mock[ServiceName]Factory.cs` (e.g., `MockStripeServiceFactory.cs`).
    * WireMock.Net stub definition files/classes (when implemented): Should be organized logically, perhaps by the external service they simulate (e.g., `./Virtualization/StripeStubs.cs` or `./Virtualization/Stripe/payment_success.json`).
* **Registration:** New mock factories **must** be registered in `CustomWebApplicationFactory.ConfigureTestServices`.
* **Stateless Mocks by Default:** Mock factories should provide "clean" `Mock<T>` instances by default. Any common, non-test-specific setups should be minimal and broadly applicable. Test-specific behavior should be configured within the individual test methods.

## 5. How to Work With This Code

### Using Mock Factories in Integration Tests

1.  Ensure the relevant mock factory (e.g., `MockOpenAIServiceFactory`, `AudioClientMockFactory`, `AuthServiceMockFactory`, `SessionManagerMockFactory`, `SwaggerMockFactory`, `UserManagerMockFactory`) is registered in `CustomWebApplicationFactory.ConfigureTestServices`.
2.  In your integration test method (which has access to `Factory` from a base class like `IntegrationTestBase`):
    ```csharp
    // Arrange
    var mockLlmService = Factory.Services.GetRequiredService<Mock<ILlmService>>(); // Assuming ILlmService is mocked by MockOpenAIServiceFactory
    mockLlmService
        .Setup(s => s.GetCompletionAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync("Mocked LLM response");

    // Act: Make an API call that internally uses ILlmService
    var response = await ApiClient.InvokeAiPoweredEndpointAsync(new RequestDto { Prompt = "Test" });

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.OK);
    // ... further assertions based on the mocked response ...

    mockLlmService.Verify(s => s.GetCompletionAsync("Test", It.IsAny<CancellationToken>()), Times.Once);

    // Examples with new factories
    var mockAuthService = Factory.Services.GetRequiredService<Mock<IAuthService>>();
    mockAuthService
        .Setup(s => s.AuthenticateAsync(It.IsAny<LoginRequest>()))
        .ReturnsAsync(new AuthResult { Success = true });

    var mockAudioClient = Factory.Services.GetRequiredService<Mock<AudioClient>>();
    // Configure audio client mock behavior

    var mockSessionManager = Factory.Services.GetRequiredService<Mock<ISessionManager>>();
    mockSessionManager
        .Setup(s => s.GetSessionAsync(It.IsAny<string>()))
        .ReturnsAsync(new Session { Id = "test-session" });
    ```

### Using WireMock.Net (Conceptual - Based on TDD FRMK-004)

1.  **Setup (Likely handled by a fixture or `CustomWebApplicationFactory`):** The WireMock.Net server will be started, and `HttpClient`s used by `Zarichney.Server` for external calls will be configured to point to it.
2.  **Define Stubs (Per-test or pre-configured):**
    ```csharp
    // Example: Accessing a WireMock.Net server instance (hypothetical _wireMockServer)
    _wireMockServer
        .Given(Request.Create().WithPath("/api/external/resource/123").UsingGet())
        .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(new { id = "123", data = "mocked data" }));
    ```
3.  **Act:** Make an API call through `ApiClient` that triggers the `Zarichney.Server` to call the (now virtualized) external HTTP service.
4.  **Assert:** Verify your `Zarichney.Server`'s behavior based on the controlled response from WireMock.Net. Optionally, verify that WireMock.Net received the expected request(s).

### Adding Mocking for a New Service

* **For Internal-Like Mocking (using Moq factories):**
    1.  Create a new factory class in `./Factories/` (e.g., `MockNewServiceFactory.cs`).
    2.  Have it return `Mock<INewService>`.
    3.  Register this factory in `CustomWebApplicationFactory.ConfigureTestServices`.
* **For External HTTP Service Virtualization (using WireMock.Net - Planned):**
    1.  Identify the base URL of the external service.
    2.  Ensure `CustomWebApplicationFactory` is configured to redirect calls for this base URL to WireMock.Net.
    3.  Define necessary stubs, either globally or per-test, in the designated `./Virtualization/` area.

## 6. Dependencies

### Internal Dependencies

* **`../Fixtures/CustomWebApplicationFactory.cs`:** The primary consumer of the mock factories. It will also be responsible for integrating WireMock.Net.
* **`Zarichney.Server` project:** Provides the interfaces (e.g., `ILlmService`, `IStripeService`, `IAuthService`, `ISessionManager`) that are mocked by the factories. The HTTP client configurations within `Zarichney.Server` will be targeted by WireMock.Net redirection.
* **`../../Integration/` tests:** Consume the `Mock<T>` instances provided via `Factory.Services` or interact with APIs whose external calls are handled by WireMock.Net.
* **Mock Factories in `./Factories/`**: AudioClientMockFactory, AuthServiceMockFactory, SessionManagerMockFactory, SwaggerMockFactory, UserManagerMockFactory, and existing factories provide standardized mock creation for integration testing.

### Key External Libraries

* **`Moq`**: Used by all mock factories in `./Factories/`.
* **`WireMock.Net`** (Planned): Will be the core library for HTTP service virtualization.

## 7. Rationale & Key Historical Context

* **Mock Factories:** These were introduced to provide a clean, centralized way for `CustomWebApplicationFactory` to inject singleton `Mock<T>` instances into the test DI container. This allows tests to easily retrieve and configure these shared mocks.
* **WireMock.Net (Rationale for Planned Adoption):** While `Moq` is excellent for mocking C# interfaces, it doesn't directly test the HTTP client logic within the `Zarichney.Server` (e.g., correct URL construction, header manipulation, resilience policies like Polly, HTTP error code handling). WireMock.Net allows testing these aspects by simulating the actual HTTP interaction, leading to higher confidence for services that are heavily reliant on external HTTP APIs. This aligns with TDD FRMK-004.

## 8. Known Issues & TODOs

* **WireMock.Net Implementation (TDD FRMK-004):** This is a critical upcoming enhancement. This includes:
    * Setting up the WireMock.Net server instance (likely via a new fixture).
    * Configuring `CustomWebApplicationFactory` to redirect HTTP clients.
    * Establishing patterns for defining and managing WireMock stubs.
    * Documenting its usage thoroughly in `../../../Docs/Standards/IntegrationTestCaseDevelopment.md`.
* **Factory Coverage Expansion**: Recently completed addition of 5 new mock factories (AudioClient, AuthService, SessionManager, Swagger, UserManager) significantly improves integration testing coverage for authentication, audio processing, and session management workflows.
* **Dynamic Mock Configuration:** The current mock factories provide basic `Mock<T>` instances. More complex shared default behaviors for mocks, if needed, might require enhancements to the factories or a different strategy.
* **Contract Testing (PactNet - TDD FRMK-005):** Once WireMock.Net is in place, contract testing should be evaluated to ensure WireMock stubs stay synchronized with actual external API contracts.
* Refer to the "Framework Augmentation Roadmap (TODOs)" in `../../TechnicalDesignDocument.md` for broader framework enhancements.

---
