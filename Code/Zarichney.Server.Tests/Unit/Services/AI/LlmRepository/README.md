# Module/Directory: /Unit/Services/AI/LlmRepository

**Last Updated:** 2025-04-18

> **Parent:** [`AI`](../README.md)
> **Related:**
> * **Source:** [`Services/AI/LlmRepository.cs`](../../../../../Zarichney.Server/Services/AI/LlmRepository.cs)
> * **Dependencies:** `OpenAIClient` (from Azure.AI.OpenAI), `IConfiguration` / `IOptions<OpenAISettings>`
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Standards/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `LlmRepository` class. This repository is responsible for the direct interaction with the external Large Language Model provider's SDK (e.g., `Azure.AI.OpenAI`).

* **Why Unit Tests?** To validate the repository's logic for constructing requests to the LLM API, calling the correct SDK methods, parsing responses, and handling SDK-specific errors, all in isolation from the actual external API.
* **Isolation:** Achieved primarily by mocking the LLM SDK client (e.g., `OpenAIClient`) and configuration access (`IConfiguration` or `IOptions`).

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `LlmRepository` focus on:

* **`GenerateResponseAsync` Method:**
    * Retrieving necessary configuration (API key, endpoint, deployment name) from mocked options/configuration.
    * Correctly mapping input parameters (prompt text, chat history, settings) to the corresponding SDK request objects (e.g., `ChatCompletionsOptions`).
    * Invoking the correct method on the mocked SDK client (e.g., `OpenAIClient.GetChatCompletionsAsync`).
    * Successfully parsing valid responses returned by the mocked SDK call into an `LlmResult`.
    * Handling various error conditions or exceptions thrown by the mocked SDK call (e.g., `RequestFailedException` with different status codes, network errors) and translating them appropriately (e.g., into specific exceptions or error states in `LlmResult`).

## 3. Test Environment Setup

* **Instantiation:** `LlmRepository` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<OpenAIClient>` (or a mockable interface/wrapper if one exists/is created). This is the most critical mock.
    * `Mock<IOptions<OpenAISettings>>` or `Mock<IConfiguration>` to provide necessary settings like API keys, endpoint, deployment names.
    * `Mock<ILogger<LlmRepository>>`

## 4. Maintenance Notes & Troubleshooting

* **SDK Client Mocking:** Mocking the `OpenAIClient` (or similar SDKs) can be challenging as it might involve mocking complex request/response objects and specific exception types (`RequestFailedException`). Ensure mocks cover:
    * Successful responses with expected content.
    * Error responses with different HTTP status codes (4xx, 5xx).
    * Exceptions representing network issues or timeouts.
    * Refer to the `Azure.AI.OpenAI` library documentation for details on response/exception structures.
* **Configuration Changes:** Changes to `OpenAISettings` or how configuration is accessed may require test updates.
* **SDK Updates:** Updates to the underlying LLM SDK library might introduce breaking changes requiring adjustments to mock setups and assertions.

## 5. Test Cases & TODOs

* **TODO:** Test `GenerateResponseAsync` successfully retrieves settings from mocked `IOptions<OpenAISettings>`.
* **TODO:** Test `GenerateResponseAsync` correctly maps input prompt/history/settings to `ChatCompletionsOptions`.
* **TODO:** Test `GenerateResponseAsync` calls `_openAIClient.GetChatCompletionsAsync` (or similar) with the correct options object.
* **TODO:** Test `GenerateResponseAsync` successfully parses a valid `ChatCompletions` response from the mocked client into `LlmResult`.
* **TODO:** Test `GenerateResponseAsync` handles `RequestFailedException` (e.g., 400 Bad Request) from mock client -> returns appropriate error `LlmResult` or throws specific exception.
* **TODO:** Test `GenerateResponseAsync` handles `RequestFailedException` (e.g., 401 Unauthorized - bad API key) from mock client.
* **TODO:** Test `GenerateResponseAsync` handles `RequestFailedException` (e.g., 429 Too Many Requests) from mock client.
* **TODO:** Test `GenerateResponseAsync` handles `RequestFailedException` (e.g., 500 Internal Server Error) from mock client.
* **TODO:** Test `GenerateResponseAsync` handles other potential exceptions (e.g., `TaskCanceledException` for timeouts) from mock client.
* **TODO:** Test handling of null or unexpected response structures from the mocked client.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `LlmRepository` unit tests. (Gemini)

