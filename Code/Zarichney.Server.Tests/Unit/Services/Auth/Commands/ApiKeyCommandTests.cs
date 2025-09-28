using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Commands;
using Zarichney.Services.Auth.Models;

namespace Zarichney.Tests.Unit.Services.Auth.Commands;

/// <summary>
/// Unit tests for API key command and query handlers in the authentication system.
/// Tests CreateApiKeyCommandHandler, RevokeApiKeyCommandHandler, GetApiKeysQueryHandler,
/// GetApiKeyByIdQueryHandler, and ApiKeyResponse functionality to ensure proper API key
/// management operations with authentication validation and user authorization.
/// </summary>
public class ApiKeyCommandTests
{
  private readonly Mock<IApiKeyService> _mockApiKeyService;
  private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
  private readonly Mock<IHttpContextAccessor> _mockHttpContextAccessor;
  private readonly Mock<IRoleManager> _mockRoleManager;

  public ApiKeyCommandTests()
  {
    _mockApiKeyService = new Mock<IApiKeyService>();
    var store = new Mock<IUserStore<ApplicationUser>>();
    _mockUserManager = new Mock<UserManager<ApplicationUser>>(
        store.Object, null!, null!, null!, null!, null!, null!, null!, null!);
    _mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
    _mockRoleManager = new Mock<IRoleManager>();
  }

  #region CreateApiKeyCommandHandler Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CreateApiKeyCommandHandler_WithValidRequest_ReturnsApiKeyResponse()
  {
    // Arrange
    var userId = "test-user-123";
    var command = new CreateApiKeyCommand
    {
      Name = "Test API Key",
      Description = "Test Description",
      ExpiresAt = DateTime.UtcNow.AddDays(30)
    };

    var apiKey = new ApiKey
    {
      Id = 1,
      UserId = userId,
      KeyValue = "test-key-value",
      Name = command.Name,
      Description = command.Description,
      CreatedAt = DateTime.UtcNow,
      ExpiresAt = command.ExpiresAt,
      IsActive = true
    };

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

    _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
    _mockApiKeyService.Setup(x => x.CreateApiKey(userId, command.Name, command.Description, command.ExpiresAt))
        .ReturnsAsync(apiKey);

    var handler = new CreateApiKeyCommandHandler(_mockApiKeyService.Object, _mockUserManager.Object, _mockHttpContextAccessor.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull("because a valid API key response should be returned");
    result.Id.Should().Be(apiKey.Id);
    result.KeyValue.Should().Be(apiKey.KeyValue);
    result.Name.Should().Be(apiKey.Name);
    result.Description.Should().Be(apiKey.Description);
    result.ExpiresAt.Should().Be(apiKey.ExpiresAt);
    result.IsActive.Should().BeTrue();

    _mockApiKeyService.Verify(x => x.CreateApiKey(userId, command.Name, command.Description, command.ExpiresAt), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CreateApiKeyCommandHandler_WithUnauthenticatedUser_ThrowsUnauthorizedAccessException()
  {
    // Arrange
    var command = new CreateApiKeyCommand
    {
      Name = "Test API Key"
    };

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

    _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

    var handler = new CreateApiKeyCommandHandler(_mockApiKeyService.Object, _mockUserManager.Object, _mockHttpContextAccessor.Object);

    // Act
    var act = () => handler.Handle(command, CancellationToken.None);

    // Assert
    await act.Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("User is not authenticated", "because unauthenticated users cannot create API keys");

    _mockApiKeyService.Verify(x => x.CreateApiKey(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<DateTime?>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CreateApiKeyCommandHandler_WithoutDescription_CreatesApiKeySuccessfully()
  {
    // Arrange
    var userId = "test-user-123";
    var command = new CreateApiKeyCommand
    {
      Name = "Test API Key",
      Description = null,
      ExpiresAt = null
    };

    var apiKey = new ApiKey
    {
      Id = 2,
      UserId = userId,
      KeyValue = "test-key-value-2",
      Name = command.Name,
      Description = null,
      CreatedAt = DateTime.UtcNow,
      ExpiresAt = null,
      IsActive = true
    };

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

    _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
    _mockApiKeyService.Setup(x => x.CreateApiKey(userId, command.Name, null, null))
        .ReturnsAsync(apiKey);

    var handler = new CreateApiKeyCommandHandler(_mockApiKeyService.Object, _mockUserManager.Object, _mockHttpContextAccessor.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Description.Should().BeNull("because no description was provided");
    result.ExpiresAt.Should().BeNull("because no expiration was set");
  }

  #endregion

  #region RevokeApiKeyCommandHandler Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RevokeApiKeyCommandHandler_WithValidApiKey_ReturnsTrue()
  {
    // Arrange
    var userId = "test-user-123";
    var apiKeyId = 1;
    var command = new RevokeApiKeyCommand(apiKeyId);

    var apiKey = new ApiKey
    {
      Id = apiKeyId,
      UserId = userId,
      KeyValue = "test-key-value",
      Name = "Test Key",
      IsActive = true
    };

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

    _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
    _mockApiKeyService.Setup(x => x.GetApiKey(apiKeyId)).ReturnsAsync(apiKey);
    _mockApiKeyService.Setup(x => x.RevokeApiKey(apiKeyId)).ReturnsAsync(true);

    var handler = new RevokeApiKeyCommandHandler(_mockApiKeyService.Object, _mockUserManager.Object, _mockHttpContextAccessor.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeTrue("because the API key was successfully revoked");
    _mockApiKeyService.Verify(x => x.GetApiKey(apiKeyId), Times.Once);
    _mockApiKeyService.Verify(x => x.RevokeApiKey(apiKeyId), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RevokeApiKeyCommandHandler_WithNonExistentApiKey_ReturnsFalse()
  {
    // Arrange
    var userId = "test-user-123";
    var apiKeyId = 999;
    var command = new RevokeApiKeyCommand(apiKeyId);

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

    _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
    _mockApiKeyService.Setup(x => x.GetApiKey(apiKeyId)).ReturnsAsync((ApiKey?)null);

    var handler = new RevokeApiKeyCommandHandler(_mockApiKeyService.Object, _mockUserManager.Object, _mockHttpContextAccessor.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeFalse("because the API key does not exist");
    _mockApiKeyService.Verify(x => x.GetApiKey(apiKeyId), Times.Once);
    _mockApiKeyService.Verify(x => x.RevokeApiKey(It.IsAny<int>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RevokeApiKeyCommandHandler_WithApiKeyBelongingToAnotherUser_ReturnsFalse()
  {
    // Arrange
    var userId = "test-user-123";
    var otherUserId = "other-user-456";
    var apiKeyId = 1;
    var command = new RevokeApiKeyCommand(apiKeyId);

    var apiKey = new ApiKey
    {
      Id = apiKeyId,
      UserId = otherUserId, // Different user
      KeyValue = "test-key-value",
      Name = "Test Key",
      IsActive = true
    };

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

    _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
    _mockApiKeyService.Setup(x => x.GetApiKey(apiKeyId)).ReturnsAsync(apiKey);

    var handler = new RevokeApiKeyCommandHandler(_mockApiKeyService.Object, _mockUserManager.Object, _mockHttpContextAccessor.Object);

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().BeFalse("because the API key belongs to another user");
    _mockApiKeyService.Verify(x => x.GetApiKey(apiKeyId), Times.Once);
    _mockApiKeyService.Verify(x => x.RevokeApiKey(It.IsAny<int>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RevokeApiKeyCommandHandler_WithUnauthenticatedUser_ThrowsUnauthorizedAccessException()
  {
    // Arrange
    var command = new RevokeApiKeyCommand(1);

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

    _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

    var handler = new RevokeApiKeyCommandHandler(_mockApiKeyService.Object, _mockUserManager.Object, _mockHttpContextAccessor.Object);

    // Act
    var act = () => handler.Handle(command, CancellationToken.None);

    // Assert
    await act.Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("User is not authenticated");

    _mockApiKeyService.Verify(x => x.GetApiKey(It.IsAny<int>()), Times.Never);
  }

  #endregion

  #region GetApiKeysQueryHandler Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetApiKeysQueryHandler_WithValidUser_ReturnsApiKeysList()
  {
    // Arrange
    var userId = "test-user-123";
    var query = new GetApiKeysQuery();

    List<ApiKey> apiKeys = [
        new() { Id = 1, UserId = userId, KeyValue = "key1", Name = "Key 1", IsActive = true, CreatedAt = DateTime.UtcNow },
            new() { Id = 2, UserId = userId, KeyValue = "key2", Name = "Key 2", IsActive = false, CreatedAt = DateTime.UtcNow.AddDays(-1) }
    ];

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

    _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
    _mockApiKeyService.Setup(x => x.GetApiKeysByUserId(userId)).ReturnsAsync(apiKeys);

    var handler = new GetApiKeysQueryHandler(_mockApiKeyService.Object, _mockUserManager.Object, _mockHttpContextAccessor.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Should().HaveCount(2, "because the user has 2 API keys");
    result[0].Id.Should().Be(1);
    result[0].Name.Should().Be("Key 1");
    result[0].IsActive.Should().BeTrue();
    result[1].Id.Should().Be(2);
    result[1].Name.Should().Be("Key 2");
    result[1].IsActive.Should().BeFalse();

    _mockApiKeyService.Verify(x => x.GetApiKeysByUserId(userId), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetApiKeysQueryHandler_WithNoApiKeys_ReturnsEmptyList()
  {
    // Arrange
    var userId = "test-user-123";
    var query = new GetApiKeysQuery();

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

    _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
    _mockApiKeyService.Setup(x => x.GetApiKeysByUserId(userId)).ReturnsAsync([]);

    var handler = new GetApiKeysQueryHandler(_mockApiKeyService.Object, _mockUserManager.Object, _mockHttpContextAccessor.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Should().BeEmpty("because the user has no API keys");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetApiKeysQueryHandler_WithUnauthenticatedUser_ThrowsUnauthorizedAccessException()
  {
    // Arrange
    var query = new GetApiKeysQuery();

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

    _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

    var handler = new GetApiKeysQueryHandler(_mockApiKeyService.Object, _mockUserManager.Object, _mockHttpContextAccessor.Object);

    // Act
    var act = () => handler.Handle(query, CancellationToken.None);

    // Assert
    await act.Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("User is not authenticated");

    _mockApiKeyService.Verify(x => x.GetApiKeysByUserId(It.IsAny<string>()), Times.Never);
  }

  #endregion

  #region GetApiKeyByIdQueryHandler Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetApiKeyByIdQueryHandler_WithValidApiKey_ReturnsApiKeyResponse()
  {
    // Arrange
    var userId = "test-user-123";
    var apiKeyId = 1;
    var query = new GetApiKeyByIdQuery(apiKeyId);

    var apiKey = new ApiKey
    {
      Id = apiKeyId,
      UserId = userId,
      KeyValue = "test-key-value",
      Name = "Test Key",
      IsActive = true,
      CreatedAt = DateTime.UtcNow
    };

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

    _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
    _mockApiKeyService.Setup(x => x.GetApiKey(apiKeyId)).ReturnsAsync(apiKey);

    var handler = new GetApiKeyByIdQueryHandler(_mockApiKeyService.Object, _mockUserManager.Object, _mockHttpContextAccessor.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result!.Id.Should().Be(apiKeyId);
    result.Name.Should().Be("Test Key");
    result.IsActive.Should().BeTrue();

    _mockApiKeyService.Verify(x => x.GetApiKey(apiKeyId), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetApiKeyByIdQueryHandler_WithNonExistentApiKey_ReturnsNull()
  {
    // Arrange
    var userId = "test-user-123";
    var apiKeyId = 999;
    var query = new GetApiKeyByIdQuery(apiKeyId);

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

    _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
    _mockApiKeyService.Setup(x => x.GetApiKey(apiKeyId)).ReturnsAsync((ApiKey?)null);

    var handler = new GetApiKeyByIdQueryHandler(_mockApiKeyService.Object, _mockUserManager.Object, _mockHttpContextAccessor.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().BeNull("because the API key does not exist");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetApiKeyByIdQueryHandler_WithApiKeyBelongingToAnotherUser_ReturnsNull()
  {
    // Arrange
    var userId = "test-user-123";
    var otherUserId = "other-user-456";
    var apiKeyId = 1;
    var query = new GetApiKeyByIdQuery(apiKeyId);

    var apiKey = new ApiKey
    {
      Id = apiKeyId,
      UserId = otherUserId, // Different user
      KeyValue = "test-key-value",
      Name = "Test Key",
      IsActive = true
    };

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
    {
            new Claim(ClaimTypes.NameIdentifier, userId)
        }));

    _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns(userId);
    _mockApiKeyService.Setup(x => x.GetApiKey(apiKeyId)).ReturnsAsync(apiKey);

    var handler = new GetApiKeyByIdQueryHandler(_mockApiKeyService.Object, _mockUserManager.Object, _mockHttpContextAccessor.Object);

    // Act
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert
    result.Should().BeNull("because the API key belongs to another user");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetApiKeyByIdQueryHandler_WithUnauthenticatedUser_ThrowsUnauthorizedAccessException()
  {
    // Arrange
    var query = new GetApiKeyByIdQuery(1);

    var httpContext = new DefaultHttpContext();
    httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

    _mockHttpContextAccessor.Setup(x => x.HttpContext).Returns(httpContext);
    _mockUserManager.Setup(x => x.GetUserId(It.IsAny<ClaimsPrincipal>())).Returns((string?)null);

    var handler = new GetApiKeyByIdQueryHandler(_mockApiKeyService.Object, _mockUserManager.Object, _mockHttpContextAccessor.Object);

    // Act
    var act = () => handler.Handle(query, CancellationToken.None);

    // Assert
    await act.Should().ThrowAsync<UnauthorizedAccessException>()
        .WithMessage("User is not authenticated");

    _mockApiKeyService.Verify(x => x.GetApiKey(It.IsAny<int>()), Times.Never);
  }

  #endregion

  #region ApiKeyResponse Tests

  [Fact]
  [Trait("Category", "Unit")]
  public void ApiKeyResponse_FromApiKey_MapsAllPropertiesCorrectly()
  {
    // Arrange
    var apiKey = new ApiKey
    {
      Id = 123,
      KeyValue = "test-key-value-123",
      Name = "Production API Key",
      CreatedAt = new DateTime(2024, 1, 15, 12, 30, 45),
      ExpiresAt = new DateTime(2025, 1, 15, 12, 30, 45),
      IsActive = true,
      Description = "Used for production API access"
    };

    // Act
    var response = ApiKeyResponse.FromApiKey(apiKey);

    // Assert
    response.Should().NotBeNull();
    response.Id.Should().Be(apiKey.Id);
    response.KeyValue.Should().Be(apiKey.KeyValue);
    response.Name.Should().Be(apiKey.Name);
    response.CreatedAt.Should().Be(apiKey.CreatedAt);
    response.ExpiresAt.Should().Be(apiKey.ExpiresAt);
    response.IsActive.Should().Be(apiKey.IsActive);
    response.Description.Should().Be(apiKey.Description);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ApiKeyResponse_FromApiKey_WithNullOptionalFields_MapsCorrectly()
  {
    // Arrange
    var apiKey = new ApiKey
    {
      Id = 456,
      KeyValue = "minimal-key",
      Name = "Minimal Key",
      CreatedAt = DateTime.UtcNow,
      ExpiresAt = null,
      IsActive = false,
      Description = null
    };

    // Act
    var response = ApiKeyResponse.FromApiKey(apiKey);

    // Assert
    response.Should().NotBeNull();
    response.ExpiresAt.Should().BeNull("because no expiration was set");
    response.Description.Should().BeNull("because no description was provided");
    response.IsActive.Should().BeFalse("because the key is inactive");
  }

  #endregion
}
