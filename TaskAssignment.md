**IMPORTANT:** You are a specialized, session-isolated AI Coder. You **lack memory** of prior tasks. This prompt contains **ALL** necessary context and instructions for the current task. Adhere strictly to the specified workflow and standards. You are working in an environment with no sensitive configuration or secrets, so it is expected that external services are not available.

## 1. Context Recap

* **Overall Goal:** This task continues the refactoring of configuration handling for graceful service availability (Epic #1).
    * Previous Task 1.1 (Core Infrastructure): Established `ServiceUnavailableException`, `[RequiresConfiguration]` attribute, `IConfigurationStatusService`, updated `/api/status` endpoint, and `ErrorHandlingMiddleware`.
    * Previous Task 1.2 (Service/DI Adaptation): Adapted service DI registrations and `IConfig` classes.
* **Immediate Task Goal:** Implement a Swagger UI visualization to indicate when API endpoints may be unavailable due to missing underlying configurations. This involves creating a custom attribute for controllers/actions and a Swagger Operation Filter that leverages `IConfigurationStatusService`.
* **Session Context:** This task starts with a **fresh context**. Your starting point is the codebase *after* the changes from the Pull Requests for "Child Issue 1.1" and "Child Issue 1.2" have been merged into the `develop` branch (or the current main development branch). **You MUST review the changes in those PRs to understand the current state and the `IConfigurationStatusService` you will be using.**

## 2. Associated Task

* **GitHub Epic:** [https://github.com/Zarichney-Development/zarichney-api/issues/1](https://github.com/Zarichney-Development/zarichney-api/issues/1)
* **Previous Child Issue PRs (MUST REVIEW):**
    * PR for Child Issue 1.1 (Core Infrastructure): {Orchestrator: Please provide the PR link for Child Issue 1.1}
    * PR for Child Issue 1.2 (Service/DI Adaptation): {Orchestrator: Please provide the PR link for Child Issue 1.2}
* **Current Child Task:** This prompt addresses Child Issue 1.3 (Swagger Integration) of Epic #1.

## 3. Relevant Documentation

* **MUST REVIEW (Previous Task Context):**
    * The Pull Requests for Child Issue 1.1 and 1.2.
* **MUST READ (Local Context & Contracts):**
    * `/Services/Status/IConfigurationStatusService.cs` (and its implementation) - this is a key service you will use.
    * `/Services/Status/README.md` ([View Content](content/zarichney-api/api-server/Services/Status/README.md))
    * `/Startup/ServiceStartup.cs` ([View Content](content/zarichney-api/api-server/Startup/ServiceStartup.cs)) (for registering the filter)
    * `/Startup/README.md` ([View Content](content/zarichney-api/api-server/Startup/README.md))
    * Example controllers like `/Controllers/AiController.cs` ([View Content](content/zarichney-api/api-server/Controllers/AiController.cs)) and `/Controllers/PaymentController.cs` ([View Content](content/zarichney-api/api-server/Controllers/PaymentController.cs)) for applying the new attribute.
* **MUST CONSULT (Global Rules):**
    * Primary Code Rules: **[`/Docs/Standards/CodingStandards.md`](content/zarichney-api/Docs/Standards/CodingStandards.md)**
    * Task/Git Rules: **[`/Docs/Standards/TaskManagementStandards.md`](content/zarichney-api/Docs/Standards/TaskManagementStandards.md)**
    * README Update Rules: **[`/Docs/Standards/DocumentationStandards.md`](content/zarichney-api/Docs/Standards/DocumentationStandards.md)**
    * Testing Rules: **[`/Docs/Standards/TestingStandards.md`](content/zarichney-api/Docs/Standards/TestingStandards.md)**
* **KEY SECTIONS (Focus Areas):**
    * `IConfigurationStatusService` interface and its expected `ServiceStatusInfo` return type.
    * SwaggerGen options in `ServiceStartup.ConfigureSwagger`.

## 4. Workflow & Task (Standard)

You **MUST** execute the workflow detailed in the referenced file below. Follow the steps sequentially and precisely.

* **Active Workflow Steps File:** **[`/Docs/Development/StandardWorkflow.md`](content/zarichney-api/Docs/Development/StandardWorkflow.md)**

## 5. Specific Coding Task

1.  **Define `[RequiresFeatureEnabled]` Attribute:**
    * Create a new C# attribute class named `RequiresFeatureEnabledAttribute.cs` (e.g., in a new directory like `/Filters/Attributes/` or within `/Config/` if more appropriate).
    * This attribute should inherit from `System.Attribute`.
    * It should be applicable to `AttributeTargets.Class` and `AttributeTargets.Method`.
    * It should have a constructor that accepts one or more `string featureName` arguments (e.g., `public RequiresFeatureEnabledAttribute(params string[] featureNames)`).
    * Store these feature names in a public property (e.g., `public string[] FeatureNames { get; }`). These feature names should correspond to the keys used by `IConfigurationStatusService` (e.g., "LLM", "EmailSending", "Payments").
2.  **Implement `ServiceAvailabilityOperationFilter`:**
    * Create a new C# class `ServiceAvailabilityOperationFilter.cs` (e.g., in `/Startup/` or `/Filters/`).
    * Implement the `Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter` interface.
    * The constructor should inject `IConfigurationStatusService`.
    * In the `Apply(OpenApiOperation operation, OperationFilterContext context)` method:
        * Retrieve any `RequiresFeatureEnabledAttribute` instances applied to the controller action method (`context.MethodInfo`) or the controller class (`context.MethodInfo.DeclaringType`).
        * If the attribute(s) are found, iterate through the `FeatureNames`.
        * For each `featureName`, call `await _statusService.GetServiceStatusAsync()` (note: `GetServiceStatusAsync` is async, Swagger filters are sync. You might need to adapt `IConfigurationStatusService` or how it's consumed here. A synchronous `IsFeatureAvailable(string featureName)` and `GetUnavailableReasons(string featureName)` on `IConfigurationStatusService` might be better for filters. Alternatively, the status could be pre-loaded at startup and cached for the filter).
            * **Self-correction for AI Coder:** Given Swagger filter limitations, the `IConfigurationStatusService` should ideally provide a synchronous way to check status, or status should be determined at startup and cached for the filter's use. For this task, let's assume `IConfigurationStatusService` can provide the necessary status information synchronously or from a cache. Adapt the implementation of `ConfigurationStatusService` if necessary to support this, or if too complex, the filter can fetch status once per Swagger generation. *Focus on getting the filter to work with the existing `IConfigurationStatusService` first.*
        * If `statusService.GetServiceStatusAsync().Result` (or equivalent synchronous check) indicates a required feature is unavailable:
            * Let `serviceStatusInfo = statusResult[featureName]`.
            * If `!serviceStatusInfo.IsAvailable`, prepend a warning symbol and message to `operation.Summary` or `operation.Description`. Example: `operation.Summary = "⚠️ (Unavailable: Missing " + string.Join(", ", serviceStatusInfo.MissingConfigurations) + ") " + operation.Summary;`
            * Ensure you handle multiple required features; if any one is unavailable, the endpoint should be marked. Concatenate reasons if multiple features are down.
3.  **Register Swagger Filter:**
    * In `/Startup/ServiceStartup.cs`, within the `ConfigureSwagger` method, add the new filter to SwaggerGen options: `c.OperationFilter<ServiceAvailabilityOperationFilter>();`.
4.  **Apply `[RequiresFeatureEnabled]` Attribute (Examples):**
    * Apply this attribute to a few diverse controller actions or entire controllers to demonstrate its usage. For example:
        * In `AiController.cs`: `[RequiresFeatureEnabled("LLM")]` or `[RequiresFeatureEnabled("OpenAI")]` on `GetCompletion`.
        * In `AiController.cs`: `[RequiresFeatureEnabled("Transcription")]` or `[RequiresFeatureEnabled("OpenAI")]` on `TranscribeAudio`.
        * In `PaymentController.cs`: `[RequiresFeatureEnabled("Payments")]` on class level or specific actions like `CreateCheckoutSession`.
        * Ensure the feature names used match the keys that `ConfigurationStatusService` will use/report on.
5.  **Update Unit/Integration Tests:**
    * Create unit tests for `ServiceAvailabilityOperationFilter`, mocking `IConfigurationStatusService` and `OperationFilterContext` to verify that `operation.Summary/Description` is modified correctly based on different availability statuses.
    * If you modified `ConfigurationStatusService` for synchronous access, add tests for that.
    * Add/modify integration tests (in `api-server.Tests`) that fetch the Swagger JSON (`/api/swagger/swagger.json`). These tests should:
        * Set up the `CustomWebApplicationFactory` to simulate missing configurations for specific features (e.g., by providing an `IConfiguration` that makes `LlmConfig:ApiKey` missing).
        * Fetch the Swagger JSON.
        * Assert that the summaries/descriptions for the attributed endpoints now contain the "⚠️ (Unavailable...)" warning.
6.  **Update Documentation:**
    * Create a `README.md` in `/Filters/Attributes/` if you created `RequiresFeatureEnabledAttribute.cs` there, or update `/Config/README.md`. Document the attribute's purpose and usage.
    * Update `/Startup/README.md` (or create `/Filters/README.md`) to document the `ServiceAvailabilityOperationFilter`.
    * Update `/Docs/Standards/CodingStandards.md` to recommend using the `[RequiresFeatureEnabled]` attribute for controllers/actions dependent on configurable features.
    * Update `/Services/Status/README.md` to mention its consumption by the Swagger filter.

## 6. Task Completion & Output

* **Expected Output:** Provide the final commit hash on your feature branch (e.g., `refactor/issue-1-swagger-viz`) and the URL of the created Pull Request.
* **Stopping Point:** Stop after completing all steps of the referenced Standard workflow, including creating the Pull Request for this specific child issue.

---