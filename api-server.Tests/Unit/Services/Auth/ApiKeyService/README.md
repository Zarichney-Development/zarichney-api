# Module/Directory: /Unit/Services/Auth/ApiKeyService

**Last Updated:** 2025-04-18

> **Parent:** [`Auth`](../README.md)
> **Related:**
> * **Source:** [`Services/Auth/ApiKeyService.cs`](../../../../../api-server/Services/Auth/ApiKeyService.cs)
> * **Dependencies:** `UserDbContext`, `UserManager<IdentityUser>`, `ILogger<ApiKeyService>`
> * **Models:** [`Models/ApiKey.cs`](../../../../../api-server/Services/Auth/Models/ApiKey.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Zarichney.Standards/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Zarichney.Standards/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `ApiKeyService` class. This service is responsible for the core logic of creating, validating, retrieving, and revoking API keys associated with users.

* **Why Unit Tests?** To validate the internal logic of `ApiKeyService` methods in isolation from the actual database and ASP.NET Core Identity framework implementations. Tests ensure correct key generation, validation rules (activity status, expiry), database interactions (via mocked `DbContext`), and user lookups (via mocked `UserManager`).
* **Isolation:** Achieved by mocking `UserDbContext` (including the `DbSet<ApiKey>`), `UserManager<IdentityUser>`, and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `ApiKeyService` focus on its public methods:

* **`CreateApiKeyAsync`:**
    * Verifying successful key generation for a valid user ID.
    * Handling cases where the user ID does not exist (mock `UserManager.FindByIdAsync` returning null).
    * Correctly setting optional description and expiry dates.
    * Ensuring the generated key is added to the mocked `DbSet<ApiKey>` and `SaveChangesAsync` is called.
* **`GetApiKeyAsync` (by key value):**
    * Retrieving an active, non-expired key.
    * Handling cases where the key doesn't exist, is inactive, or is expired (should return null).
* **`GetApiKeyByIdAsync`:**
    * Retrieving a key by its primary ID.
    * Handling cases where the ID doesn't exist.
* **`GetApiKeysByUserIdAsync`:**
    * Retrieving all keys associated with a specific user ID.
    * Handling cases where the user has no keys (should return an empty list).
* **`RevokeApiKeyAsync`:**
    * Successfully marking an existing key as inactive.
    * Verifying `SaveChangesAsync` is called.
    * Handling cases where the key to revoke doesn't exist.
* **`ValidateApiKeyAsync`:**
    * Validating an active, non-expired key.
    * Returning false for inactive, expired, or non-existent keys.
* **`AuthenticateWithApiKey`:** (If this logic resides here and not solely in middleware)
    * Successfully authenticating a request given a valid API key.
    * Verifying interaction with mocked `UserManager` to retrieve user details and roles/claims.
    * Handling invalid/inactive/expired keys (should not authenticate).
    * Handling cases where the associated user no longer exists.

## 3. Test Environment Setup

* **Instantiation:** `ApiKeyService` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<UserDbContext>`: Requires setting up the `ApiKeys` `DbSet<ApiKey>` property to return a mock `DbSet` (often backed by an in-memory list for testing queries and updates). Mock `SaveChangesAsync()`.
    * `Mock<UserManager<IdentityUser>>`: Mock `FindByIdAsync` and potentially `GetRolesAsync`/`GetClaimsAsync` if used in authentication logic. Requires mocking `IUserStore`.
    * `Mock<ILogger<ApiKeyService>>`.

## 4. Maintenance Notes & Troubleshooting

* **DbContext/DbSet Mocking:** Setting up mock `DbSet`s that correctly simulate querying (`FindAsync`, `Where`, `ToListAsync`) and updates (`Add`, `Remove`, state changes) requires care. Consider using libraries like `MockQueryable` or established patterns for mocking EF Core.
* **UserManager Mocking:** Mocking `UserManager` requires mocking its underlying `IUserStore`. Ensure mocks return appropriate `IdentityUser` objects or nulls based on the test scenario.
* **Validation Logic:** Pay close attention to testing edge cases for key validation, especially around expiry dates (e.g., exactly expired, just about to expire).

## 5. Test Cases & TODOs

### `ApiKeyServiceTests.cs`
* **TODO (`CreateApiKeyAsync`):** Test success path - mock `UserManager.FindByIdAsync` returns user -> verify key added to mock `DbSet`, `SaveChangesAsync` called, returned key is valid.
* **TODO (`CreateApiKeyAsync`):** Test with description and expiry date set.
* **TODO (`CreateApiKeyAsync`):** Test user not found - mock `UserManager.FindByIdAsync` returns null -> verify `KeyNotFoundException` (or appropriate exception/result).
* **TODO (`GetApiKeyAsync` - by value):** Test finding active, non-expired key -> verify correct key returned.
* **TODO (`GetApiKeyAsync` - by value):** Test key not found -> verify null returned.
* **TODO (`GetApiKeyAsync` - by value):** Test key found but inactive -> verify null returned.
* **TODO (`GetApiKeyAsync` - by value):** Test key found but expired -> verify null returned.
* **TODO (`GetApiKeyByIdAsync`):** Test finding key by ID -> verify correct key returned.
* **TODO (`GetApiKeyByIdAsync`):** Test ID not found -> verify null returned.
* **TODO (`GetApiKeysByUserIdAsync`):** Test user with multiple keys -> verify correct list returned.
* **TODO (`GetApiKeysByUserIdAsync`):** Test user with no keys -> verify empty list returned.
* **TODO (`RevokeApiKeyAsync`):** Test success path -> find key, set `IsActive = false`, verify `SaveChangesAsync` called, return true.
* **TODO (`RevokeApiKeyAsync`):** Test key not found -> verify return false.
* **TODO (`ValidateApiKeyAsync`):** Test valid key -> verify true returned.
* **TODO (`ValidateApiKeyAsync`):** Test inactive key -> verify false returned.
* **TODO (`ValidateApiKeyAsync`):** Test expired key -> verify false returned.
* **TODO (`ValidateApiKeyAsync`):** Test key not found -> verify false returned.
* **TODO (`AuthenticateWithApiKey`):** Test success -> mock `UserManager` calls -> verify user populated correctly.
* **TODO (`AuthenticateWithApiKey`):** Test invalid key -> verify false/null returned.
* **TODO (`AuthenticateWithApiKey`):** Test valid key, user not found -> verify false/null returned.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `ApiKeyService` unit tests. (Gemini)

