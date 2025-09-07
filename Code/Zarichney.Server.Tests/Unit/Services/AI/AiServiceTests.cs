using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Moq;
using FluentAssertions;
using Xunit;
using Zarichney.Services.AI;
using Zarichney.Services.Email;
using Zarichney.Services.GitHub;
using Zarichney.Services.Sessions;
using Zarichney.Services.Status;

namespace Zarichney.Tests.Unit.Services.AI;

[Trait("Category", "Unit")]
public class AiServiceTests
{
    private readonly Mock<ILlmService> _mockLlmService;
    private readonly Mock<ITranscribeService> _mockTranscribeService;
    private readonly Mock<IGitHubService> _mockGitHubService;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ISessionManager> _mockSessionManager;
    private readonly Mock<IScopeContainer> _mockScope;
    private readonly Mock<ILogger<AiService>> _mockLogger;
    private readonly AiService _sut;

    public AiServiceTests()
    {
        _mockLlmService = new Mock<ILlmService>();
        _mockTranscribeService = new Mock<ITranscribeService>();
        _mockGitHubService = new Mock<IGitHubService>();
        _mockEmailService = new Mock<IEmailService>();
        _mockSessionManager = new Mock<ISessionManager>();
        _mockScope = new Mock<IScopeContainer>();
        _mockLogger = new Mock<ILogger<AiService>>();

        _sut = new AiService(
            _mockLlmService.Object,
            _mockTranscribeService.Object,
            _mockGitHubService.Object,
            _mockEmailService.Object,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_InitializesSuccessfully()
    {
        // Arrange & Act
        var service = new AiService(
            _mockLlmService.Object,
            _mockTranscribeService.Object,
            _mockGitHubService.Object,
            _mockEmailService.Object,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Assert
        service.Should().NotBeNull("service should initialize with valid dependencies");
        service.Should().BeAssignableTo<IAiService>("service should implement IAiService interface");
    }

    #endregion

    #region ProcessAudioTranscriptionAsync Tests

    [Fact]
    public async Task ProcessAudioTranscriptionAsync_NullAudioFile_ThrowsArgumentNullException()
    {
        // Act
        Func<Task> act = async () => await _sut.ProcessAudioTranscriptionAsync(null!);

        // Assert
        var exception = await act.Should().ThrowAsync<ArgumentNullException>();
        exception.WithParameterName("audioFile");
    }

    [Fact]
    public async Task ProcessAudioTranscriptionAsync_InvalidAudioFile_ThrowsArgumentException()
    {
        // Arrange
        var mockAudioFile = CreateMockFormFile("test.txt", "text/plain");
        _mockTranscribeService.Setup(x => x.ValidateAudioFile(It.IsAny<IFormFile>()))
            .Returns((false, "Invalid audio file format"));

        // Act
        Func<Task> act = async () => await _sut.ProcessAudioTranscriptionAsync(mockAudioFile);

        // Assert
        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.WithParameterName("audioFile");
        exception.WithMessage("*Invalid audio file format*");
    }

    [Fact]
    public async Task ProcessAudioTranscriptionAsync_ValidAudioFile_ReturnsSuccessResult()
    {
        // Arrange
        var mockAudioFile = CreateMockFormFile("test.wav", "audio/wav");
        var expectedTranscription = new TranscriptionResult
        {
            AudioFileName = "test_20250907_180000.wav",
            TranscriptFileName = "test_20250907_180000.txt",
            Timestamp = "2025-09-07T18:00:00Z",
            Transcript = "Test transcription content"
        };

        _mockTranscribeService.Setup(x => x.ValidateAudioFile(It.IsAny<IFormFile>()))
            .Returns((true, string.Empty));
        _mockTranscribeService.Setup(x => x.ProcessAudioFileAsync(It.IsAny<IFormFile>()))
            .ReturnsAsync(expectedTranscription);
        _mockGitHubService.Setup(x => x.StoreAudioAndTranscriptAsync(
            It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.ProcessAudioTranscriptionAsync(mockAudioFile);

        // Assert
        result.Should().NotBeNull("processing should return a result");
        result.AudioFileName.Should().Be(expectedTranscription.AudioFileName,
            "result should contain the audio file name");
        result.TranscriptFileName.Should().Be(expectedTranscription.TranscriptFileName,
            "result should contain the transcript file name");
        result.Timestamp.Should().Be(expectedTranscription.Timestamp,
            "result should contain the timestamp");
        result.Transcript.Should().Be(expectedTranscription.Transcript,
            "result should contain the transcript content");
        result.Message.Should().Be("Audio file processed and transcript stored successfully",
            "result should contain the success message");
    }

    [Fact]
    public async Task ProcessAudioTranscriptionAsync_GitHubServiceUnavailable_LogsErrorAndContinues()
    {
        // Arrange
        var mockAudioFile = CreateMockFormFile("test.wav", "audio/wav");
        var expectedTranscription = new TranscriptionResult
        {
            AudioFileName = "test.wav",
            TranscriptFileName = "test.txt",
            Timestamp = "2025-09-07T18:00:00Z",
            Transcript = "Test content"
        };

        _mockTranscribeService.Setup(x => x.ValidateAudioFile(It.IsAny<IFormFile>()))
            .Returns((true, string.Empty));
        _mockTranscribeService.Setup(x => x.ProcessAudioFileAsync(It.IsAny<IFormFile>()))
            .ReturnsAsync(expectedTranscription);
        _mockGitHubService.Setup(x => x.StoreAudioAndTranscriptAsync(
            It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new ServiceUnavailableException("GitHub service unavailable"));

        // Act
        var result = await _sut.ProcessAudioTranscriptionAsync(mockAudioFile);

        // Assert
        result.Should().NotBeNull("processing should complete despite GitHub service being unavailable");
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unable to store to github")),
                It.IsAny<ServiceUnavailableException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "error should be logged when GitHub service is unavailable");
    }

    [Fact]
    public async Task ProcessAudioTranscriptionAsync_GitHubServiceError_ThrowsAndSendsNotification()
    {
        // Arrange
        var mockAudioFile = CreateMockFormFile("test.wav", "audio/wav");
        var expectedTranscription = new TranscriptionResult
        {
            AudioFileName = "test.wav",
            TranscriptFileName = "test.txt",
            Timestamp = "2025-09-07T18:00:00Z",
            Transcript = "Test content"
        };
        var githubException = new InvalidOperationException("GitHub API error");

        _mockTranscribeService.Setup(x => x.ValidateAudioFile(It.IsAny<IFormFile>()))
            .Returns((true, string.Empty));
        _mockTranscribeService.Setup(x => x.ProcessAudioFileAsync(It.IsAny<IFormFile>()))
            .ReturnsAsync(expectedTranscription);
        _mockGitHubService.Setup(x => x.StoreAudioAndTranscriptAsync(
            It.IsAny<string>(), It.IsAny<byte[]>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(githubException);

        // Act
        Func<Task> act = async () => await _sut.ProcessAudioTranscriptionAsync(mockAudioFile);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.WithMessage("*GitHub API error*");

        _mockEmailService.Verify(
            x => x.SendErrorNotification(
                "GitHub Storage",
                githubException,
                "AiService",
                It.IsAny<Dictionary<string, string>>()),
            Times.Once,
            "error notification should be sent for GitHub failures");
    }

    #endregion

    #region GetCompletionAsync Tests

    [Fact]
    public async Task GetCompletionAsync_ValidTextPrompt_ReturnsCompletionResult()
    {
        // Arrange
        var prompt = new CompletionPrompt { TextPrompt = "What is AI?" };
        var expectedResponse = "AI is artificial intelligence.";
        var sessionId = Guid.NewGuid();
        var session = new Session { Id = sessionId };

        _mockLlmService.Setup(x => x.GetCompletionContent(It.IsAny<string>(), null, null, null))
            .ReturnsAsync(expectedResponse);
        _mockScope.Setup(x => x.Id).Returns(Guid.NewGuid());
        _mockSessionManager.Setup(x => x.GetSessionByScope(It.IsAny<Guid>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.GetCompletionAsync(prompt);

        // Assert
        result.Should().NotBeNull("completion should return a result");
        result.Response.Should().Be(expectedResponse, "response should match LLM output");
        result.SourceType.Should().Be("text", "source type should be text for text prompts");
        result.TranscribedPrompt.Should().BeNull("transcribed prompt should be null for text input");
        result.SessionId.Should().Be(sessionId, "session ID should be included in result");
    }

    [Fact]
    public async Task GetCompletionAsync_ValidAudioPrompt_ReturnsCompletionResultWithTranscription()
    {
        // Arrange
        var mockAudioFile = CreateMockFormFile("test.wav", "audio/wav");
        var prompt = new CompletionPrompt { AudioFile = mockAudioFile };
        var transcribedText = "What is AI?";
        var expectedResponse = "AI is artificial intelligence.";
        var sessionId = Guid.NewGuid();
        var session = new Session { Id = sessionId };

        _mockTranscribeService.Setup(x => x.ValidateAudioFile(It.IsAny<IFormFile>()))
            .Returns((true, string.Empty));
        _mockTranscribeService.Setup(x => x.TranscribeAudioAsync(It.IsAny<Stream>(), null))
            .ReturnsAsync(transcribedText);
        _mockLlmService.Setup(x => x.GetCompletionContent(transcribedText, null, null, null))
            .ReturnsAsync(expectedResponse);
        _mockScope.Setup(x => x.Id).Returns(Guid.NewGuid());
        _mockSessionManager.Setup(x => x.GetSessionByScope(It.IsAny<Guid>()))
            .ReturnsAsync(session);

        // Act
        var result = await _sut.GetCompletionAsync(prompt);

        // Assert
        result.Should().NotBeNull("completion should return a result");
        result.Response.Should().Be(expectedResponse, "response should match LLM output");
        result.SourceType.Should().Be("audio", "source type should be audio for audio prompts");
        result.TranscribedPrompt.Should().Be(transcribedText, "transcribed prompt should be included");
        result.SessionId.Should().Be(sessionId, "session ID should be included in result");
    }

    [Fact]
    public async Task GetCompletionAsync_InvalidAudioFile_ThrowsArgumentException()
    {
        // Arrange
        var mockAudioFile = CreateMockFormFile("test.txt", "text/plain");
        var prompt = new CompletionPrompt { AudioFile = mockAudioFile };

        _mockTranscribeService.Setup(x => x.ValidateAudioFile(It.IsAny<IFormFile>()))
            .Returns((false, "Invalid audio format"));

        // Act
        Func<Task> act = async () => await _sut.GetCompletionAsync(prompt);

        // Assert
        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.WithParameterName("prompt");
        exception.WithMessage("*Invalid audio format*");
    }

    [Fact]
    public async Task GetCompletionAsync_TranscriptionFailure_ThrowsInvalidOperationExceptionAndSendsNotification()
    {
        // Arrange
        var mockAudioFile = CreateMockFormFile("test.wav", "audio/wav");
        var prompt = new CompletionPrompt { AudioFile = mockAudioFile };
        var transcriptionException = new InvalidOperationException("Transcription service error");

        _mockTranscribeService.Setup(x => x.ValidateAudioFile(It.IsAny<IFormFile>()))
            .Returns((true, string.Empty));
        _mockTranscribeService.Setup(x => x.TranscribeAudioAsync(It.IsAny<Stream>(), null))
            .ThrowsAsync(transcriptionException);

        // Act
        Func<Task> act = async () => await _sut.GetCompletionAsync(prompt);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.WithMessage("*Failed to transcribe the provided audio prompt*");

        _mockEmailService.Verify(
            x => x.SendErrorNotification(
                "LLM Audio Transcription",
                transcriptionException,
                "AiService",
                It.IsAny<Dictionary<string, string>>()),
            Times.Once,
            "error notification should be sent for transcription failures");
    }

    [Fact]
    public async Task GetCompletionAsync_LlmServiceFailure_ThrowsAndSendsNotification()
    {
        // Arrange
        var prompt = new CompletionPrompt { TextPrompt = "What is AI?" };
        var llmException = new InvalidOperationException("LLM service unavailable");

        _mockLlmService.Setup(x => x.GetCompletionContent(It.IsAny<string>(), null, null, null))
            .ThrowsAsync(llmException);

        // Act
        Func<Task> act = async () => await _sut.GetCompletionAsync(prompt);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.WithMessage("*LLM service unavailable*");

        _mockEmailService.Verify(
            x => x.SendErrorNotification(
                "LLM Completion",
                llmException,
                "AiService",
                It.IsAny<Dictionary<string, string>>()),
            Times.Once,
            "error notification should be sent for LLM failures");
    }

    [Fact]
    public async Task GetCompletionAsync_EmptyPrompt_ThrowsArgumentException()
    {
        // Arrange
        var prompt = new CompletionPrompt(); // No text or audio

        // Act
        Func<Task> act = async () => await _sut.GetCompletionAsync(prompt);

        // Assert
        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.WithParameterName("prompt");
        exception.WithMessage("*Either text prompt or audio file must be provided*");
    }

    [Fact]
    public async Task GetCompletionAsync_WhitespaceTextPrompt_ThrowsArgumentException()
    {
        // Arrange
        var prompt = new CompletionPrompt { TextPrompt = "   " }; // Only whitespace

        // Act
        Func<Task> act = async () => await _sut.GetCompletionAsync(prompt);

        // Assert
        var exception = await act.Should().ThrowAsync<ArgumentException>();
        exception.WithParameterName("prompt");
        exception.WithMessage("*Either text prompt or audio file must be provided*");
    }

    [Fact]
    public async Task GetCompletionAsync_EmailServiceUnavailable_ContinuesProcessing()
    {
        // Arrange
        var prompt = new CompletionPrompt { TextPrompt = "What is AI?" };
        var llmException = new InvalidOperationException("LLM service error");

        _mockLlmService.Setup(x => x.GetCompletionContent(It.IsAny<string>(), null, null, null))
            .ThrowsAsync(llmException);
        _mockEmailService.Setup(x => x.SendErrorNotification(
            It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(new ServiceUnavailableException("Email service unavailable"));

        // Act
        Func<Task> act = async () => await _sut.GetCompletionAsync(prompt);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.WithMessage("*LLM service error*");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Unable to send email as email service is unavailable")),
                It.IsAny<ServiceUnavailableException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "email service unavailability should be logged");
    }

    [Fact]
    public async Task GetCompletionAsync_EmailServiceFailure_PropagatesEmailException()
    {
        // Arrange
        var prompt = new CompletionPrompt { TextPrompt = "What is AI?" };
        var llmException = new InvalidOperationException("LLM service error");
        var emailException = new InvalidOperationException("Email service error");

        _mockLlmService.Setup(x => x.GetCompletionContent(It.IsAny<string>(), null, null, null))
            .ThrowsAsync(llmException);
        _mockEmailService.Setup(x => x.SendErrorNotification(
            It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<Dictionary<string, string>>()))
            .ThrowsAsync(emailException);

        // Act
        Func<Task> act = async () => await _sut.GetCompletionAsync(prompt);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidOperationException>();
        exception.WithMessage("*Email service error*");
    }

    #endregion

    #region Helper Methods

    private static IFormFile CreateMockFormFile(string fileName, string contentType, byte[]? content = null)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(x => x.FileName).Returns(fileName);
        mockFile.Setup(x => x.ContentType).Returns(contentType);
        mockFile.Setup(x => x.Length).Returns(content?.Length ?? 1024);

        if (content != null)
        {
            var stream = new MemoryStream(content);
            mockFile.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns((Stream target, CancellationToken token) => stream.CopyToAsync(target, token));
        }
        else
        {
            mockFile.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Returns(Task.CompletedTask);
        }

        return mockFile.Object;
    }

    #endregion
}