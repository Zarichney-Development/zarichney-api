using MediatR;
using Zarichney.Server.Services.Auth;
using Zarichney.Server.Services.Auth.Models;

namespace Zarichney.Server.Services.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthResult>;

public class RefreshTokenCommandHandler(
  ILogger<RefreshTokenCommandHandler> logger,
  IAuthService authService) : IRequestHandler<RefreshTokenCommand, AuthResult>
{
  public async Task<AuthResult> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Validate request
      if (string.IsNullOrEmpty(request.RefreshToken))
        return AuthResult.Fail("Refresh token is required");

      // Find the refresh token
      var tokenEntity = await authService.FindRefreshTokenAsync(request.RefreshToken);

      // Validate refresh token
      if (tokenEntity == null)
        return AuthResult.Fail("Invalid refresh token");

      if (tokenEntity.IsUsed)
        return AuthResult.Fail("Refresh token has been used");

      if (tokenEntity.IsRevoked)
        return AuthResult.Fail("Refresh token has been revoked");

      if (tokenEntity.ExpiresAt < DateTime.UtcNow)
        return AuthResult.Fail("Refresh token has expired");

      // Get the user associated with the token
      var user = tokenEntity.User;
      if (user == null)
        return AuthResult.Fail("User not found");

      // Generate new JWT token with role claims
      var newAccessToken = await authService.GenerateJwtTokenAsync(user);

      // Generate new refresh token
      var newRefreshToken = authService.GenerateRefreshToken();

      // Mark the current token as used
      await authService.MarkRefreshTokenAsUsedAsync(tokenEntity);

      // Save new refresh token with sliding expiry
      await authService.SaveRefreshTokenAsync(
        user,
        newRefreshToken,
        tokenEntity.DeviceName,
        tokenEntity.DeviceIp,
        tokenEntity.UserAgent);

      logger.LogInformation("User {Email} refresh token used successfully", user.Email);

      return AuthResult.Ok(
        "Token refreshed successfully",
        user.Email,
        newAccessToken,
        newRefreshToken);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during token refresh");
      return AuthResult.Fail("Failed to refresh token");
    }
  }
}