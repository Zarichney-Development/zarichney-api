# Module/Directory: /Unit/Services/AI/AiService

**Last Updated:** 2025-05-15

> **Parent:** [`AI`](../README.md)
> **Related:**
> * **Source:** [`Services/AI/AiService.cs`](../../../../../api-server/Services/AI/AiService.cs)
> * **Dependencies:** `ILlmService`, `ITranscribeService`, `IGitHubService`, `IEmailService`, `ISessionManager`, `IScopeContainer`
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `AiService` class, which serves as a high-level orchestration service that integrates various AI-related services. This service is responsible for coordinating the interactions between multiple lower-level services to provide simplified workflows for audio transcription and LLM completion operations.

* **Why Unit Tests?** To validate the coordination logic within `AiService` in complete isolation from external services and APIs. This focuses on testing how the service orchestrates calls to its dependencies (`ILlmService`, `ITranscribeService`, `IGitHubService`, etc.) and processes their results, without actually invoking those dependencies' real implementations.
* **Isolation:** Achieved by mocking all service dependencies using Moq, ensuring that tests verify only the behavior of the `AiService` itself.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `AiService` focus on:

* **`ProcessAudioTranscriptionAsync` method:**
  * Handling audio file input validation by delegating to `ITranscribeService.ValidateAudioFile`.
  * Coordinating the transcription workflow by calling `ITranscribeService.ProcessAudioFileAsync`.
  * Managing storage of audio and transcript files via `IGitHubService.StoreAudioAndTranscriptAsync`.
  * Error handling, including proper error notifications via `IEmailService.SendErrorNotification`.
  * Correctly assembling and returning the `AudioTranscriptionResult`.

* **`GetCompletionAsync` method:**
  * Processing different types of prompts (text or audio).
  * Handling audio transcription for audio prompts via `ITranscribeService`.
  * Retrieving LLM completions via `ILlmService.GetCompletionContent`.
  * Managing session information via `ISessionManager`.
  * Error handling with appropriate error notifications.
  * Correctly assembling and returning the `CompletionResult`.

## 3. Test Environment Setup

* **Instantiation:** `AiService` is instantiated directly in test methods.
* **Mocking:** All dependencies are mocked using Moq:
  * `Mock<ILlmService>`
  * `Mock<ITranscribeService>`
  * `Mock<IGitHubService>`
  * `Mock<IEmailService>`
  * `Mock<ISessionManager>`
  * `Mock<IScopeContainer>`
* **Input Simulation:** For tests involving audio input, use `Mock<IFormFile>` to simulate form file uploads.

## 4. Maintenance Notes & Troubleshooting

* **Dependency Changes:** If dependencies of `AiService` change, update the mock setup in tests.
* **Mock Behavior Configuration:** Pay careful attention to configuring the behavior of mocked services, particularly for cases where `AiService` relies on specific return values or behavior.
* **Error Case Coverage:** Ensure good coverage of error cases, as `AiService` contains logic for handling and reporting errors from its various dependencies.
* **File Simulation:** When testing with mock `IFormFile`, ensure content streams are properly initialized if the service attempts to read their contents.

## 5. Test Cases & TODOs

### `ProcessAudioTranscriptionAsync` Tests

* **TODO:** Test successful end-to-end processing with valid audio file:
  * Validate that `ITranscribeService.ValidateAudioFile` is called with correct parameters.
  * Verify `ITranscribeService.ProcessAudioFileAsync` is called when validation passes.
  * Ensure `IGitHubService.StoreAudioAndTranscriptAsync` is called with correct parameters.
  * Verify correct `AudioTranscriptionResult` is returned with expected values.

* **TODO:** Test validation failure handling:
  * Mock `ITranscribeService.ValidateAudioFile` to return invalid result.
  * Verify `ArgumentException` is thrown with expected message.
  * Verify `ITranscribeService.ProcessAudioFileAsync` is not called.

* **TODO:** Test null audio file handling:
  * Verify `ArgumentNullException` is thrown when passing null audio file.

* **TODO:** Test transcription error handling:
  * Mock `ITranscribeService.ProcessAudioFileAsync` to throw exception.
  * Verify exception is re-thrown.

* **TODO:** Test GitHub storage error handling:
  * Mock `IGitHubService.StoreAudioAndTranscriptAsync` to throw exception.
  * Verify `IEmailService.SendErrorNotification` is called with correct parameters.
  * Verify exception is re-thrown.

### `GetCompletionAsync` Tests

* **TODO:** Test successful text prompt processing:
  * Provide a text-only prompt.
  * Verify `ILlmService.GetCompletionContent` is called with the text prompt.
  * Verify `ISessionManager.GetSessionByScope` is called to retrieve session.
  * Verify correct `CompletionResult` is returned with expected values.

* **TODO:** Test successful audio prompt processing:
  * Provide an audio prompt (mock `IFormFile`).
  * Verify `ITranscribeService.ValidateAudioFile` and `TranscribeAudioAsync` are called.
  * Verify `ILlmService.GetCompletionContent` is called with the transcribed text.
  * Verify correct `CompletionResult` is returned with source type "audio".

* **TODO:** Test invalid prompt handling:
  * Provide neither text nor audio prompt.
  * Verify `ArgumentException` is thrown with expected message.

* **TODO:** Test invalid audio file handling:
  * Mock `ITranscribeService.ValidateAudioFile` to return invalid result.
  * Verify `ArgumentException` is thrown with expected message.

* **TODO:** Test audio transcription error handling:
  * Mock `ITranscribeService.TranscribeAudioAsync` to throw exception.
  * Verify `IEmailService.SendErrorNotification` is called with correct parameters.
  * Verify `InvalidOperationException` is thrown with expected message.

* **TODO:** Test LLM service error handling:
  * Mock `ILlmService.GetCompletionContent` to throw exception.
  * Verify `IEmailService.SendErrorNotification` is called with correct parameters.
  * Verify exception is re-thrown.

## 6. Changelog

* **2025-05-15:** Initial creation - Defined purpose, scope, setup, and TODOs for `AiService` unit tests.