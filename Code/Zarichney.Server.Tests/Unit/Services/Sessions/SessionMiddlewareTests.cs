using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Logging;
using Moq;
using System.Security.Claims;
using Xunit;
using Zarichney.Services.Sessions;
using Zarichney.Tests.Framework.Mocks.Factories;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Services.Sessions;

/// <summary>
/// Unit tests for SessionMiddleware covering authentication scenarios,
/// session lifecycle, and error handling.
/// </summary>
public class SessionMiddlewareTests
{
  private readonly Mock<RequestDelegate> _nextDelegateMock;
  private readonly Mock<ISessionManager> _sessionManagerMock;
  private readonly Mock<ILogger<SessionMiddleware>> _loggerMock;
  private readonly Mock<IScopeFactory> _scopeFactoryMock;
  private readonly SessionMiddleware _middleware;
  private readonly DefaultHttpContext _httpContext;
  private readonly Mock<IScopeContainer> _scopeContainerMock;

  public SessionMiddlewareTests()
  {
    _nextDelegateMock = new Mock<RequestDelegate>();
    _sessionManagerMock = SessionManagerMockFactory.CreateDefault();
    _loggerMock = new Mock<ILogger<SessionMiddleware>>();
    _scopeFactoryMock = new Mock<IScopeFactory>();
    _scopeContainerMock = new Mock<IScopeContainer>();

    // Setup scope factory to return mock scope container
    _scopeContainerMock.Setup(x => x.Id).Returns(Guid.NewGuid());
    _scopeContainerMock.SetupProperty(x => x.SessionId);
    _scopeFactoryMock.Setup(x => x.CreateScope(null))
        .Returns(_scopeContainerMock.Object);

    _middleware = new SessionMiddleware(
        _nextDelegateMock.Object,
        _sessionManagerMock.Object,
        _loggerMock.Object,
        _scopeFactoryMock.Object
    );

    _httpContext = new DefaultHttpContext();
    _nextDelegateMock.Setup(x => x(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);
  }

  #region Bypass Scenarios

  [Theory]
  [Trait("Category", "Unit")]
  [InlineData("/api/swagger")]
  [InlineData("/api/swagger/index.html")]
  [InlineData("/api/auth/login")]
  [InlineData("/api/auth/register")]
  public async Task InvokeAsync_BypassPaths_SkipsSessionProcessing(string path)
  {
    // Arrange
    _httpContext.Request.Path = path;

    // Act
    await _middleware.InvokeAsync(_httpContext);

    // Assert
    _nextDelegateMock.Verify(x => x(_httpContext), Times.Once,
        "Middleware should call next delegate for bypass paths");
    _sessionManagerMock.Verify(x => x.GetSessionByUserId(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never,
        "Session manager should not be called for bypass paths");
    _sessionManagerMock.Verify(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()), Times.Never,
        "Session should not be created for bypass paths");
  }

  #endregion

  #region Authentication Scenarios

  [Fact]
  [Trait("Category", "Unit")]
  public async Task InvokeAsync_AuthenticatedUserWithJwt_CreatesUserSession()
  {
    // Arrange
    const string userId = "test-user-123";
    _httpContext.Request.Path = "/api/test";

    var claims = new[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, "Test User")
        };
    _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));

    var expectedSession = new SessionBuilder()
        .WithUserId(userId)
        .Build();

    _sessionManagerMock.Setup(x => x.GetSessionByUserId(userId, It.IsAny<Guid>()))
        .ReturnsAsync(expectedSession);

    // Act
    await _middleware.InvokeAsync(_httpContext);

    // Assert
    _sessionManagerMock.Verify(x => x.GetSessionByUserId(userId, It.IsAny<Guid>()), Times.Once,
        "Should retrieve session for authenticated user");
    _sessionManagerMock.Verify(x => x.AddScopeToSession(expectedSession, It.IsAny<Guid>()), Times.Once,
        "Should add scope to session");
    _scopeContainerMock.VerifySet(x => x.SessionId = expectedSession.Id, Times.Once,
        "Should set session ID on scope container");
    _httpContext.Features.Get<IScopeContainer>().Should().NotBeNull(
        "Scope container should be added to HttpContext features");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task InvokeAsync_ApiKeyAuthentication_CreatesSessionWithApiKey()
  {
    // Arrange
    const string userId = "api-user-456";
    const string apiKey = "test-api-key";
    _httpContext.Request.Path = "/api/test";

    // Simulate API key authentication via HttpContext.Items
    _httpContext.Items["ApiKeyUserId"] = userId;
    _httpContext.Items["ApiKey"] = apiKey;

    var expectedSession = new SessionBuilder()
        .WithUserId(userId)
        .WithApiKey(apiKey)
        .Build();

    _sessionManagerMock.Setup(x => x.GetSessionByUserId(userId, It.IsAny<Guid>()))
        .ReturnsAsync(expectedSession);

    // Act
    await _middleware.InvokeAsync(_httpContext);

    // Assert
    _sessionManagerMock.Verify(x => x.GetSessionByUserId(userId, It.IsAny<Guid>()), Times.Once,
        "Should retrieve session for API key authenticated user");
    expectedSession.ApiKeyValue.Should().Be(apiKey,
        "Session should contain the API key value");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task InvokeAsync_UnauthenticatedRequest_CreatesAnonymousSession()
  {
    // Arrange
    _httpContext.Request.Path = "/api/public";
    _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity()); // Unauthenticated

    var anonymousSession = new SessionBuilder()
        .Anonymous()
        .Build();

    _sessionManagerMock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
        .ReturnsAsync(anonymousSession);

    // Act
    await _middleware.InvokeAsync(_httpContext);

    // Assert
    _sessionManagerMock.Verify(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()), Times.Once,
        "Should create anonymous session for unauthenticated request");
    _sessionManagerMock.Verify(x => x.GetSessionByUserId(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never,
        "Should not attempt to get session by user ID for anonymous request");
    anonymousSession.UserId.Should().BeNull(
        "Anonymous session should not have a user ID");
  }

  #endregion

  #region Session Lifecycle

  [Fact]
  [Trait("Category", "Unit")]
  public async Task InvokeAsync_ValidSession_AddsAndRemovesScope()
  {
    // Arrange
    _httpContext.Request.Path = "/api/test";
    var scopeId = Guid.NewGuid();
    _scopeContainerMock.Setup(x => x.Id).Returns(scopeId);

    var session = new SessionBuilder().Build();
    _sessionManagerMock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
        .ReturnsAsync(session);

    // Act
    await _middleware.InvokeAsync(_httpContext);

    // Assert
    _sessionManagerMock.Verify(x => x.AddScopeToSession(session, scopeId), Times.Once,
        "Should add scope to session before processing");
    _sessionManagerMock.Verify(x => x.RemoveScopeFromSession(scopeId, It.IsAny<Session?>()), Times.Once,
        "Should remove scope from session after processing");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task InvokeAsync_SessionWithImmediateExpiry_EndsSessionAfterRequest()
  {
    // Arrange
    _httpContext.Request.Path = "/api/test";

    var immediateExpirySession = new SessionBuilder()
        .WithImmediateExpiry()
        .Build();

    _sessionManagerMock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
        .ReturnsAsync(immediateExpirySession);

    // Setup RemoveScopeFromSession to actually remove the scope
    _sessionManagerMock.Setup(x => x.RemoveScopeFromSession(It.IsAny<Guid>(), It.IsAny<Session?>()))
        .Callback((Guid scopeId, Session? session) =>
        {
          immediateExpirySession.Scopes.TryRemove(scopeId, out _);
        });

    // Act
    await _middleware.InvokeAsync(_httpContext);

    // Assert
    _sessionManagerMock.Verify(x => x.EndSession(immediateExpirySession), Times.Once,
        "Should end session with immediate expiry when no scopes remain");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task InvokeAsync_ExpiredSession_EndsSessionAfterRequest()
  {
    // Arrange
    _httpContext.Request.Path = "/api/test";

    var expiredSession = new SessionBuilder()
        .WithExpiresAt(DateTime.UtcNow.AddMinutes(-5))
        .Build();

    _sessionManagerMock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
        .ReturnsAsync(expiredSession);

    // Act
    await _middleware.InvokeAsync(_httpContext);

    // Assert
    _sessionManagerMock.Verify(x => x.EndSession(expiredSession), Times.Once,
        "Should end expired session after request processing");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task InvokeAsync_SessionWithActiveScopes_DoesNotEndSession()
  {
    // Arrange
    _httpContext.Request.Path = "/api/test";

    var activeSession = new SessionBuilder()
        .WithScopes(Guid.NewGuid(), Guid.NewGuid())
        .WithExpiresAt(DateTime.UtcNow.AddMinutes(15))
        .Build();

    _sessionManagerMock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
        .ReturnsAsync(activeSession);

    // Act
    await _middleware.InvokeAsync(_httpContext);

    // Assert
    _sessionManagerMock.Verify(x => x.EndSession(It.IsAny<Session>()), Times.Never,
        "Should not end session with active scopes and valid expiry");
  }

  #endregion

  #region Error Handling

  [Fact]
  [Trait("Category", "Unit")]
  public async Task InvokeAsync_EndSessionThrowsException_LogsErrorAndContinues()
  {
    // Arrange
    _httpContext.Request.Path = "/api/test";

    var expiringSession = new SessionBuilder()
        .WithImmediateExpiry()
        .Build();
    expiringSession.Scopes.Clear();

    _sessionManagerMock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
        .ReturnsAsync(expiringSession);

    var expectedException = new InvalidOperationException("Database connection failed");
    _sessionManagerMock.Setup(x => x.EndSession(It.IsAny<Session>()))
        .ThrowsAsync(expectedException);

    // Act
    Func<Task> act = async () => await _middleware.InvokeAsync(_httpContext);

    // Assert
    await act.Should().NotThrowAsync(
        "Middleware should handle EndSession exceptions gracefully");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task InvokeAsync_NextDelegateThrows_StillRemovesScope()
  {
    // Arrange
    _httpContext.Request.Path = "/api/test";
    var scopeId = Guid.NewGuid();
    _scopeContainerMock.Setup(x => x.Id).Returns(scopeId);

    var session = new SessionBuilder().Build();
    _sessionManagerMock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
        .ReturnsAsync(session);

    var expectedException = new InvalidOperationException("Request processing failed");
    _nextDelegateMock.Setup(x => x(It.IsAny<HttpContext>()))
        .ThrowsAsync(expectedException);

    // Act
    Func<Task> act = async () => await _middleware.InvokeAsync(_httpContext);

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>(
        "Should propagate exception from next delegate");

    _sessionManagerMock.Verify(x => x.RemoveScopeFromSession(scopeId, It.IsAny<Session?>()), Times.Once,
        "Should still remove scope even when next delegate throws");
  }

  #endregion

  #region Edge Cases

  [Fact]
  [Trait("Category", "Unit")]
  public async Task InvokeAsync_SessionUserIdMismatch_PreservesExistingUserId()
  {
    // Arrange
    _httpContext.Request.Path = "/api/test";
    const string existingUserId = "existing-user";
    const string newUserId = "new-user";

    // Create session with existing user
    var session = new SessionBuilder()
        .WithUserId(existingUserId)
        .Build();

    _sessionManagerMock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
        .ReturnsAsync(session);

    // Set a different user ID in context items (edge case scenario)
    _httpContext.Items["ApiKeyUserId"] = newUserId;

    // Act
    await _middleware.InvokeAsync(_httpContext);

    // Assert
    session.UserId.Should().Be(existingUserId,
        "Should preserve existing user ID when mismatch occurs");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task InvokeAsync_NullRequestPath_HandlesGracefully()
  {
    // Arrange
    _httpContext.Request.Path = new PathString(); // Default/empty path

    var session = new SessionBuilder().Build();
    _sessionManagerMock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
        .ReturnsAsync(session);

    // Act
    Func<Task> act = async () => await _middleware.InvokeAsync(_httpContext);

    // Assert
    await act.Should().NotThrowAsync(
        "Should handle null or empty request path gracefully");

    _nextDelegateMock.Verify(x => x(_httpContext), Times.Once,
        "Should process request normally with empty path");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task InvokeAsync_AuthenticatedButNoNameIdentifierClaim_CreatesAnonymousSession()
  {
    // Arrange
    _httpContext.Request.Path = "/api/test";

    // Create authenticated identity but without NameIdentifier claim
    var claims = new[]
    {
            new Claim(ClaimTypes.Name, "Test User"),
            new Claim(ClaimTypes.Email, "test@example.com")
        };
    _httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Bearer"));

    var anonymousSession = new SessionBuilder().Anonymous().Build();
    _sessionManagerMock.Setup(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()))
        .ReturnsAsync(anonymousSession);

    // Act
    await _middleware.InvokeAsync(_httpContext);

    // Assert
    _sessionManagerMock.Verify(x => x.CreateSession(It.IsAny<Guid>(), It.IsAny<TimeSpan?>()), Times.Once,
        "Should create anonymous session when authenticated but no user ID claim");
    _sessionManagerMock.Verify(x => x.GetSessionByUserId(It.IsAny<string>(), It.IsAny<Guid>()), Times.Never,
        "Should not attempt to get session by user ID without NameIdentifier claim");
  }

  #endregion
}
