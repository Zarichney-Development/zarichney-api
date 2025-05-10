**IMPORTANT:** You are a specialized, session-isolated AI Coder. You **lack memory** of prior tasks. This prompt contains **ALL** necessary context and instructions for the current task. Adhere strictly to the specified workflow and standards. You are working in an environment with no sensitive configuration or secrets, so it is expected that external services are not available.

## 1. Context Recap

* **Overall Goal:** This task continues the refactoring of configuration handling for graceful service availability, as outlined in Epic #1. The previous task (Child Issue 1.1) established the core infrastructure (`ServiceUnavailableException`, `[RequiresConfiguration]` attribute, `IConfigurationStatusService`, updated `/api/status` endpoint, and `ErrorHandlingMiddleware` updates).
* **Immediate Task Goal:** Adapt service DI registrations and `IConfig` classes to use the new unavailability infrastructure. This involves:
    1.  Decorating `IConfig` classes with the `[RequiresConfiguration]` attribute for their critical properties.
    2.  Modifying service factories in `/Startup/ServiceStartup.cs` (for external clients like OpenAI, Graph) to check configuration status via `IConfigurationStatusService`. If configuration is missing, these factories should register "throwing proxies" or "null object" implementations that throw `ServiceUnavailableException` when their methods are invoked.
    3.  Auditing internal services that consume `IConfig` objects, removing their old runtime `ConfigurationMissingException` checks, and ensuring they correctly handle scenarios where dependencies (now potentially throwing proxies) are unavailable.
* **Session Context:** This task starts with a **fresh context**. Your starting point is the codebase *after* the changes from the Pull Request for "Child Issue 1.1" (Core Infrastructure for configuration refactoring related to Epic #1) have been merged into the `develop` branch (or the current main development branch). **You MUST review the changes in that PR to understand the current state and the new infrastructure components you will be using.**

## 2. Associated Task

* **GitHub Epic:** [https://github.com/Zarichney-Development/zarichney-api/issues/1](https://github.com/Zarichney-Development/zarichney-api/issues/1)
* **Previous Child Issue PR (for review):** {Please provide the PR link for Child Issue 1.1 here for the AI Coder to review the current state}
* **Current Child Task:** This prompt addresses Child Issue 1.2 (Service/DI Adaptation) of Epic #1.

## 3. Relevant Documentation

* **MUST REVIEW (Previous Task Context):**
    * The Pull Request for Child Issue 1.1 (Core Infrastructure of Config Refactor).
* **MUST READ (Local Context & Contracts):**
    * `/Config/README.md` ([View Content](content/zarichney-api/api-server/Config/README.md)) - Especially `ServiceUnavailableException.cs` and `RequiresConfigurationAttribute.cs`.
    * `/Startup/ServiceStartup.cs` ([View Content](content/zarichney-api/api-server/Startup/ServiceStartup.cs)) & `/Startup/README.md` ([View Content](content/zarichney-api/api-server/Startup/README.md))
    * `/Services/Status/IConfigurationStatusService.cs` (and its implementation)
    * All `IConfig` implementing classes (e.g., `/Services/AI/LlmService.cs` for `LlmConfig`, `/Services/Email/EmailModels.cs` for `EmailConfig`, `/Services/Payment/PaymentModels.cs` for `PaymentConfig`, etc.).
    * Various service implementation files that consume `IConfig` objects.
* **MUST CONSULT (Global Rules):**
    * Primary Code Rules: **[`/Docs/Standards/CodingStandards.md`](content/zarichney-api/Docs/Standards/CodingStandards.md)** (DI, Error Handling)
    * Task/Git Rules: **[`/Docs/Standards/TaskManagementStandards.md`](content/zarichney-api/Docs/Standards/TaskManagementStandards.md)**
    * README Update Rules: **[`/Docs/Standards/DocumentationStandards.md`](content/zarichney-api/Docs/Standards/DocumentationStandards.md)**
    * Testing Rules: **[`/Docs/Standards/TestingStandards.md`](content/zarichney-api/Docs/Standards/TestingStandards.md)**
* **KEY SECTIONS (Focus Areas):**
    * `ServiceStartup.cs`: Focus on `ConfigureEmailServices`, `ConfigureOpenAiServices`, and other service registration methods.
    * The constructors and relevant methods of services that directly inject `IConfig` objects.

## 4. Workflow & Task (Standard)

You **MUST** execute the workflow detailed in the referenced file below. Follow the steps sequentially and precisely.

* **Active Workflow Steps File:** **[`/Docs/Development/StandardWorkflow.md`](content/zarichney-api/Docs/Development/StandardWorkflow.md)**

## 5. Specific Coding Task

1.  **Decorate `IConfig` Implementations:**
    * Go through all classes implementing `IConfig` (e.g., `LlmConfig`, `EmailConfig`, `GitHubConfig`, `PaymentConfig`, `TranscribeConfig`, `JwtSettings`, etc.).
    * Identify properties that are critical for the functioning of the associated service (e.g., `ApiKey`, `SecretKey`, `AccessToken`, `ConnectionString`).
    * Apply the `[RequiresConfiguration("SectionName:PropertyName")]` attribute to these properties. Example: For `LlmConfig.ApiKey`, the attribute would be `[RequiresConfiguration("LlmConfig:ApiKey")]`.
2.  **Adapt Service Factories in `/Startup/ServiceStartup.cs`:**
    * **For `ConfigureEmailServices` (GraphServiceClient):**
        * Inject `IConfigurationStatusService`.
        * Before `new GraphServiceClient(...)`, use `statusService.IsServiceAvailable("EmailSending")` (or a similar check based on the required config keys for `EmailConfig` like `AzureTenantId`, `AzureAppId`, `AzureAppSecret`, `FromEmail`).
        * If unavailable, register a proxy/mock for `GraphServiceClient` that, when its methods (e.g., `SendMail.PostAsync`) are called, throws a `ServiceUnavailableException` stating "EmailSending service is unavailable due to missing configuration: [list missing keys]".
        * Remove the old `logger.LogWarning(...)` and `return null!;` logic.
    * **For `ConfigureOpenAiServices` (OpenAIClient & AudioClient):**
        * Inject `IConfigurationStatusService`.
        * For `OpenAIClient`: Check availability based on `LlmConfig:ApiKey`. If unavailable, register a proxy/mock for `OpenAIClient` that throws `ServiceUnavailableException` (e.g., "LLM service unavailable due to missing LlmConfig:ApiKey") when methods like `GetChatClient` are called.
        * For `AudioClient`: Check availability based on `LlmConfig:ApiKey` (as it's shared). If unavailable, register a proxy/mock for `AudioClient` that throws `ServiceUnavailableException` when methods like `TranscribeAudioAsync` are called.
        * Remove the old `logger.LogWarning(...)` and `return null!;` logic.
    * *(Self-correction: No factory adaptations needed for internal services like `GitHubService`, `PaymentService`, etc. as they will be handled in step 3 if they consume IConfig directly, or will fail if their external client dependencies like OpenAIClient are unavailable).*
3.  **Audit Internal Services Consuming `IConfig`:**
    * Review services such as:
        * `GitHubService` (consumes `GitHubConfig`)
        * `StripeService` (consumes `PaymentConfig`)
        * `SessionManager` (consumes `SessionConfig`)
        * `FileService` and `FileWriteQueueService` (if they consume any specific `IConfig`)
        * `RecipeRepository`, `OrderRepository`, `CustomerRepository` (if they consume `IConfig`)
        * Other services in `/Cookbook/` or `/Services/` that directly inject `IConfig` objects.
    * For each service, remove any existing `if (string.IsNullOrEmpty(_config.SomeKey) || _config.SomeKey == PlaceholderValue) throw new ConfigurationMissingException(...)` checks.
    * The expectation is that if a critical config (now marked with `[RequiresConfiguration]`) is missing, the `IConfigurationStatusService` will report the feature/service as unavailable. If an attempt is made to use a service whose external client (like `OpenAIClient`) was proxied to throw `ServiceUnavailableException`, that exception will propagate. For services directly using a misconfigured `IConfig` object where the config object *itself* is still injected (but its properties are bad), these services should fail gracefully if they try to use the bad property. *No, this part is incorrect. The goal is to make the *service* unavailable. If a service `MyInternalService` depends on `MyInternalConfig`, and `MyInternalConfig:CriticalKey` is missing, `MyInternalService` itself should effectively be unavailable. This might be achieved by having `IConfigurationStatusService` also track availability of internal services based on their dependent `IConfig`s, and the DI registration for `MyInternalService` would use a factory to register a throwing proxy if `IConfigurationStatusService` reports it as unavailable.*
    * **Revised approach for Internal Services:** For internal services directly injecting an `IConfig` object (e.g., `GitHubService(GitHubConfig config, ...)`), if *that specific `GitHubConfig`* has properties marked `[RequiresConfiguration]` that are invalid (checked by `IConfigurationStatusService`), then the DI registration for `IGitHubService` itself should register a throwing proxy. This would be a broader change to `ServiceStartup.ConfigureApplicationServices`. For now, focus on services whose *clients* are created via factories (OpenAI, Email). The audit for other services should note where they might throw `NullReferenceException` if a config property is used without being checked, assuming the old `ConfigurationMissingException` checks are removed. *Let's simplify: For this task, focus only on the factories in `ServiceStartup.cs`. The broader audit and DI changes for all other internal services can be a subsequent task if needed.*
4.  **Update Unit Tests:**
    * For `ServiceStartupTests.cs` (if such tests for DI exist, as per `/api-server.Tests/Unit/Startup/README.md`):
        * Add tests to verify that if `IConfigurationStatusService` reports "EmailSending" (or similar feature name) as unavailable, the `GraphServiceClient` resolves to an implementation that throws `ServiceUnavailableException` upon method invocation.
        * Add similar tests for `OpenAIClient` and `AudioClient` based on "LLM" and "Transcription" feature availability.
    * Update unit tests for any services that *previously* threw `ConfigurationMissingException`. If those services now depend on clients that might throw `ServiceUnavailableException` (due to the factory changes), ensure tests cover this new exception path.
5.  **Update Documentation:**
    * In `/Startup/ServiceStartup.cs`, add comments explaining the new DI registration logic for services with external client dependencies (checking config status and registering throwing proxies).
    * Update `/Startup/README.md` to reflect this new DI strategy.
    * Update READMEs for any `IConfig` class that now has `[RequiresConfiguration]` attributes, documenting these requirements (e.g., in relevant files within `/Services/AI/`, `/Services/Email/`, `/Services/Payment/`, `/Services/GitHub/`, `/Cookbook/Recipes/`, etc.).

## 6. Task Completion & Output

* **Expected Output:** Provide the final commit hash on your feature branch (e.g., `refactor/issue-1-service-di-adaptation`) and the URL of the created Pull Request.
* **Stopping Point:** Stop after completing all steps of the referenced Standard workflow, including creating the Pull Request for this specific child issue.

---