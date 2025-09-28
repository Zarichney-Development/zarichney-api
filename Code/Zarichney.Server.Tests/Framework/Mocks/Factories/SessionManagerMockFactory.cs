using Moq;
using Zarichney.Services.Sessions;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Tests.Framework.Mocks.Factories;

/// <summary>
/// Factory for creating mock ISessionManager instances with common setups.
/// Follows established patterns for framework test infrastructure.
/// </summary>
public static class SessionManagerMockFactory
{
  /// <summary>
  /// Creates a default mock ISessionManager with basic behavior.
  /// </summary>
  public static Mock<ISessionManager> CreateDefault()
  {
    var mock = new Mock<ISessionManager>();

    // Setup default GetSessionByUserId to return a new session
    mock.Setup(x => x.GetSessionByUserId(It.IsAny<string>(), It.IsAny<Guid>()))
        .ReturnsAsync((string userId, Guid scopeId) =>
        {
          var builder = new SessionBuilder();
          builder.WithUserId(userId);
          builder.WithScope(scopeId);
          return builder.Build();
        });

    // Setup default CreateSession to return a new anonymous session
    mock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
        .ReturnsAsync((Guid scopeId, TimeSpan? duration) =>
        {
          var builder = new SessionBuilder();
          builder.Anonymous();
          builder.WithScope(scopeId);
          return builder.Build();
        });

    // Setup AddScopeToSession and RemoveScopeFromSession as void operations
    mock.Setup(x => x.AddScopeToSession(It.IsAny<Session>(), It.IsAny<Guid>()))
        .Callback((Session session, Guid scopeId) =>
        {
          session.Scopes.TryAdd(scopeId, 0);
        });

    mock.Setup(x => x.RemoveScopeFromSession(It.IsAny<Guid>(), It.IsAny<Session?>()))
        .Callback((Guid scopeId, Session? session) => { /* No-op for default */ });

    // Setup EndSession to complete successfully by default
    mock.Setup(x => x.EndSession(It.IsAny<Session>()))
        .Returns(Task.CompletedTask);

    return mock;
  }

  /// <summary>
  /// Creates a mock that returns a specific session for any user ID.
  /// </summary>
  public static Mock<ISessionManager> CreateWithSession(Session session)
  {
    var mock = CreateDefault();

    mock.Setup(x => x.GetSessionByUserId(It.IsAny<string>(), It.IsAny<Guid>()))
        .ReturnsAsync(session);

    mock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
        .ReturnsAsync(session);

    return mock;
  }

  /// <summary>
  /// Creates a mock that returns sessions with specific user authentication state.
  /// </summary>
  public static Mock<ISessionManager> CreateForAuthenticatedUser(string userId, string? apiKey = null)
  {
    var mock = CreateDefault();

    mock.Setup(x => x.GetSessionByUserId(userId, It.IsAny<Guid>()))
        .ReturnsAsync((string uid, Guid scopeId) =>
        {
          var builder = new SessionBuilder();
          builder.WithUserId(uid);
          builder.WithScope(scopeId);

          if (apiKey != null)
          {
            builder.WithApiKey(apiKey);
          }

          return builder.Build();
        });

    return mock;
  }

  /// <summary>
  /// Creates a mock that always creates anonymous sessions.
  /// </summary>
  public static Mock<ISessionManager> CreateAnonymousOnly()
  {
    var mock = CreateDefault();

    // Override to always return anonymous sessions
    mock.Setup(x => x.GetSessionByUserId(It.IsAny<string>(), It.IsAny<Guid>()))
        .ReturnsAsync((string userId, Guid scopeId) =>
        {
          var builder = new SessionBuilder();
          builder.Anonymous();
          builder.WithScope(scopeId);
          return builder.Build();
        });

    return mock;
  }

  /// <summary>
  /// Creates a mock that simulates session expiry behavior.
  /// </summary>
  public static Mock<ISessionManager> CreateWithExpiredSession()
  {
    var mock = CreateDefault();

    var sessionBuilder = new SessionBuilder();
    sessionBuilder.WithExpiresAt(DateTime.UtcNow.AddMinutes(-5));
    var expiredSession = sessionBuilder.Build();

    mock.Setup(x => x.GetSessionByUserId(It.IsAny<string>(), It.IsAny<Guid>()))
        .ReturnsAsync(expiredSession);

    mock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
        .ReturnsAsync(expiredSession);

    return mock;
  }

  /// <summary>
  /// Creates a mock that throws exceptions on EndSession calls.
  /// </summary>
  public static Mock<ISessionManager> CreateWithFailingEndSession()
  {
    var mock = CreateDefault();

    mock.Setup(x => x.EndSession(It.IsAny<Session>()))
        .ThrowsAsync(new InvalidOperationException("Failed to end session"));

    return mock;
  }

  /// <summary>
  /// Creates a mock with immediate expiry sessions.
  /// </summary>
  public static Mock<ISessionManager> CreateWithImmediateExpiry()
  {
    var mock = CreateDefault();

    var sessionBuilder = new SessionBuilder();
    sessionBuilder.WithImmediateExpiry();
    var immediateExpirySession = sessionBuilder.Build();

    mock.Setup(x => x.GetSessionByUserId(It.IsAny<string>(), It.IsAny<Guid>()))
        .ReturnsAsync(immediateExpirySession);

    mock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
        .ReturnsAsync(immediateExpirySession);

    return mock;
  }
}
