using System.IO;
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

namespace Zarichney.Server.Tests.Unit.Controllers.AuthController;

public class AuthControllerTests : IDisposable
{
    private readonly Mock<IMediator> _mockMediator;
    private readonly Mock<ILogger<ControllersAuthController>> _mockLogger;
    private readonly Mock<ICookieAuthManager> _mockCookieManager;
    private readonly ControllersAuthController _sut;
    private readonly ControllerContext _controllerContext;
    private readonly DefaultHttpContext _httpContext;
    private readonly MemoryStream _requestBodyStream;
    private readonly MemoryStream _responseBodyStream;
    private bool _disposed;

    public AuthControllerTests()
    {
    _mockMediator = new Mock<IMediator>();
    _mockLogger = new Mock<ILogger<ControllersAuthController>>();
        _mockCookieManager = CookieAuthManagerMockFactory.CreateDefault();

    _sut = new ControllersAuthController(_mockMediator.Object, _mockLogger.Object, _mockCookieManager.Object);

        _httpContext = new DefaultHttpContext();
        _requestBodyStream = new MemoryStream();
        _responseBodyStream = new MemoryStream();
        _httpContext.Request.Body = _requestBodyStream;
        _httpContext.Response.Body = _responseBodyStream;

        _controllerContext = new ControllerContext
        {
            HttpContext = _httpContext
        };
        _sut.ControllerContext = _controllerContext;
    }

    #region Register Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Register_ValidRequest_ReturnsOkWithSuccessResponse()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "newuser@example.com",
            Password = "StrongPassword123!"
        };

        var expectedResult = new AuthResultBuilder()
            .AsRegistrationSuccess(request.Email)
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.Register(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>("because successful registration should return HTTP 200 with authentication data");
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response.Should().NotBeNull("because the controller must return a valid AuthResponse object for successful registration");
        response!.Success.Should().BeTrue("because valid registration requests must indicate success to enable proper client-side authentication flow");
        response.Message.Should().Be(expectedResult.Message, "because the response message must match the service result to provide consistent user feedback");
        response.Email.Should().Be(request.Email, "because the response must confirm the registered email address for client verification and audit trails");

        _mockMediator.Verify(x => x.Send(
            It.Is<RegisterCommand>(cmd => cmd.Email == request.Email && cmd.Password == request.Password),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Register_FailedRegistration_ReturnsBadRequest()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "existing@example.com",
            Password = "Password123!"
        };

        var expectedResult = new AuthResultBuilder()
            .AsFailure("User with this email already exists")
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.Register(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        var response = badRequest!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("User with this email already exists");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Register_ExceptionThrown_ReturnsApiErrorResult()
    {
        // Arrange
        var request = new RegisterRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<RegisterCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database connection failed"));

        // Act
        var result = await _sut.Register(request);

        // Assert
        result.Should().BeOfType<ApiErrorResult>();
        var errorResult = result as ApiErrorResult;
        errorResult.Should().NotBeNull();
    }

    #endregion

    #region Login Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Login_ValidCredentials_ReturnsOkAndSetsCookies()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "Password123!"
        };

        var expectedResult = new AuthResultBuilder()
            .AsLoginSuccess(request.Email)
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.Login(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Login successful");
        response.Email.Should().Be(request.Email);

        _mockCookieManager.Verify(x => x.SetAuthCookies(
            It.IsAny<HttpContext>(),
            expectedResult.AccessToken!,
            expectedResult.RefreshToken!), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Login_InvalidCredentials_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "user@example.com",
            Password = "WrongPassword"
        };

        var expectedResult = new AuthResultBuilder()
            .AsInvalidCredentials()
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        var response = badRequest!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Invalid email or password");

        _mockCookieManager.Verify(x => x.SetAuthCookies(
            It.IsAny<HttpContext>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Login_EmailNotConfirmed_ReturnsBadRequest()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "unconfirmed@example.com",
            Password = "Password123!"
        };

        var expectedResult = new AuthResultBuilder()
            .AsEmailNotConfirmed()
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.Login(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        var response = badRequest!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Please verify your email address before logging in");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Login_ExceptionThrown_ReturnsApiErrorResult()
    {
        // Arrange
        var request = new LoginRequest
        {
            Email = "test@example.com",
            Password = "Password123!"
        };

        _mockMediator.Setup(x => x.Send(It.IsAny<LoginCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Authentication service unavailable"));

        // Act
        var result = await _sut.Login(request);

        // Assert
        result.Should().BeOfType<ApiErrorResult>();
    }

    #endregion

    #region RefreshToken Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task RefreshToken_ValidToken_ReturnsOkAndUpdatesCookies()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";
        _mockCookieManager.Setup(x => x.GetRefreshTokenFromCookie(It.IsAny<HttpContext>()))
            .Returns(refreshToken);

        var expectedResult = new AuthResultBuilder()
            .AsSuccess()
            .WithMessage("Tokens refreshed successfully.")
            .WithEmail("user@example.com")
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.RefreshToken();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Tokens refreshed successfully.");

        _mockCookieManager.Verify(x => x.SetAuthCookies(
            It.IsAny<HttpContext>(),
            expectedResult.AccessToken!,
            expectedResult.RefreshToken!), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task RefreshToken_MissingToken_ReturnsUnauthorized()
    {
        // Arrange
        _mockCookieManager.Setup(x => x.GetRefreshTokenFromCookie(It.IsAny<HttpContext>()))
            .Returns((string?)null);

        // Act
        var result = await _sut.RefreshToken();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorized = result as UnauthorizedObjectResult;
        var response = unauthorized!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Refresh token is missing or invalid. Please login again.");

        _mockMediator.Verify(x => x.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task RefreshToken_InvalidToken_ReturnsUnauthorizedAndClearsCookies()
    {
        // Arrange
        var refreshToken = "invalid-refresh-token";
        _mockCookieManager.Setup(x => x.GetRefreshTokenFromCookie(It.IsAny<HttpContext>()))
            .Returns(refreshToken);

        var expectedResult = new AuthResultBuilder()
            .AsFailure("Invalid or expired refresh token")
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<RefreshTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.RefreshToken();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorized = result as UnauthorizedObjectResult;
        var response = unauthorized!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be(
            expectedResult.Message ?? "Failed to refresh token. Please login again.",
            "controller should surface either the command's message or its default failure message");

        _mockCookieManager.Verify(x => x.ClearAuthCookies(It.IsAny<HttpContext>()), Times.Once);
    }

    #endregion

    #region RevokeRefreshToken Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task RevokeRefreshToken_ValidToken_ReturnsOkAndClearsCookies()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";
        _mockCookieManager.Setup(x => x.GetRefreshTokenFromCookie(It.IsAny<HttpContext>()))
            .Returns(refreshToken);

        var expectedResult = new AuthResultBuilder()
            .AsSuccess()
            .WithMessage("Token revoked successfully.")
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<RevokeTokenCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.RevokeRefreshToken();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Token revoked successfully.");

        _mockCookieManager.Verify(x => x.ClearAuthCookies(It.IsAny<HttpContext>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task RevokeRefreshToken_MissingToken_ReturnsBadRequest()
    {
        // Arrange
        _mockCookieManager.Setup(x => x.GetRefreshTokenFromCookie(It.IsAny<HttpContext>()))
            .Returns((string?)null);

        // Act
        var result = await _sut.RevokeRefreshToken();

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        var response = badRequest!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Refresh token is missing in request.");
    }

    #endregion

    #region ForgotPassword Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task EmailForgetPassword_ValidEmail_ReturnsGenericSuccessResponse()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "user@example.com" };

        var expectedResult = new AuthResultBuilder()
            .AsSuccess()
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.EmailForgetPassword(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("If an account with that email exists, a password reset link has been sent.");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task EmailForgetPassword_NonExistentEmail_StillReturnsGenericSuccessResponse()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "nonexistent@example.com" };

        var expectedResult = new AuthResultBuilder()
            .AsFailure("User not found")
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.EmailForgetPassword(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue("to prevent email enumeration");
        response.Message.Should().Be("If an account with that email exists, a password reset link has been sent.");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task EmailForgetPassword_ExceptionThrown_StillReturnsGenericSuccessResponse()
    {
        // Arrange
        var request = new ForgotPasswordRequest { Email = "test@example.com" };

        _mockMediator.Setup(x => x.Send(It.IsAny<ForgotPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Email service unavailable"));

        // Act
        var result = await _sut.EmailForgetPassword(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue("to prevent information leakage");
        response.Message.Should().Be("If an account with that email exists, a password reset link has been sent.");
    }

    #endregion

    #region ResetPassword Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ResetPassword_ValidToken_ReturnsOk()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "user@example.com",
            Token = "valid-reset-token",
            NewPassword = "NewSecurePassword123!"
        };

        var expectedResult = new AuthResultBuilder()
            .AsSuccess()
            .WithMessage("Password has been reset successfully.")
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.ResetPassword(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Password has been reset successfully.");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ResetPassword_InvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var request = new ResetPasswordRequest
        {
            Email = "user@example.com",
            Token = "invalid-token",
            NewPassword = "NewPassword123!"
        };

        var expectedResult = new AuthResultBuilder()
            .AsFailure("Invalid or expired reset token")
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<ResetPasswordCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.ResetPassword(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        var response = badRequest!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
        response.Message.Should().Be("Invalid or expired reset token");
    }

    #endregion

    #region ConfirmEmail Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ConfirmEmail_ValidToken_ReturnsOkAndSetsCookies()
    {
        // Arrange
        var request = new ConfirmEmailRequest
        {
            UserId = "user-id-123",
            Token = "confirmation-token"
        };

        var expectedResult = new AuthResultBuilder()
            .AsLoginSuccess("confirmed@example.com")
            .WithMessage("Email confirmed successfully.")
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<ConfirmEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.ConfirmEmail(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Email confirmed successfully.");

        _mockCookieManager.Verify(x => x.SetAuthCookies(
            It.IsAny<HttpContext>(),
            expectedResult.AccessToken!,
            expectedResult.RefreshToken!), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ConfirmEmail_WithRedirectUrl_ReturnsRedirect()
    {
        // Arrange
        var request = new ConfirmEmailRequest
        {
            UserId = "user-id-123",
            Token = "confirmation-token"
        };

        var expectedResult = new AuthResultBuilder()
            .AsSuccess()
            .WithRedirectUrl("/dashboard")
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<ConfirmEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.ConfirmEmail(request);

        // Assert
        result.Should().BeOfType<RedirectResult>();
        var redirectResult = result as RedirectResult;
        redirectResult!.Url.Should().Be("/dashboard");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ConfirmEmail_InvalidToken_ReturnsBadRequest()
    {
        // Arrange
        var request = new ConfirmEmailRequest
        {
            UserId = "user-id",
            Token = "invalid-token"
        };

        var expectedResult = new AuthResultBuilder()
            .AsFailure("Invalid confirmation token")
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<ConfirmEmailCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.ConfirmEmail(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequest = result as BadRequestObjectResult;
        var response = badRequest!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeFalse();
    }

    #endregion

    #region ResendConfirmation Tests

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ResendConfirmation_ValidEmail_ReturnsGenericSuccessResponse()
    {
        // Arrange
        var request = new ResendConfirmationRequest { Email = "unconfirmed@example.com" };

        var expectedResult = new AuthResultBuilder()
            .AsSuccess()
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<ResendConfirmationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.ResendConfirmation(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("If an account requiring confirmation exists for that email, a new confirmation link has been sent.");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task ResendConfirmation_AlreadyConfirmed_StillReturnsGenericSuccessResponse()
    {
        // Arrange
        var request = new ResendConfirmationRequest { Email = "confirmed@example.com" };

        var expectedResult = new AuthResultBuilder()
            .AsFailure("Email already confirmed")
            .Build();

        _mockMediator.Setup(x => x.Send(It.IsAny<ResendConfirmationCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.ResendConfirmation(request);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue("to prevent email enumeration");
        response.Message.Should().Be("If an account requiring confirmation exists for that email, a new confirmation link has been sent.");
    }

    #endregion

    #region Logout Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void Logout_ValidUser_ReturnsOkAndClearsCookies()
    {
        // Act
        var result = _sut.Logout();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as AuthResponse;

        response.Should().NotBeNull();
        response!.Success.Should().BeTrue();
        response.Message.Should().Be("Logged out successfully");

        _mockCookieManager.Verify(x => x.ClearAuthCookies(It.IsAny<HttpContext>()), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void Logout_ExceptionThrown_ReturnsApiErrorResult()
    {
        // Arrange
        _mockCookieManager.Setup(x => x.ClearAuthCookies(It.IsAny<HttpContext>()))
            .Throws(new Exception("Cookie clearing failed"));

        // Act
        var result = _sut.Logout();

        // Assert
        result.Should().BeOfType<ApiErrorResult>();
    }

    #endregion

    #region CheckAuthentication Tests

    [Fact]
    [Trait("Category", "Unit")]
    public void CheckAuthentication_AuthenticatedUser_ReturnsUserInfo()
    {
        // Arrange
        var userId = "user-123";
        var userEmail = "user@example.com";
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
        var result = _sut.CheckAuthentication();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as Dictionary<string, object>;

        response.Should().NotBeNull();
        response!["userId"].Should().Be(userId);
        response["isAdmin"].Should().Be(true);
        response["authenticationType"].Should().Be("Bearer");
        response["isAuthenticated"].Should().Be(true);

        var roles = response["roles"] as List<string>;
        roles.Should().NotBeNull();
        roles!.Should().Contain("admin");
        roles.Should().Contain("user");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void CheckAuthentication_UserWithoutAdminRole_ReturnsCorrectIsAdminStatus()
    {
        // Arrange
        var userId = "user-456";
        List<Claim> claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, "user")
        ];

        var identity = new ClaimsIdentity(claims, "Bearer");
        var principal = new ClaimsPrincipal(identity);
        _controllerContext.HttpContext.User = principal;

        // Act
        var result = _sut.CheckAuthentication();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var response = okResult!.Value as Dictionary<string, object>;

        response.Should().NotBeNull();
        response!["isAdmin"].Should().Be(false);
    }

    #endregion

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            _sut.ControllerContext = new ControllerContext();
            _responseBodyStream?.Dispose();
            _requestBodyStream?.Dispose();
            _disposed = true;
        }
    }
}
