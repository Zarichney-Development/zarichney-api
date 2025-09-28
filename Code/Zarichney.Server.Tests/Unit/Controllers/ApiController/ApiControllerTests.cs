using System.Security.Claims;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Controllers;
using Zarichney.Services.Email;

namespace Zarichney.Server.Tests.Unit.Controllers.ApiControllerTests;

/// <summary>
/// Unit test coverage for <see cref="Zarichney.Controllers.ApiController"/>,
/// validating email verification endpoints and controller health checks.
/// </summary>
public class ApiControllerTests
{
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<ILogger<Zarichney.Controllers.ApiController>> _mockLogger;
    private readonly Zarichney.Controllers.ApiController _sut;
    private readonly ControllerContext _controllerContext;

    public ApiControllerTests()
    {
        _mockEmailService = new Mock<IEmailService>();
        _mockLogger = new Mock<ILogger<Zarichney.Controllers.ApiController>>();
        _sut = new Zarichney.Controllers.ApiController(_mockEmailService.Object, _mockLogger.Object);

        _controllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        _sut.ControllerContext = _controllerContext;
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateEmail_ValidEmail_ReturnsOkResult()
    {
        // Arrange
        var email = "test@example.com";
        _mockEmailService.Setup(x => x.ValidateEmail(email))
            .ReturnsAsync(true);

        // Act
        var result = await _sut.ValidateEmail(email);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().Be("Valid");

        _mockEmailService.Verify(x => x.ValidateEmail(email), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateEmail_EmptyEmail_ReturnsBadRequest()
    {
        // Arrange
        var email = "";

        // Act
        var result = await _sut.ValidateEmail(email);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Email parameter is required");

        _mockEmailService.Verify(x => x.ValidateEmail(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateEmail_NullEmail_ReturnsBadRequest()
    {
        // Arrange
        string? email = null;

        // Act
        var result = await _sut.ValidateEmail(email!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Email parameter is required");

        _mockEmailService.Verify(x => x.ValidateEmail(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateEmail_WhitespaceEmail_ReturnsBadRequest()
    {
        // Arrange
        var email = "   ";

        // Act
        var result = await _sut.ValidateEmail(email);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().Be("Email parameter is required");

        _mockEmailService.Verify(x => x.ValidateEmail(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateEmail_InvalidEmailException_ReturnsBadRequestWithDetails()
    {
        // Arrange
        var email = "invalid@test.com";
        var exception = new InvalidEmailException("Invalid email format", email, InvalidEmailReason.InvalidSyntax);
        _mockEmailService.Setup(x => x.ValidateEmail(email))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.ValidateEmail(email);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();

        var badRequest = result as BadRequestObjectResult;
        badRequest!.Value.Should().NotBeNull();
        var response = badRequest.Value;
        response.Should().BeEquivalentTo(new
        {
            error = "Invalid email format",
            email = email,
            reason = "InvalidSyntax"
        });

        _mockEmailService.Verify(x => x.ValidateEmail(email), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ValidateEmail_GeneralException_ReturnsApiErrorResult()
    {
        // Arrange
        var email = "test@example.com";
        var exception = new InvalidOperationException("Service unavailable");
        _mockEmailService.Setup(x => x.ValidateEmail(email))
            .ThrowsAsync(exception);

        // Act
        var result = await _sut.ValidateEmail(email);

        // Assert
        result.Should().BeOfType<ApiErrorResult>();

        var errorResult = result as ApiErrorResult;
        // ApiErrorResult inherits from ObjectResult which has StatusCode

        _mockEmailService.Verify(x => x.ValidateEmail(email), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HealthCheck_AuthenticatedUser_ReturnsOkWithUserInfo()
    {
        // Arrange
        var userEmail = "user@example.com";
        List<Claim> claims =
        [
            new Claim(ClaimTypes.Email, userEmail)
        ];
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controllerContext.HttpContext.User = principal;

        // Act
        var result = _sut.HealthCheck();

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var response = okResult.Value;
        response.Should().BeEquivalentTo(new
        {
            Success = true,
            Time = DateTime.Now.ToLocalTime(),
            User = userEmail
        }, options => options
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1)))
            .WhenTypeIs<DateTime>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void HealthCheck_NoEmailClaim_ReturnsOkWithUnknownUser()
    {
        // Arrange
        List<Claim> claims = [];
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controllerContext.HttpContext.User = principal;

        // Act
        var result = _sut.HealthCheck();

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var response = okResult.Value;
        response.Should().BeEquivalentTo(new
        {
            Success = true,
            Time = DateTime.Now.ToLocalTime(),
            User = "Unknown"
        }, options => options
            .Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, TimeSpan.FromSeconds(1)))
            .WhenTypeIs<DateTime>());
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void TestAuth_AuthenticatedUserWithRoles_ReturnsCompleteAuthInfo()
    {
        // Arrange
        var userId = "user-123";
        var userEmail = "admin@example.com";
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, userEmail),
            new Claim(ClaimTypes.Role, "admin"),
            new Claim(ClaimTypes.Role, "user")
        ];
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);
        _controllerContext.HttpContext.User = principal;

        // Act
        var result = _sut.TestAuth();

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var response = okResult.Value;
        response.Should().BeEquivalentTo(new
        {
            userId = userId,
            authType = "Bearer",
            isAuthenticated = true,
            isAdmin = true,
            roles = new[] { "admin", "user" },
            isApiKeyAuth = false,
            apiKeyInfo = (object?)null,
            message = "Authentication successful!"
        });
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void TestAuth_ApiKeyAuthentication_ReturnsApiKeyInfo()
    {
        // Arrange
        var userId = "api-user";
        var apiKeyId = "key-123";
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userId)
        ];
        var identity = new ClaimsIdentity(claims, "ApiKey");
        var principal = new ClaimsPrincipal(identity);
        _controllerContext.HttpContext.User = principal;
        _controllerContext.HttpContext.Items["ApiKey"] = apiKeyId;

        // Act
        var result = _sut.TestAuth();

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var response = okResult.Value;
        List<string> expectedRoles = [];
        response.Should().BeEquivalentTo(new
        {
            userId = userId,
            authType = "ApiKey",
            isAuthenticated = true,
            isAdmin = false,
            roles = expectedRoles,
            isApiKeyAuth = true,
            apiKeyInfo = new { keyId = apiKeyId },
            message = "Authentication successful!"
        });
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void TestAuth_UnauthenticatedUser_ReturnsMinimalInfo()
    {
        // Arrange
        var principal = new ClaimsPrincipal();
        _controllerContext.HttpContext.User = principal;

        // Act
        var result = _sut.TestAuth();

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var response = okResult.Value;
        List<string> expectedRoles = [];
        response.Should().BeEquivalentTo(new
        {
            userId = "Unknown",
            authType = "None",
            isAuthenticated = false,
            isAdmin = false,
            roles = expectedRoles,
            isApiKeyAuth = false,
            apiKeyInfo = (object?)null,
            message = "Authentication successful!"
        });
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void TestAuth_UserWithoutAdminRole_ReturnsIsAdminFalse()
    {
        // Arrange
        var userId = "user-456";
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, "user"),
            new Claim(ClaimTypes.Role, "moderator")
        ];
        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);
        _controllerContext.HttpContext.User = principal;

        // Act
        var result = _sut.TestAuth();

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        var okResult = result as OkObjectResult;
        okResult!.Value.Should().NotBeNull();

        var response = okResult.Value;
        response.Should().BeEquivalentTo(new
        {
            userId = userId,
            authType = "Bearer",
            isAuthenticated = true,
            isAdmin = false,
            roles = new[] { "user", "moderator" },
            isApiKeyAuth = false,
            apiKeyInfo = (object?)null,
            message = "Authentication successful!"
        });
    }
}
