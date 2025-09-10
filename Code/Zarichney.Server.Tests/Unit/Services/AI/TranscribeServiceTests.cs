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

namespace Zarichney.Tests.Unit.Services.AI;

[Trait("Category", "Unit")]
public class TranscribeServiceTests
{
    private readonly Mock<AudioClient> _mockAudioClient;
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
        _sut.Should().NotBeNull()
            .Because("the service should initialize with valid dependencies");
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
            .WithParameterName("client")
            .Because("AudioClient is a required dependency");
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
            .WithParameterName("config")
            .Because("TranscribeConfig is a required dependency");
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
            .WithParameterName("emailService")
            .Because("IEmailService is a required dependency");
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
            .WithParameterName("logger")
            .Because("ILogger is a required dependency");
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
        result.Should().Be(expectedTranscript)
            .Because("the service should return the transcript from the AudioClient");
    }

    [Fact]
    public async Task TranscribeAudioAsync_WithNullStream_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = async () => await _sut.TranscribeAudioAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("audioStream")
            .Because("audio stream is required for transcription");
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
            .WithMessage("Transcription failed")
            .Because("the service should propagate transcription failures after retries");

        _mockAudioClient.Verify(x => x.TranscribeAudioAsync(
            It.IsAny<string>(), 
            It.IsAny<AudioTranscriptionOptions>()), 
            Times.Exactly(_config.RetryAttempts + 1),
            "the service should retry the configured number of times");
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
            Times.Once,
            "the service should use Text format by default");
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
            Times.Once,
            "the service should use the provided custom options");
    }

    #endregion

    #region ValidateAudioFile Tests

    [Fact]
    public void ValidateAudioFile_WithNullFile_ReturnsFalseWithErrorMessage()
    {
        // Act
        var (isValid, errorMessage) = _sut.ValidateAudioFile(null);

        // Assert
        isValid.Should().BeFalse()
            .Because("null audio files should be invalid");
        errorMessage.Should().Be("Audio file is required.")
            .Because("the error message should indicate that the file is required");
    }

    [Fact]
    public void ValidateAudioFile_WithEmptyFile_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        var emptyFile = CreateMockAudioFile("test.mp3", "audio/mpeg", 0);

        // Act
        var (isValid, errorMessage) = _sut.ValidateAudioFile(emptyFile.Object);

        // Assert
        isValid.Should().BeFalse()
            .Because("empty audio files should be invalid");
        errorMessage.Should().Be("Audio file must not be empty.")
            .Because("the error message should indicate that empty files are not allowed");
    }

    [Fact]
    public void ValidateAudioFile_WithNullContentType_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        var fileWithNullContentType = CreateMockAudioFile("test.mp3", null, 1024);

        // Act
        var (isValid, errorMessage) = _sut.ValidateAudioFile(fileWithNullContentType.Object);

        // Assert
        isValid.Should().BeFalse()
            .Because("files with null content type should be invalid");
        errorMessage.Should().Contain("Invalid or missing content type")
            .Because("the error message should indicate the content type issue");
    }

    [Fact]
    public void ValidateAudioFile_WithEmptyContentType_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        var fileWithEmptyContentType = CreateMockAudioFile("test.mp3", "", 1024);

        // Act
        var (isValid, errorMessage) = _sut.ValidateAudioFile(fileWithEmptyContentType.Object);

        // Assert
        isValid.Should().BeFalse()
            .Because("files with empty content type should be invalid");
        errorMessage.Should().Contain("Invalid or missing content type")
            .Because("the error message should indicate the content type issue");
    }

    [Fact]
    public void ValidateAudioFile_WithNonAudioContentType_ReturnsFalseWithErrorMessage()
    {
        // Arrange
        var nonAudioFile = CreateMockAudioFile("test.txt", "text/plain", 1024);

        // Act
        var (isValid, errorMessage) = _sut.ValidateAudioFile(nonAudioFile.Object);

        // Assert
        isValid.Should().BeFalse()
            .Because("non-audio files should be invalid");
        errorMessage.Should().Contain("Invalid or missing content type")
            .And.Contain("Expected audio/*")
            .Because("the error message should indicate the expected content type");
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
        isValid.Should().BeTrue()
            .Because("valid audio files should pass validation");
        errorMessage.Should().BeNull()
            .Because("valid files should not have error messages");
    }

    #endregion

    #region ProcessAudioFileAsync Tests

    [Fact]
    public async Task ProcessAudioFileAsync_WithNullFile_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = async () => await _sut.ProcessAudioFileAsync(null!);

        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("audioFile")
            .Because("audio file is required for processing");
    }

    [Fact]
    public async Task ProcessAudioFileAsync_WithInvalidFile_ThrowsArgumentException()
    {
        // Arrange
        var invalidFile = CreateMockAudioFile("test.txt", "text/plain", 1024);

        // Act & Assert
        var act = async () => await _sut.ProcessAudioFileAsync(invalidFile.Object);

        await act.Should().ThrowAsync<ArgumentException>()
            .WithParameterName("audioFile")
            .Because("invalid audio files should not be processed");
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
        result.Should().NotBeNull()
            .Because("the service should return a transcription result");
        result.Transcript.Should().Be(expectedTranscript)
            .Because("the transcript should match the expected result");
        result.AudioFileName.Should().Contain("test")
            .And.EndWith(".mp3")
            .Because("the audio filename should be based on the original file");
        result.TranscriptFileName.Should().Contain("test")
            .And.EndWith(".txt")
            .Because("the transcript filename should be a text file based on the original");
        result.Timestamp.Should().NotBeNullOrEmpty()
            .Because("the timestamp should be populated");
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
            .WithMessage("Processing failed")
            .Because("the service should propagate processing failures");

        _mockEmailService.Verify(x => x.SendErrorNotification(
            "Audio Transcription",
            It.IsAny<Exception>(),
            "TranscriptionService",
            It.IsAny<Dictionary<string, string>>()),
            Times.Once,
            "the service should send error notification on failure");
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
        result.AudioFileName.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}Z_my_test_file\.mp3")
            .Because("the audio filename should include timestamp and sanitized original name");
        result.TranscriptFileName.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}Z_my_test_file\.txt")
            .Because("the transcript filename should include timestamp and sanitized original name");
        result.Timestamp.Should().MatchRegex(@"\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-\d{2}Z")
            .Because("the timestamp should be in ISO 8601 format");
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
            .And.EndWith(".mpeg")
            .Because("the service should guess extension from content type");
        result.TranscriptFileName.Should().Contain("audiofile")
            .And.EndWith(".txt")
            .Because("the transcript filename should always end with .txt");
    }

    #endregion

    #region TranscribeConfig Tests

    [Fact]
    public void TranscribeConfig_DefaultValues_AreSetCorrectly()
    {
        // Arrange
        var config = new TranscribeConfig();

        // Assert
        config.ModelName.Should().Be("whisper-1")
            .Because("the default model should be whisper-1");
        config.RetryAttempts.Should().Be(5)
            .Because("the default retry attempts should be 5");
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
        config.ModelName.Should().Be("custom-model")
            .Because("the builder should set the custom model name");
        config.RetryAttempts.Should().Be(3)
            .Because("the builder should set the custom retry attempts");
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
        result.Transcript.Should().Be("Custom transcript")
            .Because("the builder should set the custom transcript");
        result.AudioFileName.Should().Be("custom_audio.mp3")
            .Because("the builder should set the custom audio filename");
        result.TranscriptFileName.Should().Be("custom_transcript.txt")
            .Because("the builder should set the custom transcript filename");
        result.Timestamp.Should().Be("2025-01-01T00-00-00Z")
            .Because("the builder should set the custom timestamp");
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
        mockFile.Setup(f => f.ContentType).Returns(contentType);
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