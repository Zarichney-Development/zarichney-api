using System.Text.Json.Serialization;

namespace Zarichney.Models.Auth;

/// <summary>
/// Generic result for auth operations
/// </summary>
public class AuthResult
{
    public bool Success { get; init; }
    public string? Message { get; init; }
    public string? Email { get; init; }
    public string? RedirectUrl { get; init; }

    public AuthResult(bool success, string? message, string? email = null, string? redirectUrl = null)
    {
        Success = success;
        Message = message;
        Email = email;
        RedirectUrl = redirectUrl;
    }

    public static AuthResult Fail(string message)
    {
        return new AuthResult(false, message);
    }

    public static AuthResult Ok(string message, string? email = null, string? redirectUrl = null)
    {
        return new AuthResult(true, message, email, redirectUrl);
    }
}