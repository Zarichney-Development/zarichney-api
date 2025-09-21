using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Zarichney.Services.Logging;
using Zarichney.Services.Logging.Models;
using Zarichney.Services.ProcessExecution;

namespace Zarichney.Server.Tests.Unit.Services.Logging;

/// <summary>
/// Unit tests for LoggingStatus service - tests logging system status and configuration management
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Logging")]
[Trait("Feature", "LoggingStatus")]
public class LoggingStatusTests
{
  private readonly IFixture _fixture;
  private readonly Mock<ILogger<LoggingStatus>> _mockLogger;
  private readonly Mock<IConfiguration> _mockConfiguration;
  private readonly Mock<IOptions<LoggingConfig>> _mockConfig;
  private readonly Mock<ISeqConnectivity> _mockSeqConnectivity;
  private readonly Mock<IProcessExecutor> _mockProcessExecutor;
  private readonly LoggingConfig _loggingConfig;
  private readonly LoggingStatus _sut;

  public LoggingStatusTests()
  {
    _fixture = new Fixture();
    _mockLogger = new Mock<ILogger<LoggingStatus>>();
    _mockConfiguration = new Mock<IConfiguration>();
    _mockConfig = new Mock<IOptions<LoggingConfig>>();
    _mockSeqConnectivity = new Mock<ISeqConnectivity>();
    _mockProcessExecutor = new Mock<IProcessExecutor>();

    _loggingConfig = new LoggingConfig
    {
      SeqUrl = "http://localhost:5341",
      ProcessTimeoutMs = 5000,
      DefaultFileLoggingPath = "logs"
    };

    _mockConfig.Setup(x => x.Value).Returns(_loggingConfig);

    _sut = new LoggingStatus(
      _mockLogger.Object,
      _mockConfiguration.Object,
      _mockConfig.Object,
      _mockSeqConnectivity.Object,
      _mockProcessExecutor.Object);
  }

  #region GetLoggingStatusAsync Tests

  [Fact]
  public async Task GetLoggingStatusAsync_WithSeqAvailable_ReturnsStatusWithSeq()
  {
    // Arrange
    var activeUrl = "http://localhost:5341";
    _mockSeqConnectivity.Setup(x => x.TryConnectToSeqAsync(_loggingConfig.SeqUrl, It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);
    _mockSeqConnectivity.Setup(x => x.GetActiveSeqUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(activeUrl);
    _mockConfiguration.Setup(x => x["Serilog:MinimumLevel:Default"])
      .Returns("Information");
    SetupNativeSeqRunning(true);

    // Act
    var result = await _sut.GetLoggingStatusAsync();

    // Assert
    result.Should().NotBeNull();
    result.SeqAvailable.Should().BeTrue("Seq is available");
    result.SeqUrl.Should().Be(activeUrl);
    result.Method.Should().Be("Native Seq Service");
    result.FallbackActive.Should().BeFalse("Seq is available so fallback is not active");
    result.ConfiguredSeqUrl.Should().Be(_loggingConfig.SeqUrl);
    result.LogLevel.Should().Be("Information");
    result.FileLoggingPath.Should().Contain(_loggingConfig.DefaultFileLoggingPath);
    result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
  }

  [Fact]
  public async Task GetLoggingStatusAsync_WithSeqUnavailable_ReturnsStatusWithFallback()
  {
    // Arrange
    _mockSeqConnectivity.Setup(x => x.TryConnectToSeqAsync(_loggingConfig.SeqUrl, It.IsAny<CancellationToken>()))
      .ReturnsAsync(false);
    _mockSeqConnectivity.Setup(x => x.GetActiveSeqUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync((string?)null);
    _mockConfiguration.Setup(x => x["Serilog:MinimumLevel:Default"])
      .Returns("Warning");

    // Act
    var result = await _sut.GetLoggingStatusAsync();

    // Assert
    result.Should().NotBeNull();
    result.SeqAvailable.Should().BeFalse("Seq is not available");
    result.SeqUrl.Should().BeNull();
    result.Method.Should().Be("File Logging (Fallback)");
    result.FallbackActive.Should().BeTrue("Seq is unavailable so fallback is active");
    result.LogLevel.Should().Be("Warning");
  }

  [Fact]
  public async Task GetLoggingStatusAsync_WithNoConfiguredLogLevel_ReturnsDefaultWarning()
  {
    // Arrange
    _mockSeqConnectivity.Setup(x => x.TryConnectToSeqAsync(_loggingConfig.SeqUrl, It.IsAny<CancellationToken>()))
      .ReturnsAsync(false);
    _mockConfiguration.Setup(x => x["Serilog:MinimumLevel:Default"])
      .Returns((string?)null);

    // Act
    var result = await _sut.GetLoggingStatusAsync();

    // Assert
    result.LogLevel.Should().Be("Warning", "default log level when not configured");
  }

  [Fact]
  public async Task GetLoggingStatusAsync_WithException_ThrowsAndLogsError()
  {
    // Arrange
    var exception = new Exception("Test exception");
    _mockSeqConnectivity.Setup(x => x.TryConnectToSeqAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .ThrowsAsync(exception);

    // Act & Assert
    await Assert.ThrowsAsync<Exception>(() => _sut.GetLoggingStatusAsync());

    _mockLogger.Verify(
      x => x.Log(
        LogLevel.Error,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error retrieving logging status")),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.Once,
      "error should be logged");
  }

  #endregion

  #region GetAvailableLoggingMethodsAsync Tests

  [Fact]
  public async Task GetAvailableLoggingMethodsAsync_WithAllMethodsAvailable_ReturnsAllMethods()
  {
    // Arrange
    SetupNativeSeqRunning(true);
    SetupDockerSeqRunning(true, "seq-container");
    // Setup Seq connectivity to be available
    _mockSeqConnectivity.Setup(x => x.TryConnectToSeqAsync(_loggingConfig.SeqUrl, It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);

    // Act
    var result = await _sut.GetAvailableLoggingMethodsAsync();

    // Assert
    result.Should().NotBeNull();

    result.NativeSeq.Should().NotBeNull();
    result.NativeSeq.Available.Should().BeTrue("native Seq is running");
    result.NativeSeq.ServiceName.Should().Be("seq");
    result.NativeSeq.Port.Should().Be(5341);
    result.NativeSeq.Method.Should().Be("Native systemd service");

    result.DockerSeq.Should().NotBeNull();
    result.DockerSeq.Available.Should().BeTrue("Docker Seq is running");
    result.DockerSeq.ServiceName.Should().Be("seq-container");
    result.DockerSeq.Port.Should().Be(5341);
    result.DockerSeq.Method.Should().Be("Docker container");

    result.FileLogging.Should().NotBeNull();
    result.FileLogging.Available.Should().BeTrue("file logging is always available");
    result.FileLogging.Path.Should().Contain(_loggingConfig.DefaultFileLoggingPath);
    result.FileLogging.Method.Should().Be("File-based logging (always available)");

    result.CurrentMethod.Should().Be("Native Seq Service");
  }

  [Fact]
  public async Task GetAvailableLoggingMethodsAsync_WithOnlyDockerAvailable_ReturnsDockerMethod()
  {
    // Arrange
    SetupNativeSeqRunning(false);
    SetupDockerSeqRunning(true, "seq-docker");
    _mockSeqConnectivity.Setup(x => x.TryConnectToSeqAsync(_loggingConfig.SeqUrl, It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);

    // Act
    var result = await _sut.GetAvailableLoggingMethodsAsync();

    // Assert
    result.NativeSeq.Available.Should().BeFalse();
    result.DockerSeq.Available.Should().BeTrue();
    result.DockerSeq.ServiceName.Should().Be("seq-docker");
    result.CurrentMethod.Should().Be("Docker Container");
  }

  [Fact]
  public async Task GetAvailableLoggingMethodsAsync_WithNoSeqAvailable_ReturnsFileLoggingOnly()
  {
    // Arrange
    SetupNativeSeqRunning(false);
    SetupDockerSeqRunning(false, null);
    _mockSeqConnectivity.Setup(x => x.TryConnectToSeqAsync(_loggingConfig.SeqUrl, It.IsAny<CancellationToken>()))
      .ReturnsAsync(false);

    // Act
    var result = await _sut.GetAvailableLoggingMethodsAsync();

    // Assert
    result.NativeSeq.Available.Should().BeFalse();
    result.DockerSeq.Available.Should().BeFalse();
    result.FileLogging.Available.Should().BeTrue("file logging is always available");
    result.CurrentMethod.Should().Be("File Logging (Fallback)");
  }

  #endregion

  #region GetLoggingMethodAsync Tests

  [Fact]
  public async Task GetLoggingMethodAsync_WithNativeSeqRunning_ReturnsNativeSeqService()
  {
    // Arrange
    _mockSeqConnectivity.Setup(x => x.TryConnectToSeqAsync(_loggingConfig.SeqUrl, It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);
    SetupNativeSeqRunning(true);

    // Act
    var result = await _sut.GetLoggingMethodAsync();

    // Assert
    result.Should().Be("Native Seq Service");
  }

  [Fact]
  public async Task GetLoggingMethodAsync_WithDockerSeqRunning_ReturnsDockerContainer()
  {
    // Arrange
    _mockSeqConnectivity.Setup(x => x.TryConnectToSeqAsync(_loggingConfig.SeqUrl, It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);
    SetupNativeSeqRunning(false);
    SetupDockerSeqRunning(true, "seq");

    // Act
    var result = await _sut.GetLoggingMethodAsync();

    // Assert
    result.Should().Be("Docker Container");
  }

  [Fact]
  public async Task GetLoggingMethodAsync_WithSeqUnavailable_ReturnsFileFallback()
  {
    // Arrange
    _mockSeqConnectivity.Setup(x => x.TryConnectToSeqAsync(_loggingConfig.SeqUrl, It.IsAny<CancellationToken>()))
      .ReturnsAsync(false);

    // Act
    var result = await _sut.GetLoggingMethodAsync();

    // Assert
    result.Should().Be("File Logging (Fallback)");
  }

  [Fact]
  public async Task GetLoggingMethodAsync_WithSeqAvailableButUnknownMethod_ReturnsUnknown()
  {
    // Arrange
    _mockSeqConnectivity.Setup(x => x.TryConnectToSeqAsync(_loggingConfig.SeqUrl, It.IsAny<CancellationToken>()))
      .ReturnsAsync(true);
    SetupNativeSeqRunning(false);
    SetupDockerSeqRunning(false, null);

    // Act
    var result = await _sut.GetLoggingMethodAsync();

    // Assert
    result.Should().Be("Unknown Seq Method");
  }

  #endregion

  #region Edge Cases and Error Scenarios

  [Fact]
  public async Task GetLoggingStatusAsync_WithNullConfiguredSeqUrl_HandlesGracefully()
  {
    // Arrange
    _loggingConfig.SeqUrl = null;
    _mockSeqConnectivity.Setup(x => x.GetActiveSeqUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync((string?)null);

    // Act
    var result = await _sut.GetLoggingStatusAsync();

    // Assert
    result.Should().NotBeNull();
    result.SeqAvailable.Should().BeFalse();
    result.ConfiguredSeqUrl.Should().BeNull();
    result.Method.Should().Be("File Logging (Fallback)");
  }

  [Fact]
  public async Task GetAvailableLoggingMethodsAsync_WithProcessExecutorException_HandlesGracefully()
  {
    // Arrange
    _mockProcessExecutor.Setup(x => x.RunCommandAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
      .ThrowsAsync(new Exception("Process error"));
    _mockSeqConnectivity.Setup(x => x.TryConnectToSeqAsync(_loggingConfig.SeqUrl, It.IsAny<CancellationToken>()))
      .ReturnsAsync(false);

    // Act
    var result = await _sut.GetAvailableLoggingMethodsAsync();

    // Assert
    result.Should().NotBeNull();
    result.NativeSeq.Available.Should().BeFalse("exception occurred checking native Seq");
    result.DockerSeq.Available.Should().BeFalse("exception occurred checking Docker Seq");
    result.FileLogging.Available.Should().BeTrue("file logging is always available");
  }

  [Fact]
  public async Task GetLoggingMethodAsync_WithCancellation_PropagatesCancellationToken()
  {
    // Arrange
    using var cts = new CancellationTokenSource();
    cts.Cancel();

    _mockSeqConnectivity.Setup(x => x.TryConnectToSeqAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .Returns<string?, CancellationToken>((url, ct) => Task.FromCanceled<bool>(ct));

    // Act & Assert
    await Assert.ThrowsAsync<TaskCanceledException>(() => _sut.GetLoggingMethodAsync(cts.Token));
  }

  #endregion

  #region Helper Methods

  private void SetupNativeSeqRunning(bool isRunning)
  {
    var exitCode = isRunning ? 0 : 1;
    var output = isRunning ? "active" : "inactive";

    // User service check
    _mockProcessExecutor
      .Setup(x => x.RunCommandAsync("systemctl", "--user is-active seq", It.IsAny<int>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((exitCode, output));

    // System service check (only if user service is not active)
    if (!isRunning)
    {
      _mockProcessExecutor
        .Setup(x => x.RunCommandAsync("systemctl", "is-active seq", It.IsAny<int>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((exitCode, output));
    }
  }

  private void SetupDockerSeqRunning(bool isRunning, string? containerName)
  {
    var exitCode = isRunning ? 0 : 1;
    var output = isRunning ? containerName ?? "seq" : "";

    _mockProcessExecutor
      .Setup(x => x.RunCommandAsync("docker", It.Is<string>(s => s.Contains("ps")), It.IsAny<int>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((exitCode, output));
  }

  #endregion
}
