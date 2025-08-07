using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Services.ProcessExecution;

namespace Zarichney.Tests.Unit.Services.ProcessExecution;

/// <summary>
/// Unit tests for ProcessExecutor service - tests system command execution with proper async handling
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "ProcessExecution")]
public class ProcessExecutorTests
{
  private readonly IFixture _fixture;
  private readonly Mock<ILogger<ProcessExecutor>> _mockLogger;
  private readonly ProcessExecutor _sut;

  public ProcessExecutorTests()
  {
    _fixture = new Fixture();
    _mockLogger = new Mock<ILogger<ProcessExecutor>>();
    _sut = new ProcessExecutor(_mockLogger.Object);
  }

  #region RunCommandAsync Tests - Success Scenarios

  [Fact]
  public async Task RunCommandAsync_WithValidCommand_ReturnsSuccessfulResult()
  {
    // Arrange
    var command = "echo";
    var arguments = "test output";

    // Act
    var result = await _sut.RunCommandAsync(command, arguments);

    // Assert
    result.exitCode.Should().Be(0, "echo command should succeed");
    result.output.Should().Contain("test output", "output should contain the echoed text");
  }

  [Fact]
  public async Task RunCommandAsync_WithEmptyArguments_ReturnsSuccessfulResult()
  {
    // Arrange
    var command = "echo";
    var arguments = "";

    // Act
    var result = await _sut.RunCommandAsync(command, arguments);

    // Assert
    result.exitCode.Should().Be(0, "echo command should succeed even with empty arguments");
  }

  #endregion

  #region RunCommandAsync Tests - Timeout Scenarios

  [Fact]
  public async Task RunCommandAsync_WithTimeout_ReturnsFailureWhenExceeded()
  {
    // Arrange
    var command = "sleep";
    var arguments = "5"; // Sleep for 5 seconds
    var timeoutMs = 100; // Timeout after 100ms

    // Act
    var result = await _sut.RunCommandAsync(command, arguments, timeoutMs);

    // Assert
    result.exitCode.Should().Be(-1, "command should fail due to timeout");
    result.output.Should().BeEmpty("output should be empty when timeout occurs");

    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Debug,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("timed out")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once,
      "timeout should be logged");
  }

  #endregion

  #region RunCommandAsync Tests - Cancellation Scenarios

  [Fact]
  public async Task RunCommandAsync_WithCancellation_ThrowsOperationCanceledException()
  {
    // Arrange
    var command = "sleep";
    var arguments = "5"; // Sleep for 5 seconds
    using var cts = new CancellationTokenSource();
    
    // Act & Assert
    var task = _sut.RunCommandAsync(command, arguments, 5000, cts.Token);
    cts.Cancel();

    // TaskCanceledException derives from OperationCanceledException
    await Assert.ThrowsAsync<TaskCanceledException>(() => task);
  }

  [Fact]
  public async Task RunCommandAsync_WithPreCancelledToken_ThrowsImmediately()
  {
    // Arrange
    var command = "echo";
    var arguments = "test";
    using var cts = new CancellationTokenSource();
    cts.Cancel();

    // Act & Assert
    await Assert.ThrowsAsync<OperationCanceledException>(
      () => _sut.RunCommandAsync(command, arguments, 5000, cts.Token));
  }

  #endregion

  #region RunCommandAsync Tests - Error Scenarios

  [Fact]
  public async Task RunCommandAsync_WithInvalidCommand_ReturnsFailureResult()
  {
    // Arrange
    var command = "nonexistentcommand123456";
    var arguments = "test";

    // Act
    var result = await _sut.RunCommandAsync(command, arguments);

    // Assert
    result.exitCode.Should().Be(-1, "invalid command should return failure exit code");
    result.output.Should().BeEmpty("output should be empty for invalid command");

    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Debug,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error running command")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once,
      "error should be logged");
  }

  [Fact]
  public async Task RunCommandAsync_WithCommandReturningNonZeroExitCode_ReturnsExitCode()
  {
    // Arrange
    var command = "bash";
    var arguments = "-c \"exit 42\"";

    // Act
    var result = await _sut.RunCommandAsync(command, arguments);

    // Assert
    result.exitCode.Should().Be(42, "should return the actual exit code from the command");
  }

  #endregion

  #region RunCommandAsync Tests - Output Handling

  [Fact]
  public async Task RunCommandAsync_WithMultilineOutput_CapturesAllLines()
  {
    // Arrange
    var command = "bash";
    var arguments = "-c \"echo line1; echo line2; echo line3\"";

    // Act
    var result = await _sut.RunCommandAsync(command, arguments);

    // Assert
    result.exitCode.Should().Be(0, "command should succeed");
    result.output.Should().Contain("line1", "output should contain first line");
    result.output.Should().Contain("line2", "output should contain second line");
    result.output.Should().Contain("line3", "output should contain third line");
  }

  [Fact]
  public async Task RunCommandAsync_WithStderrOutput_LogsErrorOutput()
  {
    // Arrange
    var command = "bash";
    var arguments = "-c \"echo 'stdout output'; echo 'stderr output' >&2\"";

    // Act
    var result = await _sut.RunCommandAsync(command, arguments);

    // Assert
    result.exitCode.Should().Be(0, "command should succeed");
    result.output.Should().Contain("stdout output", "stdout should be captured");

    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Debug,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("stderr output")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once,
      "stderr output should be logged");
  }

  #endregion

  #region RunCommandAsync Tests - Edge Cases

  [Fact]
  public async Task RunCommandAsync_WithLargeOutput_HandlesCorrectly()
  {
    // Arrange
    var command = "bash";
    // Generate 1000 lines of output
    var arguments = "-c \"for i in {1..1000}; do echo 'Line number '$i; done\"";

    // Act
    var result = await _sut.RunCommandAsync(command, arguments, 10000);

    // Assert
    result.exitCode.Should().Be(0, "command should succeed");
    result.output.Should().Contain("Line number 1", "should contain first line");
    result.output.Should().Contain("Line number 1000", "should contain last line");
    result.output.Split('\n').Length.Should().BeGreaterThan(999, "should contain all lines");
  }

  [Fact]
  public async Task RunCommandAsync_WithQuotesInArguments_HandlesCorrectly()
  {
    // Arrange
    var command = "echo";
    var arguments = "\"test with quotes\"";

    // Act
    var result = await _sut.RunCommandAsync(command, arguments);

    // Assert
    result.exitCode.Should().Be(0, "command should succeed");
    result.output.Should().Contain("test with quotes", "output should contain the text with quotes");
  }

  [Fact]
  public async Task RunCommandAsync_WithSpecialCharactersInArguments_HandlesCorrectly()
  {
    // Arrange
    var command = "echo";
    var arguments = "test$special!characters@here#";

    // Act
    var result = await _sut.RunCommandAsync(command, arguments);

    // Assert
    result.exitCode.Should().Be(0, "command should succeed");
    result.output.Should().Contain("test", "output should contain the text");
  }

  #endregion
}