using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Controllers;
using Zarichney.Server.Tests.Framework.Mocks;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Commands;
using Zarichney.Services.Auth.Models;
using ControllersAuthController = Zarichney.Controllers.AuthController;

namespace Zarichney.Server.Tests.Unit.Controllers.AuthControllerTests;

public class AuthControllerApiKeyTests
{
  private readonly Mock<IMediator> _mockMediator;
  private readonly Mock<ILogger<ControllersAuthController>> _mockLogger;
  private readonly Mock<ICookieAuthManager> _mockCookieManager;
  private readonly ControllersAuthController _sut;

  public AuthControllerApiKeyTests()
  {
    _mockMediator = new Mock<IMediator>();
    _mockLogger = new Mock<ILogger<ControllersAuthController>>();
    _mockCookieManager = CookieAuthManagerMockFactory.CreateDefault();

    _sut = new ControllersAuthController(_mockMediator.Object, _mockLogger.Object, _mockCookieManager.Object)
    {
      ControllerContext = new ControllerContext
      {
        HttpContext = new DefaultHttpContext()
      }
    };
  }

  #region CreateApiKey Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CreateApiKey_ValidRequest_ReturnsOkWithApiKey()
  {
    // Arrange
    var command = new CreateApiKeyCommand
    {
      Name = "Test API Key",
      Description = "Test API Key Description",
      ExpiresAt = DateTime.UtcNow.AddDays(30)
    };

    var expectedResponse = new ApiKeyResponse
    {
      Id = 1,
      KeyValue = "generated-api-key-123",
      Name = command.Name,
      Description = command.Description,
      CreatedAt = DateTime.UtcNow,
      ExpiresAt = command.ExpiresAt,
      IsActive = true
    };

    _mockMediator.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedResponse);

    // Act
    var result = await _sut.CreateApiKey(command);

    // Assert
    result.Should().BeOfType<OkObjectResult>();
    var okResult = result as OkObjectResult;
    var response = okResult!.Value as ApiKeyResponse;

    response.Should().NotBeNull();
    response!.Id.Should().Be(1);
    response.KeyValue.Should().Be("generated-api-key-123");
    response.Description.Should().Be(command.Description);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task CreateApiKey_ExceptionThrown_ReturnsApiErrorResult()
  {
    // Arrange
    var command = new CreateApiKeyCommand
    {
      Name = "Test API Key",
      Description = "Test description"
    };

    _mockMediator.Setup(x => x.Send(command, It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Database error"));

    // Act
    var result = await _sut.CreateApiKey(command);

    // Assert
    result.Should().BeOfType<ApiErrorResult>();
    var errorResult = result as ApiErrorResult;
    errorResult.Should().NotBeNull();
  }

  #endregion

  #region GetApiKeys Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetApiKeys_ReturnsListOfApiKeys()
  {
    // Arrange
    var expectedKeys = new List<ApiKeyResponse>
        {
            new ApiKeyResponse
            {
                Id = 1,
                Name = "Key 1",
                Description = "Description 1",
                KeyValue = string.Empty,
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                ExpiresAt = DateTime.UtcNow.AddDays(20),
                IsActive = true
            },
            new ApiKeyResponse
            {
                Id = 2,
                Name = "Key 2",
                Description = "Description 2",
                KeyValue = string.Empty,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                ExpiresAt = DateTime.UtcNow.AddDays(25),
                IsActive = true
            }
        };

    _mockMediator.Setup(x => x.Send(It.IsAny<GetApiKeysQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedKeys);

    // Act
    var result = await _sut.GetApiKeys();

    // Assert
    result.Should().BeOfType<OkObjectResult>();
    var okResult = result as OkObjectResult;
    var response = okResult!.Value as List<ApiKeyResponse>;

    response.Should().NotBeNull();
    response!.Should().HaveCount(2);
    response[0].Id.Should().Be(1);
    response[1].Id.Should().Be(2);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetApiKeys_EmptyList_ReturnsEmptyList()
  {
    // Arrange
    _mockMediator.Setup(x => x.Send(It.IsAny<GetApiKeysQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<ApiKeyResponse>());

    // Act
    var result = await _sut.GetApiKeys();

    // Assert
    result.Should().BeOfType<OkObjectResult>();
    var okResult = result as OkObjectResult;
    var response = okResult!.Value as List<ApiKeyResponse>;

    response.Should().NotBeNull();
    response!.Should().BeEmpty();
  }

  #endregion

  #region GetApiKeyById Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetApiKeyById_ExistingKey_ReturnsApiKey()
  {
    // Arrange
    var keyId = 123;
    var expectedKey = new ApiKeyResponse
    {
      Id = keyId,
      Name = "Test Key",
      Description = "Test Key Description",
      KeyValue = string.Empty,
      CreatedAt = DateTime.UtcNow.AddDays(-5),
      ExpiresAt = DateTime.UtcNow.AddDays(25),
      IsActive = true
    };

    _mockMediator.Setup(x => x.Send(It.IsAny<GetApiKeyByIdQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedKey);

    // Act
    var result = await _sut.GetApiKeyById(keyId);

    // Assert
    result.Should().BeOfType<OkObjectResult>();
    var okResult = result as OkObjectResult;
    var response = okResult!.Value as ApiKeyResponse;

    response.Should().NotBeNull();
    response!.Id.Should().Be(keyId);
    response.Description.Should().Be("Test Key Description");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetApiKeyById_NonExistentKey_ReturnsNotFound()
  {
    // Arrange
    var keyId = 999;

    _mockMediator.Setup(x => x.Send(It.IsAny<GetApiKeyByIdQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync((ApiKeyResponse?)null);

    // Act
    var result = await _sut.GetApiKeyById(keyId);

    // Assert
    result.Should().BeOfType<NotFoundObjectResult>();
    var notFound = result as NotFoundObjectResult;
    var response = notFound!.Value;

    response.Should().NotBeNull();
    response.ToString().Should().Contain("API key with ID 999 not found");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetApiKeyById_ExceptionThrown_ReturnsApiErrorResult()
  {
    // Arrange
    var keyId = 1;

    _mockMediator.Setup(x => x.Send(It.IsAny<GetApiKeyByIdQuery>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Database connection error"));

    // Act
    var result = await _sut.GetApiKeyById(keyId);

    // Assert
    result.Should().BeOfType<ApiErrorResult>();
  }

  #endregion

  #region RevokeApiKey Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RevokeApiKey_ExistingKey_ReturnsOk()
  {
    // Arrange
    var keyId = 123;

    _mockMediator.Setup(x => x.Send(It.IsAny<RevokeApiKeyCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(true);

    // Act
    var result = await _sut.RevokeApiKey(keyId);

    // Assert
    result.Should().BeOfType<OkObjectResult>();
    var okResult = result as OkObjectResult;
    var response = okResult!.Value;

    response.Should().NotBeNull();
    response.ToString().Should().Contain("API key 123 revoked successfully");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RevokeApiKey_NonExistentKey_ReturnsNotFound()
  {
    // Arrange
    var keyId = 999;

    _mockMediator.Setup(x => x.Send(It.IsAny<RevokeApiKeyCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(false);

    // Act
    var result = await _sut.RevokeApiKey(keyId);

    // Assert
    result.Should().BeOfType<NotFoundObjectResult>();
    var notFound = result as NotFoundObjectResult;
    var response = notFound!.Value;

    response.Should().NotBeNull();
    response.ToString().Should().Contain("API key with ID 999 not found or already revoked");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RevokeApiKey_ExceptionThrown_ReturnsApiErrorResult()
  {
    // Arrange
    var keyId = 1;

    _mockMediator.Setup(x => x.Send(It.IsAny<RevokeApiKeyCommand>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Revocation failed"));

    // Act
    var result = await _sut.RevokeApiKey(keyId);

    // Assert
    result.Should().BeOfType<ApiErrorResult>();
  }

  #endregion
}
