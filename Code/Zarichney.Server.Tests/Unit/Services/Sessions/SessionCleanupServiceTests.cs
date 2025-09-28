using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Cookbook.Orders;
using Zarichney.Tests.TestData.Builders;
using Zarichney.Services.Sessions;

namespace Zarichney.Tests.Unit.Services.Sessions;

/// <summary>
/// Comprehensive unit tests for SessionCleanupService covering background service functionality,
/// session cleanup logic, error handling, and resource management.
/// Tests follow established patterns with complete isolation, proper mocking, and framework-first approach.
/// </summary>
[Trait("Category", "Unit")]
public class SessionCleanupServiceTests
{
  private readonly Mock<ISessionManager> _mockSessionManager;
  private readonly Mock<ILogger<SessionCleanupService>> _mockLogger;
  private readonly SessionConfig _sessionConfig;
  private readonly ConcurrentDictionary<Guid, Session> _sessions;

  public SessionCleanupServiceTests()
  {
    _mockSessionManager = new Mock<ISessionManager>();
    _mockLogger = new Mock<ILogger<SessionCleanupService>>();
    _sessions = new ConcurrentDictionary<Guid, Session>();

    _sessionConfig = new SessionConfigBuilder()
        .WithCleanupInterval(1)
        .WithMaxConcurrentCleanup(10)
        .Build();

    _mockSessionManager.Setup(x => x.Sessions).Returns(_sessions);
  }

  #region Constructor Tests

  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_NullSessionManager_ThrowsArgumentNullException()
  {
    // Arrange
    ISessionManager? nullManager = null;

    // Act
    Action act = () => new SessionCleanupService(nullManager!, _sessionConfig, _mockLogger.Object);

    // Assert
    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("sessionManager");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_NullConfig_ThrowsArgumentNullException()
  {
    // Arrange
    SessionConfig? nullConfig = null;

    // Act
    Action act = () => new SessionCleanupService(_mockSessionManager.Object, nullConfig!, _mockLogger.Object);

    // Assert
    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("config");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_NullLogger_ThrowsArgumentNullException()
  {
    // Arrange
    ILogger<SessionCleanupService>? nullLogger = null;

    // Act
    Action act = () => new SessionCleanupService(_mockSessionManager.Object, _sessionConfig, nullLogger!);

    // Assert
    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("logger");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_ValidParameters_CreatesServiceSuccessfully()
  {
    // Act
    var service = new SessionCleanupService(_mockSessionManager.Object, _sessionConfig, _mockLogger.Object);

    // Assert
    service.Should().NotBeNull();
  }

  #endregion

  #region ExecuteAsync Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ExecuteAsync_NoExpiredSessions_DoesNotCallEndSession()
  {
    // Arrange
    var futureSession = new SessionBuilder()
        .WithExpiresAt(DateTime.UtcNow.AddMinutes(10))
        .Build();
    _sessions.TryAdd(futureSession.Id, futureSession);

    using var cts = new CancellationTokenSource();
    var service = new SessionCleanupService(_mockSessionManager.Object, _sessionConfig, _mockLogger.Object);

    // Act
    var executeTask = ExecuteServiceAsync(service, cts.Token);
    await Task.Delay(50); // Give service time to run one cleanup cycle
    cts.Cancel();
    await executeTask;

    // Assert
    _mockSessionManager.Verify(x => x.EndSession(It.IsAny<string>()), Times.Never,
        "should not end any sessions when none are expired");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ExecuteAsync_ExpiredSessionWithOrder_CallsEndSessionWithOrderId()
  {
    // Arrange
    var expiredSession = new SessionBuilder()
        .WithExpiresAt(DateTime.UtcNow.AddMinutes(-5))
        .Build();
    expiredSession.Order = new CookbookOrder { OrderId = "TEST-ORDER-123" };
    _sessions.TryAdd(expiredSession.Id, expiredSession);

    using var cts = new CancellationTokenSource();
    var service = new SessionCleanupService(_mockSessionManager.Object, _sessionConfig, _mockLogger.Object);

    // Act
    var executeTask = ExecuteServiceAsync(service, cts.Token);
    await Task.Delay(100); // Give service time to run cleanup
    cts.Cancel();
    await executeTask;

    // Assert
    _mockSessionManager.Verify(x => x.EndSession("TEST-ORDER-123"), Times.Once,
        "should end session using order ID when session has an associated order");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ExecuteAsync_ExpiredSessionWithoutOrder_RemovesFromCollection()
  {
    // Arrange
    var expiredSession = new SessionBuilder()
        .WithExpiresAt(DateTime.UtcNow.AddMinutes(-5))
        .Anonymous()
        .Build();
    _sessions.TryAdd(expiredSession.Id, expiredSession);

    using var cts = new CancellationTokenSource();
    var service = new SessionCleanupService(_mockSessionManager.Object, _sessionConfig, _mockLogger.Object);

    // Act
    var executeTask = ExecuteServiceAsync(service, cts.Token);
    await Task.Delay(100); // Give service time to run cleanup
    cts.Cancel();
    await executeTask;

    // Assert
    _sessions.Should().BeEmpty(
        "expired session without order should be removed from the collection");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ExecuteAsync_MultipleExpiredSessions_ProcessesAllConcurrently()
  {
    // Arrange
    var expiredSessions = Enumerable.Range(1, 5)
        .Select(_ => new SessionBuilder()
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(-10))
            .Anonymous()
            .Build())
        .ToList();

    foreach (var session in expiredSessions)
    {
      _sessions.TryAdd(session.Id, session);
    }

    using var cts = new CancellationTokenSource();
    var service = new SessionCleanupService(_mockSessionManager.Object, _sessionConfig, _mockLogger.Object);

    // Act
    var executeTask = ExecuteServiceAsync(service, cts.Token);
    await Task.Delay(150); // Give service time to process all sessions
    cts.Cancel();
    await executeTask;

    // Assert
    _sessions.Should().BeEmpty(
        "all expired sessions should be removed");
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Found 5 expired sessions")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "should log the number of expired sessions found");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ExecuteAsync_CancellationRequested_StopsGracefully()
  {
    // Arrange
    using var cts = new CancellationTokenSource();
    var service = new SessionCleanupService(_mockSessionManager.Object, _sessionConfig, _mockLogger.Object);

    // Act
    var executeTask = ExecuteServiceAsync(service, cts.Token);
    cts.Cancel();
    await executeTask;

    // Assert
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Session cleanup service stopped")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.AtLeastOnce,
        "should log service stopped message");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ExecuteAsync_CleanupThrowsException_LogsErrorAndContinues()
  {
    // Arrange
    var expiredSession = new SessionBuilder()
        .WithExpiresAt(DateTime.UtcNow.AddMinutes(-5))
        .Build();
    expiredSession.Order = new CookbookOrder { OrderId = "ERROR-ORDER" };
    _sessions.TryAdd(expiredSession.Id, expiredSession);

    _mockSessionManager.Setup(x => x.EndSession("ERROR-ORDER"))
        .ThrowsAsync(new InvalidOperationException("Test exception"));

    using var cts = new CancellationTokenSource();
    var service = new SessionCleanupService(_mockSessionManager.Object, _sessionConfig, _mockLogger.Object);

    // Act
    var executeTask = ExecuteServiceAsync(service, cts.Token);
    await Task.Delay(100); // Give service time to encounter error
    cts.Cancel();
    await executeTask;

    // Assert
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error cleaning up session")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "should log error when session cleanup fails");
  }

  #endregion

  #region Concurrent Cleanup Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CleanupExpiredSessions_RespectsMaxConcurrentCleanup()
  {
    // Arrange
    var config = new SessionConfigBuilder()
        .WithCleanupInterval(1)
        .WithMaxConcurrentCleanup(2) // Limit to 2 concurrent cleanups
        .Build();

    var concurrentCleanups = 0;
    var maxConcurrentObserved = 0;
    var cleanupLock = new SemaphoreSlim(1);

    // Create many expired sessions
    var expiredSessions = Enumerable.Range(1, 10)
        .Select(_ => new SessionBuilder()
            .WithExpiresAt(DateTime.UtcNow.AddMinutes(-5))
            .Build())
        .ToList();

    foreach (var session in expiredSessions)
    {
      session.Order = new CookbookOrder { OrderId = $"ORDER-{session.Id}" };
      _sessions.TryAdd(session.Id, session);
    }

    // Track concurrent executions
    _mockSessionManager.Setup(x => x.EndSession(It.IsAny<string>()))
        .Returns(async (string orderId) =>
        {
          await cleanupLock.WaitAsync();
          try
          {
            concurrentCleanups++;
            maxConcurrentObserved = Math.Max(maxConcurrentObserved, concurrentCleanups);
          }
          finally
          {
            cleanupLock.Release();
          }

          await Task.Delay(50); // Simulate work

          await cleanupLock.WaitAsync();
          try
          {
            concurrentCleanups--;
          }
          finally
          {
            cleanupLock.Release();
          }
        });

    using var cts = new CancellationTokenSource();
    var service = new SessionCleanupService(_mockSessionManager.Object, config, _mockLogger.Object);

    // Act
    var executeTask = ExecuteServiceAsync(service, cts.Token);
    await Task.Delay(500); // Give enough time for cleanup
    cts.Cancel();
    await executeTask;

    // Assert
    maxConcurrentObserved.Should().BeLessThanOrEqualTo(2,
        "concurrent cleanups should respect the configured maximum");
  }

  #endregion

  #region StopAsync and Dispose Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task StopAsync_LogsStoppingAndStoppedMessages()
  {
    // Arrange
    var service = new SessionCleanupService(_mockSessionManager.Object, _sessionConfig, _mockLogger.Object);
    using var cts = new CancellationTokenSource();

    // Act
    await service.StopAsync(cts.Token);

    // Assert
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Session cleanup service is stopping")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "should log stopping message");

    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Session cleanup service stopped")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "should log stopped message");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Dispose_DisposesResourcesProperly()
  {
    // Arrange
    var service = new SessionCleanupService(_mockSessionManager.Object, _sessionConfig, _mockLogger.Object);

    // Act
    Action act = () => service.Dispose();

    // Assert
    act.Should().NotThrow(
        "dispose should complete without throwing exceptions");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Dispose_MultipleCallsDoNotThrow()
  {
    // Arrange
    var service = new SessionCleanupService(_mockSessionManager.Object, _sessionConfig, _mockLogger.Object);

    // Act
    Action act = () =>
    {
      service.Dispose();
      service.Dispose(); // Second call
    };

    // Assert
    act.Should().NotThrow(
        "multiple dispose calls should be safe");
  }

  #endregion

  #region Edge Cases and Error Scenarios

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ExecuteAsync_SessionExpiresWhileProcessing_HandlesGracefully()
  {
    // Arrange
    var session1 = new SessionBuilder()
        .WithExpiresAt(DateTime.UtcNow.AddMilliseconds(75))
        .Build();
    var session2 = new SessionBuilder()
        .WithExpiresAt(DateTime.UtcNow.AddMinutes(-5))
        .Build();

    session1.Order = new CookbookOrder { OrderId = "FUTURE-ORDER" };
    session2.Order = new CookbookOrder { OrderId = "EXPIRED-ORDER" };

    _sessions.TryAdd(session1.Id, session1);
    _sessions.TryAdd(session2.Id, session2);

    using var cts = new CancellationTokenSource();
    var service = new SessionCleanupService(_mockSessionManager.Object, _sessionConfig, _mockLogger.Object);

    // Act
    var executeTask = ExecuteServiceAsync(service, cts.Token);
    await Task.Delay(150); // Wait for session1 to expire and get processed
    cts.Cancel();
    await executeTask;

    // Assert
    _mockSessionManager.Verify(x => x.EndSession("EXPIRED-ORDER"), Times.AtLeastOnce,
        "should process initially expired session");
    // Session1 may or may not be processed depending on timing
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ExecuteAsync_SessionRemovedDuringCleanup_HandlesGracefully()
  {
    // Arrange
    var session = new SessionBuilder()
        .WithExpiresAt(DateTime.UtcNow.AddMinutes(-5))
        .Anonymous()
        .Build();
    _sessions.TryAdd(session.Id, session);

    // Simulate session being removed by another process
    _mockSessionManager.Setup(x => x.Sessions)
        .Returns(() =>
        {
          // Remove session after first access
          _sessions.TryRemove(session.Id, out _);
          return _sessions;
        });

    using var cts = new CancellationTokenSource();
    var service = new SessionCleanupService(_mockSessionManager.Object, _sessionConfig, _mockLogger.Object);

    // Act
    var executeTask = ExecuteServiceAsync(service, cts.Token);
    await Task.Delay(100);
    cts.Cancel();
    await executeTask;

    // Assert - should not throw and handle gracefully
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Never,
        "should not log errors for sessions removed by other processes");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task ExecuteAsync_GeneralExceptionDuringCleanup_LogsAndContinues()
  {
    // Arrange
    var mockSessionManager = new Mock<ISessionManager>();
    var problematicSessions = new ConcurrentDictionary<Guid, Session>();

    // Setup to throw exception when accessing Sessions property
    var accessCount = 0;
    mockSessionManager.Setup(x => x.Sessions)
        .Returns(() =>
        {
          accessCount++;
          if (accessCount == 1)
          {
            throw new InvalidOperationException("Database connection lost");
          }
          return problematicSessions;
        });

    using var cts = new CancellationTokenSource();
    var service = new SessionCleanupService(mockSessionManager.Object, _sessionConfig, _mockLogger.Object);

    // Act
    var executeTask = ExecuteServiceAsync(service, cts.Token);
    await Task.Delay(100);
    cts.Cancel();
    await executeTask;

    // Assert
    _mockLogger.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error occurred during session cleanup")),
            It.IsAny<InvalidOperationException>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "should log general errors during cleanup");
  }

  #endregion

  #region Helper Methods

  private static async Task ExecuteServiceAsync(SessionCleanupService service, CancellationToken cancellationToken)
  {
    try
    {
      await service.StartAsync(cancellationToken);
      // ExecuteAsync runs in background, so we just wait for cancellation
      await Task.Delay(Timeout.Infinite, cancellationToken);
    }
    catch (OperationCanceledException)
    {
      // Expected when cancellation is requested
    }
    finally
    {
      await service.StopAsync(CancellationToken.None);
    }
  }

  #endregion
}
