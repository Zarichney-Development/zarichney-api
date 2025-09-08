using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using FluentAssertions;
using Xunit;
using Zarichney.Controllers;
using Zarichney.Services.AI;

namespace Zarichney.Tests.Unit.Controllers.AiController;

[Trait("Category", "Unit")]
public class AiControllerTests
{
    private readonly Mock<IAiService> _mockAiService;
    private readonly Mock<ILogger<Zarichney.Controllers.AiController>> _mockLogger;
    private readonly Zarichney.Controllers.AiController _sut;
    private readonly Mock<HttpContext> _mockHttpContext;
    private readonly Mock<HttpResponse> _mockHttpResponse;
    private readonly Mock<IHeaderDictionary> _mockHeaders;

    public AiControllerTests()
    {
        _mockAiService = new Mock<IAiService>();
        _mockLogger = new Mock<ILogger<Zarichney.Controllers.AiController>>();
        _mockHttpContext = new Mock<HttpContext>();
        _mockHttpResponse = new Mock<HttpResponse>();
        _mockHeaders = new Mock<IHeaderDictionary>();

        _mockHttpContext.Setup(x => x.Response).Returns(_mockHttpResponse.Object);
        _mockHttpResponse.Setup(x => x.Headers).Returns(_mockHeaders.Object);

        _sut = new Zarichney.Controllers.AiController(_mockAiService.Object, _mockLogger.Object);
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = _mockHttpContext.Object
        };
    }

    #region GetCompletion Tests

    [Fact]
    public async Task GetCompletion_ValidTextPrompt_ReturnsOkWithCompletionResult()
    {
        // Arrange
        var request = new CompletionRequest { TextPrompt = "What is AI?" };
        var sessionId = Guid.NewGuid();
        var expectedResult = new CompletionResult
        {
            Response = "AI is artificial intelligence.",
            SourceType = "text",
            TranscribedPrompt = null,
            SessionId = sessionId
        };

        _mockAiService.Setup(x => x.GetCompletionAsync(It.IsAny<CompletionPrompt>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.GetCompletion(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>("valid requests should return OK");
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().NotBeNull("response should contain data");

        _mockAiService.Verify(
            x => x.GetCompletionAsync(It.Is<CompletionPrompt>(p => p.TextPrompt == request.TextPrompt)),
            Times.Once,
            "service should be called with correct text prompt");

        _mockHeaders.Verify(
            x => x.Append("X-Session-Id", sessionId.ToString()),
            Times.Once,
            "session ID should be added to response headers");
    }

    [Fact]
    public async Task GetCompletion_ValidAudioPrompt_ReturnsOkWithTranscriptionResult()
    {
        // Arrange
        var mockAudioFile = CreateMockFormFile("test.wav", "audio/wav");
        var request = new CompletionRequest { AudioPrompt = mockAudioFile };
        var sessionId = Guid.NewGuid();
        var expectedResult = new CompletionResult
        {
            Response = "AI is artificial intelligence.",
            SourceType = "audio",
            TranscribedPrompt = "What is AI?",
            SessionId = sessionId
        };

        _mockAiService.Setup(x => x.GetCompletionAsync(It.IsAny<CompletionPrompt>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.GetCompletion(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>("valid requests should return OK");
        _mockAiService.Verify(
            x => x.GetCompletionAsync(It.Is<CompletionPrompt>(p => p.AudioFile == request.AudioPrompt)),
            Times.Once,
            "service should be called with correct audio file");

        _mockHeaders.Verify(
            x => x.Append("X-Session-Id", sessionId.ToString()),
            Times.Once,
            "session ID should be added to response headers");
    }

    [Fact]
    public async Task GetCompletion_ArgumentException_ReturnsBadRequest()
    {
        // Arrange
        var request = new CompletionRequest { TextPrompt = "Invalid prompt" };
        var exceptionMessage = "Invalid prompt format";

        _mockAiService.Setup(x => x.GetCompletionAsync(It.IsAny<CompletionPrompt>()))
            .ThrowsAsync(new ArgumentException(exceptionMessage));

        // Act
        var result = await _sut.GetCompletion(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("ArgumentException should return BadRequest");
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().Be(exceptionMessage,
            "error message should be included in response");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid prompt provided")),
                It.IsAny<ArgumentException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "argument exception should be logged as warning");
    }

    [Fact]
    public async Task GetCompletion_InvalidOperationException_ReturnsBadRequest()
    {
        // Arrange
        var request = new CompletionRequest { TextPrompt = "Test prompt" };
        var exceptionMessage = "Operation failed";

        _mockAiService.Setup(x => x.GetCompletionAsync(It.IsAny<CompletionPrompt>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act
        var result = await _sut.GetCompletion(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("InvalidOperationException should return BadRequest");
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().Be(exceptionMessage,
            "error message should be included in response");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Operation failed")),
                It.IsAny<InvalidOperationException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "operation exception should be logged as error");
    }

    [Fact]
    public async Task GetCompletion_GeneralException_ReturnsApiErrorResult()
    {
        // Arrange
        var request = new CompletionRequest { TextPrompt = "Test prompt" };
        var exception = new Exception("Unexpected error");

        _mockAiService.Setup(x => x.GetCompletionAsync(It.IsAny<CompletionPrompt>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.GetCompletion(request);

        // Assert
        result.Should().BeOfType<ApiErrorResult>("general exceptions should return ApiErrorResult");
        var errorResult = (ApiErrorResult)result;
        errorResult.Should().NotBeNull("error result should contain exception details");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to get LLM completion")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "general exception should be logged as error");
    }

    #endregion

    #region TranscribeAudio Tests

    [Fact]
    public async Task TranscribeAudio_ValidAudioFile_ReturnsOkWithTranscriptionResult()
    {
        // Arrange
        var mockAudioFile = CreateMockFormFile("test.wav", "audio/wav");
        var request = new TranscribeAudioRequest { AudioFile = mockAudioFile };
        var expectedResult = new AudioTranscriptionResult
        {
            Message = "Audio processed successfully",
            AudioFileName = "test_20250907_180000.wav",
            TranscriptFileName = "test_20250907_180000.txt",
            Timestamp = "2025-09-07T18:00:00Z",
            Transcript = "This is the transcribed content."
        };

        _mockAiService.Setup(x => x.ProcessAudioTranscriptionAsync(mockAudioFile))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.TranscribeAudio(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>("valid requests should return OK");
        _mockAiService.Verify(
            x => x.ProcessAudioTranscriptionAsync(mockAudioFile),
            Times.Once,
            "service should be called with the correct audio file");
    }

    [Fact]
    public async Task TranscribeAudio_ArgumentNullException_ReturnsBadRequest()
    {
        // Arrange
        var mockAudioFile = CreateMockFormFile("test.wav", "audio/wav");
        var request = new TranscribeAudioRequest { AudioFile = mockAudioFile };

        _mockAiService.Setup(x => x.ProcessAudioTranscriptionAsync(It.IsAny<IFormFile>()))
            .ThrowsAsync(new ArgumentNullException("audioFile"));

        // Act
        var result = await _sut.TranscribeAudio(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("ArgumentNullException should return BadRequest");
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().Be("Audio file is required.",
            "user-friendly message should be returned for null argument");
    }

    [Fact]
    public async Task TranscribeAudio_ArgumentException_ReturnsBadRequestWithMessage()
    {
        // Arrange
        var mockAudioFile = CreateMockFormFile("test.txt", "text/plain");
        var request = new TranscribeAudioRequest { AudioFile = mockAudioFile };
        var exceptionMessage = "Invalid audio file format";

        _mockAiService.Setup(x => x.ProcessAudioTranscriptionAsync(It.IsAny<IFormFile>()))
            .ThrowsAsync(new ArgumentException(exceptionMessage));

        // Act
        var result = await _sut.TranscribeAudio(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>("ArgumentException should return BadRequest");
        var badRequestResult = (BadRequestObjectResult)result;
        badRequestResult.Value.Should().Be(exceptionMessage,
            "exception message should be returned to user");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Invalid audio file")),
                It.IsAny<ArgumentException>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "argument exception should be logged as warning");
    }

    [Fact]
    public async Task TranscribeAudio_GeneralException_ReturnsApiErrorResult()
    {
        // Arrange
        var mockAudioFile = CreateMockFormFile("test.wav", "audio/wav");
        var request = new TranscribeAudioRequest { AudioFile = mockAudioFile };
        var exception = new Exception("Unexpected transcription error");

        _mockAiService.Setup(x => x.ProcessAudioTranscriptionAsync(It.IsAny<IFormFile>()))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.TranscribeAudio(request);

        // Assert
        result.Should().BeOfType<ApiErrorResult>("general exceptions should return ApiErrorResult");
        var errorResult = (ApiErrorResult)result;
        errorResult.Should().NotBeNull("error result should contain exception details");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to process audio file")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "general exception should be logged as error");
    }

    #endregion

    #region Helper Methods

    private static IFormFile CreateMockFormFile(string fileName, string contentType)
    {
        var mockFile = new Mock<IFormFile>();
        mockFile.Setup(x => x.FileName).Returns(fileName);
        mockFile.Setup(x => x.ContentType).Returns(contentType);
        mockFile.Setup(x => x.Length).Returns(1024);
        mockFile.Setup(x => x.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        return mockFile.Object;
    }

    #endregion
}