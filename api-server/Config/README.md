# Module/Directory: /Config

**Last Updated:** 2025-05-04

> **Parent:** [`Server`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This directory provides shared configuration models, core middleware components, and utilities used across the application. Configuration registration logic and `PromptBase` have been moved to the `/Startup` and `/Services/AI` modules respectively. [cite: api-server/Config/ConfigModels.cs, api-server/Config/ErrorHandlingMiddleware.cs, api-server/Config/LoggingMiddleware.cs]
* **Key Responsibilities:**
    * Defining shared configuration models (e.g., `ServerConfig`, `ClientConfig`) identified by the `IConfig` interface. [cite: api-server/Config/ConfigModels.cs]
    * Providing core middleware for request/response logging and global error handling. [cite: api-server/Config/LoggingMiddleware.cs, api-server/Config/ErrorHandlingMiddleware.cs]
    * Providing custom utilities like JSON converters and exception types. [cite: api-server/Config/JsonConverter.cs, api-server/Config/NotExpectedException.cs, api-server/Config/ConfigurationMissingException.cs, api-server/Config/ServiceUnavailableException.cs]
    * Supplying attributes for configuration requirement decoration (`RequiresConfigurationAttribute`). [cite: api-server/Config/RequiresConfigurationAttribute.cs]
    * Supplying filters for external integrations like Swagger (`FormFileOperationFilter`). [cite: api-server/Config/FormFileOperationFilter.cs]
    * Hosting static configuration files like `site_selectors.json`. [cite: api-server/Config/site_selectors.json]
* **Why it exists:** To define shared configuration models, provide reusable cross-cutting concerns via middleware, and centralize utilities needed by multiple modules.

## 2. Architecture & Key Concepts

* **Configuration Loading:** The actual loading and registration of configuration is now handled by the `/Startup/Configuration` module. This module focuses on defining the configuration models and the `IConfig` interface. [cite: api-server/Startup/Configuration/ConfigurationStartup.cs]
* **Strongly-Typed Configuration:** Configuration is accessed via injected, strongly-typed `XConfig` objects (e.g., `LlmConfig`, `RecipeConfig`). [cite: api-server/Cookbook/Recipes/RecipeService.cs, api-server/Services/AI/LlmService.cs]
* **Configuration Requirements:** Properties in configuration classes can be marked with `[RequiresConfiguration]` attribute to indicate specific configuration keys they depend on. This is used by `IConfigurationStatusService` to determine service availability. [cite: api-server/Config/RequiresConfigurationAttribute.cs]
* **Middleware:** Standard ASP.NET Core middleware pattern is used for logging (`LoggingMiddleware`) and error handling (`ErrorHandlingMiddleware`). Their order of registration in the application pipeline is important. [cite: api-server/Startup/App/ApplicationStartup.cs, api-server/Config/LoggingMiddleware.cs, api-server/Config/ErrorHandlingMiddleware.cs]
* **ErrorHandlingMiddleware:**
    * Global exception handler for all HTTP requests. Catches unhandled exceptions and returns a structured JSON error response.
    * **Now specifically catches `ConfigurationMissingException` and `ServiceUnavailableException` to return a 503 Service Unavailable response** with a user-friendly message if a required configuration is missing at runtime or a service is unavailable (see Section 5 for details).
    * Logs all exceptions using the injected logger, including request method and path for traceability.
    * For all other exceptions, returns a 500 Internal Server Error with a generic error message and trace ID.
* **Static Configuration Files:** Files like `site_selectors.json` contain data loaded and used by specific services (`WebScraperService`), although the file itself resides within this config directory for organization. [cite: api-server/Config/site_selectors.json, api-server/Cookbook/Recipes/WebScraperService.cs]

## 3. Interface Contract & Assumptions

* **Key Public Interfaces (for external callers):**
    * `IConfig`: Marker interface for configuration classes automatically registered by the system.
    * `ConfigurationMissingException`: Exception type that services should throw when they detect missing required configuration at runtime. Has properties to identify the affected configuration section and specific key details.
    * `ServiceUnavailableException`: Exception type that indicates a service is unavailable due to missing configuration or other reasons. Contains a collection of specific reasons for unavailability.
    * `RequiresConfigurationAttribute`: Attribute to decorate properties in configuration classes that are required for a service to function. Specifies the configuration key path to check.
    * Middleware (`ErrorHandlingMiddleware`, `LoggingMiddleware`): Consumed implicitly via registration in the application pipeline. They alter the request pipeline behavior.
* **Assumptions:**
    * **Configuration Registration:** Assumes configuration is registered via the `RegisterConfigurationServices` method in `/Startup/Configuration/ConfigurationStartup.cs`.
    * **Middleware Registration:** Assumes the middleware components (`UseMiddleware<T>`) are registered in the correct order in the application pipeline to function as intended (e.g., ErrorHandling typically early, Logging often early). [cite: api-server/Startup/App/ApplicationStartup.cs]
    * **File Existence:** Assumes `site_selectors.json` exists at the expected path relative to the application root when needed by services consuming it (like `WebScraperService`). [cite: api-server/Cookbook/Recipes/WebScraperService.cs]

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Configuration Class Naming:** Classes holding configuration sections must implement `IConfig` and typically end with the suffix `Config` (e.g., `RecipeConfig`). [cite: api-server/Config/ConfigModels.cs]
* **Required Configuration:** 
    * Properties in configuration classes can be marked with `[RequiresConfiguration("Section:Key")]` to specify the configuration key path they depend on.
    * Services should check for null/placeholder values at runtime and throw `ServiceUnavailableException` when attempting to use a service that depends on missing configuration.
    * The `IConfigurationStatusService` uses the `[RequiresConfiguration]` attribute to check the status of services based on their configuration requirements.
* **Middleware Order:** The order in which `LoggingMiddleware` and `ErrorHandlingMiddleware` are added in the application pipeline affects their behavior (e.g., whether errors are logged before the error handler formats the response). [cite: api-server/Startup/App/ApplicationStartup.cs]
* **Static JSON Files:** Format and schema of `site_selectors.json` are specific to the needs of the `WebScraperService`.

## 5. How to Work With This Code

* **Adding New Configuration:**
    1.  Create a new class `MyNewConfig : IConfig` in an appropriate module (or here if truly generic). [cite: api-server/Startup/Configuration/ConfigurationStartup.cs]
    2.  Add properties matching the desired configuration structure.
    3.  Mark properties that are required for specific services to function with `[RequiresConfiguration("MyNewConfig:PropertyName")]`.
    4.  Add a corresponding section (e.g., `"MyNewConfig": { ... }`) to `appsettings.json` or other config sources.
    5.  The `RegisterConfigurationServices` extension in `/Startup/Configuration/ConfigurationStartup.cs` will automatically handle binding, validation, and DI registration.
    6.  Inject `MyNewConfig` via constructor DI where needed.
* **Adding New Middleware:**
    1.  Create a new middleware class following the ASP.NET Core pattern (constructor with `RequestDelegate`, `InvokeAsync` method).
    2.  Register the middleware using `app.UseMiddleware<MyNewMiddleware>()` in `/Startup/App/ApplicationStartup.cs` at the appropriate stage in the request pipeline.
* **Error Handling for Service Unavailability:**
    * When a service can't operate due to missing configuration, throw a `ServiceUnavailableException` with one or more specific reasons for unavailability.
    * The `ErrorHandlingMiddleware` will catch the exception and return a 503 Service Unavailable response with the list of missing configurations.
    * Example: `throw new ServiceUnavailableException("Email service is unavailable", new List<string> { "Missing API key", "Missing sender email" });`
* **Configuration Status Checking:**
    * The `IConfigurationStatusService` can be injected into a component to check the status of services based on their configuration requirements.
    * Services can use this to determine if other services they depend on are available before attempting to use them.
* **Testing:**
    * **Configuration:** Testing services that consume config usually involves creating mock `IOptions<T>` or directly instantiating config objects with test values.
    * **Middleware:** Often requires integration tests using `WebApplicationFactory` or specialized middleware testing harnesses to mock `HttpContext` and `RequestDelegate`.
* **Common Pitfalls / Gotchas:** Missing required configuration values causing startup failure. Incorrect middleware registration order leading to unexpected behavior (e.g., errors not being logged). Invalid format in `site_selectors.json` causing scraping failures.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/Startup/ConfigurationStartup.cs`](../Startup/ConfigurationStartup.cs) - For registration of configuration.
    * [`/Startup/AppplicationStartup.cs`](../Startup/ApplicationStartup.cs) - For registration of middleware.
    * [`/Services/Status/IConfigurationStatusService.cs`](../Services/Status/IConfigurationStatusService.cs) - For checking service status based on configuration requirements.
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
* **Middleware for Cross-Cutting Concerns:** Logging and Error Handling are implemented as middleware to apply these concerns globally without cluttering individual controller actions or services.
* **PromptBase Relocation:** The `PromptBase` class has been moved to `/Services/AI` where it more logically belongs with other AI-related functionality.

## 8. Known Issues & TODOs

* The validation in `ConfigurationStartup.ValidateAndReplaceProperties` currently only checks for `null` or a specific placeholder string for `[Required]` properties; more sophisticated validation (e.g., range checks, regex) could be added if needed.
* `site_selectors.json` parsing within `WebScraperService` could benefit from more robust error handling or schema validation upon loading.
* Error Handling middleware provides a generic error response; could be enhanced to provide slightly more specific (but still safe) error details based on exception types in non-production environments.
* Future improvements could include a centralized configuration validation service that runs at startup to provide more comprehensive validation of all required configurations.