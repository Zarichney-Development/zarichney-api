namespace Zarichney.Server.Auth.Common;

/// <summary>
/// Generic result for auth operations
/// </summary>
public class AuthResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? Email { get; init; }
    
    // These will only be stored in cookies, not in the response body anymore
    internal string? AccessToken { get; init; }
    internal string? RefreshToken { get; init; }
    
    // For redirects (email confirmation)
    public string? RedirectUrl { get; init; }
    
    protected AuthResult(bool success, string? message, string? email = null, string? accessToken = null, string? refreshToken = null, string? redirectUrl = null)
    {
        Success = success;
        Message = message;
        Email = email;
        AccessToken = accessToken;
        RefreshToken = refreshToken;
        RedirectUrl = redirectUrl;
    }
    
    public static AuthResult Fail(string message)
    {
        return new AuthResult(false, message);
    }
    
    public static AuthResult Ok(string message, string? email = null, string? accessToken = null, string? refreshToken = null, string? redirectUrl = null)
    {
        return new AuthResult(true, message, email, accessToken, refreshToken, redirectUrl);
    }
}