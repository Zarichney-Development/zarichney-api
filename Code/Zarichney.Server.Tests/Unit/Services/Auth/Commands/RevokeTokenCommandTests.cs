using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Commands;
using Zarichney.Services.Auth.Models;

namespace Zarichney.Server.Tests.Unit.Services.Auth.Commands;

public class RevokeTokenCommandTests
{
    private readonly Mock<ILogger<RevokeTokenCommandHandler>> _mockLogger;
    private readonly Mock<IAuthService> _mockAuthService;
    private readonly RevokeTokenCommandHandler _handler;

    public RevokeTokenCommandTests()
    {
        _mockLogger = new Mock<ILogger<RevokeTokenCommandHandler>>();
        _mockAuthService = new Mock<IAuthService>();
        _handler = new RevokeTokenCommandHandler(_mockLogger.Object, _mockAuthService.Object);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithValidRefreshToken_ReturnsSuccessAndRevokesToken()
    {
        // Arrange
        var refreshToken = "valid-refresh-token-123";
        var command = new RevokeTokenCommand(refreshToken);

        var tokenEntity = new RefreshToken
        {
            Id = 1,
            Token = refreshToken,
            UserId = "test-user-123",
            IsRevoked = false,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow.AddMinutes(-30),
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        _mockAuthService.Setup(x => x.FindRefreshTokenAsync(refreshToken))
            .ReturnsAsync(tokenEntity);

        _mockAuthService.Setup(x => x.RevokeRefreshTokenAsync(tokenEntity))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("because the token was successfully revoked");
        result.Message.Should().Be("Refresh token revoked successfully");
        result.Email.Should().BeNull();
        result.AccessToken.Should().BeNull();
        result.RefreshToken.Should().BeNull();

        _mockAuthService.Verify(x => x.FindRefreshTokenAsync(refreshToken), Times.Once);
        _mockAuthService.Verify(x => x.RevokeRefreshTokenAsync(tokenEntity), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithEmptyRefreshToken_ReturnsFailure()
    {
        // Arrange
        var command = new RevokeTokenCommand("");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("because refresh token is required");
        result.Message.Should().Be("Refresh token is required");

        _mockAuthService.Verify(x => x.FindRefreshTokenAsync(It.IsAny<string>()), Times.Never);
        _mockAuthService.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithNullRefreshToken_ReturnsFailure()
    {
        // Arrange
        var command = new RevokeTokenCommand(null!);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("because refresh token cannot be null");
        result.Message.Should().Be("Refresh token is required");

        _mockAuthService.Verify(x => x.FindRefreshTokenAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithNonExistentRefreshToken_ReturnsFailure()
    {
        // Arrange
        var refreshToken = "non-existent-token";
        var command = new RevokeTokenCommand(refreshToken);

        _mockAuthService.Setup(x => x.FindRefreshTokenAsync(refreshToken))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("because the refresh token does not exist");
        result.Message.Should().Be("Invalid refresh token");

        _mockAuthService.Verify(x => x.FindRefreshTokenAsync(refreshToken), Times.Once);
        _mockAuthService.Verify(x => x.RevokeRefreshTokenAsync(It.IsAny<RefreshToken>()), Times.Never);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithExpiredToken_StillRevokesSuccessfully()
    {
        // Arrange
        var refreshToken = "expired-refresh-token";
        var command = new RevokeTokenCommand(refreshToken);

        var expiredTokenEntity = new RefreshToken
        {
            Id = 2,
            Token = refreshToken,
            UserId = "test-user-456",
            IsRevoked = false,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow.AddDays(-30),
            ExpiresAt = DateTime.UtcNow.AddDays(-1) // Expired yesterday
        };

        _mockAuthService.Setup(x => x.FindRefreshTokenAsync(refreshToken))
            .ReturnsAsync(expiredTokenEntity);

        _mockAuthService.Setup(x => x.RevokeRefreshTokenAsync(expiredTokenEntity))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("because even expired tokens should be revokable");
        result.Message.Should().Be("Refresh token revoked successfully");

        _mockAuthService.Verify(x => x.RevokeRefreshTokenAsync(expiredTokenEntity), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithAlreadyRevokedToken_StillReturnsSuccess()
    {
        // Arrange
        var refreshToken = "already-revoked-token";
        var command = new RevokeTokenCommand(refreshToken);

        var revokedTokenEntity = new RefreshToken
        {
            Id = 3,
            Token = refreshToken,
            UserId = "test-user-789",
            IsRevoked = true, // Already revoked
            IsUsed = false,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            LastUsedAt = DateTime.UtcNow.AddHours(-1)
        };

        _mockAuthService.Setup(x => x.FindRefreshTokenAsync(refreshToken))
            .ReturnsAsync(revokedTokenEntity);

        _mockAuthService.Setup(x => x.RevokeRefreshTokenAsync(revokedTokenEntity))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("because revoking an already revoked token should not fail");
        result.Message.Should().Be("Refresh token revoked successfully");

        _mockAuthService.Verify(x => x.RevokeRefreshTokenAsync(revokedTokenEntity), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WhenExceptionOccurs_ReturnsFailureAndLogsError()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";
        var command = new RevokeTokenCommand(refreshToken);

        var expectedException = new InvalidOperationException("Database connection failed");

        _mockAuthService.Setup(x => x.FindRefreshTokenAsync(refreshToken))
            .ThrowsAsync(expectedException);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("because an exception occurred");
        result.Message.Should().Be("Failed to revoke token");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Error during token revocation")),
                expectedException,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_Success_LogsInformationMessage()
    {
        // Arrange
        var refreshToken = "valid-refresh-token";
        var command = new RevokeTokenCommand(refreshToken);

        var tokenEntity = new RefreshToken
        {
            Id = 4,
            Token = refreshToken,
            UserId = "test-user-999",
            IsRevoked = false,
            IsUsed = false
        };

        _mockAuthService.Setup(x => x.FindRefreshTokenAsync(refreshToken))
            .ReturnsAsync(tokenEntity);

        _mockAuthService.Setup(x => x.RevokeRefreshTokenAsync(tokenEntity))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) => o.ToString()!.Contains("Refresh token revoked successfully")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithWhitespaceToken_ReturnsFailure()
    {
        // Arrange
        var command = new RevokeTokenCommand("   ");

        _mockAuthService.Setup(x => x.FindRefreshTokenAsync("   "))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse("because whitespace-only token is invalid");
        result.Message.Should().Be("Invalid refresh token");

        _mockAuthService.Verify(x => x.FindRefreshTokenAsync("   "), Times.Once);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public async Task Handle_WithVeryLongToken_ProcessesNormally()
    {
        // Arrange
        var veryLongToken = new string('a', 1000); // 1000 character token
        var command = new RevokeTokenCommand(veryLongToken);

        var tokenEntity = new RefreshToken
        {
            Id = 5,
            Token = veryLongToken,
            UserId = "test-user-long",
            IsRevoked = false,
            IsUsed = false
        };

        _mockAuthService.Setup(x => x.FindRefreshTokenAsync(veryLongToken))
            .ReturnsAsync(tokenEntity);

        _mockAuthService.Setup(x => x.RevokeRefreshTokenAsync(tokenEntity))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue("because token length should not affect revocation");
        result.Message.Should().Be("Refresh token revoked successfully");
    }
}