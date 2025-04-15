# Module/Directory: /Services/Sessions

**Last Updated:** 2025-04-13

> **Parent:** [`/Services`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This module provides infrastructure for managing user and request sessions, maintaining state across multiple interactions, and ensuring operations (including background tasks) execute within correctly scoped contexts.
* **Key Responsibilities:**
    * Defining the `Session` object, which holds state associated with a user interaction (e.g., associated `CookbookOrder`, LLM conversation history, UserID/APIKey, timing information). [cite: api-server/Services/Sessions/SessionModels.cs]
    * Providing a central manager (`ISessionManager` / `SessionManager`) for creating, retrieving, updating, and ending sessions (currently stored in memory). [cite: api-server/Services/Sessions/SessionManager.cs]
    * Implementing middleware (`SessionMiddleware`) to automatically associate incoming HTTP requests with existing or new sessions based on authentication status or other identifiers (like order ID). [cite: api-server/Services/Sessions/SessionMiddleware.cs]
    * Defining and managing Dependency Injection scopes (`IScopeContainer`, `IScopeFactory`) linked to sessions, ensuring services resolved within a request or background task have the correct lifetime and access to session context. [cite: api-server/Services/Sessions/SessionModels.cs]
    * Providing a mechanism (`ScopeHolder`) to flow scope/session context using `AsyncLocal`, especially for background tasks initiated by the [`BackgroundTasks`](../BackgroundTasks/README.md) module. [cite: api-server/Services/Sessions/SessionModels.cs, api-server/Services/BackgroundWorker.cs]
    * Running a background service (`SessionCleanupService`) to periodically remove expired or inactive sessions from memory. [cite: api-server/Services/Sessions/SessionCleanup.cs]
    * Providing extension methods (`SessionExtensions`) for easy registration of services and middleware in `Program.cs`. [cite: api-server/Services/Sessions/SessionExtensions.cs]
* **Why it exists:** To handle state management in a stateless application architecture, allowing correlation of multiple requests or background tasks to a single user interaction or workflow (like a cookbook order). It ensures correct DI scope handling, particularly for background tasks needing scoped services (e.g., DbContexts).

## 2. Architecture & Key Concepts

* **In-Memory Session Store:** `SessionManager` uses a `ConcurrentDictionary<Guid, Session>` to hold active `Session` objects in memory. Session state (excluding data explicitly persisted elsewhere, like saved orders) is lost on application restart. [cite: api-server/Services/Sessions/SessionManager.cs]
* **Session Middleware (`SessionMiddleware`):** Intercepts incoming requests. It attempts to identify an existing session based on authenticated user ID (`ClaimTypes.NameIdentifier`), API Key (`X-Api-Key` via `HttpContext.Items`), or potentially an Order ID. If no existing session is found, it creates a new one via `ISessionManager`. It sets the `IScopeContainer` feature on the `HttpContext` and ensures sessions are potentially ended after the request completes. [cite: api-server/Services/Sessions/SessionMiddleware.cs]
* **Scope Management (`IScopeContainer`, `IScopeFactory`, `ScopeHolder`):**
    * `IScopeFactory` creates new DI scopes.
    * `IScopeContainer` holds the unique `Id` for the current scope and the associated `SessionId`. It's registered with a scoped lifetime in DI.
    * `ScopeHolder` uses `AsyncLocal<IScopeContainer>` to make the current scope available implicitly, which is essential for background tasks that don't have direct access to the `HttpContext`. [cite: api-server/Services/Sessions/SessionModels.cs]
* **Session Lifecycle:** Sessions are created by `SessionMiddleware` or `SessionManager.CreateSession`. They are refreshed on access (`RefreshSession` method). `SessionManager.EndSession` removes the session from memory and triggers persistence logic (e.g., saving associated `CookbookOrder` via `IOrderRepository` and `LlmConversation` via `ILlmRepository`). `SessionCleanupService` periodically removes sessions that have passed their `ExpiresAt` time. [cite: api-server/Services/Sessions/SessionManager.cs, api-server/Services/Sessions/SessionCleanup.cs]
* **Reusable Anonymous Sessions:** Provides a mechanism (`FindReusableAnonymousSession`) to find existing anonymous sessions (those without a `UserId` or `ApiKeyValue`) that are not set to expire immediately. This enables background tasks to reuse existing sessions rather than always creating new ones, which is particularly useful for maintaining context across related tasks and ensuring related LLM conversations are logged under the same session structure. [cite: api-server/Services/Sessions/SessionManager.cs]
* **Configuration:** Uses `SessionConfig` for settings like default session duration, cleanup interval, and maximum concurrent cleanup tasks. [cite: api-server/Services/Sessions/SessionModels.cs, api-server/appsettings.json]

## 3. Interface Contract & Assumptions

* **Key Public Interfaces:** `ISessionManager`, `IScopeContainer`, `IScopeFactory`.
  * **New Method:** `FindReusableAnonymousSession()` returns an existing anonymous session (with no `UserId` or `ApiKeyValue`) that is not set to expire immediately, or `null` if none is found. This allows background tasks to reuse existing sessions rather than always creating new ones.
* **Assumptions:**
    * **Middleware Order:** Assumes `SessionMiddleware` (`UseSessionManagement`) is registered *after* Authentication middleware in `Program.cs` so that user identity is available when the session middleware runs. [cite: api-server/Program.cs]
    * **DI Registration:** Assumes all services (`ISessionManager`, `IScopeFactory`, `SessionCleanupService`, etc.) are correctly registered with appropriate lifetimes in `Program.cs` via `AddSessionManagement`. [cite: api-server/Services/Sessions/SessionExtensions.cs]
    * **Configuration:** Assumes `SessionConfig` is correctly configured.
    * **Persistence on EndSession:** Assumes that the `EndSession` logic in `SessionManager` correctly identifies and persists necessary related data (like Orders, Conversations) via injected repositories (`IOrderRepository`, `ILlmRepository`). The session data *itself* (besides the persisted related data) is lost. [cite: api-server/Services/Sessions/SessionManager.cs]
    * **Scope Usage:** Assumes consumers (especially background tasks) correctly use the provided `IScopeContainer` to resolve scoped dependencies.
    * **Reusable Sessions:** When reusing an anonymous session, assumes callers are aware that any existing state in that session (such as conversation history) will be accessible to the new task.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Storage:** Sessions are stored entirely in memory within the `SessionManager`'s `ConcurrentDictionary`. No external distributed cache or database is used for session state itself.
* **Context Propagation:** Relies on `AsyncLocal` via `ScopeHolder` for propagating scope context, particularly to background tasks.
* **Cleanup:** Uses a timer-based `IHostedService` (`SessionCleanupService`) for cleanup, not real-time eviction.

## 5. How to Work With This Code

* **Accessing Session Data:** In services needing session context (usually registered as Scoped or Transient):
    1. Inject `IScopeContainer`.
    2. Get the `SessionId` from `scopeContainer.SessionId`.
    3. Inject `ISessionManager`.
    4. Call `await sessionManager.GetSession(sessionId.Value)` (handle potential null `SessionId`).
    5. Access/modify properties on the returned `Session` object.
* **Finding Reusable Anonymous Sessions:** For background tasks that can work within an existing anonymous session:
    1. Inject `ISessionManager`.
    2. Call `sessionManager.FindReusableAnonymousSession()` to locate an existing anonymous session.
    3. If a session is found, use `sessionManager.AddScopeToSession(scopeId, session.Id)` to add the current scope to it.
    4. If no session is found, fall back to creating a new session with `await sessionManager.CreateSession(scopeId)`.
    5. This approach is particularly useful for background tasks that process related data and should share conversation context or GitHub logging paths.
* **Adding Session State:** Add new properties to the `Session` class in `SessionModels.cs`. Implement logic in `SessionManager.EndSession` if this new state needs to be persisted when the session ends.
* **Testing:**
    * Mock `ISessionManager`, `IScopeContainer`, `IScopeFactory`.
    * Test `SessionMiddleware` requires mocking `HttpContext`, `RequestDelegate`, and related services.
    * Test `SessionCleanupService` by mocking `ISessionManager` and verifying cleanup logic.
* **Common Pitfalls / Gotchas:** Accessing `IScopeContainer` from a Singleton service (will likely get an empty/incorrect scope). Forgetting to handle null `SessionId`. Background tasks failing if `ScopeHolder` context isn't properly set up by the task runner (like `BackgroundTaskService`). Assuming session state persists across application restarts. Memory consumption if a very large number of sessions remain active. When reusing anonymous sessions, be aware that existing conversation history will be shared across tasks.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/Config`](../../Config/README.md): Consumes `SessionConfig`.
    * [`/Auth`](../Auth/README.md): `SessionMiddleware` interacts with `HttpContext.User` populated by Auth middleware.
    * [`/Cookbook/Orders`](../../Cookbook/Orders/README.md): Uses `CookbookOrder` model, `IOrderRepository`.
    * [`/Cookbook/Customers`](../../Cookbook/Customers/README.md): Uses `ICustomerRepository`.
    * [`/Services/AI`](../AI/README.md): Uses `LlmConversation` model, `ILlmRepository`.
* **External Library Dependencies:**
    * `Microsoft.AspNetCore.Http.Abstractions`: For `HttpContext`, `RequestDelegate`.
    * `Microsoft.Extensions.Hosting.Abstractions`: For `IHostedService`.
    * `System.Collections.Concurrent`: For `ConcurrentDictionary`.
    * `Serilog`: For logging.
* **Dependents (Impact of Changes):**
    * [`/Services/BackgroundTasks`](../BackgroundTasks/README.md): `BackgroundTaskService` relies heavily on `IScopeFactory`, `ISessionManager`, `ScopeHolder`.
    * Services that require session state or scoped execution context (e.g., `LlmService`, `OrderService`, `RecipeRepository`).
    * Middleware pipeline in `Program.cs` (`UseSessionManagement`).

## 7. Rationale & Key Historical Context

* **State Management Need:** Required to manage state across requests and link background tasks to specific user interactions or workflows, particularly for the multi-step cookbook generation process.
* **Scope Management:** The `IScopeContainer`/`ScopeHolder` mechanism was introduced to solve the common problem of accessing correctly scoped services (like `DbContext`) from background threads which don't have an `HttpContext`.
* **In-Memory Choice:** An in-memory dictionary was likely chosen for simplicity and performance for the expected scale, with the understanding that critical associated data (like completed orders) would be persisted separately.

## 8. Known Issues & TODOs

* **Persistence:** Session state is volatile and lost on application restart. Critical workflow state must be persisted externally (as is done for Orders). For higher availability or scale, a distributed session store (e.g., Redis, database) might be necessary.
* **Memory Usage:** A very large number of concurrent active sessions could lead to high memory consumption. The effectiveness of the `SessionCleanupService` is important.
* **Cleanup Granularity:** Cleanup is timer-based; sessions might persist slightly longer than their theoretical expiry time.