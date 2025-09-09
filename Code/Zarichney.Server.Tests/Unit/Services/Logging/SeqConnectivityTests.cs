using System.Net;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using Zarichney.Services.Logging;
using Zarichney.Services.Logging.Models;
using Zarichney.Services.ProcessExecution;

namespace Zarichney.Tests.Unit.Services.Logging;

/// <summary>
/// Unit tests for SeqConnectivity service - tests Seq connectivity and container management
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Logging")]
[Trait("Feature", "SeqConnectivity")]
public class SeqConnectivityTests
{
  private readonly IFixture _fixture;
  private readonly Mock<ILogger<SeqConnectivity>> _mockLogger;
  private readonly Mock<IOptions<LoggingConfig>> _mockConfig;
  private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
  private readonly HttpClient _httpClient;
  private readonly Mock<IProcessExecutor> _mockProcessExecutor;
  private readonly LoggingConfig _loggingConfig;
  private readonly SeqConnectivity _sut;

  public SeqConnectivityTests()
  {
    _fixture = new Fixture();
    _mockLogger = new Mock<ILogger<SeqConnectivity>>();
    _mockConfig = new Mock<IOptions<LoggingConfig>>();
    _mockHttpMessageHandler = new Mock<HttpMessageHandler>();
    _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
    _mockProcessExecutor = new Mock<IProcessExecutor>();

    _loggingConfig = new LoggingConfig
    {
      SeqUrl = "http://localhost:5341",
      SeqTimeoutSeconds = 5,
      EnableDockerFallback = true,
      DockerContainerName = "seq",
      DockerImage = "datalust/seq:latest",
      ProcessTimeoutMs = 5000,
      CommonSeqUrls = ["http://localhost:5341", "http://localhost:80"]
    };

    _mockConfig.Setup(x => x.Value).Returns(_loggingConfig);

    _sut = new SeqConnectivity(
      _mockLogger.Object,
      _mockConfig.Object,
      _httpClient,
      _mockProcessExecutor.Object);
  }

  #region TestSeqConnectivityAsync Tests

  [Fact]
  public async Task TestSeqConnectivityAsync_WithSuccessfulConnection_ReturnsConnectedResult()
  {
    // Arrange
    var testUrl = "http://test.seq:5341";
    SetupHttpResponse(HttpStatusCode.OK, testUrl);

    // Act
    var result = await _sut.TestSeqConnectivityAsync(testUrl);

    // Assert
    result.Should().NotBeNull();
    result.Url.Should().Be(testUrl);
    result.IsConnected.Should().BeTrue("connection was successful");
    result.ResponseTime.Should().BeGreaterThan(-1, "response time should be measured");
    result.Error.Should().BeNull();
    result.TestedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
  }

  [Fact]
  public async Task TestSeqConnectivityAsync_WithFailedConnection_ReturnsDisconnectedResult()
  {
    // Arrange
    var testUrl = "http://test.seq:5341";
    SetupHttpResponse(HttpStatusCode.InternalServerError, testUrl);

    // Act
    var result = await _sut.TestSeqConnectivityAsync(testUrl);

    // Assert
    result.Should().NotBeNull();
    result.Url.Should().Be(testUrl);
    result.IsConnected.Should().BeFalse("connection failed");
    result.ResponseTime.Should().Be(-1, "response time should be -1 for failed connections");
    result.Error.Should().BeNull();
  }

  [Fact]
  public async Task TestSeqConnectivityAsync_WithNullUrl_UsesConfiguredUrl()
  {
    // Arrange
    SetupHttpResponse(HttpStatusCode.OK, _loggingConfig.SeqUrl!);

    // Act
    var result = await _sut.TestSeqConnectivityAsync(null);

    // Assert
    result.Should().NotBeNull();
    result.Url.Should().Be(_loggingConfig.SeqUrl);
    result.IsConnected.Should().BeTrue();
  }

  [Fact]
  public async Task TestSeqConnectivityAsync_WithException_ReturnsErrorResult()
  {
    // Arrange
    var testUrl = "http://test.seq:5341";
    SetupHttpResponse(HttpStatusCode.InternalServerError, testUrl);

    // Act
    var result = await _sut.TestSeqConnectivityAsync(testUrl);

    // Assert
    result.Should().NotBeNull();
    result.Url.Should().Be(testUrl);
    result.IsConnected.Should().BeFalse("connection failed with error status");
    result.ResponseTime.Should().Be(-1);
    result.Error.Should().BeNull("error is only set when an exception is thrown");
  }

  #endregion

  #region TryConnectToSeqAsync Tests

  [Fact]
  public async Task TryConnectToSeqAsync_WithValidUrl_ReturnsTrue()
  {
    // Arrange
    var testUrl = "http://test.seq:5341";
    SetupHttpResponse(HttpStatusCode.OK, testUrl);

    // Act
    var result = await _sut.TryConnectToSeqAsync(testUrl);

    // Assert
    result.Should().BeTrue("connection was successful");
  }

  [Fact]
  public async Task TryConnectToSeqAsync_WithInvalidUrl_ReturnsFalse()
  {
    // Arrange
    var testUrl = "not-a-valid-url";

    // Act
    var result = await _sut.TryConnectToSeqAsync(testUrl);

    // Assert
    result.Should().BeFalse("URL is not well-formed");
  }

  [Fact]
  public async Task TryConnectToSeqAsync_WithNullUrl_ReturnsFalse()
  {
    // Arrange & Act
    var result = await _sut.TryConnectToSeqAsync(null);

    // Assert
    result.Should().BeFalse("URL is null");
  }

  [Fact]
  public async Task TryConnectToSeqAsync_WithEmptyUrl_ReturnsFalse()
  {
    // Arrange & Act
    var result = await _sut.TryConnectToSeqAsync("");

    // Assert
    result.Should().BeFalse("URL is empty");
  }

  [Fact]
  public async Task TryConnectToSeqAsync_WithTimeout_ReturnsFalse()
  {
    // Arrange
    var testUrl = "http://test.seq:5341";
    SetupHttpTimeout(testUrl);

    // Act
    var result = await _sut.TryConnectToSeqAsync(testUrl);

    // Assert
    result.Should().BeFalse("connection timed out");
  }

  [Fact]
  public async Task TryConnectToSeqAsync_WithCancellation_ThrowsOperationCanceledException()
  {
    // Arrange
    var testUrl = "http://test.seq:5341";
    using var cts = new CancellationTokenSource();
    cts.Cancel();

    SetupHttpCancellation(testUrl);

    // Act & Assert
    // TaskCanceledException derives from OperationCanceledException
    await Assert.ThrowsAsync<TaskCanceledException>(
      () => _sut.TryConnectToSeqAsync(testUrl, cts.Token));
  }

  #endregion

  #region GetBestAvailableSeqUrlAsync Tests

  [Fact]
  public async Task GetBestAvailableSeqUrlAsync_WithConfiguredUrlAvailable_ReturnsConfiguredUrl()
  {
    // Arrange
    SetupHttpResponse(HttpStatusCode.OK, _loggingConfig.SeqUrl!);

    // Act
    var result = await _sut.GetBestAvailableSeqUrlAsync();

    // Assert
    result.Should().Be(_loggingConfig.SeqUrl);
  }

  [Fact]
  public async Task GetBestAvailableSeqUrlAsync_WithConfiguredUrlUnavailable_TriesCommonUrls()
  {
    // Arrange
    var expectedUrl = _loggingConfig.CommonSeqUrls[1];

    // Use SetupSequence to setup different responses for the same URL endpoint
    _mockHttpMessageHandler.Protected()
      .SetupSequence<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.InternalServerError,
        Content = new StringContent("{}")
      }) // First call to configured URL fails
      .ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.InternalServerError,
        Content = new StringContent("{}")
      }) // Second call to first common URL fails
      .ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent("{}")
      }); // Third call to second common URL succeeds

    // Act
    var result = await _sut.GetBestAvailableSeqUrlAsync();

    // Assert
    result.Should().Be(expectedUrl, "should return the first available common URL");
  }

  [Fact]
  public async Task GetBestAvailableSeqUrlAsync_WithNoAvailableUrls_TriesDockerFallback()
  {
    // Arrange
    // Setup sequence for HTTP calls
    _mockHttpMessageHandler.Protected()
      .SetupSequence<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.IsAny<HttpRequestMessage>(),
        ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.InternalServerError,
        Content = new StringContent("{}")
      }) // Configured URL fails
      .ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.InternalServerError,
        Content = new StringContent("{}")
      }) // First common URL fails
      .ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.InternalServerError,
        Content = new StringContent("{}")
      }) // Second common URL fails
      .ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = HttpStatusCode.OK,
        Content = new StringContent("{}")
      }); // After Docker starts, localhost:5341 succeeds

    _mockProcessExecutor
      .Setup(x => x.RunCommandAsync("docker", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((0, "seq"));

    // Act
    var result = await _sut.GetBestAvailableSeqUrlAsync();

    // Assert
    result.Should().Be("http://localhost:5341", "should return Docker Seq URL after starting container");
    _mockProcessExecutor.Verify(
      x => x.RunCommandAsync("docker", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
      Times.AtLeastOnce,
      "should attempt to start Docker container");
  }

  [Fact]
  public async Task GetBestAvailableSeqUrlAsync_WithAllUnavailable_ReturnsNull()
  {
    // Arrange
    SetupAllUrlsUnavailable();
    _loggingConfig.EnableDockerFallback = false;

    // Act
    var result = await _sut.GetBestAvailableSeqUrlAsync();

    // Assert
    result.Should().BeNull("no Seq instance is available");
  }

  #endregion

  #region GetActiveSeqUrlAsync Tests

  [Fact]
  public async Task GetActiveSeqUrlAsync_WithConfiguredUrlActive_ReturnsConfiguredUrl()
  {
    // Arrange
    SetupHttpResponse(HttpStatusCode.OK, _loggingConfig.SeqUrl!);

    // Act
    var result = await _sut.GetActiveSeqUrlAsync();

    // Assert
    result.Should().Be(_loggingConfig.SeqUrl);
  }

  [Fact]
  public async Task GetActiveSeqUrlAsync_WithNoActiveUrl_ReturnsNull()
  {
    // Arrange
    SetupAllUrlsUnavailable();

    // Act
    var result = await _sut.GetActiveSeqUrlAsync();

    // Assert
    result.Should().BeNull("no active Seq URL found");
  }

  #endregion

  #region TryStartDockerSeqAsync Tests

  [Fact]
  public async Task TryStartDockerSeqAsync_WithDockerDisabled_ReturnsFalse()
  {
    // Arrange
    _loggingConfig.EnableDockerFallback = false;

    // Act
    var result = await _sut.TryStartDockerSeqAsync();

    // Assert
    result.Should().BeFalse("Docker fallback is disabled");
    _mockProcessExecutor.Verify(
      x => x.RunCommandAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()),
      Times.Never,
      "should not attempt to run Docker commands");
  }

  [Fact]
  public async Task TryStartDockerSeqAsync_WithContainerAlreadyRunning_ReturnsTrue()
  {
    // Arrange
    _mockProcessExecutor
      .Setup(x => x.RunCommandAsync("docker", It.Is<string>(s => s.Contains("ps")), It.IsAny<int>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((0, "seq"));

    // Act
    var result = await _sut.TryStartDockerSeqAsync();

    // Assert
    result.Should().BeTrue("container is already running");
    _mockProcessExecutor.Verify(
      x => x.RunCommandAsync("docker", It.Is<string>(s => s.Contains("run")), It.IsAny<int>(), It.IsAny<CancellationToken>()),
      Times.Never,
      "should not attempt to start a new container");
  }

  [Fact]
  public async Task TryStartDockerSeqAsync_WithSuccessfulStart_ReturnsTrue()
  {
    // Arrange
    _mockProcessExecutor
      .Setup(x => x.RunCommandAsync("docker", It.Is<string>(s => s.Contains("ps")), It.IsAny<int>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((0, "")); // No container running

    _mockProcessExecutor
      .Setup(x => x.RunCommandAsync("docker", It.Is<string>(s => s.Contains("run")), It.IsAny<int>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((0, "container-id"));

    // Act
    var result = await _sut.TryStartDockerSeqAsync();

    // Assert
    result.Should().BeTrue("container started successfully");
    _mockProcessExecutor.Verify(
      x => x.RunCommandAsync("docker", It.Is<string>(s => s.Contains("run")), It.IsAny<int>(), It.IsAny<CancellationToken>()),
      Times.Once,
      "should attempt to start container");
  }

  [Fact]
  public async Task TryStartDockerSeqAsync_WithFailedStart_ReturnsFalse()
  {
    // Arrange
    _mockProcessExecutor
      .Setup(x => x.RunCommandAsync("docker", It.Is<string>(s => s.Contains("ps")), It.IsAny<int>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((0, "")); // No container running

    _mockProcessExecutor
      .Setup(x => x.RunCommandAsync("docker", It.Is<string>(s => s.Contains("run")), It.IsAny<int>(), It.IsAny<CancellationToken>()))
      .ReturnsAsync((1, "")); // Failed to start

    // Act
    var result = await _sut.TryStartDockerSeqAsync();

    // Assert
    result.Should().BeFalse("container failed to start");
  }

  [Fact]
  public async Task TryStartDockerSeqAsync_WithException_ReturnsFalse()
  {
    // Arrange
    _mockProcessExecutor
      .Setup(x => x.RunCommandAsync("docker", It.IsAny<string>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
      .ThrowsAsync(new Exception("Docker not available"));

    // Act
    var result = await _sut.TryStartDockerSeqAsync();

    // Assert
    result.Should().BeFalse("exception occurred");
  }

  #endregion

  #region Helper Methods

  private void SetupHttpResponse(HttpStatusCode statusCode, string url)
  {
    _mockHttpMessageHandler.Protected()
      .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().StartsWith(url)),
        ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = statusCode,
        Content = new StringContent("{}")
      });
  }

  private void SetupHttpResponseSequence(string url, params HttpStatusCode[] statusCodes)
  {
    var sequence = _mockHttpMessageHandler.Protected()
      .SetupSequence<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().StartsWith(url)),
        ItExpr.IsAny<CancellationToken>());

    foreach (var statusCode in statusCodes)
    {
      sequence.ReturnsAsync(new HttpResponseMessage
      {
        StatusCode = statusCode,
        Content = new StringContent("{}")
      });
    }
  }

  private void SetupHttpException(Exception exception, string url)
  {
    _mockHttpMessageHandler.Protected()
      .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().StartsWith(url)),
        ItExpr.IsAny<CancellationToken>())
      .ThrowsAsync(exception);
  }

  private void SetupHttpTimeout(string url)
  {
    _mockHttpMessageHandler.Protected()
      .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().StartsWith(url)),
        ItExpr.IsAny<CancellationToken>())
      .ThrowsAsync(new TaskCanceledException());
  }

  private void SetupHttpCancellation(string url)
  {
    _mockHttpMessageHandler.Protected()
      .Setup<Task<HttpResponseMessage>>(
        "SendAsync",
        ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString().StartsWith(url)),
        ItExpr.IsAny<CancellationToken>())
      .ThrowsAsync(new OperationCanceledException());
  }

  private void SetupAllUrlsUnavailable()
  {
    SetupHttpResponse(HttpStatusCode.InternalServerError, _loggingConfig.SeqUrl!);
    foreach (var url in _loggingConfig.CommonSeqUrls)
    {
      SetupHttpResponse(HttpStatusCode.InternalServerError, url);
    }
  }

  #endregion
}
