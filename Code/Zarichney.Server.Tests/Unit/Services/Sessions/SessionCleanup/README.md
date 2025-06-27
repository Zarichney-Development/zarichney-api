# Module/Directory: /Unit/Services/Sessions/SessionCleanup

**Last Updated:** 2025-04-18

> **Parent:** [`Sessions`](../README.md)
> *(Note: A README for `/Unit/Services/Sessions/` may be needed)*
> **Related:**
> * **Source:** [`Services/Sessions/SessionCleanup.cs`](../../../../../Zarichney.Server/Services/Sessions/SessionCleanup.cs)
> * **Dependencies:** `IServiceScopeFactory`, `ISessionManager` / Repositories (`IOrderRepository`, etc.), `ILogger<SessionCleanup>`, `TimeProvider` (potentially)
> * **Models:** [`Services/Sessions/SessionModels.cs`](../../../../../Zarichney.Server/Services/Sessions/SessionModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `SessionCleanup` class. This service typically runs as a background task (`IHostedService`) responsible for periodically removing old or inactive user sessions to prevent data accumulation.

* **Why Unit Tests?** To validate the core cleanup logic of the service in isolation from the actual database/persistence layer and the hosting environment's timer mechanism. Tests ensure the service correctly identifies sessions eligible for cleanup based on criteria (e.g., last accessed time), and interacts appropriately with the (mocked) `ISessionManager` or repositories to delete them.
* **Isolation:** Achieved by mocking `IServiceScopeFactory` (to simulate accessing scoped services like `ISessionManager` or repositories), the relevant session data access interface (`ISessionManager` or repositories), `ILogger`, and potentially `TimeProvider` to control time-based cleanup logic.

## 2. Scope & Key Functionality Tested (What?)

Unit tests primarily focus on the method containing the cleanup logic (often a private method like `DoWork` called periodically by `ExecuteAsync`):

* **Identifying Expired/Inactive Sessions:** Verifying that the correct method on the mocked `ISessionManager` or repository (e.g., `GetInactiveSessionsAsync`, `DeleteSessionsOlderThanAsync`) is called with the correct parameters (e.g., timestamp based on mocked current time).
* **Session Deletion:** Ensuring that the deletion method on the mocked `ISessionManager` or repository is invoked correctly when inactive sessions are identified.
* **Logging:** Checking that appropriate messages are logged (e.g., starting cleanup, number of sessions removed, completion, errors) via the mocked `ILogger`.
* **Error Handling:** Testing how exceptions during the cleanup process (e.g., mocked repository throws an error) are caught and logged.

## 3. Test Environment Setup

* **Instantiation:** `SessionCleanup` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<IServiceScopeFactory>` (and related `IServiceScope`, `IServiceProvider`).
    * `Mock<ISessionManager>` or Mocks for relevant repositories (`IOrderRepository`, `ILlmRepository`, etc.) depending on how session data/metadata is stored and queried for cleanup. Setup methods like `DeleteSessionsOlderThanAsync` or `GetInactiveSessionsAsync` and subsequent `DeleteSessionAsync`.
    * `Mock<ILogger<SessionCleanup>>`.
    * `Mock<TimeProvider>`: (Recommended for .NET 8+) To provide a controllable source of the current UTC time for reliable inactivity/expiry checks.

## 4. Maintenance Notes & Troubleshooting

* **Repository/Manager Mocking:** Ensure mocks accurately simulate the retrieval of inactive session IDs/objects and the successful or failed execution of deletion commands.
* **Time Dependency:** Use `TimeProvider` and mock it to reliably test time-based cleanup logic (e.g., "delete sessions inactive for more than 7 days").
* **Testing `IHostedService`:** Similar to other cleanup services, testing the full `ExecuteAsync` loop might be complex. Focus unit tests on the core `DoWork` logic triggered by the timer.

## 5. Test Cases & TODOs

### `SessionCleanupTests.cs` (Testing the core cleanup logic method)
* **TODO:** Test scenario where inactive sessions exist:
    * Mock `ISessionManager`/repository method (e.g., `DeleteSessionsOlderThanAsync`) is called with correct timestamp based on mocked time.
    * Mock method indicates sessions were deleted (e.g., returns count > 0).
    * Verify logger output indicates sessions were removed.
* **TODO:** Test scenario where *no* inactive sessions exist:
    * Mock `ISessionManager`/repository method indicates no sessions were deleted (e.g., returns count = 0).
    * Verify logger output indicates no sessions were removed.
    * Verify deletion methods were still called (or verify query method returned empty).
* **TODO:** Test scenario where `ISessionManager`/repository method throws an exception during cleanup:
    * Verify the exception is caught.
    * Verify `ILogger` logs an error with exception details.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `SessionCleanup` unit tests. (Gemini)

