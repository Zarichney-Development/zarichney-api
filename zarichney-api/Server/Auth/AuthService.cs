using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Zarichney.Server.Auth;
using Zarichney.Server.Services.Emails;

namespace Zarichney.Server.Auth;

public interface IAuthService
{
    Task<(bool success, string message, ApplicationUser? user, string? confirmationToken)> RegisterUserAsync(string email, string password);
    Task<(bool success, string message, ApplicationUser? user, string? token, string? refreshToken)> LoginUserAsync(string email, string password);
    Task<(bool success, string message, ApplicationUser? user, string? token, string? refreshToken)> RefreshTokenAsync(string refreshToken);
    Task<(bool success, string message)> RevokeRefreshTokenAsync(string refreshToken);
    Task<(bool success, string message, string? resetToken)> ForgotPasswordAsync(string email);
    Task<(bool success, string message)> ResetPasswordAsync(string email, string token, string newPassword);
    Task<(bool success, string message)> ConfirmEmailAsync(string userId, string token);
    Task<(bool success, string message, string? confirmationToken)> ResendEmailConfirmationAsync(string email);
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly UserDbContext _dbContext;
    private readonly ILogger<AuthService> _logger;
    private readonly IEmailService? _emailService;
    private readonly string _appUrl;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtSettings> jwtSettings,
        UserDbContext dbContext,
        ILogger<AuthService> logger,
        IEmailService? emailService = null,
        IConfiguration? configuration = null)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
        _dbContext = dbContext;
        _logger = logger;
        _emailService = emailService;
        _appUrl = configuration?.GetValue<string>("AppUrl") ?? "https://localhost:5173";
    }

    public async Task<(bool success, string message, ApplicationUser? user, string? confirmationToken)> RegisterUserAsync(string email, string password)
    {
        try
        {
            // Validate the request
            if (string.IsNullOrEmpty(email))
                return (false, "Email is required", null, null);

            if (string.IsNullOrEmpty(password))
                return (false, "A password is required", null, null);

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                return (false, "User with this email already exists", null, null);

            // Create the user
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                EmailConfirmed = false // Explicitly set to false, though this is the default
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} created successfully", email);
                
                // Generate email confirmation token
                var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                
                // Send email confirmation if email service is available
                if (_emailService != null)
                {
                    try
                    {
                        // Create verification URL
                        var verificationUrl = $"{_appUrl}/confirm-email?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(confirmationToken)}";
                        
                        // Send email verification email
                        var templateData = new Dictionary<string, object>
                        {
                            { "verification_url", verificationUrl }
                        };
                        
                        await _emailService.SendEmail(
                            email,
                            "Verify Your Email Address",
                            "email-verification",
                            templateData
                        );
                        
                        _logger.LogInformation("Email verification sent to {Email}", email);
                    }
                    catch (Exception emailEx)
                    {
                        // Log error but don't fail the request - we'll return the token for development
                        _logger.LogError(emailEx, "Failed to send verification email to {Email}", email);
                    }
                }
                
                return (true, "User registered successfully. Please check your email to verify your account.", user, confirmationToken);
            }

            // If we get here, something went wrong
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create user {Email}: {Errors}", email, errors);
            return (false, errors, null, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return (false, "Failed to register user", null, null);
        }
    }

    public async Task<(bool success, string message, ApplicationUser? user, string? token, string? refreshToken)> LoginUserAsync(string email, string password)
    {
        try
        {
            // Validate the request
            if (string.IsNullOrEmpty(email))
                return (false, "Email is required", null, null, null);

            if (string.IsNullOrEmpty(password))
                return (false, "A password is required", null, null, null);

            // Find the user
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "Invalid email or password", null, null, null);

            // Verify password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
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

            _logger.LogInformation("User {Email} logged in successfully", email);

            return (true, "Login successful", user, token, refreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user login");
            return (false, "Failed to login", null, null, null);
        }
    }

    public async Task<(bool success, string message, ApplicationUser? user, string? token, string? refreshToken)> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(refreshToken))
                return (false, "Refresh token is required", null, null, null);

            // Find the refresh token
            var tokenEntity = await _dbContext.RefreshTokens
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

            // Mark current refresh token as used
            tokenEntity.IsUsed = true;
            _dbContext.RefreshTokens.Update(tokenEntity);
            
            // Generate new JWT token
            var newToken = GenerateJwtToken(user);
            
            // Generate new refresh token
            var newRefreshToken = GenerateRefreshToken();
            await SaveRefreshTokenAsync(user, newRefreshToken);
            
            // Save changes to database
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("User {Email} refresh token used successfully", user.Email);

            return (true, "Token refreshed successfully", user, newToken, newRefreshToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token refresh");
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
            var tokenEntity = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == refreshToken);

            // Check if token exists
            if (tokenEntity == null)
                return (false, "Invalid refresh token");

            // Revoke the token
            tokenEntity.IsRevoked = true;
            _dbContext.RefreshTokens.Update(tokenEntity);
            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("Refresh token revoked successfully");

            return (true, "Refresh token revoked successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during token revocation");
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
    
    private string GenerateRefreshToken()
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
        await _dbContext.RefreshTokens.AddAsync(token);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<(bool success, string message, string? resetToken)> ForgotPasswordAsync(string email)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(email))
                return (false, "Email is required", null);

            // Find the user
            var user = await _userManager.FindByEmailAsync(email);
            
            // If user not found, return generic message for security
            if (user == null)
                return (true, "If your email is registered, a password reset link will be sent to your inbox", null);

            // Generate password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            
            // Send email with reset token if email service is available
            if (_emailService != null)
            {
                try 
                {
                    // Create reset URL
                    var resetUrl = $"{_appUrl}/reset-password?email={Uri.EscapeDataString(email)}&token={Uri.EscapeDataString(resetToken)}";
                    
                    // Send password reset email
                    var templateData = new Dictionary<string, object>
                    {
                        { "reset_url", resetUrl }
                    };
                    
                    await _emailService.SendEmail(
                        email,
                        "Reset Your Password",
                        "password-reset",
                        templateData
                    );
                    
                    _logger.LogInformation("Password reset email sent to {Email}", email);
                }
                catch (Exception emailEx)
                {
                    // Log error but don't fail the request - we'll return the token for development
                    _logger.LogError(emailEx, "Failed to send password reset email to {Email}", email);
                }
            }
            
            // For development, return the token directly
            // In production, this would only return a message
            _logger.LogInformation("Password reset token generated for user {Email}", email);
            
            return (true, "Password reset link has been sent to your email", resetToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset request");
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

            // Find the user
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return (false, "Invalid email address");

            // Reset the password
            var result = await _userManager.ResetPasswordAsync(user, token, newPassword);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Password reset successful for user {Email}", email);
                
                // Send confirmation email if email service is available
                if (_emailService != null)
                {
                    try
                    {
                        // Create login URL
                        var loginUrl = $"{_appUrl}/login";
                        
                        // Send confirmation email
                        var templateData = new Dictionary<string, object>
                        {
                            { "user_email", email },
                            { "login_url", loginUrl },
                            { "reset_time", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss") + " UTC" }
                        };
                        
                        await _emailService.SendEmail(
                            email,
                            "Your Password Has Been Reset",
                            "password-reset-confirmation",
                            templateData
                        );
                        
                        _logger.LogInformation("Password reset confirmation email sent to {Email}", email);
                    }
                    catch (Exception emailEx)
                    {
                        // Log error but don't fail the request
                        _logger.LogError(emailEx, "Failed to send password reset confirmation email to {Email}", email);
                    }
                }
                
                return (true, "Password has been reset successfully");
            }
            
            // If we get here, something went wrong
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to reset password for user {Email}: {Errors}", email, errors);
            return (false, errors);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during password reset");
            return (false, "Failed to reset password");
        }
    }
    
    public async Task<(bool success, string message)> ConfirmEmailAsync(string userId, string token)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(userId))
                return (false, "User ID is required");
                
            if (string.IsNullOrEmpty(token))
                return (false, "Confirmation token is required");

            // Find the user by ID
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return (false, "Invalid user ID");
                
            // Check if email is already confirmed
            if (user.EmailConfirmed)
                return (true, "Email has already been confirmed");

            // Confirm the email
            var result = await _userManager.ConfirmEmailAsync(user, token);
            
            if (result.Succeeded)
            {
                _logger.LogInformation("Email confirmed successfully for user {Email}", user.Email);
                return (true, "Email has been confirmed successfully");
            }
            
            // If we get here, something went wrong
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to confirm email for user {UserId}: {Errors}", userId, errors);
            return (false, "Invalid or expired confirmation token");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email confirmation");
            return (false, "Failed to confirm email");
        }
    }
    
    public async Task<(bool success, string message, string? confirmationToken)> ResendEmailConfirmationAsync(string email)
    {
        try
        {
            // Validate request
            if (string.IsNullOrEmpty(email))
                return (false, "Email is required", null);

            // Find the user
            var user = await _userManager.FindByEmailAsync(email);
            
            // If user not found, return error
            if (user == null)
                return (false, "No account found with this email address", null);
                
            // Check if email is already confirmed
            if (user.EmailConfirmed)
                return (false, "Email has already been confirmed", null);

            // Generate confirmation token
            var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            
            // Send email verification if email service is available
            if (_emailService != null)
            {
                try
                {
                    // Create verification URL
                    var verificationUrl = $"{_appUrl}/confirm-email?userId={Uri.EscapeDataString(user.Id)}&token={Uri.EscapeDataString(confirmationToken)}";
                    
                    // Send email verification email
                    var templateData = new Dictionary<string, object>
                    {
                        { "verification_url", verificationUrl }
                    };
                    
                    await _emailService.SendEmail(
                        email,
                        "Verify Your Email Address",
                        "email-verification",
                        templateData
                    );
                    
                    _logger.LogInformation("Email verification resent to {Email}", email);
                }
                catch (Exception emailEx)
                {
                    // Log error but don't fail the request - we'll return the token for development
                    _logger.LogError(emailEx, "Failed to resend verification email to {Email}", email);
                }
            }
            
            return (true, "Verification email has been sent to your email address", confirmationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during email verification resend");
            return (false, "Failed to resend verification email", null);
        }
    }
}