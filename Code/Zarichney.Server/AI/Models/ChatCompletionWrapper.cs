using OpenAI.Chat;
using Zarichney.Services.AI.Interfaces;

namespace Zarichney.Services.AI.Models;

/// <summary>
/// Production wrapper for ChatCompletion objects to enable proper interface abstraction.
/// Provides a wrapper around the OpenAI ChatCompletion type that implements IChatCompletionWrapper.
/// </summary>
public class ChatCompletionWrapper : IChatCompletionWrapper
{
    private readonly ChatCompletion _chatCompletion;

    /// <summary>
    /// Initializes a new instance of the ChatCompletionWrapper class.
    /// </summary>
    /// <param name="chatCompletion">The ChatCompletion instance to wrap</param>
    public ChatCompletionWrapper(ChatCompletion chatCompletion)
    {
        _chatCompletion = chatCompletion ?? throw new ArgumentNullException(nameof(chatCompletion));
    }

    /// <summary>
    /// Gets the content of the chat completion response.
    /// </summary>
    public string? Content => _chatCompletion.Content?.FirstOrDefault()?.Text;

    /// <summary>
    /// Gets the role of the chat completion (e.g., "assistant", "user").
    /// </summary>
    public string? Role => _chatCompletion.Role.ToString();

    /// <summary>
    /// Gets the timestamp when the completion was created.
    /// </summary>
    public DateTime CreatedAt => _chatCompletion.CreatedAt.DateTime;

    /// <summary>
    /// Gets the underlying ChatCompletion object.
    /// </summary>
    public ChatCompletion UnderlyingCompletion => _chatCompletion;
}