using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using Zarichney.Server.Services.Auth.Models;
using ILogger = Serilog.ILogger;

namespace Zarichney.Server.Services.Auth;

public interface IApiKeyService
{
  Task<ApiKey> CreateApiKey(string userId, string name, string? description = null, DateTime? expiresAt = null);
  Task<ApiKey?> GetApiKey(string keyValue);
  Task<ApiKey?> GetApiKey(int id);
  Task<List<ApiKey>> GetApiKeysByUserId(string userId);
  Task<bool> RevokeApiKey(int id);
  Task<(bool IsValid, string? UserId)> ValidateApiKey(string keyValue);
  Task<bool> AuthenticateWithApiKey(HttpContext context, string apiKeyValue, ILogger logger);
}

public class ApiKeyService(
  UserDbContext dbContext,
  UserManager<ApplicationUser> userManager
) : IApiKeyService
{
  private const string ApiKeyHeader = "X-Api-Key";

  public async Task<ApiKey> CreateApiKey(string userId, string name, string? description = null,
    DateTime? expiresAt = null)
  {
    if (string.IsNullOrEmpty(userId))
    {
      throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }

    // Check if user exists
    var userExists = await dbContext.Users.AnyAsync(u => u.Id == userId);
    if (!userExists)
    {
      throw new KeyNotFoundException($"User with ID {userId} not found");
    }

    // Generate a secure random API key
    var keyValue = GenerateApiKey();

    var apiKey = new ApiKey
    {
      KeyValue = keyValue,
      Name = name,
      Description = description,
      CreatedAt = DateTime.UtcNow,
      ExpiresAt = expiresAt,
      IsActive = true,
      UserId = userId
    };

    await dbContext.ApiKeys.AddAsync(apiKey);
    await dbContext.SaveChangesAsync();

    return apiKey;
  }

  public async Task<ApiKey?> GetApiKey(string keyValue)
  {
    if (string.IsNullOrEmpty(keyValue))
    {
      return null;
    }

    return await dbContext.ApiKeys
      .Include(k => k.User)
      .FirstOrDefaultAsync(k => k.KeyValue == keyValue && k.IsActive);
  }

  public async Task<ApiKey?> GetApiKey(int id)
  {
    return await dbContext.ApiKeys
      .Include(k => k.User)
      .FirstOrDefaultAsync(k => k.Id == id);
  }

  public async Task<List<ApiKey>> GetApiKeysByUserId(string userId)
  {
    if (string.IsNullOrEmpty(userId))
    {
      throw new ArgumentException("User ID cannot be empty", nameof(userId));
    }

    return await dbContext.ApiKeys
      .Where(k => k.UserId == userId)
      .OrderByDescending(k => k.CreatedAt)
      .ToListAsync();
  }

  public async Task<bool> RevokeApiKey(int id)
  {
    var apiKey = await dbContext.ApiKeys.FindAsync(id);
    if (apiKey == null)
    {
      return false;
    }

    apiKey.IsActive = false;
    await dbContext.SaveChangesAsync();

    return true;
  }

  public async Task<(bool IsValid, string? UserId)> ValidateApiKey(string keyValue)
  {
    if (string.IsNullOrEmpty(keyValue))
    {
      return (false, null);
    }

    var apiKey = await dbContext.ApiKeys
      .FirstOrDefaultAsync(k =>
        k.KeyValue == keyValue &&
        k.IsActive &&
        (k.ExpiresAt == null || k.ExpiresAt > DateTime.UtcNow));

    return apiKey == null
      ? (false, null)
      : (true, apiKey.UserId);
  }

  public async Task<bool> AuthenticateWithApiKey(HttpContext context, string apiKeyValue, ILogger logger)
  {
    var path = context.Request.Path.Value ?? string.Empty;

    try
    {
      // Validate the API key and get associated user ID
      var (isValid, userId) = await ValidateApiKey(apiKeyValue);

      if (!isValid || string.IsNullOrEmpty(userId))
      {
        logger.Warning("Invalid API key attempted for path: {Path}", path);
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        await context.Response.WriteAsJsonAsync(new
        {
          error = "Invalid API key",
          path,
          timestamp = DateTimeOffset.UtcNow
        });
        return false;
      }

      // Fetch the user from the database to get roles and other info
      var user = await userManager.FindByIdAsync(userId);

      if (user == null)
      {
        logger.Warning("User {UserId} associated with API key not found for path: {Path}", userId, path);
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        await context.Response.WriteAsJsonAsync(new
        {
          error = "User associated with API key not found",
          path,
          timestamp = DateTimeOffset.UtcNow
        });
        return false;
      }

      // Get user roles
      var roles = await userManager.GetRolesAsync(user);

      // If we reach here, the API key is valid and we have a user ID
      // Create an authenticated identity with "ApiKey" as authentication type
      // The authentication type must be non-null for IsAuthenticated to return true
      var identity = new ClaimsIdentity("ApiKey");
      identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, userId));

      // Add email claim if available
      if (!string.IsNullOrEmpty(user.Email))
      {
        identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
      }

      // Add role claims
      foreach (var role in roles)
      {
        identity.AddClaim(new Claim(ClaimTypes.Role, role));
      }

      // Store the original principal if it exists
      var originalPrincipal = context.User;

      // Create a new ClaimsPrincipal with both identities (if original had any)
      var identities = new List<ClaimsIdentity> { identity };
      if (originalPrincipal.Identity != null)
      {
        identities.AddRange(originalPrincipal.Identities);
      }

      // Set the new principal with all identities
      context.User = new ClaimsPrincipal(identities);

      // Add the API key to the HttpContext items for potential use in controllers
      context.Items["ApiKey"] = apiKeyValue;
      context.Items["ApiKeyUserId"] = userId;

      logger.Information("Request authenticated with API key for user {UserId} on path: {Path}", userId, path);
      return true;
    }
    catch (Exception ex)
    {
      logger.Error(ex, "Error processing API key authentication for path: {Path}", path);
      context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
      await context.Response.WriteAsJsonAsync(new
      {
        error = "Internal server error during API key authentication",
        path,
        timestamp = DateTimeOffset.UtcNow
      });
      return false;
    }
  }

  private static string GenerateApiKey(int length = 32)
  {
    // Generate a random API key of specified length
    var randomBytes = new byte[length];
    using (var rng = RandomNumberGenerator.Create())
    {
      rng.GetBytes(randomBytes);
    }

    // Convert to base64 and remove any non-alphanumeric characters
    return Convert.ToBase64String(randomBytes)
      .Replace("/", "")
      .Replace("+", "")
      .Replace("=", "")
      .Substring(0, length);
  }
}