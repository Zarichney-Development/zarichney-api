using MediatR;
using Microsoft.AspNetCore.Identity;
using Zarichney.Server.Services.Auth;
using Zarichney.Server.Services.Auth.Models;

namespace Zarichney.Server.Services.Auth.Commands;

public record RefreshUserClaimsCommand(string UserId, string Email) : IRequest<AuthResult>;

public class RefreshUserClaimsCommandHandler(
  ILogger<RefreshUserClaimsCommandHandler> logger,
  IAuthService authService,
  UserManager<ApplicationUser> userManager) : IRequestHandler<RefreshUserClaimsCommand, AuthResult>
{
  public async Task<AuthResult> Handle(RefreshUserClaimsCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Validate request
      if (string.IsNullOrEmpty(request.UserId))
        return AuthResult.Fail("User ID is required");

      if (string.IsNullOrEmpty(request.Email))
        return AuthResult.Fail("Email is required");

      // Find the user by ID
      var user = await userManager.FindByIdAsync(request.UserId);

      if (user == null)
        return AuthResult.Fail("User not found");

      // Verify email matches
      if (!string.Equals(user.Email, request.Email, StringComparison.OrdinalIgnoreCase))
        return AuthResult.Fail("User email mismatch");

      // Generate new JWT token with updated claims including roles
      var newAccessToken = await authService.GenerateJwtTokenAsync(user);

      // Generate new refresh token
      var newRefreshToken = authService.GenerateRefreshToken();

      // Save new refresh token
      await authService.SaveRefreshTokenAsync(
        user,
        newRefreshToken,
        deviceName: "Claims Refresh", // Could be enhanced to pass actual device info
        deviceIp: "",
        userAgent: "");

      logger.LogInformation("User {Email} claims refreshed successfully", user.Email);

      return AuthResult.Ok(
        "User claims refreshed successfully",
        user.Email,
        newAccessToken,
        newRefreshToken);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during claims refresh");
      return AuthResult.Fail("Failed to refresh user claims");
    }
  }
}