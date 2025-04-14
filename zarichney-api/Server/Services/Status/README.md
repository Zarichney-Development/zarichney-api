# Module/Directory: Server/Services/Status

**Last Updated:** 2025-04-14

> **Parent:** [`Server/Services/README.md`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Provides runtime status and health reporting for critical configuration and other system checks.
* **Key Responsibilities:**
    * Checks the presence and validity of essential configuration values (API keys, secrets, connection strings) at runtime.
    * Exposes a service (`IStatusService`) and implementation (`StatusService`) for status checks.
    * Enables health checks and automation to verify configuration without triggering runtime failures.
* **Why it exists:** To allow both humans and automation to quickly verify the application's configuration health and readiness, supporting operational monitoring and diagnostics.

## 2. Architecture & Key Concepts

* **StatusService:**
    * Implements `IStatusService`.
    * Checks all injected config objects (OpenAI, Email, GitHub, Stripe, DB connection) for missing/placeholder values and reports their status via a simple model (`ConfigurationItemStatus`).
    * Can be extended to report other runtime status/health checks in the future.
* **ConfigurationItemStatus:**
    * Record type with `Name`, `Status` ("Configured" or "Missing/Invalid"), and optional `Details` (never the secret value).

## 3. Interface Contract & Assumptions

* **IStatusService:**
    * `Task<List<ConfigurationItemStatus>> GetConfigurationStatusAsync();` — Returns a list of status objects for each critical configuration item. Does not expose secret values.
* **Critical Assumptions:**
    * Assumes all required config objects are registered and injected.
    * Assumes placeholder value is "recommended to set in app secrets".

## 4. Local Conventions & Constraints

* **Security:** Never exposes actual secret values in status responses.
* **Extensibility:** Designed to be extended for additional status/health checks as needed.

## 5. How to Work With This Code

* **Setup:** No special setup required; service is registered in DI and used by the public status endpoint.
* **Testing:**
    * Unit test by injecting mock config objects and verifying status output.
* **Common Pitfalls / Gotchas:**
    * Ensure new config keys are added to the status check logic if they become critical.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`Server/Config`](../../Config/README.md) - Provides configuration models.
    * [`Server/Services`](../README.md) - Parent module.
* **External Library Dependencies:**
    * `Microsoft.Extensions.Configuration` - For reading connection strings.
    * `Microsoft.Extensions.Logging` - For logging.
* **Dependents (Impact of Changes):**
    * [`Server/Controllers/PublicController.cs`](../../Controllers/PublicController.cs) - Consumes `IStatusService` for the `/api/status/config` endpoint.

## 7. Rationale & Key Historical Context

* Created to support operational health checks and diagnostics after refactoring configuration validation to runtime.

## 8. Known Issues & TODOs

* Consider extending to report additional runtime health checks (e.g., external service reachability, database connectivity).
