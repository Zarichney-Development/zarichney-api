using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Config;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Commands;
using Zarichney.Services.Auth.Models;
using Zarichney.Tests.Framework.Mocks.Factories;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Services.Auth.Commands;

public class ConfirmEmailCommandTests
{
  private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
  private readonly Mock<ILogger<ConfirmEmailCommandHandler>> _loggerMock;
  private readonly Mock<IAuthService> _authServiceMock;
  private readonly ClientConfig _clientConfig;
  private readonly ConfirmEmailCommandHandler _handler;

  public ConfirmEmailCommandTests()
  {
    _userManagerMock = UserManagerMockFactory.CreateDefault();
    _loggerMock = new Mock<ILogger<ConfirmEmailCommandHandler>>();
    _authServiceMock = AuthServiceMockFactory.CreateDefault();
    _clientConfig = new ClientConfig { BaseUrl = "https://test.example.com" };
    _handler = new ConfirmEmailCommandHandler(
        _userManagerMock.Object,
        _loggerMock.Object,
        _clientConfig,
        _authServiceMock.Object);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ValidTokenAndUnconfirmedEmail_ReturnsSuccessResultWithTokens()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithId("test-user-id")
        .WithEmail("test@example.com")
        .WithEmailConfirmed(false)
        .Build();

    var command = new ConfirmEmailCommand("test-user-id", "valid-token");

    _userManagerMock.Setup(x => x.FindByIdAsync(command.UserId))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, command.Token))
        .ReturnsAsync(IdentityResult.Success);

    _authServiceMock.Setup(x => x.GenerateJwtTokenAsync(user))
        .ReturnsAsync("jwt-token");

    _authServiceMock.Setup(x => x.GenerateRefreshToken())
        .Returns("refresh-token");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeTrue("because email confirmation succeeded");
    result.Email.Should().Be("test@example.com");
    result.AccessToken.Should().Be("jwt-token");
    result.RefreshToken.Should().Be("refresh-token");
    result.RedirectUrl.Should().Be("https://test.example.com/auth/email-confirmation");
    result.Message.Should().Be("Email has been confirmed");

    _userManagerMock.Verify(x => x.ConfirmEmailAsync(user, command.Token), Times.Once);
    _authServiceMock.Verify(x => x.SaveRefreshTokenAsync(user, "refresh-token", "Email Confirmation", null, null), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_AlreadyConfirmedEmail_ReturnsSuccessResultWithoutCallingConfirm()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithId("test-user-id")
        .WithEmail("test@example.com")
        .WithEmailConfirmed(true)
        .Build();

    var command = new ConfirmEmailCommand("test-user-id", "valid-token");

    _userManagerMock.Setup(x => x.FindByIdAsync(command.UserId))
        .ReturnsAsync(user);

    _authServiceMock.Setup(x => x.GenerateJwtTokenAsync(user))
        .ReturnsAsync("jwt-token");

    _authServiceMock.Setup(x => x.GenerateRefreshToken())
        .Returns("refresh-token");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeTrue("because email is already confirmed");
    result.Email.Should().Be("test@example.com");
    result.AccessToken.Should().Be("jwt-token");
    result.RefreshToken.Should().Be("refresh-token");
    result.Message.Should().Be("Email has been confirmed");

    _userManagerMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmptyUserId_ReturnsFailureResult()
  {
    // Arrange
    var command = new ConfirmEmailCommand("", "valid-token");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because user ID is required");
    result.Message.Should().Be("User ID is required");

    _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_NullUserId_ReturnsFailureResult()
  {
    // Arrange
    var command = new ConfirmEmailCommand(null!, "valid-token");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because user ID is required");
    result.Message.Should().Be("User ID is required");

    _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmptyToken_ReturnsFailureResult()
  {
    // Arrange
    var command = new ConfirmEmailCommand("test-user-id", "");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because confirmation token is required");
    result.Message.Should().Be("Confirmation token is required");

    _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_NullToken_ReturnsFailureResult()
  {
    // Arrange
    var command = new ConfirmEmailCommand("test-user-id", null!);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because confirmation token is required");
    result.Message.Should().Be("Confirmation token is required");

    _userManagerMock.Verify(x => x.FindByIdAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_UserNotFound_ReturnsFailureResult()
  {
    // Arrange
    var command = new ConfirmEmailCommand("non-existent-user", "valid-token");

    _userManagerMock.Setup(x => x.FindByIdAsync(command.UserId))
        .ReturnsAsync((ApplicationUser)null!);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because user was not found");
    result.Message.Should().Be("Invalid user ID");

    _userManagerMock.Verify(x => x.ConfirmEmailAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_InvalidToken_ReturnsFailureResult()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithId("test-user-id")
        .WithEmail("test@example.com")
        .WithEmailConfirmed(false)
        .Build();

    var command = new ConfirmEmailCommand("test-user-id", "invalid-token");

    _userManagerMock.Setup(x => x.FindByIdAsync(command.UserId))
        .ReturnsAsync(user);

    var errors = new[] { new IdentityError { Code = "InvalidToken", Description = "Invalid token" } };
    _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, command.Token))
        .ReturnsAsync(IdentityResult.Failed(errors));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because the token was invalid");
    result.Message.Should().Be("Invalid or expired confirmation token");

    _authServiceMock.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ConfirmEmailThrowsException_ReturnsFailureResult()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithId("test-user-id")
        .WithEmail("test@example.com")
        .WithEmailConfirmed(false)
        .Build();

    var command = new ConfirmEmailCommand("test-user-id", "valid-token");

    _userManagerMock.Setup(x => x.FindByIdAsync(command.UserId))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, command.Token))
        .ThrowsAsync(new InvalidOperationException("Database error"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because an exception occurred");
    result.Message.Should().Be("Failed to confirm email");

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Error during email confirmation")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_MultipleIdentityErrors_CombinesErrorMessages()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithId("test-user-id")
        .WithEmail("test@example.com")
        .WithEmailConfirmed(false)
        .Build();

    var command = new ConfirmEmailCommand("test-user-id", "invalid-token");

    _userManagerMock.Setup(x => x.FindByIdAsync(command.UserId))
        .ReturnsAsync(user);

    var errors = new[]
    {
      new IdentityError { Code = "InvalidToken", Description = "Invalid token" },
      new IdentityError { Code = "ExpiredToken", Description = "Token has expired" }
    };
    _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, command.Token))
        .ReturnsAsync(IdentityResult.Failed(errors));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because email confirmation failed with multiple errors");
    result.Message.Should().Be("Invalid or expired confirmation token");

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Invalid token, Token has expired")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_SaveRefreshTokenFails_ReturnsFailureResult()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithId("test-user-id")
        .WithEmail("test@example.com")
        .WithEmailConfirmed(false)
        .Build();

    var command = new ConfirmEmailCommand("test-user-id", "valid-token");

    _userManagerMock.Setup(x => x.FindByIdAsync(command.UserId))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.ConfirmEmailAsync(user, command.Token))
        .ReturnsAsync(IdentityResult.Success);

    _authServiceMock.Setup(x => x.GenerateJwtTokenAsync(user))
        .ReturnsAsync("jwt-token");

    _authServiceMock.Setup(x => x.GenerateRefreshToken())
        .Returns("refresh-token");

    _authServiceMock.Setup(x => x.SaveRefreshTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ThrowsAsync(new Exception("Database save failed"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because an exception occurred during token save");
    result.Message.Should().Be("Failed to confirm email");

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Error during email confirmation")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }
}
