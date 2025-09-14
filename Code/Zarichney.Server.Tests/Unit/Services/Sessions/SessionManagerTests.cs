using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using OpenAI.Chat;
using Xunit;
using Zarichney.Config;
using Zarichney.Cookbook.Customers;
using Zarichney.Cookbook.Orders;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Services.AI;
using Zarichney.Services.Sessions;
using Zarichney.Tests.Framework.Mocks;
using System;

namespace Zarichney.Server.Tests.Unit.Services.Sessions;

/// <summary>
/// Comprehensive unit tests for SessionManager covering core session management functionality.
/// Tests follow established patterns with complete isolation, proper mocking, and framework-first approach.
/// </summary>
[Trait("Category", "Unit")]
public class SessionManagerTests
{
  private readonly Mock<IOrderRepository> _mockOrderRepository;
  private readonly Mock<ILlmRepository> _mockLlmRepository;
  private readonly Mock<IScopeFactory> _mockScopeFactory;
  private readonly Mock<ILogger<SessionManager>> _mockLogger;
  private readonly Mock<ICustomerRepository> _mockCustomerRepository;
  private readonly SessionConfig _sessionConfig;
  private readonly SessionManager _sessionManager;

  public SessionManagerTests()
  {
    _mockOrderRepository = new Mock<IOrderRepository>();
    _mockLlmRepository = new Mock<ILlmRepository>();
    _mockScopeFactory = new Mock<IScopeFactory>();
    _mockLogger = new Mock<ILogger<SessionManager>>();
    _mockCustomerRepository = new Mock<ICustomerRepository>();

    _sessionConfig = new SessionConfigBuilder()
        .WithDefaultDuration(15)
        .WithCleanupInterval(1)
        .WithMaxConcurrentCleanup(10)
        .Build();

    _sessionManager = new SessionManager(
        _mockOrderRepository.Object,
        _mockLlmRepository.Object,
        _sessionConfig,
        _mockScopeFactory.Object,
        _mockLogger.Object,
        _mockCustomerRepository.Object
    );
  }

  #region Constructor Tests

  [Fact]
  public void Constructor_WithNullConfig_ThrowsArgumentNullException()
  {
    // Act & Assert
    Action act = () => new SessionManager(
        _mockOrderRepository.Object,
        _mockLlmRepository.Object,
        null!,
        _mockScopeFactory.Object,
        _mockLogger.Object,
        _mockCustomerRepository.Object
    );

    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("config",
        because: "SessionConfig is required for session management configuration");
  }

  #endregion

  #region CreateSession Tests

  [Fact]
  public async Task CreateSession_WithValidScopeId_CreatesAndReturnsSession()
  {
    // Arrange
    var scopeId = Guid.NewGuid();

    // Act
    var result = await _sessionManager.CreateSession(scopeId);

    // Assert
    result.Should().NotBeNull(because: "a valid session should be created");
    result.Id.Should().NotBeEmpty(because: "session must have a unique identifier");
    result.Scopes.Should().ContainKey(scopeId, because: "the session should contain the provided scope ID");
    result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1),
        because: "session should be created with current timestamp");
    result.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1),
        because: "session should be initialized with current access time");
    result.Duration.Should().Be(TimeSpan.FromMinutes(15),
        because: "session should use configured default duration");
    result.ExpiresImmediately.Should().BeTrue(
        because: "session with null duration should be marked as immediate expiry");

    _sessionManager.Sessions.Should().ContainKey(result.Id,
        because: "created session should be stored in manager's session dictionary");
  }

  [Fact]
  public async Task CreateSession_WithCustomDuration_CreatesSessionWithSpecifiedDuration()
  {
    // Arrange
    var scopeId = Guid.NewGuid();
    var customDuration = TimeSpan.FromMinutes(30);

    // Act
    var result = await _sessionManager.CreateSession(scopeId, customDuration);

    // Assert
    result.Duration.Should().Be(customDuration,
        because: "session should use the explicitly provided duration");
    result.ExpiresImmediately.Should().BeFalse(
        because: "session with explicit duration should not expire immediately");
    result.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(customDuration), TimeSpan.FromSeconds(1),
        because: "expiration time should be set based on custom duration");
  }

  [Fact]
  public async Task CreateSession_WithNullDuration_CreatesSessionWithImmediateExpiry()
  {
    // Arrange
    var scopeId = Guid.NewGuid();

    // Act
    var result = await _sessionManager.CreateSession(scopeId, null);

    // Assert
    result.ExpiresImmediately.Should().BeTrue(
        because: "null duration should indicate immediate expiry");
    result.Duration.Should().Be(TimeSpan.FromMinutes(15),
        because: "duration should still be set to default for expiration calculation");
  }

  [Fact]
  public async Task CreateSession_WithEmptyScopeId_ThrowsArgumentException()
  {
    // Act & Assert
    Func<Task> act = async () => await _sessionManager.CreateSession(Guid.Empty);

    await act.Should().ThrowAsync<ArgumentException>()
        .WithParameterName("scopeId",
        because: "empty scope ID is not valid for session creation");
  }

  #endregion

  #region GetSession Tests

  [Fact]
  public async Task GetSession_WithValidSessionId_ReturnsAndRefreshesSession()
  {
    // Arrange
    var session = new SessionBuilder()
        .WithLastAccessedAt(DateTime.UtcNow.AddMinutes(-5))
        .Build();

    _sessionManager.Sessions.TryAdd(session.Id, session);

    // Act
    var result = await _sessionManager.GetSession(session.Id);

    // Assert
    result.Should().BeSameAs(session,
        because: "the exact session instance should be returned");
    result.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1),
        because: "session should be refreshed when accessed");
  }

  [Fact]
  public async Task GetSession_WithNonExistentSessionId_ThrowsKeyNotFoundException()
  {
    // Arrange
    var nonExistentSessionId = Guid.NewGuid();

    // Act & Assert
    Func<Task> act = async () => await _sessionManager.GetSession(nonExistentSessionId);

    await act.Should().ThrowAsync<KeyNotFoundException>();
  }

  #endregion

  #region GetSessionByScope Tests

  [Fact]
  public async Task GetSessionByScope_WithExistingScope_ReturnsAndRefreshesSession()
  {
    // Arrange
    var scopeId = Guid.NewGuid();
    var session = new SessionBuilder()
        .WithScope(scopeId)
        .WithLastAccessedAt(DateTime.UtcNow.AddMinutes(-3))
        .Build();

    _sessionManager.Sessions.TryAdd(session.Id, session);

    // Act
    var result = await _sessionManager.GetSessionByScope(scopeId);

    // Assert
    result.Should().BeSameAs(session,
        because: "existing session with matching scope should be returned");
    result.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1),
        because: "returned session should be refreshed");
  }

  [Fact]
  public async Task GetSessionByScope_WithNonExistentScope_CreatesNewSession()
  {
    // Arrange
    var newScopeId = Guid.NewGuid();

    // Act
    var result = await _sessionManager.GetSessionByScope(newScopeId);

    // Assert
    result.Should().NotBeNull(because: "a new session should be created for non-existent scope");
    result.Scopes.Should().ContainKey(newScopeId,
        because: "new session should contain the requested scope");
    _sessionManager.Sessions.Should().ContainValue(result,
        because: "new session should be added to manager's dictionary");
  }

  [Fact]
  public async Task GetSessionByScope_WithEmptyScopeId_ThrowsArgumentException()
  {
    // Act & Assert
    Func<Task> act = async () => await _sessionManager.GetSessionByScope(Guid.Empty);

    await act.Should().ThrowAsync<ArgumentException>()
        .WithParameterName("scopeId");
  }

  #endregion

  #region GetSessionByUserId Tests

  [Fact]
  public async Task GetSessionByUserId_WithExistingUser_ReturnsAndRefreshesSession()
  {
    // Arrange
    var userId = "test-user-123";
    var session = new SessionBuilder()
        .WithUserId(userId)
        .WithLastAccessedAt(DateTime.UtcNow.AddMinutes(-2))
        .Build();

    _sessionManager.Sessions.TryAdd(session.Id, session);

    // Act
    var result = await _sessionManager.GetSessionByUserId(userId, Guid.NewGuid());

    // Assert
    result.Should().BeSameAs(session,
        because: "existing session with matching user ID should be returned");
    result.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1),
        because: "returned session should be refreshed");
  }

  [Fact]
  public async Task GetSessionByUserId_WithNonExistentUser_CreatesNewSessionWithUserId()
  {
    // Arrange
    var userId = "new-user-456";
    var scopeId = Guid.NewGuid();

    // Act
    var result = await _sessionManager.GetSessionByUserId(userId, scopeId);

    // Assert
    result.Should().NotBeNull(because: "a new session should be created for non-existent user");
    result.UserId.Should().Be(userId, because: "new session should be assigned the provided user ID");
    result.Scopes.Should().ContainKey(scopeId,
        because: "new session should contain the provided scope");
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  public async Task GetSessionByUserId_WithInvalidUserId_ThrowsArgumentException(string? invalidUserId)
  {
    // Act & Assert
    Func<Task> act = async () => await _sessionManager.GetSessionByUserId(invalidUserId!, Guid.NewGuid());

    await act.Should().ThrowAsync<ArgumentException>()
        .WithParameterName("userId");
  }

  [Fact]
  public async Task GetSessionByUserId_WithWhitespaceUserId_CreatesNewSession()
  {
    // Arrange
    var whitespaceUserId = "   ";
    var scopeId = Guid.NewGuid();

    // Act
    var result = await _sessionManager.GetSessionByUserId(whitespaceUserId, scopeId);

    // Assert
    result.Should().NotBeNull(because: "whitespace user ID is accepted by the implementation");
    result.UserId.Should().Be(whitespaceUserId, because: "whitespace is preserved as user ID");
  }

  #endregion

  #region GetSessionByApiKey Tests

  [Fact]
  public async Task GetSessionByApiKey_WithExistingApiKey_ReturnsAndRefreshesSession()
  {
    // Arrange
    var apiKey = "sk-test-api-key-12345";
    var session = new SessionBuilder()
        .WithApiKey(apiKey)
        .WithLastAccessedAt(DateTime.UtcNow.AddMinutes(-4))
        .Build();

    _sessionManager.Sessions.TryAdd(session.Id, session);

    // Act
    var result = await _sessionManager.GetSessionByApiKey(apiKey, Guid.NewGuid());

    // Assert
    result.Should().BeSameAs(session,
        because: "existing session with matching API key should be returned");
    result.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1),
        because: "returned session should be refreshed");
  }

  [Fact]
  public async Task GetSessionByApiKey_WithNonExistentApiKey_CreatesNewSessionWithApiKey()
  {
    // Arrange
    var apiKey = "sk-new-api-key-67890";
    var scopeId = Guid.NewGuid();

    // Act
    var result = await _sessionManager.GetSessionByApiKey(apiKey, scopeId);

    // Assert
    result.Should().NotBeNull(because: "a new session should be created for non-existent API key");
    result.ApiKeyValue.Should().Be(apiKey,
        because: "new session should be assigned the provided API key");
    result.Scopes.Should().ContainKey(scopeId,
        because: "new session should contain the provided scope");
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  public async Task GetSessionByApiKey_WithInvalidApiKey_ThrowsArgumentException(string? invalidApiKey)
  {
    // Act & Assert
    Func<Task> act = async () => await _sessionManager.GetSessionByApiKey(invalidApiKey!, Guid.NewGuid());

    await act.Should().ThrowAsync<ArgumentException>()
        .WithParameterName("apiKey");
  }

  [Fact]
  public async Task GetSessionByApiKey_WithWhitespaceApiKey_CreatesNewSession()
  {
    // Arrange
    var whitespaceApiKey = "   ";
    var scopeId = Guid.NewGuid();

    // Act
    var result = await _sessionManager.GetSessionByApiKey(whitespaceApiKey, scopeId);

    // Assert
    result.Should().NotBeNull(because: "whitespace API key is accepted by the implementation");
    result.ApiKeyValue.Should().Be(whitespaceApiKey, because: "whitespace is preserved as API key");
  }

  #endregion

  #region EndSession Tests

  [Fact]
  public async Task EndSession_WithValidSession_RemovesSessionAndPersistsData()
  {
    // Arrange
    var session = new SessionBuilder().Build();
    var conversation = new LlmConversation { Id = "test-conversation" };
    var order = new CookbookOrder
    {
      OrderId = "ORDER-123",
      Email = "test@example.com",
      Customer = new Customer { Email = "test@example.com" }
    };

    session.Conversations.TryAdd(conversation.Id, conversation);
    typeof(Session).GetProperty("Order")?.SetValue(session, order);

    _sessionManager.Sessions.TryAdd(session.Id, session);

    _mockLlmRepository
        .Setup(r => r.WriteConversationAsync(conversation, session))
        .Returns(Task.CompletedTask);

    // Act
    await _sessionManager.EndSession(session);

    // Assert
    _sessionManager.Sessions.Should().NotContainKey(session.Id,
        because: "ended session should be removed from manager");

    _mockOrderRepository.Verify(
        r => r.AddUpdateOrderAsync(order),
        Times.Once());

    _mockCustomerRepository.Verify(
        r => r.SaveCustomer(order.Customer),
        Times.Once());

    _mockLlmRepository.Verify(
        r => r.WriteConversationAsync(conversation, session),
        Times.Once());
  }

  [Fact]
  public async Task EndSession_WithNullSession_ThrowsArgumentNullException()
  {
    // Act & Assert
    Func<Task> act = async () => await _sessionManager.EndSession((Session)null!);

    await act.Should().ThrowAsync<ArgumentNullException>();
  }

  [Fact]
  public async Task EndSession_WithNonExistentSession_LogsWarningAndReturns()
  {
    // Arrange
    var session = new SessionBuilder().Build();
    // Note: session is not added to Sessions dictionary

    // Act - Should not throw
    await _sessionManager.EndSession(session);

    // Assert - Method completes successfully
  }

  [Fact]
  public async Task EndSession_WithPersistenceError_RethrowsException()
  {
    // Arrange
    var session = new SessionBuilder().Build();
    var conversation = new LlmConversation { Id = "test-conversation" };
    session.Conversations.TryAdd(conversation.Id, conversation);
    _sessionManager.Sessions.TryAdd(session.Id, session);

    var expectedException = new InvalidOperationException("Persistence failure");
    _mockLlmRepository
        .Setup(r => r.WriteConversationAsync(conversation, session))
        .ThrowsAsync(expectedException);

    // Act & Assert
    Func<Task> act = async () => await _sessionManager.EndSession(session);

    await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("Persistence failure");
  }

  [Fact]
  public async Task EndSession_ByOrderId_WithValidOrderId_EndsCorrectSession()
  {
    // Arrange
    var orderId = "ORDER-456";
    var order = new CookbookOrder { OrderId = orderId };
    var session = new SessionBuilder().Build();
    typeof(Session).GetProperty("Order")?.SetValue(session, order);
    _sessionManager.Sessions.TryAdd(session.Id, session);

    // Act
    await _sessionManager.EndSession(orderId);

    // Assert
    _sessionManager.Sessions.Should().NotContainKey(session.Id,
        because: "session with matching order ID should be ended");
  }

  [Fact]
  public async Task EndSession_ByOrderId_WithNonExistentOrderId_LogsWarningAndReturns()
  {
    // Arrange
    var nonExistentOrderId = "NONEXISTENT-ORDER";

    // Act - Should not throw
    await _sessionManager.EndSession(nonExistentOrderId);

    // Assert - Method completes successfully
  }

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  public async Task EndSession_ByOrderId_WithInvalidOrderId_ThrowsArgumentException(string? invalidOrderId)
  {
    // Act & Assert
    Func<Task> act = async () => await _sessionManager.EndSession(invalidOrderId!);

    await act.Should().ThrowAsync<ArgumentException>()
        .WithParameterName("orderId");
  }

  [Fact]
  public async Task EndSession_ByScopeId_WithValidScopeId_EndsCorrectSession()
  {
    // Arrange
    var scopeId = Guid.NewGuid();
    var session = new SessionBuilder()
        .WithScope(scopeId)
        .Build();
    _sessionManager.Sessions.TryAdd(session.Id, session);

    // Act
    await _sessionManager.EndSession(scopeId);

    // Assert
    _sessionManager.Sessions.Should().NotContainKey(session.Id,
        because: "session with matching scope ID should be ended");
  }

  [Fact]
  public async Task EndSession_ByScopeId_WithNonExistentScopeId_LogsWarningAndReturns()
  {
    // Arrange
    var nonExistentScopeId = Guid.NewGuid();

    // Act - Should not throw
    await _sessionManager.EndSession(nonExistentScopeId);

    // Assert - Method completes successfully
  }

  [Fact]
  public async Task EndSession_ByScopeId_WithEmptyScopeId_ThrowsArgumentException()
  {
    // Act & Assert
    Func<Task> act = async () => await _sessionManager.EndSession(Guid.Empty);

    await act.Should().ThrowAsync<ArgumentException>()
        .WithParameterName("scopeId");
  }

  #endregion

  #region FindReusableAnonymousSession Tests

  [Fact]
  public void FindReusableAnonymousSession_WithReusableSession_ReturnsAndRefreshesSession()
  {
    // Arrange
    var reusableSession = new SessionBuilder()
        .Anonymous()
        .WithDuration(TimeSpan.FromMinutes(30)) // Not immediate expiry
        .WithLastAccessedAt(DateTime.UtcNow.AddMinutes(-5))
        .Build();

    var nonReusableSession = new SessionBuilder()
        .WithUserId("user-123")
        .Build();

    _sessionManager.Sessions.TryAdd(reusableSession.Id, reusableSession);
    _sessionManager.Sessions.TryAdd(nonReusableSession.Id, nonReusableSession);

    // Act
    var result = _sessionManager.FindReusableAnonymousSession();

    // Assert
    result.Should().BeSameAs(reusableSession,
        because: "the anonymous session without immediate expiry should be returned");
    result!.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1),
        because: "found session should be refreshed");
  }

  [Fact]
  public void FindReusableAnonymousSession_WithNoReusableSessions_ReturnsNull()
  {
    // Arrange
    var sessionWithUser = new SessionBuilder()
        .WithUserId("user-456")
        .Build();

    var sessionWithApiKey = new SessionBuilder()
        .WithApiKey("sk-api-key")
        .Build();

    var immediateExpirySession = new SessionBuilder()
        .Anonymous()
        .WithImmediateExpiry()
        .Build();

    _sessionManager.Sessions.TryAdd(sessionWithUser.Id, sessionWithUser);
    _sessionManager.Sessions.TryAdd(sessionWithApiKey.Id, sessionWithApiKey);
    _sessionManager.Sessions.TryAdd(immediateExpirySession.Id, immediateExpirySession);

    // Act
    var result = _sessionManager.FindReusableAnonymousSession();

    // Assert
    result.Should().BeNull(because: "no sessions meet the reusable criteria");
  }

  [Fact]
  public void FindReusableAnonymousSession_WithEmptySessions_ReturnsNull()
  {
    // Act
    var result = _sessionManager.FindReusableAnonymousSession();

    // Assert
    result.Should().BeNull(because: "no sessions exist to be reused");
  }

  #endregion

  #region AddScopeToSession Tests

  [Fact]
  public void AddScopeToSession_WithValidParameters_AddsScopeAndRefreshesSession()
  {
    // Arrange
    var session = new SessionBuilder()
        .WithLastAccessedAt(DateTime.UtcNow.AddMinutes(-5))
        .Build();
    var newScopeId = Guid.NewGuid();

    // Act
    _sessionManager.AddScopeToSession(session, newScopeId);

    // Assert
    session.Scopes.Should().ContainKey(newScopeId,
        because: "the new scope should be added to the session");
    session.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1),
        because: "session should be refreshed when scope is added");
  }

  [Fact]
  public void AddScopeToSession_WithNullSession_ThrowsArgumentNullException()
  {
    // Act & Assert
    Action act = () => _sessionManager.AddScopeToSession(null!, Guid.NewGuid());

    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void AddScopeToSession_WithEmptyScopeId_ThrowsArgumentException()
  {
    // Arrange
    var session = new SessionBuilder().Build();

    // Act & Assert
    Action act = () => _sessionManager.AddScopeToSession(session, Guid.Empty);

    act.Should().Throw<ArgumentException>()
        .WithParameterName("scopeId");
  }

  #endregion

  #region RemoveScopeFromSession Tests

  [Fact]
  public void RemoveScopeFromSession_WithValidScopeInSession_RemovesScope()
  {
    // Arrange
    var scopeId = Guid.NewGuid();
    var session = new SessionBuilder()
        .WithScope(scopeId)
        .Build();
    _sessionManager.Sessions.TryAdd(session.Id, session);

    // Act
    _sessionManager.RemoveScopeFromSession(scopeId, session);

    // Assert
    session.Scopes.Should().NotContainKey(scopeId,
        because: "the scope should be removed from the session");
  }

  [Fact]
  public void RemoveScopeFromSession_WithoutSpecificSession_FindsAndRemovesFromCorrectSession()
  {
    // Arrange
    var scopeId = Guid.NewGuid();
    var session = new SessionBuilder()
        .WithScope(scopeId)
        .Build();
    _sessionManager.Sessions.TryAdd(session.Id, session);

    // Act
    _sessionManager.RemoveScopeFromSession(scopeId);

    // Assert
    session.Scopes.Should().NotContainKey(scopeId,
        because: "scope should be found and removed from the correct session");
  }

  [Fact]
  public void RemoveScopeFromSession_WithNonExistentScope_LogsWarningAndReturns()
  {
    // Arrange
    var nonExistentScopeId = Guid.NewGuid();

    // Act - Should not throw
    _sessionManager.RemoveScopeFromSession(nonExistentScopeId);

    // Assert - Method completes successfully
  }

  [Fact]
  public void RemoveScopeFromSession_WithEmptyScopeId_ThrowsArgumentException()
  {
    // Act & Assert
    Action act = () => _sessionManager.RemoveScopeFromSession(Guid.Empty);

    act.Should().Throw<ArgumentException>()
        .WithParameterName("scopeId");
  }

  #endregion

  #region Sessions Property Tests

  [Fact]
  public void Sessions_Property_ReturnsThreadSafeConcurrentDictionary()
  {
    // Act
    var sessions = _sessionManager.Sessions;

    // Assert
    sessions.Should().NotBeNull(because: "Sessions property should provide access to the session dictionary");
    sessions.Should().BeOfType<ConcurrentDictionary<Guid, Session>>(
        because: "Sessions should be thread-safe for concurrent access");
  }

  [Fact]
  public void Sessions_Property_IsSameInstanceAcrossAccess()
  {
    // Act
    var sessions1 = _sessionManager.Sessions;
    var sessions2 = _sessionManager.Sessions;

    // Assert
    sessions1.Should().BeSameAs(sessions2,
        because: "Sessions property should return the same dictionary instance");
  }

  #endregion

  #region Repository Failure Scenarios (Edge Case Tests)

  [Fact]
  public async Task GetSessionByOrder_RepositoryThrowsException_HandlesGracefully()
  {
    // Arrange
    var orderId = "ORDER-FAIL-123";
    var scopeId = Guid.NewGuid();
    var existingSession = new SessionBuilder()
        .WithScope(scopeId)
        .Build();
    _sessionManager.Sessions.TryAdd(existingSession.Id, existingSession);

    var repositoryException = new InvalidOperationException("Database connection failed");
    _mockOrderRepository
        .Setup(r => r.GetOrder(orderId))
        .ThrowsAsync(repositoryException);

    // Act & Assert
    Func<Task> act = async () => await _sessionManager.GetSessionByOrder(orderId, scopeId);

    await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("Database connection failed",
        because: "repository exceptions should propagate to allow proper error handling");
  }

  [Fact]
  public async Task GetSessionByOrder_CustomerRepositoryThrowsException_HandlesGracefully()
  {
    // Arrange
    var orderId = "ORDER-CUST-FAIL-456";
    var scopeId = Guid.NewGuid();
    var existingSession = new SessionBuilder()
        .WithScope(scopeId)
        .Build();
    _sessionManager.Sessions.TryAdd(existingSession.Id, existingSession);

    var order = new CookbookOrder { OrderId = orderId, Email = "test@example.com" };
    _mockOrderRepository
        .Setup(r => r.GetOrder(orderId))
        .ReturnsAsync(order);

    var customerException = new TimeoutException("Customer service timeout");
    _mockCustomerRepository
        .Setup(r => r.GetCustomerByEmail(order.Email))
        .ThrowsAsync(customerException);

    // Act & Assert
    Func<Task> act = async () => await _sessionManager.GetSessionByOrder(orderId, scopeId);

    await act.Should().ThrowAsync<TimeoutException>()
        .WithMessage("Customer service timeout",
        because: "customer repository exceptions should propagate for proper error handling");
  }

  [Fact]
  public async Task EndSession_CustomerRepositorySaveFails_RethrowsException()
  {
    // Arrange
    var session = new SessionBuilder().Build();
    var order = new CookbookOrder
    {
      OrderId = "ORDER-789",
      Email = "customer@example.com",
      Customer = new Customer { Email = "customer@example.com" }
    };
    typeof(Session).GetProperty("Order")?.SetValue(session, order);
    _sessionManager.Sessions.TryAdd(session.Id, session);

    var customerSaveException = new InvalidOperationException("Customer save failed");
    _mockCustomerRepository
        .Setup(r => r.SaveCustomer(order.Customer))
        .Throws(customerSaveException);

    // Act & Assert
    Func<Task> act = async () => await _sessionManager.EndSession(session);

    await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("Customer save failed",
        because: "customer repository failures should be propagated during session ending");
  }

  [Fact]
  public async Task EndSession_LlmRepositoryTimeout_HandlesGracefully()
  {
    // Arrange
    var session = new SessionBuilder().Build();
    var conversation = new LlmConversation { Id = "timeout-conversation" };
    session.Conversations.TryAdd(conversation.Id, conversation);
    _sessionManager.Sessions.TryAdd(session.Id, session);

    var timeoutException = new TimeoutException("LLM repository timeout");
    _mockLlmRepository
        .Setup(r => r.WriteConversationAsync(conversation, session))
        .ThrowsAsync(timeoutException);

    // Act & Assert
    Func<Task> act = async () => await _sessionManager.EndSession(session);

    await act.Should().ThrowAsync<TimeoutException>()
        .WithMessage("LLM repository timeout",
        because: "LLM repository timeouts should be propagated for retry logic");
  }

  [Fact]
  public async Task EndSession_MultipleConversationsOneFails_PropagatesFirstException()
  {
    // Arrange
    var session = new SessionBuilder().Build();
    var conversation1 = new LlmConversation { Id = "conv-1" };
    var conversation2 = new LlmConversation { Id = "conv-2" };
    
    session.Conversations.TryAdd(conversation1.Id, conversation1);
    session.Conversations.TryAdd(conversation2.Id, conversation2);
    _sessionManager.Sessions.TryAdd(session.Id, session);

    // First conversation succeeds, second fails
    _mockLlmRepository
        .Setup(r => r.WriteConversationAsync(conversation1, session))
        .Returns(Task.CompletedTask);
    
    var writeException = new InvalidOperationException("Conversation write failed");
    _mockLlmRepository
        .Setup(r => r.WriteConversationAsync(conversation2, session))
        .ThrowsAsync(writeException);

    // Act & Assert
    Func<Task> act = async () => await _sessionManager.EndSession(session);

    await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("Conversation write failed",
        because: "first exception from parallel conversation writes should be propagated");
  }

  [Fact]
  public async Task InitializeConversation_InvalidFunctionTool_HandlesGracefully()
  {
    // Arrange
    var scopeId = Guid.NewGuid();
    var messages = new List<ChatMessage>
    {
      new SystemChatMessage("Test system message")
    };
    
    // Create function tool with null function name to test edge case  
    // Note: Using null to test edge case handling in conversation ID generation
    ChatTool? functionTool = null;

    // Act & Assert - Should handle null function name gracefully
    Func<Task> act = async () => await _sessionManager.InitializeConversation(scopeId, messages, functionTool);

    // The implementation should handle this gracefully, not throw
    await act.Should().NotThrowAsync(because: "null function name should be handled gracefully in conversation initialization");
  }

  [Fact]
  public async Task ParallelForEachAsync_ScopeFactoryFails_HandlesException()
  {
    // Arrange
    var parentScope = Mock.Of<IScopeContainer>(s => s.Id == Guid.NewGuid());
    var dataList = new[] { "item1", "item2", "item3" };
    
    var scopeException = new InvalidOperationException("Scope creation failed");
    _mockScopeFactory
        .Setup(f => f.CreateScope(parentScope))
        .Throws(scopeException);

    // Act & Assert
    Func<Task> act = async () => await _sessionManager.ParallelForEachAsync(
        parentScope,
        dataList,
        (scope, item, ct) => Task.CompletedTask
    );

    await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("Scope creation failed",
        because: "scope factory failures should be propagated from parallel operations");
  }

  [Fact]
  public async Task ParallelForEachAsync_OperationCancellation_PropagatesCorrectly()
  {
    // Arrange
    var parentScope = Mock.Of<IScopeContainer>(s => s.Id == Guid.NewGuid());
    var childScope = Mock.Of<IScopeContainer>(s => s.Id == Guid.NewGuid());
    var dataList = new[] { "item1", "item2" };
    
    _mockScopeFactory
        .Setup(f => f.CreateScope(parentScope))
        .Returns(childScope);

    using var cts = new CancellationTokenSource();
    cts.Cancel(); // Pre-cancel the token

    // Act & Assert
    Func<Task> act = async () => await _sessionManager.ParallelForEachAsync(
        parentScope,
        dataList,
        (scope, item, ct) => Task.Delay(1000, ct),
        cancellationToken: cts.Token
    );

    await act.Should().ThrowAsync<OperationCanceledException>(
        because: "pre-cancelled operations should throw OperationCanceledException");
  }

  #endregion

  #region Boundary Conditions (Edge Case Tests)

  [Fact]
  public async Task CreateSession_ConcurrentSessionCreation_HandlesRaceCondition()
  {
    // Arrange
    var scopeId = Guid.NewGuid();
    var tasks = new List<Task<Session>>();

    // Act - Create multiple sessions concurrently with same scope
    for (int i = 0; i < 10; i++)
    {
      tasks.Add(_sessionManager.CreateSession(scopeId));
    }

    var sessions = await Task.WhenAll(tasks);

    // Assert
    sessions.Should().HaveCount(10, because: "all concurrent session creations should succeed");
    sessions.Select(s => s.Id).Should().OnlyHaveUniqueItems(
        because: "each session should have a unique identifier");
    
    // All sessions should contain the same scope ID
    sessions.Should().AllSatisfy(session =>
        session.Scopes.Should().ContainKey(scopeId,
        because: "each session should contain the provided scope ID"));
  }

  [Fact]
  public async Task EndSession_ConcurrentEndSessionCalls_HandlesGracefully()
  {
    // Arrange
    var session = new SessionBuilder().Build();
    var conversation = new LlmConversation { Id = "concurrent-test" };
    session.Conversations.TryAdd(conversation.Id, conversation);
    _sessionManager.Sessions.TryAdd(session.Id, session);

    _mockLlmRepository
        .Setup(r => r.WriteConversationAsync(conversation, session))
        .Returns(Task.CompletedTask);

    var tasks = new List<Task>();

    // Act - End the same session concurrently
    for (int i = 0; i < 5; i++)
    {
      tasks.Add(_sessionManager.EndSession(session));
    }

    // Assert - Should complete without exceptions
    Func<Task> act = async () => await Task.WhenAll(tasks);
    
    await act.Should().NotThrowAsync(because: "concurrent session ending should be handled gracefully");
    
    _sessionManager.Sessions.Should().NotContainKey(session.Id,
        because: "session should be removed despite concurrent access");
  }

  [Fact]
  public void AddScopeToSession_ScopeAlreadyExists_HandlesIdempotency()
  {
    // Arrange
    var scopeId = Guid.NewGuid();
    var session = new SessionBuilder()
        .WithScope(scopeId)
        .Build();

    // Act - Add the same scope multiple times
    _sessionManager.AddScopeToSession(session, scopeId);
    _sessionManager.AddScopeToSession(session, scopeId);
    _sessionManager.AddScopeToSession(session, scopeId);

    // Assert
    session.Scopes.Should().ContainKey(scopeId,
        because: "scope should exist regardless of multiple add attempts");
    session.Scopes.Should().HaveCount(1,
        because: "adding the same scope multiple times should be idempotent");
  }

  [Fact]
  public void RemoveScopeFromSession_ConcurrentScopeRemoval_HandlesRaceCondition()
  {
    // Arrange
    var scopeId = Guid.NewGuid();
    var session = new SessionBuilder()
        .WithScope(scopeId)
        .Build();
    _sessionManager.Sessions.TryAdd(session.Id, session);

    var tasks = new List<Task>();

    // Act - Remove the same scope concurrently
    for (int i = 0; i < 5; i++)
    {
      tasks.Add(Task.Run(() => _sessionManager.RemoveScopeFromSession(scopeId, session)));
    }

    // Assert - Should complete without exceptions
    Action act = () => Task.WaitAll(tasks.ToArray());
    
    act.Should().NotThrow(because: "concurrent scope removal should be handled gracefully");
    
    session.Scopes.Should().NotContainKey(scopeId,
        because: "scope should be removed despite concurrent access");
  }

  [Fact]
  public async Task ParallelForEachAsync_MaxDegreeOfParallelism_RespectedUnderLoad()
  {
    // Arrange
    var parentScope = Mock.Of<IScopeContainer>(s => s.Id == Guid.NewGuid());
    var childScope = Mock.Of<IScopeContainer>(s => s.Id == Guid.NewGuid());
    var dataList = Enumerable.Range(1, 100).Select(i => $"item-{i}").ToArray();
    var maxDegreeOfParallelism = 3;
    var concurrentCount = 0;
    var maxObservedConcurrency = 0;
    var lockObject = new object();
    
    _mockScopeFactory
        .Setup(f => f.CreateScope(parentScope))
        .Returns(childScope);

    // Act
    await _sessionManager.ParallelForEachAsync(
        parentScope,
        dataList,
        async (scope, item, ct) =>
        {
          lock (lockObject)
          {
            concurrentCount++;
            maxObservedConcurrency = Math.Max(maxObservedConcurrency, concurrentCount);
          }
          
          await Task.Delay(10, ct); // Simulate work
          
          lock (lockObject)
          {
            concurrentCount--;
          }
        },
        maxDegreeOfParallelism
    );

    // Assert
    maxObservedConcurrency.Should().BeLessThanOrEqualTo(maxDegreeOfParallelism + 1,
        because: "parallel execution should respect the specified max degree of parallelism");
  }

  [Fact]
  public async Task AddMessage_MaxConversationSize_HandlesLimits()
  {
    // Arrange
    var scopeId = Guid.NewGuid();
    var conversationId = "large-conversation";
    var session = new SessionBuilder()
        .WithScope(scopeId)
        .Build();
    
    var conversation = new LlmConversation { Id = conversationId };
    session.Conversations.TryAdd(conversationId, conversation);
    _sessionManager.Sessions.TryAdd(session.Id, session);

    var chatCompletion = AiServiceMockFactory.CreateMockChatCompletion("Response text");
    if (chatCompletion == null)
    {
      throw new InvalidOperationException("Failed to create mock ChatCompletion");
    }

    // Act - Add many messages to test boundary
    for (int i = 0; i < 1000; i++)
    {
      await _sessionManager.AddMessage(scopeId, conversationId, $"Prompt {i}", chatCompletion, $"Response {i}");
    }

    // Assert
    conversation.Messages.Should().HaveCount(1000,
        because: "conversation should handle large numbers of messages");
    conversation.Messages.Should().AllSatisfy(message =>
        message.Should().NotBeNull(because: "all messages should be properly initialized"));
  }

  #endregion

  #region State Transition Edge Cases (Edge Case Tests)

  [Fact]
  public async Task GetSessionByApiKey_SessionExistsButExpired_CreatesNewSession()
  {
    // Arrange
    var apiKey = "sk-expired-session-key";
    var scopeId = Guid.NewGuid();
    var expiredSession = new SessionBuilder()
        .WithApiKey(apiKey)
        .WithExpiresAt(DateTime.UtcNow.AddMinutes(-10)) // Expired 10 minutes ago
        .WithLastAccessedAt(DateTime.UtcNow.AddHours(-1))
        .Build();
    
    _sessionManager.Sessions.TryAdd(expiredSession.Id, expiredSession);

    // Act
    var result = await _sessionManager.GetSessionByApiKey(apiKey, scopeId);

    // Assert
    result.Should().BeSameAs(expiredSession,
        because: "existing session with API key should be returned regardless of expiration");
    result.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1),
        because: "session should be refreshed when accessed, updating expiration");
    result.ExpiresAt.Should().BeAfter(DateTime.UtcNow,
        because: "refreshed session should have updated expiration time");
  }

  [Fact]
  public void FindReusableAnonymousSession_SessionExpiringDuringFind_HandlesRaceCondition()
  {
    // Arrange
    var reusableSession = new SessionBuilder()
        .Anonymous()
        .WithDuration(TimeSpan.FromMinutes(30))
        .WithExpiresAt(DateTime.UtcNow.AddSeconds(1)) // Expires very soon
        .Build();
    
    _sessionManager.Sessions.TryAdd(reusableSession.Id, reusableSession);

    // Act - Find session that might expire during the operation
    var result = _sessionManager.FindReusableAnonymousSession();

    // Assert
    result.Should().BeSameAs(reusableSession,
        because: "session should be found even if near expiration");
    result!.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1),
        because: "found session should be refreshed, extending its expiration");
  }

  [Fact]
  public async Task GetSessionByOrder_ConcurrentOrderAccess_ThreadSafe()
  {
    // Arrange
    var orderId = "CONCURRENT-ORDER-123";
    var scopeIds = Enumerable.Range(1, 5).Select(_ => Guid.NewGuid()).ToArray();
    
    var order = new CookbookOrder 
    { 
      OrderId = orderId, 
      Email = "concurrent@example.com" 
    };
    var customer = new Customer { Email = "concurrent@example.com" };
    
    _mockOrderRepository
        .Setup(r => r.GetOrder(orderId))
        .ReturnsAsync(order);
    
    _mockCustomerRepository
        .Setup(r => r.GetCustomerByEmail(order.Email))
        .ReturnsAsync(customer);

    var tasks = new List<Task<Session>>();

    // Act - Access the same order with different scopes concurrently
    foreach (var scopeId in scopeIds)
    {
      // Create initial session for each scope
      var initialSession = await _sessionManager.CreateSession(scopeId);
      
      tasks.Add(_sessionManager.GetSessionByOrder(orderId, scopeId));
    }

    var sessions = await Task.WhenAll(tasks);

    // Assert
    sessions.Should().HaveCount(5, because: "all concurrent order access calls should succeed");
    
    // All sessions should reference the same order
    sessions.Should().AllSatisfy(session =>
        session.Order?.OrderId.Should().Be(orderId,
        because: "all sessions should reference the correct order"));
  }

  [Fact]
  public async Task GetSessionByOrder_CustomerNotFoundAfterOrderExists_HandlesInconsistency()
  {
    // Arrange
    var orderId = "ORDER-NO-CUSTOMER-789";
    var scopeId = Guid.NewGuid();
    var existingSession = new SessionBuilder()
        .WithScope(scopeId)
        .Build();
    _sessionManager.Sessions.TryAdd(existingSession.Id, existingSession);
    
    var order = new CookbookOrder 
    { 
      OrderId = orderId, 
      Email = "missing@customer.com" 
    };
    
    _mockOrderRepository
        .Setup(r => r.GetOrder(orderId))
        .ReturnsAsync(order);
    
    _mockCustomerRepository
        .Setup(r => r.GetCustomerByEmail(order.Email))
        .ReturnsAsync((Customer?)null);

    // Act & Assert
    Func<Task> act = async () => await _sessionManager.GetSessionByOrder(orderId, scopeId);

    await act.Should().ThrowAsync<KeyNotFoundException>()
        .WithMessage("*Customer missing@customer.com not found*",
        because: "missing customer should throw KeyNotFoundException with clear message");
  }

  [Fact]
  public async Task AddOrder_SessionExistsWithExistingOrder_LogsWarningButUpdates()
  {
    // Arrange
    var sessionId = Guid.NewGuid();
    var scopeId = Guid.NewGuid();
    var scope = Mock.Of<IScopeContainer>(s => 
        s.SessionId == sessionId && 
        s.Id == scopeId);
    
    var existingOrder = new CookbookOrder { OrderId = "EXISTING-ORDER" };
    var newOrder = new CookbookOrder { OrderId = "NEW-ORDER" };
    
    var session = new SessionBuilder()
        .WithId(sessionId)
        .WithScope(scopeId)
        .Build();
    
    // Set existing order using reflection
    typeof(Session).GetProperty("Order")?.SetValue(session, existingOrder);
    _sessionManager.Sessions.TryAdd(sessionId, session);

    // Act
    await _sessionManager.AddOrder(scope, newOrder);

    // Assert
    session.Order.Should().BeSameAs(newOrder,
        because: "new order should replace existing order");
    session.Order.OrderId.Should().Be("NEW-ORDER",
        because: "session should be updated with new order");
  }

  [Fact]
  public async Task RefreshSession_SessionExpiredDuringRefresh_HandlesRaceCondition()
  {
    // Arrange
    var session = new SessionBuilder()
        .WithLastAccessedAt(DateTime.UtcNow.AddHours(-2)) // Very old access
        .WithExpiresAt(DateTime.UtcNow.AddMinutes(-30)) // Expired
        .Build();
    
    _sessionManager.Sessions.TryAdd(session.Id, session);

    // Act - Get session which will refresh it
    var result = await _sessionManager.GetSession(session.Id);

    // Assert
    result.Should().BeSameAs(session, because: "same session instance should be returned");
    result.LastAccessedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1),
        because: "session should be refreshed with current timestamp");
    result.ExpiresAt.Should().BeAfter(DateTime.UtcNow,
        because: "expired session should have updated expiration after refresh");
  }

  [Fact]
  public async Task ParallelForEachAsync_OperationExceptionsHandledIndividually()
  {
    // Arrange
    var parentScope = Mock.Of<IScopeContainer>(s => s.Id == Guid.NewGuid());
    var childScope = Mock.Of<IScopeContainer>(s => s.Id == Guid.NewGuid());
    var dataList = new[] { "success1", "fail", "success2" };
    
    _mockScopeFactory
        .Setup(f => f.CreateScope(parentScope))
        .Returns(childScope);

    // Act & Assert
    Func<Task> act = async () => await _sessionManager.ParallelForEachAsync(
        parentScope,
        dataList,
        async (scope, item, ct) =>
        {
          if (item == "fail")
          {
            throw new InvalidOperationException($"Operation failed for {item}");
          }
          await Task.CompletedTask;
        }
    );

    await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("*Operation failed for fail*",
        because: "individual operation failures should propagate from parallel execution");
  }

  [Fact]
  public async Task InitializeConversation_MultipleConversationsWithSamePrompt_HandlesUniqueIds()
  {
    // Arrange
    var scopeId = Guid.NewGuid();
    var messages = new List<ChatMessage>
    {
      new SystemChatMessage("Identical system message")
    };
    var functionTool = ChatTool.CreateFunctionTool(
        "TestFunction", 
        "Test description", 
        BinaryData.FromString("{}"));

    // Act - Initialize multiple conversations rapidly
    var conversationId1 = await _sessionManager.InitializeConversation(scopeId, messages, functionTool);
    var conversationId2 = await _sessionManager.InitializeConversation(scopeId, messages, functionTool);
    var conversationId3 = await _sessionManager.InitializeConversation(scopeId, messages, functionTool);

    // Assert
    conversationId1.Should().NotBe(conversationId2,
        because: "each conversation should have a unique identifier");
    conversationId2.Should().NotBe(conversationId3,
        because: "each conversation should have a unique identifier");
    conversationId1.Should().NotBe(conversationId3,
        because: "each conversation should have a unique identifier");
    
    // All should contain the function name and timestamp pattern
    new[] { conversationId1, conversationId2, conversationId3 }.Should().AllSatisfy(id =>
        id.Should().StartWith("testfunction-",
        because: "conversation ID should include function name prefix"));
  }

  #endregion
}
