# Module/Directory: /Integration/Controllers/AiController

**Last Updated:** 2025-04-18

> **Parent:** [`Controllers`](../README.md)
> **Related:**
> * **Source:** [`AiController.cs`](../../../../api-server/Controllers/AiController.cs)
> * **Test Infrastructure:** [`IntegrationTestBase.cs`](../../IntegrationTestBase.cs), [`CustomWebApplicationFactory.cs`](../../../Framework/Fixtures/CustomWebApplicationFactory.cs)
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Development/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

These tests verify the behavior of the `AiController` endpoints (`/api/completion`, `/api/transcribe`) **within the context of the full ASP.NET Core request pipeline**.

* **Why Integration Tests?** To ensure the controller interacts correctly with routing, model binding, authentication/authorization middleware, session management (`SessionMiddleware`), error handling (`ErrorHandlingMiddleware`), and other pipeline components.
* **Why Mock External Services?** To isolate the controller and pipeline logic from external dependencies (`OpenAI`, `GitHub`), ensuring tests are reliable, fast, and focused on the controller's integration points. This is achieved by mocking specific service interfaces.

## 2. Scope & Key Functionality Tested (What?)

These tests focus on the externally observable behavior of the `AiController` endpoints:

* **`/api/completion` Endpoint:**
    * Validates handling of `POST` requests (JSON body).
    * Confirms authentication enforcement.
    * Verifies interaction signature with the mocked `ILlmService`.
    * Checks session header (`X-Session-Id`) presence and handling.
    * Validates success (200 OK) and error (4xx, 5xx) HTTP responses.
    * Confirms correct response payload structure (`AiCompletionResponse`).
* **`/api/transcribe` Endpoint:**
    * Validates handling of `POST` requests (`multipart/form-data`).
    * Confirms authentication enforcement.
    * Verifies interaction signatures with mocked `ITranscribeService` and `IGitHubService`.
    * Checks session header (`X-Session-Id`) presence and handling.
    * Validates success (200 OK) and error (4xx, 5xx) HTTP responses.

## 3. Test Environment Setup

* **Test Server:** Provided by `CustomWebApplicationFactory<Program>`, bootstrapping the application stack in-memory. Refer to `TestingStandards.md` for the general approach.
* **Authentication:** Simulated using `TestAuthHandler`. Client instances representing specific users/roles are created via helpers in `AuthTestHelper`. See `TestingStandards.md` for details.
* **Mocked Dependencies:** Key external service interactions are replaced with mocks configured within `CustomWebApplicationFactory.ConfigureWebHost`. The crucial mocked interfaces for *these* tests are:
    * `Zarichnyi.ApiServer.Services.AI.ILlmService`
    * `Zarichnyi.ApiServer.Services.AI.ITranscribeService`
    * `Zarichnyi.ApiServer.Services.GitHub.IGitHubService`
    * *(Note: `ISessionManager` might also be mocked or use a test-specific implementation depending on factory setup).*

## 4. Maintenance Notes & Troubleshooting

* **Adding Dependencies:** If `AiController` gains new service dependencies, ensure they are appropriately mocked within `CustomWebApplicationFactory` to maintain isolation for these integration tests.
* **Mock Configuration:** Details of how services are mocked (e.g., default return values, specific setups per test) can be found in `CustomWebApplicationFactory.cs`. Tests requiring specific mock behavior might need to access and configure the mock instances exposed by the factory *before* making the HTTP request.
* **Authentication Issues (401):** Verify the test client is created correctly using `AuthTestHelper` and that the simulated user has the necessary claims/roles if specific authorization policies are applied to the controller actions.
* **Bad Requests (400):** For `/api/transcribe`, pay close attention to the construction of `MultipartFormDataContent`, ensuring the correct form field name (`audioFile`) is used. For `/api/completion`, check JSON serialization.
* **Server Errors (500):** Usually indicates an issue with mock setup (e.g., a mocked method wasn't configured to handle the specific input) or an unhandled exception within the controller/middleware during the test execution. Review the mock setup in the factory and the specific test logic.

## 5. Test Cases & TODOs

* **TODO:** Test `/api/completion` POST with valid text prompt, authenticated user, verify 200 OK and response structure.
* **TODO:** Test `/api/completion` POST with valid audio prompt (`audioPrompt` form field), authenticated user, verify 200 OK.
* **TODO:** Test `/api/completion` POST with *both* text and audio prompts (define expected behavior - prioritize one or return 400?).
* **TODO:** Test `/api/completion` POST with missing prompt (should return 400 Bad Request).
* **TODO:** Test `/api/completion` POST unauthenticated (should return 401 Unauthorized).
* **TODO:** Test `/api/completion` POST when `ILlmService` mock throws an exception (should return 500 Internal Server Error via `ErrorHandlingMiddleware`).
* **TODO:** Test `/api/completion` POST verifies `X-Session-Id` header is present in the response.
* **TODO:** Test `/api/transcribe` POST with valid audio file (`audioFile` form field), authenticated user, verify 200 OK.
* **TODO:** Test `/api/transcribe` POST with missing audio file (should return 400).
* **TODO:** Test `/api/transcribe` POST with empty audio file (should return 400 or specific error).
* **TODO:** Test `/api/transcribe` POST with incorrect form field name (e.g., `file` instead of `audioFile`) (should return 400).
* **TODO:** Test `/api/transcribe` POST unauthenticated (should return 401).
* **TODO:** Test `/api/transcribe` POST when `ITranscribeService` mock throws exception (should return 500).
* **TODO:** Test `/api/transcribe` POST when `IGitHubService` mock throws exception (should return 500).
* **TODO:** Test `/api/transcribe` POST verifies `X-Session-Id` header is present in the response.
* **TODO:** Consider tests for different audio file types/sizes if relevant validation exists.
* **TODO:** Add tests verifying specific claims/roles are required if authorization policies are applied.

## 6. Changelog

* **2025-04-18:** Revision 2 - Refocused content on why/what, aligned with standards, added Maintenance Notes, streamlined setup/dependency info. (Gemini)
* **2025-04-18:** Revision 1 - Initial creation and population of sections. (Gemini)

