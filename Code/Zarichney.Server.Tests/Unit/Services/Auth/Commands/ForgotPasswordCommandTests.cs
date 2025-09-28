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
using Zarichney.Server.Tests.Framework.Mocks.Factories;
using Zarichney.Server.Tests.TestData.Builders;
using Microsoft.Graph.Models;

namespace Zarichney.Server.Tests.Unit.Services.Auth.Commands;

public class ForgotPasswordCommandTests
{
  private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
  private readonly Mock<ILogger<ForgotPasswordCommandHandler>> _loggerMock;
  private readonly Mock<IEmailService> _emailServiceMock;
  private readonly ClientConfig _clientConfig;
  private readonly ForgotPasswordCommandHandler _handler;

  public ForgotPasswordCommandTests()
  {
    _userManagerMock = UserManagerMockFactory.CreateDefault();
    _loggerMock = new Mock<ILogger<ForgotPasswordCommandHandler>>();
    _emailServiceMock = new Mock<IEmailService>();
    _clientConfig = new ClientConfig { BaseUrl = "https://test.example.com" };
    _handler = new ForgotPasswordCommandHandler(
        _userManagerMock.Object,
        _loggerMock.Object,
        _emailServiceMock.Object,
        _clientConfig);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ValidEmail_SendsResetEmailAndReturnsSuccess()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .Build();

    var command = new ForgotPasswordCommand("test@example.com");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
        .ReturnsAsync("reset-token-123");

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
    result.Success.Should().BeTrue("because the password reset process succeeded");
    result.Message.Should().Be("Password reset link has been sent to your email");

    _emailServiceMock.Verify(x => x.SendEmail(
        "test@example.com",
        "Reset Your Password",
        "password-reset",
        It.IsAny<Dictionary<string, object>>(),
        It.IsAny<FileAttachment>()),
        Times.Once);

    capturedTemplateData.Should().NotBeNull();
    capturedTemplateData!["title"].Should().Be("Zarichney Development");
    capturedTemplateData["reset_url"].Should().Be($"https://test.example.com/auth/reset-password?email=test%40example.com&token=reset-token-123");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmptyEmail_ReturnsFailure()
  {
    // Arrange
    var command = new ForgotPasswordCommand("");

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
    var command = new ForgotPasswordCommand(null!);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because email is required");
    result.Message.Should().Be("Email is required");

    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_UserNotFound_ReturnsGenericSuccessMessage()
  {
    // Arrange
    var command = new ForgotPasswordCommand("nonexistent@example.com");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync((ApplicationUser)null!);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeTrue("because we don't reveal if email exists for security");
    result.Message.Should().Be("If your email is registered, a password reset link will be sent to your inbox");

    _userManagerMock.Verify(x => x.GeneratePasswordResetTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    _emailServiceMock.Verify(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<FileAttachment>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmailServiceThrows_LogsErrorButReturnsSuccess()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .Build();

    var command = new ForgotPasswordCommand("test@example.com");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
        .ReturnsAsync("reset-token");

    _emailServiceMock.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<FileAttachment>()))
        .ThrowsAsync(new SmtpException("SMTP server unavailable"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeTrue("because we handle email failures gracefully");
    result.Message.Should().Be("Password reset link has been sent to your email");

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Failed to send password reset email")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_GenerateTokenThrows_ReturnsFailure()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .Build();

    var command = new ForgotPasswordCommand("test@example.com");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
        .ThrowsAsync(new InvalidOperationException("Token generation failed"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because token generation failed");
    result.Message.Should().Be("Failed to process password reset request");

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Error during password reset request")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmailWithSpecialCharacters_ProperlyEncodesInUrl()
  {
    // Arrange
    var email = "test+special@example.com";
    var user = new ApplicationUserBuilder()
        .WithEmail(email)
        .Build();

    var command = new ForgotPasswordCommand(email);

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
        .ReturnsAsync("token/with+special=chars");

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
    result.Success.Should().BeTrue();

    capturedTemplateData.Should().NotBeNull();
    var resetUrl = capturedTemplateData!["reset_url"] as string;
    resetUrl.Should().Contain("test%2Bspecial%40example.com", "because email should be URL encoded");
    resetUrl.Should().Contain("token%2Fwith%2Bspecial%3Dchars", "because token should be URL encoded");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_SuccessfulReset_LogsInformation()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .Build();

    var command = new ForgotPasswordCommand("test@example.com");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.GeneratePasswordResetTokenAsync(user))
        .ReturnsAsync("reset-token");

    _emailServiceMock.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<FileAttachment>()))
        .Returns(Task.CompletedTask);

    // Act
    await _handler.Handle(command, CancellationToken.None);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Password reset email sent to")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Password reset token generated for user")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }
}

// Helper exception for email testing
public class SmtpException : Exception
{
  public SmtpException(string message) : base(message) { }
}
