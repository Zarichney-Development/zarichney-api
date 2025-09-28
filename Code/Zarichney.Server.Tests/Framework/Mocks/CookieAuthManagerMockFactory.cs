using Microsoft.AspNetCore.Http;
using Moq;
using Zarichney.Services.Auth;

namespace Zarichney.Server.Tests.Framework.Mocks;

/// <summary>
/// Factory for creating mock instances of ICookieAuthManager with various configurations for testing
/// </summary>
public static class CookieAuthManagerMockFactory
{
  /// <summary>
  /// Creates a default mock with successful operations
  /// </summary>
  public static Mock<ICookieAuthManager> CreateDefault()
  {
    var mock = new Mock<ICookieAuthManager>();

    mock.Setup(x => x.SetAuthCookies(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<string>()))
        .Verifiable();

    mock.Setup(x => x.ClearAuthCookies(It.IsAny<HttpContext>()))
        .Verifiable();

    mock.Setup(x => x.GetRefreshTokenFromCookie(It.IsAny<HttpContext>()))
        .Returns("default-refresh-token");

    return mock;
  }

  /// <summary>
  /// Creates a mock with a specific refresh token response
  /// </summary>
  public static Mock<ICookieAuthManager> CreateWithRefreshToken(string? refreshToken)
  {
    var mock = CreateDefault();

    mock.Setup(x => x.GetRefreshTokenFromCookie(It.IsAny<HttpContext>()))
        .Returns(refreshToken);

    return mock;
  }

  /// <summary>
  /// Creates a mock that simulates missing refresh token
  /// </summary>
  public static Mock<ICookieAuthManager> CreateWithNoRefreshToken()
  {
    return CreateWithRefreshToken(null);
  }

  /// <summary>
  /// Creates a mock that simulates empty refresh token
  /// </summary>
  public static Mock<ICookieAuthManager> CreateWithEmptyRefreshToken()
  {
    return CreateWithRefreshToken(string.Empty);
  }

  /// <summary>
  /// Creates a mock that tracks all cookie operations
  /// </summary>
  public static Mock<ICookieAuthManager> CreateWithTracking()
  {
    var mock = CreateDefault();
    var setAuthCallCount = 0;
    var clearAuthCallCount = 0;

    mock.Setup(x => x.SetAuthCookies(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<string>()))
        .Callback(() => setAuthCallCount++)
        .Verifiable();

    mock.Setup(x => x.ClearAuthCookies(It.IsAny<HttpContext>()))
        .Callback(() => clearAuthCallCount++)
        .Verifiable();

    // Expose deterministic token for predictable assertions
    mock.Setup(x => x.GetRefreshTokenFromCookie(It.IsAny<HttpContext>()))
        .Returns("tracked-token");

    return mock;
  }

  /// <summary>
  /// Creates a mock that throws exceptions to test error handling
  /// </summary>
  public static Mock<ICookieAuthManager> CreateWithException(Exception exception)
  {
    var mock = new Mock<ICookieAuthManager>();

    mock.Setup(x => x.SetAuthCookies(It.IsAny<HttpContext>(), It.IsAny<string>(), It.IsAny<string>()))
        .Throws(exception);

    mock.Setup(x => x.ClearAuthCookies(It.IsAny<HttpContext>()))
        .Throws(exception);

    mock.Setup(x => x.GetRefreshTokenFromCookie(It.IsAny<HttpContext>()))
        .Throws(exception);

    return mock;
  }
}
