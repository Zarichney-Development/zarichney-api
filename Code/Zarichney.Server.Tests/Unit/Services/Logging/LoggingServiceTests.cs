using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Services.Logging;
using Zarichney.Services.Logging.Models;

namespace Zarichney.Tests.Unit.Services.Logging;

/// <summary>
/// Unit tests for LoggingService - tests the orchestration layer that delegates to focused services
/// </summary>
[Trait("Category", "Unit")]
public class LoggingServiceTests
{
  private readonly IFixture _fixture;
  private readonly Mock<ILogger<LoggingService>> _mockLogger;
  private readonly Mock<ILoggingStatus> _mockLoggingStatus;
  private readonly Mock<ISeqConnectivity> _mockSeqConnectivity;
  private readonly LoggingService _sut;

  public LoggingServiceTests()
  {
    _fixture = new Fixture();
    _mockLogger = new Mock<ILogger<LoggingService>>();
    _mockLoggingStatus = new Mock<ILoggingStatus>();
    _mockSeqConnectivity = new Mock<ISeqConnectivity>();

    _sut = new LoggingService(_mockLogger.Object, _mockLoggingStatus.Object, _mockSeqConnectivity.Object);
  }

  #region GetLoggingStatusAsync Tests

  [Fact]
  public async Task GetLoggingStatusAsync_DelegatesToLoggingStatusService()
  {
    // Arrange
    var expectedResult = _fixture.Create<LoggingStatusResult>();
    _mockLoggingStatus.Setup(x => x.GetLoggingStatusAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.GetLoggingStatusAsync();

    // Assert
    result.Should().Be(expectedResult, "the service should delegate to ILoggingStatus");
    _mockLoggingStatus.Verify(x => x.GetLoggingStatusAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  #endregion

  #region TestSeqConnectivityAsync Tests

  [Fact]
  public async Task TestSeqConnectivityAsync_DelegatesToSeqConnectivityService()
  {
    // Arrange
    var expectedResult = _fixture.Create<SeqConnectivityResult>();
    var testUrl = _fixture.Create<string>();
    _mockSeqConnectivity.Setup(x => x.TestSeqConnectivityAsync(testUrl, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.TestSeqConnectivityAsync(testUrl);

    // Assert
    result.Should().Be(expectedResult, "the service should delegate to ISeqConnectivity");
    _mockSeqConnectivity.Verify(x => x.TestSeqConnectivityAsync(testUrl, It.IsAny<CancellationToken>()), Times.Once);
  }

  #endregion

  #region GetAvailableLoggingMethodsAsync Tests

  [Fact]
  public async Task GetAvailableLoggingMethodsAsync_DelegatesToLoggingStatusService()
  {
    // Arrange
    var expectedResult = _fixture.Create<LoggingMethodsResult>();
    _mockLoggingStatus.Setup(x => x.GetAvailableLoggingMethodsAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.GetAvailableLoggingMethodsAsync();

    // Assert
    result.Should().Be(expectedResult, "the service should delegate to ILoggingStatus");
    _mockLoggingStatus.Verify(x => x.GetAvailableLoggingMethodsAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  #endregion

  #region GetBestAvailableSeqUrlAsync Tests

  [Fact]
  public async Task GetBestAvailableSeqUrlAsync_DelegatesToSeqConnectivityService()
  {
    // Arrange
    var expectedResult = _fixture.Create<string>();
    _mockSeqConnectivity.Setup(x => x.GetBestAvailableSeqUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.GetBestAvailableSeqUrlAsync();

    // Assert
    result.Should().Be(expectedResult, "the service should delegate to ISeqConnectivity");
    _mockSeqConnectivity.Verify(x => x.GetBestAvailableSeqUrlAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  #endregion

  #region TryConnectToSeqAsync Tests

  [Fact]
  public async Task TryConnectToSeqAsync_DelegatesToSeqConnectivityService()
  {
    // Arrange
    var expectedResult = _fixture.Create<bool>();
    var testUrl = _fixture.Create<string>();
    _mockSeqConnectivity.Setup(x => x.TryConnectToSeqAsync(testUrl, It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.TryConnectToSeqAsync(testUrl);

    // Assert
    result.Should().Be(expectedResult, "the service should delegate to ISeqConnectivity");
    _mockSeqConnectivity.Verify(x => x.TryConnectToSeqAsync(testUrl, It.IsAny<CancellationToken>()), Times.Once);
  }

  #endregion

  #region TryStartDockerSeqAsync Tests

  [Fact]
  public async Task TryStartDockerSeqAsync_DelegatesToSeqConnectivityService()
  {
    // Arrange
    var expectedResult = _fixture.Create<bool>();
    _mockSeqConnectivity.Setup(x => x.TryStartDockerSeqAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.TryStartDockerSeqAsync();

    // Assert
    result.Should().Be(expectedResult, "the service should delegate to ISeqConnectivity");
    _mockSeqConnectivity.Verify(x => x.TryStartDockerSeqAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  #endregion

  #region GetActiveSeqUrlAsync Tests

  [Fact]
  public async Task GetActiveSeqUrlAsync_DelegatesToSeqConnectivityService()
  {
    // Arrange
    var expectedResult = _fixture.Create<string>();
    _mockSeqConnectivity.Setup(x => x.GetActiveSeqUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.GetActiveSeqUrlAsync();

    // Assert
    result.Should().Be(expectedResult, "the service should delegate to ISeqConnectivity");
    _mockSeqConnectivity.Verify(x => x.GetActiveSeqUrlAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  #endregion

  #region GetLoggingMethodAsync Tests

  [Fact]
  public async Task GetLoggingMethodAsync_DelegatesToLoggingStatusService()
  {
    // Arrange
    var expectedResult = _fixture.Create<string>();
    _mockLoggingStatus.Setup(x => x.GetLoggingMethodAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.GetLoggingMethodAsync();

    // Assert
    result.Should().Be(expectedResult, "the service should delegate to ILoggingStatus");
    _mockLoggingStatus.Verify(x => x.GetLoggingMethodAsync(It.IsAny<CancellationToken>()), Times.Once);
  }

  #endregion
}
