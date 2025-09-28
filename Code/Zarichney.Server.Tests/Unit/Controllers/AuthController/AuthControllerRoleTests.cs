using System.Security.Claims;
using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Controllers;
using Zarichney.Server.Tests.Framework.Mocks;
using Zarichney.Server.Tests.TestData.Builders;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Commands;
using Zarichney.Services.Auth.Models;
using ControllersAuthController = Zarichney.Controllers.AuthController;
using static Zarichney.Controllers.AuthController;

namespace Zarichney.Server.Tests.Unit.Controllers.AuthControllerTests;

public class AuthControllerRoleTests
{
  private readonly Mock<IMediator> _mockMediator;
  private readonly Mock<ILogger<ControllersAuthController>> _mockLogger;
  private readonly Mock<ICookieAuthManager> _mockCookieManager;
  private readonly ControllersAuthController _sut;

  public AuthControllerRoleTests()
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

  #region AddUserToRole Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task AddUserToRole_ValidRequest_ReturnsOk()
  {
    // Arrange
    var request = new RoleRequest
    {
      Identifier = "user@example.com",
      RoleName = "admin"
    };

    var expectedResult = new RoleCommandResult
    {
      Success = true,
      Message = "User added to role successfully"
    };

    _mockMediator.Setup(x => x.Send(It.IsAny<AddUserToRoleCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.AddUserToRole(request);

    // Assert
    result.Should().BeOfType<OkObjectResult>();
    var okResult = result as OkObjectResult;
    var response = okResult!.Value as RoleCommandResult;

    response.Should().NotBeNull();
    response!.Success.Should().BeTrue();
    response.Message.Should().Be("User added to role successfully");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task AddUserToRole_UserNotFound_ReturnsBadRequest()
  {
    // Arrange
    var request = new RoleRequest
    {
      Identifier = "nonexistent@example.com",
      RoleName = "admin"
    };

    var expectedResult = new RoleCommandResult
    {
      Success = false,
      Message = "User not found"
    };

    _mockMediator.Setup(x => x.Send(It.IsAny<AddUserToRoleCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.AddUserToRole(request);

    // Assert
    result.Should().BeOfType<BadRequestObjectResult>();
    var badRequest = result as BadRequestObjectResult;
    var response = badRequest!.Value as RoleCommandResult;

    response.Should().NotBeNull();
    response!.Success.Should().BeFalse();
    response.Message.Should().Be("User not found");
  }

  [Theory]
  [Trait("Category", "Unit")]
  [InlineData("", "admin", "User identifier and RoleName are required.")]
  [InlineData("user@example.com", "", "User identifier and RoleName are required.")]
  [InlineData("", "", "User identifier and RoleName are required.")]
  [InlineData("   ", "admin", "User identifier and RoleName are required.")]
  [InlineData("user@example.com", "   ", "User identifier and RoleName are required.")]
  public async Task AddUserToRole_InvalidInput_ReturnsBadRequest(string identifier, string roleName, string expectedMessage)
  {
    // Arrange
    var request = new RoleRequest
    {
      Identifier = identifier,
      RoleName = roleName
    };

    // Act
    var result = await _sut.AddUserToRole(request);

    // Assert
    result.Should().BeOfType<BadRequestObjectResult>();
    var badRequest = result as BadRequestObjectResult;
    var response = badRequest!.Value as RoleCommandResult;

    response.Should().NotBeNull();
    response!.Success.Should().BeFalse();
    response.Message.Should().Be(expectedMessage);

    _mockMediator.Verify(x => x.Send(It.IsAny<AddUserToRoleCommand>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task AddUserToRole_ExceptionThrown_ReturnsApiErrorResult()
  {
    // Arrange
    var request = new RoleRequest
    {
      Identifier = "user@example.com",
      RoleName = "admin"
    };

    _mockMediator.Setup(x => x.Send(It.IsAny<AddUserToRoleCommand>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Database error"));

    // Act
    var result = await _sut.AddUserToRole(request);

    // Assert
    result.Should().BeOfType<ApiErrorResult>();
  }

  #endregion

  #region RemoveUserFromRole Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RemoveUserFromRole_ValidRequest_ReturnsOk()
  {
    // Arrange
    var request = new RoleRequest
    {
      Identifier = "user@example.com",
      RoleName = "admin"
    };

    var expectedResult = new RoleCommandResult
    {
      Success = true,
      Message = "User removed from role successfully"
    };

    _mockMediator.Setup(x => x.Send(It.IsAny<RemoveUserFromRoleCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.RemoveUserFromRole(request);

    // Assert
    result.Should().BeOfType<OkObjectResult>();
    var okResult = result as OkObjectResult;
    var response = okResult!.Value as RoleCommandResult;

    response.Should().NotBeNull();
    response!.Success.Should().BeTrue();
    response.Message.Should().Be("User removed from role successfully");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RemoveUserFromRole_UserNotInRole_ReturnsBadRequest()
  {
    // Arrange
    var request = new RoleRequest
    {
      Identifier = "user@example.com",
      RoleName = "admin"
    };

    var expectedResult = new RoleCommandResult
    {
      Success = false,
      Message = "User is not in the specified role"
    };

    _mockMediator.Setup(x => x.Send(It.IsAny<RemoveUserFromRoleCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.RemoveUserFromRole(request);

    // Assert
    result.Should().BeOfType<BadRequestObjectResult>();
    var badRequest = result as BadRequestObjectResult;
    var response = badRequest!.Value as RoleCommandResult;

    response.Should().NotBeNull();
    response!.Success.Should().BeFalse();
    response.Message.Should().Be("User is not in the specified role");
  }

  [Theory]
  [Trait("Category", "Unit")]
  [InlineData("", "editor", "User identifier and RoleName are required.")]
  [InlineData("user@example.com", "", "User identifier and RoleName are required.")]
  [InlineData(null, "editor", "User identifier and RoleName are required.")]
  [InlineData("user@example.com", null, "User identifier and RoleName are required.")]
  public async Task RemoveUserFromRole_InvalidInput_ReturnsBadRequest(string? identifier, string? roleName, string expectedMessage)
  {
    // Arrange
    var request = new RoleRequest
    {
      Identifier = identifier ?? string.Empty,
      RoleName = roleName ?? string.Empty
    };

    // Act
    var result = await _sut.RemoveUserFromRole(request);

    // Assert
    result.Should().BeOfType<BadRequestObjectResult>();
    var badRequest = result as BadRequestObjectResult;
    var response = badRequest!.Value as RoleCommandResult;

    response.Should().NotBeNull();
    response!.Success.Should().BeFalse();
    response.Message.Should().Be(expectedMessage);
  }

  #endregion

  #region GetUserRoles Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUserRoles_ValidUser_ReturnsRolesList()
  {
    // Arrange
    var identifier = "user@example.com";
    var expectedResult = new RoleCommandResult
    {
      Success = true,
      Message = "Roles retrieved successfully",
      Roles = new List<string> { "user", "editor", "admin" }
    };

    _mockMediator.Setup(x => x.Send(It.IsAny<GetUserRolesQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.GetUserRoles(identifier);

    // Assert
    result.Should().BeOfType<OkObjectResult>();
    var okResult = result as OkObjectResult;
    var response = okResult!.Value as RoleCommandResult;

    response.Should().NotBeNull();
    response!.Success.Should().BeTrue();
    response.Roles.Should().NotBeNull();
    response.Roles!.Should().HaveCount(3);
    response.Roles.Should().Contain("admin");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUserRoles_UserNotFound_ReturnsNotFound()
  {
    // Arrange
    var identifier = "nonexistent@example.com";
    var expectedResult = new RoleCommandResult
    {
      Success = false,
      Message = "User not found"
    };

    _mockMediator.Setup(x => x.Send(It.IsAny<GetUserRolesQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.GetUserRoles(identifier);

    // Assert
    result.Should().BeOfType<NotFoundObjectResult>();
    var notFound = result as NotFoundObjectResult;
    var response = notFound!.Value as RoleCommandResult;

    response.Should().NotBeNull();
    response!.Success.Should().BeFalse();
    response.Message.Should().Be("User not found");
  }

  [Theory]
  [Trait("Category", "Unit")]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public async Task GetUserRoles_InvalidIdentifier_ReturnsBadRequest(string? identifier)
  {
    // Act
    var result = await _sut.GetUserRoles(identifier!);

    // Assert
    result.Should().BeOfType<BadRequestObjectResult>();
    var badRequest = result as BadRequestObjectResult;
    var response = badRequest!.Value as RoleCommandResult;

    response.Should().NotBeNull();
    response!.Success.Should().BeFalse();
    response.Message.Should().Be("User identifier (ID or email) is required.");

    _mockMediator.Verify(x => x.Send(It.IsAny<GetUserRolesQuery>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  #endregion

  #region GetUsersInRole Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUsersInRole_ValidRole_ReturnsUsersList()
  {
    // Arrange
    var roleName = "admin";
    var expectedUsers = new List<UserRoleInfo>
        {
            new UserRoleInfo { UserId = "user1", Email = "admin1@example.com", UserName = "admin1" },
            new UserRoleInfo { UserId = "user2", Email = "admin2@example.com", UserName = "admin2" }
        };

    _mockMediator.Setup(x => x.Send(It.IsAny<GetUsersInRoleQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedUsers);

    // Act
    var result = await _sut.GetUsersInRole(roleName);

    // Assert
    result.Should().BeOfType<OkObjectResult>();
    var okResult = result as OkObjectResult;
    var response = okResult!.Value as List<UserRoleInfo>;

    response.Should().NotBeNull();
    response!.Should().HaveCount(2);
    response[0].Email.Should().Be("admin1@example.com");
    response[1].Email.Should().Be("admin2@example.com");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUsersInRole_EmptyRole_ReturnsEmptyList()
  {
    // Arrange
    var roleName = "unused-role";

    _mockMediator.Setup(x => x.Send(It.IsAny<GetUsersInRoleQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<UserRoleInfo>());

    // Act
    var result = await _sut.GetUsersInRole(roleName);

    // Assert
    result.Should().BeOfType<OkObjectResult>();
    var okResult = result as OkObjectResult;
    var response = okResult!.Value as List<UserRoleInfo>;

    response.Should().NotBeNull();
    response!.Should().BeEmpty();
  }

  [Theory]
  [Trait("Category", "Unit")]
  [InlineData("")]
  [InlineData("   ")]
  [InlineData(null)]
  public async Task GetUsersInRole_InvalidRoleName_ReturnsBadRequest(string? roleName)
  {
    // Act
    var result = await _sut.GetUsersInRole(roleName!);

    // Assert
    result.Should().BeOfType<BadRequestObjectResult>();
    var badRequest = result as BadRequestObjectResult;

    badRequest!.Value.Should().NotBeNull();
    badRequest.Value.ToString().Should().Contain("RoleName is required");

    _mockMediator.Verify(x => x.Send(It.IsAny<GetUsersInRoleQuery>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetUsersInRole_ExceptionThrown_ReturnsApiErrorResult()
  {
    // Arrange
    var roleName = "admin";

    _mockMediator.Setup(x => x.Send(It.IsAny<GetUsersInRoleQuery>(), It.IsAny<CancellationToken>()))
        .ThrowsAsync(new Exception("Query failed"));

    // Act
    var result = await _sut.GetUsersInRole(roleName);

    // Assert
    result.Should().BeOfType<ApiErrorResult>();
  }

  #endregion

  #region RefreshUserClaims Tests

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RefreshUserClaims_ValidUser_ReturnsOkAndUpdatesCookies()
  {
    // Arrange
    var userId = "user-123";
    var userEmail = "user@example.com";
    var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail)
        };

    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);
    _sut.ControllerContext.HttpContext.User = principal;

    var expectedResult = new AuthResultBuilder()
        .AsSuccess()
        .WithMessage("Claims refreshed successfully")
        .WithEmail(userEmail)
        .Build();

    _mockMediator.Setup(x => x.Send(It.IsAny<RefreshUserClaimsCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.RefreshUserClaims();

    // Assert
    result.Should().BeOfType<OkObjectResult>();
    var okResult = result as OkObjectResult;
    var response = okResult!.Value as ControllersAuthController.AuthResponse;

    response.Should().NotBeNull();
    response!.Success.Should().BeTrue();
    response.Message.Should().Be("User claims refreshed successfully.");
    response.Email.Should().Be(userEmail);

    _mockCookieManager.Verify(x => x.SetAuthCookies(
        It.IsAny<HttpContext>(),
        expectedResult.AccessToken!,
        expectedResult.RefreshToken!), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RefreshUserClaims_MissingUserId_ReturnsBadRequest()
  {
    // Arrange
    var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Email, "user@example.com")
            // Missing NameIdentifier claim
        };

    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);
    _sut.ControllerContext.HttpContext.User = principal;

    // Act
    var result = await _sut.RefreshUserClaims();

    // Assert
    result.Should().BeOfType<BadRequestObjectResult>();
    var badRequest = result as BadRequestObjectResult;
    var response = badRequest!.Value as ControllersAuthController.AuthResponse;

    response.Should().NotBeNull();
    response!.Success.Should().BeFalse();
    response.Message.Should().Contain("User information not found");

    _mockMediator.Verify(x => x.Send(It.IsAny<RefreshUserClaimsCommand>(), It.IsAny<CancellationToken>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RefreshUserClaims_FailedRefresh_ReturnsBadRequest()
  {
    // Arrange
    var userId = "user-123";
    var userEmail = "user@example.com";
    var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail)
        };

    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);
    _sut.ControllerContext.HttpContext.User = principal;

    var expectedResult = new AuthResultBuilder()
        .AsFailure("Unable to refresh claims")
        .Build();

    _mockMediator.Setup(x => x.Send(It.IsAny<RefreshUserClaimsCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.RefreshUserClaims();

    // Assert
    result.Should().BeOfType<BadRequestObjectResult>();
    var badRequest = result as BadRequestObjectResult;
    var response = badRequest!.Value as ControllersAuthController.AuthResponse;

    response.Should().NotBeNull();
    response!.Success.Should().BeFalse();
    response.Message.Should().Be("Unable to refresh claims");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task RefreshUserClaims_SuccessWithoutTokens_ReturnsApiErrorResult()
  {
    // Arrange
    var userId = "user-123";
    var userEmail = "user@example.com";
    var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail)
        };

    var identity = new ClaimsIdentity(claims, "Bearer");
    var principal = new ClaimsPrincipal(identity);
    _sut.ControllerContext.HttpContext.User = principal;

    var expectedResult = new AuthResultBuilder()
        .AsSuccess()
        .WithAccessToken(null)
        .WithRefreshToken(null)
        .Build();

    _mockMediator.Setup(x => x.Send(It.IsAny<RefreshUserClaimsCommand>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedResult);

    // Act
    var result = await _sut.RefreshUserClaims();

    // Assert
    result.Should().BeOfType<ApiErrorResult>();
  }

  #endregion
}
