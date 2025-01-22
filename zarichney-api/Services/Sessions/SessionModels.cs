using System.Collections.Concurrent;
using Zarichney.Config;
using Zarichney.Cookbook.Orders;

namespace Zarichney.Services.Sessions;

/// <summary>
/// Represents a user's session, which may contain:
/// - An Order object (if related to an order)
/// - A set of Scopes (identified by scopeId) that reference this session
/// - A set of conversation histories (LlmConversations)
/// - Timestamps for creation, last access, expiration, etc.
/// </summary>
public class Session
{
  /// <summary>
  /// Unique identifier for this session
  /// </summary>
  public Guid Id { get; init; }

  /// <summary>
  /// Tracks any number of scopeIds (Guid) that currently reference this session.
  /// The byte value is unused; we simply leverage ConcurrentDictionary for thread-safety.
  /// </summary>
  public ConcurrentDictionary<Guid, byte> Scopes { get; } = new();

  /// <summary>
  /// Optional user ID associated with this session
  /// </summary>
  public string? UserId { get; set; }

  /// <summary>
  /// UTC timestamp when this session was initially created
  /// </summary>
  public DateTime CreatedAt { get; init; }

  /// <summary>
  /// UTC timestamp when this session was last accessed or refreshed
  /// </summary>
  public DateTime LastAccessedAt { get; set; }

  /// <summary>
  /// Overall session duration or timespan. If null, it defaults to a configured fallback.
  /// </summary>
  public TimeSpan? Duration { get; set; }

  /// <summary>
  /// UTC timestamp when this session will expire
  /// </summary>
  public DateTime ExpiresAt { get; set; }

  /// <summary>
  /// If true, the session will effectively expire as soon as all scopes are removed
  /// </summary>
  public bool ExpiresImmediately { get; set; }

  /// <summary>
  /// Optional CookbookOrder object associated with this session
  /// </summary>
  public CookbookOrder? Order { get; internal set; }

  /// <summary>
  /// Set of LlmConversations in this session. The key is the conversationId.
  /// </summary>
  public ConcurrentDictionary<string, LlmConversation> Conversations { get; init; } = new();
}

/// <summary>
/// Configuration settings for session management and cleanup
/// </summary>
public class SessionConfig : IConfig
{
  /// <summary>
  /// Interval (in minutes) between consecutive cleanup scans
  /// </summary>
  public int CleanupIntervalMins { get; set; } = 1;

  /// <summary>
  /// Default time (in minutes) before a newly-created session expires
  /// </summary>
  public int DefaultDurationMins { get; set; } = 15;

  /// <summary>
  /// Maximum concurrency allowed for cleanup tasks
  /// </summary>
  public int MaxConcurrentCleanup { get; set; } = 10;
}

/// <summary>
/// Defines a container for managing scoped services and session data
/// </summary>
public interface IScopeContainer
{
  public IServiceProvider ServiceProvider { get; }

  /// <summary>
  /// Unique identifier for this scope container
  /// </summary>
  Guid Id { get; set; }

  /// <summary>
  /// Optional session ID associated with this scope
  /// </summary>
  Guid? SessionId { get; set; }

  /// <summary>
  /// Gets a scoped service of the specified type
  /// </summary>
  T GetService<T>() where T : notnull;
}

/// <summary>
/// Container for managing scoped services and session data.
/// Registered with scoped lifetime to persist the same Id across a scope.
/// </summary>
public class ScopeContainer(
  IServiceProvider serviceProvider,
  IScopeContainer? parentScope = null
)
  : IScopeContainer
{
  public IServiceProvider ServiceProvider { get; } = serviceProvider;
  public Guid Id { get; set; } = parentScope?.Id ?? Guid.NewGuid();
  public Guid? SessionId { get; set; } = parentScope?.SessionId;

  public T GetService<T>() where T : notnull
  {
    return ServiceProvider.GetRequiredService<T>();
  }
}

public static class ScopeHolder
{
  private static readonly AsyncLocal<IScopeContainer?> Scope = new();

  public static IScopeContainer? CurrentScope
  {
    get => Scope.Value;
    set => Scope.Value = value;
  }
}

/// <summary>
/// Factory for creating scope containers
/// </summary>
public interface IScopeFactory
{
  IScopeContainer CreateScope(IScopeContainer? parentScope = null);
}

/// <summary>
/// Implementation of IScopeFactory that creates new scope containers
/// </summary>
public class ScopeFactory(IServiceProvider serviceProvider) : IScopeFactory
{
  public IScopeContainer CreateScope(IScopeContainer? parentScope = null)
  {
    var newScope = (parentScope?.ServiceProvider ?? serviceProvider).CreateScope();
    var scope = new ScopeContainer(newScope.ServiceProvider, parentScope);
    return scope;
  }
}