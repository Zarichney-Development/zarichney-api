using Microsoft.Extensions.Logging;
using Moq;
using OpenAI.Chat;
using Zarichney.Services.AI;
using Zarichney.Services.GitHub;
using Zarichney.Services.Status;

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
    /// Creates a mock ChatCompletion for testing purposes.
    /// This is a complex object from the OpenAI SDK that's difficult to instantiate directly.
    /// </summary>
    /// <param name="content">The content to include in the completion (default: "Test completion")</param>
    /// <returns>Mock ChatCompletion or null if creation fails</returns>
    public static ChatCompletion? CreateMockChatCompletion(string content = "Test completion")
    {
        try
        {
            // Note: ChatCompletion is from Azure.AI.OpenAI and may be difficult to mock
            // In real scenarios, we might need to use reflection or a different approach
            // This is a placeholder implementation - may need adjustment based on SDK version
            return null; // For now, returning null - tests will handle accordingly
        }
        catch
        {
            return null;
        }
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