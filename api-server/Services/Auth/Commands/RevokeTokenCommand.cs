using MediatR;
using Zarichney.Services.Auth.Models;

namespace Zarichney.Services.Auth.Commands;

public record RevokeTokenCommand(string RefreshToken) : IRequest<AuthResult>;

public class RevokeTokenCommandHandler(
  ILogger<RevokeTokenCommandHandler> logger,
  IAuthService authService) : IRequestHandler<RevokeTokenCommand, AuthResult>
{
  public async Task<AuthResult> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Validate request
      if (string.IsNullOrEmpty(request.RefreshToken))
        return AuthResult.Fail("Refresh token is required");

      // Find the refresh token
      var tokenEntity = await authService.FindRefreshTokenAsync(request.RefreshToken);

      // Check if token exists
      if (tokenEntity == null)
        return AuthResult.Fail("Invalid refresh token");

      // Revoke the token
      await authService.RevokeRefreshTokenAsync(tokenEntity);

      logger.LogInformation("Refresh token revoked successfully");

      return AuthResult.Ok("Refresh token revoked successfully");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during token revocation");
      return AuthResult.Fail("Failed to revoke token");
    }
  }
}
