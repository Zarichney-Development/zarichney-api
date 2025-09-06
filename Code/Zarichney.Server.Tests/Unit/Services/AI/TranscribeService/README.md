# Module/Directory: /Unit/Services/AI/TranscribeService

**Last Updated:** 2025-04-18

> **Parent:** [`AI`](../README.md)
> **Related:**
> * **Source:** [`Services/AI/TranscribeService.cs`](../../../../../Zarichney.Server/Services/AI/TranscribeService.cs)
> * **Dependencies:** `OpenAIClient` (from Azure.AI.OpenAI), `IFileService`, `IGitHubService`, `IConfiguration` / `IOptions<OpenAISettings>`
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Standards/DocumentationStandards.md)

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

