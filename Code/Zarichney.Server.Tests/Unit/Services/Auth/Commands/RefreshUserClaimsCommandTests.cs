using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Commands;
using Zarichney.Services.Auth.Models;

namespace Zarichney.Server.Tests.Unit.Services.Auth.Commands;

public class RefreshUserClaimsCommandTests
{
    private readonly Mock<ILogger<RefreshUserClaimsCommandHandler>> _mockLogger;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly Mock<UserManager<ApplicationUser>> _mockUserManager;
    private readonly RefreshUserClaimsCommandHandler _handler;

    public RefreshUserClaimsCommandTests()
    {
        _mockLogger = new Mock<ILogger<RefreshUserClaimsCommandHandler>>();
        _mockAuthService = new Mock<IAuthService>();
        var store = new Mock<IUserStore<ApplicationUser>>();
        _mockUserManager = new Mock<UserManager<ApplicationUser>>(
            store.Object, null!, null!, null!, null!, null!, null!, null!, null!);

        _handler = new RefreshUserClaimsCommandHandler(
            _mockLogger.Object,
            _mockAuthService.Object,
            _mockUserManager.Object);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithValidUserAndMatchingEmail_ReturnsSuccessWithNewTokens()
    {
        // Arrange
        var userId = "test-user-123";
        var email = "test@example.com";
        var command = new RefreshUserClaimsCommand(userId, email);

        var user = new ApplicationUser
        {
            Id = userId,
            Email = email,
            UserName = "testuser"
        };

        var newAccessToken = "new-access-token";
        var newRefreshToken = "new-refresh-token";

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _mockAuthService.Setup(x => x.GenerateJwtTokenAsync(user))
            .ReturnsAsync(newAccessToken);

        _mockAuthService.Setup(x => x.GenerateRefreshToken())
            .Returns(newRefreshToken);

        _mockAuthService.Setup(x => x.SaveRefreshTokenAsync(
                user,
                newRefreshToken,
                "Claims Refresh",
                "",
                ""))
            .ReturnsAsync(new RefreshToken { Token = newRefreshToken, UserId = userId });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("because the claims were successfully refreshed");
        result.Message.Should().Be("User claims refreshed successfully");
        result.Email.Should().Be(email);
        result.AccessToken.Should().Be(newAccessToken);
        result.RefreshToken.Should().Be(newRefreshToken);

        _mockUserManager.Verify(x => x.FindByIdAsync(userId), Times.Once);
        _mockAuthService.Verify(x => x.GenerateJwtTokenAsync(user), Times.Once);
        _mockAuthService.Verify(x => x.GenerateRefreshToken(), Times.Once);
        _mockAuthService.Verify(x => x.SaveRefreshTokenAsync(
            user,
            newRefreshToken,
            "Claims Refresh",
            "",
            ""), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithEmptyUserId_ReturnsFailure()
    {
        // Arrange
        var command = new RefreshUserClaimsCommand("", "test@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("because user ID is required");
        result.Message.Should().Be("User ID is required");
        result.AccessToken.Should().BeNull();
        result.RefreshToken.Should().BeNull();

        _mockUserManager.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
        _mockAuthService.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithNullUserId_ReturnsFailure()
    {
        // Arrange
        var command = new RefreshUserClaimsCommand(null!, "test@example.com");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("because user ID cannot be null");
        result.Message.Should().Be("User ID is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithEmptyEmail_ReturnsFailure()
    {
        // Arrange
        var command = new RefreshUserClaimsCommand("test-user-123", "");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("because email is required");
        result.Message.Should().Be("Email is required");

        _mockUserManager.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithNullEmail_ReturnsFailure()
    {
        // Arrange
        var command = new RefreshUserClaimsCommand("test-user-123", null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("because email cannot be null");
        result.Message.Should().Be("Email is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithNonExistentUser_ReturnsFailure()
    {
        // Arrange
        var userId = "non-existent-user";
        var email = "test@example.com";
        var command = new RefreshUserClaimsCommand(userId, email);

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync((ApplicationUser?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("because the user does not exist");
        result.Message.Should().Be("User not found");

        _mockUserManager.Verify(x => x.FindByIdAsync(userId), Times.Once);
        _mockAuthService.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithMismatchedEmail_ReturnsFailure()
    {
        // Arrange
        var userId = "test-user-123";
        var requestEmail = "wrong@example.com";
        var actualEmail = "correct@example.com";
        var command = new RefreshUserClaimsCommand(userId, requestEmail);

        var user = new ApplicationUser
        {
            Id = userId,
            Email = actualEmail,
            UserName = "testuser"
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("because the email does not match the user's email");
        result.Message.Should().Be("User email mismatch");

        _mockUserManager.Verify(x => x.FindByIdAsync(userId), Times.Once);
        _mockAuthService.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithCaseInsensitiveEmailMatch_ReturnsSuccess()
    {
        // Arrange
        var userId = "test-user-123";
        var requestEmail = "TEST@EXAMPLE.COM";
        var actualEmail = "test@example.com";
        var command = new RefreshUserClaimsCommand(userId, requestEmail);

        var user = new ApplicationUser
        {
            Id = userId,
            Email = actualEmail,
            UserName = "testuser"
        };

        var newAccessToken = "new-access-token";
        var newRefreshToken = "new-refresh-token";

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _mockAuthService.Setup(x => x.GenerateJwtTokenAsync(user))
            .ReturnsAsync(newAccessToken);

        _mockAuthService.Setup(x => x.GenerateRefreshToken())
            .Returns(newRefreshToken);

        _mockAuthService.Setup(x => x.SaveRefreshTokenAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new RefreshToken { Token = newRefreshToken, UserId = userId });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("because email comparison is case-insensitive");
        result.Email.Should().Be(actualEmail);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        // Arrange
        var userId = "test-user-123";
        var email = "test@example.com";
        var command = new RefreshUserClaimsCommand(userId, email);

        var expectedException = new InvalidOperationException("Database connection failed");

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ThrowsAsync(expectedException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("because an exception occurred");
        result.Message.Should().Be("Failed to refresh user claims");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error during claims refresh")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_Success_LogsInformationMessage()
    {
        // Arrange
        var userId = "test-user-123";
        var email = "test@example.com";
        var command = new RefreshUserClaimsCommand(userId, email);

        var user = new ApplicationUser
        {
            Id = userId,
            Email = email,
            UserName = "testuser"
        };

        _mockUserManager.Setup(x => x.FindByIdAsync(userId))
            .ReturnsAsync(user);

        _mockAuthService.Setup(x => x.GenerateJwtTokenAsync(user))
            .ReturnsAsync("access-token");

        _mockAuthService.Setup(x => x.GenerateRefreshToken())
            .Returns("refresh-token");

        _mockAuthService.Setup(x => x.SaveRefreshTokenAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(new RefreshToken { Token = "refresh-token", UserId = userId });

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains($"User {email} claims refreshed successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}