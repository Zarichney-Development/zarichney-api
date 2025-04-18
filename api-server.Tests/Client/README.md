# Module/Directory: /api-server.Tests/Client

**Last Updated:** 2025-04-18

> **Parent:** [`api-server.Tests`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** Contains the auto-generated C# Refit client code (`IZarichneyAPI` interface and associated DTOs) for interacting with the `api-server` API during integration tests.
* **Key Responsibilities:**
    * Providing a strongly-typed interface (`IZarichneyAPI`) to make HTTP requests to the `api-server`.
    * Defining C# record/class representations of the API's request and response models (DTOs).
* **Why it exists:** To enable robust, type-safe, and maintainable integration tests by abstracting away raw `HttpClient` calls and providing compile-time checks against the API contract.

## 2. Architecture & Key Concepts

* **Generation Process:** This code is **automatically generated** and **should not be manually edited**.
    * It is created by running the `Scripts/GenerateApiClient.ps1` script.
    * The script uses `dotnet swagger` to fetch the OpenAPI specification from a running instance of `api-server` and then uses `refitter` to generate the C# code based on that specification.
* **`IZarichneyAPI` Interface:** Defines methods corresponding to the API endpoints, decorated with Refit attributes (`[Get]`, `[Post]`, `[Body]`, `[Query]`, etc.).
* **DTOs:** Contains C# records/classes matching the request and response schemas defined in the OpenAPI specification.
* **Usage:** Integration tests obtain an instance of `IZarichneyAPI` (typically via `CustomWebApplicationFactory.CreateRefitClient` or `CreateAuthenticatedRefitClient`) and call its methods to interact with the API.

## 3. Interface Contract & Assumptions

* **Interface:** The methods and DTOs defined in `ZarichneyAPI.cs` and related files represent the client-side view of the `api-server` contract *at the time the generation script was last run*.
* **Critical Assumptions:**
    * Assumes the generated code accurately reflects the actual API behavior. **If the API changes, this code MUST be regenerated.**
    * Assumes the underlying `HttpClient` used to create the Refit instance is correctly configured (e.g., `BaseAddress`, authentication headers) by the test setup (e.g., `CustomWebApplicationFactory`).

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Generated Code:** Files in this directory are auto-generated. Style conventions are determined by the `refitter` tool.
* **Namespace:** Code resides in the `Zarichney.Client` namespace.

## 5. How to Work With This Code

* **Setup:** No manual setup required, but **MUST run `Scripts/GenerateApiClient.ps1`** whenever the `api-server` API contract (routes, method signatures, request/response models) changes.
* **Testing:** This code is not tested directly; it is used *by* the integration tests in [`/api-server.Tests/Integration/`](../Integration/README.md). Failures in integration tests may indicate a mismatch between this client code and the actual API, requiring regeneration.
* **Common Pitfalls / Gotchas:**
    * Forgetting to regenerate the client after API changes is a common source of integration test failures or unexpected behavior.
    * Serialization issues can occur if the generated DTOs don't perfectly match the API's JSON structure (though Refit/System.Text.Json are usually robust).

## 6. Dependencies

* **Internal Code Dependencies:** None directly, but conceptually tied to the API defined in `api-server/Controllers`.
* **External Library Dependencies:**
    * `Refit` - Required for the attributes and runtime implementation.
    * `System.Text.Json` - Used for serialization/deserialization.
* **Dependents (Impact of Changes):**
    * [`/api-server.Tests/Integration/`](../Integration/README.md) - All integration tests depend on this generated client. Regenerating the client might require updates to integration tests if method signatures or DTOs change significantly.

## 7. Rationale & Key Historical Context

* Using a generated Refit client significantly improves the reliability and maintainability of integration tests compared to manual `HttpClient` usage and string-based URLs/JSON manipulation.

## 8. Known Issues & TODOs

* Ensure the `GenerateApiClient.ps1` script is robust and easy to run in all development and CI environments.