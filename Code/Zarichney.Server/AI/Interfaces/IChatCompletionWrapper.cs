using OpenAI.Chat;

namespace Zarichney.Services.AI.Interfaces;

/// <summary>
/// Wrapper interface for ChatCompletion objects to enable proper unit testing.
/// Provides a testable abstraction over the OpenAI ChatCompletion type.
/// </summary>
public interface IChatCompletionWrapper
{
    /// <summary>
    /// Gets the content of the chat completion response.
    /// </summary>
    string? Content { get; }

    /// <summary>
    /// Gets the role of the chat completion (e.g., "assistant", "user").
    /// </summary>
    string? Role { get; }

    /// <summary>
    /// Gets the timestamp when the completion was created.
    /// </summary>
    DateTime CreatedAt { get; }
}

/// <summary>
/// Wrapper interface for chat client operations to enable proper unit testing.
/// Provides a testable abstraction over OpenAI chat completion functionality.
/// </summary>
public interface IChatClientWrapper
{
    /// <summary>
    /// Completes a chat conversation using the specified model and messages.
    /// </summary>
    /// <param name="model">The model to use for completion</param>
    /// <param name="messages">The conversation messages</param>
    /// <param name="options">Optional chat completion options</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A wrapped chat completion response</returns>
    Task<IChatCompletionWrapper> CompleteChatAsync(
        string model,
        IEnumerable<object> messages,
        ChatCompletionOptions? options = null,
        CancellationToken cancellationToken = default);
}