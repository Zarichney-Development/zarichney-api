using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Commands;
using Zarichney.Services.Auth.Models;
using Zarichney.Tests.Framework.Mocks.Factories;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Services.Auth.Commands;

public class RefreshTokenCommandTests
{
    private readonly Mock<ILogger<RefreshTokenCommandHandler>> _loggerMock;
    private readonly Mock<IAuthService> _authServiceMock;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandTests()
    {
        _loggerMock = new Mock<ILogger<RefreshTokenCommandHandler>>();
        _authServiceMock = AuthServiceMockFactory.CreateDefault();
        _handler = new RefreshTokenCommandHandler(_loggerMock.Object, _authServiceMock.Object);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_ValidRefreshToken_ReturnsSuccessResult()
    {
        // Arrange
        var user = new ApplicationUserBuilder()
            .WithEmail("test@example.com")
            .Build();
        
        var refreshToken = new RefreshTokenBuilder()
            .WithToken("valid-refresh-token")
            .WithUser(user)
            .AsValid()
            .Build();
        
        var command = new RefreshTokenCommand("valid-refresh-token");
        
        _authServiceMock.Setup(x => x.FindRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(refreshToken);
        
        _authServiceMock.Setup(x => x.GenerateJwtTokenAsync(user))
            .ReturnsAsync("new-jwt-token");
        
        _authServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");
        
        _authServiceMock.Setup(x => x.MarkRefreshTokenAsUsedAsync(refreshToken))
            .Returns(Task.CompletedTask);
        
        _authServiceMock.Setup(x => x.SaveRefreshTokenAsync(
                user, 
                "new-refresh-token", 
                refreshToken.DeviceName,
                refreshToken.DeviceIp,
                refreshToken.UserAgent))
            .ReturnsAsync(new RefreshToken { Token = "new-refresh-token" });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("valid refresh token should succeed");
        result.Message.Should().Be("Token refreshed successfully");
        result.Email.Should().Be(user.Email);
        
        _authServiceMock.Verify(x => x.GenerateJwtTokenAsync(user), Times.Once);
        _authServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
        _authServiceMock.Verify(x => x.MarkRefreshTokenAsUsedAsync(refreshToken), Times.Once);
        _authServiceMock.Verify(x => x.SaveRefreshTokenAsync(
            user, 
            "new-refresh-token", 
            refreshToken.DeviceName,
            refreshToken.DeviceIp,
            refreshToken.UserAgent), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_EmptyRefreshToken_ReturnsFailureResult()
    {
        // Arrange
        var command = new RefreshTokenCommand("");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("empty refresh token should fail validation");
        result.Message.Should().Be("Refresh token is required");
        
        _authServiceMock.Verify(x => x.FindRefreshTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_NullRefreshToken_ReturnsFailureResult()
    {
        // Arrange
        var command = new RefreshTokenCommand(null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("null refresh token should fail validation");
        result.Message.Should().Be("Refresh token is required");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_TokenNotFound_ReturnsFailureResult()
    {
        // Arrange
        var command = new RefreshTokenCommand("non-existent-token");
        
        _authServiceMock.Setup(x => x.FindRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync((RefreshToken)null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("non-existent token should fail");
        result.Message.Should().Be("Invalid refresh token");
        
        _authServiceMock.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_UsedToken_ReturnsFailureResult()
    {
        // Arrange
        var refreshToken = new RefreshTokenBuilder()
            .WithToken("used-token")
            .AsUsed()
            .Build();
        
        var command = new RefreshTokenCommand("used-token");
        
        _authServiceMock.Setup(x => x.FindRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("used token should fail");
        result.Message.Should().Be("Refresh token has been used");
        
        _authServiceMock.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_RevokedToken_ReturnsFailureResult()
    {
        // Arrange
        var refreshToken = new RefreshTokenBuilder()
            .WithToken("revoked-token")
            .AsRevoked()
            .Build();
        
        var command = new RefreshTokenCommand("revoked-token");
        
        _authServiceMock.Setup(x => x.FindRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("revoked token should fail");
        result.Message.Should().Be("Refresh token has been revoked");
        
        _authServiceMock.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_ExpiredToken_ReturnsFailureResult()
    {
        // Arrange
        var refreshToken = new RefreshTokenBuilder()
            .WithToken("expired-token")
            .AsExpired()
            .Build();
        
        var command = new RefreshTokenCommand("expired-token");
        
        _authServiceMock.Setup(x => x.FindRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("expired token should fail");
        result.Message.Should().Be("Refresh token has expired");
        
        _authServiceMock.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_TokenWithoutUser_ReturnsFailureResult()
    {
        // Arrange
        var refreshToken = new RefreshTokenBuilder()
            .WithToken("token-without-user")
            .WithUser(null!)
            .Build();
        
        var command = new RefreshTokenCommand("token-without-user");
        
        _authServiceMock.Setup(x => x.FindRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("token without user should fail");
        result.Message.Should().Be("User not found");
        
        _authServiceMock.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_ExceptionDuringTokenGeneration_ReturnsFailureResult()
    {
        // Arrange
        var user = new ApplicationUserBuilder()
            .WithEmail("test@example.com")
            .Build();
        
        var refreshToken = new RefreshTokenBuilder()
            .WithToken("valid-token")
            .WithUser(user)
            .AsValid()
            .Build();
        
        var command = new RefreshTokenCommand("valid-token");
        
        _authServiceMock.Setup(x => x.FindRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(refreshToken);
        
        _authServiceMock.Setup(x => x.GenerateJwtTokenAsync(user))
            .ThrowsAsync(new InvalidOperationException("JWT generation failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("exception during token generation should fail");
        result.Message.Should().Be("Failed to refresh token");
        
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_ExceptionDuringMarkAsUsed_ReturnsFailureResult()
    {
        // Arrange
        var user = new ApplicationUserBuilder()
            .WithEmail("test@example.com")
            .Build();
        
        var refreshToken = new RefreshTokenBuilder()
            .WithToken("valid-token")
            .WithUser(user)
            .AsValid()
            .Build();
        
        var command = new RefreshTokenCommand("valid-token");
        
        _authServiceMock.Setup(x => x.FindRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(refreshToken);
        
        _authServiceMock.Setup(x => x.GenerateJwtTokenAsync(user))
            .ReturnsAsync("new-jwt-token");
        
        _authServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");
        
        _authServiceMock.Setup(x => x.MarkRefreshTokenAsUsedAsync(refreshToken))
            .ThrowsAsync(new InvalidOperationException("Database error"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("exception during mark as used should fail");
        result.Message.Should().Be("Failed to refresh token");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_ValidRefreshToken_PreservesDeviceInfo()
    {
        // Arrange
        var user = new ApplicationUserBuilder()
            .WithEmail("test@example.com")
            .Build();
        
        var refreshToken = new RefreshTokenBuilder()
            .WithToken("valid-token")
            .WithUser(user)
            .WithDeviceName("iPhone 14")
            .WithDeviceIp("192.168.1.100")
            .WithUserAgent("Mozilla/5.0 iPhone")
            .AsValid()
            .Build();
        
        var command = new RefreshTokenCommand("valid-token");
        
        _authServiceMock.Setup(x => x.FindRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(refreshToken);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _authServiceMock.Verify(x => x.SaveRefreshTokenAsync(
            user,
            It.IsAny<string>(),
            "iPhone 14",
            "192.168.1.100",
            "Mozilla/5.0 iPhone"), 
            Times.Once,
            "device info should be preserved when creating new refresh token");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_SuccessfulRefresh_LogsInformation()
    {
        // Arrange
        var user = new ApplicationUserBuilder()
            .WithEmail("test@example.com")
            .Build();
        
        var refreshToken = new RefreshTokenBuilder()
            .WithToken("valid-token")
            .WithUser(user)
            .AsValid()
            .Build();
        
        var command = new RefreshTokenCommand("valid-token");
        
        _authServiceMock.Setup(x => x.FindRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(refreshToken);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "successful token refresh should be logged");
    }

    [Theory]
    [InlineData("short-token")]
    [InlineData("very-long-refresh-token-string-with-many-characters-123456789")]
    [InlineData("token-with-special-chars-!@#$%")]
    [Trait("Category", "Unit")]
    public async Task Handle_VariousValidTokenFormats_ProcessesCorrectly(string tokenValue)
    {
        // Arrange
        var user = new ApplicationUserBuilder().Build();
        var refreshToken = new RefreshTokenBuilder()
            .WithToken(tokenValue)
            .WithUser(user)
            .AsValid()
            .Build();
        
        var command = new RefreshTokenCommand(tokenValue);
        
        _authServiceMock.Setup(x => x.FindRefreshTokenAsync(tokenValue))
            .ReturnsAsync(refreshToken);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("valid token formats should be processed correctly");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_ExceptionDuringSaveNewToken_ReturnsFailureResult()
    {
        // Arrange
        var user = new ApplicationUserBuilder()
            .WithEmail("test@example.com")
            .Build();
        
        var refreshToken = new RefreshTokenBuilder()
            .WithToken("valid-token")
            .WithUser(user)
            .AsValid()
            .Build();
        
        var command = new RefreshTokenCommand("valid-token");
        
        _authServiceMock.Setup(x => x.FindRefreshTokenAsync(command.RefreshToken))
            .ReturnsAsync(refreshToken);
        
        _authServiceMock.Setup(x => x.GenerateJwtTokenAsync(user))
            .ReturnsAsync("new-jwt-token");
        
        _authServiceMock.Setup(x => x.GenerateRefreshToken())
            .Returns("new-refresh-token");
        
        _authServiceMock.Setup(x => x.MarkRefreshTokenAsUsedAsync(refreshToken))
            .Returns(Task.CompletedTask);
        
        _authServiceMock.Setup(x => x.SaveRefreshTokenAsync(
                It.IsAny<ApplicationUser>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Save failed"));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("exception during save should fail");
        result.Message.Should().Be("Failed to refresh token");
    }
}