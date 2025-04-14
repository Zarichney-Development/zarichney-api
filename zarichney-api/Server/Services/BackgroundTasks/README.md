# Module/Directory: Server/Services/BackgroundTasks

**Last Updated:** 2025-04-13

> **Parent:** [`Server/Services`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This module provides the application's infrastructure for queuing and processing tasks asynchronously in the background, separate from the web request thread.
* **Key Responsibilities:**
    * Defining an interface (`IBackgroundWorker`) for queuing work items. [cite: zarichney-api/Server/Services/BackgroundWorker.cs]
    * Providing a channel-based implementation (`BackgroundWorker`) for an in-memory background task queue. [cite: zarichney-api/Server/Services/BackgroundWorker.cs]
    * Running a hosted service (`BackgroundTaskService`) that continuously dequeues and executes work items. [cite: zarichney-api/Server/Services/BackgroundWorker.cs]
    * Ensuring that each background task executes within its own Dependency Injection scope and Session context. [cite: zarichney-api/Server/Services/BackgroundWorker.cs]
* **Why it exists:** To decouple potentially long-running or non-critical operations (like sending emails, processing data, interacting with slow external APIs) from the user-facing request lifecycle, improving API responsiveness and resilience. It also ensures background tasks have access to correctly scoped services and session information.

## 2. Architecture & Key Concepts

* **Queue Implementation:** Uses `System.Threading.Channels.Channel<BackgroundWorkItem>` within the `BackgroundWorker` class as a thread-safe, bounded in-memory queue. The `BackgroundWorkItem` record combines the work delegate with an optional parent `Session`. The capacity and behavior when full (currently `BoundedChannelFullMode.Wait`) are set during instantiation. [cite: zarichney-api/Server/Services/BackgroundWorker.cs]
* **Work Item Definition:** Background tasks are represented as delegates: `Func<IScopeContainer, CancellationToken, Task>`, optionally associated with a parent `Session`. This signature requires the task logic to accept an `IScopeContainer` (for resolving scoped services) and a `CancellationToken`.
* **Hosted Service Runner:** `BackgroundTaskService` inherits from `BackgroundService` and runs continuously for the application's lifetime. Its `ExecuteAsync` method waits for items from the `IBackgroundWorker`'s channel. [cite: zarichney-api/Server/Services/BackgroundWorker.cs]
* **Scope & Session Management:** Before executing a dequeued work item, `BackgroundTaskService` performs these steps: [cite: zarichney-api/Server/Services/BackgroundWorker.cs]
    1. Creates a new DI scope using `IScopeFactory`.
    2. Sets this scope in an `AsyncLocal` variable (`ScopeHolder.CurrentScope`) so it's available implicitly to services within the task's execution context.
    3. If a parent session was provided with the work item, adds the new scope to that existing session. Otherwise, creates a new `Session` using `ISessionManager`.
    4. Executes the work item delegate, passing the `IScopeContainer` and `CancellationToken`.
    5. Ensures the session is ended (`ISessionManager.EndSession`) and the `AsyncLocal` scope is cleared in a `finally` block.

## 3. Interface Contract & Assumptions

* **Key Public Interfaces:** `IBackgroundWorker` with its `QueueBackgroundWorkAsync` method is the primary interface for producers wanting to queue tasks.
    * `QueueBackgroundWorkAsync(Func<IScopeContainer, CancellationToken, Task> workItem, Session? parentSession = null)`: Queues a work item with an optional parent session. If provided, the background task will execute within the context of this session.
    * `DequeueAsync(CancellationToken cancellationToken)`: Returns the next work item and its associated parent session (if any).
* **Assumptions:**
    * **Registration:** Assumes `IBackgroundWorker` (usually `BackgroundWorker` as singleton) and `BackgroundTaskService` (`AddHostedService`) are correctly registered in `Program.cs`. Assumes `IScopeFactory` and `ISessionManager` are also registered (typically as singletons). [cite: zarichney-api/Program.cs, zarichney-api/Server/Services/Sessions/SessionExtensions.cs]
    * **Work Item Implementation:** Assumes the `Func` delegate provided to `QueueBackgroundWorkAsync` correctly uses the passed `IScopeContainer` to resolve any required *scoped* services (like DbContexts or services depending on `IScopeContainer`). Failure to do so can lead to incorrect service lifetimes or errors.
    * **Parent Session:** When a parent session is provided, assumes the session is still valid when the work item is processed. If the session has been ended or expired, the task will fallback to creating a new session.
    * **Channel Capacity:** Assumes the configured channel capacity is adequate. If the queue becomes full and `BoundedChannelFullMode.Wait` is used, producer threads calling `QueueBackgroundWorkAsync` will block until space is available.
    * **Application Lifetime:** Assumes the application runs long enough for queued tasks to be processed. Tasks in the queue are lost if the application shuts down unexpectedly.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Work Item Signature:** Background work must conform to the `Func<IScopeContainer, CancellationToken, Task>` delegate signature.
* **Queue Type:** Uses `System.Threading.Channels` specifically.
* **Scope/Session Integration:** Tightly coupled with the session management system defined in [`Server/Services/Sessions`](../Sessions/README.md) to provide context to background tasks.

## 5. How to Work With This Code

* **Queuing a Task:**
    1. Inject `IBackgroundWorker` into the service that needs to offload work.
    2. Define the work to be done as an `async` lambda or method matching the `Func` signature.
    3. Inside the lambda/method, use the provided `IScopeContainer` parameter (e.g., `scope.GetService<IMyScopedService>()`) to resolve necessary scoped services.
    4. Call `_worker.QueueBackgroundWorkAsync(async (scope, ct) => { /* your logic using scope */ });` for a new independent session. [cite: zarichney-api/Server/Cookbook/Recipes/RecipeRepository.cs]
    5. Alternatively, to run the task within an existing session context, pass the current session: `_worker.QueueBackgroundWorkAsync(async (scope, ct) => { /* your logic using scope */ }, existingSession);`
* **Use Cases for Parent Session:**
    * **Maintaining Context:** When background work should have access to the same conversation histories, order data, or other session-specific context as the request that initiated it.
    * **GitHub Log Continuity:** Ensures related LLM interactions are logged under the same session directory structure in GitHub.
    * **Session-Scoped Resource Sharing:** Allows background tasks to share resources (like cached data) that are scoped to the parent session.
* **Testing:**
    * **`BackgroundTaskService`:** Can be tested by mocking `IBackgroundWorker`, `IScopeFactory`, and `ISessionManager` to verify that it dequeues items, creates scopes/sessions, executes the delegate, and cleans up correctly.
    * **Consumers (Producers):** Test services that *queue* work by mocking `IBackgroundWorker` and verifying that `QueueBackgroundWorkAsync` is called with the expected delegate and parent session (when applicable).
* **Common Pitfalls / Gotchas:** 
    * Forgetting to use the `IScopeContainer` parameter inside the background task delegate to resolve scoped services. 
    * Passing an invalid or expired session as the parent session.
    * Infinite waits if the channel capacity is too small and the producer blocks. 
    * Unhandled exceptions within the work delegate (logged by `BackgroundTaskService` but won't automatically retry). 
    * Losing queued tasks on application shutdown.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`Server/Services/Sessions`](../Sessions/README.md): Relies on `ISessionManager`, `IScopeFactory`, `IScopeContainer`, `ScopeHolder`.
* **External Library Dependencies:**
    * `Microsoft.Extensions.Hosting.Abstractions`: For `IHostedService`, `BackgroundService`.
    * `System.Threading.Channels`: Core queuing mechanism.
    * `Serilog`: For logging within the `BackgroundTaskService`.
* **Dependents (Impact of Changes):**
    * Any service that injects `IBackgroundWorker` to queue tasks (e.g., [`Server/Cookbook/Recipes/RecipeRepository.cs`](../../Cookbook/Recipes/RecipeRepository.cs), [`Server/Cookbook/Orders/OrderService.cs`](../../Cookbook/Orders/OrderService.cs)).
    * `Program.cs`: Registers `IBackgroundWorker` and `BackgroundTaskService`.

## 7. Rationale & Key Historical Context

* **Responsiveness:** Chosen to prevent long-running tasks from blocking web request threads, improving API responsiveness.
* **Scope/Session Integrity:** The design explicitly ensures background tasks run with proper DI scopes and session context, preventing common issues with service lifetimes in background processing.
* **Simplicity:** Uses built-in .NET features (`BackgroundService`, `System.Threading.Channels`) providing a relatively simple in-memory queue without requiring external dependencies like RabbitMQ or Hangfire, suitable for the project's current scope.

## 8. Known Issues & TODOs

* **Durability:** Being an in-memory queue, tasks are lost if the application restarts before processing is complete. For critical tasks requiring guaranteed execution, a persistent queuing mechanism (e.g., database-backed queue, external message broker) would be needed.
* **Scalability:** A single `BackgroundTaskService` processes items sequentially (though the work *within* the delegate can be parallel). For very high throughput, multiple consumer instances or more advanced queuing patterns might be required.
* **Error Handling:** Relies on the queued delegate to handle its own errors appropriately. The service itself only logs exceptions encountered during execution. No built-in retry mechanism at the service level (retries must be implemented within the delegate or via Polly if needed).
* **Monitoring:** Limited built-in monitoring of queue depth or processing times.