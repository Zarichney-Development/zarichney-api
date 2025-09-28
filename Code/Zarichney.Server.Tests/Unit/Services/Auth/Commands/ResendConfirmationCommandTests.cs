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

public class ResendConfirmationCommandTests
{
  private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
  private readonly Mock<ILogger<ResendConfirmationCommandHandler>> _loggerMock;
  private readonly Mock<IEmailService> _emailServiceMock;
  private readonly ServerConfig _serverConfig;
  private readonly ResendConfirmationCommandHandler _handler;

  public ResendConfirmationCommandTests()
  {
    _userManagerMock = UserManagerMockFactory.CreateDefault();
    _loggerMock = new Mock<ILogger<ResendConfirmationCommandHandler>>();
    _emailServiceMock = new Mock<IEmailService>();
    _serverConfig = new ServerConfig { BaseUrl = "https://api.test.example.com" };
    _handler = new ResendConfirmationCommandHandler(
        _userManagerMock.Object,
        _loggerMock.Object,
        _emailServiceMock.Object,
        _serverConfig);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_UnconfirmedEmail_SendsVerificationEmailAndReturnsSuccess()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithId("user-123")
        .WithEmail("test@example.com")
        .WithEmailConfirmed(false)
        .Build();

    var command = new ResendConfirmationCommand("test@example.com");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
        .ReturnsAsync("confirmation-token-123");

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
    result.Success.Should().BeTrue("because the email verification was sent");
    result.Message.Should().Be("Verification email has been sent to your email address");

    _emailServiceMock.Verify(x => x.SendEmail(
        "test@example.com",
        "Verify Your Email Address",
        "email-verification",
        It.IsAny<Dictionary<string, object>>(),
        It.IsAny<FileAttachment>()),
        Times.Once);

    capturedTemplateData.Should().NotBeNull();
    capturedTemplateData!["verification_url"].Should().Be($"https://api.test.example.com/api/auth/confirm-email?userId=user-123&token=confirmation-token-123");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmptyEmail_ReturnsFailure()
  {
    // Arrange
    var command = new ResendConfirmationCommand("");

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
    var command = new ResendConfirmationCommand(null!);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because email is required");
    result.Message.Should().Be("Email is required");

    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_UserNotFound_ReturnsFailure()
  {
    // Arrange
    var command = new ResendConfirmationCommand("nonexistent@example.com");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync((ApplicationUser)null!);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because user was not found");
    result.Message.Should().Be("No account found with this email address");

    _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_AlreadyConfirmedEmail_ReturnsFailure()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithEmail("test@example.com")
        .WithEmailConfirmed(true)
        .Build();

    var command = new ResendConfirmationCommand("test@example.com");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because email is already confirmed");
    result.Message.Should().Be("Email has already been confirmed");

    _userManagerMock.Verify(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()), Times.Never);
    _emailServiceMock.Verify(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<FileAttachment>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmailServiceThrows_StillReturnsSuccess()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithId("user-123")
        .WithEmail("test@example.com")
        .WithEmailConfirmed(false)
        .Build();

    var command = new ResendConfirmationCommand("test@example.com");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
        .ReturnsAsync("confirmation-token");

    _emailServiceMock.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<FileAttachment>()))
        .ThrowsAsync(new Exception("Email service unavailable"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeTrue("because token generation succeeded even if email failed");
    result.Message.Should().Be("Verification email has been sent to your email address");

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Failed to resend verification email")),
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
        .WithEmailConfirmed(false)
        .Build();

    var command = new ResendConfirmationCommand("test@example.com");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
        .ThrowsAsync(new InvalidOperationException("Token generation failed"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Success.Should().BeFalse("because token generation failed");
    result.Message.Should().Be("Failed to resend verification email");

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Error during email verification resend")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_SpecialCharactersInUserId_ProperlyEncodesInUrl()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithId("user+special/id=123")
        .WithEmail("test@example.com")
        .WithEmailConfirmed(false)
        .Build();

    var command = new ResendConfirmationCommand("test@example.com");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
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
    var verificationUrl = capturedTemplateData!["verification_url"] as string;
    verificationUrl.Should().Contain("user%2Bspecial%2Fid%3D123", "because user ID should be URL encoded");
    verificationUrl.Should().Contain("token%2Fwith%2Bspecial%3Dchars", "because token should be URL encoded");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_SuccessfulResend_LogsInformation()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithId("user-123")
        .WithEmail("test@example.com")
        .WithEmailConfirmed(false)
        .Build();

    var command = new ResendConfirmationCommand("test@example.com");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
        .ReturnsAsync("confirmation-token");

    _emailServiceMock.Setup(x => x.SendEmail(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<FileAttachment>()))
        .Returns(Task.CompletedTask);

    // Act
    await _handler.Handle(command, CancellationToken.None);

    // Assert
    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((o, t) => o != null && o.ToString()!.Contains("Email verification resent to")),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_VerificationUrlFormat_IncludesCorrectApiPath()
  {
    // Arrange
    var user = new ApplicationUserBuilder()
        .WithId("user-123")
        .WithEmail("test@example.com")
        .WithEmailConfirmed(false)
        .Build();

    var command = new ResendConfirmationCommand("test@example.com");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(user);

    _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(user))
        .ReturnsAsync("token");

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
    await _handler.Handle(command, CancellationToken.None);

    // Assert
    capturedTemplateData.Should().NotBeNull();
    var verificationUrl = capturedTemplateData!["verification_url"] as string;
    verificationUrl.Should().StartWith("https://api.test.example.com/api/auth/confirm-email");
    verificationUrl.Should().Contain("userId=user-123");
    verificationUrl.Should().Contain("token=token");
  }
}
