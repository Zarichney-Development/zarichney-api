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
using Zarichney.Services.Email;
using Zarichney.Tests.Framework.Mocks.Factories;
using Zarichney.Tests.TestData.Builders;
using Microsoft.Graph.Models;

namespace Zarichney.Tests.Unit.Services.Auth.Commands;

public class ResetPasswordCommandTests
{
  private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
  private readonly Mock<ILogger<ResetPasswordCommandHandler>> _loggerMock;
  private readonly Mock<IEmailService> _emailServiceMock;
  private readonly ClientConfig _clientConfig;
  private readonly ResetPasswordCommandHandler _handler;

  public ResetPasswordCommandTests()
  {
    _userManagerMock = UserManagerMockFactory.CreateDefault();
    _loggerMock = new Mock<ILogger<ResetPasswordCommandHandler>>();
    _emailServiceMock = new Mock<IEmailService>();
    _clientConfig = new ClientConfig { BaseUrl = "https://test.example.com" };
    _handler = new ResetPasswordCommandHandler(
        _userManagerMock.Object,
        _loggerMock.Object,
        _emailServiceMock.Object,
        _clientConfig);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ValidRequest_ResetsPasswordAndSendsEmail()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .Build();

    var command = new ResetPasswordCommand("test@example.com", "valid-token", "NewPassword123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword))
        .ReturnsAsync(IdentityResult.Success);

    Dictionary<string, object> capturedTemplateData = null!;
    _emailServiceMock.Setup(x => x.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<FileAttachment>()))
        .Callback<string, string, string, Dictionary<string, object>?, FileAttachment?>((_, _, _, data, _) => capturedTemplateData = data!)
        .Returns(Task.CompletedTask);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeTrue("because password reset succeeded");
    result.Message.Should().Be("Password has been reset successfully");

    _emailServiceMock.Verify(x => x.SendEmail(
        "test@example.com",
        "Your Password Has Been Reset",
        "password-reset-confirmation",
        It.IsAny<Dictionary<string, object>>(),
        It.IsAny<FileAttachment>()),
        Times.Once);

    capturedTemplateData.Should().NotBeNull();
    capturedTemplateData!["user_email"].Should().Be("test@example.com");
    capturedTemplateData["login_url"].Should().Be("https://test.example.com/login");
    capturedTemplateData["reset_time"].Should().NotBeNull();
    capturedTemplateData["reset_time"].ToString().Should().EndWith(" UTC");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmptyEmail_ReturnsFailure()
  {
    // Arrange
    var command = new ResetPasswordCommand("", "token", "password");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because email is required");
    result.Message.Should().Be("Email is required");

    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_NullEmail_ReturnsFailure()
  {
    // Arrange
    var command = new ResetPasswordCommand(null!, "token", "password");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because email is required");
    result.Message.Should().Be("Email is required");

    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmptyToken_ReturnsFailure()
  {
    // Arrange
    var command = new ResetPasswordCommand("test@example.com", "", "password");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because token is required");
    result.Message.Should().Be("Reset token is required");

    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_NullToken_ReturnsFailure()
  {
    // Arrange
    var command = new ResetPasswordCommand("test@example.com", null!, "password");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because token is required");
    result.Message.Should().Be("Reset token is required");

    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmptyPassword_ReturnsFailure()
  {
    // Arrange
    var command = new ResetPasswordCommand("test@example.com", "token", "");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because password is required");
    result.Message.Should().Be("New password is required");

    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_NullPassword_ReturnsFailure()
  {
    // Arrange
    var command = new ResetPasswordCommand("test@example.com", "token", null!);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because password is required");
    result.Message.Should().Be("New password is required");

    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_UserNotFound_ReturnsFailure()
  {
    // Arrange
    var command = new ResetPasswordCommand("nonexistent@example.com", "token", "password");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync((ApplicationUser)null!);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because user was not found");
    result.Message.Should().Be("Invalid email address");

    _userManagerMock.Verify(x => x.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_InvalidToken_ReturnsFailure()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .Build();

    var command = new ResetPasswordCommand("test@example.com", "invalid-token", "NewPassword123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    var errors = new[] { new IdentityError { Code = "InvalidToken", Description = "Invalid or expired token" } };
    _userManagerMock.Setup(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword))
        .ReturnsAsync(IdentityResult.Failed(errors));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because the token was invalid");
    result.Message.Should().Be("Invalid or expired token");

    _emailServiceMock.Verify(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<FileAttachment>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_WeakPassword_ReturnsPasswordRequirementsErrors()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .Build();

    var command = new ResetPasswordCommand("test@example.com", "valid-token", "weak");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    var errors = new[]
    {
      new IdentityError { Code = "PasswordTooShort", Description = "Password must be at least 6 characters" },
      new IdentityError { Code = "PasswordRequiresUpper", Description = "Password must have at least one uppercase character" }
    };
    _userManagerMock.Setup(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword))
        .ReturnsAsync(IdentityResult.Failed(errors));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because password didn't meet requirements");
    result.Message.Should().Contain("Password must be at least 6 characters");
    result.Message.Should().Contain("Password must have at least one uppercase character");

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Failed to reset password")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmailServiceThrows_StillReturnsSuccess()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .Build();

    var command = new ResetPasswordCommand("test@example.com", "valid-token", "NewPassword123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword))
        .ReturnsAsync(IdentityResult.Success);

    _emailServiceMock.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<FileAttachment>()))
        .ThrowsAsync(new Exception("Email service unavailable"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeTrue("because password reset succeeded despite email failure");
    result.Message.Should().Be("Password has been reset successfully");

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Failed to send password reset confirmation email")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ResetPasswordThrows_ReturnsFailure()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .Build();

    var command = new ResetPasswordCommand("test@example.com", "valid-token", "NewPassword123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword))
        .ThrowsAsync(new InvalidOperationException("Database error"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because an exception occurred");
    result.Message.Should().Be("Failed to reset password");

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Error during password reset")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_SuccessfulReset_LogsInformation()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .Build();

    var command = new ResetPasswordCommand("test@example.com", "valid-token", "NewPassword123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.ResetPasswordAsync(user, command.Token, command.NewPassword))
        .ReturnsAsync(IdentityResult.Success);

    _emailServiceMock.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<FileAttachment>()))
        .Returns(Task.CompletedTask);

    // Act
    await _handler.Handle(command, CancellationToken.None);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Password reset successful for user")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Password reset confirmation email sent")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }
}
