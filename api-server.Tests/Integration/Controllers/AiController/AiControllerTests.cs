using System.Net;
using Zarichney.Tests.Framework.Fixtures;
using Zarichney.Tests.Integration;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Services.Status;
using Refit;
using Xunit;
using Xunit.Abstractions;

namespace Zarichney.Tests.Integration.Controllers.AiController;

/// <summary>
/// Integration tests for the AiController endpoints (/api/completion and /api/transcribe).
/// These tests verify that the Refit client generation with addContentTypeHeaders: false
/// resolves multipart/form-data Content-Type header conflicts.
/// </summary>
[Collection("Integration")]
public class AiControllerTests : IntegrationTestBase
{
  public AiControllerTests(ApiClientFixture apiClientFixture, ITestOutputHelper output)
      : base(apiClientFixture, output)
  {
  }

  // --- /api/completion Tests ---

  [Fact]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Completion_WithValidTextPrompt_ReturnsOkOrServiceUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;
    var textPrompt = "What is the capital of France?";

    // Act
    var response = await client.Completion(textPrompt, null);

    // Assert - Should either succeed (if OpenAI available) or return 503 (if unavailable)
    Assert.True(
        response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.ServiceUnavailable,
        $"Expected 200 OK or 503 Service Unavailable but got {response.StatusCode}: {response.Error?.Content}");

    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
    {
      // Verify it's specifically due to AI services being unavailable
      var errorContent = response.Error?.Content;
      Assert.True(
          !string.IsNullOrEmpty(errorContent) && (
              errorContent.Contains("OpenAiApi") ||
              errorContent.Contains("OpenAI") ||
              errorContent.Contains("AI") ||
              errorContent.Contains("Service Temporarily Unavailable")
          ),
          $"Expected error content to mention AI service unavailability, but got: {errorContent}");
    }
  }

  [Fact]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Completion_WithValidAudioPrompt_ReturnsOkOrServiceUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;
    var audioData = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x08, 0x00, 0x00 }; // Mock WAV header
    var audioPrompt = new StreamPart(new MemoryStream(audioData), "test.wav", "audio/wav");

    // Act
    var response = await client.Completion(null, audioPrompt);

    // Assert - Should either succeed (if OpenAI available) or return 503 (if unavailable)
    Assert.True(
        response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.ServiceUnavailable,
        $"Expected 200 OK or 503 Service Unavailable but got {response.StatusCode}: {response.Error?.Content}");

    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
    {
      // Verify it's specifically due to AI services being unavailable
      var errorContent = response.Error?.Content;
      Assert.True(
          !string.IsNullOrEmpty(errorContent) && (
              errorContent.Contains("OpenAiApi") ||
              errorContent.Contains("OpenAI") ||
              errorContent.Contains("AI") ||
              errorContent.Contains("Service Temporarily Unavailable")
          ),
          $"Expected error content to mention AI service unavailability, but got: {errorContent}");
    }
  }

  [Fact]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Completion_WithBothTextAndAudioPrompts_ReturnsOkOrServiceUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;
    var textPrompt = "Analyze this audio:";
    var audioData = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x08, 0x00, 0x00 }; // Mock WAV header
    var audioPrompt = new StreamPart(new MemoryStream(audioData), "test.wav", "audio/wav");

    // Act
    var response = await client.Completion(textPrompt, audioPrompt);

    // Assert - Should either succeed (if OpenAI available) or return 503 (if unavailable)
    Assert.True(
        response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.ServiceUnavailable,
        $"Expected 200 OK or 503 Service Unavailable but got {response.StatusCode}: {response.Error?.Content}");

    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
    {
      // Verify it's specifically due to AI services being unavailable
      var errorContent = response.Error?.Content;
      Assert.True(
          !string.IsNullOrEmpty(errorContent) && (
              errorContent.Contains("OpenAiApi") ||
              errorContent.Contains("OpenAI") ||
              errorContent.Contains("AI") ||
              errorContent.Contains("Service Temporarily Unavailable")
          ),
          $"Expected error content to mention AI service unavailability, but got: {errorContent}");
    }
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
    var response = await client.Completion(textPrompt, null);

    // Assert
    Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
  }

  [Fact]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Completion_WithEmptyRequest_ReturnsBadRequestOrServiceUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;

    // Act
    var response = await client.Completion(null, null);

    // Assert - Should return 400 Bad Request if service available, or 503 if OpenAI unavailable
    Assert.True(
        response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.ServiceUnavailable,
        $"Expected 400 Bad Request or 503 Service Unavailable but got {response.StatusCode}: {response.Error?.Content}");

    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
    {
      // Verify it's specifically due to AI services being unavailable
      var errorContent = response.Error?.Content;
      Assert.True(
          !string.IsNullOrEmpty(errorContent) && (
              errorContent.Contains("OpenAiApi") ||
              errorContent.Contains("OpenAI") ||
              errorContent.Contains("AI") ||
              errorContent.Contains("Service Temporarily Unavailable")
          ),
          $"Expected error content to mention AI service unavailability, but got: {errorContent}");
    }
  }

  // --- /api/transcribe Tests ---

  [Fact]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Transcribe_WithValidAudioFile_ReturnsOkOrServiceUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;
    var audioData = new byte[] { 0x52, 0x49, 0x46, 0x46, 0x24, 0x08, 0x00, 0x00 }; // Mock WAV header
    var audioFile = new StreamPart(new MemoryStream(audioData), "test.wav", "audio/wav");

    // Act
    var response = await client.Transcribe(audioFile);

    // Assert - Should either succeed (if OpenAI available) or return 503 (if unavailable)
    Assert.True(
        response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.ServiceUnavailable,
        $"Expected 200 OK or 503 Service Unavailable but got {response.StatusCode}: {response.Error?.Content}");

    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
    {
      // Verify it's specifically due to AI services being unavailable
      var errorContent = response.Error?.Content;
      Assert.True(
          !string.IsNullOrEmpty(errorContent) && (
              errorContent.Contains("OpenAiApi") ||
              errorContent.Contains("OpenAI") ||
              errorContent.Contains("AI") ||
              errorContent.Contains("Service Temporarily Unavailable")
          ),
          $"Expected error content to mention AI service unavailability, but got: {errorContent}");
    }
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

  [Fact]
  [Trait(TestCategories.Category, TestCategories.Integration)]
  [Trait(TestCategories.Component, TestCategories.Controller)]
  [Trait(TestCategories.Feature, TestCategories.AI)]
  public async Task Transcribe_WithoutAudioFile_ReturnsBadRequestOrServiceUnavailable()
  {
    // Arrange
    var client = _apiClientFixture.AuthenticatedAiApi;

    // Act
    var response = await client.Transcribe(null);

    // Assert - Should return 400 Bad Request if service available, or 503 if OpenAI unavailable
    Assert.True(
        response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.ServiceUnavailable,
        $"Expected 400 Bad Request or 503 Service Unavailable but got {response.StatusCode}: {response.Error?.Content}");

    if (response.StatusCode == HttpStatusCode.ServiceUnavailable)
    {
      // Verify it's specifically due to AI services being unavailable
      var errorContent = response.Error?.Content;
      Assert.True(
          !string.IsNullOrEmpty(errorContent) && (
              errorContent.Contains("OpenAiApi") ||
              errorContent.Contains("OpenAI") ||
              errorContent.Contains("AI") ||
              errorContent.Contains("Service Temporarily Unavailable")
          ),
          $"Expected error content to mention AI service unavailability, but got: {errorContent}");
    }
  }
}
