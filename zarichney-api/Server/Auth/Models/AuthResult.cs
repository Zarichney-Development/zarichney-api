namespace Zarichney.Server.Auth.Models;

/// <summary>
/// Generic result for auth operations
/// </summary>
public class AuthResult(
    bool success,
    string? message,
    string? email = null,
    string? accessToken = null,
    string? refreshToken = null,
    string? redirectUrl = null)
{
    public bool Success { get; init; } = success;
    public string? Message { get; init; } = message;
    public string? Email { get; init; } = email;

    // These will only be stored in cookies, not in the response body anymore
    internal string? AccessToken { get; init; } = accessToken;
    internal string? RefreshToken { get; init; } = refreshToken;

    // For redirects (email confirmation)
    public string? RedirectUrl { get; init; } = redirectUrl;

    public static AuthResult Fail(string message)
    {
        return new AuthResult(false, message);
    }
    
    public static AuthResult Ok(string message, string? email = null, string? accessToken = null, string? refreshToken = null, string? redirectUrl = null)
    {
        return new AuthResult(true, message, email, accessToken, refreshToken, redirectUrl);
    }
}