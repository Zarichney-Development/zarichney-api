# Module/Directory: /Unit/Services/GitHub

**Last Updated:** 2025-05-15

> **Parent:** [`Services`](../README.md)
> *(Note: A README for `/Unit/Services/` may be needed)*
> **Related:**
> * **Source:** [`Services/GitHub/GitHubService.cs`](../../../../api-server/Services/GitHub/GitHubService.cs)
> * **Dependencies:** `Octokit.IGitHubClient` (or `GitHubClient`), `IOptions<GitHubSettings>`, `ILogger<GitHubService>`
> * **Models:** [`Config/ConfigModels.cs`](../../../../api-server/Config/ConfigModels.cs) (for `GitHubSettings`)
> * **Standards:** [`TestingStandards.md`](../../../../Zarichney.Standards/Standards/TestingStandards.md), [`DocumentationStandards.md`](../../../../Zarichney.Standards/Development/DocumentationStandards.md)

## 1. Purpose & Rationale (Why?)

This directory contains unit tests for the `GitHubService` class. This service encapsulates logic for interacting with the GitHub API, likely using the Octokit.net library, to perform application-specific tasks such as creating issues or queuing files/data by creating or updating files within a designated repository.

* **Why Unit Tests?** To validate the internal logic of `GitHubService` methods in isolation from the actual GitHub API. Tests ensure the service correctly uses configuration settings, constructs requests for the (mocked) Octokit client, calls the appropriate client methods, and handles success or failure responses/exceptions from the client.
* **Isolation:** Achieved by mocking the `Octokit.IGitHubClient` (or its concrete implementation if necessary), `IOptions<GitHubSettings>`, and `ILogger`.

## 2. Scope & Key Functionality Tested (What?)

Unit tests for `GitHubService` focus on its public methods, such as:

* **`EnqueueCommitAsync(string filePath, byte[] content, string targetDirectory, string commitMessage)` method:**
    * Retrieving repository details (owner, name, branch) from mocked `GitHubSettings`.
    * Constructing parameters for Octokit API calls (e.g., `CreateFileRequest`, `UpdateFileRequest`).
    * Correctly invoking methods on the mocked `IGitHubClient` (e.g., `Repository.Content.CreateFile`, `Repository.Content.UpdateFile`, potentially `GetContents` first to check existence/get SHA).
    * Handling successful responses from the mocked Octokit client.
    * Handling specific Octokit exceptions (e.g., `NotFoundException`, `ApiValidationException`) returned by the mocked client.

* **`StoreAudioAndTranscriptAsync(string audioFileName, byte[] audioData, string transcriptFileName, string transcriptText)` method:**
    * Correctly calling `EnqueueCommitAsync` twice in parallel with `Task.WhenAll`.
    * Verifying audio file is stored in the "recordings" directory.
    * Verifying transcript file is stored in the "transcripts" directory.
    * Proper logging for both success and failure scenarios.
    * Error handling if any of the storage operations fail.
    
* **Other potential methods:** (e.g., creating issues via `client.Issue.Create`) would be tested similarly.

## 3. Test Environment Setup

* **Instantiation:** `GitHubService` is instantiated directly in test methods.
* **Mocking:** Dependencies are mocked using frameworks like Moq. Key mocks include:
    * `Mock<Octokit.IGitHubClient>`: Requires setting up mocks for specific client properties (like `Repository.Content` or `Issue`) and their methods (`CreateFile`, `UpdateFile`, `GetContents`, `Create`) to return expected responses (e.g., `RepositoryContentChangeSet`, `Issue`) or throw Octokit-specific exceptions.
    * `Mock<IOptions<GitHubSettings>>`: Provide mock `GitHubSettings` values (repo owner, name, branch, credentials/token - though token usage is usually internal to the client).
    * `Mock<ILogger<GitHubService>>`.

## 4. Maintenance Notes & Troubleshooting

* **Octokit Mocking:** Mocking the fluent interface of Octokit (`client.Repository.Content.CreateFile(...)`) requires careful setup. Ensure mocks accurately simulate the expected return types and exceptions for the methods being called. Refer to Octokit.net documentation for details.
* **Settings:** Ensure the mocked `GitHubSettings` provide all necessary configuration used by the service methods under test.
* **API Changes:** Changes in the GitHub API or the Octokit.net library might require updating tests and mock setups.

## 5. Test Cases & TODOs

### `GitHubServiceTests.cs`

* **TODO (`EnqueueCommitAsync` - Create):** Test scenario where file does not exist.
    * Verify `GitHubSettings` accessed.
    * Verify `client.Repository.Content.CreateFile` called with correct owner, repo, path, content, message, branch.
    * Mock `CreateFile` success -> verify method returns success/expected result.
* **TODO (`EnqueueCommitAsync` - Update):** Test scenario where file already exists (if update logic is implemented).
    * Verify `client.Repository.Content.GetContents` called first (potentially).
    * Verify `client.Repository.Content.UpdateFile` called with correct owner, repo, path, content, message, SHA, branch.
    * Mock `UpdateFile` success -> verify method returns success/expected result.
* **TODO (`EnqueueCommitAsync` - Repo Not Found):** Test when Octokit mock throws `NotFoundException` -> verify exception handled/logged appropriately, verify failure result returned.
* **TODO (`EnqueueCommitAsync` - Validation Error):** Test when Octokit mock throws `ApiValidationException` -> verify exception handled/logged, verify failure result returned.
* **TODO (`EnqueueCommitAsync` - Other Exception):** Test when Octokit mock throws generic exception -> verify exception handled/logged, verify failure result returned.

* **TODO (`StoreAudioAndTranscriptAsync` - Success):** Test successful parallel storage of audio and transcript.
    * Verify `EnqueueCommitAsync` called twice with correct parameters.
    * Verify `Task.WhenAll` used to run operations in parallel.
    * Verify appropriate success logging.
* **TODO (`StoreAudioAndTranscriptAsync` - Handle Exception):** Test exception handling during file storage.
    * Mock `EnqueueCommitAsync` to throw an exception.
    * Verify exception is handled properly and propagated to caller.
    * Verify error is logged appropriately.

* **TODO:** *(Add tests for other public methods, e.g., creating issues)*

## 6. Changelog

* **2025-05-15:** Updated to include documentation for the new `StoreAudioAndTranscriptAsync` method.
* **2025-04-18:** Initial creation - Defined purpose, scope, setup, and TODOs for `GitHubService` unit tests. (Gemini)

