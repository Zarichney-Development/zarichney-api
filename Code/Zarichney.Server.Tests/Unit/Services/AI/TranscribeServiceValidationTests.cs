using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Moq;
using FluentAssertions;
using Xunit;
using OpenAI.Audio;
using Zarichney.Services.AI;
using Zarichney.Services.Email;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Server.Tests.Framework.Attributes;

namespace Zarichney.Server.Tests.Unit.Services.AI;

[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Feature, TestCategories.AI)]
public class TranscribeServiceValidationTests
{
  private readonly Mock<AudioClient> _mockAudioClient;
  private readonly Mock<IEmailService> _mockEmailService;
  private readonly Mock<ILogger<TranscribeService>> _mockLogger;
  private readonly TranscribeConfig _config;

  public TranscribeServiceValidationTests()
  {
    _mockAudioClient = new Mock<AudioClient>();
    _mockEmailService = new Mock<IEmailService>();
    _mockLogger = new Mock<ILogger<TranscribeService>>();
    _config = new TranscribeConfigBuilder().Build();

    _mockEmailService.Setup(x => x.SendErrorNotification(
            It.IsAny<string>(),
            It.IsAny<Exception>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, string>>()))
        .Returns(Task.CompletedTask);
  }

  #region Constructor Tests

  [Fact]
  public void Constructor_WithValidDependencies_InitializesSuccessfully()
  {
    // Act
    var service = new TranscribeService(
        _mockAudioClient.Object,
        _config,
        _mockEmailService.Object,
        _mockLogger.Object
    );

    // Assert
    service.Should().NotBeNull("the service should initialize with valid dependencies");
  }

  [Fact]
  public void Constructor_WithNullAudioClient_ThrowsArgumentNullException()
  {
    // Act & Assert
    var act = () => new TranscribeService(
        null!,
        _config,
        _mockEmailService.Object,
        _mockLogger.Object
    );

    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("client");
  }

  [Fact]
  public void Constructor_WithNullConfig_ThrowsArgumentNullException()
  {
    // Act & Assert
    var act = () => new TranscribeService(
        _mockAudioClient.Object,
        null!,
        _mockEmailService.Object,
        _mockLogger.Object
    );

    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("config");
  }

  [Fact]
  public void Constructor_WithNullEmailService_ThrowsArgumentNullException()
  {
    // Act & Assert
    var act = () => new TranscribeService(
        _mockAudioClient.Object,
        _config,
        null!,
        _mockLogger.Object
    );

    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("emailService");
  }

  [Fact]
  public void Constructor_WithNullLogger_ThrowsArgumentNullException()
  {
    // Act & Assert
    var act = () => new TranscribeService(
        _mockAudioClient.Object,
        _config,
        _mockEmailService.Object,
        null!
    );

    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("logger");
  }

  #endregion

  #region ValidateAudioFile Tests

  [Fact]
  public void ValidateAudioFile_WithNullFile_ReturnsFalseWithErrorMessage()
  {
    // Arrange
    var service = CreateService();

    // Act
    var (isValid, errorMessage) = service.ValidateAudioFile(null);

    // Assert
    isValid.Should().BeFalse("null audio files should be invalid");
    errorMessage.Should().Be("Audio file is required.");
  }

  [Fact]
  public void ValidateAudioFile_WithEmptyFile_ReturnsFalseWithErrorMessage()
  {
    // Arrange
    var service = CreateService();
    var emptyFile = CreateMockAudioFile("test.mp3", "audio/mpeg", 0);

    // Act
    var (isValid, errorMessage) = service.ValidateAudioFile(emptyFile.Object);

    // Assert
    isValid.Should().BeFalse("empty audio files should be invalid");
    errorMessage.Should().Be("Audio file must not be empty.");
  }

  [Fact]
  public void ValidateAudioFile_WithNullContentType_ReturnsFalseWithErrorMessage()
  {
    // Arrange
    var service = CreateService();
    var fileWithNullContentType = CreateMockAudioFile("test.mp3", null, 1024);

    // Act
    var (isValid, errorMessage) = service.ValidateAudioFile(fileWithNullContentType.Object);

    // Assert
    isValid.Should().BeFalse("files with null content type should be invalid");
    errorMessage.Should().Contain("Invalid or missing content type");
  }

  [Fact]
  public void ValidateAudioFile_WithEmptyContentType_ReturnsFalseWithErrorMessage()
  {
    // Arrange
    var service = CreateService();
    var fileWithEmptyContentType = CreateMockAudioFile("test.mp3", "", 1024);

    // Act
    var (isValid, errorMessage) = service.ValidateAudioFile(fileWithEmptyContentType.Object);

    // Assert
    isValid.Should().BeFalse("files with empty content type should be invalid");
    errorMessage.Should().Contain("Invalid or missing content type");
  }

  [Fact]
  public void ValidateAudioFile_WithNonAudioContentType_ReturnsFalseWithErrorMessage()
  {
    // Arrange
    var service = CreateService();
    var nonAudioFile = CreateMockAudioFile("test.txt", "text/plain", 1024);

    // Act
    var (isValid, errorMessage) = service.ValidateAudioFile(nonAudioFile.Object);

    // Assert
    isValid.Should().BeFalse("non-audio files should be invalid");
    errorMessage.Should().Contain("Invalid or missing content type");
    errorMessage.Should().Contain("Expected audio/*");
  }

  [Theory]
  [InlineData("audio/mpeg", "test.mp3")]
  [InlineData("audio/wav", "test.wav")]
  [InlineData("audio/ogg", "test.ogg")]
  [InlineData("audio/webm", "test.webm")]
  public void ValidateAudioFile_WithValidAudioFile_ReturnsTrueWithNoError(string contentType, string fileName)
  {
    // Arrange
    var service = CreateService();
    var validAudioFile = CreateMockAudioFile(fileName, contentType, 1024);

    // Act
    var (isValid, errorMessage) = service.ValidateAudioFile(validAudioFile.Object);

    // Assert
    isValid.Should().BeTrue("valid audio files should pass validation");
    errorMessage.Should().BeNull("valid files should not have error messages");
  }

  #endregion

  #region ProcessAudioFileAsync Input Validation Tests

  [Fact]
  public async Task ProcessAudioFileAsync_WithNullFile_ThrowsArgumentNullException()
  {
    // Arrange
    var service = CreateService();

    // Act & Assert
    var act = async () => await service.ProcessAudioFileAsync(null!);

    await act.Should().ThrowAsync<ArgumentNullException>()
        .WithParameterName("audioFile");
  }

  [Fact]
  public async Task ProcessAudioFileAsync_WithInvalidFile_ThrowsArgumentException()
  {
    // Arrange
    var service = CreateService();
    var invalidFile = CreateMockAudioFile("test.txt", "text/plain", 1024);

    // Act & Assert
    var act = async () => await service.ProcessAudioFileAsync(invalidFile.Object);

    await act.Should().ThrowAsync<ArgumentException>()
        .WithParameterName("audioFile");
  }

  #endregion

  #region TranscribeAudioAsync Input Validation Tests

  [Fact]
  public async Task TranscribeAudioAsync_WithNullStream_ThrowsArgumentNullException()
  {
    // Arrange
    var service = CreateService();

    // Act & Assert
    var act = async () => await service.TranscribeAudioAsync(null!);

    await act.Should().ThrowAsync<ArgumentNullException>()
        .WithParameterName("audioStream");
  }

  #endregion

  #region TranscribeConfig Tests

  [Fact]
  public void TranscribeConfig_DefaultValues_AreSetCorrectly()
  {
    // Arrange
    var config = new TranscribeConfig();

    // Assert
    config.ModelName.Should().Be("whisper-1");
    config.RetryAttempts.Should().Be(5);
  }

  [Fact]
  public void TranscribeConfig_WithBuilder_SetsPropertiesCorrectly()
  {
    // Arrange
    var config = new TranscribeConfigBuilder()
        .WithModelName("custom-model")
        .WithRetryAttempts(3)
        .Build();

    // Assert
    config.ModelName.Should().Be("custom-model");
    config.RetryAttempts.Should().Be(3);
  }

  #endregion

  #region TranscriptionResult Tests

  [Fact]
  public void TranscriptionResult_WithBuilder_SetsPropertiesCorrectly()
  {
    // Arrange
    var result = new TranscriptionResultBuilder()
        .WithTranscript("Custom transcript")
        .WithAudioFileName("custom_audio.mp3")
        .WithTranscriptFileName("custom_transcript.txt")
        .WithTimestamp("2025-01-01T00-00-00Z")
        .Build();

    // Assert
    result.Transcript.Should().Be("Custom transcript");
    result.AudioFileName.Should().Be("custom_audio.mp3");
    result.TranscriptFileName.Should().Be("custom_transcript.txt");
    result.Timestamp.Should().Be("2025-01-01T00-00-00Z");
  }

  #endregion

  #region Private Helper Methods

  private TranscribeService CreateService()
  {
    return new TranscribeService(
        _mockAudioClient.Object,
        _config,
        _mockEmailService.Object,
        _mockLogger.Object
    );
  }

  private static Mock<IFormFile> CreateMockAudioFile(string fileName, string? contentType, long length)
  {
    var mockFile = new Mock<IFormFile>();
    mockFile.Setup(f => f.FileName).Returns(fileName);

    // Handle null ContentType by suppressing nullable warnings since we need to test null scenarios
    mockFile.SetupGet(f => f.ContentType).Returns(contentType!);

    mockFile.Setup(f => f.Length).Returns(length);
    return mockFile;
  }

  #endregion
}
