using Zarichney.Services.AI.Interfaces;

namespace Zarichney.Server.Tests.Framework.Mocks;

/// <summary>
/// Test implementation of IChatCompletionWrapper for unit testing.
/// Provides a simple, testable implementation of chat completion functionality.
/// </summary>
public class TestChatCompletionWrapper : IChatCompletionWrapper
{
  /// <inheritdoc />
  public string? Content { get; set; }

  /// <inheritdoc />
  public string? Role { get; set; }

  /// <inheritdoc />
  public DateTime CreatedAt { get; set; }

  /// <summary>
  /// Initializes a new instance of the TestChatCompletionWrapper class.
  /// </summary>
  /// <param name="content">The content for the chat completion (default: "Test completion")</param>
  /// <param name="role">The role for the chat completion (default: "assistant")</param>
  public TestChatCompletionWrapper(string content = "Test completion", string role = "assistant")
  {
    Content = content;
    Role = role;
    CreatedAt = DateTime.UtcNow;
  }
}
