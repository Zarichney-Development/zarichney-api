using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Zarichney.Server.Auth;

namespace Zarichney.Server.Auth;

public interface IAuthService
{
    Task<(bool success, string message, ApplicationUser? user)> RegisterUserAsync(string email, string password);
    Task<(bool success, string message, ApplicationUser? user, string? token, string? refreshToken)> LoginUserAsync(string email, string password);
    Task<(bool success, string message, ApplicationUser? user, string? token, string? refreshToken)> RefreshTokenAsync(string refreshToken);
    Task<(bool success, string message)> RevokeRefreshTokenAsync(string refreshToken);
}

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly JwtSettings _jwtSettings;
    private readonly UserDbContext _dbContext;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        IOptions<JwtSettings> jwtSettings,
        UserDbContext dbContext,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _jwtSettings = jwtSettings.Value;
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<(bool success, string message, ApplicationUser? user)> RegisterUserAsync(string email, string password)
    {
        try
        {
            // Validate the request
            if (string.IsNullOrEmpty(email))
                return (false, "Email is required", null);

            if (string.IsNullOrEmpty(password))
                return (false, "A password is required", null);

            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
                return (false, "User with this email already exists", null);

            // Create the user
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                _logger.LogInformation("User {Email} created successfully", email);
                return (true, "User registered successfully", user);
            }

            // If we get here, something went wrong
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            _logger.LogWarning("Failed to create user {Email}: {Errors}", email, errors);
            return (false, errors, null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during user registration");
            return (false, "Failed to register user", null);
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
}