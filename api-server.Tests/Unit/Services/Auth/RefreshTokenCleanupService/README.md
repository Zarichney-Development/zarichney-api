# Module/Directory: /Unit/Services/Auth/RefreshTokenCleanupService

**Last Updated:** 2025-04-18

> **Parent:** [`Auth`](../README.md)
> **Related:**
> * **Source:** [`Services/Auth/RefreshTokenCleanupService.cs`](../../../../../api-server/Services/Auth/RefreshTokenCleanupService.cs)
> * **Dependencies:** `IServiceScopeFactory`, `UserDbContext`, `ILogger<RefreshTokenCleanupService>`, `TimeProvider` (potentially)
> * **Models:** [`Models/RefreshToken.cs`](../../../../../api-server/Services/Auth/Models/RefreshToken.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Zarichney.Standards/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Zarichney.Standards/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `RefreshTokenCleanupService` class. This service typically runs as a background task (`IHostedService`) responsible for periodically removing expired refresh tokens from the database to maintain data hygiene.

* **Why Unit Tests?** To validate the core cleanup logic of the service in isolation from the actual database and the hosting environment's timer mechanism. Tests ensure the service correctly identifies expired tokens based on the current time, interacts appropriately with the (mocked) `UserDbContext` to remove them, and handles potential errors during the process.
* **Isolation:** Achieved by mocking `IServiceScopeFactory` (to simulate accessing scoped services like `UserDbContext`), `UserDbContext` (including the `DbSet<RefreshToken>`), `ILogger`, and potentially `TimeProvider` to control the concept of "now" for expiry checks.

## 2. Scope & Key Functionality Tested (What?)

Unit tests primarily focus on the method containing the cleanup logic (often a private method like `DoWork` or similar, called periodically by `ExecuteAsync`):

* **Expired Token Identification:** Verifying that the query against the mocked `DbSet<RefreshToken>` correctly filters tokens where `ExpiresUtc` is less than the current time (controlled by mocked `TimeProvider` or system clock).
* **Token Removal:** Ensuring that *only* the identified expired tokens are marked for removal from the mocked `DbSet`.
* **Database Interaction:** Verifying that `_dbContext.SaveChangesAsync()` is called *if and only if* expired tokens were found and removed.
* **Logging:** Checking that appropriate messages are logged (e.g., starting cleanup, number of tokens removed, completion, errors) via the mocked `ILogger`.
* **Error Handling:** Testing how exceptions during database interaction (e.g., mocked `SaveChangesAsync` throws) are caught and logged.

## 3. Test Environment Setup

* **Instantiation:** `RefreshTokenCleanupService` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<IServiceScopeFactory>`: Set up to return a `Mock<IServiceScope>`.
    * `Mock<IServiceScope>`: Set up to return a `Mock<IServiceProvider>`.
    * `Mock<IServiceProvider>`: Set up to return a `Mock<UserDbContext>`.
    * `Mock<UserDbContext>`: Requires setting up the `RefreshTokens` `DbSet<RefreshToken>` property to return a mock `DbSet`. The mock `DbSet` should be backed by an in-memory list containing test `RefreshToken` data (some expired, some not). Mock `SaveChangesAsync()`.
    * `Mock<ILogger<RefreshTokenCleanupService>>`.
    * `Mock<TimeProvider>`: (Recommended for .NET 8+) To provide a controllable source of the current UTC time for reliable expiry checks.
* **Test Data:** Prepare lists of `RefreshToken` objects with varying `ExpiresUtc` dates relative to the mocked current time.

## 4. Maintenance Notes & Troubleshooting

* **DbContext/DbSet Mocking:** Setting up the mock `DbSet<RefreshToken>` to accurately simulate filtering (`Where`), removal (`RemoveRange`), and saving requires care. Ensure the backing list reflects the state changes correctly.
* **Time Dependency:** Using `DateTime.UtcNow` directly in the service makes testing difficult. Injecting `TimeProvider` (available in .NET 8+) and mocking it is the preferred approach for controlling time in tests. If not using `TimeProvider`, tests might be less reliable or require more complex workarounds.
* **Testing `IHostedService`:** While these tests focus on the core cleanup logic method, testing the full `IHostedService` `ExecuteAsync` loop (including timer delays) is often complex and might be better suited for integration or manual testing if deemed necessary.

## 5. Test Cases & TODOs

### `RefreshTokenCleanupServiceTests.cs` (Testing the core cleanup logic method)
* **TODO:** Test scenario with several expired and several non-expired tokens in mock `DbSet`.
    * Verify query identifies *only* expired tokens based on mocked time.
    * Verify `DbSet.RemoveRange` is called with the correct expired tokens.
    * Verify `_dbContext.SaveChangesAsync` is called.
    * Verify logger output indicates tokens were removed.
* **TODO:** Test scenario with *no* expired tokens in mock `DbSet`.
    * Verify query returns empty list.
    * Verify `DbSet.RemoveRange` is *not* called.
    * Verify `_dbContext.SaveChangesAsync` is *not* called.
    * Verify logger output indicates no tokens were removed.
* **TODO:** Test scenario with *only* expired tokens in mock `DbSet`.
    * Verify all tokens are identified and removed.
    * Verify `SaveChangesAsync` is called.
* **TODO:** Test scenario where `_dbContext.SaveChangesAsync` throws an exception.
    * Verify the exception is caught.
    * Verify `ILogger` logs an error with exception details.
* **TODO:** Test scenario with an empty `RefreshTokens` table in mock `DbSet`.
    * Verify no errors occur and `SaveChangesAsync` is not called.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `RefreshTokenCleanupService` unit tests. (Gemini)

