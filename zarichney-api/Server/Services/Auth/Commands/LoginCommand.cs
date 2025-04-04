using MediatR;
using Microsoft.AspNetCore.Identity;
using Zarichney.Server.Services.Auth;
using Zarichney.Server.Services.Auth.Models;

namespace Zarichney.Server.Services.Auth.Commands;

public record LoginCommand(string Email, string Password) : IRequest<AuthResult>;

public class LoginCommandHandler(
  UserManager<ApplicationUser> userManager,
  ILogger<LoginCommandHandler> logger,
  IAuthService authService) : IRequestHandler<LoginCommand, AuthResult>
{
  public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Validate the request
      if (string.IsNullOrEmpty(request.Email))
        return AuthResult.Fail("Email is required");

      if (string.IsNullOrEmpty(request.Password))
        return AuthResult.Fail("A password is required");

      // Find the user
      var user = await userManager.FindByEmailAsync(request.Email);
      if (user == null)
        return AuthResult.Fail("Invalid email or password");

      // Verify password
      var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
      if (!isPasswordValid)
        return AuthResult.Fail("Invalid email or password");

      // Check if email is confirmed
      if (!user.EmailConfirmed)
        return AuthResult.Fail("Please verify your email address before logging in");

      // Generate JWT token with role claims
      var accessToken = await authService.GenerateJwtTokenAsync(user);

      // Generate and save refresh token
      var refreshToken = authService.GenerateRefreshToken();
      await authService.SaveRefreshTokenAsync(user, refreshToken);

      logger.LogInformation("User {Email} logged in successfully", request.Email);

      return AuthResult.Ok(
        "Login successful",
        user.Email,
        accessToken,
        refreshToken);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during user login");
      return AuthResult.Fail("Failed to login");
    }
  }

  // Uses the shared AuthService methods instead of duplicating the code
}