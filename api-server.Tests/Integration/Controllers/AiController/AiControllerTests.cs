/* commenting out until the syntax errors are able to be resolved
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using OpenAI.Audio;
using Xunit;
using Zarichney.Services.AI;
using Zarichney.Services.GitHub;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration.Controllers.AiController;

/// <summary>
/// Integration tests for the AiController endpoints (/api/completion and /api/transcribe).
/// These tests verify the behavior of AI-related endpoints within the ASP.NET Core pipeline.
/// </summary>
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Component, TestCategories.Controller)]
[Trait(TestCategories.Feature, TestCategories.AI)]
[Trait(TestCategories.Dependency, TestCategories.ExternalOpenAI)]
[Trait(TestCategories.Dependency, TestCategories.ExternalGitHub)]
[Collection("Integration Tests")]
public class AiControllerTests(CustomWebApplicationFactory factory, ApiClientFixture apiClientFixture) : IntegrationTestBase(factory, apiClientFixture)
{
    private const string TestUserId = "ai-test-user";
    private static readonly string[] TestUserRoles = ["User"];
    private const string ValidTextPrompt = "Write a short story.";
    private const string ValidTranscriptionResult = "This is the transcribed audio.";
    private const string ValidCompletionResult = "Here is your short story.";
    private const string AudioFileName = "test_audio.wav";
    private const string AudioContentType = "audio/wav";
    private static readonly byte[] ValidAudioBytes = Encoding.UTF8.GetBytes("fake audio data");
    private static readonly byte[] EmptyAudioBytes = [];

    // Helper method to create MultipartFormDataContent for completion
    private MultipartFormDataContent CreateCompletionContent(string? textPrompt = null, IFormFile? audioPrompt = null)
    {
        var content = new MultipartFormDataContent();
        if (textPrompt != null)
        {
            content.Add(new StringContent(textPrompt), "textPrompt");
        }
        if (audioPrompt != null)
        {
            var streamContent = new StreamContent(audioPrompt.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(audioPrompt.ContentType);
            content.Add(streamContent, "audioPrompt", audioPrompt.FileName);
        }
        return content;
    }

    // Helper method to create MultipartFormDataContent for transcription
    private MultipartFormDataContent CreateTranscribeContent(IFormFile? audioFile, string formFieldName = "audioFile")
    {
        var content = new MultipartFormDataContent();
        if (audioFile != null)
        {
            var streamContent = new StreamContent(audioFile.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(audioFile.ContentType);
            content.Add(streamContent, formFieldName, audioFile.FileName);
        }
        return content;
    }

    // Helper to create a mock IFormFile
    private IFormFile CreateMockFormFile(string fileName, string contentType, byte[] content)
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "audioFile", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }

    // --- /api/completion Tests ---

    /// <summary>
    /// Verifies that sending a valid text prompt to /api/completion while authenticated
    /// returns 200 OK, the expected completion response, correct source type,
    /// and the X-Session-Id header.
    /// </summary>
    [Fact]
    public async Task Completion_WithValidTextPrompt_Authenticated_ReturnsOkAndCompletion()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var mockLlmService = Factory.Services.GetRequiredService<Mock<ILlmService>>();

        mockLlmService
            .Setup(s => s.GetCompletionContent(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ValidCompletionResult);

        var formData = CreateCompletionContent(textPrompt: ValidTextPrompt);

        // Act
        var response = await client.PostAsync("/api/completion", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: "a valid text prompt should be completed");
        response.Headers.Should().ContainKey("X-Session-Id", because: "session ID should be returned");
        
        var responseBody = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseBody);
        jsonDoc.RootElement.GetProperty("response").GetString().Should().Be(ValidCompletionResult);
        jsonDoc.RootElement.GetProperty("sourceType").GetString().Should().Be("text");
        jsonDoc.RootElement.TryGetProperty("transcribedPrompt", out _).Should().BeFalse(because: "no audio was provided");

        mockLlmService.Verify(
            s => s.GetCompletionContent(ValidTextPrompt, It.IsAny<string>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that sending a valid audio prompt to /api/completion while authenticated
    /// results in successful transcription and completion, returning 200 OK,
    /// the expected completion response, correct source type ('audio'), the transcribed text,
    /// and the X-Session-Id header.
    /// </summary>
    [Fact]
    public async Task Completion_WithValidAudioPrompt_Authenticated_ReturnsOkAndCompletion()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var mockTranscribeService = Factory.Services.GetRequiredService<Mock<ITranscribeService>>();
        var mockLlmService = Factory.Services.GetRequiredService<Mock<ILlmService>>();

        mockTranscribeService
            .Setup(s => s.TranscribeAudioAsync(It.IsAny<Stream>(), It.IsAny<AudioTranscriptionOptions>()))
            .ReturnsAsync(ValidTranscriptionResult);
            
        mockLlmService
            .Setup(s => s.GetCompletionContent(It.Is<string>(prompt => prompt == ValidTranscriptionResult), It.IsAny<string>()))
            .ReturnsAsync(ValidCompletionResult);

        var audioFile = CreateMockFormFile(AudioFileName, AudioContentType, ValidAudioBytes);
        var formData = CreateCompletionContent(audioPrompt: audioFile);

        // Act
        var response = await client.PostAsync("/api/completion", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: "a valid audio prompt should be transcribed and completed");
        response.Headers.Should().ContainKey("X-Session-Id", because: "session ID should be returned");
        
        var responseBody = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseBody);
        jsonDoc.RootElement.GetProperty("response").GetString().Should().Be(ValidCompletionResult);
        jsonDoc.RootElement.GetProperty("sourceType").GetString().Should().Be("audio");
        jsonDoc.RootElement.GetProperty("transcribedPrompt").GetString().Should().Be(ValidTranscriptionResult);
    }

    /// <summary>
    /// Verifies the behavior when both text and audio prompts are sent to /api/completion.
    /// Based on the controller logic, it should prioritize the audio prompt.
    /// This test confirms that behavior, returning 200 OK and using the audio source.
    /// </summary>
    [Fact]
    public async Task Completion_WithBothTextAndAudioPrompt_PrioritizesAudioAndReturnsOk()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var mockTranscribeService = Factory.Services.GetRequiredService<Mock<ITranscribeService>>();
        var mockLlmService = Factory.Services.GetRequiredService<Mock<ILlmService>>();

        mockTranscribeService
            .Setup(s => s.TranscribeAudioAsync(It.IsAny<Stream>(), It.IsAny<AudioTranscriptionOptions>()))
            .ReturnsAsync(ValidTranscriptionResult);
            
        mockLlmService
            .Setup(s => s.GetCompletionContent(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(ValidCompletionResult);

        var audioFile = CreateMockFormFile(AudioFileName, AudioContentType, ValidAudioBytes);
        var formData = CreateCompletionContent(textPrompt: "This text prompt should be ignored", audioPrompt: audioFile);

        // Act
        var response = await client.PostAsync("/api/completion", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: "controller prioritizes audio when both prompts are sent");
        
        var responseBody = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseBody);
        jsonDoc.RootElement.GetProperty("sourceType").GetString().Should().Be("audio", because: "audio prompt should take precedence");
        jsonDoc.RootElement.GetProperty("transcribedPrompt").GetString().Should().Be(ValidTranscriptionResult);
        jsonDoc.RootElement.GetProperty("response").GetString().Should().Be(ValidCompletionResult);
    }

    /// <summary>
    /// Verifies that if the ILlmService throws an exception during processing,
    /// the /api/completion endpoint catches it and returns a 500 Internal Server Error.
    /// </summary>
    [Fact]
    public async Task Completion_WhenLlmServiceThrows_ReturnsInternalServerError()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var mockLlmService = Factory.Services.GetRequiredService<Mock<ILlmService>>();
        var exceptionMessage = "LLM service failed unexpectedly";
        
        mockLlmService
            .Setup(s => s.GetCompletionContent(It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        var formData = CreateCompletionContent(textPrompt: ValidTextPrompt);

        // Act
        var response = await client.PostAsync("/api/completion", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError, 
            because: "unhandled exceptions in services should result in 500 via ErrorHandlingMiddleware");
        
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Failed to get LLM completion", 
            because: "the controller's catch block should provide a generic error message");
    }

    [Fact]
    public async Task Completion_WithMissingPrompt_ReturnsBadRequest()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var formData = CreateCompletionContent(); // No text or audio

        // Act
        var response = await client.PostAsync("/api/completion", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because: "the endpoint requires either textPrompt or audioPrompt");
    }

    [Fact]
    public async Task Completion_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = ApiClient; // Unauthenticated
        var formData = CreateCompletionContent(textPrompt: "Test prompt");

        // Act
        var response = await client.PostAsync("/api/completion", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, because: "authentication is required");
    }

    // --- /api/transcribe Tests ---

    /// <summary>
    /// Verifies that sending a valid audio file to /api/transcribe while authenticated
    /// results in successful transcription and GitHub commits, returning 200 OK,
    /// the transcript, file metadata, and the expected response structure.
    /// </summary>
    [Fact]
    public async Task Transcribe_WithValidAudioFile_Authenticated_ReturnsOkAndTranscription()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var mockTranscribeService = Factory.Services.GetRequiredService<Mock<ITranscribeService>>();
        var mockGitHubService = Factory.Services.GetRequiredService<Mock<IGitHubService>>();

        mockTranscribeService
            .Setup(s => s.TranscribeAudioAsync(It.IsAny<Stream>(), It.IsAny<AudioTranscriptionOptions>()))
            .ReturnsAsync(ValidTranscriptionResult);
        
        mockGitHubService
            .Setup(s => s.EnqueueCommitAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var audioFile = CreateMockFormFile(AudioFileName, AudioContentType, ValidAudioBytes);
        var formData = CreateTranscribeContent(audioFile);

        // Act
        var response = await client.PostAsync("/api/transcribe", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: "a valid audio file should be transcribed successfully");
        
        var responseBody = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseBody);
        jsonDoc.RootElement.GetProperty("transcript").GetString().Should().Be(ValidTranscriptionResult);
        jsonDoc.RootElement.GetProperty("message").GetString().Should().Contain("processed and transcript stored successfully");
        jsonDoc.RootElement.GetProperty("audioFile").GetString().Should().Contain(AudioFileName.Split('.')[0], 
            because: "response should include the generated audio filename");
        jsonDoc.RootElement.GetProperty("transcriptFile").GetString().Should().Contain(AudioFileName.Split('.')[0], 
            because: "response should include the generated transcript filename");
        jsonDoc.RootElement.GetProperty("transcriptFile").GetString().Should().EndWith(".txt", 
            because: "transcript file should have .txt extension");

        // Verify GitHub commit calls
        mockGitHubService.Verify(
            s => s.EnqueueCommitAsync(
                It.Is<string>(f => f.EndsWith(AudioFileName)),
                It.IsAny<byte[]>(),
                "recordings",
                It.IsAny<string>()),
            Times.Once);
        
        mockGitHubService.Verify(
            s => s.EnqueueCommitAsync(
                It.Is<string>(f => f.EndsWith(".txt")),
                It.Is<byte[]>(b => Encoding.UTF8.GetString(b) == ValidTranscriptionResult),
                "transcripts",
                It.IsAny<string>()),
            Times.Once);
    }

    /// <summary>
    /// Verifies that sending a valid audio file to /api/transcribe returns 200 OK
    /// and the transcription result.
    /// </summary>
    [Fact]
    public async Task Transcribe_WithValidAudioFile_ReturnsOkAndTranscription()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var mockTranscribeService = Factory.Services.GetRequiredService<Mock<ITranscribeService>>();
        var mockGitHubService = Factory.Services.GetRequiredService<Mock<IGitHubService>>();
        
        mockTranscribeService
            .Setup(s => s.TranscribeAudioAsync(It.IsAny<Stream>(), It.IsAny<AudioTranscriptionOptions>()))
            .ReturnsAsync(ValidTranscriptionResult);
            
        mockGitHubService
            .Setup(s => s.EnqueueCommitAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);
        
        // Create a mock audio file
        var audioFile = CreateMockFormFile(AudioFileName, AudioContentType, ValidAudioBytes);
        var formData = CreateTranscribeContent(audioFile);
        
        // Act
        var response = await client.PostAsync("/api/transcribe", formData);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, because: "a valid audio file should be transcribed");
        response.Headers.Should().ContainKey("X-Session-Id", because: "session ID should be returned");
        
        var responseBody = await response.Content.ReadAsStringAsync();
        using var jsonDoc = JsonDocument.Parse(responseBody);
        jsonDoc.RootElement.GetProperty("transcript").GetString().Should().Be(ValidTranscriptionResult);
        
        // Verify service calls were made
        mockTranscribeService.Verify(
            s => s.TranscribeAudioAsync(It.IsAny<Stream>(), It.IsAny<AudioTranscriptionOptions>()),
            Times.Once);
            
        mockGitHubService.Verify(
            s => s.EnqueueCommitAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<string>(),
                It.IsAny<string>()),
            Times.Exactly(2));
    }

    /// <summary>
    /// Verifies that if the ITranscribeService throws an exception during processing,
    /// the /api/transcribe endpoint catches it and returns a 500 Internal Server Error.
    /// It also verifies that the initial GitHub commit for the audio file was still attempted.
    /// </summary>
    [Fact]
    public async Task Transcribe_WhenTranscribeServiceThrows_ReturnsInternalServerError()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var mockTranscribeService = Factory.Services.GetRequiredService<Mock<ITranscribeService>>();
        var mockGitHubService = Factory.Services.GetRequiredService<Mock<IGitHubService>>();
        var exceptionMessage = "Transcription service failed";

        mockGitHubService
            .Setup(s => s.EnqueueCommitAsync(
                It.IsAny<string>(),
                It.IsAny<byte[]>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        mockTranscribeService
            .Setup(s => s.TranscribeAudioAsync(It.IsAny<Stream>(), It.IsAny<AudioTranscriptionOptions>()))
            .ThrowsAsync(new ApplicationException(exceptionMessage));

        var audioFile = CreateMockFormFile(AudioFileName, AudioContentType, ValidAudioBytes);
        var formData = CreateTranscribeContent(audioFile);

        // Act
        var response = await client.PostAsync("/api/transcribe", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError, because: "exceptions during transcription should result in 500");
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Failed to process audio file", because: "the generic error message from the outer catch block should be returned");

        mockGitHubService.Verify(
            s => s.EnqueueCommitAsync(
                It.Is<string>(f => f.EndsWith(AudioFileName)),
                It.IsAny<byte[]>(),
                "recordings",
                It.IsAny<string>()),
            Times.Once);
            
        mockGitHubService.Verify(
            s => s.EnqueueCommitAsync(
                It.Is<string>(f => f.EndsWith(".txt")),
                It.IsAny<byte[]>(),
                "transcripts",
                It.IsAny<string>()),
            Times.Never);
    }

    /// <summary>
    /// Verifies that if the IGitHubService throws an exception during the *first* commit (saving the audio file),
    /// the /api/transcribe endpoint catches it and returns a 500 Internal Server Error.
    /// It also verifies that transcription and the second GitHub commit were not attempted.
    /// </summary>
    [Fact]
    public async Task Transcribe_WhenGitHubServiceThrowsOnAudioCommit_ReturnsInternalServerError()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var mockTranscribeService = Factory.Services.GetRequiredService<Mock<ITranscribeService>>();
        var mockGitHubService = Factory.Services.GetRequiredService<Mock<IGitHubService>>();
        var exceptionMessage = "GitHub audio commit failed";

        mockGitHubService
            .Setup(s => s.EnqueueCommitAsync(
                It.Is<string>(f => f.EndsWith(AudioFileName)),
                It.IsAny<byte[]>(),
                "recordings",
                It.IsAny<string>()))
            .ThrowsAsync(new HttpRequestException(exceptionMessage));

        var audioFile = CreateMockFormFile(AudioFileName, AudioContentType, ValidAudioBytes);
        var formData = CreateTranscribeContent(audioFile);

        // Act
        var response = await client.PostAsync("/api/transcribe", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError, because: "exceptions during the initial GitHub audio commit should result in 500");
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Failed to process audio file", because: "the generic error message should be returned");

        mockTranscribeService.Verify(
            s => s.TranscribeAudioAsync(It.IsAny<Stream>(), It.IsAny<AudioTranscriptionOptions>()), 
            Times.Never);
            
        mockGitHubService.Verify(
            s => s.EnqueueCommitAsync(
                It.Is<string>(f => f.EndsWith(".txt")),
                It.IsAny<byte[]>(),
                "transcripts", 
                It.IsAny<string>()), 
            Times.Never);
    }

    /// <summary>
    /// Verifies that if the IGitHubService throws an exception during the *second* commit (saving the transcript file),
    /// the /api/transcribe endpoint catches it and returns a 500 Internal Server Error.
    /// It also verifies that transcription and the first GitHub commit were attempted.
    /// </summary>
    [Fact]
    public async Task Transcribe_WhenGitHubServiceThrowsOnTranscriptCommit_ReturnsInternalServerError()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var mockTranscribeService = Factory.Services.GetRequiredService<Mock<ITranscribeService>>();
        var mockGitHubService = Factory.Services.GetRequiredService<Mock<IGitHubService>>();
        var exceptionMessage = "GitHub transcript commit failed";
        
        mockTranscribeService
            .Setup(s => s.TranscribeAudioAsync(It.IsAny<Stream>(), It.IsAny<AudioTranscriptionOptions>()))
            .ReturnsAsync(ValidTranscriptionResult);
        
        mockGitHubService.SetupSequence(s => s.EnqueueCommitAsync(
                It.IsAny<string>(), 
                It.IsAny<byte[]>(), 
                It.IsAny<string>(), 
                It.IsAny<string>()))
            .Returns(Task.CompletedTask)
            .ThrowsAsync(new HttpRequestException(exceptionMessage));

        var audioFile = CreateMockFormFile(AudioFileName, AudioContentType, ValidAudioBytes);
        var formData = CreateTranscribeContent(audioFile);

        // Act
        var response = await client.PostAsync("/api/transcribe", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError, because: "exceptions during the second GitHub commit should result in 500");
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Failed to process audio file", because: "the generic error message should be returned");

        // Verify service calls happened in the correct order before failure
        mockTranscribeService.Verify(
            s => s.TranscribeAudioAsync(It.IsAny<Stream>(), It.IsAny<AudioTranscriptionOptions>()), 
            Times.Once);
            
        mockGitHubService.Verify(
            s => s.EnqueueCommitAsync(
                It.Is<string>(f => f.EndsWith(AudioFileName)),
                It.IsAny<byte[]>(),
                "recordings", 
                It.IsAny<string>()), 
            Times.Once);
            
        mockGitHubService.Verify(
            s => s.EnqueueCommitAsync(
                It.Is<string>(f => f.EndsWith(".txt")),
                It.IsAny<byte[]>(),
                "transcripts", 
                It.IsAny<string>()), 
            Times.Once);
    }

    /// <summary>
    /// Verifies that when the ITranscribeService throws an exception, the ErrorHandlingMiddleware
    /// correctly returns a 500 Internal Server Error response.
    /// </summary>
    [Fact]
    public async Task Transcribe_WhenTranscribeServiceThrowsException_Returns500Error()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var mockTranscribeService = Factory.Services.GetRequiredService<Mock<ITranscribeService>>();
        
        mockTranscribeService
            .Setup(s => s.TranscribeAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Simulated transcription service failure"));
        
        var fileName = "test-audio.mp3";
        var fileContent = new MemoryStream(Encoding.UTF8.GetBytes("Mock audio content"));
        var formData = new MultipartFormDataContent();
        formData.Add(new StreamContent(fileContent), "audioFile", fileName);
        
        // Act
        var response = await client.PostAsync("/api/transcribe", formData);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError, 
            because: "errors in the transcribe service should be handled by ErrorHandlingMiddleware");
    }

    /// <summary>
    /// Verifies that the Transcribe endpoint returns a 500 status code when the transcribe service throws an exception.
    /// </summary>
    [Fact]
    public async Task Transcribe_WhenTranscribeServiceThrowsException_Returns500()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var mockTranscribeService = Factory.Services.GetRequiredService<Mock<ITranscribeService>>();
        
        mockTranscribeService
            .Setup(s => s.TranscribeAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Service error"));
        
        var fileName = "test-audio.mp3";
        var fileContent = new MemoryStream(Encoding.UTF8.GetBytes("Mock audio content"));
        var formData = new MultipartFormDataContent();
        formData.Add(new StreamContent(fileContent), "audioFile", fileName);
        
        // Act
        var response = await client.PostAsync("/api/transcribe", formData);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        error.Should().NotBeNull();
        error!.Message.Should().Contain("An unexpected error occurred");
    }

    /// <summary>
    /// Verifies that when the IGitHubService throws an exception, the ErrorHandlingMiddleware
    /// correctly returns a 500 Internal Server Error response.
    /// </summary>
    [Fact]
    public async Task Transcribe_WhenGitHubServiceThrowsException_Returns500Error()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var mockTranscribeService = Factory.Services.GetRequiredService<Mock<ITranscribeService>>();
        var mockGitHubService = Factory.Services.GetRequiredService<Mock<IGitHubService>>();
        
        mockTranscribeService
            .Setup(s => s.TranscribeAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(new TranscriptionResult { Text = "Test transcription" });
        
        mockGitHubService
            .Setup(s => s.SaveTranscriptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("Simulated GitHub service failure"));
        
        var fileName = "test-audio.mp3";
        var fileContent = new MemoryStream(Encoding.UTF8.GetBytes("Mock audio content"));
        var formData = new MultipartFormDataContent();
        formData.Add(new StreamContent(fileContent), "audioFile", fileName);
        
        // Act
        var response = await client.PostAsync("/api/transcribe", formData);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError, 
            because: "errors in the GitHub service should be handled by ErrorHandlingMiddleware");
    }

    /// <summary>
    /// Verifies that the Transcribe endpoint returns a 500 status code when the GitHub service throws an exception.
    /// </summary>
    [Fact]
    public async Task Transcribe_WhenGitHubServiceThrowsException_Returns500()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var mockTranscribeService = Factory.Services.GetRequiredService<Mock<ITranscribeService>>();
        var mockGitHubService = Factory.Services.GetRequiredService<Mock<IGitHubService>>();
        
        mockTranscribeService
            .Setup(s => s.TranscribeAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync("Transcription result");
        
        mockGitHubService
            .Setup(s => s.SaveTranscriptionAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .ThrowsAsync(new Exception("GitHub service error"));
        
        var fileName = "test-audio.mp3";
        var fileContent = new MemoryStream(Encoding.UTF8.GetBytes("Mock audio content"));
        var formData = new MultipartFormDataContent();
        formData.Add(new StreamContent(fileContent), "audioFile", fileName);
        
        // Act
        var response = await client.PostAsync("/api/transcribe", formData);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        error.Should().NotBeNull();
        error!.Message.Should().Contain("An unexpected error occurred");
    }

    [Fact]
    public async Task Transcribe_WithMissingAudioFile_ReturnsBadRequest()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var formData = CreateTranscribeContent(null);

        // Act
        var response = await client.PostAsync("/api/transcribe", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because: "the audioFile is required");
    }

    [Fact]
    public async Task Transcribe_WithEmptyAudioFile_ReturnsBadRequest()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var audioFile = CreateMockFormFile(AudioFileName, AudioContentType, EmptyAudioBytes);
        var formData = CreateTranscribeContent(audioFile);

        // Act
        var response = await client.PostAsync("/api/transcribe", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because: "empty audio files are not allowed");
    }

    [Fact]
    public async Task Transcribe_WithIncorrectFormFieldName_ReturnsBadRequest()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var audioFile = CreateMockFormFile(AudioFileName, AudioContentType, ValidAudioBytes);
        var formData = CreateTranscribeContent(audioFile, formFieldName: "file");

        // Act
        var response = await client.PostAsync("/api/transcribe", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, because: "the form field name must be 'audioFile'");
    }

    [Fact]
    public async Task Transcribe_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = ApiClient;
        var audioFile = CreateMockFormFile(AudioFileName, AudioContentType, ValidAudioBytes);
        var formData = CreateTranscribeContent(audioFile);

        // Act
        var response = await client.PostAsync("/api/transcribe", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, because: "authentication is required");
    }

    /// <summary>
    /// Verifies that the Transcribe endpoint includes the X-Session-Id header in the response.
    /// </summary>
    [Fact]
    public async Task Transcribe_WithValidRequest_IncludesSessionIdHeader()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var mockTranscribeService = Factory.Services.GetRequiredService<Mock<ITranscribeService>>();
        var expectedTranscription = "This is a transcribed text";
        
        mockTranscribeService
            .Setup(s => s.TranscribeAsync(It.IsAny<Stream>(), It.IsAny<string>()))
            .ReturnsAsync(expectedTranscription);
        
        var fileName = "test-audio.mp3";
        var fileContent = new MemoryStream(Encoding.UTF8.GetBytes("Mock audio content"));
        var formData = new MultipartFormDataContent();
        formData.Add(new StreamContent(fileContent), "audioFile", fileName);
        
        // Act
        var response = await client.PostAsync("/api/transcribe", formData);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.Should().ContainKey("X-Session-Id");
        response.Headers.GetValues("X-Session-Id").First().Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// Verifies that the /api/transcribe endpoint returns 400 Bad Request
    /// when no audio file is provided in the request.
    /// </summary>
    [Fact]
    public async Task Transcribe_WithNoAudioFile_ReturnsBadRequest()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var formData = new MultipartFormDataContent(); // Empty form data

        // Act
        var response = await client.PostAsync("/api/transcribe", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, 
            because: "the endpoint requires an audio file to be provided");
            
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("No audio file provided", 
            because: "the error should indicate that the audio file is missing");
    }

    /// <summary>
    /// Verifies that the /api/transcribe endpoint returns 400 Bad Request
    /// when the audio file is provided with an empty or zero-length content.
    /// </summary>
    [Fact]
    public async Task Transcribe_WithEmptyAudioFile_ReturnsBadRequest()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var audioFile = CreateMockFormFile(AudioFileName, AudioContentType, EmptyAudioBytes);
        var formData = CreateTranscribeContent(audioFile);

        // Act
        var response = await client.PostAsync("/api/transcribe", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, 
            because: "the endpoint requires a non-empty audio file");
            
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Empty audio file provided", 
            because: "the error should indicate that the audio file is empty");
    }

    /// <summary>
    /// Verifies that the /api/transcribe endpoint returns 400 Bad Request
    /// when the audio file is provided with an unsupported content type.
    /// </summary>
    [Fact]
    public async Task Transcribe_WithInvalidAudioContentType_ReturnsBadRequest()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var audioFile = CreateMockFormFile(AudioFileName, "application/pdf", ValidAudioBytes);
        var formData = CreateTranscribeContent(audioFile);

        // Act
        var response = await client.PostAsync("/api/transcribe", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, 
            because: "the endpoint requires a supported audio content type");
            
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("Unsupported audio format", 
            because: "the error should indicate that the content type is not supported");
    }

    /// <summary>
    /// Verifies that the /api/transcribe endpoint returns 401 Unauthorized
    /// when called without authentication.
    /// </summary>
    [Fact]
    public async Task Transcribe_Unauthenticated_ReturnsUnauthorized()
    {
        // Arrange
        var client = ApiClient; // Unauthenticated client
        var audioFile = CreateMockFormFile(AudioFileName, AudioContentType, ValidAudioBytes);
        var formData = CreateTranscribeContent(audioFile);

        // Act
        var response = await client.PostAsync("/api/transcribe", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized, 
            because: "authentication is required for the /api/transcribe endpoint");
    }

    /// <summary>
    /// Verifies that the /api/transcribe endpoint returns 400 Bad Request
    /// when the audio file is provided with the wrong form field name.
    /// </summary>
    [Fact]
    public async Task Transcribe_WithIncorrectFormFieldName_ReturnsBadRequest()
    {
        // Arrange
        var client = AuthenticatedApiClient;
        var audioFile = CreateMockFormFile(AudioFileName, AudioContentType, ValidAudioBytes);
        var formData = CreateTranscribeContent(audioFile, formFieldName: "wrongFieldName");

        // Act
        var response = await client.PostAsync("/api/transcribe", formData);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest, 
            because: "the endpoint expects the audio file in the 'audioFile' form field");
            
        var responseBody = await response.Content.ReadAsStringAsync();
        responseBody.Should().Contain("No audio file provided", 
            because: "the error should indicate that the expected audio file is missing");
    }
}
*/
