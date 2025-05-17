# Module/Directory: /Unit/Services/AI

**Last Updated:** 2025-05-15

> **Parent:** [`Services`](../README.md)
> *(Note: A README for `/Unit/Services/` may be needed)*
> **Related:**
> * **Service Sources:**
>   * [`Services/AI/AiService.cs`](../../../../api-server/Services/AI/AiService.cs)
>   * [`Services/AI/LlmService.cs`](../../../../api-server/Services/AI/LlmService.cs)
>   * [`Services/AI/LlmRepository.cs`](../../../../api-server/Services/AI/LlmRepository.cs)
>   * [`Services/AI/TranscribeService.cs`](../../../../api-server/Services/AI/TranscribeService.cs)
>   * [`Services/AI/PromptBase.cs`](../../../../api-server/Services/AI/PromptBase.cs)
> * **Interfaces:** [`Services/AI/ILlmRepository.cs`](../../../../api-server/Services/AI/ILlmRepository.cs) (Implicitly defined by LlmRepository)
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the services responsible for Artificial Intelligence (AI) functionalities, primarily interacting with Large Language Models (LLMs like OpenAI's GPT series) and transcription services (like OpenAI's Whisper).

* **Why Unit Tests?** To validate the internal logic of each AI service (`AiService`, `LlmService`, `LlmRepository`, `TranscribeService`) in complete isolation from external API calls, databases, file systems, or other services. This provides fast feedback on prompt handling, context management, repository interactions, SDK usage, result processing, and error handling within each service layer.
* **Isolation:** Achieved by mocking all external dependencies for each service under test. For example, `AiService` tests mock all service dependencies, `LlmService` tests mock `ILlmRepository`, and `LlmRepository` tests mock the actual OpenAI SDK client.

## 2. Scope & Key Functionality Tested (What?)

Unit tests within this directory cover the distinct responsibilities of each AI service:

* **`AiServiceTests.cs`:**
    * Orchestrating the coordination between multiple lower-level services.
    * Processing audio transcription workflows through mocked service dependencies.
    * Handling LLM completion workflows through mocked service dependencies.
    * Managing error handling and notification across different service interactions.
    * Formatting and returning standardized result objects.

* **`LlmServiceTests.cs`:**
    * Handling various `PromptBase` derived types.
    * Managing conversation context (interaction with mocked `ISessionManager`).
    * Correctly invoking methods on the mocked `ILlmRepository`.
    * Formatting results received from the repository.
    * Implementing retry logic or error handling specific to the service layer.

* **`LlmRepositoryTests.cs`:**
    * Interacting with the mocked external LLM SDK client (e.g., `OpenAIClient`).
    * Correctly constructing API requests based on input parameters (model, prompt, settings).
    * Parsing successful responses from the mocked SDK.
    * Handling exceptions or error responses from the mocked SDK.
    * Managing API key configuration access (via mocked `IConfiguration` or options).

* **`TranscribeServiceTests.cs`:**
    * Interacting with the mocked external transcription SDK client (e.g., OpenAI Whisper via `OpenAIClient`).
    * Handling input audio data (e.g., from mocked `IFormFile` or `Stream`).
    * Correctly constructing transcription API requests.
    * Processing results from the mocked SDK.
    * Interacting with mocked `IFileService` (if applicable, e.g., for temporary storage).
    * Interacting with mocked `IGitHubService` (if results are queued for processing).
    * Handling SDK exceptions or errors.

## 3. Test Environment Setup

* **Instantiation:** Services under test (`AiService`, `LlmService`, `LlmRepository`, `TranscribeService`) are instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * For `AiService`: `ILlmService`, `ITranscribeService`, `IGitHubService`, `IEmailService`, `ISessionManager`, `IScopeContainer`, `ILogger`.
    * For `LlmService`: `ILlmRepository`, `ISessionManager`, `ILogger`.
    * For `LlmRepository`: `OpenAIClient` (or its specific interface/abstractions), `IConfiguration` or `IOptions<OpenAISettings>`, `ILogger`.
    * For `TranscribeService`: `OpenAIClient`, `IEmailService`, `IFileService`, `IGitHubService`, `ILogger`.
* **External SDK Mocking:** Mocking external SDK clients (like `OpenAIClient`) requires understanding their interfaces or creating wrappers if necessary. Focus on mocking the specific methods used by the repository/service.

## 4. Maintenance Notes & Troubleshooting

* **Test File Structure:** Organize tests into separate files corresponding to the service being tested (`AiServiceTests.cs`, `LlmServiceTests.cs`, `LlmRepositoryTests.cs`, `TranscribeServiceTests.cs`).
* **Dependency Changes:** If dependencies of AI services change, update the mock setups in the corresponding tests.
* **SDK Client Mocking:** This can be complex. Ensure mocks accurately simulate both success and various failure modes (API errors, network issues, invalid responses) of the external SDKs. Refer to the SDK library's documentation.
* **Prompt Changes:** Modifications to `PromptBase` or specific prompt classes might necessitate updates in `LlmServiceTests` to ensure correct handling.

## 5. Test Cases & TODOs

*(High-level TODOs for each service - detailed cases belong in specific test files)*

### `AiServiceTests.cs`
* **TODO:** Test `ProcessAudioTranscriptionAsync` with valid audio file.
* **TODO:** Test `ProcessAudioTranscriptionAsync` with null/invalid audio file.
* **TODO:** Test `ProcessAudioTranscriptionAsync` error handling.
* **TODO:** Test `GetCompletionAsync` with text prompt.
* **TODO:** Test `GetCompletionAsync` with audio prompt.
* **TODO:** Test `GetCompletionAsync` with null/invalid prompt.
* **TODO:** Test `GetCompletionAsync` error handling.

### `LlmServiceTests.cs`
* **TODO:** Test `GenerateResponseAsync` handles different `PromptBase` types correctly.
* **TODO:** Test context retrieval/update via mocked `ISessionManager`.
* **TODO:** Test `ILlmRepository.GenerateResponseAsync` is called with correct parameters.
* **TODO:** Test handling of successful response from repository.
* **TODO:** Test handling of exceptions/errors from repository.
* **TODO:** Test retry logic if implemented.

### `LlmRepositoryTests.cs`
* **TODO:** Test `GenerateResponseAsync` calls the correct mocked `OpenAIClient` method (e.g., `GetChatCompletionsAsync`).
* **TODO:** Test correct mapping of parameters (model, prompt, settings) to SDK method arguments.
* **TODO:** Test parsing of successful response from mocked SDK call.
* **TODO:** Test handling of various exceptions (`RequestFailedException`, etc.) from mocked SDK call.
* **TODO:** Test handling of null/empty responses from mocked SDK call.
* **TODO:** Test retrieval/usage of API key from mocked configuration.

### `TranscribeServiceTests.cs`
* **TODO:** Test `TranscribeAudioAsync` correctly handles audio stream input.
* **TODO:** Test `ValidateAudioFile` correctly validates different audio file scenarios.
* **TODO:** Test `ProcessAudioFileAsync` correctly orchestrates the full transcription process.
* **TODO:** Test integration with `IEmailService` for error notifications.
* **TODO:** Test handling of exceptions from the transcription SDK.

## 6. Changelog

* **2025-05-15:** Updated to include AiService tests and restructured for clarity.
* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for AI service unit tests. (Gemini)

