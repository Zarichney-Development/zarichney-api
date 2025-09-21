using OpenAI.Chat;
using Zarichney.Services.AI;
using Zarichney.Services.AI.Interfaces;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Server.Tests.Framework.Mocks;

namespace Zarichney.Server.Tests.TestData.Builders;

/// <summary>
/// Test data builder for LlmConversation entities following established patterns.
/// Enables fluent construction of test conversations with controlled state.
/// </summary>
public class LlmConversationBuilder : BaseBuilder<LlmConversationBuilder, LlmConversation>
{
  public LlmConversationBuilder()
  {
    WithDefaults();
  }

  /// <summary>
  /// Applies sensible default values for a test LlmConversation.
  /// </summary>
  /// <returns>The builder for method chaining.</returns>
  public LlmConversationBuilder WithDefaults()
  {
    // Set default values using reflection for init-only properties
    var idProperty = typeof(LlmConversation).GetProperty(nameof(LlmConversation.Id));
    idProperty?.SetValue(Entity, Guid.NewGuid().ToString());

    var systemPromptProperty = typeof(LlmConversation).GetProperty(nameof(LlmConversation.SystemPrompt));
    systemPromptProperty?.SetValue(Entity, "You are a helpful assistant.");

    var catalogNameProperty = typeof(LlmConversation).GetProperty(nameof(LlmConversation.PromptCatalogName));
    catalogNameProperty?.SetValue(Entity, "test-catalog");

    return Self();
  }

  /// <summary>
  /// Sets a specific conversation ID.
  /// </summary>
  public LlmConversationBuilder WithId(string id)
  {
    var idProperty = typeof(LlmConversation).GetProperty(nameof(LlmConversation.Id));
    idProperty?.SetValue(Entity, id);
    return Self();
  }

  /// <summary>
  /// Sets a specific system prompt.
  /// </summary>
  public LlmConversationBuilder WithSystemPrompt(string systemPrompt)
  {
    var systemPromptProperty = typeof(LlmConversation).GetProperty(nameof(LlmConversation.SystemPrompt));
    systemPromptProperty?.SetValue(Entity, systemPrompt);
    return Self();
  }

  /// <summary>
  /// Sets a specific prompt catalog name.
  /// </summary>
  public LlmConversationBuilder WithPromptCatalogName(string catalogName)
  {
    var catalogNameProperty = typeof(LlmConversation).GetProperty(nameof(LlmConversation.PromptCatalogName));
    catalogNameProperty?.SetValue(Entity, catalogName);
    return Self();
  }

  /// <summary>
  /// Adds a message to the conversation.
  /// </summary>
  public LlmConversationBuilder WithMessage(LlmMessage message)
  {
    Entity.Messages.Add(message);
    return Self();
  }

  /// <summary>
  /// Adds multiple messages to the conversation.
  /// </summary>
  public LlmConversationBuilder WithMessages(params LlmMessage[] messages)
  {
    foreach (var message in messages)
    {
      Entity.Messages.Add(message);
    }
    return Self();
  }

  /// <summary>
  /// Clears all messages from the conversation.
  /// </summary>
  public LlmConversationBuilder WithoutMessages()
  {
    Entity.Messages.Clear();
    return Self();
  }
}

/// <summary>
/// Test data builder for LlmMessage entities.
/// Note: LlmMessage has required members, so we create it directly rather than inheriting from BaseBuilder.
/// </summary>
public class LlmMessageBuilder
{
  private string _request = "Test request message";
  private object _response = "Test response";
  private DateTime _timestamp = DateTime.UtcNow;
  private ChatCompletionOptions? _options;
  private IChatCompletionWrapper? _chatCompletion;

  /// <summary>
  /// Sets a specific request message.
  /// </summary>
  public LlmMessageBuilder WithRequest(string request)
  {
    _request = request;
    return this;
  }

  /// <summary>
  /// Sets a specific response.
  /// </summary>
  public LlmMessageBuilder WithResponse(object response)
  {
    _response = response;
    return this;
  }

  /// <summary>
  /// Sets a specific timestamp.
  /// </summary>
  public LlmMessageBuilder WithTimestamp(DateTime timestamp)
  {
    _timestamp = timestamp;
    return this;
  }

  /// <summary>
  /// Sets specific chat completion options.
  /// </summary>
  public LlmMessageBuilder WithOptions(ChatCompletionOptions options)
  {
    _options = options;
    return this;
  }

  /// <summary>
  /// Sets a specific ChatCompletion.
  /// </summary>
  public LlmMessageBuilder WithChatCompletion(IChatCompletionWrapper chatCompletion)
  {
    _chatCompletion = chatCompletion;
    return this;
  }

  /// <summary>
  /// Builds the LlmMessage with the configured properties.
  /// </summary>
  public LlmMessage Build()
  {
    // Create the required ChatCompletion if not provided
    var chatCompletion = _chatCompletion ?? CreateMockChatCompletion();

    var message = new LlmMessage
    {
      Request = _request,
      Response = _response,
      Timestamp = _timestamp,
      ChatCompletion = chatCompletion,
      Options = _options
    };

    return message;
  }

  /// <summary>
  /// Creates a mock ChatCompletion for testing purposes.
  /// </summary>
  private static IChatCompletionWrapper CreateMockChatCompletion()
  {
    // Use new wrapper pattern instead of reflection-based approach
    return AiServiceMockFactory.CreateChatCompletionWrapper("Test completion");
  }
}
