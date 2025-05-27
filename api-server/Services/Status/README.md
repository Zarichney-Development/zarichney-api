# Module/Directory: /Services/Status

**Last Updated:** 2025-05-19

> **Parent:** [`/Services/README.md`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Provides runtime status and health reporting for critical configuration and other system checks.
* **Key Responsibilities:**
    * Checks the presence and validity of essential configuration values (API keys, secrets, connection strings) at runtime.
    * Exposes a unified service interface (`IStatusService`) that handles both direct configuration checking and service/feature availability based on configuration requirements.
    * Reports service availability based on configuration requirements marked with `[RequiresConfiguration]` attributes.
    * Provides synchronous access to feature availability status for the `ServiceAvailabilityOperationFilter` used in Swagger UI.
    * Enables health checks and automation to verify configuration without triggering runtime failures.
    * Supports graceful degradation of services when critical dependencies like the Identity Database are unavailable in non-Production environments.
* **Why it exists:** To allow both humans and automation to quickly verify the application's configuration health, service availability, and readiness, supporting operational monitoring and diagnostics.

## 2. Architecture & Key Concepts

* **StatusService:**
    * Implements the unified `IStatusService` interface.
    * Provides both direct configuration status checks and feature/service availability reporting.
    * **Direct Configuration Status:**
        * Checks all injected config objects (OpenAI, Email, GitHub, Stripe, DB connection) for missing/placeholder values and reports their status via a simple model (`ConfigurationItemStatus`).
        * Can be extended to report other runtime status/health checks in the future.
    * **Service and Feature Availability:**
        * Uses reflection (via an injected `Assembly` instance) to find types implementing `IConfig`.
        * For each discovered `IConfig` type, it resolves an instance from the `IServiceProvider`.
        * It then inspects properties marked with `[RequiresConfiguration]` attributes in these `IConfig` instances.
        * Determines service availability based on the presence and validity of required configuration values.
        * Returns a dictionary mapping `ExternalServices` enum names to their status information using `ServiceStatusInfo` records.
        * Provides synchronous access to feature availability status for the `ServiceAvailabilityOperationFilter` used in Swagger UI.
        * Includes a constructor that accepts an `Assembly` instance, allowing the assembly to be scanned for `IConfig` types to be specified (primarily for testability).
        * Supports direct service availability overrides via the `SetServiceAvailability` method, used for services like PostgresIdentityDb that are checked at application startup.
* **FeatureAvailabilityMiddleware:**
    * Inspects endpoints for `[DependsOnService]` attributes at both controller and action levels.
    * Uses `IStatusService` to check if all required features are available.
    * If any required feature is unavailable, throws a `ServiceUnavailableException` with details about missing configurations.
    * Implements middleware caching to improve performance.
* **ConfigurationItemStatus:**
    * Record type with `Name`, `Status` ("Configured" or "Missing/Invalid"), and optional `Details` (never the secret value).
* **ServiceStatusInfo:**
    * Record type with `ServiceName` (`ExternalServices` enum), `IsAvailable` flag, and `MissingConfigurations` list, providing detailed status information about a service.
    * Used both internally (as dictionary values) and externally (in API responses as list items).

## 3. Interface Contract & Assumptions

* **IStatusService:**
    * `Task<List<ConfigurationItemStatus>> GetConfigurationStatusAsync();` — Returns a list of status objects for each critical configuration item. Does not expose secret values.
    * `Task<Dictionary<ExternalServices, ServiceStatusInfo>> GetServiceStatusAsync();` — Returns a dictionary mapping `ExternalServices` enum values to their status information. Each `ServiceStatusInfo` contains the service name, availability status, and a list of missing configurations.
    * `ServiceStatusInfo? GetFeatureStatus(ExternalServices service);` — Returns status information for a specific service using the strongly-typed `ExternalServices` enum, or null if the service is not found. Primarily used by the `ServiceAvailabilityOperationFilter` for Swagger UI integration.
    * `bool IsFeatureAvailable(ExternalServices service);` — Returns whether a specific service is available (properly configured) using the strongly-typed `ExternalServices` enum. Primarily used by the `ServiceAvailabilityOperationFilter` for Swagger UI integration.
    * `void SetServiceAvailability(ExternalServices service, bool isAvailable, List<string>? missingConfigurations = null);` — Allows direct override of a service's availability status, used for services like PostgresIdentityDb that are checked at application startup.
    * Note: All string-based overloads have been removed as part of the enum refactoring.
* **Critical Assumptions:**
    * Assumes all required config objects are registered and injected.
    * Assumes placeholder value is "recommended to set in app secrets".
    * Assumes properties requiring configuration are marked with the `[RequiresConfiguration]` attribute with appropriate Feature enum values.
    * Assumes configuration property names are used to derive configuration keys (e.g., "ApiKey" property in "LlmConfig" class → "LlmConfig:ApiKey").
    * Uses `ExternalServices` enum values from `[RequiresConfiguration]` attributes to derive service names.
    * Assumes `IConfig` instances are registered as singletons in the DI container for efficient resolution by `StatusService`.
    * Assumes the `Assembly` provided to (or defaulted by) `StatusService` contains all relevant `IConfig` implementations.
    * Assumes all controller endpoints that require specific features are decorated with the `[DependsOnService]` attribute with appropriate Feature enum values.
    * Assumes the `FeatureAvailabilityMiddleware` is registered in the pipeline after routing but before the endpoint is executed.

## 4. Local Conventions & Constraints

* **Security:** Never exposes actual secret values in status responses.
* **Extensibility:** Designed to be extended for additional status/health checks as needed.
* **Configuration Requirements:** Uses `[RequiresConfiguration]` attributes to specify configuration dependencies for services.
* **Testing:**
    * Unit test `StatusService` by injecting a mock `Assembly` to control which `IConfig` types are "discovered", a mock `IServiceProvider` to control the resolution of these types, and a mock `IConfiguration` to control the reported configuration values.
    * Test the reflection logic for finding `[RequiresConfiguration]` attributes.
* **Common Pitfalls / Gotchas:**
    * Ensure new config properties are properly marked with `[RequiresConfiguration]` attributes with appropriate Feature enum values.
    * Service names in the status dictionary are derived exclusively from the `ExternalServices` enum values in `[RequiresConfiguration]` attributes.
    * When adding new features to the `Feature` enum, ensure all related configuration properties and controllers/actions are updated to use the new enum value instead of string literals.
    * Configuration keys are now automatically derived from class and property names (e.g., "ApiKey" property in "LlmConfig" class → "LlmConfig:ApiKey"), so ensure naming is consistent.

## 5. How to Work With This Code

* **Setup:**
    * The unified `StatusService` is registered as a singleton in DI (in `ServiceStartup.cs`), implementing the `IStatusService` interface.
    * `GetServiceStatusAsync()` is used by the `/api/status` endpoint and provides feature availability information for Swagger integration.
    * `GetConfigurationStatusAsync()` is used by the `/api/config` endpoint to report specific configuration status.
    * The `FeatureAvailabilityMiddleware` is registered in the request pipeline in `ApplicationStartup.cs` using the extension method `UseFeatureAvailability()`.
    * For the PostgresIdentityDb service, its availability is determined at startup by the `ValidateStartup.ValidateProductionConfiguration` method, which sets the static `ValidateStartup.IsIdentityDbAvailable` property. This property is then used to update the `StatusService` via the `SetServiceAvailability` method in `Program.cs`.
* **Marking Configuration Requirements:**
    * Decorate properties in configuration classes with `[RequiresConfiguration(ExternalServices.ServiceName)]` to indicate they are required for specific services to function.
    * Example: `[RequiresConfiguration(ExternalServices.OpenAiApi)] public string ApiKey { get; set; } = string.Empty;`
    * You can specify multiple services if the property is required for more than one service: `[RequiresConfiguration(ExternalServices.OpenAiApi, ExternalServices.GitHubAccess)]`
* **Marking Feature Dependencies:**
    * Decorate controllers or action methods with `[DependsOnService(ExternalServices.ServiceName)]` to indicate which services are required for them to function.
    * Example: `[DependsOnService(ExternalServices.OpenAiApi)] public async Task<ActionResult> Completion([FromBody] string prompt) { ... }`
    * The middleware will automatically prevent access to these endpoints if any of the required services are unavailable.
* **Checking Service Availability:**
    * Inject `IStatusService` to check if services are available based on their configuration requirements.
    * Example: `var statuses = await _statusService.GetServiceStatusAsync(); bool isLlmAvailable = statuses[ExternalServices.OpenAiApi].IsAvailable;`

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/Config`](../../Config/README.md) - Provides configuration models.
    * [`/Services`](../README.md) - Parent module.
* **External Library Dependencies:**
    * `Microsoft.Extensions.Configuration` - For reading connection strings and configuration values.
    * `Microsoft.Extensions.Logging` - For logging.
    * `Microsoft.Extensions.DependencyInjection` - For resolving registered `IConfig` instances via `IServiceProvider`.
    * `System.Reflection` - Used by `StatusService` to discover `IConfig` implementations and their attributed properties.
    * `Microsoft.OpenApi.Models` - Used by `ServiceAvailabilityOperationFilter` for Swagger documentation integration.
    * `Swashbuckle.AspNetCore.SwaggerGen` - Used by `ServiceAvailabilityOperationFilter` for Swagger documentation integration.
* **Dependents (Impact of Changes):**
    * [`/Controllers/PublicController.cs`](../../Controllers/PublicController.cs) - Uses `IStatusService` for the `/api/status` and `/api/config` endpoints.
    * [`/Controllers/AuthController.cs`](../../Controllers/AuthController.cs) - Uses `DependsOnService(ExternalServices.PostgresIdentityDb)` to indicate which actions require the Identity Database to be available.
    * [`/Startup/ValidateStartup.cs`](../../Startup/ValidateStartup.cs) - Checks the availability of the Identity Database at startup.
    * [`/Program.cs`](../../Program.cs) - Updates the `StatusService` with the Identity Database availability.
    * Any service that needs to check the availability of other services based on their configuration requirements.
    * Controllers and actions that use the `[DependsOnService]` attribute.
    * Configuration properties that use the `[RequiresConfiguration]` attribute.
    * [`/Startup/ApplicationStartup.cs`](../../Startup/ApplicationStartup.cs) - Registers the `FeatureAvailabilityMiddleware` in the request pipeline.

## 7. Rationale & Key Historical Context

* **StatusService** was originally created to support operational health checks and diagnostics after refactoring configuration validation to runtime.
* The service architecture was refactored to provide a consolidated interface (`IStatusService`) that handles both direct configuration status and service/feature availability, simplifying the API and reducing confusion.
* The new `[RequiresConfiguration]` approach provides a more declarative way to specify configuration dependencies, making it easier to maintain and extend.
* **May 2025 Refactor (`StatusService`):** `StatusService` was refactored to discover `IConfig` implementations by scanning a specified `Assembly` (defaulting to the main application assembly) and then resolving these types via `IServiceProvider`. A new constructor was added to allow injection of the `Assembly` to be scanned, significantly improving the testability of the service by allowing tests to provide a mock assembly with controlled `IConfig` types.
* **May 2025 Enum-Based Service Refactor:** The `ExternalServices` enum was used to replace string-based service names, improving type safety and making dependencies between configurations and services more explicit. The `[RequiresConfiguration]` attribute was updated to accept `ExternalServices` enum values instead of string configuration keys, and configuration keys are now automatically derived from class and property names. This allows for more reliable static analysis and compile-time checking of service dependencies.
* **May 2025 Interface Consolidation:** The previously separate `IConfigurationStatusService` interface was merged into `IStatusService` to provide a unified API for all status-related functionality, simplifying service registration and usage.
* **May 2025 API Response Format:** The external API response format for `/api/status` was changed from a Dictionary to a List of `ServiceStatusInfo` objects to better align with RESTful API practices and improve client usability. The internal API (`IStatusService.GetServiceStatusAsync()`) still returns a Dictionary for efficient lookups within the application code.
* **May 2025 Code Organization:** The service availability related files (`RequiresConfigurationAttribute`, `DependsOnServiceAttribute`, `ServiceUnavailableException`, and `ServiceAvailabilityOperationFilter`) were relocated from the `/Config` directory to the `/Services/Status` directory to better reflect their functional relationship with the status service functionality.
* **May 2025 Feature Availability Middleware:** The `FeatureAvailabilityMiddleware` was added to proactively enforce feature availability at runtime, preventing API consumers from accessing endpoints that can't function due to missing configurations. This provides a cleaner user experience by returning explicit 503 responses with detailed reasons, rather than allowing requests to proceed and fail with less specific errors.
* **May 2025 Identity Database Graceful Degradation:** Added the `PostgresIdentityDb` to the `ExternalServices` enum and implemented graceful degradation in non-Production environments when the Identity Database is unavailable. The `ValidateStartup` class checks the availability of the Identity Database at startup, and the `Program` class updates the `StatusService` with this information via the new `SetServiceAvailability` method. The `AuthController` endpoints that require the Identity Database are now decorated with the `[DependsOnService]` attribute to ensure they return 503 Service Unavailable when the database is unavailable, allowing the application to start and remain partially functional in Development environments.

## 8. Known Issues & TODOs

* Consider extending to report additional runtime health checks (e.g., external service reachability, database connectivity).
* As more service-specific configuration classes are added, expand the mapping between configuration types and logical service names in `StatusService`.
* Consider optimizing the caching mechanism in `StatusService` to better support high-concurrency scenarios where Swagger might be making multiple requests.
* Consider adding a mechanism to manually refresh the configuration status cache when configuration changes are known to have occurred.
* Implement a code analyzer or static analysis tool to ensure configuration keys are always valid and match between `appsettings.json` and `[RequiresConfiguration]` attributes. This would help catch configuration issues at compile-time rather than runtime.
* Consider adding validation or normalization in the external API for consistency between response formats.
* Add better documentation of the `ExternalServices` enum values in the API response to make the values more self-explanatory to API consumers.
