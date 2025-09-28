using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAI;
using Xunit;
using Zarichney.Config;
using Zarichney.Services.AI;
using Zarichney.Services.Sessions;
using Zarichney.Services.Status;
using AutoMapper;
using Zarichney.Tests.TestData.Builders;
using Zarichney.Tests.Framework.Attributes;

namespace Zarichney.Tests.Unit.Services.AI;

/// <summary>
/// Simplified unit tests for LlmService focusing on essential behaviors
/// </summary>
[Trait(TestCategories.Feature, TestCategories.AI)]
public class LlmServiceTests
{
  private readonly Mock<ILogger<LlmService>> _mockLogger;
  private readonly Mock<OpenAIClient> _mockClient;
  private readonly Mock<IMapper> _mockMapper;
  private readonly Mock<ISessionManager> _mockSessionManager;
  private readonly Mock<IScopeContainer> _mockScope;
  private readonly LlmConfig _config;
  private readonly LlmService _sut;

  public LlmServiceTests()
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

  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_WithValidDependencies_CreatesInstance()
  {
    // Assert
    _sut.Should().NotBeNull("because the service should be created with valid dependencies");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CreateThread_WhenClientIsNull_ThrowsConfigurationMissingException()
  {
    // Arrange
    var sutWithNullClient = new LlmServiceBuilder()
        .WithNullClient()
        .WithMapper(_mockMapper.Object)
        .WithConfig(_config)
        .WithSessionManager(_mockSessionManager)
        .WithScope(_mockScope.Object)
        .WithLogger(_mockLogger)
        .Build();

    // Act
    var act = () => sutWithNullClient.CreateThread();

    // Assert
    await act.Should().ThrowAsync<ConfigurationMissingException>()
        .WithMessage($"*{nameof(LlmConfig)}*{nameof(LlmConfig.ApiKey)}*");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CreateMessage_WhenClientIsNull_ThrowsConfigurationMissingException()
  {
    // Arrange
    var sutWithNullClient = new LlmServiceBuilder()
        .WithNullClient()
        .WithMapper(_mockMapper.Object)
        .WithConfig(_config)
        .WithSessionManager(_mockSessionManager)
        .WithScope(_mockScope.Object)
        .WithLogger(_mockLogger)
        .Build();

    // Act
    var act = () => sutWithNullClient.CreateMessage("thread_123", "content");

    // Assert
    await act.Should().ThrowAsync<ConfigurationMissingException>();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CreateRun_WhenClientIsNull_ThrowsConfigurationMissingException()
  {
    // Arrange
    var sutWithNullClient = new LlmServiceBuilder()
        .WithNullClient()
        .WithMapper(_mockMapper.Object)
        .WithConfig(_config)
        .WithSessionManager(_mockSessionManager)
        .WithScope(_mockScope.Object)
        .WithLogger(_mockLogger)
        .Build();

    // Act
    var act = () => sutWithNullClient.CreateRun("thread_123", "asst_456");

    // Assert
    await act.Should().ThrowAsync<ConfigurationMissingException>();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetRun_WhenClientIsNull_ThrowsConfigurationMissingException()
  {
    // Arrange
    var sutWithNullClient = new LlmServiceBuilder()
        .WithNullClient()
        .WithMapper(_mockMapper.Object)
        .WithConfig(_config)
        .WithSessionManager(_mockSessionManager)
        .WithScope(_mockScope.Object)
        .WithLogger(_mockLogger)
        .Build();

    // Act
    var act = () => sutWithNullClient.GetRun("thread_123", "run_456");

    // Assert
    await act.Should().ThrowAsync<ConfigurationMissingException>();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task SubmitToolOutputToRun_WhenClientIsNull_ThrowsConfigurationMissingException()
  {
    // Arrange
    var sutWithNullClient = new LlmServiceBuilder()
        .WithNullClient()
        .WithMapper(_mockMapper.Object)
        .WithConfig(_config)
        .WithSessionManager(_mockSessionManager)
        .WithScope(_mockScope.Object)
        .WithLogger(_mockLogger)
        .Build();

    // Act
    var act = () => sutWithNullClient.SubmitToolOutputToRun("thread_123", "run_456", "tool_789", "output");

    // Assert
    await act.Should().ThrowAsync<ConfigurationMissingException>();
  }
}
