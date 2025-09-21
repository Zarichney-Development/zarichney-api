using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Zarichney.Server.Tests.Framework.Helpers;

/// <summary>
/// Helper class for authentication-related test functionality.
/// Provides methods for generating and parsing test tokens and creating authenticated clients.
/// </summary>
public static class AuthTestHelper
{
  /// <summary>
  /// Generates a test token for the specified user ID and roles.
  /// </summary>
  /// <param name="userId">The user ID to include in the token.</param>
  /// <param name="roles">The roles to include in the token.</param>
  /// <returns>A test token string.</returns>
  public static string GenerateTestToken(string userId, string[] roles)
  {
    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, $"TestUser_{userId}")
        };
    claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

    // Add role claims

    // Serialize claims to JSON and encode as Base64
    var claimsJson = JsonSerializer.Serialize(claims.Select(c => new { c.Type, c.Value }));
    return Convert.ToBase64String(Encoding.UTF8.GetBytes(claimsJson));
  }

  /// <summary>
  /// Parses a test token and returns the claims.
  /// </summary>
  /// <param name="token">The test token to parse.</param>
  /// <returns>A collection of claims from the token.</returns>
  public static IEnumerable<Claim> ParseTestToken(string token)
  {
    try
    {
      // Decode the Base64 token and deserialize the JSON
      var json = Encoding.UTF8.GetString(Convert.FromBase64String(token));
      var claimDtos = JsonSerializer.Deserialize<List<ClaimDto>>(json)
          ?? throw new InvalidOperationException("Failed to deserialize claims from token");

      // Convert back to Claim objects
      return claimDtos.Select(dto => new Claim(dto.Type, dto.Value));
    }
    catch (Exception ex)
    {
      throw new InvalidOperationException($"Failed to parse test token: {ex.Message}", ex);
    }
  }

  /// <summary>
  /// Creates a set of standard claims for testing.
  /// </summary>
  /// <param name="userId">The user ID to include in the claims.</param>
  /// <param name="roles">The roles to include in the claims.</param>
  /// <returns>A collection of claims.</returns>
  public static IEnumerable<Claim> CreateTestClaims(string userId, string[] roles)
  {
    var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, $"TestUser_{userId}")
        };

    // Add role claims
    var index = 0;
    for (; index < roles.Length; index++)
    {
      var role = roles[index];
      claims.Add(new Claim(ClaimTypes.Role, role));
    }

    return claims;
  }

  /// <summary>
  /// DTO for serializing and deserializing claims.
  /// </summary>
  private class ClaimDto
  {
    public string Type { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
  }
}
