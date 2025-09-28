using System.Collections.Concurrent;
using Zarichney.Cookbook.Orders;
using Zarichney.Services.Sessions;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Test data builder for Session entities following established patterns.
/// Enables fluent construction of test Sessions with controlled state.
/// </summary>
public class SessionBuilder : BaseBuilder<SessionBuilder, Session>
{
  public SessionBuilder()
  {
    WithDefaults();
  }

  /// <summary>
  /// Applies sensible default values for a test Session.
  /// </summary>
  /// <returns>The builder for method chaining.</returns>
  public SessionBuilder WithDefaults()
  {
    var now = DateTime.UtcNow;

    // Use reflection to set init-only properties
    var idProperty = typeof(Session).GetProperty(nameof(Session.Id));
    idProperty?.SetValue(Entity, Guid.NewGuid());

    var createdAtProperty = typeof(Session).GetProperty(nameof(Session.CreatedAt));
    createdAtProperty?.SetValue(Entity, now);

    Entity.LastAccessedAt = now;
    Entity.ExpiresAt = now.AddMinutes(15);
    Entity.Duration = TimeSpan.FromMinutes(15);
    Entity.ExpiresImmediately = false;

    return Self();
  }

  /// <summary>
  /// Sets a specific session ID.
  /// </summary>
  public SessionBuilder WithId(Guid id)
  {
    var idProperty = typeof(Session).GetProperty(nameof(Session.Id));
    idProperty?.SetValue(Entity, id);
    return Self();
  }

  /// <summary>
  /// Sets creation timestamp.
  /// </summary>
  public SessionBuilder WithCreatedAt(DateTime createdAt)
  {
    var createdAtProperty = typeof(Session).GetProperty(nameof(Session.CreatedAt));
    createdAtProperty?.SetValue(Entity, createdAt);
    return Self();
  }

  /// <summary>
  /// Sets the session to expire immediately.
  /// </summary>
  public SessionBuilder WithImmediateExpiry()
  {
    Entity.ExpiresImmediately = true;
    Entity.Duration = null;
    return Self();
  }

  /// <summary>
  /// Sets a specific duration for the session.
  /// </summary>
  public SessionBuilder WithDuration(TimeSpan duration)
  {
    Entity.Duration = duration;
    Entity.ExpiresImmediately = false;
    return Self();
  }

  /// <summary>
  /// Associates a user ID with the session.
  /// </summary>
  public SessionBuilder WithUserId(string userId)
  {
    Entity.UserId = userId;
    return Self();
  }

  /// <summary>
  /// Associates an API key with the session.
  /// </summary>
  public SessionBuilder WithApiKey(string apiKey)
  {
    Entity.ApiKeyValue = apiKey;
    return Self();
  }

  /// <summary>
  /// Adds a scope to the session.
  /// </summary>
  public SessionBuilder WithScope(Guid scopeId)
  {
    Entity.Scopes.TryAdd(scopeId, 0);
    return Self();
  }

  /// <summary>
  /// Adds multiple scopes to the session.
  /// </summary>
  public SessionBuilder WithScopes(params Guid[] scopeIds)
  {
    foreach (var scopeId in scopeIds)
    {
      Entity.Scopes.TryAdd(scopeId, 0);
    }
    return Self();
  }

  /// <summary>
  /// Creates an anonymous session (no user ID or API key).
  /// </summary>
  public SessionBuilder Anonymous()
  {
    Entity.UserId = null;
    Entity.ApiKeyValue = null;
    return Self();
  }

  /// <summary>
  /// Sets last accessed timestamp.
  /// </summary>
  public SessionBuilder WithLastAccessedAt(DateTime lastAccessedAt)
  {
    Entity.LastAccessedAt = lastAccessedAt;
    return Self();
  }

  /// <summary>
  /// Sets expiration timestamp.
  /// </summary>
  public SessionBuilder WithExpiresAt(DateTime expiresAt)
  {
    Entity.ExpiresAt = expiresAt;
    return Self();
  }

  /// <summary>
  /// Sets the CookbookOrder associated with this session.
  /// </summary>
  public SessionBuilder WithOrder(CookbookOrder? order)
  {
    // Use reflection to set the internal setter property
    var orderProperty = typeof(Session).GetProperty(nameof(Session.Order));
    orderProperty?.SetValue(Entity, order);
    return Self();
  }
}

/// <summary>
/// Test data builder for SessionConfig entities following established patterns.
/// Enables fluent construction of test SessionConfig with controlled values.
/// </summary>
public class SessionConfigBuilder : BaseBuilder<SessionConfigBuilder, SessionConfig>
{
  public SessionConfigBuilder()
  {
    WithDefaults();
  }

  /// <summary>
  /// Applies standard default values for SessionConfig.
  /// </summary>
  public SessionConfigBuilder WithDefaults()
  {
    Entity.CleanupIntervalMins = 1;
    Entity.DefaultDurationMins = 15;
    Entity.MaxConcurrentCleanup = 10;
    return Self();
  }

  /// <summary>
  /// Sets custom cleanup interval.
  /// </summary>
  public SessionConfigBuilder WithCleanupInterval(int minutes)
  {
    Entity.CleanupIntervalMins = minutes;
    return Self();
  }

  /// <summary>
  /// Sets custom default session duration.
  /// </summary>
  public SessionConfigBuilder WithDefaultDuration(int minutes)
  {
    Entity.DefaultDurationMins = minutes;
    return Self();
  }

  /// <summary>
  /// Sets custom max concurrent cleanup limit.
  /// </summary>
  public SessionConfigBuilder WithMaxConcurrentCleanup(int maxConcurrent)
  {
    Entity.MaxConcurrentCleanup = maxConcurrent;
    return Self();
  }
}
