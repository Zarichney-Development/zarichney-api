using System.Text.Json;
using AutoMapper;
using OpenAI;
using OpenAI.Assistants;
using OpenAI.Chat;
using Polly;
using Polly.Retry;
using Zarichney.Config;
using Zarichney.Services.Sessions;
using ILogger = Serilog.ILogger;

namespace Zarichney.Services.AI;

/// <summary>
/// A generic result class that contains both the primary data and the conversation ID.
/// </summary>
/// <typeparam name="T">The type of the data being returned.</typeparam>
public class LlmResult<T>
{
  /// <summary>
  /// The primary data returned from the LLM interaction.
  /// </summary>
  public required T Data { get; init; }

  /// <summary>
  /// The ID of the conversation associated with this interaction.
  /// </summary>
  public required string ConversationId { get; init; }
}

public class LlmConfig : IConfig
{
  public string ModelName { get; init; } = LlmModels.Gpt4Omini;
  public int RetryAttempts { get; init; } = 5;
  [RequiresConfiguration(Feature.LLM, Feature.AiServices)]
  public string ApiKey { get; init; } = string.Empty;
}

public static class LlmModels
{
  public const string Gpt4Omini = "gpt-4o-mini";
  public const string Gpt4O = "gpt-4o";
  public const string O1Mini = "gpt-o1-mini";
  public const string O1 = "gpt-o1";
}

public interface ILlmService
{
  Task<string> CreateAssistant(PromptBase prompt);
  Task<string> CreateThread();
  Task CreateMessage(string threadId, string content, MessageRole role = MessageRole.User);
  Task<string> CreateRun(string threadId, string assistantId, bool requiresToolConstraint = true);

  Task<string> GetCompletionContent(string userPrompt, string? conversationId = null,
    ChatCompletionOptions? options = null, int? retryCount = null);

  Task<LlmResult<string>> GetCompletionContent(List<ChatMessage> messages, string? conversationId = null,
    ChatCompletionOptions? options = null, int? retryCount = null);

  Task<(bool isComplete, RunStatus status)> GetRun(string threadId, string runId);
  Task<string> CancelRun(string threadId, string runId);
  Task SubmitToolOutputToRun(string threadId, string runId, string toolCallId, string output);
  Task DeleteAssistant(string assistantId);
  Task DeleteThread(string threadId);

  Task<LlmResult<T>> CallFunction<T>(string systemPrompt, string userPrompt, FunctionDefinition function,
    string? conversationId = null, int? retryCount = null);

  Task<(string, T)> GetRunAction<T>(string threadId, string runId, string functionName);
}

public class LlmService : ILlmService
{
  private static readonly ILogger Log = Serilog.Log.ForContext<LlmService>();
  private readonly OpenAIClient _client;
  private readonly IMapper _mapper;
  private readonly LlmConfig _config;
  private readonly ISessionManager _sessionManager;
  private readonly IScopeContainer _scope;
  private readonly Dictionary<string, List<(string toolCallId, string output)>> _runToolOutputs = new();

  private readonly AsyncRetryPolicy _retryPolicy;

  public LlmService(
    OpenAIClient client,
    IMapper mapper,
    LlmConfig config,
    ISessionManager sessionManager,
    IScopeContainer scope
  )
  {
    _client = client;
    _mapper = mapper;
    _config = config;
    _sessionManager = sessionManager;
    _scope = scope;

    _retryPolicy = Policy
      .Handle<Exception>()
      .WaitAndRetryAsync(
        retryCount: config.RetryAttempts,
        sleepDurationProvider: _ => TimeSpan.FromSeconds(1),
        onRetry: (exception, _, retryCount, context) =>
        {
          Log.Warning(exception,
            "LLM attempt {retryCount}: Retrying due to {exception}. Retry Context: {@Context}",
            retryCount, exception.Message, context);
        }
      );
  }

  private async Task<T> ExecuteWithRetry<T>(int? retryCount, Func<Task<T>> action)
  {
    var policy = Policy
      .Handle<Exception>()
      .WaitAndRetryAsync(
        retryCount: retryCount ?? _config.RetryAttempts,
        sleepDurationProvider: _ => TimeSpan.FromSeconds(1),
        onRetry: (exception, _, currentRetry, context) =>
        {
          Log.Warning(exception,
            "LLM attempt {retryCount}: Retrying due to {exception}. Retry Context: {@Context}",
            currentRetry, exception.Message, context);
        }
      );

    return await policy.ExecuteAsync(action);
  }

  public async Task<string> CreateAssistant(PromptBase prompt)
  {
    // ServiceUnavailableException will be thrown by the proxy if LLM service is unavailable
    ArgumentNullException.ThrowIfNull(_client, nameof(_client));

    try
    {
      var assistantClient = _client.GetAssistantClient();
      var functionToolDefinition = _mapper.Map<FunctionToolDefinition>(prompt.GetFunction());
      functionToolDefinition.StrictParameterSchemaEnabled = true;
      var response = await assistantClient.CreateAssistantAsync(prompt.Model ?? _config.ModelName,
        new AssistantCreationOptions
        {
          Name = prompt.Name,
          Description = prompt.Description,
          Instructions = prompt.SystemPrompt,
          Tools = { functionToolDefinition }
        });
      return response.Value.Id;
    }
    catch (Exception e)
    {
      Log.Error(e, "Error occurred while creating assistant");
      throw;
    }
  }

  public async Task<string> CreateThread()
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    try
    {
      var assistantClient = _client.GetAssistantClient();
      var response = await assistantClient.CreateThreadAsync();
      return response.Value.Id;
    }
    catch (Exception e)
    {
      Log.Error(e, "Error occurred while creating thread");
      throw;
    }
  }

  public async Task CreateMessage(string threadId, string content, MessageRole role = MessageRole.User)
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    try
    {
      Log.Information("Creating message for thread {threadId}, message: {content}", threadId, content);
      var assistantClient = _client.GetAssistantClient();
      await assistantClient.CreateMessageAsync(
        threadId,
        role,
        [MessageContent.FromText(content)]
      );
    }
    catch (Exception e)
    {
      Log.Error(e, "Error occurred while creating message");
      throw;
    }
  }

  public async Task<string> CreateRun(string threadId, string assistantId, bool requiresToolConstraint = true)
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    try
    {
      var assistantClient = _client.GetAssistantClient();
      var result = await assistantClient.CreateRunAsync(threadId, assistantId, new RunCreationOptions
      {
        ToolConstraint = requiresToolConstraint ? ToolConstraint.Required : ToolConstraint.None,
        AllowParallelToolCalls = false
      });

      var threadRun = result.Value;

      return threadRun.Id;
    }
    catch (Exception e)
    {
      Log.Error(e, "Error occurred while creating run for thread {threadId}, assistant {assistantId}", threadId,
        assistantId);
      throw;
    }
  }

  public async Task<(bool isComplete, RunStatus status)> GetRun(string threadId, string runId)
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    try
    {
      var run = await GetThreadRun(threadId, runId);

      // If the run is in a terminal state, clean up the tool outputs
      if (run.Status.IsTerminal)
      {
        _runToolOutputs.Remove(runId);
      }

      return (run.Status.IsTerminal, run.Status);
    }
    catch (Exception e)
    {
      Log.Error(e, "Error occurred while getting run {runId} for thread {threadId}", runId, threadId);
      throw;
    }
  }

  public async Task<string> CancelRun(string threadId, string runId)
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    Log.Information("Cancelling run: {runId} for thread: {threadId}", runId, threadId);
    try
    {
      var assistantClient = _client.GetAssistantClient();

      var run = await GetRun(threadId, runId);
      if (run.isComplete)
      {
        return "Run is already complete.";
      }

      var responseResult = await assistantClient.CancelRunAsync(threadId, runId);

      var response = responseResult.Value
                     ?? throw new Exception("Failed to cancel run. Response was null.");

      return response.Status.ToString()!;
    }
    catch (Exception e)
    {
      Log.Error(e, "Error occurred while cancelling run {runId} for thread {threadId}", runId, threadId);
      return "Failed to cancel run.";
    }
    finally
    {
      // Clean up the tool outputs
      _runToolOutputs.Remove(runId);
    }
  }

  public async Task SubmitToolOutputToRun(string threadId, string runId, string toolCallId, string output)
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    try
    {
      Log.Information(
        "Submitting tool outputs for run {runId}, thread {threadId} for tool call {toolCallId}, Output: {output}",
        runId, threadId, toolCallId, output);

      var run = await GetThreadRun(threadId, runId);
      var requiredToolCallIds = run.RequiredActions.Select(ra => ra.ToolCallId);

      // Retrieve existing tool outputs for this runId
      if (!_runToolOutputs.TryGetValue(runId, out var toolOutputs))
      {
        toolOutputs = [];
        _runToolOutputs[runId] = toolOutputs;
      }

      // Stash the tool call ID and arguments for later, required to submit for additional tool calls
      toolOutputs.Add((toolCallId, output));

      var toolOutputsToSubmit = toolOutputs.Where(to => requiredToolCallIds.Contains(to.toolCallId)).ToList();

      var assistantClient = _client.GetAssistantClient();
      await assistantClient.SubmitToolOutputsToRunAsync(threadId, runId,
        toolOutputsToSubmit.Select(to => new ToolOutput(to.toolCallId, to.output)).ToList());
    }
    catch (Exception e)
    {
      Log.Error(e, "Error occurred while submitting tool outputs for run {runId}, thread {threadId}", runId,
        threadId);
      throw;
    }
  }

  private async Task<ThreadRun> GetThreadRun(string threadId, string runId)
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    try
    {
      var assistantClient = _client.GetAssistantClient();
      var runResult = await assistantClient.GetRunAsync(threadId, runId);
      return runResult.Value;
    }
    catch (Exception e)
    {
      Log.Error(e, "Error occurred while getting run {runId} for thread {threadId}", runId, threadId);
      throw;
    }
  }

  public async Task DeleteAssistant(string assistantId)
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    try
    {
      var assistantClient = _client.GetAssistantClient();
      await assistantClient.DeleteAssistantAsync(assistantId);
    }
    catch (Exception e)
    {
      Log.Error(e, "Error occurred while deleting assistant {assistantId}", assistantId);
    }
  }

  public async Task DeleteThread(string threadId)
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    try
    {
      var assistantClient = _client.GetAssistantClient();
      await assistantClient.DeleteThreadAsync(threadId);
    }
    catch (Exception e)
    {
      Log.Error(e, "Error occurred while deleting thread {threadId}", threadId);
    }
  }

  public async Task<LlmResult<T>> CallFunction<T>(
    string systemPrompt,
    string userPrompt,
    FunctionDefinition function,
    string? conversationId = null,
    int? retryCount = null
  )
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    Log.Information(
      "Getting response from model. System prompt: {systemPrompt}, User prompt: {userPrompt}, Function: {@function}",
      systemPrompt, userPrompt, function);

    var messages = new List<ChatMessage>
    {
      new SystemChatMessage(systemPrompt),
      new UserChatMessage(userPrompt)
    };

    var result = await CallFunction<T>(function, messages, conversationId, retryCount);

    return result;
  }


  private async Task<LlmResult<T>> CallFunction<T>(
    FunctionDefinition function,
    List<ChatMessage> messages,
    string? conversationId = null,
    int? retryCount = null
  )
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    var functionToolDefinition = _mapper.Map<FunctionToolDefinition>(function);
    functionToolDefinition.StrictParameterSchemaEnabled = true;

    return await CallFunction<T>(
      messages,
      ChatTool.CreateFunctionTool(
        functionName: function.Name,
        functionDescription: function.Description,
        functionParameters: functionToolDefinition.Parameters,
        true
      ),
      conversationId,
      retryCount
    );
  }

  private async Task<LlmResult<T>> CallFunction<T>(
    List<ChatMessage> messages,
    ChatTool functionTool,
    string? conversationId = null,
    int? retryCount = null
  )
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    var scopeId = _scope.Id;
    // If no conversation ID is provided, create a new conversation
    conversationId ??= await _sessionManager.InitializeConversation(scopeId, messages, functionTool);

    var allMessages = await GetConversation(conversationId);

    // Add new messages
    allMessages.AddRange(messages);

    var chatCompletion = await GetCompletion(messages, new ChatCompletionOptions
    {
      Tools = { functionTool },
      ToolChoice = ChatToolChoice.CreateFunctionChoice(functionTool.FunctionName),
      AllowParallelToolCalls = false
    }, retryCount);

    if (chatCompletion.FinishReason == ChatFinishReason.ContentFilter)
    {
      throw new OpenAiContentFilterException("Content filter triggered");
    }

    if (chatCompletion.FinishReason != ChatFinishReason.ToolCalls
        && chatCompletion.FinishReason != ChatFinishReason.Stop)
    {
      throw new Exception(
        $"Expected to receive a tool call from the model. Received: '{chatCompletion.FinishReason}'");
    }

    if (chatCompletion.ToolCalls.All(t => t.FunctionName != functionTool.FunctionName))
    {
      throw new Exception(
        $"Expected function name {functionTool.FunctionName} but got {chatCompletion.ToolCalls.First(t => t.FunctionName != null).FunctionName}");
    }

    foreach (var toolCall in chatCompletion.ToolCalls)
    {
      if (toolCall.FunctionName != functionTool.FunctionName)
      {
        Log.Error("Expected function name {functionName} but got {toolCall.FunctionName}",
          functionTool.FunctionName, toolCall.FunctionName);
        continue;
      }

      try
      {
        Log.Information("Function arguments: {FunctionArguments}", toolCall.FunctionArguments);
        using var argumentsJson = JsonDocument.Parse(toolCall.FunctionArguments);

        Log.Debug("Deserializing tool call arguments to type {type}", typeof(T).Name);
        var result = Utils.Deserialize<T>(argumentsJson)
          ?? throw new Exception("Failed to deserialize tool call arguments");

        var prompt = messages.Last(m => m is UserChatMessage).Content[0].Text;
        // Store the message in the session
        await _sessionManager.AddMessage(_scope.Id, conversationId, prompt, chatCompletion, result);

        return new LlmResult<T>
        {
          Data = result,
          ConversationId = conversationId
        };
      }
      catch (Exception e)
      {
        Log.Error(e,
          "Failed to deserialize tool call arguments to type {type}, attempted to deserialize {FunctionArguments}",
          typeof(T).Name, toolCall.FunctionArguments);
        throw;
      }
    }

    throw new Exception("Failed to get a valid response from the model.");
  }

  public async Task<string> GetCompletionContent(
    string userPrompt,
    string? conversationId = null,
    ChatCompletionOptions? options = null,
    int? retryCount = null)
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    var messages = new List<ChatMessage> { new UserChatMessage(userPrompt) };
    var result = await GetCompletionContent(messages, conversationId, options, retryCount);
    return result.Data;
  }

  public async Task<LlmResult<string>> GetCompletionContent(
    List<ChatMessage> messages,
    string? conversationId = null,
    ChatCompletionOptions? options = null,
    int? retryCount = null
  )
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    // If no conversation ID is provided, create a new conversation
    conversationId ??= await _sessionManager.InitializeConversation(_scope.Id, messages);

    var allMessages = await GetConversation(conversationId);

    // Add new messages
    allMessages.AddRange(messages);

    // Get completion from LLM
    var completion = await GetCompletion(allMessages, options, retryCount);

    var prompt = messages.Last(m => m is UserChatMessage).Content[0].Text;
    var response = completion.Content[0].Text;

    // Store the message in the session
    await _sessionManager.AddMessage(_scope.Id, conversationId, prompt, completion);

    return new LlmResult<string>
    {
      Data = response,
      ConversationId = conversationId
    };
  }

  private async Task<List<ChatMessage>> GetConversation(string conversationId)
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    // Get existing conversation messages if any
    var conversation = await _sessionManager.GetConversation(_scope.Id, conversationId);

    // Prepend existing messages if there are any
    var allMessages = GetChatMessages(conversation);

    return allMessages;
  }

  private static List<ChatMessage> GetChatMessages(LlmConversation conversation)
  {
    var allMessages = new List<ChatMessage>();

    // Add system message if it exists
    if (!string.IsNullOrEmpty(conversation.SystemPrompt))
    {
      allMessages.Add(new SystemChatMessage(conversation.SystemPrompt));
    }

    // Add existing conversation messages
    foreach (var msg in conversation.Messages)
    {
      allMessages.Add(new UserChatMessage(msg.Request));
      allMessages.Add(new SystemChatMessage(JsonSerializer.Serialize(msg.Response)));
    }

    return allMessages;
  }

  private async Task<ChatCompletion> GetCompletion(List<ChatMessage> messages, ChatCompletionOptions? options = null,
    int? retryCount = null)
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    return await ExecuteWithRetry(retryCount, async () =>
    {
      var chatClient = _client.GetChatClient(_config.ModelName);

      try
      {
        Log.Information("Sending prompts to model. Messages: {@message}. Options: {@options}", messages, options);

        var result = await chatClient.CompleteChatAsync(messages, options);

        Log.Information("Received response from model: {@result}", result);

        return result;
      }
      catch (Exception e)
      {
        Log.Error(e, "Error occurred while getting response from model");
        throw;
      }
    });
  }

  public async Task<(string, T)> GetRunAction<T>(string threadId, string runId, string functionName)
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    return await _retryPolicy.ExecuteAsync(async () =>
    {
      try
      {
        var run = await GetThreadRun(threadId, runId);

        if (run.Status != RunStatus.RequiresAction)
        {
          throw new InvalidOperationException($"Run status is {run.Status}, expected RequiresAction");
        }

        if (run.RequiredActions.Count > 1)
        {
          Log.Warning("Multiple actions found for run {runId}. Actions:", runId);
          foreach (var toolCall in run.RequiredActions)
          {
            Log.Warning("Tool call ID: {toolCallId}, Function name: {functionName}, Arguments: {arguments}",
              toolCall.ToolCallId, toolCall.FunctionName, toolCall.FunctionArguments);
          }
        }

        var action = run.RequiredActions.FirstOrDefault(a => a.FunctionName == functionName);
        if (action == null)
        {
          throw new InvalidOperationException($"No action found for function {functionName}");
        }

        // Initialize the tool outputs list if it doesn't exist
        if (!_runToolOutputs.TryGetValue(runId, out var value))
        {
          value = [];
          _runToolOutputs[runId] = value;
        }

        return (action.ToolCallId, Utils.Deserialize<T>(action.FunctionArguments));
      }
      catch (Exception e)
      {
        Log.Error(e, "Error occurred while getting run action");
        throw;
      }
    });
  }

  internal async Task<string> GetToolCallId(string threadId, string runId, string functionName)
  {
    if (_client == null)
    {
      throw new ConfigurationMissingException(nameof(LlmConfig), nameof(LlmConfig.ApiKey));
    }

    try
    {
      var run = await GetThreadRun(threadId, runId);

      if (run.Status != RunStatus.RequiresAction)
      {
        throw new InvalidOperationException($"Run status is {run.Status}, expected RequiresAction");
      }

      var action = run.RequiredActions.FirstOrDefault(a => a.FunctionName == functionName);
      if (action == null)
      {
        throw new InvalidOperationException($"No action found for function {functionName}");
      }

      return action.ToolCallId;
    }
    catch (Exception e)
    {
      Log.Error(e, "Error occurred while getting tool call ID");
      throw;
    }
  }
}

internal class OpenAiContentFilterException(string errorMessage) : Exception
{
  public string ErrorMessage { get; } = errorMessage;
}
