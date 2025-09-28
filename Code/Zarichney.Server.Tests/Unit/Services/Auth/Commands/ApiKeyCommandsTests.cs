using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System.Security.Claims;
using Xunit;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Commands;
using Zarichney.Services.Auth.Models;
using Zarichney.Tests.Framework.Mocks.Factories;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Services.Auth.Commands;

/// <summary>
/// Comprehensive unit tests for API Key Commands covering all CQRS handlers.
/// Tests MediatR command/query handlers with complete isolation, proper mocking, and framework-first approach.
/// Ensures authentication, authorization, validation, and error handling scenarios are thoroughly covered.
/// </summary>
[Trait("Category", "Unit")]
public class ApiKeyCommandsTests
{
  private readonly Mock<IApiKeyService> _apiKeyServiceMock;
  private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
  private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
  private readonly Mock<HttpContext> _httpContextMock;
  private readonly string _testUserId = "test-user-123";
  private readonly string _otherUserId = "other-user-456";

  public ApiKeyCommandsTests()
  {
    _apiKeyServiceMock = new Mock<IApiKeyService>();
    _userManagerMock = UserManagerMockFactory.CreateDefault();
    _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
    _httpContextMock = new Mock<HttpContext>();

    // Setup HTTP context with authenticated user
    var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, _testUserId),
            new Claim(ClaimTypes.Email, "test@example.com"),
            new Claim(ClaimTypes.Name, "testuser")
        }, "ApiKey"));

    _httpContextMock.Setup(x => x.User).Returns(user);
    _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(_httpContextMock.Object);
  }

  #region CreateApiKeyCommandHandler Tests

  [Fact]
  public async Task CreateApiKeyCommand_WithValidRequest_CreatesApiKeySuccessfully()
  {
    // Arrange
    var command = new CreateApiKeyCommand
    {
      Name = "Test API Key",
      Description = "Test description for API key",
      ExpiresAt = DateTime.UtcNow.AddDays(30)
    };

    var createdApiKey = new ApiKeyBuilder()
        .WithId(1)
        .WithName(command.Name)
        .WithDescription(command.Description)
        .WithExpiresAt(command.ExpiresAt)
        .WithUserId(_testUserId)
        .WithIsActive(true)
        .Build();

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(_testUserId);

    _apiKeyServiceMock.Setup(s => s.CreateApiKey(_testUserId, command.Name, command.Description, command.ExpiresAt))
        .ReturnsAsync(createdApiKey);

    var handler = new CreateApiKeyCommandHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull(because: "valid command should return a response");
    result.Id.Should().Be(createdApiKey.Id, because: "response should include the created API key ID");
    result.Name.Should().Be(command.Name, because: "response should preserve the API key name");
    result.Description.Should().Be(command.Description, because: "response should preserve the API key description");
    result.ExpiresAt.Should().Be(command.ExpiresAt, because: "response should preserve the expiration date");
    result.IsActive.Should().BeTrue(because: "new API key should be active by default");
    result.KeyValue.Should().Be(createdApiKey.KeyValue, because: "response should include the generated key value");

    _apiKeyServiceMock.Verify(s => s.CreateApiKey(_testUserId, command.Name, command.Description, command.ExpiresAt), Times.Once);
    _userManagerMock.Verify(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()), Times.Once);
  }

  [Fact]
  public async Task CreateApiKeyCommand_WithMinimalData_CreatesApiKeyWithDefaults()
  {
    // Arrange
    var command = new CreateApiKeyCommand
    {
      Name = "Minimal API Key"
      // No description or expiration
    };

    var createdApiKey = new ApiKeyBuilder()
        .WithId(2)
        .WithName(command.Name)
        .WithDescription(null)
        .WithExpiresAt(null)
        .WithUserId(_testUserId)
        .Build();

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(_testUserId);

    _apiKeyServiceMock.Setup(s => s.CreateApiKey(_testUserId, command.Name, null, null))
        .ReturnsAsync(createdApiKey);

    var handler = new CreateApiKeyCommandHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull(because: "minimal command should still create API key");
    result.Name.Should().Be(command.Name, because: "name should be preserved");
    result.Description.Should().BeNull(because: "description should be null when not provided");
    result.ExpiresAt.Should().BeNull(because: "expiration should be null when not provided");
  }

  [Fact]
  public async Task CreateApiKeyCommand_WithUnauthenticatedUser_ThrowsUnauthorizedAccessException()
  {
    // Arrange
    var command = new CreateApiKeyCommand { Name = "Test Key" };

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns((string)null!);

    var handler = new CreateApiKeyCommandHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act & Assert
    var act = async () => await handler.Handle(command, CancellationToken.None);

    await act.Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("User is not authenticated",
        because: "unauthenticated users should not be able to create API keys");

    _apiKeyServiceMock.Verify(s => s.CreateApiKey(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()), Times.Never);
  }

  [Fact]
  public async Task CreateApiKeyCommand_WithEmptyUserId_ThrowsUnauthorizedAccessException()
  {
    // Arrange
    var command = new CreateApiKeyCommand { Name = "Test Key" };

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(string.Empty);

    var handler = new CreateApiKeyCommandHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act & Assert
    var act = async () => await handler.Handle(command, CancellationToken.None);

    await act.Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("User is not authenticated",
        because: "empty user ID should be treated as unauthenticated");
  }

  [Fact]
  public async Task CreateApiKeyCommand_WhenServiceThrowsException_PropagatesException()
  {
    // Arrange
    var command = new CreateApiKeyCommand { Name = "Test Key" };

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(_testUserId);

    _apiKeyServiceMock.Setup(s => s.CreateApiKey(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<DateTime?>()))
        .ThrowsAsync(new InvalidOperationException("Database connection failed"));

    var handler = new CreateApiKeyCommandHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act & Assert
    var act = async () => await handler.Handle(command, CancellationToken.None);

    await act.Should().ThrowAsync<InvalidOperationException>()
        .WithMessage("Database connection failed",
        because: "service exceptions should be propagated to caller");
  }

  #endregion

  #region RevokeApiKeyCommandHandler Tests

  [Fact]
  public async Task RevokeApiKeyCommand_WithValidOwnedKey_RevokesSuccessfully()
  {
    // Arrange
    var command = new RevokeApiKeyCommand(1);
    var apiKey = new ApiKeyBuilder()
        .WithId(1)
        .WithUserId(_testUserId)
        .WithIsActive(true)
        .Build();

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(_testUserId);

    _apiKeyServiceMock.Setup(s => s.GetApiKey(command.ApiKeyId))
        .ReturnsAsync(apiKey);

    _apiKeyServiceMock.Setup(s => s.RevokeApiKey(command.ApiKeyId))
        .ReturnsAsync(true);

    var handler = new RevokeApiKeyCommandHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeTrue(because: "user should be able to revoke their own API key");

    _apiKeyServiceMock.Verify(s => s.GetApiKey(command.ApiKeyId), Times.Once);
    _apiKeyServiceMock.Verify(s => s.RevokeApiKey(command.ApiKeyId), Times.Once);
  }

  [Fact]
  public async Task RevokeApiKeyCommand_WithNonExistentKey_ReturnsFalse()
  {
    // Arrange
    var command = new RevokeApiKeyCommand(999);

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(_testUserId);

    _apiKeyServiceMock.Setup(s => s.GetApiKey(command.ApiKeyId))
        .ReturnsAsync((ApiKey)null!);

    var handler = new RevokeApiKeyCommandHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeFalse(because: "non-existent API key cannot be revoked");

    _apiKeyServiceMock.Verify(s => s.RevokeApiKey(It.IsAny<int>()), Times.Never);
  }

  [Fact]
  public async Task RevokeApiKeyCommand_WithOtherUserKey_ReturnsFalse()
  {
    // Arrange
    var command = new RevokeApiKeyCommand(1);
    var otherUserApiKey = new ApiKeyBuilder()
        .WithId(1)
        .WithUserId(_otherUserId) // Different user
        .WithIsActive(true)
        .Build();

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(_testUserId);

    _apiKeyServiceMock.Setup(s => s.GetApiKey(command.ApiKeyId))
        .ReturnsAsync(otherUserApiKey);

    var handler = new RevokeApiKeyCommandHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeFalse(because: "users should not be able to revoke other users' API keys");

    _apiKeyServiceMock.Verify(s => s.RevokeApiKey(It.IsAny<int>()), Times.Never);
  }

  [Fact]
  public async Task RevokeApiKeyCommand_WithUnauthenticatedUser_ThrowsUnauthorizedAccessException()
  {
    // Arrange
    var command = new RevokeApiKeyCommand(1);

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns((string)null!);

    var handler = new RevokeApiKeyCommandHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act & Assert
    var act = async () => await handler.Handle(command, CancellationToken.None);

    await act.Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("User is not authenticated",
        because: "unauthenticated users should not be able to revoke API keys");
  }

  [Fact]
  public async Task RevokeApiKeyCommand_WhenRevocationFails_ReturnsFalse()
  {
    // Arrange
    var command = new RevokeApiKeyCommand(1);
    var apiKey = new ApiKeyBuilder()
        .WithId(1)
        .WithUserId(_testUserId)
        .Build();

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(_testUserId);

    _apiKeyServiceMock.Setup(s => s.GetApiKey(command.ApiKeyId))
        .ReturnsAsync(apiKey);

    _apiKeyServiceMock.Setup(s => s.RevokeApiKey(command.ApiKeyId))
        .ReturnsAsync(false);

    var handler = new RevokeApiKeyCommandHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeFalse(because: "revocation failure should be reflected in the result");
  }

  #endregion

  #region GetApiKeysQueryHandler Tests

  [Fact]
  public async Task GetApiKeysQuery_WithAuthenticatedUser_ReturnsUserApiKeys()
  {
    // Arrange
    var query = new GetApiKeysQuery();
    var userApiKeys = new List<ApiKey>
        {
            new ApiKeyBuilder().WithId(1).WithName("Key 1").WithUserId(_testUserId).Build(),
            new ApiKeyBuilder().WithId(2).WithName("Key 2").WithUserId(_testUserId).Build()
        };

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(_testUserId);

    _apiKeyServiceMock.Setup(s => s.GetApiKeysByUserId(_testUserId))
        .ReturnsAsync(userApiKeys);

    var handler = new GetApiKeysQueryHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull(because: "query should return a list");
    result.Should().HaveCount(2, because: "user has two API keys");
    result.Should().OnlyContain(r => r.Id == 1 || r.Id == 2, because: "result should contain the correct API keys");
    result.Should().OnlyContain(r => r.Name == "Key 1" || r.Name == "Key 2", because: "names should be preserved");

    _apiKeyServiceMock.Verify(s => s.GetApiKeysByUserId(_testUserId), Times.Once);
  }

  [Fact]
  public async Task GetApiKeysQuery_WithNoApiKeys_ReturnsEmptyList()
  {
    // Arrange
    var query = new GetApiKeysQuery();

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(_testUserId);

    _apiKeyServiceMock.Setup(s => s.GetApiKeysByUserId(_testUserId))
        .ReturnsAsync(new List<ApiKey>());

    var handler = new GetApiKeysQueryHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull(because: "query should always return a list");
    result.Should().BeEmpty(because: "user has no API keys");
  }

  [Fact]
  public async Task GetApiKeysQuery_WithUnauthenticatedUser_ThrowsUnauthorizedAccessException()
  {
    // Arrange
    var query = new GetApiKeysQuery();

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns((string)null!);

    var handler = new GetApiKeysQueryHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act & Assert
    var act = async () => await handler.Handle(query, CancellationToken.None);

    await act.Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("User is not authenticated",
        because: "unauthenticated users should not be able to query API keys");
  }

  [Fact]
  public async Task GetApiKeysQuery_VerifiesApiKeyResponseMapping()
  {
    // Arrange
    var query = new GetApiKeysQuery();
    var testApiKey = new ApiKeyBuilder()
        .WithId(1)
        .WithName("Test Key")
        .WithDescription("Test Description")
        .WithKeyValue("test-key-value")
        .WithUserId(_testUserId)
        .WithIsActive(true)
        .WithCreatedAt(DateTime.UtcNow.AddDays(-1))
        .WithExpiresAt(DateTime.UtcNow.AddDays(30))
        .Build();

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(_testUserId);

    _apiKeyServiceMock.Setup(s => s.GetApiKeysByUserId(_testUserId))
        .ReturnsAsync(new List<ApiKey> { testApiKey });

    var handler = new GetApiKeysQueryHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    var response = result.First();
    response.Id.Should().Be(testApiKey.Id, because: "ID should be mapped correctly");
    response.Name.Should().Be(testApiKey.Name, because: "name should be mapped correctly");
    response.Description.Should().Be(testApiKey.Description, because: "description should be mapped correctly");
    response.KeyValue.Should().Be(testApiKey.KeyValue, because: "key value should be mapped correctly");
    response.IsActive.Should().Be(testApiKey.IsActive, because: "active status should be mapped correctly");
    response.CreatedAt.Should().Be(testApiKey.CreatedAt, because: "creation date should be mapped correctly");
    response.ExpiresAt.Should().Be(testApiKey.ExpiresAt, because: "expiration date should be mapped correctly");
  }

  #endregion

  #region GetApiKeyByIdQueryHandler Tests

  [Fact]
  public async Task GetApiKeyByIdQuery_WithValidOwnedKey_ReturnsApiKey()
  {
    // Arrange
    var query = new GetApiKeyByIdQuery(1);
    var apiKey = new ApiKeyBuilder()
        .WithId(1)
        .WithUserId(_testUserId)
        .WithName("Test Key")
        .Build();

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(_testUserId);

    _apiKeyServiceMock.Setup(s => s.GetApiKey(query.ApiKeyId))
        .ReturnsAsync(apiKey);

    var handler = new GetApiKeyByIdQueryHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull(because: "owned API key should be returned");
    result!.Id.Should().Be(query.ApiKeyId, because: "returned API key should have correct ID");
    result.Name.Should().Be("Test Key", because: "API key properties should be mapped correctly");
  }

  [Fact]
  public async Task GetApiKeyByIdQuery_WithNonExistentKey_ReturnsNull()
  {
    // Arrange
    var query = new GetApiKeyByIdQuery(999);

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(_testUserId);

    _apiKeyServiceMock.Setup(s => s.GetApiKey(query.ApiKeyId))
        .ReturnsAsync((ApiKey)null!);

    var handler = new GetApiKeyByIdQueryHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().BeNull(because: "non-existent API key should return null");
  }

  [Fact]
  public async Task GetApiKeyByIdQuery_WithOtherUserKey_ReturnsNull()
  {
    // Arrange
    var query = new GetApiKeyByIdQuery(1);
    var otherUserApiKey = new ApiKeyBuilder()
        .WithId(1)
        .WithUserId(_otherUserId) // Different user
        .Build();

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(_testUserId);

    _apiKeyServiceMock.Setup(s => s.GetApiKey(query.ApiKeyId))
        .ReturnsAsync(otherUserApiKey);

    var handler = new GetApiKeyByIdQueryHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().BeNull(because: "users should not be able to access other users' API keys");
  }

  [Fact]
  public async Task GetApiKeyByIdQuery_WithUnauthenticatedUser_ThrowsUnauthorizedAccessException()
  {
    // Arrange
    var query = new GetApiKeyByIdQuery(1);

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns((string)null!);

    var handler = new GetApiKeyByIdQueryHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act & Assert
    var act = async () => await handler.Handle(query, CancellationToken.None);

    await act.Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("User is not authenticated",
        because: "unauthenticated users should not be able to query API keys");
  }

  [Fact]
  public async Task GetApiKeyByIdQuery_VerifiesCompleteApiKeyResponseMapping()
  {
    // Arrange
    var query = new GetApiKeyByIdQuery(1);
    var testApiKey = new ApiKeyBuilder()
        .WithId(1)
        .WithName("Complete Test Key")
        .WithDescription("Complete test description")
        .WithKeyValue("complete-test-key-value")
        .WithUserId(_testUserId)
        .WithIsActive(false) // Test inactive key
        .WithCreatedAt(new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc))
        .WithExpiresAt(new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc))
        .Build();

    _userManagerMock.Setup(um => um.GetUserId(It.IsAny<ClaimsPrincipal>()))
        .Returns(_testUserId);

    _apiKeyServiceMock.Setup(s => s.GetApiKey(query.ApiKeyId))
        .ReturnsAsync(testApiKey);

    var handler = new GetApiKeyByIdQueryHandler(_apiKeyServiceMock.Object, _userManagerMock.Object, _httpContextAccessorMock.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull(because: "owned API key should be returned");
    result!.Id.Should().Be(1, because: "ID should be mapped correctly");
    result.Name.Should().Be("Complete Test Key", because: "name should be mapped correctly");
    result.Description.Should().Be("Complete test description", because: "description should be mapped correctly");
    result.KeyValue.Should().Be("complete-test-key-value", because: "key value should be mapped correctly");
    result.IsActive.Should().BeFalse(because: "active status should be mapped correctly");
    result.CreatedAt.Should().Be(new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc), because: "creation date should be mapped correctly");
    result.ExpiresAt.Should().Be(new DateTime(2024, 12, 31, 23, 59, 59, DateTimeKind.Utc), because: "expiration date should be mapped correctly");
  }

  #endregion

  #region ApiKeyResponse Static Method Tests

  [Fact]
  public void ApiKeyResponse_FromApiKey_MapsAllPropertiesCorrectly()
  {
    // Arrange
    var apiKey = new ApiKeyBuilder()
        .WithId(42)
        .WithName("Mapping Test Key")
        .WithDescription("Test mapping description")
        .WithKeyValue("mapping-test-key-value")
        .WithIsActive(true)
        .WithCreatedAt(new DateTime(2023, 6, 15, 10, 30, 0, DateTimeKind.Utc))
        .WithExpiresAt(new DateTime(2023, 12, 15, 10, 30, 0, DateTimeKind.Utc))
        .Build();

    // Act
    var response = ApiKeyResponse.FromApiKey(apiKey);

    // Assert
    response.Should().NotBeNull(because: "mapping should produce a valid response");
    response.Id.Should().Be(42, because: "ID should be mapped correctly");
    response.Name.Should().Be("Mapping Test Key", because: "name should be mapped correctly");
    response.Description.Should().Be("Test mapping description", because: "description should be mapped correctly");
    response.KeyValue.Should().Be("mapping-test-key-value", because: "key value should be mapped correctly");
    response.IsActive.Should().BeTrue(because: "active status should be mapped correctly");
    response.CreatedAt.Should().Be(new DateTime(2023, 6, 15, 10, 30, 0, DateTimeKind.Utc), because: "creation date should be mapped correctly");
    response.ExpiresAt.Should().Be(new DateTime(2023, 12, 15, 10, 30, 0, DateTimeKind.Utc), because: "expiration date should be mapped correctly");
  }

  [Fact]
  public void ApiKeyResponse_FromApiKey_WithNullableFields_MapsCorrectly()
  {
    // Arrange
    var apiKey = new ApiKeyBuilder()
        .WithId(1)
        .WithName("Minimal Key")
        .WithDescription(null)
        .WithExpiresAt(null)
        .Build();

    // Act
    var response = ApiKeyResponse.FromApiKey(apiKey);

    // Assert
    response.Should().NotBeNull(because: "mapping should handle nullable fields correctly");
    response.Description.Should().BeNull(because: "null description should be mapped as null");
    response.ExpiresAt.Should().BeNull(because: "null expiration should be mapped as null");
    response.Name.Should().Be("Minimal Key", because: "required fields should still be mapped");
  }

  #endregion
}
