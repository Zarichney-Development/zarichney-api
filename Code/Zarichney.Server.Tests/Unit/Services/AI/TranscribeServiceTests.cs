using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Moq;
using FluentAssertions;
using Xunit;
using OpenAI.Audio;
using Zarichney.Services.AI;
using Zarichney.Services.Email;
using Zarichney.Tests.TestData.Builders;
using Zarichney.Tests.Framework.Mocks.Factories;
using System.Text;
using Zarichney.Tests.Framework.Attributes;

namespace Zarichney.Tests.Unit.Services.AI;

[Trait(TestCategories.Category, TestCategories.Unit)]
[Trait(TestCategories.Feature, TestCategories.AI)]
public class TranscribeServiceTests
{
  private Mock<AudioClient> _mockAudioClient;
  private readonly Mock<IEmailService> _mockEmailService;
  private readonly Mock<ILogger<TranscribeService>> _mockLogger;
  private readonly TranscribeConfig _config;
  private readonly TranscribeService _sut;

  public TranscribeServiceTests()
  {
    _mockAudioClient = AudioClientMockFactory.CreateDefault();
    _mockEmailService = AudioClientMockFactory.CreateEmailServiceMock();
    _mockLogger = AudioClientMockFactory.CreateLoggerMock();
    _config = new TranscribeConfigBuilder().WithDefaults().Build();

    _sut = new TranscribeService(
        _mockAudioClient.Object,
        _config,
        _mockEmailService.Object,
        _mockLogger.Object
    );
  }

  #region Constructor Tests

  [Fact]
  public void Constructor_WithValidDependencies_InitializesSuccessfully()
  {
    // Act & Assert
    _sut.Should().NotBeNull();
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

  #region TranscribeAudioAsync Tests

  [Fact]
  public async Task TranscribeAudioAsync_WithValidStream_ReturnsTranscriptText()
  {
    // Arrange
    const string expectedTranscript = "This is the expected transcription";
    var audioStream = CreateTestAudioStream();

    _mockAudioClient = AudioClientMockFactory.CreateWithSuccessfulTranscription(expectedTranscript);
    var service = new TranscribeService(_mockAudioClient.Object, _config, _mockEmailService.Object, _mockLogger.Object);

    // Act
    var result = await service.TranscribeAudioAsync(audioStream);

    // Assert
    result.Should().Be(expectedTranscript);
  }

  [Fact]
  public async Task TranscribeAudioAsync_WithNullStream_ThrowsArgumentNullException()
  {
    // Act & Assert
    var act = async () => await _sut.TranscribeAudioAsync(null!);

    await act.Should().ThrowAsync<ArgumentNullException>()
        .WithParameterName("audioStream");
  }

  [Fact]
  public async Task TranscribeAudioAsync_WithTranscriptionFailure_RetriesAndThrows()
  {
    // Arrange
    var expectedError = new InvalidOperationException("Transcription failed");
    var audioStream = CreateTestAudioStream();

    _mockAudioClient = AudioClientMockFactory.CreateWithFailure(expectedError);
    var service = new TranscribeService(_mockAudioClient.Object, _config, _mockEmailService.Object, _mockLogger.Object);

    // Act & Assert
    var act = async () => await service.TranscribeAudioAsync(audioStream);

    await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("Transcription failed");

    _mockAudioClient.Verify(x => x.TranscribeAudioAsync(
        It.IsAny<string>(),
        It.IsAny<AudioTranscriptionOptions>()),
        Times.Exactly(_config.RetryAttempts + 1));
  }

  [Fact]
  public async Task TranscribeAudioAsync_WithDefaultOptions_UsesTextFormat()
  {
    // Arrange
    var audioStream = CreateTestAudioStream();
    _mockAudioClient = AudioClientMockFactory.CreateWithSuccessfulTranscription();
    var service = new TranscribeService(_mockAudioClient.Object, _config, _mockEmailService.Object, _mockLogger.Object);

    // Act
    await service.TranscribeAudioAsync(audioStream);

    // Assert
    _mockAudioClient.Verify(x => x.TranscribeAudioAsync(
        It.IsAny<string>(),
        It.Is<AudioTranscriptionOptions>(o => o.ResponseFormat == AudioTranscriptionFormat.Text)),
        Times.Once);
  }

  [Fact]
  public async Task TranscribeAudioAsync_WithCustomOptions_UsesProvidedOptions()
  {
    // Arrange
    var audioStream = CreateTestAudioStream();
    var customOptions = new AudioTranscriptionOptions
    {
      ResponseFormat = AudioTranscriptionFormat.Verbose
    };

    _mockAudioClient = AudioClientMockFactory.CreateWithSuccessfulTranscription();
    var service = new TranscribeService(_mockAudioClient.Object, _config, _mockEmailService.Object, _mockLogger.Object);

    // Act
    await service.TranscribeAudioAsync(audioStream, customOptions);

    // Assert
    _mockAudioClient.Verify(x => x.TranscribeAudioAsync(
        It.IsAny<string>(),
        It.Is<AudioTranscriptionOptions>(o => o.ResponseFormat == AudioTranscriptionFormat.Verbose)),
        Times.Once);
  }

  #endregion

  #region ValidateAudioFile Tests

  [Fact]
  public void ValidateAudioFile_WithNullFile_ReturnsFalseWithErrorMessage()
  {
    // Act
    var (isValid, errorMessage) = _sut.ValidateAudioFile(null);

    // Assert
    isValid.Should().BeFalse();
    errorMessage.Should().Be("Audio file is required.");
  }

  [Fact]
  public void ValidateAudioFile_WithEmptyFile_ReturnsFalseWithErrorMessage()
  {
    // Arrange
    var emptyFile = CreateMockAudioFile("test.mp3", "audio/mpeg", 0);

    // Act
    var (isValid, errorMessage) = _sut.ValidateAudioFile(emptyFile.Object);

    // Assert
    isValid.Should().BeFalse();
    errorMessage.Should().Be("Audio file must not be empty.");
  }

  [Fact]
  public void ValidateAudioFile_WithNullContentType_ReturnsFalseWithErrorMessage()
  {
    // Arrange
    var fileWithNullContentType = CreateMockAudioFile("test.mp3", null, 1024);

    // Act
    var (isValid, errorMessage) = _sut.ValidateAudioFile(fileWithNullContentType.Object);

    // Assert
    isValid.Should().BeFalse();
    errorMessage.Should().Contain("Invalid or missing content type");
  }

  [Fact]
  public void ValidateAudioFile_WithEmptyContentType_ReturnsFalseWithErrorMessage()
  {
    // Arrange
    var fileWithEmptyContentType = CreateMockAudioFile("test.mp3", "", 1024);

    // Act
    var (isValid, errorMessage) = _sut.ValidateAudioFile(fileWithEmptyContentType.Object);

    // Assert
    isValid.Should().BeFalse();
    errorMessage.Should().Contain("Invalid or missing content type");
  }

  [Fact]
  public void ValidateAudioFile_WithNonAudioContentType_ReturnsFalseWithErrorMessage()
  {
    // Arrange
    var nonAudioFile = CreateMockAudioFile("test.txt", "text/plain", 1024);

    // Act
    var (isValid, errorMessage) = _sut.ValidateAudioFile(nonAudioFile.Object);

    // Assert
    isValid.Should().BeFalse();
    errorMessage.Should().Contain("Invalid or missing content type")
        .And.Contain("Expected audio/*");
  }

  [Theory]
  [InlineData("audio/mpeg", "test.mp3")]
  [InlineData("audio/wav", "test.wav")]
  [InlineData("audio/ogg", "test.ogg")]
  [InlineData("audio/webm", "test.webm")]
  public void ValidateAudioFile_WithValidAudioFile_ReturnsTrueWithNoError(string contentType, string fileName)
  {
    // Arrange
    var validAudioFile = CreateMockAudioFile(fileName, contentType, 1024);

    // Act
    var (isValid, errorMessage) = _sut.ValidateAudioFile(validAudioFile.Object);

    // Assert
    isValid.Should().BeTrue();
    errorMessage.Should().BeNull();
  }

  #endregion

  #region ProcessAudioFileAsync Tests

  [Fact]
  public async Task ProcessAudioFileAsync_WithNullFile_ThrowsArgumentNullException()
  {
    // Act & Assert
    var act = async () => await _sut.ProcessAudioFileAsync(null!);

    await act.Should().ThrowAsync<ArgumentNullException>()
        .WithParameterName("audioFile");
  }

  [Fact]
  public async Task ProcessAudioFileAsync_WithInvalidFile_ThrowsArgumentException()
  {
    // Arrange
    var invalidFile = CreateMockAudioFile("test.txt", "text/plain", 1024);

    // Act & Assert
    var act = async () => await _sut.ProcessAudioFileAsync(invalidFile.Object);

    await act.Should().ThrowAsync<ArgumentException>()
        .WithParameterName("audioFile");
  }

  [Fact]
  public async Task ProcessAudioFileAsync_WithValidFile_ReturnsTranscriptionResult()
  {
    // Arrange
    const string expectedTranscript = "This is the processed transcription";
    var validFile = CreateMockAudioFileWithStream("test.mp3", "audio/mpeg", 1024);

    _mockAudioClient = AudioClientMockFactory.CreateWithSuccessfulTranscription(expectedTranscript);
    var service = new TranscribeService(_mockAudioClient.Object, _config, _mockEmailService.Object, _mockLogger.Object);

    // Act
    var result = await service.ProcessAudioFileAsync(validFile.Object);

    // Assert
    result.Should().NotBeNull();
    result.Transcript.Should().Be(expectedTranscript);
    result.AudioFileName.Should().Contain("test")
        .And.EndWith(".mp3");
    result.TranscriptFileName.Should().Contain("test")
        .And.EndWith(".txt");
    result.Timestamp.Should().NotBeNullOrEmpty();
  }

  [Fact]
  public async Task ProcessAudioFileAsync_WithTranscriptionFailure_SendsErrorEmailAndRethrows()
  {
    // Arrange
    var expectedError = new InvalidOperationException("Processing failed");
    var validFile = CreateMockAudioFileWithStream("test.mp3", "audio/mpeg", 1024);

    _mockAudioClient = AudioClientMockFactory.CreateWithFailure(expectedError);
    var service = new TranscribeService(_mockAudioClient.Object, _config, _mockEmailService.Object, _mockLogger.Object);

    // Act & Assert
    var act = async () => await service.ProcessAudioFileAsync(validFile.Object);

    await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("Processing failed");

    _mockEmailService.Verify(x => x.SendErrorNotification(
        "Audio Transcription",
        It.IsAny<Exception>(),
        "TranscriptionService",
        It.IsAny<Dictionary<string, string>>()),
        Times.Once);
  }

  [Fact]
  public async Task ProcessAudioFileAsync_GeneratesTimestampBasedFilenames()
  {
    // Arrange
    var validFile = CreateMockAudioFileWithStream("my test file.mp3", "audio/mpeg", 1024);

    _mockAudioClient = AudioClientMockFactory.CreateWithSuccessfulTranscription();
    var service = new TranscribeService(_mockAudioClient.Object, _config, _mockEmailService.Object, _mockLogger.Object);

    // Act
    var result = await service.ProcessAudioFileAsync(validFile.Object);

    // Assert
    result.AudioFileName.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}Z_my_test_file\.mp3");
    result.TranscriptFileName.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}Z_my_test_file\.txt");
    result.Timestamp.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}Z");
  }

  [Fact]
  public async Task ProcessAudioFileAsync_WithFileWithoutExtension_HandlesGracefully()
  {
    // Arrange
    var fileWithoutExtension = CreateMockAudioFileWithStream("audiofile", "audio/mpeg", 1024);

    _mockAudioClient = AudioClientMockFactory.CreateWithSuccessfulTranscription();
    var service = new TranscribeService(_mockAudioClient.Object, _config, _mockEmailService.Object, _mockLogger.Object);

    // Act
    var result = await service.ProcessAudioFileAsync(fileWithoutExtension.Object);

    // Assert
    result.AudioFileName.Should().Contain("audiofile")
        .And.EndWith(".mpeg");
    result.TranscriptFileName.Should().Contain("audiofile")
        .And.EndWith(".txt");
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

  private static Stream CreateTestAudioStream()
  {
    var testData = Encoding.UTF8.GetBytes("fake audio data");
    return new MemoryStream(testData);
  }

  private static Mock<IFormFile> CreateMockAudioFile(string fileName, string? contentType, long length)
  {
    var mockFile = new Mock<IFormFile>();
    mockFile.Setup(f => f.FileName).Returns(fileName);
    mockFile.Setup(f => f.ContentType).Returns(contentType ?? string.Empty);
    mockFile.Setup(f => f.Length).Returns(length);
    return mockFile;
  }

  private static Mock<IFormFile> CreateMockAudioFileWithStream(string fileName, string contentType, long length)
  {
    var mockFile = CreateMockAudioFile(fileName, contentType, length);
    var stream = CreateTestAudioStream();
    mockFile.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
           .Returns((Stream target, CancellationToken token) =>
           {
             stream.Position = 0;
             return stream.CopyToAsync(target, token);
           });
    return mockFile;
  }

  #endregion
}
