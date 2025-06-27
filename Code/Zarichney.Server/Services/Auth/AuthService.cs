using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Zarichney.Services.Auth.Models;

namespace Zarichney.Services.Auth;

public interface IAuthService
{
  Task<string> GenerateJwtTokenAsync(ApplicationUser user);
  string GenerateRefreshToken();

  Task<RefreshToken> SaveRefreshTokenAsync(ApplicationUser user, string refreshToken, string? deviceName = null,
    string? deviceIp = null, string? userAgent = null);

  Task<RefreshToken?> FindRefreshTokenAsync(string token);
  Task MarkRefreshTokenAsUsedAsync(RefreshToken token);
  Task RevokeRefreshTokenAsync(RefreshToken token);
}

/// <summary>
/// Service that provides shared functionality for authentication commands/queries
/// </summary>
public class AuthService(
  IOptions<JwtSettings> jwtSettings,
  UserDbContext dbContext,
  IHttpContextAccessor httpContextAccessor,
  UserManager<ApplicationUser> userManager) : IAuthService
{
  /// <summary>
  /// Generates a JWT token for the specified user, including their roles
  /// </summary>
  public async Task<string> GenerateJwtTokenAsync(ApplicationUser user)
  {
    var config = jwtSettings.Value;

    // Get secret key bytes
    var key = Encoding.UTF8.GetBytes(config.SecretKey);

    // Create signing credentials
    var signingCredentials = new SigningCredentials(
      new SymmetricSecurityKey(key),
      SecurityAlgorithms.HmacSha256);

    // Create claims for the token
    var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Sub, user.Id),
      new(JwtRegisteredClaimNames.Email, user.Email!),
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

    // Get user roles and add them as claims
    var userRoles = await userManager.GetRolesAsync(user);
    foreach (var role in userRoles)
    {
      claims.Add(new Claim(ClaimTypes.Role, role));
    }

    // Calculate expiration time
    var expires = DateTime.UtcNow.AddMinutes(config.ExpiryMinutes);

    // Create token descriptor
    var tokenDescriptor = new JwtSecurityToken(
      issuer: config.Issuer,
      audience: config.Audience,
      claims: claims,
      expires: expires,
      signingCredentials: signingCredentials
    );

    // Create and return the token
    return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
  }

  /// <summary>
  /// Generates a cryptographically secure refresh token
  /// </summary>
  public string GenerateRefreshToken()
  {
    // Generate a secure random token
    var randomBytes = new byte[64];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomBytes);
    return Convert.ToBase64String(randomBytes);
  }

  /// <summary>
  /// Saves a refresh token to the database
  /// </summary>
  public async Task<RefreshToken> SaveRefreshTokenAsync(ApplicationUser user, string refreshToken,
    string? deviceName = null, string? deviceIp = null, string? userAgent = null)
  {
    // Get client information if not provided
    var httpContext = httpContextAccessor.HttpContext;
    deviceIp ??= httpContext?.Connection.RemoteIpAddress?.ToString();
    userAgent ??= httpContext?.Request.Headers.UserAgent.ToString();

    // Calculate expiry date
    var expiryDate = DateTime.UtcNow.AddDays(jwtSettings.Value.RefreshTokenExpiryDays);

    // Create new refresh token
    var token = new RefreshToken
    {
      UserId = user.Id,
      Token = refreshToken,
      CreatedAt = DateTime.UtcNow,
      ExpiresAt = expiryDate,
      IsUsed = false,
      IsRevoked = false,
      DeviceName = deviceName,
      DeviceIp = deviceIp,
      UserAgent = userAgent,
      LastUsedAt = DateTime.UtcNow
    };

    // Save to database
    await dbContext.RefreshTokens.AddAsync(token);
    await dbContext.SaveChangesAsync();

    return token;
  }

  /// <summary>
  /// Finds a refresh token by its value
  /// </summary>
  public async Task<RefreshToken?> FindRefreshTokenAsync(string token)
  {
    return await dbContext.RefreshTokens
      .Include(t => t.User)
      .FirstOrDefaultAsync(t => t.Token == token);
  }

  /// <summary>
  /// Marks a refresh token as used
  /// </summary>
  public async Task MarkRefreshTokenAsUsedAsync(RefreshToken token)
  {
    token.IsUsed = true;
    token.LastUsedAt = DateTime.UtcNow;
    dbContext.RefreshTokens.Update(token);
    await dbContext.SaveChangesAsync();
  }

  /// <summary>
  /// Revokes a refresh token
  /// </summary>
  public async Task RevokeRefreshTokenAsync(RefreshToken token)
  {
    token.IsRevoked = true;
    dbContext.RefreshTokens.Update(token);
    await dbContext.SaveChangesAsync();
  }
}
