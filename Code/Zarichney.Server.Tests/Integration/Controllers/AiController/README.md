# Module/Directory: /Integration/Controllers/AiController

**Last Updated:** 2025-05-15

> **Parent:** [`Controllers`](../README.md)
> **Related:**
> * **Source:** [`AiController.cs`](../../../../Zarichney.Server/Controllers/AiController.cs)
> * **Services:** [`AiService.cs`](../../../../Zarichney.Server/Services/AI/AiService.cs), [`LlmService.cs`](../../../../Zarichney.Server/Services/AI/LlmService.cs), [`TranscribeService.cs`](../../../../Zarichney.Server/Services/AI/TranscribeService.cs)
> * **Test Infrastructure:** [`IntegrationTestBase.cs`](../../IntegrationTestBase.cs), [`CustomWebApplicationFactory.cs`](../../../Framework/Fixtures/CustomWebApplicationFactory.cs)
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Standards/DocumentationStandards.md)

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
    * `Zarichnyi.ApiServer.Services.AI.IAiService` - The primary service used by the controller for coordinating AI operations. This service orchestrates operations between other services.
    * `Zarichnyi.ApiServer.Services.AI.ILlmService` - Used by AiService for LLM completions.
    * `Zarichnyi.ApiServer.Services.AI.ITranscribeService` - Used by AiService for audio transcription and validation.
    * `Zarichnyi.ApiServer.Services.GitHub.IGitHubService` - Used by AiService for storing audio files and transcripts.
    * `Zarichnyi.ApiServer.Services.Email.IEmailService` - Used by AiService for sending error notifications.
    * *(Note: `ISessionManager` might also be mocked or use a test-specific implementation depending on factory setup).*

## 4. Maintenance Notes & Troubleshooting

* **Adding Dependencies:** If `AiController` gains new service dependencies, ensure they are appropriately mocked within `CustomWebApplicationFactory` to maintain isolation for these integration tests.
* **Mock Configuration:** Details of how services are mocked (e.g., default return values, specific setups per test) can be found in `CustomWebApplicationFactory.cs`. Tests requiring specific mock behavior might need to access and configure the mock instances exposed by the factory *before* making the HTTP request.
* **Authentication Issues (401):** Verify the test client is created correctly using `AuthTestHelper` and that the simulated user has the necessary claims/roles if specific authorization policies are applied to the controller actions.
* **Bad Requests (400):** For `/api/transcribe`, pay close attention to the construction of `MultipartFormDataContent`, ensuring the correct form field name (`audioFile`) is used. For `/api/completion`, check JSON serialization.
* **Server Errors (500):** Usually indicates an issue with mock setup (e.g., a mocked method wasn't configured to handle the specific input) or an unhandled exception within the controller/middleware during the test execution. Review the mock setup in the factory and the specific test logic.

## 5. Test Cases & Implementation Status

### Completed Test Cases:

* **✅ Authentication / Error Handling Tests:**
    * `Completion_WithMissingPrompt_ReturnsBadRequest` - Verifies correct 400 response when no prompt is provided
    * `Completion_Unauthenticated_ReturnsUnauthorized` - Verifies 401 response for unauthenticated requests
    * `Transcribe_WithMissingAudioFile_ReturnsBadRequest` - Verifies correct 400 response when no audio file is provided
    * `Transcribe_WithEmptyAudioFile_ReturnsBadRequest` - Verifies correct 400 response when an empty audio file is sent
    * `Transcribe_WithIncorrectFormFieldName_ReturnsBadRequest` - Verifies 400 response when incorrect form field is used
    * `Transcribe_Unauthenticated_ReturnsUnauthorized` - Verifies 401 response for unauthenticated requests

* **✅ Success Path Tests:**
    * `Completion_WithValidTextPrompt_Authenticated_ReturnsOkAndCompletion` - Verifies 200 OK with valid text prompt, authenticated user
    * `Completion_WithValidAudioPrompt_Authenticated_ReturnsOkAndCompletion` - Verifies 200 OK with valid audio prompt, authenticated user
    * `Completion_WithBothTextAndAudioPrompt_PrioritizesAudioAndReturnsOk` - Verifies 200 OK when both prompts are present
    * `Transcribe_WithValidAudioFile_Authenticated_ReturnsOkAndTranscription` - Verifies 200 OK with valid audio file, authenticated user
    * `Transcribe_WithValidAudioFile_ReturnsOkAndTranscription` - Another successful transcription test case
    * `Transcribe_WithValidRequest_IncludesSessionIdHeader` - Verifies X-Session-Id header is present in transcription response

* **✅ Error Handling Tests:**
    * `Completion_WhenLlmServiceThrows_ReturnsInternalServerError` - Verifies 500 response when LlmService throws
    * `Transcribe_WhenTranscribeServiceThrows_ReturnsInternalServerError` - Verifies 500 response when TranscribeService throws
    * `Transcribe_WhenGitHubServiceThrowsOnAudioCommit_ReturnsInternalServerError` - Verifies 500 response when GitHub service throws on audio commit
    * `Transcribe_WhenGitHubServiceThrowsOnTranscriptCommit_ReturnsInternalServerError` - Verifies 500 response when GitHub service throws on transcript commit
    * `Transcribe_WhenTranscribeServiceThrowsException_Returns500Error` - Verifies ErrorHandlingMiddleware returns 500
    * `Transcribe_WhenTranscribeServiceThrowsException_Returns500` - Verifies 500 response with ApiError message
    * `Transcribe_WhenGitHubServiceThrowsException_Returns500Error` - Verifies ErrorHandlingMiddleware returns 500
    * `Transcribe_WhenGitHubServiceThrowsException_Returns500` - Verifies 500 response with ApiError message

### Notes on AiService Usage

With the recent architectural refactoring, the `AiController` now delegates to the orchestration layer `IAiService` rather than directly calling individual services. This means:

1. The integration tests should mock `IAiService` rather than mocking individual service implementations
2. Test cases should verify the `AiController` correctly forwards requests to `IAiService` methods
3. Unit tests for `AiService` should cover the orchestration logic between services

### Outstanding TODOs:

* **Test Updates:**
    * **TODO:** Update tests to mock IAiService methods instead of individual services
    * **TODO:** Ensure all existing test cases verify the controller's interaction with AiService
    * **TODO:** Add tests verifying AiController's error handling when AiService throws exceptions

* **Additional Test Ideas:**
    * **TODO:** Consider tests for different audio file types/sizes if relevant validation exists
    * **TODO:** Add tests verifying specific claims/roles are required if authorization policies are applied

## 6. Changelog

* **2025-05-15:** Revision 4 - Updated documentation to reflect the architectural refactoring which moved controller logic to AiService
* **2025-04-18:** Revision 3 - Updated test implementation status, marking completed tests and outstanding TODOs
* **2025-04-18:** Revision 2 - Refocused content on why/what, aligned with standards, added Maintenance Notes, streamlined setup/dependency info. (Gemini)
* **2025-04-18:** Revision 1 - Initial creation and population of sections. (Gemini)

