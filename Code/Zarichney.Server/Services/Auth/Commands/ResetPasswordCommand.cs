using MediatR;
using Microsoft.AspNetCore.Identity;
using Zarichney.Services.Auth.Models;
using Zarichney.Config;
using Zarichney.Services.Email;

namespace Zarichney.Services.Auth.Commands;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<AuthResult>;

public class ResetPasswordCommandHandler(
  UserManager<ApplicationUser> userManager,
  ILogger<ResetPasswordCommandHandler> logger,
  IEmailService emailService,
  ClientConfig clientConfig) : IRequestHandler<ResetPasswordCommand, AuthResult>
{
  public async Task<AuthResult> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Validate request
      if (string.IsNullOrEmpty(request.Email))
        return AuthResult.Fail("Email is required");

      if (string.IsNullOrEmpty(request.Token))
        return AuthResult.Fail("Reset token is required");

      if (string.IsNullOrEmpty(request.NewPassword))
        return AuthResult.Fail("New password is required");

      var user = await userManager.FindByEmailAsync(request.Email);
      if (user == null)
        return AuthResult.Fail("Invalid email address");

      var result = await userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

      if (result.Succeeded)
      {
        logger.LogInformation("Password reset successful for user {Email}", request.Email);

        // Send confirmation email if email service is available
        try
        {
          // Create login URL
          var loginUrl = $"{clientConfig.BaseUrl}/login";

          // Send confirmation email
          var templateData = new Dictionary<string, object>
          {
            { "user_email", request.Email },
            { "login_url", loginUrl },
            { "reset_time", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC" }
          };

          await emailService.SendEmail(
            request.Email,
            "Your Password Has Been Reset",
            "password-reset-confirmation",
            templateData
          );

          logger.LogInformation("Password reset confirmation email sent to {Email}", request.Email);
        }
        catch (Exception emailEx)
        {
          // Log error but don't fail the request
          logger.LogError(emailEx, "Failed to send password reset confirmation email to {Email}", request.Email);
        }

        return AuthResult.Ok("Password has been reset successfully");
      }

      // If we get here, something went wrong
      var errors = string.Join(", ", result.Errors.Select(e => e.Description));
      logger.LogWarning("Failed to reset password for user {Email}: {Errors}", request.Email, errors);
      return AuthResult.Fail(errors);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during password reset");
      return AuthResult.Fail("Failed to reset password");
    }
  }
}
