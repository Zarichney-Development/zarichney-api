using System.Collections.Concurrent;
using OpenAI.Chat;
using Zarichney.Server.Config;
using Zarichney.Server.Cookbook.Customers;
using Zarichney.Server.Cookbook.Orders;
using Zarichney.Server.Services.AI;

namespace Zarichney.Server.Services.Sessions;

/// <summary>
/// Defines the operations that the SessionManager must support,
/// including creating, retrieving, and ending sessions.
/// </summary>
public interface ISessionManager
{
  ConcurrentDictionary<Guid, Session> Sessions { get; }
  Task<Session> CreateSession(Guid scopeId, TimeSpan? duration = null);
  Task EndSession(string orderId);
  Task EndSession(Guid scopeId);
  Task EndSession(Session session);
  Task<Session> GetSession(Guid sessionId);
  Task<Session> GetSessionByScope(Guid scopeId);
  Task<Session> GetSessionByOrder(string orderId, Guid scopeId);
  Task<Session> GetSessionByUserId(string userId, Guid scopeId);
  Task<Session> GetSessionByApiKey(string apiKey, Guid scopeId);
  void AddScopeToSession(Session session, Guid scopeId);
  void RemoveScopeFromSession(Guid scopeId);
  Task AddOrder(Guid scopeId, CookbookOrder order);

  Task AddMessage(Guid scopeId, string conversationId, string prompt, ChatCompletion completion,
    object? toolResponse = null);

  Task<string> InitializeConversation(Guid scopeId, List<ChatMessage> messages, ChatTool? functionTool = null);
  Task<LlmConversation> GetConversation(Guid scopeId, string conversationId);

  Task ParallelForEachAsync<T>(
    IScopeContainer parentScope,
    IEnumerable<T> dataList,
    Func<IScopeContainer, T, CancellationToken, Task> operation,
    int? maxDegreeOfParallelism = null,
    CancellationToken cancellationToken = default
  );
}

/// <summary>
/// Manages all active sessions and provides methods to create, retrieve, and
/// persist session data. Uses ConcurrentDictionary for thread-safe operations.
/// </summary>
public class SessionManager(
  IOrderRepository orderRepository,
  ILlmRepository llmRepository,
  SessionConfig config,
  IScopeFactory scopeFactory,
  ILogger<SessionManager> logger,
  ICustomerRepository customerRepository
) : ISessionManager
{
  private readonly SessionConfig _config = config ?? throw new ArgumentNullException(nameof(config));

  public ConcurrentDictionary<Guid, Session> Sessions { get; } = new();

  public Task<Session> CreateSession(Guid scopeId, TimeSpan? duration = null)
  {
    if (scopeId == Guid.Empty)
    {
      throw new ArgumentException("ScopeId cannot be empty", nameof(scopeId));
    }

    var newSession = new Session
    {
      Id = Guid.NewGuid(),
      ExpiresImmediately = duration == null,
      CreatedAt = DateTime.UtcNow,
      LastAccessedAt = DateTime.UtcNow,
      Duration = duration ?? TimeSpan.FromMinutes(_config.DefaultDurationMins)
    };

    AddScopeToSession(newSession, scopeId);
    RefreshSession(newSession);

    if (!Sessions.TryAdd(newSession.Id, newSession))
    {
      throw new InvalidOperationException($"Failed to add session {newSession.Id}");
    }

    logger.LogInformation("Created new Session {SessionId} for scope {ScopeId}", newSession.Id, scopeId);
    return Task.FromResult(newSession);
  }

  public async Task EndSession(string orderId)
  {
    if (string.IsNullOrEmpty(orderId))
    {
      throw new ArgumentException("OrderId cannot be null or empty", nameof(orderId));
    }

    var session = Sessions.Values.FirstOrDefault(s => s.Order?.OrderId == orderId);
    if (session == null)
    {
      logger.LogWarning("Attempted to end non-existent session for Order {OrderId}", orderId);
      return;
    }

    await EndSession(session);
  }

  public async Task EndSession(Guid scopeId)
  {
    if (scopeId == Guid.Empty)
    {
      throw new ArgumentException("ScopeId cannot be empty", nameof(scopeId));
    }

    var session = Sessions.Values.FirstOrDefault(s => s.Scopes.ContainsKey(scopeId));
    if (session == null)
    {
      logger.LogWarning("Attempted to end non-existent session for Scope {ScopeId}", scopeId);
      return;
    }

    await EndSession(session);
  }

  public async Task EndSession(Session session)
  {
    ArgumentNullException.ThrowIfNull(session);

    // Gather data we need to persist
    var conversationsToWrite = session.Conversations.Values.ToList();
    var orderToWrite = session.Order;

    // Remove session from dictionary
    if (!Sessions.TryRemove(session.Id, out _))
    {
      logger.LogWarning("Failed to remove Session {SessionId}", session.Id);
      return;
    }

    logger.LogInformation("Session {SessionId} ended", session.Id);

    // Persist data
    try
    {
      if (orderToWrite != null)
      {
        // No wait because file service queues background work
        orderRepository.AddUpdateOrderAsync(orderToWrite);
        customerRepository.SaveCustomer(orderToWrite.Customer);
      }

      // Write conversations in parallel
      var tasks = new List<Task>();
      tasks.AddRange(conversationsToWrite.Select(conversation =>
        llmRepository.WriteConversationAsync(conversation, session)));
      // Wait for GitHub service to complete
      await Task.WhenAll(tasks);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error persisting session data for session {SessionId}", session.Id);
      throw; // Rethrow to let caller handle the error
    }
  }

  public Task<Session> GetSession(Guid sessionId)
  {
    if (!Sessions.TryGetValue(sessionId, out var session))
    {
      throw new KeyNotFoundException($"Session {sessionId} not found");
    }

    RefreshSession(session);
    return Task.FromResult(session);
  }

  public async Task<Session> GetSessionByOrder(string orderId, Guid scopeId)
  {
    if (string.IsNullOrEmpty(orderId))
    {
      throw new ArgumentException("OrderId cannot be null or empty", nameof(orderId));
    }

    if (scopeId == Guid.Empty)
    {
      throw new ArgumentException("ScopeId cannot be empty", nameof(scopeId));
    }

    var currentSession = Sessions.Values.FirstOrDefault(s => s.Scopes.ContainsKey(scopeId))
                         ?? throw new NotExpectedException($"Session not found for scope {scopeId}");

    var existingSession = Sessions.Values.FirstOrDefault(s => s.Order?.OrderId == orderId);
    if (existingSession != null)
    {
      // TODO: Auth check
      // if (currentSession.UserId != null && 
      //     existingSession.UserId != null &&
      //     currentSession.UserId != existingSession.UserId)
      // {
      //   throw new KeyNotFoundException(
      //     $"User attempted to access session {existingSession.Id} with order {orderId} but is not authorized");
      // }

      AddScopeToSession(existingSession, scopeId);
      return existingSession;
    }

    var order = await orderRepository.GetOrder(orderId)
                ?? throw new KeyNotFoundException($"Order {orderId} not found");

    var customer = await customerRepository.GetCustomerByEmail(order.Email)
                   ?? throw new KeyNotFoundException($"Customer {order.Email} not found");

    order.Customer = customer;

    existingSession = Sessions.Values.FirstOrDefault(s => s.Scopes.ContainsKey(scopeId));
    if (existingSession != null)
    {
      AddScopeToSession(existingSession, scopeId);
      existingSession.Order = order;
      return existingSession;
    }

    var newSession = await CreateSession(scopeId);
    newSession.Order = order;

    return newSession;
  }

  public async Task<Session> GetSessionByScope(Guid scopeId)
  {
    if (scopeId == Guid.Empty)
    {
      throw new ArgumentException("ScopeId cannot be empty", nameof(scopeId));
    }

    var existingSession = Sessions.Values.FirstOrDefault(s => s.Scopes.ContainsKey(scopeId));
    if (existingSession != null)
    {
      RefreshSession(existingSession);
      return existingSession;
    }

    return await CreateSession(scopeId);
  }

  public async Task<Session> GetSessionByUserId(string userId, Guid scopeId)
  {
    if (string.IsNullOrEmpty(userId))
    {
      throw new ArgumentException("UserId cannot be null or empty", nameof(userId));
    }

    var existingSession = Sessions.Values.FirstOrDefault(s => s.UserId == userId);
    if (existingSession != null)
    {
      RefreshSession(existingSession);
      return existingSession;
    }

    var newSession = await CreateSession(scopeId);
    newSession.UserId = userId;
    return newSession;
  }

  public async Task<Session> GetSessionByApiKey(string apiKey, Guid scopeId)
  {
    if (string.IsNullOrEmpty(apiKey))
    {
      throw new ArgumentException("ApiKey cannot be null or empty", nameof(apiKey));
    }

    var existingSession = Sessions.Values.FirstOrDefault(s => s.ApiKeyValue == apiKey);
    if (existingSession != null)
    {
      RefreshSession(existingSession);
      return existingSession;
    }

    var newSession = await CreateSession(scopeId);
    newSession.ApiKeyValue = apiKey;
    return newSession;
  }

  public void AddScopeToSession(Session session, Guid scopeId)
  {
    ArgumentNullException.ThrowIfNull(session);

    if (scopeId == Guid.Empty)
    {
      throw new ArgumentException("ScopeId cannot be empty", nameof(scopeId));
    }

    if (session.Scopes.TryAdd(scopeId, 0))
    {
      logger.LogInformation("Scope {ScopeId} added to Session {SessionId}", scopeId, session.Id);
    }

    RefreshSession(session);
  }

  public void RemoveScopeFromSession(Guid scopeId)
  {
    if (scopeId == Guid.Empty)
    {
      throw new ArgumentException("ScopeId cannot be empty", nameof(scopeId));
    }

    var session = Sessions.Values.FirstOrDefault(s => s.Scopes.ContainsKey(scopeId));
    if (session == null)
    {
      logger.LogWarning("Attempted to remove ScopeId {ScopeId} from a non-existent session", scopeId);
      return;
    }

    if (session.Scopes.TryRemove(scopeId, out _))
    {
      logger.LogInformation("Scope {ScopeId} removed from Session {SessionId}", scopeId, session.Id);
    }
    else
    {
      logger.LogWarning("Failed to remove ScopeId {ScopeId} from Session {SessionId}",
        scopeId, session.Id);
    }
  }

  public Task AddOrder(Guid scopeId, CookbookOrder order)
  {
    ArgumentNullException.ThrowIfNull(order);
    if (scopeId == Guid.Empty)
    {
      throw new ArgumentException("ScopeId cannot be empty", nameof(scopeId));
    }

    var session = Sessions.Values.FirstOrDefault(s => s.Scopes.ContainsKey(scopeId))
                  ?? throw new KeyNotFoundException("Session not found for scope");

    session.Order = order;
    return Task.CompletedTask;
  }

  public Task AddMessage(
    Guid scopeId,
    string conversationId,
    string prompt,
    ChatCompletion completion,
    object? toolResponse = null)
  {
    ArgumentNullException.ThrowIfNull(completion);
    if (scopeId == Guid.Empty)
    {
      throw new ArgumentException("ScopeId cannot be empty", nameof(scopeId));
    }

    if (string.IsNullOrEmpty(conversationId))
    {
      throw new ArgumentException("ConversationId cannot be null or empty",
        nameof(conversationId));
    }

    if (string.IsNullOrEmpty(prompt))
    {
      throw new ArgumentException("Prompt cannot be null or empty", nameof(prompt));
    }


    var session = Sessions.Values.FirstOrDefault(s => s.Scopes.ContainsKey(scopeId))
                  ?? throw new KeyNotFoundException("Session not found for scope");

    if (!session.Conversations.TryGetValue(conversationId, out var conversation))
    {
      throw new KeyNotFoundException($"Conversation {conversationId} not found in Session {session.Id}");
    }

    var response = toolResponse ?? completion.Content[0].Text;

    var message = new LlmMessage
    {
      Request = prompt,
      Response = response,
      Timestamp = DateTime.UtcNow,
      ChatCompletion = completion
    };

    conversation.Messages.Add(message);
    return Task.CompletedTask;
  }

  public async Task<string> InitializeConversation(Guid scopeId, List<ChatMessage> messages,
    ChatTool? functionTool = null)
  {
    if (scopeId == Guid.Empty)
    {
      throw new ArgumentException("ScopeId cannot be empty", nameof(scopeId));
    }

    var session = Sessions.Values.FirstOrDefault(s => s.Scopes.ContainsKey(scopeId))
                  ?? await CreateSession(scopeId);

    var systemMessage = messages.FirstOrDefault(m => m is SystemChatMessage);
    var systemPrompt = systemMessage?.Content[0]?.Text ?? string.Empty;

    var promptCatalogName = functionTool?.FunctionName;

    var conversationId = $"{promptCatalogName + "-"}{DateTime.UtcNow:yyyyMMdd-HHmmss.fff}".ToLower();
    var conversation = new LlmConversation
    {
      Id = conversationId,
      PromptCatalogName = promptCatalogName,
      SystemPrompt = systemPrompt
    };

    session.Conversations.TryAdd(conversationId, conversation);
    return conversationId;
  }

  public async Task<LlmConversation> GetConversation(Guid scopeId, string conversationId)
  {
    if (scopeId == Guid.Empty)
    {
      throw new ArgumentException("ScopeId cannot be empty", nameof(scopeId));
    }

    if (string.IsNullOrEmpty(conversationId))
    {
      throw new ArgumentException("ConversationId cannot be null or empty",
        nameof(conversationId));
    }

    var session = await GetSessionByScope(scopeId);

    if (!session.Conversations.TryGetValue(conversationId, out var conversation))
    {
      throw new KeyNotFoundException(
        $"Conversation {conversationId} not found in session {session.Id}");
    }

    return conversation;
  }

  private void RefreshSession(Session session)
  {
    ArgumentNullException.ThrowIfNull(session);

    session.LastAccessedAt = DateTime.UtcNow;
    var duration = session.Duration ?? TimeSpan.FromMinutes(_config.DefaultDurationMins);
    session.ExpiresAt = session.LastAccessedAt + duration;
  }

  public async Task ParallelForEachAsync<T>(
    IScopeContainer parentScope,
    IEnumerable<T> dataList,
    Func<IScopeContainer, T, CancellationToken, Task> operation,
    int? maxDegreeOfParallelism = null,
    CancellationToken cancellationToken = default)
  {
    try
    {
      await Parallel.ForEachAsync(
        dataList,
        new ParallelOptions
        {
          MaxDegreeOfParallelism = maxDegreeOfParallelism ?? Environment.ProcessorCount,
          CancellationToken = cancellationToken
        },
        async (item, ct) =>
        {
          var scope = scopeFactory.CreateScope(parentScope);

          await operation(scope, item, ct);
        });
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      throw;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error in parallel operation");
      throw;
    }
  }
}