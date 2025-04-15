# Module/Directory: /Services/FileSystem

**Last Updated:** 2025-04-03

> **Parent:** [`/Services`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This module provides a robust abstraction layer for interacting with the local file system.
* **Key Responsibilities:**
    * Defining an interface (`IFileService`) for common file operations (read, write, check existence, delete, list files, create specific file types). [cite: api-server/Services/FileService.cs]
    * Providing a concrete implementation (`FileService`) that handles file I/O. [cite: api-server/Services/FileService.cs]
    * Managing concurrency for file write operations using a background queue to prevent race conditions. [cite: api-server/Services/FileService.cs]
    * Implementing retry logic (using Polly) for file read operations to handle transient access issues. [cite: api-server/Services/FileService.cs]
    * Handling serialization/deserialization for JSON files and raw text/byte handling for other formats (e.g., `.md`, `.txt`, `.pdf`, `.jpg`). [cite: api-server/Services/FileService.cs]
    * Providing filename sanitization utilities. [cite: api-server/Services/FileService.cs]
* **Why it exists:** To centralize file system interactions, abstract away direct `System.IO` calls, provide consistent error handling and retry mechanisms, and manage write concurrency safely.

## 2. Architecture & Key Concepts

* **Interface/Implementation:** Defines `IFileService` and provides the `FileService` implementation, registered typically as a Singleton. [cite: api-server/Services/FileService.cs, api-server/Program.cs]
* **Asynchronous Write Queue:** `FileService` uses a `ConcurrentQueue<WriteOperation>` and a background `Task` (`_processQueueTask`) started in its constructor. Calls to `WriteToFileAsync` enqueue a `WriteOperation` which is processed sequentially by the background task, ensuring writes to potentially the same file path are serialized. [cite: api-server/Services/FileService.cs]
* **Synchronous Write:** `WriteToFile` performs the write operation immediately on the calling thread but still leverages the internal `PerformWriteOperationAsync` logic. It uses a `ConcurrentDictionary` (`_pendingWrites`) to signal completion, ensuring subsequent reads wait if necessary. [cite: api-server/Services/FileService.cs]
* **Read Operation Synchronization:** Before reading a file (`ReadFromFile<T>`), the service checks the `_pendingWrites` dictionary. If a write operation is in progress for that specific file path, the read operation `await`s its completion `TaskCompletionSource` to ensure data consistency. [cite: api-server/Services/FileService.cs]
* **Read Retries:** Uses a Polly `AsyncRetryPolicy` (`_retryPolicy`) to automatically retry file read operations (`LoadExistingData`) several times with delays upon encountering exceptions (like `IOException`), enhancing resilience. [cite: api-server/Services/FileService.cs]
* **File Handling:** Supports reading/writing JSON (using `System.Text.Json`), plain text (`.md`, `.txt`), PDF (`byte[]`), and creating JPEGs (using `SixLabors.ImageSharp`). [cite: api-server/Services/FileService.cs]
* **Filename Sanitization:** Includes a static `SanitizeFileName` method to replace invalid path characters and spaces with underscores. [cite: api-server/Services/FileService.cs]
* **Resource Management:** Implements `IDisposable` to gracefully shut down the background processing task on application disposal. [cite: api-server/Services/FileService.cs]

## 3. Interface Contract & Assumptions

* **Key Public Interfaces:** `IFileService` defines the available file operations.
    * `WriteToFileAsync(dir, filename, data, ext)`: Enqueues a write operation. Returns immediately; completion happens in the background. Fire-and-forget style persistence.
    * `WriteToFile(dir, filename, data, ext)`: Performs write operation and waits for completion before returning.
    * `ReadFromFile<T>(dir, filename, ext)`: Reads and potentially deserializes file content. Waits for any pending writes on the same path to complete first. Retries on transient errors.
    * `FileExists(path)`, `DeleteFile(path)`, `GetFiles(dirPath)`, `GetFile(path)`, `GetFileAsync(path)`, `CreateFile(path, data, type)`: Standard file utility operations with some built-in retry/safety for deletion.
* **Assumptions:**
    * **Permissions:** Assumes the application process has the necessary read/write permissions for the directories specified in method calls.
    * **File System Reliability:** Assumes the underlying file system is generally available and reliable.
    * **Path Validity:** Assumes paths provided by consumers are valid for the operating system.
    * **Data Types:** Assumes the `data` provided to write methods is compatible with the specified `extension` (e.g., `byte[]` for `pdf`, serializable object for `json`). Assumes the type `T` in `ReadFromFile<T>` matches the content of the file being read (especially for JSON).
    * **Concurrency Model:** Relies on the internal queue and dictionary mechanism to handle concurrent access correctly.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Async Write Behavior:** `WriteToFileAsync` is non-blocking; callers should not assume the write is complete immediately after the call returns.
* **Read-Write Synchronization:** Reading operations (`ReadFromFile`) are designed to wait for pending writes queued via *either* `WriteToFile` or `WriteToFileAsync` for the *same specific file path*.
* **Error Handling:** Read operations incorporate Polly retries. Write operations log errors encountered in the background queue. Deletion uses limited retries for specific exceptions.
* **Supported Formats:** Explicit handling for JSON, text-like (`md`, `txt`), PDF (`byte[]`), and JPEG creation. Adding support for other formats would require modifying `FileService`.

## 5. How to Work With This Code

* **Consumption:** Inject `IFileService` into services or repositories that require file system access.
* **Choosing Write Method:** Use `WriteToFileAsync` for performance when immediate confirmation isn't needed (e.g., saving logs, non-critical data). Use `WriteToFile` when subsequent operations depend on the file being written successfully.
* **Testing:**
    * Mocking `IFileService` is the standard approach for unit testing consumers.
    * Testing `FileService` itself typically requires mocking the file system (complex) or using integration tests that write to temporary directories and verify file contents/existence, potentially testing concurrency scenarios.
* **Common Pitfalls / Gotchas:** Misunderstanding the asynchronous nature of `WriteToFileAsync`. Providing incorrect paths or data types. Filesystem permission errors. Background queue growing very large if disk I/O is consistently slow, potentially consuming significant memory. `ReadFromFile` blocking unexpectedly if a write to the same file is stuck in the queue.

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`/Config`](../../Config/README.md) - Indirectly, if configuration were used for paths, but currently appears self-contained.
* **External Library Dependencies:**
    * `Polly`: For retry logic on read operations.
    * `System.Text.Json`: For JSON serialization/deserialization.
    * `System.Collections.Concurrent`: For `ConcurrentQueue` and `ConcurrentDictionary`.
    * `SixLabors.ImageSharp`: Used by `CreateFile` for saving JPEG images.
    * `Serilog`: Used for logging within the service.
* **Dependents (Impact of Changes):**
    * [`/Cookbook/Recipes/RecipeRepository.cs`](../../Cookbook/Recipes/RecipeRepository.cs)
    * [`/Cookbook/Orders/OrderRepository.cs`](../../Cookbook/Orders/OrderRepository.cs)
    * [`/Cookbook/Customers/CustomerRepository.cs`](../../Cookbook/Customers/CustomerRepository.cs)
    * [`/Services/Email/TemplateService.cs`](../Email/TemplateService.cs)
    * [`/Services/PdfGeneration/PdfCompiler.cs`](../PdfGeneration/PdfCompiler.cs)
    * Any other module performing direct file I/O through this service.
    * `Program.cs`: Registers `IFileService`.

## 7. Rationale & Key Historical Context

* **Abstraction:** Provides a clean interface over raw `System.IO` operations, simplifying consuming code.
* **Concurrency Management:** The background write queue and read synchronization were implemented to prevent data corruption that could occur with naive concurrent file writes from multiple threads or background tasks.
* **Resilience:** Polly retries were added to `ReadFromFile` to handle transient file locking or access issues more gracefully.
* **Centralization:** Consolidates file handling logic, including format-specific operations (JSON, PDF) and utilities (sanitization), into one place.

## 8. Known Issues & TODOs

* **Queue Durability:** The background write queue (`WriteToFileAsync`) is in-memory; queued writes are lost if the application terminates unexpectedly before they are processed. A persistent queue mechanism would be needed for guaranteed writes.
* **Performance Benchmarking:** The performance characteristics under very high concurrent write/read loads have not been formally benchmarked.
* **Filename Sanitization:** The `SanitizeFileName` method provides basic sanitization; edge cases for different filesystems or extremely long/complex input names might exist.
* **Error Propagation:** Errors occurring during background writes (`WriteToFileAsync`) are logged but not directly propagated back to the original caller.