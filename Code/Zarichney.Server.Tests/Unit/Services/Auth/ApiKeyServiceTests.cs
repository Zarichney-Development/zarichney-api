using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Security.Claims;
using Xunit;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Models;
using ILogger = Serilog.ILogger;

namespace Zarichney.Tests.Unit.Services.Auth;

/// <summary>
/// Comprehensive unit tests for ApiKeyService covering API key management and authentication functionality.
/// Tests follow established patterns with complete isolation, proper mocking, and framework-first approach.
/// </summary>
[Trait("Category", "Unit")]
public class ApiKeyServiceTests : IDisposable
{
  private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
  private readonly Mock<ILogger> _mockLogger;
  private readonly UserDbContext _dbContext;
  private readonly ApiKeyService _apiKeyService;
  private readonly string _testUserId = "test-user-123";
  private readonly string _nonExistentUserId = "non-existent-user";

  public ApiKeyServiceTests()
  {
    // Setup in-memory database for isolated testing
    var options = new DbContextOptionsBuilder<UserDbContext>()
        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        .Options;

    _dbContext = new UserDbContext(options);

    // Setup UserManager mock
    var userStore = new Mock<IUserStore<ApplicationUser>>();
    _mockUserManager = new Mock<UserManager<ApplicationUser>>(
        userStore.Object, null!, null!, null!, null!, null!, null!, null!, null!);

    _mockLogger = new Mock<ILogger>();

    _apiKeyService = new ApiKeyService(_dbContext, _mockUserManager.Object);

    // Seed test user data
    SeedTestData();
  }

  private void SeedTestData()
  {
    var testUser = new ApplicationUser
    {
      Id = _testUserId,
      UserName = "testuser@example.com",
      Email = "testuser@example.com",
      EmailConfirmed = true
    };

    _dbContext.Users.Add(testUser);
    _dbContext.SaveChanges();
  }

  #region Constructor Tests

  [Fact]
  public void Constructor_WithValidDependencies_InitializesSuccessfully()
  {
    // Arrange & Act
    var service = new ApiKeyService(_dbContext, _mockUserManager.Object);

    // Assert
    service.Should().NotBeNull(because: "ApiKeyService should initialize with valid dependencies");
  }

  #endregion

  #region CreateApiKey Tests

  [Fact]
  public async Task CreateApiKey_WithValidParameters_CreatesAndReturnsApiKey()
  {
    // Arrange
    var keyName = "Test API Key";
    var description = "Test description";
    var expiresAt = DateTime.UtcNow.AddDays(30);

    // Act
    var result = await _apiKeyService.CreateApiKey(_testUserId, keyName, description, expiresAt);

    // Assert
    result.Should().NotBeNull(because: "a valid API key should be created");
    result.Id.Should().BeGreaterThan(0, because: "API key should have a valid database ID");
    result.KeyValue.Should().NotBeNullOrEmpty(because: "API key should have a generated key value");
    result.KeyValue.Length.Should().Be(32, because: "API key should have standard length");
    result.Name.Should().Be(keyName, because: "API key should preserve the provided name");
    result.Description.Should().Be(description, because: "API key should preserve the provided description");
    result.ExpiresAt.Should().Be(expiresAt, because: "API key should preserve the provided expiration date");
    result.IsActive.Should().BeTrue(because: "newly created API key should be active by default");
    result.UserId.Should().Be(_testUserId, because: "API key should be associated with the correct user");
    result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1),
        because: "API key should be created with current timestamp");
  }

  [Fact]
  public async Task CreateApiKey_WithMinimalParameters_CreatesApiKeyWithDefaults()
  {
    // Arrange
    var keyName = "Minimal API Key";

    // Act
    var result = await _apiKeyService.CreateApiKey(_testUserId, keyName);

    // Assert
    result.Should().NotBeNull(because: "API key should be created with minimal parameters");
    result.Name.Should().Be(keyName, because: "API key should preserve the provided name");
    result.Description.Should().BeNull(because: "description should be null when not provided");
    result.ExpiresAt.Should().BeNull(because: "expiration should be null when not provided");
    result.IsActive.Should().BeTrue(because: "API key should be active by default");
  }

  [Fact]
  public async Task CreateApiKey_WithNullUserId_ThrowsArgumentException()
  {
    // Act & Assert
    var act = async () => await _apiKeyService.CreateApiKey(null!, "Test Key");

    await act.Should().ThrowAsync<ArgumentException>()
        .WithParameterName("userId")
        .WithMessage("User ID cannot be empty*",
        because: "null user ID should be rejected");
  }

  [Fact]
  public async Task CreateApiKey_WithEmptyUserId_ThrowsArgumentException()
  {
    // Act & Assert
    var act = async () => await _apiKeyService.CreateApiKey(string.Empty, "Test Key");

    await act.Should().ThrowAsync<ArgumentException>()
        .WithParameterName("userId")
        .WithMessage("User ID cannot be empty*",
        because: "empty user ID should be rejected");
  }

  [Fact]
  public async Task CreateApiKey_WithNonExistentUser_ThrowsKeyNotFoundException()
  {
    // Act & Assert
    var act = async () => await _apiKeyService.CreateApiKey(_nonExistentUserId, "Test Key");

    await act.Should().ThrowAsync<KeyNotFoundException>()
        .WithMessage($"User with ID {_nonExistentUserId} not found",
        because: "API keys should only be created for existing users");
  }

  [Fact]
  public async Task CreateApiKey_GeneratesUniqueKeyValues()
  {
    // Arrange
    var keyCount = 5;
    var apiKeys = new List<ApiKey>();

    // Act
    for (int i = 0; i < keyCount; i++)
    {
      var apiKey = await _apiKeyService.CreateApiKey(_testUserId, $"Test Key {i}");
      apiKeys.Add(apiKey);
    }

    // Assert
    var uniqueKeyValues = apiKeys.Select(k => k.KeyValue).Distinct().ToList();
    uniqueKeyValues.Should().HaveCount(keyCount,
        because: "all generated API keys should have unique key values");
  }

  #endregion

  #region GetApiKey Tests (by key value)

  [Fact]
  public async Task GetApiKey_WithValidActiveKey_ReturnsApiKeyWithUser()
  {
    // Arrange
    var apiKey = await _apiKeyService.CreateApiKey(_testUserId, "Test Key");

    // Act
    var result = await _apiKeyService.GetApiKey(apiKey.KeyValue);

    // Assert
    result.Should().NotBeNull(because: "valid active API key should be found");
    result!.Id.Should().Be(apiKey.Id, because: "returned API key should match the requested key");
    result.KeyValue.Should().Be(apiKey.KeyValue, because: "key value should match");
    result.User.Should().NotBeNull(because: "API key should include associated user data");
    result.User!.Id.Should().Be(_testUserId, because: "associated user should be correct");
  }

  [Fact]
  public async Task GetApiKey_WithInactiveKey_ReturnsNull()
  {
    // Arrange
    var apiKey = await _apiKeyService.CreateApiKey(_testUserId, "Test Key");
    await _apiKeyService.RevokeApiKey(apiKey.Id); // Make it inactive

    // Act
    var result = await _apiKeyService.GetApiKey(apiKey.KeyValue);

    // Assert
    result.Should().BeNull(because: "inactive API keys should not be retrievable");
  }

  [Fact]
  public async Task GetApiKey_WithNullKeyValue_ReturnsNull()
  {
    // Act
    var result = await _apiKeyService.GetApiKey((string)null!);

    // Assert
    result.Should().BeNull(because: "null key value should return null gracefully");
  }

  [Fact]
  public async Task GetApiKey_WithEmptyKeyValue_ReturnsNull()
  {
    // Act
    var result = await _apiKeyService.GetApiKey(string.Empty);

    // Assert
    result.Should().BeNull(because: "empty key value should return null gracefully");
  }

  [Fact]
  public async Task GetApiKey_WithNonExistentKeyValue_ReturnsNull()
  {
    // Act
    var result = await _apiKeyService.GetApiKey("non-existent-key");

    // Assert
    result.Should().BeNull(because: "non-existent API key should return null");
  }

  #endregion

  #region GetApiKey Tests (by ID)

  [Fact]
  public async Task GetApiKey_WithValidId_ReturnsApiKeyWithUser()
  {
    // Arrange
    var apiKey = await _apiKeyService.CreateApiKey(_testUserId, "Test Key");

    // Act
    var result = await _apiKeyService.GetApiKey(apiKey.Id);

    // Assert
    result.Should().NotBeNull(because: "valid API key ID should return the API key");
    result!.Id.Should().Be(apiKey.Id, because: "returned API key should have correct ID");
    result.User.Should().NotBeNull(because: "API key should include associated user data");
  }

  [Fact]
  public async Task GetApiKey_WithInvalidId_ReturnsNull()
  {
    // Act
    var result = await _apiKeyService.GetApiKey(999999);

    // Assert
    result.Should().BeNull(because: "invalid API key ID should return null");
  }

  #endregion

  #region GetApiKeysByUserId Tests

  [Fact]
  public async Task GetApiKeysByUserId_WithValidUserId_ReturnsUserApiKeys()
  {
    // Arrange
    var apiKey1 = await _apiKeyService.CreateApiKey(_testUserId, "Key 1");
    await Task.Delay(1); // Ensure different timestamps
    var apiKey2 = await _apiKeyService.CreateApiKey(_testUserId, "Key 2");

    // Act
    var result = await _apiKeyService.GetApiKeysByUserId(_testUserId);

    // Assert
    result.Should().HaveCount(2, because: "user should have two API keys");
    result.Should().BeInDescendingOrder(k => k.CreatedAt,
        because: "API keys should be ordered by creation date descending");
    result.Should().Contain(k => k.Id == apiKey1.Id, because: "result should include first API key");
    result.Should().Contain(k => k.Id == apiKey2.Id, because: "result should include second API key");
  }

  [Fact]
  public async Task GetApiKeysByUserId_WithNoApiKeys_ReturnsEmptyList()
  {
    // Act
    var result = await _apiKeyService.GetApiKeysByUserId(_testUserId);

    // Assert
    result.Should().BeEmpty(because: "user with no API keys should return empty list");
  }

  [Fact]
  public async Task GetApiKeysByUserId_WithNullUserId_ThrowsArgumentException()
  {
    // Act & Assert
    var act = async () => await _apiKeyService.GetApiKeysByUserId(null!);

    await act.Should().ThrowAsync<ArgumentException>()
        .WithParameterName("userId")
        .WithMessage("User ID cannot be empty*",
        because: "null user ID should be rejected");
  }

  [Fact]
  public async Task GetApiKeysByUserId_WithEmptyUserId_ThrowsArgumentException()
  {
    // Act & Assert
    var act = async () => await _apiKeyService.GetApiKeysByUserId(string.Empty);

    await act.Should().ThrowAsync<ArgumentException>()
        .WithParameterName("userId")
        .WithMessage("User ID cannot be empty*",
        because: "empty user ID should be rejected");
  }

  #endregion

  #region RevokeApiKey Tests

  [Fact]
  public async Task RevokeApiKey_WithValidId_RevokesApiKeyAndReturnsTrue()
  {
    // Arrange
    var apiKey = await _apiKeyService.CreateApiKey(_testUserId, "Test Key");

    // Act
    var result = await _apiKeyService.RevokeApiKey(apiKey.Id);

    // Assert
    result.Should().BeTrue(because: "valid API key should be revoked successfully");

    // Verify the API key is marked as inactive
    var revokedKey = await _dbContext.ApiKeys.FindAsync(apiKey.Id);
    revokedKey!.IsActive.Should().BeFalse(because: "revoked API key should be marked as inactive");
  }

  [Fact]
  public async Task RevokeApiKey_WithInvalidId_ReturnsFalse()
  {
    // Act
    var result = await _apiKeyService.RevokeApiKey(999999);

    // Assert
    result.Should().BeFalse(because: "invalid API key ID should return false");
  }

  [Fact]
  public async Task RevokeApiKey_WithAlreadyRevokedKey_ReturnsTrue()
  {
    // Arrange
    var apiKey = await _apiKeyService.CreateApiKey(_testUserId, "Test Key");
    await _apiKeyService.RevokeApiKey(apiKey.Id); // First revocation

    // Act
    var result = await _apiKeyService.RevokeApiKey(apiKey.Id); // Second revocation

    // Assert
    result.Should().BeTrue(because: "revoking an already revoked key should still return true");
  }

  #endregion

  #region ValidateApiKey Tests

  [Fact]
  public async Task ValidateApiKey_WithValidActiveKey_ReturnsValidAndUserId()
  {
    // Arrange
    var apiKey = await _apiKeyService.CreateApiKey(_testUserId, "Test Key");

    // Act
    var result = await _apiKeyService.ValidateApiKey(apiKey.KeyValue);

    // Assert
    result.IsValid.Should().BeTrue(because: "valid active API key should be validated successfully");
    result.UserId.Should().Be(_testUserId, because: "validation should return correct user ID");
  }

  [Fact]
  public async Task ValidateApiKey_WithInactiveKey_ReturnsInvalid()
  {
    // Arrange
    var apiKey = await _apiKeyService.CreateApiKey(_testUserId, "Test Key");
    await _apiKeyService.RevokeApiKey(apiKey.Id);

    // Act
    var result = await _apiKeyService.ValidateApiKey(apiKey.KeyValue);

    // Assert
    result.IsValid.Should().BeFalse(because: "inactive API key should be invalid");
    result.UserId.Should().BeNull(because: "invalid key should not return user ID");
  }

  [Fact]
  public async Task ValidateApiKey_WithExpiredKey_ReturnsInvalid()
  {
    // Arrange
    var expiredDate = DateTime.UtcNow.AddDays(-1);
    var apiKey = await _apiKeyService.CreateApiKey(_testUserId, "Expired Key", null, expiredDate);

    // Act
    var result = await _apiKeyService.ValidateApiKey(apiKey.KeyValue);

    // Assert
    result.IsValid.Should().BeFalse(because: "expired API key should be invalid");
    result.UserId.Should().BeNull(because: "expired key should not return user ID");
  }

  [Fact]
  public async Task ValidateApiKey_WithFutureExpirationKey_ReturnsValid()
  {
    // Arrange
    var futureDate = DateTime.UtcNow.AddDays(1);
    var apiKey = await _apiKeyService.CreateApiKey(_testUserId, "Future Key", null, futureDate);

    // Act
    var result = await _apiKeyService.ValidateApiKey(apiKey.KeyValue);

    // Assert
    result.IsValid.Should().BeTrue(because: "API key with future expiration should be valid");
    result.UserId.Should().Be(_testUserId, because: "valid key should return correct user ID");
  }

  [Fact]
  public async Task ValidateApiKey_WithNullKeyValue_ReturnsInvalid()
  {
    // Act
    var result = await _apiKeyService.ValidateApiKey(null!);

    // Assert
    result.IsValid.Should().BeFalse(because: "null key value should be invalid");
    result.UserId.Should().BeNull(because: "null key should not return user ID");
  }

  [Fact]
  public async Task ValidateApiKey_WithEmptyKeyValue_ReturnsInvalid()
  {
    // Act
    var result = await _apiKeyService.ValidateApiKey(string.Empty);

    // Assert
    result.IsValid.Should().BeFalse(because: "empty key value should be invalid");
    result.UserId.Should().BeNull(because: "empty key should not return user ID");
  }

  #endregion

  #region AuthenticateWithApiKey Tests

  [Fact]
  public async Task AuthenticateWithApiKey_WithValidKey_AuthenticatesAndReturnsTrue()
  {
    // Arrange
    var apiKey = await _apiKeyService.CreateApiKey(_testUserId, "Test Key");
    var context = CreateMockHttpContext();
    var testUser = new ApplicationUser
    {
      Id = _testUserId,
      Email = "test@example.com",
      UserName = "testuser"
    };

    _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
        .ReturnsAsync(testUser);
    _mockUserManager.Setup(um => um.GetRolesAsync(testUser))
        .ReturnsAsync(new List<string> { "User", "Admin" });

    // Act
    var result = await _apiKeyService.AuthenticateWithApiKey(context, apiKey.KeyValue, _mockLogger.Object);

    // Assert
    result.Should().BeTrue(because: "valid API key should authenticate successfully");
    context.User.Identity!.IsAuthenticated.Should().BeTrue(because: "user should be authenticated");
    context.User.Identity.AuthenticationType.Should().Be("ApiKey", because: "authentication type should be ApiKey");

    var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
    userIdClaim.Should().NotBeNull(because: "authenticated user should have user ID claim");
    userIdClaim!.Value.Should().Be(_testUserId, because: "user ID claim should be correct");

    var emailClaim = context.User.FindFirst(ClaimTypes.Email);
    emailClaim.Should().NotBeNull(because: "authenticated user should have email claim");
    emailClaim!.Value.Should().Be("test@example.com", because: "email claim should be correct");

    var roleClaims = context.User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();
    roleClaims.Should().Contain("User", because: "user should have User role");
    roleClaims.Should().Contain("Admin", because: "user should have Admin role");

    context.Items["ApiKey"].Should().Be(apiKey.KeyValue, because: "API key should be stored in context items");
    context.Items["ApiKeyUserId"].Should().Be(_testUserId, because: "user ID should be stored in context items");
  }

  [Fact]
  public async Task AuthenticateWithApiKey_WithInvalidKey_ReturnsUnauthorizedAndFalse()
  {
    // Arrange
    var context = CreateMockHttpContext();
    var invalidKey = "invalid-key";

    // Act
    var result = await _apiKeyService.AuthenticateWithApiKey(context, invalidKey, _mockLogger.Object);

    // Assert
    result.Should().BeFalse(because: "invalid API key should fail authentication");
    context.Response.StatusCode.Should().Be(401, because: "unauthorized status should be set");
    context.User.Identity!.IsAuthenticated.Should().BeFalse(because: "user should not be authenticated");
  }

  [Fact]
  public async Task AuthenticateWithApiKey_WithValidKeyButNonExistentUser_ReturnsUnauthorizedAndFalse()
  {
    // Arrange
    var apiKey = await _apiKeyService.CreateApiKey(_testUserId, "Test Key");
    var context = CreateMockHttpContext();

    _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
        .ReturnsAsync((ApplicationUser)null!);

    // Act
    var result = await _apiKeyService.AuthenticateWithApiKey(context, apiKey.KeyValue, _mockLogger.Object);

    // Assert
    result.Should().BeFalse(because: "API key with non-existent user should fail authentication");
    context.Response.StatusCode.Should().Be(401, because: "unauthorized status should be set");
  }

  [Fact]
  public async Task AuthenticateWithApiKey_WithUserManagerException_ReturnsInternalServerErrorAndFalse()
  {
    // Arrange
    var apiKey = await _apiKeyService.CreateApiKey(_testUserId, "Test Key");
    var context = CreateMockHttpContext();

    _mockUserManager.Setup(um => um.FindByIdAsync(_testUserId))
        .ThrowsAsync(new InvalidOperationException("Database error"));

    // Act
    var result = await _apiKeyService.AuthenticateWithApiKey(context, apiKey.KeyValue, _mockLogger.Object);

    // Assert
    result.Should().BeFalse(because: "authentication should fail when UserManager throws exception");
    context.Response.StatusCode.Should().Be(500, because: "internal server error status should be set");
  }

  #endregion

  #region Helper Methods

  private static HttpContext CreateMockHttpContext()
  {
    var context = new DefaultHttpContext();
    context.Request.Path = "/api/test";
    return context;
  }

  public void Dispose()
  {
    _dbContext?.Dispose();
  }

  #endregion
}
