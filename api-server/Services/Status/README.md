# Module/Directory: /Services/Status

**Last Updated:** 2025-05-12

> **Parent:** [`/Services/README.md`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Provides runtime status and health reporting for critical configuration and other system checks.
* **Key Responsibilities:**
    * Checks the presence and validity of essential configuration values (API keys, secrets, connection strings) at runtime.
    * Exposes a unified service interface (`IStatusService`) that handles both direct configuration checking and service/feature availability based on configuration requirements.
    * Reports service availability based on configuration requirements marked with `[RequiresConfiguration]` attributes.
    * Provides synchronous access to feature availability status for the `ServiceAvailabilityOperationFilter` used in Swagger UI.
    * Enables health checks and automation to verify configuration without triggering runtime failures.
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
        * Returns a dictionary mapping service names to their status information using `ServiceStatusInfo` records.
        * Provides synchronous access to feature availability status for the `ServiceAvailabilityOperationFilter` used in Swagger UI.
        * Includes a constructor that accepts an `Assembly` instance, allowing the assembly to be scanned for `IConfig` types to be specified (primarily for testability).
* **ConfigurationItemStatus:**
    * Record type with `Name`, `Status` ("Configured" or "Missing/Invalid"), and optional `Details` (never the secret value).
* **ServiceStatusInfo:**
    * Record type with `IsAvailable` flag and `MissingConfigurations` list, providing detailed status information about a service.

## 3. Interface Contract & Assumptions

* **IStatusService:**
    * `Task<List<ConfigurationItemStatus>> GetConfigurationStatusAsync();` — Returns a list of status objects for each critical configuration item. Does not expose secret values.
    * `Task<Dictionary<string, ServiceStatusInfo>> GetServiceStatusAsync();` — Returns a dictionary mapping service names to their status information. Each `ServiceStatusInfo` contains availability status and a list of missing configurations.
    * `ServiceStatusInfo? GetFeatureStatus(Feature feature);` — Returns status information for a specific feature using the strongly-typed Feature enum, or null if the feature is not found. Primarily used by the `ServiceAvailabilityOperationFilter` for Swagger UI integration.
    * `bool IsFeatureAvailable(Feature feature);` — Returns whether a specific feature is available (properly configured) using the strongly-typed Feature enum. Primarily used by the `ServiceAvailabilityOperationFilter` for Swagger UI integration.
    * `ServiceStatusInfo? GetFeatureStatus(string featureName);` — String-based overload maintained for backward compatibility. Returns status information for a specific feature by name, or null if the feature is not found.
    * `bool IsFeatureAvailable(string featureName);` — String-based overload maintained for backward compatibility. Returns whether a specific feature is available by name.
* **Critical Assumptions:**
    * Assumes all required config objects are registered and injected.
    * Assumes placeholder value is "recommended to set in app secrets".
    * Assumes properties requiring configuration are marked with the `[RequiresConfiguration]` attribute with appropriate Feature enum values.
    * Assumes configuration property names are used to derive configuration keys (e.g., "ApiKey" property in "LlmConfig" class → "LlmConfig:ApiKey").
    * Assumes service names can be derived from config class names (e.g., "ServerConfig" → "Server" service).
    * Assumes `IConfig` instances are registered as singletons in the DI container for efficient resolution by `StatusService`.
    * Assumes the `Assembly` provided to (or defaulted by) `StatusService` contains all relevant `IConfig` implementations.
    * Assumes all controller endpoints that require specific features are decorated with the `[RequiresFeatureEnabled]` attribute with appropriate Feature enum values.

## 4. Local Conventions & Constraints

* **Security:** Never exposes actual secret values in status responses.
* **Extensibility:** Designed to be extended for additional status/health checks as needed.
* **Configuration Requirements:** Uses `[RequiresConfiguration]` attributes to specify configuration dependencies for services.
* **Testing:**
    * Unit test `StatusService` by injecting a mock `Assembly` to control which `IConfig` types are "discovered", a mock `IServiceProvider` to control the resolution of these types, and a mock `IConfiguration` to control the reported configuration values.
    * Test the reflection logic for finding `[RequiresConfiguration]` attributes.
* **Common Pitfalls / Gotchas:**
    * Ensure new config properties are properly marked with `[RequiresConfiguration]` attributes with appropriate Feature enum values.
    * Remember that service names in the status dictionary are derived from config class names, minus the "Config" suffix.
    * When adding new features to the `Feature` enum, ensure all related configuration properties and controllers/actions are updated to use the new enum value instead of string literals.
    * Configuration keys are now automatically derived from class and property names (e.g., "ApiKey" property in "LlmConfig" class → "LlmConfig:ApiKey"), so ensure naming is consistent.

## 5. How to Work With This Code

* **Setup:** 
    * The unified `StatusService` is registered as a singleton in DI (in `ServiceStartup.cs`), implementing the `IStatusService` interface.
    * `GetServiceStatusAsync()` is used by the `/api/status` endpoint and provides feature availability information for Swagger integration.
    * `GetConfigurationStatusAsync()` is used by the `/api/config` endpoint to report specific configuration status.
* **Marking Configuration Requirements:**
    * Decorate properties in configuration classes with `[RequiresConfiguration(Feature.FeatureName)]` to indicate they are required for specific features to function.
    * Example: `[RequiresConfiguration(Feature.LLM)] public string ApiKey { get; set; } = string.Empty;`
    * You can specify multiple features if the property is required for more than one feature: `[RequiresConfiguration(Feature.LLM, Feature.AiServices)]`
* **Checking Service Availability:**
    * Inject `IStatusService` to check if services are available based on their configuration requirements.
    * Example: `var statuses = await _statusService.GetServiceStatusAsync(); bool isLlmAvailable = statuses["Llm"].IsAvailable;`

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/Config`](../../Config/README.md) - Provides configuration models and the `RequiresConfigurationAttribute`.
    * [`/Config/ServiceUnavailableException.cs`](../../Config/ServiceUnavailableException.cs) - Exception thrown when a service is unavailable due to missing configuration.
    * [`/Services`](../README.md) - Parent module.
* **External Library Dependencies:**
    * `Microsoft.Extensions.Configuration` - For reading connection strings and configuration values.
    * `Microsoft.Extensions.Logging` - For logging.
    * `Microsoft.Extensions.DependencyInjection` - For resolving registered `IConfig` instances via `IServiceProvider`.
    * `System.Reflection` - Used by `StatusService` to discover `IConfig` implementations and their attributed properties.
* **Dependents (Impact of Changes):**
    * [`/Controllers/PublicController.cs`](../../Controllers/PublicController.cs) - Uses `IStatusService` for the `/api/status` and `/api/config` endpoints.
    * [`/Config/ServiceAvailabilityOperationFilter.cs`](../../Config/ServiceAvailabilityOperationFilter.cs) - Uses `IStatusService` to check feature availability for Swagger UI integration.
    * [`/Config/RequiresFeatureEnabledAttribute.cs`](../../Config/RequiresFeatureEnabledAttribute.cs) - Works with the feature availability methods of `IStatusService` to determine which API endpoints may be unavailable.
    * Any service that needs to check the availability of other services based on their configuration requirements.

## 7. Rationale & Key Historical Context

* **StatusService** was originally created to support operational health checks and diagnostics after refactoring configuration validation to runtime.
* The service architecture was refactored to provide a consolidated interface (`IStatusService`) that handles both direct configuration status and service/feature availability, simplifying the API and reducing confusion.
* The new `[RequiresConfiguration]` approach provides a more declarative way to specify configuration dependencies, making it easier to maintain and extend.
* **May 2025 Refactor (`StatusService`):** `StatusService` was refactored to discover `IConfig` implementations by scanning a specified `Assembly` (defaulting to the main application assembly) and then resolving these types via `IServiceProvider`. A new constructor was added to allow injection of the `Assembly` to be scanned, significantly improving the testability of the service by allowing tests to provide a mock assembly with controlled `IConfig` types.
* **May 2025 Feature Enumeration Refactor:** The `Feature` enum was introduced to replace string-based feature names, improving type safety and making dependencies between configurations and features more explicit. The `[RequiresConfiguration]` attribute was updated to accept `Feature` enum values instead of string configuration keys, and configuration keys are now automatically derived from class and property names.
* **May 2025 Interface Consolidation:** The previously separate `IConfigurationStatusService` interface was merged into `IStatusService` to provide a unified API for all status-related functionality, simplifying service registration and usage.

## 8. Known Issues & TODOs

* Consider extending to report additional runtime health checks (e.g., external service reachability, database connectivity).
* As more service-specific configuration classes are added, expand the mapping between configuration types and logical service names in `StatusService`.
* Consider optimizing the caching mechanism in `StatusService` to better support high-concurrency scenarios where Swagger might be making multiple requests.
* Consider adding a mechanism to manually refresh the configuration status cache when configuration changes are known to have occurred.
* Implement a code analyzer or static analysis tool to ensure configuration keys are always valid and match between `appsettings.json` and `[RequiresConfiguration]` attributes. This would help catch configuration issues at compile-time rather than runtime.