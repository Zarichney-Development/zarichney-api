# Module/Directory: /Unit/Controllers/AiController

**Last Updated:** 2025-04-18

> **Parent:** [`Controllers`](../README.md)
> **Related:**
> * **Source:** [`AiController.cs`](../../../../api-server/Controllers/AiController.cs)
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

These tests verify the **internal logic of the `AiController` methods in isolation** from the ASP.NET Core pipeline, middleware, and external dependencies.

* **Why Unit Tests?** To provide fast, focused feedback on the controller's decision-making process. They ensure the controller correctly interprets input, calls its direct dependencies with the expected arguments, and translates dependency responses (or exceptions) into appropriate `ActionResult` objects.
* **Why Mock Dependencies?** To isolate the `AiController`'s logic completely. All injected services (`ILlmService`, `ITranscribeService`, `IGitHubService`, `ISessionManager`, `ILogger`, etc.) are replaced with mocks (typically using Moq) to control their behavior and verify interactions.

## 2. Scope & Key Functionality Tested (What?)

These tests focus on the internal code paths and logic *within* the `AiController` action methods:

* **`Completion` Action:**
    * Validation of `AiCompletionRequest` input (e.g., handling null/empty prompts).
    * Correct conditional logic for text vs. audio prompts.
    * Accurate invocation of `ILlmService.GenerateResponseAsync` with correctly mapped arguments.
    * Proper handling of results from `ILlmService` (mapping success to `OkObjectResult` with `AiCompletionResponse`).
    * Correct handling of exceptions thrown by `ILlmService` (should typically re-throw or be handled by middleware in integration, but unit tests verify the immediate call).
    * Interaction with `ISessionManager`.
* **`Transcribe` Action:**
    * Validation of `IFormFile` input (`audioFile`).
    * Correct invocation of `ITranscribeService.QueueTranscriptionJobAsync` and `IGitHubService.QueueFileForProcessingAsync` with appropriate arguments derived from the request.
    * Proper handling of results from services (mapping success to `OkResult`).
    * Correct handling of exceptions thrown by `ITranscribeService` or `IGitHubService`.
    * Interaction with `ISessionManager`.

## 3. Test Environment Setup

* **Instantiation:** The `AiController` class is instantiated directly within the test methods.
* **Mocking:** All constructor dependencies (`ILlmService`, `ITranscribeService`, `IGitHubService`, `ISessionManager`, `ILogger<AiController>`) **must** be provided as mocks (e.g., `Mock<IDependency>`). Mocks are configured using frameworks like Moq to define expected behavior (return values, exceptions) for specific method calls. Refer to `TestingStandards.md` for standard unit testing practices.
* **HTTP Context:** If controller actions rely on `HttpContext` details (e.g., `User`, `Request`), a mock `HttpContext` might need to be created and assigned to the controller's `ControllerContext`.

## 4. Maintenance Notes & Troubleshooting

* **Adding Dependencies:** If new dependencies are injected into `AiController`, corresponding mocks **must** be added to the test setup for existing and new unit tests.
* **Mock Setup:** Ensure mocks are configured correctly for the specific code path being tested. Use `It.IsAny<T>()` judiciously; prefer matching specific arguments where possible using `It.Is<T>()`. Verify mock interactions (`Mock.Verify(...)`) to confirm methods were called as expected.
* **Assertion Failures:** Check the type and value of the `ActionResult` returned by the controller method. Ensure the mock setup leads to the expected outcome.
* **NullReferenceExceptions:** Usually indicates a dependency was not mocked or the mock was not passed correctly to the `AiController` constructor.

## 5. Test Cases & TODOs

* **TODO (`Completion`):** Test with valid text prompt, mock `ILlmService` success -> verify `OkObjectResult` with correct data.
* **TODO (`Completion`):** Test with valid audio prompt, mock `ILlmService` success -> verify `OkObjectResult`.
* **TODO (`Completion`):** Test with *both* text and audio prompts (verify which path is taken, likely text priority).
* **TODO (`Completion`):** Test with null/empty `AiCompletionRequest` or prompt -> verify `BadRequestResult` (or appropriate error response).
* **TODO (`Completion`):** Test when `ILlmService` mock throws exception -> verify exception propagates (or is handled if try/catch exists *within* the action).
* **TODO (`Completion`):** Test `ISessionManager` interaction verification.
* **TODO (`Transcribe`):** Test with valid `IFormFile`, mock service successes -> verify `OkResult`.
* **TODO (`Transcribe`):** Test with null `IFormFile` (`audioFile`) -> verify `BadRequestResult`.
* **TODO (`Transcribe`):** Test when `ITranscribeService` mock throws exception -> verify exception propagates.
* **TODO (`Transcribe`):** Test when `IGitHubService` mock throws exception -> verify exception propagates.
* **TODO (`Transcribe`):** Test `ISessionManager` interaction verification.
* **TODO:** Add tests verifying logging calls (`ILogger`) for key events or errors.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `AiController` unit tests. (Gemini)

