using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Models;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Builder for creating ApiKey test instances with fluent configuration.
/// Follows framework-first approach for consistent test data creation.
/// </summary>
public class ApiKeyBuilder : BaseBuilder<ApiKeyBuilder, ApiKey>
{
  public ApiKeyBuilder()
  {
    Entity.KeyValue = GenerateSecureKey();
    Entity.Name = "Test API Key";
    Entity.Description = "Test description";
    Entity.CreatedAt = DateTime.UtcNow;
    Entity.IsActive = true;
    Entity.UserId = "test-user-123";
  }

  public ApiKeyBuilder WithKeyValue(string keyValue)
  {
    Entity.KeyValue = keyValue;
    return Self();
  }

  public ApiKeyBuilder WithName(string name)
  {
    Entity.Name = name;
    return Self();
  }

  public ApiKeyBuilder WithDescription(string? description)
  {
    Entity.Description = description;
    return Self();
  }

  public ApiKeyBuilder WithUserId(string userId)
  {
    Entity.UserId = userId;
    return Self();
  }

  public ApiKeyBuilder WithExpiresAt(DateTime? expiresAt)
  {
    Entity.ExpiresAt = expiresAt;
    return Self();
  }

  public ApiKeyBuilder WithCreatedAt(DateTime createdAt)
  {
    Entity.CreatedAt = createdAt;
    return Self();
  }

  public ApiKeyBuilder WithIsActive(bool isActive)
  {
    Entity.IsActive = isActive;
    return Self();
  }

  public ApiKeyBuilder WithUser(ApplicationUser user)
  {
    Entity.User = user;
    Entity.UserId = user.Id;
    return Self();
  }

  public ApiKeyBuilder Expired()
  {
    Entity.ExpiresAt = DateTime.UtcNow.AddDays(-1);
    return Self();
  }

  public ApiKeyBuilder ExpiringIn(TimeSpan timeSpan)
  {
    Entity.ExpiresAt = DateTime.UtcNow.Add(timeSpan);
    return Self();
  }

  public ApiKeyBuilder Inactive()
  {
    Entity.IsActive = false;
    return Self();
  }

  public ApiKeyBuilder WithoutExpiration()
  {
    Entity.ExpiresAt = null;
    return Self();
  }

  private static string GenerateSecureKey(int length = 32)
  {
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    using var rng = RandomNumberGenerator.Create();
    var randomBytes = new byte[length];
    rng.GetBytes(randomBytes);
    return new string(randomBytes.Select(b => chars[b % chars.Length]).ToArray());
  }
}
