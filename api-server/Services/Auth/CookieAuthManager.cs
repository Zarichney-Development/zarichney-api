using Microsoft.Extensions.Options;
using Zarichney.Services.Auth.Models;

namespace Zarichney.Services.Auth;

public interface ICookieAuthManager
{
  void SetAuthCookies(HttpContext httpContext, string accessToken, string refreshToken);
  void ClearAuthCookies(HttpContext httpContext);
  string? GetRefreshTokenFromCookie(HttpContext httpContext);
}

public class CookieAuthManager(IOptions<JwtSettings> jwtSettings) : ICookieAuthManager
{
  private readonly JwtSettings _jwtSettings = jwtSettings.Value;
  private const string AccessTokenCookieName = "AuthAccessToken";
  private const string RefreshTokenCookieName = "AuthRefreshToken";

  public void SetAuthCookies(HttpContext httpContext, string accessToken, string refreshToken)
  {
    // Set JWT token as HTTP-only cookie
    httpContext.Response.Cookies.Append(AccessTokenCookieName, accessToken, new CookieOptions
    {
      HttpOnly = true,
      Secure = true,
      SameSite = SameSiteMode.Lax,
      Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes)
    });

    // Set refresh token as HTTP-only cookie
    httpContext.Response.Cookies.Append(RefreshTokenCookieName, refreshToken, new CookieOptions
    {
      HttpOnly = true,
      Secure = true,
      SameSite = SameSiteMode.Lax,
      Expires = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays)
    });
  }

  public void ClearAuthCookies(HttpContext httpContext)
  {
    httpContext.Response.Cookies.Delete(AccessTokenCookieName);
    httpContext.Response.Cookies.Delete(RefreshTokenCookieName);
  }

  public string? GetRefreshTokenFromCookie(HttpContext httpContext)
  {
    return httpContext.Request.Cookies[RefreshTokenCookieName];
  }
}