using FluentAssertions;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Commands;
using Zarichney.Services.Auth.Models;
using Zarichney.Server.Tests.Framework.Mocks.Factories;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Services.Auth.Commands;

public class LoginCommandTests
{
  private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
  private readonly Mock<ILogger<LoginCommandHandler>> _loggerMock;
  private readonly Mock<IAuthService> _authServiceMock;
  private readonly LoginCommandHandler _handler;

  public LoginCommandTests()
  {
    _userManagerMock = UserManagerMockFactory.CreateDefault();
    _loggerMock = new Mock<ILogger<LoginCommandHandler>>();
    _authServiceMock = AuthServiceMockFactory.CreateDefault();
    _handler = new LoginCommandHandler(_userManagerMock.Object, _loggerMock.Object, _authServiceMock.Object);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ValidCredentials_ReturnsSuccessResult()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .WithEmailConfirmed(true)
        .Build();

    var command = new LoginCommand("test@example.com", "ValidPassword123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password))
        .ReturnsAsync(true);

    _authServiceMock.Setup(x => x.GenerateJwtTokenAsync(user))
        .ReturnsAsync("jwt-token");

    _authServiceMock.Setup(x => x.GenerateRefreshToken())
        .Returns("refresh-token");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue("valid credentials should result in successful login");
    result.Message.Should().Be("Login successful");
    result.Email.Should().Be(user.Email);

    _authServiceMock.Verify(x => x.GenerateJwtTokenAsync(user), Times.Once);
    _authServiceMock.Verify(x => x.GenerateRefreshToken(), Times.Once);
    _authServiceMock.Verify(x => x.SaveRefreshTokenAsync(user, "refresh-token",
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmptyEmail_ReturnsFailureResult()
  {
    // Arrange
    var command = new LoginCommand("", "Password123!");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("empty email should fail validation");
    result.Message.Should().Be("Email is required");

    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
    _authServiceMock.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_NullEmail_ReturnsFailureResult()
  {
    // Arrange
    var command = new LoginCommand(null!, "Password123!");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("null email should fail validation");
    result.Message.Should().Be("Email is required");

    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmptyPassword_ReturnsFailureResult()
  {
    // Arrange
    var command = new LoginCommand("test@example.com", "");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("empty password should fail validation");
    result.Message.Should().Be("A password is required");

    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_NullPassword_ReturnsFailureResult()
  {
    // Arrange
    var command = new LoginCommand("test@example.com", null!);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("null password should fail validation");
    result.Message.Should().Be("A password is required");

    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_UserNotFound_ReturnsFailureResult()
  {
    // Arrange
    var command = new LoginCommand("nonexistent@example.com", "Password123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync((ApplicationUser)null!);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("non-existent user should fail login");
    result.Message.Should().Be("Invalid email or password");

    _userManagerMock.Verify(x => x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
    _authServiceMock.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_InvalidPassword_ReturnsFailureResult()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .Build();

    var command = new LoginCommand("test@example.com", "WrongPassword");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password))
        .ReturnsAsync(false);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("invalid password should fail login");
    result.Message.Should().Be("Invalid email or password");

    _authServiceMock.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmailNotConfirmed_ReturnsFailureResult()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .WithEmailConfirmed(false)
        .Build();

    var command = new LoginCommand("test@example.com", "ValidPassword123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password))
        .ReturnsAsync(true);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("unconfirmed email should fail login");
    result.Message.Should().Be("Please verify your email address before logging in");

    _authServiceMock.Verify(x => x.GenerateJwtTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ExceptionDuringTokenGeneration_ReturnsFailureResult()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .WithEmailConfirmed(true)
        .Build();

    var command = new LoginCommand("test@example.com", "ValidPassword123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password))
        .ReturnsAsync(true);

    _authServiceMock.Setup(x => x.GenerateJwtTokenAsync(user))
        .ThrowsAsync(new InvalidOperationException("Token generation failed"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("exception during token generation should fail login");
    result.Message.Should().Be("Failed to login");

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "exception should be logged");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ExceptionDuringSaveRefreshToken_ReturnsFailureResult()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .WithEmailConfirmed(true)
        .Build();

    var command = new LoginCommand("test@example.com", "ValidPassword123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password))
        .ReturnsAsync(true);

    _authServiceMock.Setup(x => x.GenerateJwtTokenAsync(user))
        .ReturnsAsync("jwt-token");

    _authServiceMock.Setup(x => x.GenerateRefreshToken())
        .Returns("refresh-token");

    _authServiceMock.Setup(x => x.SaveRefreshTokenAsync(user, It.IsAny<string>(),
        It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
        .ThrowsAsync(new InvalidOperationException("Failed to save refresh token"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("exception during refresh token save should fail login");
    result.Message.Should().Be("Failed to login");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_SuccessfulLogin_LogsInformation()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .WithEmailConfirmed(true)
        .Build();

    var command = new LoginCommand("test@example.com", "ValidPassword123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password))
        .ReturnsAsync(true);

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
        "successful login should be logged");
  }

  [Theory]
  [InlineData("admin@example.com", "AdminPass123!")]
  [InlineData("user@test.org", "UserPassword!")]
  [InlineData("test.user+tag@domain.co.uk", "ComplexPass123!@#")]
  [Trait("Category", "Unit")]
  public async Task Handle_VariousValidCredentials_ReturnsSuccessResult(string email, string password)
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail(email)
        .WithEmailConfirmed(true)
        .Build();

    var command = new LoginCommand(email, password);

    _userManagerMock.Setup(x => x.FindByEmailAsync(email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.CheckPasswordAsync(user, password))
        .ReturnsAsync(true);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue("valid credentials should always result in successful login");
    result.Email.Should().Be(email);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_UserManagerFindByEmailThrows_ReturnsFailureResult()
  {
    // Arrange
    var command = new LoginCommand("test@example.com", "Password123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ThrowsAsync(new InvalidOperationException("Database connection failed"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("exception during user lookup should fail login");
    result.Message.Should().Be("Failed to login");

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
  public async Task Handle_LockedOutUser_ShouldStillCheckPassword()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .WithEmailConfirmed(true)
        .AsLockedOut()
        .Build();

    var command = new LoginCommand("test@example.com", "ValidPassword123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.CheckPasswordAsync(user, command.Password))
        .ReturnsAsync(true);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue("locked out user is not checked in login handler - handled by Identity middleware");

    _authServiceMock.Verify(x => x.GenerateJwtTokenAsync(user), Times.Once);
  }
}
