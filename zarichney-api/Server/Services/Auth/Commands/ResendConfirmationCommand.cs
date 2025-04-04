using MediatR;
using Microsoft.AspNetCore.Identity;
using Zarichney.Server.Services.Auth;
using Zarichney.Server.Services.Auth.Models;
using Zarichney.Server.Config;
using Zarichney.Server.Services.Emails;

namespace Zarichney.Server.Services.Auth.Commands;

public record ResendConfirmationCommand(string Email) : IRequest<AuthResult>;

public class ResendConfirmationCommandHandler(
  UserManager<ApplicationUser> userManager,
  ILogger<ResendConfirmationCommandHandler> logger,
  IEmailService emailService,
  ServerConfig serverConfig) : IRequestHandler<ResendConfirmationCommand, AuthResult>
{
  public async Task<AuthResult> Handle(ResendConfirmationCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Validate request
      if (string.IsNullOrEmpty(request.Email))
        return AuthResult.Fail("Email is required");

      // Find the user
      var user = await userManager.FindByEmailAsync(request.Email);

      // If user not found, return error
      if (user == null)
        return AuthResult.Fail("No account found with this email address");

      // Check if email is already confirmed
      if (user.EmailConfirmed)
        return AuthResult.Fail("Email has already been confirmed");

      // Generate confirmation token
      var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

      // Send email verification if email service is available
      await SendEmailVerificationAsync(user, request.Email, confirmationToken, isResend: true);

      return AuthResult.Ok("Verification email has been sent to your email address");
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during email verification resend");
      return AuthResult.Fail("Failed to resend verification email");
    }
  }

  private async Task SendEmailVerificationAsync(ApplicationUser user, string email, string confirmationToken,
    bool isResend)
  {
    try
    {
      // Create verification URL - now using the POST endpoint
      var verificationUrl =
        $"{serverConfig.BaseUrl}/api/auth/confirm-email?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(confirmationToken)}";

      // Send email verification email
      var templateData = new Dictionary<string, object>
      {
        { "verification_url", verificationUrl }
      };

      await emailService.SendEmail(
        email,
        "Verify Your Email Address",
        "email-verification",
        templateData
      );

      logger.LogInformation(
        isResend ? "Email verification resent to {Email}" : "Email verification sent to {Email}", email);
    }
    catch (Exception emailEx)
    {
      // Log error but don't fail the request
      logger.LogError(emailEx,
        isResend
          ? "Failed to resend verification email to {Email}"
          : "Failed to send verification email to {Email}", email);
    }
  }
}