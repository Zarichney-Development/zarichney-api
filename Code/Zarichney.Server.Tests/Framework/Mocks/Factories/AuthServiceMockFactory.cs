using Moq;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Models;

namespace Zarichney.Tests.Framework.Mocks.Factories;

public static class AuthServiceMockFactory
{
    public static Mock<IAuthService> CreateDefault()
    {
        var mock = new Mock<IAuthService>();
        
        mock.Setup(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync("default-jwt-token");
        
        mock.Setup(x => x.GenerateRefreshToken())
            .Returns("default-refresh-token");
        
        var defaultRefreshToken = new RefreshToken
        {
            Token = "default-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        
        mock.Setup(x => x.SaveRefreshTokenAsync(
                It.IsAny<ApplicationUser>(), 
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(defaultRefreshToken);
        
        mock.Setup(x => x.FindRefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((RefreshToken)null!);
        
        mock.Setup(x => x.MarkRefreshTokenAsUsedAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);
        
        mock.Setup(x => x.RevokeRefreshTokenAsync(It.IsAny<RefreshToken>()))
            .Returns(Task.CompletedTask);
        
        return mock;
    }

    public static Mock<IAuthService> CreateWithTokenGeneration(string accessToken = "test-jwt-token", string refreshToken = "test-refresh-token")
    {
        var mock = new Mock<IAuthService>();
        
        mock.Setup(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(accessToken);
        
        mock.Setup(x => x.GenerateRefreshToken())
            .Returns(refreshToken);
        
        var tokenEntity = new RefreshToken
        {
            Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow
        };
        
        mock.Setup(x => x.SaveRefreshTokenAsync(
                It.IsAny<ApplicationUser>(), 
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ReturnsAsync(tokenEntity);
        
        return mock;
    }

    public static Mock<IAuthService> CreateWithValidRefreshToken(RefreshToken refreshToken)
    {
        var mock = CreateDefault();
        
        mock.Setup(x => x.FindRefreshTokenAsync(refreshToken.Token))
            .ReturnsAsync(refreshToken);
        
        return mock;
    }

    public static Mock<IAuthService> CreateWithInvalidRefreshToken()
    {
        var mock = CreateDefault();
        
        mock.Setup(x => x.FindRefreshTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((RefreshToken)null!);
        
        return mock;
    }

    public static Mock<IAuthService> CreateWithTokenGenerationFailure()
    {
        var mock = new Mock<IAuthService>();
        
        mock.Setup(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()))
            .ThrowsAsync(new InvalidOperationException("Failed to generate JWT token"));
        
        return mock;
    }

    public static Mock<IAuthService> CreateWithRefreshTokenSaveFailure()
    {
        var mock = CreateDefault();
        
        mock.Setup(x => x.SaveRefreshTokenAsync(
                It.IsAny<ApplicationUser>(), 
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Failed to save refresh token"));
        
        return mock;
    }
}