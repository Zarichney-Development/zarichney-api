using MediatR;
using Microsoft.AspNetCore.Identity;
using Zarichney.Services.Auth.Models;
using Zarichney.Config;
using Zarichney.Services.Email;

namespace Zarichney.Services.Auth.Commands;

public record ForgotPasswordCommand(string Email) : IRequest<AuthResult>;

public class ForgotPasswordCommandHandler(
  UserManager<ApplicationUser> userManager,
  ILogger<ForgotPasswordCommandHandler> logger,
  IEmailService emailService,
  ClientConfig clientConfig) : IRequestHandler<ForgotPasswordCommand, AuthResult>
{
  public async Task<AuthResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Validate request
      if (string.IsNullOrEmpty(request.Email))
        return AuthResult.Fail("Email is required");

      var user = await userManager.FindByEmailAsync(request.Email);

      if (user == null)
        return AuthResult.Ok("If your email is registered, a password reset link will be sent to your inbox");

      // Generate password reset token
      var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

      // Send email with reset token if email service is available
      try
      {
        // Create front end reset URL
        var resetUrl =
          $"{clientConfig.BaseUrl}/auth/reset-password?email={Uri.EscapeDataString(request.Email)}&token={Uri.EscapeDataString(resetToken)}";

        var templateData = new Dictionary<string, object>
        {
          { "title", "Zarichney Development" },
          { "reset_url", resetUrl }
        };

        await emailService.SendEmail(
          request.Email,
          "Reset Your Password",
          "password-reset",
          templateData
        );

        logger.LogInformation("Password reset email sent to {Email}", request.Email);
      }
      catch (Exception emailEx)
      {
        // Log error but don't fail the request
        logger.LogError(emailEx, "Failed to send password reset email to {Email}", request.Email);
      }

      logger.LogInformation("Password reset token generated for user {Email}", request.Email);

      return AuthResult.Ok("Password reset link has been sent to your email");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during password reset request");
      return AuthResult.Fail("Failed to process password reset request");
    }
  }
}