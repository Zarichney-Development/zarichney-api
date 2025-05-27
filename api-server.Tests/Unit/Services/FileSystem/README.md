# Module/Directory: /Unit/Services/FileSystem

**Last Updated:** 2025-04-18

> **Parent:** [`Services`](../README.md)
> *(Note: A README for `/Unit/Services/` may be needed)*
> **Related:**
> * **Source:** [`Services/FileSystem/FileService.cs`](../../../../api-server/Services/FileSystem/FileService.cs)
> * **Dependencies:** `System.IO` classes (potentially), `ILogger<FileService>`
> * **Standards:** [`TestingStandards.md`](../../../../Zarichney.Standards/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Zarichney.Standards/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains tests for the `FileService` class. This service provides an abstraction layer for interacting with the underlying file system, encapsulating operations like reading, writing, and deleting files.

* **Why Test `FileService`?** To validate the logic for constructing paths, handling streams or file contents, creating directories, and managing potential file system exceptions correctly.
* **Testing Approach & Challenges:** Unit testing classes that directly use `System.IO` static methods (`File`, `Directory`, `Path`) is challenging because these cannot be easily mocked. Therefore, tests in this directory likely adopt a pragmatic **integration-like unit testing** approach. This means they interact with the **actual file system** but confine operations to temporary directories created during test setup and removed during cleanup. This verifies the service's behavior against real file system constraints but deviates from pure unit testing isolation.
* **Refactoring Note:** For true, isolated unit testing, consider refactoring `FileService` to depend on an abstraction like `System.IO.Abstractions.IFileSystem`, which can be fully mocked.

## 2. Scope & Key Functionality Tested (What?)

Tests focus on the public methods exposed by `FileService`:

* **`WriteToFileAsync(string path, string content)` / `WriteToFileAsync(string path, Stream stream)`:**
    * Creating new files with specified content.
    * Overwriting existing files.
    * Correctly handling file paths and directory creation (e.g., via `EnsureDirectoryExists`).
    * Managing stream reading/writing.
* **`ReadFromFileAsync(string path)` / `ReadFileStreamAsync(string path)`:**
    * Reading content from existing files.
    * Handling `FileNotFoundException` when the file doesn't exist.
* **`DeleteFile(string path)`:**
    * Successfully deleting an existing file.
    * Handling attempts to delete a non-existent file gracefully (e.g., not throwing an error).
* **`FileExists(string path)`:**
    * Correctly reporting the existence or non-existence of files.
* **`EnsureDirectoryExists(string path)`:**
    * Creating a directory if it doesn't exist.
    * Handling cases where the directory already exists.

## 3. Test Environment Setup

* **Instantiation:** `FileService` is instantiated directly.
* **Mocking:** `ILogger<FileService>` is mocked. Direct `System.IO` calls are *not* typically mocked in this approach.
* **Temporary File System:** Tests **must** use temporary directories and files.
    * **Setup:** Create a unique temporary root directory for each test class or test method (e.g., using `Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())`). Use `Directory.CreateDirectory`.
    * **Execution:** All `FileService` operations under test should target paths within this temporary directory.
    * **Cleanup:** Use `try/finally` blocks, `IDisposable` fixtures (`IAsyncLifetime`, `IClassFixture`), or test framework cleanup attributes to **reliably delete** the temporary directory and its contents after each test or test class runs, even if assertions fail.
* **Test Data:** Prepare sample strings or `MemoryStream` objects for writing content.

## 4. Maintenance Notes & Troubleshooting

* **Cleanup is Critical:** Failure to properly clean up temporary files/directories can lead to test failures on subsequent runs and disk space issues. Ensure cleanup logic is robust.
* **Platform Differences:** Be mindful of path separator differences (`/` vs `\`) if not using `Path.Combine` consistently. File system behavior (e.g., permissions, case sensitivity) can vary slightly across platforms (Windows, Linux, macOS).
* **Concurrency:** Avoid running tests in parallel if they might interfere with each other's temporary file system operations, unless each test uses a completely isolated temporary directory.
* **Permissions:** Tests might fail if the test runner process lacks permissions to create/write/delete files in the chosen temporary location.
* **Slow Tests:** These tests will be slower than pure unit tests due to real I/O operations. Mark them with appropriate traits if needed.

## 5. Test Cases & TODOs

### `FileServiceTests.cs`
*(Assume `_tempDirPath` is the path to the unique temporary directory for the test)*

* **TODO (`WriteToFileAsync` - string):** Test writing content to a new file -> verify file exists at `Path.Combine(_tempDirPath, "test.txt")`, verify file content matches.
* **TODO (`WriteToFileAsync` - string):** Test overwriting an existing file -> write once, write again with different content, verify final content matches the second write.
* **TODO (`WriteToFileAsync` - stream):** Test writing from a `MemoryStream` -> verify file exists and content matches stream.
* **TODO (`WriteToFileAsync`):** Test creating necessary subdirectories via `EnsureDirectoryExists` when path includes them.
* **TODO (`ReadFromFileAsync`):** Test reading content from an existing file created during setup -> verify returned string matches expected content.
* **TODO (`ReadFromFileAsync`):** Test reading a non-existent file -> verify `FileNotFoundException` is thrown.
* **TODO (`ReadFileStreamAsync`):** Test reading an existing file -> verify returned stream is readable and content matches.
* **TODO (`ReadFileStreamAsync`):** Test reading a non-existent file -> verify `FileNotFoundException` is thrown.
* **TODO (`DeleteFile`):** Test deleting an existing file -> create file, delete it, verify `File.Exists` returns false.
* **TODO (`DeleteFile`):** Test deleting a non-existent file -> verify no exception is thrown.
* **TODO (`FileExists`):** Test returns true for an existing file.
* **TODO (`FileExists`):** Test returns false for a non-existent file.
* **TODO (`EnsureDirectoryExists`):** Test creating a new directory -> verify `Directory.Exists` returns true.
* **TODO (`EnsureDirectoryExists`):** Test calling on an already existing directory -> verify no exception is thrown and directory still exists.

## 6. Changelog

* **2025-04-18:** Initial creation - Defined purpose, scope, setup (using temporary files), and TODOs for `FileService` tests. Added note on `System.IO.Abstractions`. (Gemini)

