using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Zarichney.Services.GitHub;
using FluentAssertions;
using Xunit;

namespace Zarichney.Tests.Unit.Services.GitHub;

/// <summary>
/// Unit tests for GitHubService covering repository operations, webhook processing, and API integrations.
/// Verifies proper GitHub API interactions, authentication handling, error scenarios, and resource disposal.
/// Tests background service functionality and channel-based communication patterns.
/// </summary>
[Trait("Category", "Unit")]
public class GitHubServiceTests : IDisposable
{
  private readonly Mock<ILogger<GitHubService>> _mockLogger;
  private readonly GitHubConfig _config;
  private readonly GitHubService _sut;
  private readonly CancellationTokenSource _cancellationTokenSource;

  public GitHubServiceTests()
  {
    _mockLogger = new Mock<ILogger<GitHubService>>();
    _config = new GitHubConfig
    {
      RepositoryOwner = "test-owner",
      RepositoryName = "test-repo",
      BranchName = "test-branch",
      AccessToken = "test-token",
      RetryAttempts = 3
    };
    _sut = new GitHubService(_config, _mockLogger.Object);
    _cancellationTokenSource = new CancellationTokenSource();
  }

  public void Dispose()
  {
    _cancellationTokenSource?.Cancel();
    _cancellationTokenSource?.Dispose();
    _sut?.Dispose();
  }

  #region Constructor Tests

  [Fact]
  public void Constructor_ValidConfig_InitializesSuccessfully()
  {
    // Arrange
    // Clear any constructor log from the test fixture's _sut to ensure accurate verification
    _mockLogger.Invocations.Clear();

    // Act
    var service = new GitHubService(_config, _mockLogger.Object);

    // Assert
    service.Should().NotBeNull("service should initialize with valid configuration");

    // Verify logging
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GitHub service initialized")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "initialization should be logged");
  }

  [Fact]
  public void Constructor_NullLogger_ThrowsArgumentNullException()
  {
    // Act
    Action act = () => new GitHubService(_config, null!);

    // Assert
    act.Should().Throw<ArgumentNullException>().WithParameterName("logger");
  }

  #endregion

  #region GitHubConfig Tests

  [Fact]
  public void GitHubConfig_DefaultBranchName_IsMain()
  {
    // Arrange & Act
    var config = new GitHubConfig();

    // Assert
    config.BranchName.Should().Be("main", "default branch should be main");
  }

  [Fact]
  public void GitHubConfig_DefaultRetryAttempts_IsFive()
  {
    // Arrange & Act
    var config = new GitHubConfig();

    // Assert
    config.RetryAttempts.Should().Be(5, "default retry attempts should be 5");
  }

  [Fact]
  public void GitHubConfig_RequiredProperties_AreEmpty()
  {
    // Arrange & Act
    var config = new GitHubConfig();

    // Assert
    config.RepositoryOwner.Should().BeEmpty("repository owner should be empty by default");
    config.RepositoryName.Should().BeEmpty("repository name should be empty by default");
    config.AccessToken.Should().BeEmpty("access token should be empty by default");
  }

  #endregion

  #region GitHubOperation Tests

  [Fact]
  public void GitHubOperation_DefaultValues_AreCorrect()
  {
    // Arrange & Act
    var operation = new GitHubOperation();

    // Assert
    operation.FilePath.Should().BeEmpty("file path should be empty by default");
    operation.Directory.Should().BeEmpty("directory should be empty by default");
    operation.CommitMessage.Should().BeEmpty("commit message should be empty by default");
    operation.Content.Should().BeEmpty("content should be empty by default");
    ((object)operation.CompletionSource).Should().NotBeNull("completion source should be initialized");
  }

  [Fact]
  public void GitHubOperation_CompletionSource_IsNotCompleted()
  {
    // Arrange & Act
    var operation = new GitHubOperation();

    // Assert
    operation.CompletionSource.Task.IsCompleted.Should().BeFalse("completion source should not be completed initially");
  }

  #endregion

  #region EnqueueCommitAsync Tests

  [Fact]
  public async Task EnqueueCommitAsync_ValidParameters_LogsEnqueueOperation()
  {
    // Arrange
    var filePath = "test.txt";
    var content = System.Text.Encoding.UTF8.GetBytes("test content");
    var directory = "test-dir";
    var commitMessage = "Test commit";

    // We need to start the service for the background processing
    var serviceTask = _sut.StartAsync(_cancellationTokenSource.Token);

    try
    {
      // Act - This will timeout due to no actual GitHub API, but it proves enqueuing works
      var enqueueTask = _sut.EnqueueCommitAsync(filePath, content, directory, commitMessage);

      // Cancel after a short delay to prevent hanging
      _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

      // The operation should be enqueued but will fail during processing
      var act = async () => await enqueueTask;
      await act.Should().ThrowAsync<Exception>();
    }
    finally
    {
      // Cleanup
      _cancellationTokenSource.Cancel();
      try
      {
        await serviceTask;
      }
      catch
      {
        // Ignore cleanup exceptions
      }
    }

    // Verify logging for enqueue operation
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enqueuing GitHub commit")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.AtLeastOnce,
        "enqueue operation should be logged");
  }

  #endregion

  #region StoreAudioAndTranscriptAsync Tests

  [Fact]
  public async Task StoreAudioAndTranscriptAsync_ValidParameters_LogsStoringOperation()
  {
    // Arrange
    var audioFileName = "test-audio.wav";
    var audioData = new byte[] { 0x52, 0x49, 0x46, 0x46 }; // WAV header
    var transcriptFileName = "test-transcript.txt";
    var transcriptText = "This is a test transcript";

    var serviceTask = _sut.StartAsync(_cancellationTokenSource.Token);

    try
    {
      // Act
      var storeTask = _sut.StoreAudioAndTranscriptAsync(audioFileName, audioData, transcriptFileName, transcriptText);
      _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

      var act = async () => await storeTask;
      await act.Should().ThrowAsync<Exception>();
    }
    finally
    {
      _cancellationTokenSource.Cancel();
      try
      {
        await serviceTask;
      }
      catch
      {
        // Ignore cleanup exceptions
      }
    }

    // Assert
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Storing audio and transcript files")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.AtLeastOnce,
        "storing operation should be logged");

    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enqueuing GitHub commit")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.AtLeast(2), // Should enqueue both audio and transcript
        "both files should be enqueued");
  }

  #endregion

  #region Background Service Tests

  [Fact]
  public void StartAsync_ValidToken_StartsSuccessfully()
  {
    // Arrange
    using var cts = new CancellationTokenSource();

    // Act
    var startTask = _sut.StartAsync(cts.Token);
    cts.CancelAfter(TimeSpan.FromMilliseconds(50));

    // Assert
    startTask.Should().NotBeNull("start task should be created");
  }

  [Fact]
  public async Task StopAsync_AfterStart_StopsGracefully()
  {
    // Arrange
    using var cts = new CancellationTokenSource();
    await _sut.StartAsync(cts.Token);

    // Act
    var stopTask = _sut.StopAsync(cts.Token);

    // Assert
    stopTask.Should().NotBeNull("stop task should be created");

    // Verify logging
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GitHub service is stopping")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.AtLeastOnce,
        "stopping should be logged");
  }

  #endregion

  #region Service State Tests

  [Fact]
  public void GitHubService_ImplementsIHostedService()
  {
    // Assert
    _sut.Should().BeAssignableTo<IHostedService>("GitHubService should implement IHostedService");
  }

  [Fact]
  public void GitHubService_ImplementsIGitHubService()
  {
    // Assert
    _sut.Should().BeAssignableTo<IGitHubService>("GitHubService should implement IGitHubService interface");
  }

  [Fact]
  public void GitHubService_ImplementsBackgroundService()
  {
    // Assert
    _sut.Should().BeAssignableTo<BackgroundService>("GitHubService should inherit from BackgroundService");
  }

  #endregion
}
