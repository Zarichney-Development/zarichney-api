using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Zarichney.Server.Config;
using Zarichney.Server.Services.Emails;

namespace Zarichney.Server.Auth;

public interface IAuthService
{
    Task<(bool success, string message, ApplicationUser? user, string? confirmationToken)> RegisterUserAsync(string email,
      string password);

    Task<(bool success, string message, ApplicationUser? user, string? token, string? refreshToken)> LoginUserAsync(
      string email, string password);

    Task<(bool success, string message, ApplicationUser? user, string? token, string? refreshToken)> RefreshTokenAsync(
      string refreshToken);

    Task<(bool success, string message)> RevokeRefreshTokenAsync(string refreshToken);
    Task<(bool success, string message, string? resetToken)> EmailForgetPassword(string email);
    Task<(bool success, string message)> ResetPasswordAsync(string email, string token, string newPassword);
    Task<(bool success, string message, string? confirmationToken)> ResendEmailConfirmationAsync(string email);
    Task<(bool success, string message, string? redirectUrl)> ConfirmEmailAsync(string userId, string token);
}

public class AuthService(
  UserManager<ApplicationUser> userManager,
  IOptions<JwtSettings> jwtSettings,
  UserDbContext dbContext,
  ILogger<AuthService> logger,
  IEmailService emailService,
  ServerConfig serverConfig,
  ClientConfig clientConfig
) : IAuthService
{
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    public async Task<(bool success, string message, ApplicationUser? user, string? confirmationToken)> RegisterUserAsync(
      string email, string password)
    {
        try
        {
            // Validate the request
            if (string.IsNullOrEmpty(email))
                return (false, "Email is required", null, null);

            if (string.IsNullOrEmpty(password))
                return (false, "A password is required", null, null);

            // Check if user already exists
            var existingUser = await userManager.FindByEmailAsync(email);
            if (existingUser != null)
                return (false, "User with this email already exists", null, null);

            var valid = await emailService.ValidateEmail(email);
            if (!valid)
                return (false, "Invalid email address", null, null);

            // Create the user
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = false // Explicitly set to false, though this is the default
            };

            var result = await userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                logger.LogInformation("User {Email} created successfully", email);

                // Generate email confirmation token
                var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

                // Send email confirmation if email service is available
                await SendEmailVerificationAsync(user, email, confirmationToken, isResend: false);

                return (true, "User registered successfully. Please check your email to verify your account.", user,
                  confirmationToken);
            }

            // If we get here, something went wrong
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Failed to create user {Email}: {Errors}", email, errors);
            return (false, errors, null, null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user registration");
            return (false, "Failed to register user", null, null);
        }
    }

    public async Task<(bool success, string message, ApplicationUser? user, string? token, string? refreshToken)>
      LoginUserAsync(string email, string password)
    {
        try
        {
            // Validate the request
            if (string.IsNullOrEmpty(email))
                return (false, "Email is required", null, null, null);

            if (string.IsNullOrEmpty(password))
                return (false, "A password is required", null, null, null);

            // Find the user
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "Invalid email or password", null, null, null);

            // Verify password
            var isPasswordValid = await userManager.CheckPasswordAsync(user, password);
            if (!isPasswordValid)
                return (false, "Invalid email or password", null, null, null);

            // Check if email is confirmed
            if (!user.EmailConfirmed)
                return (false, "Please verify your email address before logging in", null, null, null);

            // Generate JWT token
            var token = GenerateJwtToken(user);

            // Generate and save refresh token
            var refreshToken = GenerateRefreshToken();
            await SaveRefreshTokenAsync(user, refreshToken);

            logger.LogInformation("User {Email} logged in successfully", email);

            return (true, "Login successful", user, token, refreshToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during user login");
            return (false, "Failed to login", null, null, null);
        }
    }

    public async Task<(bool success, string message, ApplicationUser? user, string? token, string? refreshToken)>
      RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(refreshToken))
                return (false, "Refresh token is required", null, null, null);

            // Find the refresh token
            var tokenEntity = await dbContext.RefreshTokens
              .Include(r => r.User)
              .FirstOrDefaultAsync(r => r.Token == refreshToken);

            // Validate refresh token
            if (tokenEntity == null)
                return (false, "Invalid refresh token", null, null, null);

            if (tokenEntity.IsUsed)
                return (false, "Refresh token has been used", null, null, null);

            if (tokenEntity.IsRevoked)
                return (false, "Refresh token has been revoked", null, null, null);

            if (tokenEntity.ExpiresAt < DateTime.UtcNow)
                return (false, "Refresh token has expired", null, null, null);

            // Get the user associated with the token
            var user = tokenEntity.User;
            if (user == null)
                return (false, "User not found", null, null, null);

            var newToken = GenerateJwtToken(user);
            var newRefreshToken = GenerateRefreshToken();

            await SaveRefreshTokenAsync(user, newRefreshToken);

            logger.LogInformation("User {Email} refresh token used successfully", user.Email);

            return (true, "Token refreshed successfully", user, newToken, newRefreshToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token refresh");
            return (false, "Failed to refresh token", null, null, null);
        }
    }

    public async Task<(bool success, string message)> RevokeRefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(refreshToken))
                return (false, "Refresh token is required");

            // Find the refresh token
            var tokenEntity = await dbContext.RefreshTokens
              .FirstOrDefaultAsync(r => r.Token == refreshToken);

            // Check if token exists
            if (tokenEntity == null)
                return (false, "Invalid refresh token");

            // Revoke the token
            tokenEntity.IsRevoked = true;
            dbContext.RefreshTokens.Update(tokenEntity);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Refresh token revoked successfully");

            return (true, "Refresh token revoked successfully");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during token revocation");
            return (false, "Failed to revoke token");
        }
    }

    private string GenerateJwtToken(ApplicationUser user)
    {
        // Get secret key bytes
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        // Create signing credentials
        var signingCredentials = new SigningCredentials(
          new SymmetricSecurityKey(key),
          SecurityAlgorithms.HmacSha256);

        // Create claims for the token
        var claims = new List<Claim>
    {
      new(JwtRegisteredClaimNames.Sub, user.Id),
      new(JwtRegisteredClaimNames.Email, user.Email!),
      new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
    };

        // Calculate expiration time
        var expires = DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes);

        // Create token descriptor
        var tokenDescriptor = new JwtSecurityToken(
          issuer: _jwtSettings.Issuer,
          audience: _jwtSettings.Audience,
          claims: claims,
          expires: expires,
          signingCredentials: signingCredentials
        );

        // Create and return the token
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }

    private static string GenerateRefreshToken()
    {
        // Generate a secure random token
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    private async Task SaveRefreshTokenAsync(ApplicationUser user, string refreshToken)
    {
        // Calculate expiry date
        var expiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpiryDays);

        // Create new refresh token
        var token = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshToken,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiryDate,
            IsUsed = false,
            IsRevoked = false
        };

        // Save to database
        await dbContext.RefreshTokens.AddAsync(token);
        await dbContext.SaveChangesAsync();
    }

    public async Task<(bool success, string message, string? resetToken)> EmailForgetPassword(string email)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(email))
                return (false, "Email is required", null);

            var user = await userManager.FindByEmailAsync(email);

            if (user == null)
                return (true, "If your email is registered, a password reset link will be sent to your inbox", null);

            // Generate password reset token
            var resetToken = await userManager.GeneratePasswordResetTokenAsync(user);

            // Send email with reset token if email service is available
            try
            {
                // Create front end reset URL
                var resetUrl =
                  $"{clientConfig.BaseUrl}/auth?reset-password&email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}";

                var templateData = new Dictionary<string, object>
        {
          { "title", "Zarichney Development" },
          { "reset_url", resetUrl }
        };

                await emailService.SendEmail(
                  email,
                  "Reset Your Password",
                  "password-reset",
                  templateData
                );

                logger.LogInformation("Password reset email sent to {Email}", email);
            }
            catch (Exception emailEx)
            {
                // Log error but don't fail the request - we'll return the token for development
                logger.LogError(emailEx, "Failed to send password reset email to {Email}", email);
            }

            // For development, return the token directly
            // In production, this would only return a message
            logger.LogInformation("Password reset token generated for user {Email}", email);

            return (true, "Password reset link has been sent to your email", resetToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during password reset request");
            return (false, "Failed to process password reset request", null);
        }
    }

    public async Task<(bool success, string message)> ResetPasswordAsync(string email, string token, string newPassword)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(email))
                return (false, "Email is required");

            if (string.IsNullOrEmpty(token))
                return (false, "Reset token is required");

            if (string.IsNullOrEmpty(newPassword))
                return (false, "New password is required");

            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "Invalid email address");

            var result = await userManager.ResetPasswordAsync(user, token, newPassword);

            if (result.Succeeded)
            {
                logger.LogInformation("Password reset successful for user {Email}", email);

                // Send confirmation email if email service is available
                try
                {
                    // Create login URL
                    var loginUrl = $"{clientConfig.BaseUrl}/login";

                    // Send confirmation email
                    var templateData = new Dictionary<string, object>
          {
            { "user_email", email },
            { "login_url", loginUrl },
            { "reset_time", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC" }
          };

                    await emailService.SendEmail(
                      email,
                      "Your Password Has Been Reset",
                      "password-reset-confirmation",
                      templateData
                    );

                    logger.LogInformation("Password reset confirmation email sent to {Email}", email);
                }
                catch (Exception emailEx)
                {
                    // Log error but don't fail the request
                    logger.LogError(emailEx, "Failed to send password reset confirmation email to {Email}", email);
                }

                return (true, "Password has been reset successfully");
            }

            // If we get here, something went wrong
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Failed to reset password for user {Email}: {Errors}", email, errors);
            return (false, errors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during password reset");
            return (false, "Failed to reset password");
        }
    }

    public async Task<(bool success, string message, string? redirectUrl)> ConfirmEmailAsync(string userId, string token)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(userId))
                return (false, "User ID is required", null);

            if (string.IsNullOrEmpty(token))
                return (false, "Confirmation token is required", null);

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "Invalid user ID", null);

            IdentityResult result = null!;
            if (!user.EmailConfirmed)
                result = await userManager.ConfirmEmailAsync(user, token);

            if (user.EmailConfirmed || result.Succeeded)
            {
                var email = user.Email!;
                logger.LogInformation("Email confirmed successfully for user {Email}", user.Email);

                // Generate a JWT token to provide some security
                var jwtToken = GenerateJwtToken(new ApplicationUser { Id = "temp", Email = email });

                // Create the redirect URL with the client base URL
                var redirectUrl =
                  $"{clientConfig.BaseUrl}/auth?mode=email-confirmed&email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(jwtToken)}";
                return (true, "Email has been confirmed", redirectUrl);
            }

            // If we get here, something went wrong
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            logger.LogWarning("Failed to confirm email for user {UserId}: {Errors}", userId, errors);
            return (false, "Invalid or expired confirmation token", null);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during email confirmation");
            return (false, "Failed to confirm email", null);
        }
    }

    public async Task<(bool success, string message, string? confirmationToken)>
      ResendEmailConfirmationAsync(string email)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(email))
                return (false, "Email is required", null);

            // Find the user
            var user = await userManager.FindByEmailAsync(email);

            // If user not found, return error
            if (user == null)
                return (false, "No account found with this email address", null);

            // Check if email is already confirmed
            if (user.EmailConfirmed)
                return (false, "Email has already been confirmed", null);

            // Generate confirmation token
            var confirmationToken = await userManager.GenerateEmailConfirmationTokenAsync(user);

            // Send email verification if email service is available
            await SendEmailVerificationAsync(user, email, confirmationToken, isResend: true);

            return (true, "Verification email has been sent to your email address", confirmationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during email verification resend");
            return (false, "Failed to resend verification email", null);
        }
    }

    // New private method to handle email verification
    private async Task SendEmailVerificationAsync(ApplicationUser user, string email, string confirmationToken,
      bool isResend)
    {
        try
        {
            // Create verification URL
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
            // Log error but don't fail the request - we'll return the token for development
            logger.LogError(emailEx,
              isResend
                ? "Failed to resend verification email to {Email}"
                : "Failed to send verification email to {Email}", email);
        }
    }
}