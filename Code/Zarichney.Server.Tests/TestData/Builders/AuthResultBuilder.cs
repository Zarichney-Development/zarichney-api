using Zarichney.Services.Auth.Models;

namespace Zarichney.Tests.TestData.Builders;

public class AuthResultBuilder
{
  private bool _success = true;
  private string? _message = "Operation successful";
  private string? _email = "test@example.com";
  private string? _accessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.test";
  private string? _refreshToken = "refresh-token-test-123";
  private string? _redirectUrl = null;

  public AuthResultBuilder WithSuccess(bool success)
  {
    _success = success;
    return this;
  }

  public AuthResultBuilder WithMessage(string message)
  {
    _message = message;
    return this;
  }

  public AuthResultBuilder WithEmail(string? email)
  {
    _email = email;
    return this;
  }

  public AuthResultBuilder WithAccessToken(string? accessToken)
  {
    _accessToken = accessToken;
    return this;
  }

  public AuthResultBuilder WithRefreshToken(string? refreshToken)
  {
    _refreshToken = refreshToken;
    return this;
  }

  public AuthResultBuilder WithRedirectUrl(string? redirectUrl)
  {
    _redirectUrl = redirectUrl;
    return this;
  }

  public AuthResultBuilder AsSuccess()
  {
    return WithSuccess(true)
        .WithMessage("Operation successful");
  }

  public AuthResultBuilder AsFailure(string message = "Operation failed")
  {
    return WithSuccess(false)
        .WithMessage(message)
        .WithEmail(null)
        .WithAccessToken(null)
        .WithRefreshToken(null);
  }

  public AuthResultBuilder AsLoginSuccess(string email)
  {
    return WithSuccess(true)
        .WithMessage("Login successful")
        .WithEmail(email)
        .WithAccessToken($"jwt-token-for-{email}")
        .WithRefreshToken($"refresh-token-for-{email}");
  }

  public AuthResultBuilder AsRegistrationSuccess(string email)
  {
    return WithSuccess(true)
        .WithMessage("User registered successfully. Please check your email to verify your account.")
        .WithEmail(email)
        .WithAccessToken(null)
        .WithRefreshToken(null);
  }

  public AuthResultBuilder AsEmailNotConfirmed()
  {
    return WithSuccess(false)
        .WithMessage("Please verify your email address before logging in")
        .WithAccessToken(null)
        .WithRefreshToken(null);
  }

  public AuthResultBuilder AsInvalidCredentials()
  {
    return WithSuccess(false)
        .WithMessage("Invalid email or password")
        .WithEmail(null)
        .WithAccessToken(null)
        .WithRefreshToken(null);
  }

  public AuthResult Build()
  {
    return new AuthResult(_success, _message, _email, _accessToken, _refreshToken, _redirectUrl);
  }
}
