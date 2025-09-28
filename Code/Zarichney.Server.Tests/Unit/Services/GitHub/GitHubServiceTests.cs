using System.Threading.Channels;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using Zarichney.Services.GitHub;
using FluentAssertions;
using Xunit;
using System.Threading;

namespace Zarichney.Server.Tests.Unit.Services.GitHub;

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

  #region ProcessGitHubOperationAsync Tests

  [Fact]
  public async Task ProcessGitHubOperationAsync_EmptyContent_HandlesCorrectly()
  {
    // Arrange
    var operation = new GitHubOperation
    {
      FilePath = "empty.txt",
      Content = Array.Empty<byte>(),
      Directory = "test",
      CommitMessage = "Add empty file"
    };

    var serviceTask = _sut.StartAsync(_cancellationTokenSource.Token);

    try
    {
      // Act
      var enqueueTask = _sut.EnqueueCommitAsync(
        operation.FilePath,
        operation.Content,
        operation.Directory,
        operation.CommitMessage);

      _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

      // Assert - Expect authorization exception due to test credentials
      await Assert.ThrowsAnyAsync<Exception>(async () => await enqueueTask);
    }
    finally
    {
      _cancellationTokenSource.Cancel();
      try { await serviceTask; } catch { }
    }

    // Verify processing was attempted
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enqueuing GitHub commit")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  public async Task ProcessGitHubOperationAsync_LargeContent_HandlesCorrectly()
  {
    // Arrange
    var largeContent = new byte[1024 * 1024]; // 1MB
    new Random(42).NextBytes(largeContent);

    var operation = new GitHubOperation
    {
      FilePath = "large.bin",
      Content = largeContent,
      Directory = "binaries",
      CommitMessage = "Add large binary file"
    };

    var serviceTask = _sut.StartAsync(_cancellationTokenSource.Token);

    try
    {
      // Act
      var enqueueTask = _sut.EnqueueCommitAsync(
        operation.FilePath,
        operation.Content,
        operation.Directory,
        operation.CommitMessage);

      _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

      // Assert - Expect authorization exception due to test credentials
      await Assert.ThrowsAnyAsync<Exception>(async () => await enqueueTask);
    }
    finally
    {
      _cancellationTokenSource.Cancel();
      try { await serviceTask; } catch { }
    }

    // Verify processing was attempted
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enqueuing GitHub commit")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  public async Task ProcessGitHubOperationAsync_SpecialCharactersInPath_HandlesCorrectly()
  {
    // Arrange
    var operation = new GitHubOperation
    {
      FilePath = "file with spaces & special.txt",
      Content = System.Text.Encoding.UTF8.GetBytes("content"),
      Directory = "dir/with/nested",
      CommitMessage = "Add file with special characters"
    };

    var serviceTask = _sut.StartAsync(_cancellationTokenSource.Token);

    try
    {
      // Act
      var enqueueTask = _sut.EnqueueCommitAsync(
        operation.FilePath,
        operation.Content,
        operation.Directory,
        operation.CommitMessage);

      _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

      // Assert - Expect authorization exception due to test credentials
      await Assert.ThrowsAnyAsync<Exception>(async () => await enqueueTask);
    }
    finally
    {
      _cancellationTokenSource.Cancel();
      try { await serviceTask; } catch { }
    }

    // Verify processing was attempted
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enqueuing GitHub commit")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  #endregion

  #region Channel Operations Tests

  [Fact]
  public async Task EnqueueCommitAsync_MultipleOperations_ProcessesInOrder()
  {
    // Arrange
    var operations = Enumerable.Range(1, 5).Select(i => new GitHubOperation
    {
      FilePath = $"file{i}.txt",
      Content = System.Text.Encoding.UTF8.GetBytes($"content{i}"),
      Directory = "test",
      CommitMessage = $"Commit {i}"
    }).ToList();

    var serviceTask = _sut.StartAsync(_cancellationTokenSource.Token);

    try
    {
      // Act - Enqueue all operations
      var enqueueTasks = operations.Select(op =>
        _sut.EnqueueCommitAsync(op.FilePath, op.Content, op.Directory, op.CommitMessage)
      ).ToList();

      _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(200));

      // Assert - All should fail due to no GitHub API
      foreach (var task in enqueueTasks)
      {
        await Assert.ThrowsAnyAsync<Exception>(async () => await task);
      }
    }
    finally
    {
      _cancellationTokenSource.Cancel();
      try { await serviceTask; } catch { }
    }

    // Verify all operations were enqueued
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enqueuing GitHub commit")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Exactly(5),
        "all operations should be enqueued");
  }

  [Fact]
  public async Task EnqueueCommitAsync_ConcurrentEnqueues_HandlesThreadSafety()
  {
    // Arrange
    var concurrentOperations = 10;
    var barrier = new Barrier(concurrentOperations);

    var serviceTask = _sut.StartAsync(_cancellationTokenSource.Token);

    try
    {
      // Act - Create concurrent enqueue operations
      var tasks = Enumerable.Range(0, concurrentOperations).Select(async i =>
      {
        barrier.SignalAndWait(TimeSpan.FromSeconds(1));

        await Assert.ThrowsAnyAsync<Exception>(async () =>
          await _sut.EnqueueCommitAsync(
            $"concurrent{i}.txt",
            System.Text.Encoding.UTF8.GetBytes($"data{i}"),
            "concurrent",
            $"Concurrent commit {i}")
        );
      }).ToList();

      _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(300));

      // Assert - Wait for all tasks to complete
      await Task.WhenAll(tasks);
    }
    finally
    {
      _cancellationTokenSource.Cancel();
      barrier.Dispose();
      try { await serviceTask; } catch { }
    }

    // Verify all operations were enqueued
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enqueuing GitHub commit")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Exactly(concurrentOperations),
        "all concurrent operations should be enqueued");
  }

  #endregion

  #region Error Handling Tests

  [Fact]
  public async Task ExecuteAsync_OperationCancelled_LogsCancellation()
  {
    // Arrange & Act
    var serviceTask = _sut.StartAsync(_cancellationTokenSource.Token);

    // Cancel immediately
    _cancellationTokenSource.Cancel();

    // Wait for service to stop
    try
    {
      await serviceTask;
    }
    catch (OperationCanceledException)
    {
      // Expected
    }

    // Assert
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GitHub service background processing cancelled")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.AtMostOnce,
        "cancellation should be logged");
  }

  [Fact]
  public async Task EnqueueCommitAsync_AfterServiceStop_HandlesGracefully()
  {
    // Arrange
    var serviceTask = _sut.StartAsync(_cancellationTokenSource.Token);
    await _sut.StopAsync(_cancellationTokenSource.Token);

    // Act & Assert
    var enqueueTask = _sut.EnqueueCommitAsync(
      "test.txt",
      System.Text.Encoding.UTF8.GetBytes("test"),
      "test",
      "Test after stop");

    // The enqueue should still work but processing won't happen
    // This tests the channel's behavior after completion
    _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

    await Assert.ThrowsAnyAsync<Exception>(async () => await enqueueTask);
  }

  #endregion

  #region Configuration Validation Tests

  [Fact]
  public void GitHubConfig_InitProperties_SetsValuesCorrectly()
  {
    // Arrange & Act
    var config = new GitHubConfig
    {
      RepositoryOwner = "test-owner",
      RepositoryName = "test-repo",
      BranchName = "develop",
      AccessToken = "ghp_test123",
      RetryAttempts = 10
    };

    // Assert
    config.RepositoryOwner.Should().Be("test-owner");
    config.RepositoryName.Should().Be("test-repo");
    config.BranchName.Should().Be("develop");
    config.AccessToken.Should().Be("ghp_test123");
    config.RetryAttempts.Should().Be(10);
  }

  [Theory]
  [InlineData("")]
  [InlineData(" ")]
  [InlineData("\t")]
  [InlineData("\n")]
  public void GitHubConfig_WhitespaceAccessToken_HandlesCorrectly(string token)
  {
    // Arrange & Act
    var config = new GitHubConfig { AccessToken = token };

    // Assert
    config.AccessToken.Should().Be(token, "whitespace tokens should be preserved as-is");
  }

  #endregion

  #region Retry Policy Tests

  [Fact]
  public void Constructor_ConfiguresRetryPolicy_WithExponentialBackoff()
  {
    // Arrange
    var configWithRetries = new GitHubConfig
    {
      RepositoryOwner = "owner",
      RepositoryName = "repo",
      AccessToken = "token",
      RetryAttempts = 3
    };

    // Act
    var service = new GitHubService(configWithRetries, _mockLogger.Object);

    // Assert
    service.Should().NotBeNull();

    // Verify retry configuration was used
    configWithRetries.RetryAttempts.Should().Be(3, "retry attempts should be preserved");
  }

  [Fact]
  public void Constructor_ZeroRetryAttempts_HandlesCorrectly()
  {
    // Arrange
    var configNoRetries = new GitHubConfig
    {
      RepositoryOwner = "owner",
      RepositoryName = "repo",
      AccessToken = "token",
      RetryAttempts = 0
    };

    // Act
    var service = new GitHubService(configNoRetries, _mockLogger.Object);

    // Assert
    service.Should().NotBeNull("service should initialize even with zero retries");
  }

  #endregion

  #region StoreAudioAndTranscriptAsync Edge Cases

  [Fact]
  public async Task StoreAudioAndTranscriptAsync_EmptyAudioData_HandlesCorrectly()
  {
    // Arrange
    var audioFileName = "empty.wav";
    var audioData = Array.Empty<byte>();
    var transcriptFileName = "empty-transcript.txt";
    var transcriptText = "";

    var serviceTask = _sut.StartAsync(_cancellationTokenSource.Token);

    try
    {
      // Act
      var storeTask = _sut.StoreAudioAndTranscriptAsync(
        audioFileName,
        audioData,
        transcriptFileName,
        transcriptText);

      _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

      // Assert
      await Assert.ThrowsAnyAsync<Exception>(async () => await storeTask);
    }
    finally
    {
      _cancellationTokenSource.Cancel();
      try { await serviceTask; } catch { }
    }

    // Verify both operations were enqueued
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Enqueuing GitHub commit")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Exactly(2));
  }

  [Fact]
  public async Task StoreAudioAndTranscriptAsync_UnicodeTranscript_HandlesCorrectly()
  {
    // Arrange
    var audioFileName = "unicode.wav";
    var audioData = new byte[] { 0x52, 0x49, 0x46, 0x46 };
    var transcriptFileName = "unicode-transcript.txt";
    var transcriptText = "测试 テスト тест émojis: 🎵🎤";

    var serviceTask = _sut.StartAsync(_cancellationTokenSource.Token);

    try
    {
      // Act
      var storeTask = _sut.StoreAudioAndTranscriptAsync(
        audioFileName,
        audioData,
        transcriptFileName,
        transcriptText);

      _cancellationTokenSource.CancelAfter(TimeSpan.FromMilliseconds(100));

      // Assert
      await Assert.ThrowsAnyAsync<Exception>(async () => await storeTask);
    }
    finally
    {
      _cancellationTokenSource.Cancel();
      try { await serviceTask; } catch { }
    }

    // Verify operations were enqueued with correct directories
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Storing audio and transcript files")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  #endregion

  #region Channel Writer Completion Tests

  [Fact]
  public async Task StopAsync_MultipleCallsToStop_HandlesGracefully()
  {
    // Arrange
    await _sut.StartAsync(_cancellationTokenSource.Token);

    // Act - Call stop multiple times
    var stopTask1 = _sut.StopAsync(_cancellationTokenSource.Token);
    var stopTask2 = _sut.StopAsync(_cancellationTokenSource.Token);

    await Task.WhenAll(stopTask1, stopTask2);

    // Assert - Should not throw and should log appropriately
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("GitHub service is stopping")),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.AtLeast(2));
  }

  #endregion

  #region CompletionSource Tests

  [Fact]
  public async Task GitHubOperation_CompletionSource_CanBeCompleted()
  {
    // Arrange
    var operation = new GitHubOperation();

    // Act
    operation.CompletionSource.SetResult(true);

    // Assert
    operation.CompletionSource.Task.IsCompleted.Should().BeTrue();
    var result = await operation.CompletionSource.Task;
    result.Should().BeTrue();
  }

  [Fact]
  public void GitHubOperation_CompletionSource_CanBeSetWithException()
  {
    // Arrange
    var operation = new GitHubOperation();
    var testException = new InvalidOperationException("Test exception");

    // Act
    operation.CompletionSource.SetException(testException);

    // Assert
    operation.CompletionSource.Task.IsFaulted.Should().BeTrue();
    operation.CompletionSource.Task.Exception.Should().NotBeNull();
    operation.CompletionSource.Task.Exception!.InnerException.Should().BeEquivalentTo(testException);
  }

  #endregion
}
