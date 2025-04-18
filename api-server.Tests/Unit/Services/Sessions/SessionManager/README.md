# Module/Directory: /Unit/Services/Sessions/SessionManager

**Last Updated:** 2025-04-18

> **Parent:** [`Sessions`](../README.md)
> *(Note: A README for `/Unit/Services/Sessions/` may be needed)*
> **Related:**
> * **Source:** [`Services/Sessions/SessionManager.cs`](../../../../../api-server/Services/Sessions/SessionManager.cs)
> * **Interface:** [`Services/Sessions/ISessionManager.cs`](../../../../../api-server/Services/Sessions/SessionManager.cs) (Likely implicitly defined)
> * **Dependencies:** `IOrderRepository`, `ILlmRepository`, `ICustomerRepository` (or a combined `ISessionRepository`), `ILogger<SessionManager>`
> * **Models:** [`Services/Sessions/SessionModels.cs`](../../../../../api-server/Services/Sessions/SessionModels.cs)
> * **Standards:** [`TestingStandards.md`](../../../../../Docs/Development/TestingStandards.md), [`DocumentationStandards.md`](../../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `SessionManager` class. This service is responsible for the core logic of managing user sessions, including creating new sessions, retrieving existing sessions based on various criteria (ID, user, order), updating session state (e.g., adding conversation history), and potentially handling persistence via repositories.

* **Why Unit Tests?** To validate the session lifecycle logic (create, read, update, save) in isolation from the actual database or persistence mechanism. Tests ensure the service correctly generates IDs, interacts with (mocked) repositories to load/save session data, updates session state appropriately, and handles scenarios like session-not-found.
* **Isolation:** Achieved by mocking the repositories (`IOrderRepository`, `ILlmRepository`, `ICustomerRepository`, or a combined `ISessionRepository`) that the `SessionManager` uses for data persistence and retrieval, as well as `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `SessionManager` focus on its public methods implementing `ISessionManager`:

* **`GetOrCreateSessionAsync(...)`:**
    * Handling requests with and without an existing session ID.
    * Generating a new unique session ID when creating a new session.
    * Calling repository methods (mocked) to retrieve an existing session by ID.
    * Returning a new session object if no ID is provided or the ID is not found.
    * Returning the existing session object if found.
* **`GetSessionByIdAsync(string sessionId)`:**
    * Calling repository methods (mocked) to retrieve a session by its specific ID.
    * Handling session found and session not found scenarios.
* **Retrieval by other criteria:** (e.g., `GetSessionByUserIdAsync`, `GetSessionByOrderIdAsync` - if methods exist)
    * Calling appropriate repository query methods (mocked).
    * Handling found/not found scenarios.
* **`AddConversationTurnAsync(string sessionId, ConversationTurn turn)` / `UpdateSessionAsync(Session session)`:**
    * Retrieving the session (via mocked repo).
    * Modifying the session state (e.g., adding to a list of turns).
    * Calling repository methods (mocked) to save the updated session state (e.g., `UpdateSessionAsync`, `SaveChangesAsync`).
* **Session Persistence:** Verifying that methods intended to save session state correctly call the underlying (mocked) repository's save/update methods.

## 3. Test Environment Setup

* **Instantiation:** `SessionManager` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<IOrderRepository>`, `Mock<ILlmRepository>`, `Mock<ICustomerRepository>` (or `Mock<ISessionRepository>` if combined): Setup repository methods (`GetSessionByIdAsync`, `SaveSessionAsync`, etc.) to return specific `Session` objects, nulls, or throw exceptions based on test scenarios.
    * `Mock<ILogger<SessionManager>>`.

## 4. Maintenance Notes & Troubleshooting

* **Repository Mocking:** Ensure repository mocks accurately simulate data retrieval (returning `Session` objects or null) and persistence calls (verifying `Save` methods are called).
* **Session Model:** Changes to the `Session` model or related models (`ConversationTurn`) in `SessionModels.cs` may require updating test data and assertions.
* **Concurrency:** If `SessionManager` implements any locking or concurrency control for session access, those aspects need careful testing (though potentially complex for unit tests).

## 5. Test Cases & TODOs

### `SessionManagerTests.cs`
* **TODO (`GetOrCreateSessionAsync` - Create):** Test call with null/empty session ID -> verify new session ID generated, verify new `Session` object returned, verify repository *not* called for retrieval (or called and returns null).
* **TODO (`GetOrCreateSessionAsync` - Retrieve Found):** Test call with valid session ID -> mock repository `GetSessionByIdAsync` returns existing session -> verify correct session object returned.
* **TODO (`GetOrCreateSessionAsync` - Retrieve Not Found):** Test call with invalid/unknown session ID -> mock repository `GetSessionByIdAsync` returns null -> verify new session ID generated, verify new `Session` object returned.
* **TODO (`GetSessionByIdAsync` - Found):** Test call with valid ID -> mock repository returns session -> verify correct session returned.
* **TODO (`GetSessionByIdAsync` - Not Found):** Test call with invalid ID -> mock repository returns null -> verify null returned or exception thrown.
* **TODO:** *(Add tests for other retrieval methods like `GetSessionByUserIdAsync` if they exist)*
* **TODO (`AddConversationTurnAsync` / `UpdateSessionAsync` - Success):** Test adding data -> mock retrieval returns session, verify session state updated correctly, verify repository `SaveSessionAsync` (or similar) called with updated session.
* **TODO (`AddConversationTurnAsync` / `UpdateSessionAsync` - Session Not Found):** Test adding data to non-existent session ID -> mock retrieval returns null -> verify appropriate exception thrown or error handled.
* **TODO (`AddConversationTurnAsync` / `UpdateSessionAsync` - Repo Save Fails):** Test when mocked repository `SaveSessionAsync` throws exception -> verify exception handled/propagated.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `SessionManager` unit tests. (Gemini)

