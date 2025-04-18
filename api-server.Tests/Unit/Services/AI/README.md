# Module/Directory: /Unit/Services/AI

**Last Updated:** 2025-04-18

> **Parent:** [`Services`](../README.md)
> *(Note: A README for `/Unit/Services/` may be needed)*
> **Related:**
> * **Service Sources:**
>   * [`Services/AI/LlmService.cs`](../../../../api-server/Services/AI/LlmService.cs)
>   * [`Services/AI/LlmRepository.cs`](../../../../api-server/Services/AI/LlmRepository.cs)
>   * [`Services/AI/TranscribeService.cs`](../../../../api-server/Services/AI/TranscribeService.cs)
>   * [`Services/AI/PromptBase.cs`](../../../../api-server/Services/AI/PromptBase.cs)
> * **Interfaces:** [`Services/AI/ILlmRepository.cs`](../../../../api-server/Services/AI/ILlmRepository.cs) (Implicitly defined by LlmRepository)
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Development/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the services responsible for Artificial Intelligence (AI) functionalities, primarily interacting with Large Language Models (LLMs like OpenAI's GPT series) and transcription services (like OpenAI's Whisper).

* **Why Unit Tests?** To validate the internal logic of each AI service (`LlmService`, `LlmRepository`, `TranscribeService`) in complete isolation from external API calls, databases, file systems, or other services. This provides fast feedback on prompt handling, context management, repository interactions, SDK usage, result processing, and error handling within each service layer.
* **Isolation:** Achieved by mocking all external dependencies for each service under test. For example, `LlmService` tests mock `ILlmRepository`, while `LlmRepository` tests mock the actual OpenAI SDK client.

## 2. Scope & Key Functionality Tested (What?)

Unit tests within this directory cover the distinct responsibilities# Module/Directory: /Unit/Services/AI/TranscribeService

**Last Updated:** 2025-04-18

> **Parent:** [`AI`](../README.md)
> **Related:**
> * **Source:** [`Services/AI/TranscribeService.cs`](../../../../../api-server/Services/AI/TranscribeService.cs)
> * **Dependencies:** `OpenAIClient` (from Azure.AI.OpenAI), `IFileService`, `IGitHubService`, `IConfiguration` / `IOptions<OpenAISettings>`
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Development/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `TranscribeService` class. This service is responsible for handling audio transcription requests, likely interacting with an external provider like OpenAI's Whisper model via its SDK.

* **Why Unit Tests?** To validate the service's logic for handling audio input, interacting with the transcription SDK (mocked), processing results, and potentially interacting with other services like `IFileService` or `IGitHubService` (also mocked), all in isolation from actual external APIs or file systems.
* **Isolation:** Achieved by mocking the transcription SDK client (e.g., `OpenAIClient`), `IFileService`, `IGitHubService`, configuration access, and loggers.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `TranscribeService` focus on:

* **`QueueTranscriptionJobAsync` (or similar method):**
  * Handling input audio data (e.g., reading from a mocked `IFormFile` or `Stream`).
  * Interaction with mocked `IFileService` if temporary file storage/reading is involved.
  * Retrieving necessary configuration (API key, endpoint, model name) from mocked options/configuration.
  * Correctly constructing requests for the transcription SDK (e.g., `AudioTranscriptionOptions`).
  * Invoking the correct method on the mocked SDK client (e.g., `OpenAIClient.GetAudioTranscriptionAsync`).
  * Handling successful transcription results from the mocked SDK.
  * Interacting with the mocked `IGitHubService` to queue the transcription result for further processing.
  * Handling exceptions or error responses from the mocked SDK or other mocked services.

## 3. Test Environment Setup

* **Instantiation:** `TranscribeService` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
  * `Mock<OpenAIClient>` (or its specific interface/abstractions).
  * `Mock<IFileService>` (if used).
  * `Mock<IGitHubService>` (if used for queuing).
  * `Mock<IOptions<OpenAISettings>>` or `Mock<IConfiguration>`.
  * `Mock<ILogger<TranscribeService>>`.
  * Mock `IFormFile` or provide `MemoryStream` for audio data input.

## 4. Maintenance Notes & Troubleshooting

* **SDK Client Mocking:** Similar to `LlmRepository`, mocking the `OpenAIClient` for transcription requires simulating success, various API errors (`RequestFailedException`), and potentially network issues. Mock the specific transcription methods used (e.g., `GetAudioTranscriptionAsync`).
* **File Handling Mocks:** If `IFileService` is used, ensure its methods (e.g., `WriteToFileAsync`, `ReadFileStreamAsync`) are mocked appropriately for different scenarios (success, file not found).
* **Queuing Mocks:** Ensure `IGitHubService` mocks correctly simulate successful queuing and potential queuing failures.
* **Input Simulation:** Use `MemoryStream` or mock `IFormFile` effectively to simulate incoming audio data for tests.

## 5. Test Cases & TODOs

* **TODO:** Test `QueueTranscriptionJobAsync` successfully retrieves settings from mocked configuration/options.
* **TODO:** Test `QueueTranscriptionJobAsync` correctly handles input `IFormFile`/`Stream` (e.g., reads stream content).
* **TODO:** Test `QueueTranscriptionJobAsync` interacts correctly with mocked `IFileService` if applicable.
* **TODO:** Test `QueueTranscriptionJobAsync` correctly maps parameters (audio data, model, language) to `AudioTranscriptionOptions`.
* **TODO:** Test `QueueTranscriptionJobAsync` calls the correct mocked `OpenAIClient` method (e.g., `GetAudioTranscriptionAsync`).
* **TODO:** Test `QueueTranscriptionJobAsync` handles successful transcription result from mocked SDK.
* **TODO:** Test `QueueTranscriptionJobAsync` calls mocked `IGitHubService.QueueFileForProcessingAsync` with correct transcription result.
* **TODO:** Test `QueueTranscriptionJobAsync` handles `RequestFailedException` from mocked `OpenAIClient`.
* **TODO:** Test `QueueTranscriptionJobAsync` handles exceptions from mocked `IFileService`.
* **TODO:** Test `QueueTranscriptionJobAsync` handles exceptions from mocked `IGitHubService`.
* **TODO:** Test handling of invalid input (e.g., null stream, unsupported format if checked).

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `TranscribeService` unit tests. (Gemini)

of each AI service:

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

* **Instantiation:** Services under test (`LlmService`, `LlmRepository`, `TranscribeService`) are instantiated directly.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * For `LlmService`: `ILlmRepository`, `ISessionManager`, `ILogger`.
    * For `LlmRepository`: `OpenAIClient` (or its specific interface/abstractions), `IConfiguration` or `IOptions<OpenAISettings>`, `ILogger`.
    * For `TranscribeService`: `OpenAIClient`, `IFileService`, `IGitHubService`, `ILogger`.
* **External SDK Mocking:** Mocking external SDK clients (like `OpenAIClient`) requires understanding their interfaces or creating wrappers if necessary. Focus on mocking the specific methods used by the repository/service.

## 4. Maintenance Notes & Troubleshooting

* **Test File Structure:** Organize tests into separate files corresponding to the service being tested (`LlmServiceTests.cs`, `LlmRepositoryTests.cs`, `TranscribeServiceTests.cs`).
* **Dependency Changes:** If dependencies of AI services change, update the mock setups in the corresponding tests.
* **SDK Client Mocking:** This can be complex. Ensure mocks accurately simulate both success and various failure modes (API errors, network issues, invalid responses) of the external SDKs. Refer to the SDK library's documentation.
* **Prompt Changes:** Modifications to `PromptBase` or specific prompt classes might necessitate updates in `LlmServiceTests` to ensure correct handling.

## 5. Test Cases & TODOs

*(High-level TODOs for each service - detailed cases belong in specific test files)*

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
* **TODO:** Test `QueueTranscriptionJobAsync` (or similar method).
* **TODO:** Test handling of input audio stream/file (mock `IFormFile`).
* **TODO:** Test interaction with mocked `IFileService` if temporary storage is used.
* **TODO:** Test calls to the correct mocked `OpenAIClient` method (e.g., `GetAudioTranscriptionAsync`).
* **TODO:** Test correct mapping of parameters (audio data, model, language) to SDK method arguments.
* **TODO:** Test handling of successful transcription result from mocked SDK.
* **TODO:** Test interaction with mocked `IGitHubService` for queuing results.
* **TODO:** Test handling of exceptions from mocked SDK or `IGitHubService`.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for AI service unit tests. (Gemini)

