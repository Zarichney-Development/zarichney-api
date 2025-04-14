# Module/Directory: Server/Services/GitHub

**Last Updated:** 2025-04-14

> **Parent:** [`Server/Services`](../README.md)

## 1. Purpose & Responsibility

* **What it is:** This module encapsulates interactions with the GitHub API, specifically focusing on programmatically committing files to a designated repository.
* **Key Responsibilities:**
    * Defining an interface (`IGitHubService`) for queuing file commit operations. [cite: zarichney-api/Server/Services/GitHubService.cs]
    * Providing an implementation (`GitHubService`) that runs as a background service (`IHostedService`) to process these commits asynchronously. [cite: zarichney-api/Server/Services/GitHubService.cs]
    * Authenticating with the GitHub API using a Personal Access Token (PAT).
    * Handling the Git workflow for adding/updating a single file via the API (create blob, create/update tree, create commit, update branch reference).
    * Managing a queue (`System.Threading.Channels.Channel`) for pending commit operations (`GitHubOperation`).
    * Implementing retry logic (using Polly) for transient GitHub API errors like rate limiting.
    * Defining configuration (`GitHubConfig`) for repository details and authentication. [cite: zarichney-api/Server/Services/GitHubService.cs]
* **Why it exists:** To abstract the details of GitHub API interaction for file commits, providing a simple asynchronous interface for other services (like `LlmRepository`) that need to persist data to a Git repository. Running as a background service prevents blocking calling threads during potentially slow API interactions.

## 2. Architecture & Key Concepts

* **Background Service:** `GitHubService` inherits `BackgroundService` and runs for the lifetime of the application. Its `ExecuteAsync` method continuously reads from an internal channel queue. [cite: zarichney-api/Server/Services/GitHubService.cs]
* **Asynchronous Queue:** Uses an unbounded `System.Threading.Channels.Channel<GitHubOperation>` to manage pending commit requests. `EnqueueCommitAsync` writes to the channel, and `ExecuteAsync` reads from it. [cite: zarichney-api/Server/Services/GitHubService.cs]
* **Commit Operation Task Completion:** `EnqueueCommitAsync` returns a `Task` derived from a `TaskCompletionSource` within the `GitHubOperation`. This task only completes (or throws an exception) after the background service has finished processing that specific commit attempt, allowing callers to `await` the actual completion of the GitHub operation. [cite: zarichney-api/Server/Services/GitHubService.cs]
* **GitHub API Client:** Uses the `Octokit` .NET library to interact with the GitHub REST API. A `GitHubClient` instance is initialized with credentials from `GitHubConfig`. [cite: zarichney-api/Server/Services/GitHubService.cs]
* **Git Workflow via API:** The `ProcessGitHubOperationAsync` method implements the necessary steps to commit a file: get the latest commit/tree -> create a blob for the file content -> create a new tree including the new blob -> create a commit pointing to the new tree and the previous commit -> update the branch reference (e.g., `heads/main`) to point to the new commit. [cite: zarichney-api/Server/Services/GitHubService.cs]
* **Retry Logic:** Employs a Polly `AsyncRetryPolicy` to handle specific exceptions with exponential backoff: [cite: zarichney-api/Server/Services/GitHubService.cs]
    * `RateLimitExceededException`: When GitHub API rate limits are reached.
    * `ApiValidationException` with message containing "Update is not a fast forward": Handles race conditions that occur during concurrent branch updates by automatically retrying the entire commit operation.
* **Configuration:** Requires `GitHubConfig` (registered via `IConfig`) providing repository owner, name, branch, and the essential Personal Access Token (PAT). [cite: zarichney-api/Server/Services/GitHubService.cs, zarichney-api/appsettings.json]

## 3. Interface Contract & Assumptions

* **Key Public Interfaces:** `IGitHubService` with the method:
    * `EnqueueCommitAsync(filePath, content, directory, commitMessage)`: Queues a file commit operation. The returned `Task` completes when the background service finishes processing this specific commit, or throws a `ConfigurationMissingException` if required configuration is missing. [cite: zarichney-api/Server/Services/GitHubService.cs]
* **Runtime Configuration Validation:**
    * The service will start even if `GitHubConfig` values (`AccessToken`, `RepositoryOwner`, `RepositoryName`) are missing, allowing the application to start with partial functionality.
    * Configuration is validated at runtime within `ProcessGitHubOperationAsync` when a commit operation is attempted.
    * If any required configuration is missing, a `ConfigurationMissingException` will be thrown and propagated through the `Task` returned by `EnqueueCommitAsync`.
* **Assumptions:**
    * **Configuration:** Assumes `GitHubConfig` contains valid values, especially a correct and active Personal Access Token (`AccessToken`) with sufficient permissions (`repo` scope) for the target repository (`RepositoryOwner`/`RepositoryName`, `BranchName`).
    * **Repository State:** Assumes the target repository and branch specified in the configuration exist.
    * **Network Connectivity:** Requires network access to `api.github.com`.
    * **Octokit Behavior:** Relies on the `Octokit` library correctly interacting with the GitHub API.
    * **Background Service Lifetime:** Assumes the `GitHubService` (as an `IHostedService`) is running correctly.

## 4. Local Conventions & Constraints (Beyond Global Standards)

* **Technology:** Uses `Octokit` library for GitHub interactions.
* **Authentication:** Relies solely on Personal Access Token authentication.
* **Asynchronous Processing:** Commits are handled asynchronously via the background service and channel queue. Callers `await` the `EnqueueCommitAsync` task for completion confirmation.
* **Scope:** Currently focused only on committing single files; does not handle more complex Git operations.
* **Error Handling:** Uses Polly for retries on rate limits. Exceptions during processing within the background service fail the specific operation's `TaskCompletionSource` and are logged.

## 5. How to Work With This Code

* **Consumption:** Inject `IGitHubService` into services needing to commit files to the configured GitHub repository. Call `await EnqueueCommitAsync(...)`. Services consuming `IGitHubService` should be prepared to handle `ConfigurationMissingException` if GitHubConfig values are missing at runtime.
* **Configuration:** Ensure `GitHubConfig` section in application configuration (user secrets recommended for `AccessToken`) is populated correctly. The PAT needs `repo` scope permissions. Missing configuration will not prevent application startup but will cause operations to fail with `ConfigurationMissingException` at runtime.
* **Testing:**
    * Mocking `IGitHubService` is standard for unit testing consumers.
    * Testing `GitHubService` itself requires mocking `GitHubClient` and its various sub-clients (`Git`, `Repository`, `Blob`, `Tree`, `Commit`, `Reference`) or performing integration tests against a dedicated test repository (exercise caution with PATs).
* **Common Pitfalls / Gotchas:** Invalid or expired PAT. Insufficient PAT permissions. GitHub API rate limiting (mitigated by Polly). Network errors connecting to GitHub. Errors in the complex Git workflow logic within `ProcessGitHubOperationAsync`. Race conditions if external changes are made to the target branch between fetching the base tree and pushing the new commit (though less likely for simple file additions/updates).

## 6. Dependencies

* **Internal Code Dependencies:**
    * [`Server/Config`](../../Config/README.md): Consumes `GitHubConfig`.
* **External Library Dependencies:**
    * `Octokit`: GitHub API client library.
    * `Polly`: For retry logic.
    * `System.Threading.Channels`: For the asynchronous queue.
    * `Microsoft.Extensions.Hosting.Abstractions`: For `BackgroundService`.
* **Dependents (Impact of Changes):**
    * [`Server/Services/AI/LlmRepository.cs`](../AI/LlmRepository.cs): Consumes `IGitHubService` to persist conversation logs.
    * `Program.cs`: Registers `GitHubService` as a singleton/hosted service.

## 7. Rationale & Key Historical Context

* **Abstraction:** Isolates GitHub-specific API calls and logic from consuming services like `LlmRepository`.
* **Asynchronous Handling:** Using a background service and channel prevents blocking application threads (like those handling AI responses) while waiting for potentially slow GitHub API operations.
* **Persistence Choice:** GitHub was chosen as the storage mechanism for LLM conversation logs, likely for visibility, versioning, and ease of access compared to other storage options within the project's context.

## 8. Known Issues & TODOs

* **Error Handling Granularity:** Could potentially catch and handle more specific `Octokit` exceptions within `ProcessGitHubOperationAsync` for better diagnostics or different retry strategies.
* **Conflict Handling:** Does not explicitly handle potential conflicts if the target branch's state changes significantly between the start and end of the commit process. For the current use case (adding distinct log files), conflicts are less likely.
* **PAT Security:** Management and rotation of the Personal Access Token stored in configuration are external concerns but crucial for security.
* **Alternative Storage:** If GitHub becomes unsuitable (e.g., rate limits, cost, privacy concerns), the `LlmRepository` could be refactored to use a different persistence mechanism (like `IFileService` or a database) by changing its dependency, leveraging the abstraction provided by `IGitHubService`.