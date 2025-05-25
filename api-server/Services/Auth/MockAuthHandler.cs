using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Zarichney.Config;

namespace Zarichney.Services.Auth;

/// <summary>
/// Authentication handler for mock authentication in non-Production environments
/// when the Identity Database is unavailable. Automatically authenticates requests
/// with a configurable mock user principal.
/// </summary>
public class MockAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
  private readonly MockAuthConfig _mockAuthConfig;

  /// <summary>
  /// Initializes a new instance of the <see cref="MockAuthHandler"/> class.
  /// </summary>
  public MockAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IOptions<MockAuthConfig> mockAuthConfig)
    : base(options, logger, encoder)
  {
    _mockAuthConfig = mockAuthConfig.Value;
  }

  /// <summary>
  /// Handles the authentication request by creating a mock authenticated user.
  /// Always succeeds and returns an authenticated principal with configured roles.
  /// </summary>
  /// <returns>The authentication result with a mock user principal.</returns>
  protected override Task<AuthenticateResult> HandleAuthenticateAsync()
  {
    try
    {
      // Check for dynamic role override via header (only in Development)
      var dynamicRoles = GetDynamicRolesFromHeader();
      var rolesToUse = dynamicRoles?.Any() == true ? dynamicRoles : _mockAuthConfig.DefaultRoles;

      // Create claims for the mock user
      var claims = new List<Claim>
      {
        new(ClaimTypes.NameIdentifier, _mockAuthConfig.DefaultUserId),
        new(ClaimTypes.Name, _mockAuthConfig.DefaultUsername),
        new(ClaimTypes.Email, _mockAuthConfig.DefaultEmail)
      };

      // Add role claims
      claims.AddRange(rolesToUse.Select(role => new Claim(ClaimTypes.Role, role)));

      // Create the authenticated user identity
      var identity = new ClaimsIdentity(claims, "MockAuth");
      var principal = new ClaimsPrincipal(identity);
      var ticket = new AuthenticationTicket(principal, "MockAuth");

      Logger.LogDebug("Mock authentication successful for user {UserId} with roles {Roles}",
        _mockAuthConfig.DefaultUserId, string.Join(", ", rolesToUse));

      return Task.FromResult(AuthenticateResult.Success(ticket));
    }
    catch (Exception ex)
    {
      Logger.LogError(ex, "Error during mock authentication");
      return Task.FromResult(AuthenticateResult.Fail($"Mock authentication failed: {ex.Message}"));
    }
  }

  /// <summary>
  /// Gets dynamic roles from the X-Mock-Roles header if present and in Development environment.
  /// </summary>
  /// <returns>List of roles from header, or null if not present or not in Development.</returns>
  private List<string>? GetDynamicRolesFromHeader()
  {
    try
    {
      // Only allow dynamic roles in Development environment
      var environment = Context.RequestServices.GetService<IWebHostEnvironment>();
      if (environment?.EnvironmentName != "Development")
      {
        return null;
      }

      // Check for X-Mock-Roles header
      if (Request.Headers.TryGetValue("X-Mock-Roles", out var headerValue))
      {
        var rolesString = headerValue.ToString();
        if (!string.IsNullOrWhiteSpace(rolesString))
        {
          var roles = rolesString.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(r => r.Trim())
            .Where(r => !string.IsNullOrWhiteSpace(r) && r.Length <= 50) // Add reasonable length limit
            .Take(10) // Limit number of roles to prevent abuse
            .ToList();

          if (roles.Any())
          {
            Logger.LogDebug("Using dynamic roles from X-Mock-Roles header: {Roles}",
              string.Join(", ", roles));
            return roles;
          }
        }
      }

      return null;
    }
    catch (Exception ex)
    {
      Logger.LogWarning(ex, "Error parsing X-Mock-Roles header, falling back to default roles");
      return null;
    }
  }
}
