using MediatR;
using Microsoft.AspNetCore.Identity;
using Zarichney.Services.Auth.Models;
using Zarichney.Config;

namespace Zarichney.Services.Auth.Commands;

public record ConfirmEmailCommand(string UserId, string Token) : IRequest<AuthResult>;

public class ConfirmEmailCommandHandler(
  UserManager<ApplicationUser> userManager,
  ILogger<ConfirmEmailCommandHandler> logger,
  ClientConfig clientConfig,
  IAuthService authService) : IRequestHandler<ConfirmEmailCommand, AuthResult>
{
  public async Task<AuthResult> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Validate request
      if (string.IsNullOrEmpty(request.UserId))
        return AuthResult.Fail("User ID is required");

      if (string.IsNullOrEmpty(request.Token))
        return AuthResult.Fail("Confirmation token is required");

      var user = await userManager.FindByIdAsync(request.UserId);
      if (user == null)
        return AuthResult.Fail("Invalid user ID");

      IdentityResult result = null!;
      if (!user.EmailConfirmed)
        result = await userManager.ConfirmEmailAsync(user, request.Token);

      if (user.EmailConfirmed || result.Succeeded)
      {
        var email = user.Email!;
        logger.LogInformation("Email confirmed successfully for user {Email}", user.Email);

        // Create the redirect URL with the client base URL
        var redirectUrl = $"{clientConfig.BaseUrl}/auth/email-confirmation";

        // Instead, we'll generate JWT and refresh tokens, but return them through cookies
        var accessToken = await authService.GenerateJwtTokenAsync(user);
        var refreshToken = authService.GenerateRefreshToken();
        await authService.SaveRefreshTokenAsync(user, refreshToken, "Email Confirmation");

        return AuthResult.Ok(
          "Email has been confirmed",
          email,
          accessToken,
          refreshToken,
          redirectUrl);
      }

      // If we get here, something went wrong
      var errors = string.Join(", ", result.Errors.Select(e => e.Description));
      logger.LogWarning("Failed to confirm email for user {UserId}: {Errors}", request.UserId, errors);
      return AuthResult.Fail("Invalid or expired confirmation token");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during email confirmation");
      return AuthResult.Fail("Failed to confirm email");
    }
  }
}