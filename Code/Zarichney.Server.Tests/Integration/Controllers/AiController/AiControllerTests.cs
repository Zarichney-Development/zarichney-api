using System.Net;
using FluentAssertions;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Fixtures;
using Zarichney.Tests.Framework.Attributes;
using Refit;
using Xunit;
using Xunit.Abstractions;

namespace Zarichney.Tests.Integration.Controllers.AiController;

/// <summary>
/// Integration tests for the AiController endpoints (/api/completion and /api/transcribe).
/// These tests verify that the Refit client generation with addContentTypeHeaders: false
/// resolves multipart/form-data Content-Type header conflicts.
/// </summary>
[Collection("IntegrationExternal")]
public class AiControllerTests(ApiClientFixture apiClientFixture, ITestOutputHelper output)
  : IntegrationTestBase(apiClientFixture, output)
{
  // --- /api/completion Tests ---

  [DependencyFact(ExternalServices.OpenAiApi)]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Completion_WithValidTextPrompt_ReturnsOk()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;
    var textPrompt = "What is the capital of France?";

    // Act
    var response = await client.Completion(textPrompt, default(StreamPart?));

    // Assert
    response.IsSuccessStatusCode.Should().BeTrue(
        because: "the OpenAI API should be available and return a successful response");
  }

  [ServiceUnavailableFact(ExternalServices.OpenAiApi)]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Completion_WithValidTextPrompt_ReturnsServiceUnavailable_WhenOpenAiUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;
    var textPrompt = "What is the capital of France?";

    // Act
    var response = await client.Completion(textPrompt, default(StreamPart?));

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    var errorContent = response.Error?.Content;
    errorContent.Should().NotBeNullOrEmpty();
    errorContent.Should().Contain(ExternalServices.OpenAiApi.ToString(),
        because: "the error message should indicate that the OpenAI API is the unavailable service");
  }

  [DependencyFact(ExternalServices.OpenAiApi)]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Completion_WithValidAudioPrompt_ReturnsOk()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;
    var audioData = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x08, 0x00, 0x00 }; // Mock WAV header
    var audioPrompt = new StreamPart(new MemoryStream(audioData), "test.wav", "audio/wav");

    // Act
    var response = await client.Completion(default(string?), audioPrompt);

    // Assert
    response.IsSuccessStatusCode.Should().BeTrue(
        because: "the OpenAI API should be available and return a successful response");
  }

  [ServiceUnavailableFact(ExternalServices.OpenAiApi)]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Completion_WithValidAudioPrompt_ReturnsServiceUnavailable_WhenOpenAiUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;
    var audioData = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x08, 0x00, 0x00 }; // Mock WAV header
    var audioPrompt = new StreamPart(new MemoryStream(audioData), "test.wav", "audio/wav");

    // Act
    var response = await client.Completion(default(string?), audioPrompt);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    var errorContent = response.Error?.Content;
    errorContent.Should().NotBeNullOrEmpty();
    errorContent.Should().Contain(ExternalServices.OpenAiApi.ToString(),
        because: "the error message should indicate that the OpenAI API is the unavailable service");
  }

  [DependencyFact(ExternalServices.OpenAiApi)]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Completion_WithBothTextAndAudioPrompts_ReturnsOk()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;
    var textPrompt = "Analyze this audio:";
    var audioData = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x08, 0x00, 0x00 }; // Mock WAV header
    var audioPrompt = new StreamPart(new MemoryStream(audioData), "test.wav", "audio/wav");

    // Act
    var response = await client.Completion(textPrompt, audioPrompt);

    // Assert
    response.IsSuccessStatusCode.Should().BeTrue(
        because: "the OpenAI API should be available and return a successful response");
  }

  [ServiceUnavailableFact(ExternalServices.OpenAiApi)]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Completion_WithBothTextAndAudioPrompts_ReturnsServiceUnavailable_WhenOpenAiUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;
    var textPrompt = "Analyze this audio:";
    var audioData = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x08, 0x00, 0x00 }; // Mock WAV header
    var audioPrompt = new StreamPart(new MemoryStream(audioData), "test.wav", "audio/wav");

    // Act
    var response = await client.Completion(textPrompt, audioPrompt);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    var errorContent = response.Error?.Content;
    errorContent.Should().NotBeNullOrEmpty();
    errorContent.Should().Contain(ExternalServices.OpenAiApi.ToString(),
        because: "the error message should indicate that the OpenAI API is the unavailable service");
  }

  [Fact]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Completion_WithoutAuthentication_ReturnsUnauthorized()
  {
    // Arrange
    var client = _apiClientFixture.UnauthenticatedAiApi;
    var textPrompt = "What is the capital of France?";

    // Act
    var response = await client.Completion(textPrompt, default(StreamPart?));

    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [DependencyFact(ExternalServices.OpenAiApi)]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Completion_WithEmptyRequest_ReturnsBadRequest()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;

    // Act
    var response = await client.Completion(null!, null!);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
        because: "empty request should return bad request when OpenAI API is available");
  }

  [ServiceUnavailableFact(ExternalServices.OpenAiApi)]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Completion_WithEmptyRequest_ReturnsServiceUnavailable_WhenOpenAiUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;

    // Act
    var response = await client.Completion(null!, null!);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    var errorContent = response.Error?.Content;
    errorContent.Should().NotBeNullOrEmpty();
    errorContent.Should().Contain(ExternalServices.OpenAiApi.ToString(),
        because: "the error message should indicate that the OpenAI API is the unavailable service");
  }

  // --- /api/transcribe Tests ---

  [DependencyFact(ExternalServices.OpenAiApi)]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Transcribe_WithValidAudioFile_ReturnsOk()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;
    var audioData = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x08, 0x00, 0x00 }; // Mock WAV header
    var audioFile = new StreamPart(new MemoryStream(audioData), "test.wav", "audio/wav");

    // Act
    var response = await client.Transcribe(audioFile);

    // Assert
    response.IsSuccessStatusCode.Should().BeTrue(
        because: "the OpenAI API should be available and return a successful response");
  }

  [ServiceUnavailableFact(ExternalServices.OpenAiApi)]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Transcribe_WithValidAudioFile_ReturnsServiceUnavailable_WhenOpenAiUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;
    var audioData = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x08, 0x00, 0x00 }; // Mock WAV header
    var audioFile = new StreamPart(new MemoryStream(audioData), "test.wav", "audio/wav");

    // Act
    var response = await client.Transcribe(audioFile);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    var errorContent = response.Error?.Content;
    errorContent.Should().NotBeNullOrEmpty();
    errorContent.Should().Contain(ExternalServices.OpenAiApi.ToString(),
        because: "the error message should indicate that the OpenAI API is the unavailable service");
  }

  [Fact]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Transcribe_WithoutAuthentication_ReturnsUnauthorized()
  {
    // Arrange
    var client = _apiClientFixture.UnauthenticatedAiApi;
    var audioData = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x08, 0x00, 0x00 }; // Mock WAV header
    var audioFile = new StreamPart(new MemoryStream(audioData), "test.wav", "audio/wav");

    // Act
    var response = await client.Transcribe(audioFile);

    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [DependencyFact(ExternalServices.OpenAiApi)]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Transcribe_WithoutAudioFile_ReturnsBadRequest()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;

    // Act
    var response = await client.Transcribe(null!);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.BadRequest,
        because: "missing audio file should return bad request when OpenAI API is available");
  }

  [ServiceUnavailableFact(ExternalServices.OpenAiApi)]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Transcribe_WithoutAudioFile_ReturnsServiceUnavailable_WhenOpenAiUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;

    // Act
    var response = await client.Transcribe(null!);

    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    var errorContent = response.Error?.Content;
    errorContent.Should().NotBeNullOrEmpty();
    errorContent.Should().Contain(ExternalServices.OpenAiApi.ToString(),
        because: "the error message should indicate that the OpenAI API is the unavailable service");
  }
}
