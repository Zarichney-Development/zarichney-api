# Module/Directory: /Services/Status

**Last Updated:** 2025-05-10

> **Parent:** [`/Services/README.md`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Provides runtime status and health reporting for critical configuration and other system checks.
* **Key Responsibilities:**
    * Checks the presence and validity of essential configuration values (API keys, secrets, connection strings) at runtime.
    * Exposes services (`IStatusService`, `IConfigurationStatusService`) and implementations (`StatusService`, `ConfigurationStatusService`) for status checks.
    * Reports service availability based on configuration requirements marked with `[RequiresConfiguration]` attributes.
    * Enables health checks and automation to verify configuration without triggering runtime failures.
* **Why it exists:** To allow both humans and automation to quickly verify the application's configuration health, service availability, and readiness, supporting operational monitoring and diagnostics.

## 2. Architecture & Key Concepts

* **StatusService:**
    * Implements `IStatusService`.
    * Checks all injected config objects (OpenAI, Email, GitHub, Stripe, DB connection) for missing/placeholder values and reports their status via a simple model (`ConfigurationItemStatus`).
    * Can be extended to report other runtime status/health checks in the future.
* **ConfigurationStatusService:**
    * Implements `IConfigurationStatusService`.
    * Uses reflection (via an injected `Assembly` instance) to find types implementing `IConfig`.
    * For each discovered `IConfig` type, it resolves an instance from the `IServiceProvider`.
    * It then inspects properties marked with `[RequiresConfiguration]` attributes in these `IConfig` instances.
    * Determines service availability based on the presence and validity of required configuration values.
    * Returns a dictionary mapping service names to their status information using `ServiceStatusInfo` records.
    * Includes a constructor that accepts an `Assembly` instance, allowing the assembly to be scanned for `IConfig` types to be specified (primarily for testability).
* **ConfigurationItemStatus:**
    * Record type with `Name`, `Status` ("Configured" or "Missing/Invalid"), and optional `Details` (never the secret value).
* **ServiceStatusInfo:**
    * Record type with `IsAvailable` flag and `MissingConfigurations` list, providing detailed status information about a service.

## 3. Interface Contract & Assumptions

* **IStatusService:**
    * `Task<List<ConfigurationItemStatus>> GetConfigurationStatusAsync();` — Returns a list of status objects for each critical configuration item. Does not expose secret values.
* **IConfigurationStatusService:**
    * `Task<Dictionary<string, ServiceStatusInfo>> GetServiceStatusAsync();` — Returns a dictionary mapping service names to their status information. Each `ServiceStatusInfo` contains availability status and a list of missing configurations.
* **Critical Assumptions:**
    * Assumes all required config objects are registered and injected.
    * Assumes placeholder value is "recommended to set in app secrets".
    * Assumes properties requiring configuration are marked with the `[RequiresConfiguration]` attribute.
    * Assumes service names can be derived from config class names (e.g., "ServerConfig" → "Server" service).
    * Assumes `IConfig` instances are registered as singletons in the DI container for efficient resolution by `ConfigurationStatusService`.
    * Assumes the `Assembly` provided to (or defaulted by) `ConfigurationStatusService` contains all relevant `IConfig` implementations.

## 4. Local Conventions & Constraints

* **Security:** Never exposes actual secret values in status responses.
* **Extensibility:** Designed to be extended for additional status/health checks as needed.
* **Configuration Requirements:** Uses `[RequiresConfiguration]` attributes to specify configuration dependencies for services.
* **Testing:**
    * Unit test `ConfigurationStatusService` by injecting a mock `Assembly` to control which `IConfig` types are "discovered", a mock `IServiceProvider` to control the resolution of these types, and a mock `IConfiguration` to control the reported configuration values.
    * Test the reflection logic for finding `[RequiresConfiguration]` attributes.
* **Common Pitfalls / Gotchas:**
    * Ensure new config keys are properly marked with `[RequiresConfiguration]` attributes.
    * Remember that service names in the status dictionary are derived from config class names, minus the "Config" suffix.

## 5. How to Work With This Code

* **Setup:** 
    * Services are registered in DI (in `ServiceStartup.cs`).
    * `IConfigurationStatusService` is used by the new `/api/status` endpoint.
    * `IStatusService` is maintained for backward compatibility but will be phased out.
* **Marking Configuration Requirements:**
    * Decorate properties in configuration classes with `[RequiresConfiguration("Section:Key")]` to indicate they are required for a service to function.
    * Example: `[RequiresConfiguration("LlmService:ApiKey")] public string ApiKey { get; set; } = string.Empty;`
* **Checking Service Availability:**
    * Inject `IConfigurationStatusService` to check if services are available based on their configuration requirements.
    * Example: `var statuses = await _configurationStatusService.GetServiceStatusAsync(); bool isLlmAvailable = statuses["LlmService"].IsAvailable;`

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/Config`](../../Config/README.md) - Provides configuration models and the `RequiresConfigurationAttribute`.
    * [`/Config/ServiceUnavailableException.cs`](../../Config/ServiceUnavailableException.cs) - Exception thrown when a service is unavailable due to missing configuration.
    * [`/Services`](../README.md) - Parent module.
* **External Library Dependencies:**
    * `Microsoft.Extensions.Configuration` - For reading connection strings and configuration values.
    * `Microsoft.Extensions.Logging` - For logging.
    * `Microsoft.Extensions.DependencyInjection` - For resolving registered `IConfig` instances via `IServiceProvider`.
    * `System.Reflection` - Used by `ConfigurationStatusService` to discover `IConfig` implementations and their attributed properties.
* **Dependents (Impact of Changes):**
    * [`/Controllers/PublicController.cs`](../../Controllers/PublicController.cs) - Consumes `IConfigurationStatusService` for the `/api/status` endpoint.
    * Any service that needs to check the availability of other services based on their configuration requirements.

## 7. Rationale & Key Historical Context

* **StatusService** was originally created to support operational health checks and diagnostics after refactoring configuration validation to runtime.
* **IConfigurationStatusService** was added as part of Epic #1 to provide a more structured approach to checking service availability based on configuration requirements. It enables a unified endpoint for service status and supports graceful service unavailability handling.
* The new `[RequiresConfiguration]` approach provides a more declarative way to specify configuration dependencies, making it easier to maintain and extend.
* **May 2025 Refactor (`ConfigurationStatusService`):** `ConfigurationStatusService` was refactored to discover `IConfig` implementations by scanning a specified `Assembly` (defaulting to the main application assembly) and then resolving these types via `IServiceProvider`. This replaced a previous mechanism that directly used `IServiceProvider.GetServices<IConfig>()`. A new constructor was added to allow injection of the `Assembly` to be scanned, significantly improving the testability of the service by allowing tests to provide a mock assembly with controlled `IConfig` types.

## 8. Known Issues & TODOs

* Consider extending to report additional runtime health checks (e.g., external service reachability, database connectivity).
* As more service-specific configuration classes are added, expand the mapping between configuration types and logical service names in `ConfigurationStatusService`.
* Phase out the legacy `IStatusService` once all dependents are migrated to `IConfigurationStatusService`.
