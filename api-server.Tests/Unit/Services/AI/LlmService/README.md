# Module/Directory: /Unit/Services/AI/LlmService

**Last Updated:** 2025-04-18

> **Parent:** [`AI`](../README.md)
> **Related:**
> * **Source:** [`Services/AI/LlmService.cs`](../../../../../api-server/Services/AI/LlmService.cs)
> * **Dependencies:** [`ILlmRepository`](../../../../../api-server/Services/AI/LlmRepository.cs), [`ISessionManager`](../../../../../api-server/Services/Sessions/SessionManager.cs), [`PromptBase`](../../../../../api-server/Services/AI/PromptBase.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Zarichney.Standards/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Zarichney.Standards/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `LlmService` class. This service acts as a higher-level abstraction for interacting with the language model via `ILlmRepository`, handling prompt execution, managing conversation context, and potentially adding service-level logic like retries or result formatting.

* **Why Unit Tests?** To validate the internal logic of `LlmService` in isolation from the actual `LlmRepository` implementation, session storage, or external LLM APIs. Tests ensure the service correctly processes prompts, interacts with its dependencies (`ILlmRepository`, `ISessionManager`), and handles various outcomes.
* **Isolation:** Achieved by mocking `ILlmRepository`, `ISessionManager`, and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `LlmService` focus on:

* **`GenerateResponseAsync` Method:**
    * Handling various `PromptBase` derived types and extracting necessary data.
    * Retrieving and updating conversation context using the mocked `ISessionManager`.
    * Correctly invoking `ILlmRepository.GenerateResponseAsync` with parameters derived from the prompt and context.
    * Processing and potentially formatting the `LlmResult` returned by the mocked `ILlmRepository`.
    * Handling exceptions or error states returned by the mocked `ILlmRepository`.
    * Implementing retry logic (if applicable).

## 3. Test Environment Setup

* **Instantiation:** `LlmService` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<ILlmRepository>`
    * `Mock<ISessionManager>`
    * `Mock<ILogger<LlmService>>`

## 4. Maintenance Notes & Troubleshooting

* **Dependency Mocking:** Ensure `ILlmRepository` mocks are set up to return appropriate `LlmResult` objects (simulating success, failure, specific content) or throw exceptions based on the test scenario. Verify `ISessionManager` mock interactions for context handling.
* **Prompt Changes:** Modifications to `PromptBase` or the introduction of new derived prompt classes may require adding or updating tests to ensure `LlmService` handles them correctly.
* **Error Handling Logic:** If `LlmService` implements specific error handling or retry logic beyond what `LlmRepository` provides, ensure these paths are explicitly tested.

## 5. Test Cases & TODOs

* **TODO:** Test `GenerateResponseAsync` with a simple prompt type -> verify `ISessionManager` called for context, verify `ILlmRepository.GenerateResponseAsync` called with correct parameters, verify successful `LlmResult` from repo is handled.
* **TODO:** Test `GenerateResponseAsync` with a prompt type requiring specific context handling -> verify `ISessionManager` interactions.
* **TODO:** Test `GenerateResponseAsync` when `ILlmRepository.GenerateResponseAsync` returns a failure/error state -> verify `LlmService` handles or propagates it correctly.
* **TODO:** Test `GenerateResponseAsync` when `ILlmRepository.GenerateResponseAsync` throws an exception -> verify `LlmService` handles or propagates it correctly.
* **TODO:** Test `GenerateResponseAsync` when `ISessionManager` throws an exception (if applicable).
* **TODO:** Test retry logic within `LlmService` if it exists (e.g., simulate transient failure from `ILlmRepository`).
* **TODO:** Test handling of different `PromptBase` derivatives if logic varies significantly.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `LlmService` unit tests. (Gemini)

