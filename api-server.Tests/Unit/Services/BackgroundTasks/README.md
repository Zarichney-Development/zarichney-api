# Module/Directory: /Unit/Services/BackgroundTasks

**Last Updated:** 2025-04-18

> **Parent:** [`Services`](../README.md)
> *(Note: A README for `/Unit/Services/` may be needed)*
> **Related:**
> * **Source:** [`Services/BackgroundTasks/BackgroundWorker.cs`](../../../../api-server/Services/BackgroundTasks/BackgroundWorker.cs)
> * **Dependencies:** `Microsoft.Extensions.Hosting.IHostedService`, `Microsoft.Extensions.DependencyInjection.IServiceScopeFactory`, `Microsoft.Extensions.Logging.ILogger`
> * **Standards:** [`TestingStandards.md`](../../../../Docs/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Docs/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the background task processing infrastructure, primarily focusing on the `BackgroundWorker` hosted service. These tests validate the core logic responsible for queuing, dequeuing, and executing background work items reliably.

* **Why Unit Tests?** To verify the internal mechanics of the `BackgroundWorker` in isolation from the rest of the application and actual task execution complexity. This includes testing the queuing mechanism, the background processing loop (`ExecuteAsync`), service scope creation for tasks, error handling during task execution, and logging.
* **Isolation:** Achieved by mocking dependencies such as the `IServiceScopeFactory` (and the `IServiceProvider` / `IServiceScope` it creates), the `ILogger`, and potentially an explicit `IBackgroundTaskQueue` if one is used internally instead of a simple `Channel` or `BlockingCollection`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `BackgroundWorker` focus on:

* **`QueueWorkItemAsync` Method:**
    * Verifying that work items (`Func<IServiceProvider, CancellationToken, Task>`) are correctly added to the internal queue.
* **`ExecuteAsync` Method (The `IHostedService` background loop):**
    * Correctly dequeuing work items when they become available.
    * Handling an empty queue (waiting appropriately).
    * Creating a new DI scope (`IServiceScope`) for each dequeued work item using the mocked `IServiceScopeFactory`.
    * Retrieving services needed by the task from the scoped `IServiceProvider` (simulated via mock setup).
    * Executing the work item delegate correctly within the scope.
    * Catching exceptions thrown by the work item delegate.
    * Logging relevant information (task start, completion, errors) via the mocked `ILogger`.
    * Respecting the `CancellationToken` passed to `ExecuteAsync` for graceful shutdown.

## 3. Test Environment Setup

* **Instantiation:** `BackgroundWorker` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<IServiceScopeFactory>`
    * `Mock<IServiceScope>`
    * `Mock<IServiceProvider>` (returned by the mocked scope)
    * `Mock<ILogger<BackgroundWorker>>`
    * `Mock<IBackgroundTaskQueue>` (if applicable, otherwise test internal queue directly).
* **Task Simulation:** Test work items are typically simple `Func` delegates provided during the test setup, some designed to succeed quickly and others designed to throw specific exceptions.
* **Concurrency/Async:** Testing the `ExecuteAsync` loop often requires careful use of `Task.Run`, `Task.Delay`, `CancellationTokenSource`, and potentially synchronization primitives (`ManualResetEventSlim`, etc.) to control the flow and observe behavior without race conditions or deadlocks in the test.

## 4. Maintenance Notes & Troubleshooting

* **Async Loop Testing:** Unit testing asynchronous loops and `IHostedService` implementations can be complex. Ensure tests properly manage cancellation and wait for expected actions (like dequeuing or logging) to occur. Use timeouts to prevent tests from hanging indefinitely.
* **Scope Management:** Verify that `IServiceScope` is correctly created *and disposed* for each work item to prevent resource leaks (verify `Dispose()` called on the mock scope).
* **Queue Implementation:** If the internal queue mechanism changes (e.g., from `Channel<T>` to something else), tests might need significant updates.

## 5. Test Cases & TODOs

### `BackgroundWorkerTests.cs`
* **TODO:** Test `QueueWorkItemAsync` successfully adds a work item to the internal queue.
* **TODO:** Test `ExecuteAsync` dequeues and executes a successfully queued work item.
    * Verify `IServiceScopeFactory.CreateScope` is called.
    * Verify the work item delegate is invoked with the scoped `IServiceProvider`.
    * Verify `ILogger` logs start/completion messages.
    * Verify `IServiceScope.Dispose` is called.
* **TODO:** Test `ExecuteAsync` handles an exception thrown by the work item delegate.
    * Verify the exception is caught.
    * Verify `ILogger` logs an error message with exception details.
    * Verify the loop continues to process subsequent items (if applicable).
    * Verify `IServiceScope.Dispose` is still called.
* **TODO:** Test `ExecuteAsync` waits correctly when the queue is empty.
* **TODO:** Test `ExecuteAsync` stops processing when the `CancellationToken` is cancelled.
    * Verify the loop exits gracefully.
    * Verify already executing tasks are allowed to finish or handle cancellation if designed to.
* **TODO:** Test scenario with multiple items queued and processed sequentially.
* **TODO:** Test correct services can be resolved from the scoped `IServiceProvider` passed to the work item (requires setting up the mock `IServiceProvider`).

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `BackgroundWorker` unit tests. (Gemini)

