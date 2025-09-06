# Module/Directory: /Unit/Services/Auth/RoleInitializer

**Last Updated:** 2025-04-18

> **Parent:** [`Auth`](../README.md)
> **Related:**
> * **Source:** [`Services/Auth/RoleInitializer.cs`](../../../../../Zarichney.Server/Services/Auth/RoleInitializer.cs)
> * **Dependencies:** `RoleManager<IdentityRole>`, `IServiceProvider` / `IServiceScopeFactory`, `ILogger<RoleInitializer>`
> * **Models:** [`Models/Roles.cs`](../../../../../Zarichney.Server/Services/Auth/Models/Roles.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Standards/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `RoleInitializer` class. This service is typically executed once during application startup to ensure that predefined application roles (e.g., "Admin", "User" as defined in `Roles.cs`) exist within the identity system.

* **Why Unit Tests?** To validate the initialization logic in isolation from the actual database and application startup process. Tests ensure the service correctly iterates through the predefined roles, checks for their existence using the (mocked) `RoleManager`, and attempts to create them only if they are missing.
* **Isolation:** Achieved by mocking `RoleManager<IdentityRole>`, `IServiceProvider` (or `IServiceScopeFactory` if used to create a scope for `RoleManager`), and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests focus on the core initialization method (e.g., `InitializeAsync`, `StartAsync`, or similar):

* **Role Iteration:** Verifying the logic iterates through all roles defined in the `Roles` class/constants.
* **Existence Check:** Ensuring `RoleManager.RoleExistsAsync` is called for each predefined role name.
* **Conditional Creation:** Verifying that `RoleManager.CreateAsync` is called *only* for roles where the mocked `RoleExistsAsync` returned `false`.
* **Creation Skip:** Verifying that `RoleManager.CreateAsync` is *not* called for roles where the mocked `RoleExistsAsync` returned `true`.
* **Error Handling:** Testing how `IdentityResult.Failed` responses from the mocked `CreateAsync` call are handled (e.g., logged).
* **Logging:** Checking that appropriate messages are logged for role checking and creation attempts (success or failure).

## 3. Test Environment Setup

* **Instantiation:** `RoleInitializer` is instantiated directly, or its initialization method is invoked.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<RoleManager<IdentityRole>>`: Requires mocking its `IRoleStore`. Needs setup for `RoleExistsAsync` (to return true/false based on test scenario) and `CreateAsync` (to return `IdentityResult.Success` or `IdentityResult.Failed`).
    * `Mock<IServiceProvider>` / `Mock<IServiceScopeFactory>` / `Mock<IServiceScope>`: Needed if `RoleManager` is resolved within a scope during initialization.
    * `Mock<ILogger<RoleInitializer>>`.

## 4. Maintenance Notes & Troubleshooting

* **`RoleManager` Mocking:** Mocking `RoleManager` requires mocking its underlying `IRoleStore`. Ensure the `RoleExistsAsync` and `CreateAsync` mocks are configured correctly for each scenario being tested.
* **`Roles` Definition:** If the list of predefined roles in `Roles.cs` changes, these tests must be updated to reflect the new/removed roles.
* **Scope Resolution:** If `RoleManager` is resolved via `IServiceProvider`, ensure the mock provider is correctly set up to return the mock `RoleManager`.

## 5. Test Cases & TODOs

### `RoleInitializerTests.cs` (Testing the initialization logic method)
* **TODO:** Test scenario where *no* predefined roles exist in the mock setup.
    * Verify `RoleExistsAsync` called for all roles (returns false).
    * Verify `CreateAsync` called for all roles.
    * Verify success logs for creation.
* **TODO:** Test scenario where *all* predefined roles already exist in the mock setup.
    * Verify `RoleExistsAsync` called for all roles (returns true).
    * Verify `CreateAsync` is *not* called for any role.
    * Verify logs indicating roles already exist.
* **TODO:** Test scenario with a mix of existing and non-existing roles.
    * Verify `RoleExistsAsync` called for all roles.
    * Verify `CreateAsync` called *only* for roles where `RoleExistsAsync` returned false.
* **TODO:** Test scenario where `RoleManager.CreateAsync` fails for a role.
    * Mock `CreateAsync` to return `IdentityResult.Failed` with errors.
    * Verify the failure is logged via `ILogger`.
    * Verify the process continues for other roles (if applicable).

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `RoleInitializer` unit tests. (Gemini)

