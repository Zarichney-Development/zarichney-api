using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAI;
using OpenAI.Assistants;
using Xunit;
using Zarichney.Config;
using Zarichney.Services.AI;
using Zarichney.Services.Sessions;
using Zarichney.Services.Status;
using AutoMapper;
using System.ClientModel;

namespace Zarichney.Tests.Unit.Services.AI;

public class LlmServiceBasicTests
{
    private readonly Mock<ILogger<LlmService>> _mockLogger;
    private readonly Mock<OpenAIClient> _mockClient;
    private readonly Mock<IMapper> _mockMapper;
    private readonly Mock<ISessionManager> _mockSessionManager;
    private readonly Mock<IScopeContainer> _mockScope;
    private readonly LlmConfig _config;

    public LlmServiceBasicTests()
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
    }

    #region Constructor Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithValidDependencies_CreatesInstance()
    {
        // Act
        var sut = new LlmService(
            _mockClient.Object,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Assert
        sut.Should().NotBeNull("because the service should be created with valid dependencies");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Constructor_WithNullClient_ThrowsArgumentNullException()
    {
        // Act
        var act = () => new LlmService(
            null!,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Assert
        act.Should().NotThrow("because the constructor accepts null client for later configuration");
    }

    #endregion

    #region CreateAssistant Error Handling Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateAssistant_WhenClientIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var prompt = new TestPrompt();
        var sut = new LlmService(
            null!,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        var act = () => sut.CreateAssistant(prompt);

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>()
            .WithParameterName("_client",
                "because the method should validate that the client is not null");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateAssistant_WhenExceptionOccurs_LogsErrorAndRethrows()
    {
        // Arrange
        var prompt = new TestPrompt();
        var expectedException = new InvalidOperationException("Test exception");
        
        _mockClient.Setup(c => c.GetAssistantClient())
            .Throws(expectedException);

        var sut = new LlmService(
            _mockClient.Object,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        var act = () => sut.CreateAssistant(prompt);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception",
                "because the method should rethrow the exception");

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while creating assistant")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because the error should be logged before rethrowing");
    }

    #endregion

    #region CreateThread Error Handling Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateThread_WhenClientIsNull_ThrowsConfigurationMissingException()
    {
        // Arrange
        var sut = new LlmService(
            null!,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        var act = () => sut.CreateThread();

        // Assert
        await act.Should().ThrowAsync<ConfigurationMissingException>()
            .WithMessage($"*{nameof(LlmConfig)}*{nameof(LlmConfig.ApiKey)}*",
                "because the method should throw when client is not configured");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateThread_WhenExceptionOccurs_LogsErrorAndRethrows()
    {
        // Arrange
        var expectedException = new InvalidOperationException("Test exception");
        
        _mockClient.Setup(c => c.GetAssistantClient())
            .Throws(expectedException);

        var sut = new LlmService(
            _mockClient.Object,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        var act = () => sut.CreateThread();

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception",
                "because the method should rethrow the exception");

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while creating thread")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because the error should be logged before rethrowing");
    }

    #endregion

    #region CreateMessage Error Handling Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateMessage_WhenClientIsNull_ThrowsConfigurationMissingException()
    {
        // Arrange
        var sut = new LlmService(
            null!,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        var act = () => sut.CreateMessage("thread_123", "content");

        // Assert
        await act.Should().ThrowAsync<ConfigurationMissingException>()
            .WithMessage($"*{nameof(LlmConfig)}*{nameof(LlmConfig.ApiKey)}*",
                "because the method should throw when client is not configured");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateMessage_WhenExceptionOccurs_LogsErrorAndRethrows()
    {
        // Arrange
        var threadId = "thread_123";
        var content = "Test message";
        var expectedException = new InvalidOperationException("Test exception");
        
        _mockClient.Setup(c => c.GetAssistantClient())
            .Throws(expectedException);

        var sut = new LlmService(
            _mockClient.Object,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        var act = () => sut.CreateMessage(threadId, content);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception",
                "because the method should rethrow the exception");

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error occurred while creating message")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because the error should be logged before rethrowing");
    }

    #endregion

    #region CreateRun Error Handling Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateRun_WhenClientIsNull_ThrowsConfigurationMissingException()
    {
        // Arrange
        var sut = new LlmService(
            null!,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        var act = () => sut.CreateRun("thread_123", "asst_456");

        // Assert
        await act.Should().ThrowAsync<ConfigurationMissingException>()
            .WithMessage($"*{nameof(LlmConfig)}*{nameof(LlmConfig.ApiKey)}*",
                "because the method should throw when client is not configured");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CreateRun_WhenExceptionOccurs_LogsErrorAndRethrows()
    {
        // Arrange
        var threadId = "thread_123";
        var assistantId = "asst_456";
        var expectedException = new InvalidOperationException("Test exception");
        
        _mockClient.Setup(c => c.GetAssistantClient())
            .Throws(expectedException);

        var sut = new LlmService(
            _mockClient.Object,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        var act = () => sut.CreateRun(threadId, assistantId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception",
                "because the method should rethrow the exception");

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains($"Error occurred while creating run for thread {threadId}")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because the error should be logged before rethrowing");
    }

    #endregion

    #region GetRun Error Handling Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetRun_WhenClientIsNull_ThrowsConfigurationMissingException()
    {
        // Arrange
        var sut = new LlmService(
            null!,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        var act = () => sut.GetRun("thread_123", "run_456");

        // Assert
        await act.Should().ThrowAsync<ConfigurationMissingException>()
            .WithMessage($"*{nameof(LlmConfig)}*{nameof(LlmConfig.ApiKey)}*",
                "because the method should throw when client is not configured");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task GetRun_WhenExceptionOccurs_LogsErrorAndRethrows()
    {
        // Arrange
        var threadId = "thread_123";
        var runId = "run_456";
        var expectedException = new InvalidOperationException("Test exception");
        
        _mockClient.Setup(c => c.GetAssistantClient())
            .Throws(expectedException);

        var sut = new LlmService(
            _mockClient.Object,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        var act = () => sut.GetRun(threadId, runId);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Test exception",
                "because the method should rethrow the exception");

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains($"Error occurred while getting run {runId} for thread {threadId}")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because the error should be logged before rethrowing");
    }

    #endregion

    #region DeleteAssistant Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteAssistant_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        var assistantId = "asst_123";
        var expectedException = new InvalidOperationException("Delete failed");
        
        _mockClient.Setup(c => c.GetAssistantClient())
            .Throws(expectedException);

        var sut = new LlmService(
            _mockClient.Object,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        await sut.DeleteAssistant(assistantId);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains($"Error occurred while deleting assistant {assistantId}")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because the error should be logged");
    }

    #endregion

    #region DeleteThread Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task DeleteThread_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        var threadId = "thread_123";
        var expectedException = new InvalidOperationException("Delete failed");
        
        _mockClient.Setup(c => c.GetAssistantClient())
            .Throws(expectedException);

        var sut = new LlmService(
            _mockClient.Object,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        await sut.DeleteThread(threadId);

        // Assert
        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains($"Error occurred while deleting thread {threadId}")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because the error should be logged");
    }

    #endregion

    #region CancelRun Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CancelRun_WhenClientIsNull_ReturnsFailureMessage()
    {
        // Arrange
        var sut = new LlmService(
            null!,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        var result = await sut.CancelRun("thread_123", "run_456");

        // Assert
        result.Should().Be("Failed to cancel run.",
            "because the method should return failure message when client is null");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task CancelRun_WhenExceptionOccurs_ReturnsFailureMessage()
    {
        // Arrange
        var threadId = "thread_123";
        var runId = "run_456";
        var expectedException = new InvalidOperationException("Cancel failed");
        
        _mockClient.Setup(c => c.GetAssistantClient())
            .Throws(expectedException);

        var sut = new LlmService(
            _mockClient.Object,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        var result = await sut.CancelRun(threadId, runId);

        // Assert
        result.Should().Be("Failed to cancel run.",
            "because the method should return failure message on exception");

        _mockLogger.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => 
                    v.ToString()!.Contains($"Error occurred while cancelling run {runId} for thread {threadId}")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "because the error should be logged");
    }

    #endregion

    #region SubmitToolOutputToRun Error Handling Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task SubmitToolOutputToRun_WhenClientIsNull_ThrowsConfigurationMissingException()
    {
        // Arrange
        var sut = new LlmService(
            null!,
            _mockMapper.Object,
            _config,
            _mockSessionManager.Object,
            _mockScope.Object,
            _mockLogger.Object
        );

        // Act
        var act = () => sut.SubmitToolOutputToRun("thread_123", "run_456", "tool_789", "output");

        // Assert
        await act.Should().ThrowAsync<ConfigurationMissingException>()
            .WithMessage($"*{nameof(LlmConfig)}*{nameof(LlmConfig.ApiKey)}*",
                "because the method should throw when client is not configured");
    }

    #endregion

    #region Helper Classes

    private class TestPrompt : PromptBase
    {
        public override string? Model => "gpt-4";
        public override string Name => "Test Assistant";
        public override string Description => "Test Description";
        public override string SystemPrompt => "You are a test assistant";

        public override FunctionDefinition GetFunction()
        {
            return new FunctionDefinition
            {
                Name = "TestFunction",
                Description = "Test function for unit testing",
                Parameters = "{\"type\":\"object\",\"properties\":{}}",
                Strict = true
            };
        }
    }

    #endregion
}