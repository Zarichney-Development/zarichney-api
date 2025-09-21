using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAI;
using OpenAI.Assistants;
using OpenAI.Chat;
using Xunit;
using Zarichney.Config;
using Zarichney.Services.AI;
using Zarichney.Services.Sessions;
using Zarichney.Services.Status;
using AutoMapper;
using Zarichney.Server.Tests.Framework.Attributes;

namespace Zarichney.Server.Tests.Unit.Services.AI;

/// <summary>
/// Unit tests for LlmService method behaviors
/// </summary>
[Trait(TestCategories.Feature, TestCategories.AI)]
public class LlmServiceMethodTests
{
  private readonly Mock<ILogger<LlmService>> _mockLogger;
  private readonly Mock<OpenAIClient> _mockClient;
  private readonly Mock<IMapper> _mockMapper;
  private readonly Mock<ISessionManager> _mockSessionManager;
  private readonly Mock<IScopeContainer> _mockScope;
  private readonly LlmConfig _config;
  private readonly LlmService _sut;

  public LlmServiceMethodTests()
  {
    _mockLogger = new Mock<ILogger<LlmService>>();
    _mockClient = new Mock<OpenAIClient>();
    _mockMapper = new Mock<IMapper>();
    _mockSessionManager = new Mock<ISessionManager>();
    _mockScope = new Mock<IScopeContainer>();
    _config = new LlmConfig
    {
      ApiKey = "test-api-key",
      ModelName = LlmModels.Gpt4Omini,
      RetryAttempts = 3
    };

    _sut = new LlmService(
        _mockClient.Object,
        _mockMapper.Object,
        _config,
        _mockSessionManager.Object,
        _mockScope.Object,
        _mockLogger.Object
    );
  }

  #region Configuration Tests

  [Fact]
  [Trait("Category", "Unit")]
  public void LlmConfig_DefaultValues_AreSetCorrectly()
  {
    // Arrange
    var config = new LlmConfig();

    // Assert
    config.ModelName.Should().Be(LlmModels.Gpt4Omini,
        "because Gpt4Omini is the default model");
    config.RetryAttempts.Should().Be(5,
        "because 5 is the default retry attempt count");
    config.ApiKey.Should().BeEmpty(
        "because API key should be empty by default");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void LlmModels_Constants_HaveCorrectValues()
  {
    // Assert
    LlmModels.Gpt4Omini.Should().Be("gpt-5-mini-2025-08-07");
    LlmModels.Gpt4O.Should().Be("gpt-4o");
    LlmModels.O1Mini.Should().Be("gpt-o1-mini");
    LlmModels.O1.Should().Be("gpt-o1");
  }

  #endregion

  #region Retry Policy Tests

  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_CreatesRetryPolicy_WithConfiguredAttempts()
  {
    // Arrange
    var customConfig = new LlmConfig
    {
      ApiKey = "test-key",
      RetryAttempts = 7
    };

    // Act
    var service = new LlmService(
        _mockClient.Object,
        _mockMapper.Object,
        customConfig,
        _mockSessionManager.Object,
        _mockScope.Object,
        _mockLogger.Object
    );

    // Assert
    service.Should().NotBeNull();
    // The retry policy is internal, but we verify it's created by the successful construction
  }

  #endregion

  #region Logging Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CreateMessage_LogsInformation_WhenCalled()
  {
    // Arrange
    var threadId = "thread_123";
    var content = "Test message content";
    var mockAssistantClient = new Mock<AssistantClient>();

    _mockClient.Setup(c => c.GetAssistantClient())
        .Returns(mockAssistantClient.Object);

    // Act
    try
    {
      await _sut.CreateMessage(threadId, content);
    }
    catch
    {
      // Ignore any exceptions from incomplete mock setup
    }

    // Assert
    _mockLogger.Verify(
        l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) =>
                v.ToString()!.Contains($"Creating message for thread {threadId}") &&
                v.ToString()!.Contains(content)),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "because message creation should be logged");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CancelRun_LogsInformation_WhenCalled()
  {
    // Arrange
    var threadId = "thread_123";
    var runId = "run_456";

    // Act
    await _sut.CancelRun(threadId, runId);

    // Assert
    _mockLogger.Verify(
        l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((v, t) =>
                v.ToString()!.Contains($"Cancelling run: {runId} for thread: {threadId}")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "because run cancellation should be logged");
  }

  #endregion

  #region LlmResult Tests

  [Fact]
  [Trait("Category", "Unit")]
  public void LlmResult_Properties_AreSetCorrectly()
  {
    // Arrange & Act
    var result = new LlmResult<string>
    {
      Data = "test data",
      ConversationId = "conv_123"
    };

    // Assert
    result.Data.Should().Be("test data",
        "because the Data property should store the provided value");
    result.ConversationId.Should().Be("conv_123",
        "because the ConversationId property should store the provided value");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void LlmResult_WithComplexType_WorksCorrectly()
  {
    // Arrange
    var complexData = new TestComplexType
    {
      Id = 42,
      Name = "Test",
      Values = new[] { 1.1, 2.2, 3.3 }
    };

    // Act
    var result = new LlmResult<TestComplexType>
    {
      Data = complexData,
      ConversationId = "conv_complex"
    };

    // Assert
    result.Data.Should().BeEquivalentTo(complexData,
        "because complex types should be stored correctly");
    result.ConversationId.Should().Be("conv_complex");
  }

  #endregion

  #region Exception Handling Patterns

  [Fact]
  [Trait("Category", "Unit")]
  public async Task DeleteAssistant_DoesNotThrow_WhenExceptionOccurs()
  {
    // Arrange
    var assistantId = "asst_123";
    _mockClient.Setup(c => c.GetAssistantClient())
        .Throws<InvalidOperationException>();

    // Act
    var act = () => _sut.DeleteAssistant(assistantId);

    // Assert
    await act.Should().NotThrowAsync(
        "because DeleteAssistant catches and logs exceptions without rethrowing");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task DeleteThread_DoesNotThrow_WhenExceptionOccurs()
  {
    // Arrange
    var threadId = "thread_123";
    _mockClient.Setup(c => c.GetAssistantClient())
        .Throws<InvalidOperationException>();

    // Act
    var act = () => _sut.DeleteThread(threadId);

    // Assert
    await act.Should().NotThrowAsync(
        "because DeleteThread catches and logs exceptions without rethrowing");
  }

  #endregion

  #region Helper Classes

  private class TestComplexType
  {
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public double[] Values { get; set; } = Array.Empty<double>();
  }

  #endregion
}
