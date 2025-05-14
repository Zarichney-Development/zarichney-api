using Zarichney.Services.Status;
using System.Threading.Channels;
using Octokit;
using Polly;
using Polly.Retry;
using Zarichney.Config;
using FileMode = Octokit.FileMode;

namespace Zarichney.Services.GitHub;

public class GitHubConfig : IConfig
{
  [RequiresConfiguration(ExternalServices.GitHubAccess)]
  public string RepositoryOwner { get; init; } = string.Empty;

  [RequiresConfiguration(ExternalServices.GitHubAccess)]
  public string RepositoryName { get; init; } = string.Empty;

  public string BranchName { get; init; } = "main";

  [RequiresConfiguration(ExternalServices.GitHubAccess)]
  public string AccessToken { get; init; } = string.Empty;

  public int RetryAttempts { get; init; } = 5;
}

public class GitHubOperation
{
  public string FilePath { get; init; } = string.Empty;
  public byte[] Content { get; init; } = [];
  public string Directory { get; init; } = string.Empty;
  public string CommitMessage { get; init; } = string.Empty;
  public TaskCompletionSource<bool> CompletionSource { get; } = new();
}

public interface IGitHubService
{
  Task EnqueueCommitAsync(string filePath, byte[] content, string directory, string commitMessage);
}

public class GitHubService : BackgroundService, IGitHubService
{
  private readonly ILogger _logger;
  private readonly Channel<GitHubOperation> _operationChannel;
  private readonly AsyncRetryPolicy _retryPolicy;
  private readonly GitHubConfig _config;

  public GitHubService(GitHubConfig config, ILogger<GitHubService> logger)
  {
    _logger = logger;
    _config = config;
    _operationChannel = Channel.CreateUnbounded<GitHubOperation>(new UnboundedChannelOptions
    {
      SingleReader = true,
      SingleWriter = false
    });

    _retryPolicy = Policy
      .Handle<RateLimitExceededException>()
      .Or<ApiValidationException>(ex => ex.ApiError?.Message?.Contains("Update is not a fast forward") ?? ex.Message.Contains("Update is not a fast forward"))
      .WaitAndRetryAsync(
        retryCount: config.RetryAttempts,
        sleepDurationProvider: retryAttempt =>
          TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
        onRetry: (exception, timeSpan, retryCount, _) =>
        {
          var reason = exception is RateLimitExceededException ? "Rate Limit" : "Non-Fast-Forward";
          _logger.LogWarning(exception,
            "GitHub operation attempt {RetryCount}: Retrying due to {Reason} after {Delay}s. Error: {Message}",
            retryCount, reason, timeSpan.TotalSeconds, exception.Message);
        }
      );

    _logger.LogInformation("GitHub service initialized");
  }

  public async Task EnqueueCommitAsync(string filePath, byte[] content, string directory, string commitMessage)
  {
    var operation = new GitHubOperation
    {
      FilePath = filePath,
      Content = content,
      Directory = directory,
      CommitMessage = commitMessage
    };

    _logger.LogInformation("Enqueuing GitHub commit for file {FilePath} in directory {Directory}",
      operation.FilePath, operation.Directory);

    await _operationChannel.Writer.WriteAsync(operation);
    await operation.CompletionSource.Task; // Wait for completion without blocking the channel
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    try
    {
      await foreach (var operation in _operationChannel.Reader.ReadAllAsync(stoppingToken))
      {
        try
        {
          await ProcessGitHubOperationAsync(operation);
          operation.CompletionSource.SetResult(true);
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Failed to process GitHub operation for {FilePath}", operation.FilePath);
          operation.CompletionSource.SetException(ex);
        }
      }
    }
    catch (OperationCanceledException)
    {
      _logger.LogInformation("GitHub service background processing cancelled");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unhandled exception in GitHub service background processing");
    }
  }

  private async Task ProcessGitHubOperationAsync(GitHubOperation operation)
  {
    // Configuration validation is now handled by IConfigurationStatusService
    // These properties are marked with RequiresConfiguration attribute

    _logger.LogInformation("Processing GitHub commit for file {FilePath} in directory {Directory}",
      operation.FilePath, operation.Directory);

    // Create client instance on demand with validated configuration
    var client = new GitHubClient(new ProductHeaderValue(_config.RepositoryName))
    {
      Credentials = new Credentials(_config.AccessToken)
    };

    await _retryPolicy.ExecuteAsync(async () =>
    {
      try
      {
        // Get the current reference
        var reference = await client.Git.Reference.Get(
          _config.RepositoryOwner,
          _config.RepositoryName,
          $"heads/{_config.BranchName}"
        );

        var latestCommit = await client.Git.Commit.Get(
          _config.RepositoryOwner,
          _config.RepositoryName,
          reference.Object.Sha
        );

        // Create blob
        var blob = await client.Git.Blob.Create(
          _config.RepositoryOwner,
          _config.RepositoryName,
          new NewBlob
          {
            Content = Convert.ToBase64String(operation.Content),
            Encoding = EncodingType.Base64
          }
        );

        // Create tree
        var newTree = new NewTree
        {
          BaseTree = latestCommit.Tree.Sha
        };

        newTree.Tree.Add(new NewTreeItem
        {
          Path = Path.Combine(operation.Directory, operation.FilePath).Replace("\\", "/"),
          Mode = FileMode.File,
          Type = TreeType.Blob,
          Sha = blob.Sha,
        });

        var tree = await client.Git.Tree.Create(
          _config.RepositoryOwner,
          _config.RepositoryName,
          newTree
        );

        // Create commit
        var commit = await client.Git.Commit.Create(
          _config.RepositoryOwner,
          _config.RepositoryName,
          new NewCommit(operation.CommitMessage, tree.Sha, reference.Object.Sha)
        );

        // Update reference
        await client.Git.Reference.Update(
          _config.RepositoryOwner,
          _config.RepositoryName,
          $"heads/{_config.BranchName}",
          new ReferenceUpdate(commit.Sha)
        );

        _logger.LogInformation(
          "Successfully committed file {FilePath} to {Directory}. Commit SHA: {CommitSha}",
          operation.FilePath, operation.Directory, commit.Sha
        );
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error occurred during GitHub operation for file {FilePath}", operation.FilePath);
        throw;
      }
    });
  }
}
