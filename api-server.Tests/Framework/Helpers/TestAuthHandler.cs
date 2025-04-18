using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Zarichney.Tests.Framework.Helpers;

/// <summary>
/// Authentication handler for testing purposes.
/// Allows tests to simulate authenticated users without actual authentication.
/// </summary>
public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestAuthHandler"/> class.
    /// </summary>
    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    /// <summary>
    /// Handles the authentication request.
    /// </summary>
    /// <returns>The authentication result.</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Check if the Authorization header exists and contains a token
        if (!Request.Headers.TryGetValue("Authorization", out var value))
        {
            return Task.FromResult(AuthenticateResult.Fail("Authorization header not found."));
        }

        var authHeader = value.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Test ", StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid authorization scheme."));
        }

        // Extract the token
        var token = authHeader["Test ".Length..].Trim();
        if (string.IsNullOrEmpty(token))
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid token."));
        }

        try
        {
            // Parse the token to get claims
            var claims = AuthTestHelper.ParseTestToken(token);
            
            // Create the authenticated user
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "Test");

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception ex)
        {
            return Task.FromResult(AuthenticateResult.Fail($"Invalid token format: {ex.Message}"));
        }
    }
}
