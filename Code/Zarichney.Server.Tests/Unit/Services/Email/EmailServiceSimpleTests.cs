using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Moq;
using Xunit;
using Zarichney.Services.Email;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Services.Email;

[Trait("Category", "Unit")]
public class EmailServiceSimpleTests
{
    private readonly Mock<GraphServiceClient> _mockGraphClient;
    private readonly Mock<ITemplateService> _mockTemplateService;
    private readonly Mock<IMailCheckClient> _mockMailCheckClient;
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly EmailConfig _emailConfig;

    public EmailServiceSimpleTests()
    {
        _mockGraphClient = new Mock<GraphServiceClient>();
        _mockTemplateService = new Mock<ITemplateService>();
        _mockMailCheckClient = new Mock<IMailCheckClient>();
        _mockLogger = new Mock<ILogger<EmailService>>();
        
        _emailConfig = new EmailConfig
        {
            AzureTenantId = "test-tenant-id",
            AzureAppId = "test-app-id",
            AzureAppSecret = "test-secret",
            FromEmail = "from@test.com",
            TemplateDirectory = "/test/templates",
            MailCheckApiKey = "test-api-key"
        };
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        // Act
        var sut = new EmailService(
            _mockGraphClient.Object,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Assert
        sut.Should().NotBeNull("EmailService should be created with valid parameters");
    }

    #endregion

    #region ValidateEmail Tests

    [Fact]
    public async Task ValidateEmail_ValidEmail_ReturnsTrue()
    {
        // Arrange
        var email = "valid@example.com";
        var domain = "example.com";
        var validResponse = new EmailValidationResponseBuilder()
            .AsValid()
            .WithDomain(domain)
            .Build();

        _mockMailCheckClient
            .Setup(x => x.GetValidationData(domain))
            .ReturnsAsync(validResponse);

        var sut = new EmailService(
            _mockGraphClient.Object,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Act
        var result = await sut.ValidateEmail(email);

        // Assert
        result.Should().BeTrue("valid email should return true");
        _mockMailCheckClient.Verify(
            x => x.GetValidationData(domain),
            Times.Once,
            "mail check client should be called with domain"
        );
    }

    [Fact]
    public async Task ValidateEmail_InvalidEmail_ThrowsInvalidEmailException()
    {
        // Arrange
        var email = "invalid@example.com";
        var domain = "example.com";
        var invalidResponse = new EmailValidationResponseBuilder()
            .AsInvalid("invalid syntax")
            .WithDomain(domain)
            .Build();

        _mockMailCheckClient
            .Setup(x => x.GetValidationData(domain))
            .ReturnsAsync(invalidResponse);

        var sut = new EmailService(
            _mockGraphClient.Object,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Act
        Func<Task> act = async () => await sut.ValidateEmail(email);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidEmailException>("invalid email should throw exception");
        exception.Which.Email.Should().Be(email, "exception should contain the email");
        exception.Which.Reason.Should().Be(InvalidEmailReason.InvalidSyntax, "exception should contain the correct reason");
    }

    [Fact]
    public async Task ValidateEmail_BlockedEmail_ThrowsInvalidEmailException()
    {
        // Arrange
        var email = "blocked@example.com";
        var domain = "example.com";
        var blockedResponse = new EmailValidationResponseBuilder()
            .AsBlocked()
            .WithDomain(domain)
            .Build();

        _mockMailCheckClient
            .Setup(x => x.GetValidationData(domain))
            .ReturnsAsync(blockedResponse);

        var sut = new EmailService(
            _mockGraphClient.Object,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Act
        Func<Task> act = async () => await sut.ValidateEmail(email);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidEmailException>("blocked email should throw exception");
        exception.Which.Email.Should().Be(email, "exception should contain the email");
        exception.Which.Reason.Should().Be(InvalidEmailReason.InvalidDomain, "blocked email should have invalid domain reason");
    }

    [Fact]
    public async Task ValidateEmail_DisposableEmail_ThrowsInvalidEmailException()
    {
        // Arrange
        var email = "test@disposable.com";
        var domain = "disposable.com";
        var disposableResponse = new EmailValidationResponseBuilder()
            .AsDisposable()
            .WithDomain(domain)
            .Build();

        _mockMailCheckClient
            .Setup(x => x.GetValidationData(domain))
            .ReturnsAsync(disposableResponse);

        var sut = new EmailService(
            _mockGraphClient.Object,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Act
        Func<Task> act = async () => await sut.ValidateEmail(email);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidEmailException>("disposable email should throw exception");
        exception.Which.Email.Should().Be(email, "exception should contain the email");
        exception.Which.Reason.Should().Be(InvalidEmailReason.DisposableEmail, "exception should have disposable email reason");
    }

    [Fact]
    public async Task ValidateEmail_HighRiskEmail_ThrowsInvalidEmailException()
    {
        // Arrange
        var email = "risky@example.com";
        var domain = "example.com";
        var highRiskResponse = new EmailValidationResponseBuilder()
            .AsHighRisk(80) // Above HighRiskThreshold of 70
            .WithDomain(domain)
            .Build();

        _mockMailCheckClient
            .Setup(x => x.GetValidationData(domain))
            .ReturnsAsync(highRiskResponse);

        var sut = new EmailService(
            _mockGraphClient.Object,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Act
        Func<Task> act = async () => await sut.ValidateEmail(email);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidEmailException>("high risk email should throw exception");
        exception.Which.Email.Should().Be(email, "exception should contain the email");
        exception.Which.Reason.Should().Be(InvalidEmailReason.InvalidDomain, "high risk email should have invalid domain reason");
        exception.Which.Message.Should().Contain("Risk score: 80", "exception message should contain risk score");
    }

    [Theory]
    [InlineData("invalid syntax", InvalidEmailReason.InvalidSyntax)]
    [InlineData("syntax error", InvalidEmailReason.InvalidSyntax)]
    [InlineData("possible typo", InvalidEmailReason.PossibleTypo)]
    [InlineData("domain issue", InvalidEmailReason.InvalidDomain)]
    [InlineData("unknown reason", InvalidEmailReason.InvalidDomain)]
    public async Task ValidateEmail_DifferentReasons_MapsToCorrectInvalidReason(string reason, InvalidEmailReason expectedReason)
    {
        // Arrange
        var email = "test@example.com";
        var domain = "example.com";
        var builder = new EmailValidationResponseBuilder()
            .AsInvalid(reason)
            .WithDomain(domain);

        if (expectedReason == InvalidEmailReason.PossibleTypo)
        {
            builder.WithPossibleTypos("correct@example.com");
        }

        var validationResponse = builder.Build();

        _mockMailCheckClient
            .Setup(x => x.GetValidationData(domain))
            .ReturnsAsync(validationResponse);

        var sut = new EmailService(
            _mockGraphClient.Object,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Act
        Func<Task> act = async () => await sut.ValidateEmail(email);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidEmailException>($"invalid email with reason '{reason}' should throw exception");
        exception.Which.Reason.Should().Be(expectedReason, $"reason '{reason}' should map to {expectedReason}");
    }

    #endregion

    #region MakeSafeFileName Tests

    [Theory]
    [InlineData("test@example.com", "test_at_example_dot_com")]
    [InlineData("user.name@domain.co.uk", "user_dot_name_at_domain_dot_co_dot_uk")]
    [InlineData("test user@example.com", "test_user_at_example_dot_com")]
    [InlineData("test:email@example.com", "test_email_at_example_dot_com")]
    [InlineData("test;email@example.com", "test_email_at_example_dot_com")]
    public void MakeSafeFileName_VariousEmailFormats_ReturnsExpectedSafeNames(string email, string expected)
    {
        // Act
        var result = EmailService.MakeSafeFileName(email);

        // Assert
        result.Should().Be(expected, $"email '{email}' should be safely converted to filename");
    }

    [Fact]
    public void MakeSafeFileName_EmptyString_ReturnsEmpty()
    {
        // Act
        var result = EmailService.MakeSafeFileName(string.Empty);

        // Assert
        result.Should().BeEmpty("empty string should return empty string");
    }

    #endregion

    #region SendEmail Input Validation Tests

    [Fact]
    public async Task SendEmail_NullGraphClient_ThrowsArgumentNullException()
    {
        // Arrange
        var sutWithNullClient = new EmailService(
            null!,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Act
        Func<Task> act = async () => await sutWithNullClient.SendEmail("test@example.com", "Subject", "template");

        // Assert
        await act.Should().ThrowAsync<ArgumentNullException>("null graph client should throw exception")
            .WithParameterName("graphClient");
    }

    [Fact]
    public async Task SendEmail_EmptyTemplateContent_ThrowsException()
    {
        // Arrange
        var recipient = "test@example.com";
        var subject = "Test Subject";
        var templateName = "empty-template";

        _mockTemplateService
            .Setup(x => x.ApplyTemplate(templateName, It.IsAny<Dictionary<string, object>>(), subject))
            .ReturnsAsync(string.Empty);

        var sut = new EmailService(
            _mockGraphClient.Object,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Act
        Func<Task> act = async () => await sut.SendEmail(recipient, subject, templateName);

        // Assert
        await act.Should().ThrowAsync<Exception>("empty template content should throw exception")
            .WithMessage("Email body content is empty");
    }

    [Fact]
    public async Task SendEmail_TemplateServiceThrows_LogsAndRethrows()
    {
        // Arrange
        var recipient = "test@example.com";
        var subject = "Test Subject";
        var templateName = "failing-template";
        var templateException = new InvalidOperationException("Template error");

        _mockTemplateService
            .Setup(x => x.ApplyTemplate(templateName, It.IsAny<Dictionary<string, object>>(), subject))
            .ThrowsAsync(templateException);

        var sut = new EmailService(
            _mockGraphClient.Object,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Act
        Func<Task> act = async () => await sut.SendEmail(recipient, subject, templateName);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>("template service exception should be rethrown")
            .WithMessage("Template error");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error applying email template")),
                It.Is<Exception>(ex => ex == templateException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "template error should be logged"
        );
    }

    #endregion
}