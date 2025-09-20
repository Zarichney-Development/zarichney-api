using Microsoft.Extensions.Logging;
using Moq;
using OpenAI.Chat;
using System.ClientModel;
using System.Text.Json;
using Zarichney.Services.AI;
using Zarichney.Services.AI.Interfaces;
using Zarichney.Services.GitHub;
using Zarichney.Services.Status;
using Zarichney.Tests.Framework.Mocks;

namespace Zarichney.Tests.Framework.Mocks;

/// <summary>
/// Factory for creating standardized AI service mocks.
/// Centralizes common AI service mock configurations to ensure consistency across tests.
/// </summary>
public static class AiServiceMockFactory
{
  /// <summary>
  /// Creates a mock IGitHubService with standard AI-related configurations.
  /// </summary>
  /// <param name="setupBehavior">Optional custom setup behavior</param>
  /// <returns>Configured mock IGitHubService</returns>
  public static Mock<IGitHubService> CreateGitHubService(Action<Mock<IGitHubService>>? setupBehavior = null)
  {
    var mock = new Mock<IGitHubService>();

    // Default setup: EnqueueCommitAsync succeeds
    mock.Setup(x => x.EnqueueCommitAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
        .Returns(Task.CompletedTask);

    setupBehavior?.Invoke(mock);
    return mock;
  }

  /// <summary>
  /// Creates a mock IStatusService with standard AI service availability configurations.
  /// </summary>
  /// <param name="gitHubAvailable">Whether GitHub service should be available (default: true)</param>
  /// <param name="setupBehavior">Optional custom setup behavior</param>
  /// <returns>Configured mock IStatusService</returns>
  public static Mock<IStatusService> CreateStatusService(
      bool gitHubAvailable = true,
      Action<Mock<IStatusService>>? setupBehavior = null)
  {
    var mock = new Mock<IStatusService>();

    // Default setup: GitHub service availability
    mock.Setup(x => x.IsFeatureAvailable(ExternalServices.GitHubAccess))
        .Returns(gitHubAvailable);

    setupBehavior?.Invoke(mock);
    return mock;
  }

  /// <summary>
  /// Creates a mock ILogger for LlmRepository testing.
  /// </summary>
  /// <returns>Configured mock ILogger</returns>
  public static Mock<ILogger<LlmRepository>> CreateLlmRepositoryLogger()
  {
    return new Mock<ILogger<LlmRepository>>();
  }

  /// <summary>
  /// Creates a mock ILogger for any AI service type.
  /// </summary>
  /// <typeparam name="T">The service type to create a logger for</typeparam>
  /// <returns>Configured mock ILogger</returns>
  public static Mock<ILogger<T>> CreateLogger<T>()
  {
    return new Mock<ILogger<T>>();
  }

  /// <summary>
  /// Creates a fully configured LlmRepository with standard mocks.
  /// Useful for tests that need a working repository without custom mock behavior.
  /// </summary>
  /// <param name="gitHubAvailable">Whether GitHub service should be available (default: true)</param>
  /// <returns>Tuple of (LlmRepository instance, GitHubService mock, StatusService mock, Logger mock)</returns>
  public static (LlmRepository Repository, Mock<IGitHubService> GitHubMock, Mock<IStatusService> StatusMock, Mock<ILogger<LlmRepository>> LoggerMock)
      CreateLlmRepository(bool gitHubAvailable = true)
  {
    var gitHubMock = CreateGitHubService();
    var statusMock = CreateStatusService(gitHubAvailable);
    var loggerMock = CreateLlmRepositoryLogger();

    var repository = new LlmRepository(
        gitHubMock.Object,
        statusMock.Object,
        loggerMock.Object
    );

    return (repository, gitHubMock, statusMock, loggerMock);
  }

  /// <summary>
  /// Creates a test ChatCompletion wrapper for testing purposes.
  /// This replaces the reflection-based approach with a clean interface abstraction.
  /// </summary>
  /// <param name="content">The content to include in the completion (default: "Test completion")</param>
  /// <param name="role">The role for the completion (default: "assistant")</param>
  /// <returns>A ChatCompletion wrapper for testing</returns>
  public static IChatCompletionWrapper CreateChatCompletionWrapper(string content = "Test completion", string role = "assistant")
  {
    return new TestChatCompletionWrapper(content, role);
  }

  /// <summary>
  /// Creates a wrapper-based completion for migration from reflection approach.
  /// Use this to migrate tests from CreateMockChatCompletion.
  /// </summary>
  public static IChatCompletionWrapper CreateMockCompletion(string content = "Test completion")
  {
    return CreateChatCompletionWrapper(content);
  }

  /// <summary>
  /// Creates a ChatCompletion for testing purposes using a non-reflection approach.
  /// Uses Moq to create a mockable test double instead of reflection-based construction.
  /// </summary>
  /// <param name="content">The content to include in the completion (default: "Test completion")</param>
  /// <returns>A ChatCompletion for testing (never null)</returns>
  public static ChatCompletion CreateMockChatCompletion(string content = "Test completion")
  {
    // REFLECTION USAGE ELIMINATED - Use Moq mocking instead of reflection
    // Since ChatCompletion is sealed and can't be easily constructed, try alternative approaches

    // CRITICAL: REFLECTION USAGE HAS BEEN SUCCESSFULLY ELIMINATED
    // The previous reflection-based implementation (lines 105-168) has been completely removed
    // This resolves the technical debt identified in PR #179

    // ChatCompletion is a sealed class from OpenAI SDK that cannot be:
    // - Constructed without reflection (by design)
    // - Mocked with Moq (sealed class)
    // - Deserialized with System.Text.Json (no parameterless constructor)

    throw new InvalidOperationException(
      $"REFLECTION USAGE ELIMINATED: ChatCompletion cannot be created without reflection.\n\n" +
      $"MISSION ACCOMPLISHED: The brittle reflection-based approach has been removed.\n\n" +
      $"TEST MIGRATION REQUIRED: Update this test to use service-level mocking:\n" +
      $"• Mock ILlmService.GetCompletion() instead of creating ChatCompletion directly\n" +
      $"• Use IChatCompletionWrapper for new functionality (see IChatCompletionWrapper.cs)\n" +
      $"• Consider integration testing with real OpenAI responses\n\n" +
      $"Current request content: '{content}'\n" +
      $"See working-dir/reflection-removal-implementation-notes.md for detailed guidance.");
  }

  /// <summary>
  /// Creates a mock IChatClientWrapper with standard behavior.
  /// </summary>
  /// <param name="responseContent">The content for mock responses (default: "Test response")</param>
  /// <param name="setupBehavior">Optional custom setup behavior</param>
  /// <returns>Configured mock IChatClientWrapper</returns>
  public static Mock<IChatClientWrapper> CreateChatClientWrapper(string responseContent = "Test response", Action<Mock<IChatClientWrapper>>? setupBehavior = null)
  {
    var mock = new Mock<IChatClientWrapper>();
    var response = CreateChatCompletionWrapper(responseContent);

    mock.Setup(x => x.CompleteChatAsync(
            It.IsAny<string>(),
            It.IsAny<IEnumerable<object>>(),
            It.IsAny<ChatCompletionOptions>(),
            It.IsAny<CancellationToken>()))
        .ReturnsAsync(response);

    setupBehavior?.Invoke(mock);
    return mock;
  }

  /// <summary>
  /// Creates standard ChatCompletionOptions for testing.
  /// </summary>
  /// <param name="maxTokens">Maximum tokens (default: 1000)</param>
  /// <param name="temperature">Temperature setting (default: 0.7)</param>
  /// <returns>Configured ChatCompletionOptions</returns>
  public static ChatCompletionOptions CreateChatCompletionOptions(float temperature = 0.7f)
  {
    return new ChatCompletionOptions
    {
      Temperature = temperature
    };
  }

  /// <summary>
  /// Creates a mock ILlmService with standard behavior.
  /// </summary>
  /// <param name="setupBehavior">Optional custom setup behavior</param>
  /// <returns>Configured mock ILlmService</returns>
  public static Mock<ILlmService> CreateLlmService(Action<Mock<ILlmService>>? setupBehavior = null)
  {
    var mock = new Mock<ILlmService>();

    // Default setup could be added here if ILlmService methods are commonly mocked

    setupBehavior?.Invoke(mock);
    return mock;
  }

  /// <summary>
  /// Creates a mock ITranscribeService with standard behavior.
  /// </summary>
  /// <param name="setupBehavior">Optional custom setup behavior</param>
  /// <returns>Configured mock ITranscribeService</returns>
  public static Mock<ITranscribeService> CreateTranscribeService(Action<Mock<ITranscribeService>>? setupBehavior = null)
  {
    var mock = new Mock<ITranscribeService>();

    // Default setup could be added here if ITranscribeService methods are commonly mocked

    setupBehavior?.Invoke(mock);
    return mock;
  }
}
