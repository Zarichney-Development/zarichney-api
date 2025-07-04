# Module/Directory: /Config

**Last Updated:** 2025-05-11

> **Parent:** [`Server`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory provides shared configuration models, core middleware components, and utilities used across the application. Configuration registration logic and `PromptBase` have been moved to the `/Startup` and `/Services/AI` modules respectively. [cite: Zarichney.Server/Config/ConfigModels.cs, Zarichney.Server/Config/ErrorHandlingMiddleware.cs, Zarichney.Server/Config/LoggingMiddleware.cs]
* **Key Responsibilities:**
    * Defining shared configuration models (e.g., `ServerConfig`, `ClientConfig`) identified by the `IConfig` interface. [cite: Zarichney.Server/Config/ConfigModels.cs]
    * Providing core middleware for request/response logging and global error handling. [cite: Zarichney.Server/Config/LoggingMiddleware.cs, Zarichney.Server/Config/ErrorHandlingMiddleware.cs]
    * Providing custom utilities like JSON converters and exception types. [cite: Zarichney.Server/Config/JsonConverter.cs, Zarichney.Server/Config/NotExpectedException.cs, Zarichney.Server/Config/ConfigurationMissingException.cs, Zarichney.Server/Config/ServiceUnavailableException.cs]
    * Supplying attributes for configuration requirement decoration (`RequiresConfigurationAttribute`, `DependsOnServiceAttribute`). [cite: Zarichney.Server/Config/RequiresConfigurationAttribute.cs, Zarichney.Server/Config/DependsOnServiceAttribute.cs]
    * Supplying filters for external integrations like Swagger (`FormFileOperationFilter`, `ServiceAvailabilityOperationFilter`). [cite: Zarichney.Server/Config/FormFileOperationFilter.cs, Zarichney.Server/Config/ServiceAvailabilityOperationFilter.cs]
    * Hosting static configuration files like `site_selectors.json`. [cite: Zarichney.Server/Config/site_selectors.json]
* **Why it exists:** To define shared configuration models, provide reusable cross-cutting concerns via middleware, and centralize utilities needed by multiple modules.

## 2. Architecture & Key Concepts

* **Configuration Loading:** The actual loading and registration of configuration is now handled by the `/Startup/Configuration` module. This module focuses on defining the configuration models and the `IConfig` interface. [cite: Zarichney.Server/Startup/Configuration/ConfigurationStartup.cs]
* **Strongly-Typed Configuration:** Configuration is accessed via injected, strongly-typed `XConfig` objects (e.g., `LlmConfig`, `RecipeConfig`). [cite: Zarichney.Server/Cookbook/Recipes/RecipeService.cs, Zarichney.Server/Services/AI/LlmService.cs]
* **Configuration Requirements & Feature Availability:**
   * A strongly-typed `Feature` enum defines all features available in the application that can be enabled or disabled based on configuration availability (e.g., `OpenAiApi`, `MsGraph`, etc.). [cite: Zarichney.Server/Config/FeatureEnum.cs]
   * Properties in configuration classes are now marked with `[RequiresConfiguration]` attribute to indicate which features depend on them (e.g., `[RequiresConfiguration(Feature.LLM, Feature.AiServices)]`). This is used by `IConfigurationStatusService` to determine feature availability. [cite: Zarichney.Server/Config/RequiresConfigurationAttribute.cs, Zarichney.Server/Services/Status/ConfigurationStatusService.cs]
   * Controllers and actions can be marked with the `[DependsOnService]` attribute to indicate they depend on specific features that require proper configuration (e.g., `[DependsOnService(Feature.LLM)]`). The `ServiceAvailabilityOperationFilter` uses this information to update Swagger UI with visual indications when endpoints may be unavailable due to missing configuration. [cite: Zarichney.Server/Config/DependsOnServiceAttribute.cs, Zarichney.Server/Config/ServiceAvailabilityOperationFilter.cs]
* **Middleware:** Standard ASP.NET Core middleware pattern is used for logging (`LoggingMiddleware`), error handling (`ErrorHandlingMiddleware`), and feature availability checking (`FeatureAvailabilityMiddleware`). Their order of registration in the application pipeline is important. [cite: Zarichney.Server/Startup/App/ApplicationStartup.cs, Zarichney.Server/Config/LoggingMiddleware.cs, Zarichney.Server/Config/ErrorHandlingMiddleware.cs, Zarichney.Server/Services/Status/FeatureAvailabilityMiddleware.cs]
* **ErrorHandlingMiddleware:**
    * Global exception handler for all HTTP requests. Catches unhandled exceptions and returns a structured JSON error response.
    * **Now specifically catches `ConfigurationMissingException` and `ServiceUnavailableException` to return a 503 Service Unavailable response** with a user-friendly message if a required configuration is missing at runtime or a service is unavailable (see Section 5 for details).
    * Logs all exceptions using the injected logger, including request method and path for traceability.
    * For all other exceptions, returns a 500 Internal Server Error with a generic error message and trace ID.
* **FeatureAvailabilityMiddleware:**
    * Examines each incoming request to check if the target endpoint has been decorated with `[DependsOnService]` attribute(s).
    * Uses `IStatusService` to determine if all required features are currently available.
    * If any required feature is unavailable, throws a `ServiceUnavailableException` with detailed information about which configurations are missing.
    * Properly handles both controller-level and action-level attributes, aggregating all required features.
* **Static Configuration Files:** Files like `site_selectors.json` contain data loaded and used by specific services (`WebScraperService`), although the file itself resides within this config directory for organization. [cite: Zarichney.Server/Config/site_selectors.json, Zarichney.Server/Cookbook/Recipes/WebScraperService.cs]

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * `IConfig`: Marker interface for configuration classes automatically registered by the system.
    * `ConfigurationMissingException`: Exception type that services should throw when they detect missing required configuration at runtime. Has properties to identify the affected configuration section and specific key details.
    * `ServiceUnavailableException`: Exception type that indicates a service is unavailable due to missing configuration or other reasons. Contains a collection of specific reasons for unavailability.
    * `Feature`: Enum defining all features available in the application that can be enabled or disabled based on configuration availability.
    * `RequiresConfigurationAttribute`: Attribute to decorate properties in configuration classes that are required for specific features to function. Links a configuration property to one or more features that depend on it.
    * `DependsOnServiceAttribute`: Attribute to decorate controllers and actions that depend on specific features that require proper configuration. Accepts one or more `Feature` enum values. Used by `ServiceAvailabilityOperationFilter` to update Swagger UI with visual indications when endpoints may be unavailable.
    * `ServiceAvailabilityOperationFilter`: A Swagger filter that detects controllers and actions with the `[DependsOnService]` attribute and modifies their description in the Swagger UI to indicate when they may be unavailable due to missing configuration.
    * Middleware (`ErrorHandlingMiddleware`, `LoggingMiddleware`): Consumed implicitly via registration in the application pipeline. They alter the request pipeline behavior.
* **Assumptions:**
    * **Configuration Registration:** Assumes configuration is registered via the `RegisterConfigurationServices` method in `/Startup/Configuration/ConfigurationStartup.cs`.
    * **Middleware Registration:** Assumes the middleware components (`UseMiddleware<T>`) are registered in the correct order in the application pipeline to function as intended (e.g., ErrorHandling typically early, Logging often early). [cite: Zarichney.Server/Startup/App/ApplicationStartup.cs]
    * **File Existence:** Assumes `site_selectors.json` exists at the expected path relative to the application root when needed by services consuming it (like `WebScraperService`). [cite: Zarichney.Server/Cookbook/Recipes/WebScraperService.cs]

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration Class Naming:** Classes holding configuration sections must implement `IConfig` and typically end with the suffix `Config` (e.g., `RecipeConfig`). [cite: Zarichney.Server/Config/ConfigModels.cs]
* **Required Configuration:**
    * Properties in configuration classes are marked with `[RequiresConfiguration("Section:Key")]` to specify the configuration key path they depend on.
    * Services no longer need to explicitly check for null/placeholder values at runtime since service factories register proxies that throw `ServiceUnavailableException` when their methods are invoked if required configuration is missing.
    * The `IConfigurationStatusService` uses the `[RequiresConfiguration]` attribute to check the status of services based on their configuration requirements and reports their availability to service factories.
* **Middleware Order:** The order in which `LoggingMiddleware` and `ErrorHandlingMiddleware` are added in the application pipeline affects their behavior (e.g., whether errors are logged before the error handler formats the response). [cite: Zarichney.Server/Startup/App/ApplicationStartup.cs]
* **Static JSON Files:** Format and schema of `site_selectors.json` are specific to the needs of the `WebScraperService`.

## 5. How to Work With This Code

* **Adding New Configuration:**
    1.  Create a new class `MyNewConfig : IConfig` in an appropriate module (or here if truly generic). [cite: Zarichney.Server/Startup/Configuration/ConfigurationStartup.cs]
    2.  Add properties matching the desired configuration structure.
    3.  Mark properties that are required for specific features to function with `[RequiresConfiguration(Feature.FeatureName)]`. You can specify multiple features if the property is required for more than one feature.
    4.  Add a corresponding section (e.g., `"MyNewConfig": { ... }`) to `appsettings.json` or other config sources.
    5.  The `RegisterConfigurationServices` extension in `/Startup/Configuration/ConfigurationStartup.cs` will automatically handle binding, validation, and DI registration.
    6.  Inject `MyNewConfig` via constructor DI where needed.
* **Adding New Middleware:**
    1.  Create a new middleware class following the ASP.NET Core pattern (constructor with `RequestDelegate`, `InvokeAsync` method).
    2.  Consider adding an extension method for cleaner registration (e.g., `app.UseMyNewMiddleware()`).
    3.  Register the middleware using `app.UseMiddleware<MyNewMiddleware>()` or your extension method in `/Startup/App/ApplicationStartup.cs` at the appropriate stage in the request pipeline.
    4.  Be careful with ordering - some middleware (like `FeatureAvailabilityMiddleware`) must be registered after routing but before endpoint execution.
* **Error Handling for Service Unavailability:**
    * When a service can't operate due to missing configuration, throw a `ServiceUnavailableException` with one or more specific reasons for unavailability.
    * The `ErrorHandlingMiddleware` will catch the exception and return a 503 Service Unavailable response with the list of missing configurations.
    * Example: `throw new ServiceUnavailableException("Email service is unavailable", new List<string> { "Missing API key", "Missing sender email" });`
* **Configuration Status Checking:**
    * The `IConfigurationStatusService` is injected into service factories to check the status of services based on their configuration requirements.
    * Service factories use this to determine whether to register real client implementations or proxies that throw `ServiceUnavailableException` when their methods are invoked.
    * Service status can be accessed by clients via the `/api/status/config` endpoint which shows all services with their availability status and any missing configurations.
* **Marking API Endpoints That Require Specific Features:**
    * Use the `[DependsOnService]` attribute on controller classes or action methods to indicate they depend on specific features that require proper configuration.
    * The attribute accepts one or more `Feature` enum values.
    * Example at class level: `[DependsOnService(Feature.Payments)]`
    * Example at method level with multiple features: `[DependsOnService(Feature.LLM, Feature.GitHubAccess)]`
    * Two complementary systems work with this attribute:
        1. The `ServiceAvailabilityOperationFilter` automatically detects these attributes and updates the Swagger UI to show warnings when endpoints may be unavailable due to missing configuration.
        2. The `FeatureAvailabilityMiddleware` checks these attributes at runtime and prevents access to endpoints whose required features are unavailable, returning a 503 Service Unavailable response with detailed information about which configurations are missing.
* **Testing:**
    * **Configuration:** Testing services that consume config usually involves creating mock `IOptions<T>` or directly instantiating config objects with test values.
    * **Middleware:** Often requires integration tests using `WebApplicationFactory` or specialized middleware testing harnesses to mock `HttpContext` and `RequestDelegate`.
* **Common Pitfalls / Gotchas:** Missing required configuration values causing startup failure. Incorrect middleware registration order leading to unexpected behavior (e.g., errors not being logged). Invalid format in `site_selectors.json` causing scraping failures.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/Startup/ConfigurationStartup.cs`](../Startup/ConfigurationStartup.cs) - For registration of configuration.
    * [`/Startup/AppplicationStartup.cs`](../Startup/ApplicationStartup.cs) - For registration of middleware.
    * [`/Startup/ServiceStartup.cs`](../Startup/ServiceStartup.cs) - For registration of the ServiceAvailabilityOperationFilter in Swagger.
    * [`/Services/Status/IConfigurationStatusService.cs`](../Services/Status/IConfigurationStatusService.cs) - For checking service status based on configuration requirements and feature availability.
* **External Library Dependencies:**
    * `Microsoft.Extensions.Configuration.Abstractions`, `.Binder`, `.Json`: Core .NET configuration handling.
    * `Microsoft.Extensions.Options`: Used for the options pattern with configuration.
    * `Microsoft.AspNetCore.Http.Abstractions`: Used by middleware components.
    * `Serilog`: Used by logging middleware.
    * `Swashbuckle.AspNetCore.SwaggerGen`: Used by `FormFileOperationFilter`.
    * `System.Text.Json`: Used for JSON handling within utilities.
* **Dependents (Impact of Changes):**
    * [`/Startup`](../Startup/README.md): Uses the configuration models and middleware components defined here.
    * Almost all other `/*` modules: Consume configuration objects (e.g., [`/Services/Auth`](../Services/Auth/README.md), [`/Cookbook/Recipes`](../Cookbook/Recipes/README.md), [`/Services/AI`](../Services/AI/README.md)).
    * [`/Cookbook/Recipes/WebScraperService.cs`](../Cookbook/Recipes/WebScraperService.cs): Consumes `site_selectors.json` hosted here.
    * Any code relying on global error handling or request logging provided by the middleware.

## 7. Rationale & Key Historical Context

* **Refactored Configuration Registration:** The configuration registration logic has been moved to the `/Startup/Configuration` module to centralize all startup-related code and improve the organization of the codebase.
* **`IConfig` Interface:** Used as a marker interface for automatic discovery and registration of configuration classes.
* **Configuration Status Infrastructure:** The `RequiresConfigurationAttribute` and `ServiceUnavailableException` were added to support a more graceful handling of service unavailability due to missing configuration.
* **Service Proxy Pattern:** Service factories now register proxy implementations of external clients that throw `ServiceUnavailableException` when their methods are invoked if required configuration is missing, rather than returning null. This provides clearer error messages and more consistent behavior when a service can't operate due to missing configuration.
* **Feature Availability Infrastructure:**
    * The `DependsOnServiceAttribute` and `ServiceAvailabilityOperationFilter` were added to provide a visual indication in the Swagger UI when API endpoints may be unavailable due to missing configuration. This helps developers and API consumers understand which endpoints require specific features to be properly configured before they attempt to use them.
    * The `FeatureAvailabilityMiddleware` complements this by actively checking feature availability at runtime and preventing access to endpoints whose required features are unavailable. This provides immediate feedback to API consumers rather than letting them discover errors further down the request pipeline.
* **Middleware for Cross-Cutting Concerns:** Logging and Error Handling are implemented as middleware to apply these concerns globally without cluttering individual controller actions or services.
* **PromptBase Relocation:** The `PromptBase` class has been moved to `/Services/AI` where it more logically belongs with other AI-related functionality.

## 8. Known Issues & TODOs

* The validation in `ConfigurationStartup.ValidateAndReplaceProperties` currently only checks for `null` or a specific placeholder string for `[Required]` properties; more sophisticated validation (e.g., range checks, regex) could be added if needed.
* `site_selectors.json` parsing within `WebScraperService` could benefit from more robust error handling or schema validation upon loading.
* Error Handling middleware provides a generic error response; could be enhanced to provide slightly more specific (but still safe) error details based on exception types in non-production environments.
* Future improvements could include a centralized configuration validation service that runs at startup to provide more comprehensive validation of all required configurations.
* Consider adding a visual indicator in the Swagger UI's endpoint list (not just in the expanded operations) for endpoints that may be unavailable.
* Consider expanding the feature availability check to include more detailed information about the required configuration, such as links to documentation or examples of how to set up the required configuration.
* Implement a code analyzer or static analysis tool to ensure configuration keys are always valid and match between `appsettings.json` and `[RequiresConfiguration]` attributes. This would help catch configuration issues at compile-time rather than runtime.
* Explore caching optimization for `FeatureAvailabilityMiddleware` to reduce the overhead of feature availability checks on high-traffic endpoints.
* Add monitoring and metrics for feature availability to help identify configuration issues in production environments.
* Create more granular feature enums based on specific capabilities to enable more precise feature flagging.
