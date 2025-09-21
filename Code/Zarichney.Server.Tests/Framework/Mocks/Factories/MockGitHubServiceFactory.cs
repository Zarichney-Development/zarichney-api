using Moq;

namespace Zarichney.Server.Tests.Framework.Mocks.Factories;

/// <summary>
/// Factory for creating mock GitHub service.
/// </summary>
public class MockGitHubServiceFactory : BaseMockFactory<IGitHubService>
{
  /// <summary>
  /// Creates a mock GitHub service with default implementations.
  /// </summary>
  /// <returns>A Mock of the IGitHubService.</returns>
  public new static Mock<IGitHubService> CreateMock()
  {
    var factory = new MockGitHubServiceFactory();
    return factory.CreateDefaultMock();
  }

  /// <summary>
  /// Sets up default behaviors for the mock GitHub service.
  /// </summary>
  /// <param name="mock">The mock to set up.</param>
  protected override void SetupDefaultMock(Mock<IGitHubService> mock)
  {
    // Setup default repository operations
    mock.Setup(s => s.GetRepositoryAsync(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync((string owner, string repo) => new GitHubRepository
        {
          Id = 12345,
          Name = repo,
          Owner = owner,
          Description = $"Test repository for {owner}/{repo}",
          Url = $"https://github.com/{owner}/{repo}"
        });

    // Setup default issue operations
    mock.Setup(s => s.GetIssuesAsync(It.IsAny<string>(), It.IsAny<string>()))
        .ReturnsAsync((string _, string _) =>
        [
            new GitHubIssue
            {
              Id = 1,
              Number = 1,
              Title = "Test issue 1",
              Body = "This is a test issue",
              State = "open"
            },

          new GitHubIssue
          {
            Id = 2,
            Number = 2,
            Title = "Test issue 2",
            Body = "This is another test issue",
            State = "closed"
          }
        ]);

    // Setup default PR operations
    mock.Setup(s => s.CreatePullRequestAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
        .ReturnsAsync((string owner, string repo, string title, string body, string _, string _) =>
            new GitHubPullRequest
            {
              Id = 123,
              Number = 1,
              Title = title,
              Body = body,
              State = "open",
              Url = $"https://github.com/{owner}/{repo}/pull/1"
            });
  }
}

/// <summary>
/// Interface for GitHub service.
/// This would typically be defined in the actual service project.
/// </summary>
public interface IGitHubService
{
  Task<GitHubRepository> GetRepositoryAsync(string owner, string repo);
  Task<List<GitHubIssue>> GetIssuesAsync(string owner, string repo);
  Task<GitHubPullRequest> CreatePullRequestAsync(
      string owner,
      string repo,
      string title,
      string body,
      string head,
      string baseBranch);
}

/// <summary>
/// Represents a GitHub repository.
/// </summary>
public class GitHubRepository
{
  public int Id { get; set; }
  public string Name { get; set; } = string.Empty;
  public string Owner { get; set; } = string.Empty;
  public string Description { get; set; } = string.Empty;
  public string Url { get; set; } = string.Empty;
}

/// <summary>
/// Represents a GitHub issue.
/// </summary>
public class GitHubIssue
{
  public int Id { get; set; }
  public int Number { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Body { get; set; } = string.Empty;
  public string State { get; set; } = string.Empty;
}

/// <summary>
/// Represents a GitHub pull request.
/// </summary>
public class GitHubPullRequest
{
  public int Id { get; set; }
  public int Number { get; set; }
  public string Title { get; set; } = string.Empty;
  public string Body { get; set; } = string.Empty;
  public string State { get; set; } = string.Empty;
  public string Url { get; set; } = string.Empty;
}
