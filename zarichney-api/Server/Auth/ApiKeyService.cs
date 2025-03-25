using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Zarichney.Server.Auth.Models;

namespace Zarichney.Server.Auth;

public interface IApiKeyService
{
  Task<ApiKey> CreateApiKey(string userId, string name, string? description = null, DateTime? expiresAt = null);
  Task<ApiKey?> GetApiKey(string keyValue);
  Task<ApiKey?> GetApiKey(int id);
  Task<List<ApiKey>> GetApiKeysByUserId(string userId);
  Task<bool> RevokeApiKey(int id);
  Task<(bool IsValid, string? UserId)> ValidateApiKey(string keyValue);
}

public class ApiKeyService(UserDbContext dbContext) : IApiKeyService
{
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