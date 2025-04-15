using MediatR;
using Microsoft.AspNetCore.Identity;
using Zarichney.Services.Auth.Models;
using Zarichney.Config;
using Zarichney.Services.Email;

namespace Zarichney.Services.Auth.Commands;

public record RegisterCommand(string Email, string Password) : IRequest<AuthResult>;

public class RegisterCommandHandler(
  UserManager<ApplicationUser> userManager,
  ILogger<RegisterCommandHandler> logger,
  IEmailService emailService,
  ServerConfig serverConfig) : IRequestHandler<RegisterCommand, AuthResult>
{
  public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
  {
    try
    {
      // Validate the request
      if (string.IsNullOrEmpty(request.Email))
        return AuthResult.Fail("Email is required");

      if (string.IsNullOrEmpty(request.Password))
        return AuthResult.Fail("A password is required");

      // Check if user already exists
      var existingUser = await userManager.FindByEmailAsync(request.Email);
      if (existingUser != null)
        return AuthResult.Fail("User with this email already exists");

      var valid = await emailService.ValidateEmail(request.Email);
      if (!valid)
        return AuthResult.Fail("Invalid email address");

      // Create the user
      var user = new ApplicationUser
      {
        UserName = request.Email,
        Email = request.Email,
        EmailConfirmed = false // Explicitly set to false, though this is the default
      };

      var result = await userManager.CreateAsync(user, request.Password);

      if (result.Succeeded)
      {
        logger.LogInformation("User {Email} created successfully", request.Email);

        // Generate email confirmation token
        var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

        // Send email confirmation
        await SendEmailVerificationAsync(user, request.Email, confirmationToken, isResend: false);

        return AuthResult.Ok(
          "User registered successfully. Please check your email to verify your account.",
          user.Email);
      }

      // If we get here, something went wrong
      var errors = string.Join(", ", result.Errors.Select(e => e.Description));
      logger.LogWarning("Failed to create user {Email}: {Errors}", request.Email, errors);
      return AuthResult.Fail(errors);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error during user registration");
      return AuthResult.Fail("Failed to register user");
    }
  }

  private async Task SendEmailVerificationAsync(ApplicationUser user, string email, string confirmationToken,
    bool isResend)
  {
    try
    {
      // Create verification URL - now pointing to the POST endpoint instead of appending a JWT
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