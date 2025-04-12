# Module/Directory: Server/Services/AI

**Last Updated:** 2025-04-03

> **Parent:** [`Server/Services`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This module provides centralized services for interacting with external Artificial Intelligence (AI) platforms, primarily OpenAI.
* **Key Responsibilities:**
    * Providing an interface (`ILlmService`) for interacting with OpenAI's chat completion models (GPT-3.5, GPT-4, etc.), including support for standard completions and structured output via function calling. [cite: zarichney-api/Server/Services/AI/LlmService.cs]
    * Implementing support for OpenAI's Assistants API workflow (Assistants, Threads, Runs, Tool Calls). [cite: zarichney-api/Server/Services/AI/LlmService.cs]
    * Providing an interface (`ITranscribeService`) for audio transcription using OpenAI's Whisper models. [cite: zarichney-api/Server/Services/AI/TranscribeService.cs]
    * Defining configuration models (`LlmConfig`, `TranscribeConfig`) for AI service settings. [cite: zarichney-api/Server/Services/AI/LlmService.cs, zarichney-api/Server/Services/AI/TranscribeService.cs]
    * Providing an interface (`ILlmRepository`) and implementation (`LlmRepository`) for persisting LLM conversation logs (currently using GitHub). [cite: zarichney-api/Server/Services/AI/LlmRepository.cs]
    * Defining data structures for conversation logging (`LlmConversation`, `LlmMessage`). [cite: zarichney-api/Server/Services/AI/LlmRepository.cs]
    * Defining the base class for prompt definitions (`PromptBase`) used throughout the application. [cite: zarichney-api/Server/Services/AI/PromptBase.cs]
* **Why it exists:** To abstract the complexities of interacting with specific AI APIs, provide a consistent interface for other application modules, and centralize AI-related configuration and utilities.

## 2. Architecture & Key Concepts

* **Core Services:**
    * `LlmService`: Implements `ILlmService` using the official `OpenAI` .NET library (`OpenAIClient`). Handles chat completions, function calling (leveraging `PromptBase` definitions), and the stateful Assistants API flow. Incorporates Polly for retry logic. [cite: zarichney-api/Server/Services/AI/LlmService.cs]
    * `TranscribeService`: Implements `ITranscribeService` using the `OpenAI` library's `AudioClient`. Handles audio stream processing and calls to the Whisper API. Uses Polly for retries. [cite: zarichney-api/Server/Services/AI/TranscribeService.cs]
    * `LlmRepository`: Implements `ILlmRepository` for saving conversation history. Currently delegates persistence to `IGitHubService`. [cite: zarichney-api/Server/Services/AI/LlmRepository.cs, zarichney-api/Server/Services/GitHub/GitHubService.cs]
* **Prompt Base Class:** `PromptBase` defines a standard structure for creating prompt definitions used with the `LlmService`. It standardizes properties like system instructions, model name, and function definitions. [cite: zarichney-api/Server/Services/AI/PromptBase.cs]
* **Configuration:** Uses `LlmConfig` and `TranscribeConfig` classes (registered via `IConfig`) injected via DI to configure API keys, model names, and retry attempts. [cite: zarichney-api/Server/Services/AI/LlmService.cs, zarichney-api/Server/Services/AI/TranscribeService.cs]
* **Integration with Prompts:** `LlmService` consumes `PromptBase` implementations (defined elsewhere, e.g., [`Server/Cookbook/Prompts`](../../Cookbook/Prompts/README.md)) to get system instructions and function definitions for specific AI tasks. [cite: zarichney-api/Server/Services/AI/LlmService.cs, zarichney-api/Server/Services/AI/PromptBase.cs]
* **Session Management:** `LlmService` interacts with `ISessionManager` to initialize and retrieve conversation context (`LlmConversation`) stored within user sessions. `LlmRepository` requires the `Session` object to determine the storage path for logs. [cite: zarichney-api/Server/Services/AI/LlmService.cs, zarichney-api/Server/Services/AI/LlmRepository.cs, zarichney-api/Server/Services/Sessions/SessionManager.cs]

## 3. Interface Contract & Assumptions

* **Key Public Interfaces:** `ILlmService`, `ITranscribeService`, `ILlmRepository`, `PromptBase`.
* **Assumptions:**
    * **API Keys:** Valid and correctly configured OpenAI API keys are essential (`LlmConfig.ApiKey`). [cite: zarichney-api/Server/Services/AI/LlmService.cs]
    * **Network:** Reliable network connectivity to OpenAI API endpoints is required.
    * **Model Availability:** Assumes the specified AI models (`LlmConfig.ModelName`, `TranscribeConfig.ModelName`) are available and accessible via the provided API key.
    * **Prompt Validity:** `LlmService` assumes the provided `PromptBase` objects contain valid system prompts and function schemas (when used).
    * **GitHub Service (for Logging):** `LlmRepository` assumes the `IGitHubService` is correctly configured and functional for saving conversation logs. [cite: zarichney-api/Server/Services/AI/LlmRepository.cs]
    * **Input Validity:** `TranscribeService` assumes the input `Stream` contains valid audio data in a format supported by Whisper. `LlmService` assumes input prompts are appropriate and do not violate OpenAI's content policies (though it handles `OpenAiContentFilterException`). [cite: zarichney-api/Server/Services/AI/LlmService.cs]

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration:** Requires `LlmConfig` and `TranscribeConfig` sections in application configuration. [cite: zarichney-api/appsettings.json]
* **External Dependency:** Tightly coupled to the OpenAI platform and its .NET SDK.
* **Retry Logic:** Uses Polly for handling transient errors during API calls. [cite: zarichney-api/Server/Services/AI/LlmService.cs, zarichney-api/Server/Services/AI/TranscribeService.cs]
* **Logging Persistence:** Conversation logging is currently implemented via `LlmRepository` using `IGitHubService`. Changes to logging destination would require modifying `LlmRepository`. [cite: zarichney-api/Server/Services/AI/LlmRepository.cs]
* **Prompt Definition:** New AI prompts should be defined by inheriting from `PromptBase` and implementing all abstract members. These prompt classes are typically registered with DI during application startup. [cite: zarichney-api/Server/Services/AI/PromptBase.cs, zarichney-api/Server/Startup/Configuration/ConfigurationStartup.cs]

## 5. How to Work With This Code

* **Consumption:** Inject `ILlmService` for text/function-based AI interactions or `ITranscribeService` for audio transcription.
* **AI Task Definition:** Define new AI tasks by creating implementations of `PromptBase` (typically in domain-specific modules like `Server/Cookbook/Prompts`).
* **Configuration:** Ensure `LlmConfig` (API Key, Model Name) and `TranscribeConfig` are correctly set in application configuration (user secrets, environment variables, etc.).
* **Creating a New Prompt:** 
    1. Create a new class that inherits from `PromptBase`
    2. Implement all abstract members: `Name`, `Description`, `SystemPrompt`, `Model`, and `GetFunction()`
    3. Define the function schema in `GetFunction()` if using function calling
    4. The prompt is automatically registered via DI during startup (via `AddPrompts` method)
* **Testing:** Requires mocking `OpenAIClient`, `AudioClient`, `IGitHubService` (for `LlmRepository`), and potentially `ISessionManager`. Mocking `ILlmService` itself is common when testing services that consume it. Testing actual AI interactions requires careful consideration due to cost and latency.
* **Common Pitfalls / Gotchas:** Invalid or missing API keys. OpenAI API rate limits or downtime. Errors in function definition schemas provided by `PromptBase` implementations. Incorrect state management when using the Assistants API flow. OpenAI content filtering unexpectedly blocking requests or responses. Issues with `IGitHubService` preventing conversation log persistence.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`Server/Config`](../../Config/README.md): Consumes `LlmConfig`, `TranscribeConfig`.
    * [`Server/Services/GitHub`](../GitHub/README.md): Consumed by `LlmRepository`.
    * [`Server/Services/Sessions`](../Sessions/README.md): Consumed by `LlmService` and `LlmRepository`.
    * [`Server/Startup/Configuration`](../../Startup/ConfigurationStartup.cs): For registration of prompts via DI.
* **External Library Dependencies:**
    * `OpenAI`: The official .NET SDK for interacting with OpenAI APIs.
    * `Polly`: Used for implementing retry logic.
    * `System.Text.Json`: Used for handling JSON data (e.g., function arguments).
    * `AutoMapper`: Used for mapping between `FunctionDefinition` and OpenAI's `FunctionToolDefinition`.
* **Dependents (Impact of Changes):**
    * [`Server/Cookbook`](../../Cookbook/README.md): Multiple services (`RecipeService`, `OrderService`, `RecipeRepository`, `WebScraperService`) consume `ILlmService`.
    * [`Server/Controllers/AiController.cs`](../../Controllers/AiController.cs): Consumes `ILlmService` and `ITranscribeService`.
    * [`Server/Cookbook/Prompts`](../../Cookbook/Prompts/README.md): Contains implementations of `PromptBase`.
    * Changes to `ILlmService`, `ITranscribeService`, or `PromptBase` interfaces would impact all consuming modules.

## 7. Rationale & Key Historical Context

* **Centralization:** Consolidates all direct AI API interactions into dedicated services, abstracting the specifics from the rest of the application.
* **Abstraction (`ILlmService`, `ITranscribeService`):** Provides stable interfaces, allowing consuming code to be less dependent on the specific AI provider or SDK details.
* **Logging Decoupling (`LlmRepository`):** Separates the concern of *how* conversations are logged from the core AI interaction logic in `LlmService`. Currently uses GitHub, but could be swapped to a different persistence mechanism by changing only `LlmRepository`.
* **PromptBase Relocation:** `PromptBase` was moved from `Server/Config` to this module where it more logically belongs with other AI-related functionality.

## 8. Known Issues & TODOs

* The Assistants API implementation in `LlmService` might require further refinement for robust state handling across multiple requests or complex tool interactions.
* Error handling for `OpenAiContentFilterException` could be made more sophisticated in consuming services. [cite: zarichney-api/Server/Services/AI/LlmService.cs]
* The `LlmRepository`'s dependency on `IGitHubService` makes conversation logging dependent on GitHub availability and configuration. Consider alternative logging destinations (database, dedicated logging service) for better robustness or scalability.
* Lack of support for streaming responses from the LLM, which could improve perceived latency for longer completions.
* Could be extended to support other AI models or providers if needed in the future.