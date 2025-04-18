# Module/Directory: /Unit/Services/Auth/RoleManager

**Last Updated:** 2025-04-18

> **Parent:** [`Auth`](../README.md)
> **Related:**
> * **Source:** [`Services/Auth/RoleManager.cs`](../../../../../api-server/Services/Auth/RoleManager.cs)
> * **Dependencies:** `Microsoft.AspNetCore.Identity.RoleManager<IdentityRole>`, `Microsoft.AspNetCore.Identity.UserManager<IdentityUser>`, `ILogger<RoleManager>`
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Development/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the custom `RoleManager` service class. This service likely provides application-specific methods or abstractions for managing user roles, potentially wrapping or extending the functionality of the base ASP.NET Core Identity `RoleManager<IdentityRole>`.

* **Why Unit Tests?** To validate the *custom logic* added by this specific `RoleManager` service in isolation. Tests ensure it correctly interacts with the underlying (mocked) Identity `RoleManager` and `UserManager`, performs any additional validation or mapping, and returns expected results.
* **Isolation:** Achieved by mocking the Identity `RoleManager<IdentityRole>`, `UserManager<IdentityUser>`, and `ILogger`. The tests focus on the behavior of the custom service, not the underlying Identity framework implementation (which is assumed to work).

## 2. Scope & Key Functionality Tested (What?)

Unit tests focus on the public methods exposed by the custom `RoleManager.cs`:

* **Custom Role Operations:** Testing methods specific to this service (e.g., `AssignUserRoleAsync(string userId, string roleName)`, `RemoveUserRoleAsync(...)`, `GetUserRolesFormattedAsync(...)`, etc. - *actual methods depend on the implementation*).
* **Interaction with Identity Managers:** Verifying that the custom methods correctly call the appropriate methods on the mocked Identity `RoleManager` (e.g., `FindByNameAsync`, `AddToRoleAsync`, `RemoveFromRoleAsync`) and `UserManager` (e.g., `FindByIdAsync`) with the correct arguments.
* **Result Handling:** Ensuring the results from the mocked Identity manager calls (`IdentityResult`, user objects, role lists) are processed correctly and mapped to the expected return type of the custom service methods.
* **Error Handling & Validation:** Testing any custom validation logic (e.g., checking if role name is valid before calling Identity) or specific error handling added by this service.

## 3. Test Environment Setup

* **Instantiation:** The custom `RoleManager` service is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<RoleManager<IdentityRole>>`: Requires mocking its `IRoleStore`. Needs setup for methods like `FindByNameAsync`, `RoleExistsAsync`, `AddToRoleAsync`, `RemoveFromRoleAsync`, `GetRolesAsync` (for a user) to return expected roles or `IdentityResult` objects.
    * `Mock<UserManager<IdentityUser>>`: Requires mocking its `IUserStore`. Needs setup for methods like `FindByIdAsync`.
    * `Mock<ILogger<RoleManager>>`.

## 4. Maintenance Notes & Troubleshooting

* **Identity Manager Mocking:** Mocking `RoleManager` and `UserManager` requires mocking their underlying stores (`IRoleStore`, `IUserStore`). Ensure mocks are configured to simulate different scenarios (user/role found/not found, `IdentityResult.Success`, `IdentityResult.Failed`).
* **Focus on Custom Logic:** Tests should primarily target the logic *added* by this custom `RoleManager`. Avoid simply re-testing the basic functionality of the underlying Identity managers.
* **Method Signature Changes:** If methods in the custom `RoleManager` or the underlying Identity managers change, these tests will need updating.

## 5. Test Cases & TODOs

### `RoleManagerTests.cs` (Testing custom methods)
*(Examples assume methods like AssignUserRoleAsync, RemoveUserRoleAsync exist)*

* **TODO (`AssignUserRoleAsync`):** Test success path -> mock `UserManager.FindByIdAsync` finds user, mock `RoleManager.FindByNameAsync` finds role, mock `RoleManager.AddToRoleAsync` returns `Success` -> verify `AddToRoleAsync` called, verify success result returned.
* **TODO (`AssignUserRoleAsync`):** Test user not found -> mock `UserManager.FindByIdAsync` returns null -> verify appropriate failure result/exception.
* **TODO (`AssignUserRoleAsync`):** Test role not found -> mock `RoleManager.FindByNameAsync` returns null -> verify appropriate failure result/exception.
* **TODO (`AssignUserRoleAsync`):** Test user already in role -> mock `UserManager.IsInRoleAsync` returns true -> verify `AddToRoleAsync` not called (or handle as needed), verify appropriate result.
* **TODO (`AssignUserRoleAsync`):** Test `AddToRoleAsync` fails -> mock `RoleManager.AddToRoleAsync` returns `Failed` -> verify failure result returned.
* **TODO (`RemoveUserRoleAsync`):** Test success path -> mock user/role lookups success, mock `RoleManager.RemoveFromRoleAsync` returns `Success` -> verify `RemoveFromRoleAsync` called, verify success result.
* **TODO (`RemoveUserRoleAsync`):** Test user not found -> verify failure result/exception.
* **TODO (`RemoveUserRoleAsync`):** Test role not found -> verify failure result/exception.
* **TODO (`RemoveUserRoleAsync`):** Test user not in role -> mock `UserManager.IsInRoleAsync` returns false -> verify `RemoveFromRoleAsync` not called (or handle), verify appropriate result.
* **TODO (`RemoveUserRoleAsync`):** Test `RemoveFromRoleAsync` fails -> mock `RoleManager.RemoveFromRoleAsync` returns `Failed` -> verify failure result.
* **TODO:** *(Add tests for any other custom methods, e.g., getting formatted roles)*

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for custom `RoleManager` unit tests. (Gemini)

