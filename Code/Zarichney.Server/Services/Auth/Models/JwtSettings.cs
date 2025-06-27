namespace Zarichney.Services.Auth.Models;

public class JwtSettings
{
  public string SecretKey { get; init; } = string.Empty;
  public string Issuer { get; init; } = string.Empty;
  public string Audience { get; init; } = string.Empty;
  public int ExpiryMinutes { get; init; } = 60;
  public int RefreshTokenExpiryDays { get; init; } = 30;
}
