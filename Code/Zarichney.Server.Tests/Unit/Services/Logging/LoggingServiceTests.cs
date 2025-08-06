using System.Diagnostics;
using System.Net;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using Xunit;
using Zarichney.Services.Logging;
using Zarichney.Services.Logging.Models;

namespace Zarichney.Tests.Unit.Services.Logging;

/// <summary>
/// Unit tests for LoggingService
/// </summary>
[Trait("Category", "Unit")]
public class LoggingServiceTests
{
  private readonly IFixture _fixture;
  private readonly Mock<ILogger<LoggingService>> _mockLogger;
  private readonly Mock<IConfiguration> _mockConfiguration;
  private readonly Mock<IOptions<LoggingConfig>> _mockOptions;
  private readonly Mock<HttpMessageHandler> _mockHttpHandler;
  private readonly HttpClient _httpClient;
  private readonly LoggingConfig _config;
  private readonly LoggingService _sut;

  public LoggingServiceTests()
  {
    _fixture = new Fixture();
    _mockLogger = new Mock<ILogger<LoggingService>>();
    _mockConfiguration = new Mock<IConfiguration>();
    _mockOptions = new Mock<IOptions<LoggingConfig>>();
    _mockHttpHandler = new Mock<HttpMessageHandler>();
    
    _httpClient = new HttpClient(_mockHttpHandler.Object);
    
    _config = _fixture.Build<LoggingConfig>()
      .With(x => x.SeqUrl, "http://localhost:5341")
      .With(x => x.SeqTimeoutSeconds, 3)
      .With(x => x.CommonSeqUrls, new[] { "http://localhost:5341", "http://127.0.0.1:5341" })
      .With(x => x.EnableDockerFallback, true)
      .Create();

    _mockOptions.Setup(x => x.Value).Returns(_config);

    _sut = new LoggingService(_mockLogger.Object, _mockConfiguration.Object, _mockOptions.Object, _httpClient);
  }

  #region GetLoggingStatusAsync Tests

  [Fact]
  public async Task GetLoggingStatusAsync_WhenSeqAvailable_ReturnsCorrectStatus()
  {
    // Arrange
    SetupSuccessfulHttpResponse();
    SetupConfigurationValue("Serilog:MinimumLevel:Default", "Information");

    // Act
    var result = await _sut.GetLoggingStatusAsync();

    // Assert
    result.Should().NotBeNull("the service should return a status result");
    result.SeqAvailable.Should().BeTrue("Seq is configured and responding");
    result.SeqUrl.Should().Be(_config.SeqUrl, "the configured URL should be used");
    result.Method.Should().NotBeEmpty("a logging method should be determined");
    result.FallbackActive.Should().BeFalse("Seq is available");
    result.ConfiguredSeqUrl.Should().Be(_config.SeqUrl, "the configured URL should be reported");
    result.LogLevel.Should().Be("Information", "the configured log level should be returned");
    result.FileLoggingPath.Should().NotBeEmpty("file logging path should always be available");
    result.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5), "timestamp should be recent");
  }

  [Fact]
  public async Task GetLoggingStatusAsync_WhenSeqUnavailable_ReturnsFallbackStatus()
  {
    // Arrange
    SetupFailedHttpResponse();
    SetupConfigurationValue("Serilog:MinimumLevel:Default", "Warning");

    // Act
    var result = await _sut.GetLoggingStatusAsync();

    // Assert
    result.Should().NotBeNull("the service should return a status result");
    result.SeqAvailable.Should().BeFalse("Seq is not responding");
    result.FallbackActive.Should().BeTrue("Seq is not available");
    result.Method.Should().Contain("Fallback", "fallback logging should be used");
    result.LogLevel.Should().Be("Warning", "the configured log level should be returned");
  }

  [Fact]
  public async Task GetLoggingStatusAsync_WhenConfigurationMissing_UsesDefaults()
  {
    // Arrange
    SetupFailedHttpResponse();
    SetupConfigurationValue("Serilog:MinimumLevel:Default", null);

    // Act
    var result = await _sut.GetLoggingStatusAsync();

    // Assert
    result.LogLevel.Should().Be("Warning", "Warning should be the default log level");
  }

  [Fact]
  public async Task GetLoggingStatusAsync_WhenExceptionThrown_LogsErrorAndRethrows()
  {
    // Arrange
    _mockHttpHandler.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
      .ThrowsAsync(new InvalidOperationException("Test exception"));

    // Act & Assert
    var act = async () => await _sut.GetLoggingStatusAsync();
    await act.Should().ThrowAsync<InvalidOperationException>()
      .WithMessage("Test exception", "exceptions should be propagated to the caller");

    VerifyLoggerWasCalled(LogLevel.Error, "Error retrieving logging status");
  }

  #endregion

  #region TestSeqConnectivityAsync Tests

  [Fact]
  public async Task TestSeqConnectivityAsync_WithValidUrl_ReturnsSuccessResult()
  {
    // Arrange
    var testUrl = "http://test.seq:5341";
    SetupSuccessfulHttpResponse();

    // Act
    var result = await _sut.TestSeqConnectivityAsync(testUrl);

    // Assert
    result.Should().NotBeNull("the service should return a connectivity result");
    result.Url.Should().Be(testUrl, "the tested URL should be returned");
    result.IsConnected.Should().BeTrue("the HTTP request was successful");
    result.ResponseTime.Should().BeGreaterThan(0, "response time should be measured");
    result.TestedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5), "timestamp should be recent");
    result.Error.Should().BeNull("no error occurred");
  }

  [Fact]
  public async Task TestSeqConnectivityAsync_WithFailedConnection_ReturnsFailureResult()
  {
    // Arrange
    var testUrl = "http://test.seq:5341";
    SetupFailedHttpResponse();

    // Act
    var result = await _sut.TestSeqConnectivityAsync(testUrl);

    // Assert
    result.Should().NotBeNull("the service should return a connectivity result");
    result.Url.Should().Be(testUrl, "the tested URL should be returned");
    result.IsConnected.Should().BeFalse("the HTTP request failed");
    result.ResponseTime.Should().Be(-1, "response time should be -1 for failed connections");
  }

  [Fact]
  public async Task TestSeqConnectivityAsync_WithoutUrl_UsesConfiguredUrl()
  {
    // Arrange
    SetupSuccessfulHttpResponse();

    // Act
    var result = await _sut.TestSeqConnectivityAsync();

    // Assert
    result.Url.Should().Be(_config.SeqUrl, "the configured URL should be used when no URL is provided");
    result.IsConnected.Should().BeTrue("the connection should succeed");
  }

  [Fact]
  public async Task TestSeqConnectivityAsync_WithException_ReturnsErrorResult()
  {
    // Arrange
    var testUrl = "http://test.seq:5341";
    var exceptionMessage = "Network error";
    
    _mockHttpHandler.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
      .ThrowsAsync(new HttpRequestException(exceptionMessage));

    // Act
    var result = await _sut.TestSeqConnectivityAsync(testUrl);

    // Assert
    result.Should().NotBeNull("the service should return a result even on exception");
    result.IsConnected.Should().BeFalse("the connection failed due to exception");
    result.Error.Should().Be(exceptionMessage, "the exception message should be captured");
    result.ResponseTime.Should().Be(-1, "response time should be -1 for failed connections");
  }

  #endregion

  #region GetAvailableLoggingMethodsAsync Tests

  [Fact]
  public async Task GetAvailableLoggingMethodsAsync_ReturnsAllMethodInfo()
  {
    // Arrange
    SetupSuccessfulHttpResponse(); // For Seq availability check

    // Act
    var result = await _sut.GetAvailableLoggingMethodsAsync();

    // Assert
    result.Should().NotBeNull("the service should return logging methods information");
    result.NativeSeq.Should().NotBeNull("native Seq information should be included");
    result.DockerSeq.Should().NotBeNull("Docker Seq information should be included");
    result.FileLogging.Should().NotBeNull("file logging information should be included");
    result.FileLogging.Available.Should().BeTrue("file logging is always available");
    result.FileLogging.Method.Should().Contain("File-based", "file logging method should be described");
    result.CurrentMethod.Should().NotBeEmpty("current method should be determined");
  }

  #endregion

  #region TryConnectToSeqAsync Tests

  [Fact]
  public async Task TryConnectToSeqAsync_WithValidUrl_ReturnsTrue()
  {
    // Arrange
    var url = "http://localhost:5341";
    SetupSuccessfulHttpResponse();

    // Act
    var result = await _sut.TryConnectToSeqAsync(url);

    // Assert
    result.Should().BeTrue("the HTTP request should succeed");
    
    _mockHttpHandler.Protected().Verify(
      "SendAsync",
      Times.Once(),
      ItExpr.Is<HttpRequestMessage>(req => req.RequestUri!.ToString() == $"{url}/api/events/raw"),
      ItExpr.IsAny<CancellationToken>());
  }

  [Fact]
  public async Task TryConnectToSeqAsync_WithInvalidUrl_ReturnsFalse()
  {
    // Arrange
    var invalidUrls = new[] { "", null, "not-a-url", "ftp://invalid" };

    foreach (var url in invalidUrls)
    {
      // Act
      var result = await _sut.TryConnectToSeqAsync(url);

      // Assert
      result.Should().BeFalse($"invalid URL '{url}' should return false");
    }
  }

  [Fact]
  public async Task TryConnectToSeqAsync_WithTimeout_ReturnsFalse()
  {
    // Arrange
    var url = "http://localhost:5341";
    _config.SeqTimeoutSeconds = 1;
    
    _mockHttpHandler.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
      .Returns(async (HttpRequestMessage request, CancellationToken ct) =>
      {
        await Task.Delay(2000, ct); // Longer than timeout
        return new HttpResponseMessage(HttpStatusCode.OK);
      });

    // Act
    var result = await _sut.TryConnectToSeqAsync(url);

    // Assert
    result.Should().BeFalse("the request should timeout");
  }

  [Fact]
  public async Task TryConnectToSeqAsync_WithCancellation_ThrowsOperationCanceledException()
  {
    // Arrange
    var url = "http://localhost:5341";
    using var cts = new CancellationTokenSource();
    cts.Cancel();

    // Act & Assert
    var act = async () => await _sut.TryConnectToSeqAsync(url, cts.Token);
    await act.Should().ThrowAsync<OperationCanceledException>("cancellation should be respected");
  }

  #endregion

  #region GetBestAvailableSeqUrlAsync Tests

  [Fact]
  public async Task GetBestAvailableSeqUrlAsync_WhenConfiguredUrlWorks_ReturnsConfiguredUrl()
  {
    // Arrange
    SetupSuccessfulHttpResponse();

    // Act
    var result = await _sut.GetBestAvailableSeqUrlAsync();

    // Assert
    result.Should().Be(_config.SeqUrl, "configured URL should be preferred when available");
    VerifyLoggerWasCalled(LogLevel.Information, $"Using configured Seq at: {_config.SeqUrl}");
  }

  [Fact]
  public async Task GetBestAvailableSeqUrlAsync_WhenConfiguredUrlFails_TriesCommonUrls()
  {
    // Arrange
    _config.SeqUrl = "http://nonexistent:5341";
    var workingUrl = "http://127.0.0.1:5341";
    
    SetupHttpResponsesBasedOnUrl(url =>
      url.Contains("nonexistent") ? HttpStatusCode.ServiceUnavailable :
      url.Contains("127.0.0.1") ? HttpStatusCode.OK :
      HttpStatusCode.ServiceUnavailable);

    // Act
    var result = await _sut.GetBestAvailableSeqUrlAsync();

    // Assert
    result.Should().Be(workingUrl, "the first working common URL should be returned");
    VerifyLoggerWasCalled(LogLevel.Information, $"Found native Seq at: {workingUrl}");
  }

  [Fact]
  public async Task GetBestAvailableSeqUrlAsync_WhenNoSeqFound_ReturnsNull()
  {
    // Arrange
    SetupFailedHttpResponse();
    _config.EnableDockerFallback = false;

    // Act
    var result = await _sut.GetBestAvailableSeqUrlAsync();

    // Assert
    result.Should().BeNull("no Seq instances are available");
    VerifyLoggerWasCalled(LogLevel.Warning, "No Seq instance found - will use file logging");
  }

  #endregion

  #region Helper Methods

  private void SetupSuccessfulHttpResponse()
  {
    _mockHttpHandler.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
  }

  private void SetupFailedHttpResponse()
  {
    _mockHttpHandler.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
  }

  private void SetupHttpResponsesBasedOnUrl(Func<string, HttpStatusCode> statusCodeSelector)
  {
    _mockHttpHandler.Protected()
      .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
      .ReturnsAsync((HttpRequestMessage request, CancellationToken ct) =>
      {
        var statusCode = statusCodeSelector(request.RequestUri!.ToString());
        return new HttpResponseMessage(statusCode);
      });
  }

  private void SetupConfigurationValue(string key, string? value)
  {
    _mockConfiguration.Setup(x => x[key]).Returns(value);
  }

  private void VerifyLoggerWasCalled(LogLevel level, string message)
  {
    _mockLogger.Verify(
      x => x.Log(
        level,
        It.IsAny<EventId>(),
        It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(message)),
        It.IsAny<Exception>(),
        It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
      Times.AtLeastOnce,
      $"Logger should have been called with level {level} and message containing '{message}'");
  }

  #endregion
}