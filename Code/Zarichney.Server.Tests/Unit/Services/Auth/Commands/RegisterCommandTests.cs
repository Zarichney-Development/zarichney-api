using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.Models;
using Moq;
using Xunit;
using Zarichney.Config;
using Zarichney.Services.Auth;
using Zarichney.Services.Auth.Commands;
using Zarichney.Services.Email;
using Zarichney.Server.Tests.Framework.Mocks.Factories;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Services.Auth.Commands;

public class RegisterCommandTests
{
  private readonly Mock<UserManager<ApplicationUser>> _userManagerMock;
  private readonly Mock<ILogger<RegisterCommandHandler>> _loggerMock;
  private readonly Mock<IEmailService> _emailServiceMock;
  private readonly ServerConfig _serverConfig;
  private readonly RegisterCommandHandler _handler;

  public RegisterCommandTests()
  {
    _userManagerMock = UserManagerMockFactory.CreateDefault();
    _loggerMock = new Mock<ILogger<RegisterCommandHandler>>();
    _emailServiceMock = new Mock<IEmailService>();
    _serverConfig = new ServerConfig { BaseUrl = "https://test.example.com" };
    _handler = new RegisterCommandHandler(
        _userManagerMock.Object,
        _loggerMock.Object,
        _emailServiceMock.Object,
        _serverConfig);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ValidNewUser_ReturnsSuccessResult()
  {
    // Arrange
    var command = new RegisterCommand("newuser@example.com", "ValidPassword123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync((ApplicationUser)null!);

    _emailServiceMock.Setup(x => x.ValidateEmail(command.Email))
        .ReturnsAsync(true);

    _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
        .ReturnsAsync(IdentityResult.Success);

    _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
        .ReturnsAsync("confirmation-token");

    _emailServiceMock.Setup(x => x.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<FileAttachment>()))
        .Returns(Task.CompletedTask);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue("valid registration should succeed");
    result.Message.Should().Be("User registered successfully. Please check your email to verify your account.");
    result.Email.Should().Be(command.Email);

    _userManagerMock.Verify(x => x.CreateAsync(
        It.Is<ApplicationUser>(u =>
            u.Email == command.Email &&
            u.UserName == command.Email &&
            u.EmailConfirmed == false),
        command.Password), Times.Once);

    _emailServiceMock.Verify(x => x.SendEmail(
        command.Email,
        "Verify Your Email Address",
        "email-verification",
        It.IsAny<Dictionary<string, object>>(),
        It.IsAny<FileAttachment>()), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmptyEmail_ReturnsFailureResult()
  {
    // Arrange
    var command = new RegisterCommand("", "Password123!");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("empty email should fail validation");
    result.Message.Should().Be("Email is required");

    _userManagerMock.Verify(x => x.FindByEmailAsync(It.IsAny<string>()), Times.Never);
    _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_NullEmail_ReturnsFailureResult()
  {
    // Arrange
    var command = new RegisterCommand(null!, "Password123!");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("null email should fail validation");
    result.Message.Should().Be("Email is required");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmptyPassword_ReturnsFailureResult()
  {
    // Arrange
    var command = new RegisterCommand("test@example.com", "");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("empty password should fail validation");
    result.Message.Should().Be("A password is required");

    _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_NullPassword_ReturnsFailureResult()
  {
    // Arrange
    var command = new RegisterCommand("test@example.com", null!);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("null password should fail validation");
    result.Message.Should().Be("A password is required");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ExistingUser_ReturnsFailureResult()
  {
    // Arrange
    var existingUser = new ApplicationUserBuilder()
        .WithEmail("existing@example.com")
        .Build();

    var command = new RegisterCommand("existing@example.com", "Password123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync(existingUser);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("existing user should fail registration");
    result.Message.Should().Be("User with this email already exists");

    _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_InvalidEmailFormat_ReturnsFailureResult()
  {
    // Arrange
    var command = new RegisterCommand("invalid-email", "Password123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync((ApplicationUser)null!);

    _emailServiceMock.Setup(x => x.ValidateEmail(command.Email))
        .ReturnsAsync(false);

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("invalid email format should fail registration");
    result.Message.Should().Be("Invalid email address");

    _userManagerMock.Verify(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()), Times.Never);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_UserManagerCreateFails_ReturnsFailureResult()
  {
    // Arrange
    var command = new RegisterCommand("test@example.com", "weak");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync((ApplicationUser)null!);

    _emailServiceMock.Setup(x => x.ValidateEmail(command.Email))
        .ReturnsAsync(true);

    var errors = new[]
    {
            new IdentityError { Description = "Password too short" },
            new IdentityError { Description = "Password requires uppercase" }
        };

    _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
        .ReturnsAsync(IdentityResult.Failed(errors));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("weak password should fail registration");
    result.Message.Should().Be("Password too short, Password requires uppercase");

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmailServiceFailure_StillReturnsSuccess()
  {
    // Arrange
    var command = new RegisterCommand("test@example.com", "ValidPassword123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync((ApplicationUser)null!);

    _emailServiceMock.Setup(x => x.ValidateEmail(command.Email))
        .ReturnsAsync(true);

    _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
        .ReturnsAsync(IdentityResult.Success);

    _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
        .ReturnsAsync("confirmation-token");

    _emailServiceMock.Setup(x => x.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<FileAttachment>()))
        .ThrowsAsync(new InvalidOperationException("Email service unavailable"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue("email failure should not fail registration");
    result.Message.Should().Be("User registered successfully. Please check your email to verify your account.");

    _loggerMock.Verify(
        x => x.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
        Times.Once,
        "email failure should be logged");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_ExceptionDuringRegistration_ReturnsFailureResult()
  {
    // Arrange
    var command = new RegisterCommand("test@example.com", "Password123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ThrowsAsync(new InvalidOperationException("Database error"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("exception during registration should fail");
    result.Message.Should().Be("Failed to register user");

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
  public async Task Handle_VerificationUrlGeneration_ContainsCorrectParameters()
  {
    // Arrange
    var command = new RegisterCommand("test@example.com", "ValidPassword123!");
    var capturedTemplateData = (Dictionary<string, object>)null!;

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync((ApplicationUser)null!);

    _emailServiceMock.Setup(x => x.ValidateEmail(command.Email))
        .ReturnsAsync(true);

    _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
        .ReturnsAsync(IdentityResult.Success);

    _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
        .ReturnsAsync("confirmation-token");

    _emailServiceMock.Setup(x => x.SendEmail(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<FileAttachment>()))
        .Callback<string, string, string, Dictionary<string, object>, FileAttachment?>((_, _, _, data, _) =>
        {
          capturedTemplateData = data;
        })
        .Returns(Task.CompletedTask);

    // Act
    await _handler.Handle(command, CancellationToken.None);

    // Assert
    capturedTemplateData.Should().NotBeNull();
    capturedTemplateData.Should().ContainKey("verification_url");

    var verificationUrl = capturedTemplateData["verification_url"].ToString();
    verificationUrl.Should().StartWith("https://test.example.com/api/auth/confirm-email");
    verificationUrl.Should().Contain("userId=");
    verificationUrl.Should().Contain("token=");
  }

  [Theory]
  [InlineData("user1@test.com", "Pass123!@#")]
  [InlineData("test.user@domain.org", "ComplexP@ssw0rd!")]
  [InlineData("admin+tag@example.co.uk", "SecurePass123!")]
  [Trait("Category", "Unit")]
  public async Task Handle_VariousValidInputs_ReturnsSuccessResult(string email, string password)
  {
    // Arrange
    var command = new RegisterCommand(email, password);

    _userManagerMock.Setup(x => x.FindByEmailAsync(email))
        .ReturnsAsync((ApplicationUser)null!);

    _emailServiceMock.Setup(x => x.ValidateEmail(email))
        .ReturnsAsync(true);

    _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), password))
        .ReturnsAsync(IdentityResult.Success);

    _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
        .ReturnsAsync("token");

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeTrue("valid inputs should always succeed");
    result.Email.Should().Be(email);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_SuccessfulRegistration_LogsInformation()
  {
    // Arrange
    var command = new RegisterCommand("test@example.com", "ValidPassword123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync((ApplicationUser)null!);

    _emailServiceMock.Setup(x => x.ValidateEmail(command.Email))
        .ReturnsAsync(true);

    _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), command.Password))
        .ReturnsAsync(IdentityResult.Success);

    _userManagerMock.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<ApplicationUser>()))
        .ReturnsAsync("token");

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
        Times.AtLeastOnce,
        "successful registration should be logged");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task Handle_EmailValidationThrows_ReturnsFailureResult()
  {
    // Arrange
    var command = new RegisterCommand("test@example.com", "Password123!");

    _userManagerMock.Setup(x => x.FindByEmailAsync(command.Email))
        .ReturnsAsync((ApplicationUser)null!);

    _emailServiceMock.Setup(x => x.ValidateEmail(command.Email))
        .ThrowsAsync(new InvalidOperationException("Email validation service error"));

    // Act
    var result = await _handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.Success.Should().BeFalse("email validation exception should fail registration");
    result.Message.Should().Be("Failed to register user");
  }
}
