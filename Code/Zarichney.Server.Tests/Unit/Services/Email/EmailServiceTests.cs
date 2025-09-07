using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Moq;
using FluentAssertions;
using Xunit;
using Zarichney.Services.Email;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Services.Email;

/// <summary>
/// Unit tests for EmailService covering email validation and error notification functionality.
/// Focuses on testable business logic that doesn't require complex Graph API mocking.
/// </summary>
[Trait("Category", "Unit")]
public class EmailServiceTests
{
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly EmailConfig _config;
    private readonly Mock<GraphServiceClient> _mockGraphClient;
    private readonly Mock<ITemplateService> _mockTemplateService;

    public EmailServiceTests()
    {
        _mockLogger = new Mock<ILogger<EmailService>>();
        _config = new EmailConfig
        {
            AzureTenantId = "test-tenant-id",
            AzureAppId = "test-app-id", 
            AzureAppSecret = "test-app-secret",
            FromEmail = "test@zarichney.com",
            TemplateDirectory = "/test/templates",
            MailCheckApiKey = "test-mailcheck-api-key"
        };
        _mockGraphClient = new Mock<GraphServiceClient>();
        _mockTemplateService = new Mock<ITemplateService>();
        
        // Default successful template application
        _mockTemplateService.Setup(x => x.ApplyTemplate(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()))
            .ReturnsAsync((string templateName, Dictionary<string, object> templateData, string title) => 
                $"<html><body><h1>{title}</h1><p>Template: {templateName}</p></body></html>");
    }

    #region SendEmail Tests - Constructor and Input Validation

    [Fact]
    public void Constructor_WithNullGraphClient_ThrowsArgumentNullException()
    {
        // Arrange
        var mockMailCheckClient = CreateMockMailCheckClient();

        // Act
        Action act = () => new EmailService(
            null!,
            _config,
            _mockTemplateService.Object,
            mockMailCheckClient.Object,
            _mockLogger.Object);

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("graphClient");
    }

    [Fact]
    public async Task SendEmail_WithEmptyTemplateContent_ThrowsException()
    {
        // Arrange
        var mockMailCheckClient = CreateMockMailCheckClient();
        
        // Setup template service to return empty content
        _mockTemplateService.Setup(x => x.ApplyTemplate(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()))
            .ReturnsAsync(string.Empty);
        
        var sut = new EmailService(
            _mockGraphClient.Object,
            _config,
            _mockTemplateService.Object,
            mockMailCheckClient.Object,
            _mockLogger.Object);

        var recipient = "test@example.com";
        var subject = "Test Subject";
        var templateName = "empty";

        // Act
        Func<Task> act = async () => await sut.SendEmail(recipient, subject, templateName);

        // Assert
        await act.Should().ThrowAsync<Exception>()
            .WithMessage("Email body content is empty");
    }

    [Fact]
    public async Task SendEmail_WithTemplateProcessingError_LogsErrorAndRethrows()
    {
        // Arrange
        var mockMailCheckClient = CreateMockMailCheckClient();
        _mockTemplateService.Setup(x => x.ApplyTemplate(
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>(),
                It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Template processing failed"));
        
        var sut = new EmailService(
            _mockGraphClient.Object,
            _config,
            _mockTemplateService.Object,
            mockMailCheckClient.Object,
            _mockLogger.Object);

        var recipient = "test@example.com";
        var subject = "Test Subject";
        var templateName = "failing-template";

        // Act
        Func<Task> act = async () => await sut.SendEmail(recipient, subject, templateName);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Verify error logging occurred  
        VerifyLoggerCalled(LogLevel.Error, "Error applying email template");
    }

    #endregion

    #region ValidateEmail Tests

    [Fact]
    public async Task ValidateEmail_WithValidEmail_ReturnsTrue()
    {
        // Arrange
        var validResponse = new EmailValidationResponseBuilder()
            .WithValidDefaults()
            .Build();
        
        var mockMailCheckClient = CreateMockMailCheckClient(validResponse);
        var sut = new EmailService(
            _mockGraphClient.Object,
            _config,
            _mockTemplateService.Object,
            mockMailCheckClient.Object,
            _mockLogger.Object);

        var email = "valid@example.com";

        // Act
        var result = await sut.ValidateEmail(email);

        // Assert
        result.Should().BeTrue();
        mockMailCheckClient.Verify(x => x.GetValidationData("example.com"), Times.Once);
    }

    [Fact]
    public async Task ValidateEmail_WithInvalidEmail_ThrowsInvalidEmailException()
    {
        // Arrange
        var invalidResponse = new EmailValidationResponseBuilder()
            .WithInvalidEmail()
            .Build();
        
        var mockMailCheckClient = CreateMockMailCheckClient(invalidResponse);
        var sut = new EmailService(
            _mockGraphClient.Object,
            _config,
            _mockTemplateService.Object,
            mockMailCheckClient.Object,
            _mockLogger.Object);

        var email = "invalid@example.com";

        // Act
        Func<Task> act = async () => await sut.ValidateEmail(email);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidEmailException>();
        exception.Which.Email.Should().Be(email);
        exception.Which.Reason.Should().Be(InvalidEmailReason.InvalidDomain);
    }

    [Fact]
    public async Task ValidateEmail_WithBlockedEmail_ThrowsInvalidEmailException()
    {
        // Arrange
        var blockedResponse = new EmailValidationResponseBuilder()
            .WithBlockedEmail()
            .Build();
        
        var mockMailCheckClient = CreateMockMailCheckClient(blockedResponse);
        var sut = new EmailService(
            _mockGraphClient.Object,
            _config,
            _mockTemplateService.Object,
            mockMailCheckClient.Object,
            _mockLogger.Object);

        var email = "blocked@example.com";

        // Act
        Func<Task> act = async () => await sut.ValidateEmail(email);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidEmailException>()
            .WithMessage("Blocked email detected");
        exception.Which.Reason.Should().Be(InvalidEmailReason.InvalidDomain);
    }

    [Fact]
    public async Task ValidateEmail_WithDisposableEmail_ThrowsInvalidEmailException()
    {
        // Arrange
        var disposableResponse = new EmailValidationResponseBuilder()
            .WithDisposableEmail()
            .Build();
        
        var mockMailCheckClient = CreateMockMailCheckClient(disposableResponse);
        var sut = new EmailService(
            _mockGraphClient.Object,
            _config,
            _mockTemplateService.Object,
            mockMailCheckClient.Object,
            _mockLogger.Object);

        var email = "user@tempmail.org";

        // Act
        Func<Task> act = async () => await sut.ValidateEmail(email);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidEmailException>()
            .WithMessage("Disposable email detected");
        exception.Which.Reason.Should().Be(InvalidEmailReason.DisposableEmail);
    }

    [Fact]
    public async Task ValidateEmail_WithHighRiskEmail_ThrowsInvalidEmailException()
    {
        // Arrange
        var highRiskResponse = new EmailValidationResponseBuilder()
            .WithHighRiskEmail(80)
            .Build();
        
        var mockMailCheckClient = CreateMockMailCheckClient(highRiskResponse);
        var sut = new EmailService(
            _mockGraphClient.Object,
            _config,
            _mockTemplateService.Object,
            mockMailCheckClient.Object,
            _mockLogger.Object);

        var email = "risky@example.com";

        // Act
        Func<Task> act = async () => await sut.ValidateEmail(email);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidEmailException>()
            .WithMessage("High risk email detected. Risk score: 80");
        exception.Which.Reason.Should().Be(InvalidEmailReason.InvalidDomain);
    }

    [Theory]
    [InlineData(69)] // Just below threshold
    [InlineData(50)] // Moderate risk
    [InlineData(10)] // Low risk
    public async Task ValidateEmail_WithAcceptableRiskLevel_ReturnsTrue(int riskScore)
    {
        // Arrange
        var acceptableRiskResponse = new EmailValidationResponseBuilder()
            .WithValidDefaults()
            .WithRisk(riskScore)
            .Build();
        
        var mockMailCheckClient = CreateMockMailCheckClient(acceptableRiskResponse);
        var sut = new EmailService(
            _mockGraphClient.Object,
            _config,
            _mockTemplateService.Object,
            mockMailCheckClient.Object,
            _mockLogger.Object);

        var email = "acceptable@example.com";

        // Act
        var result = await sut.ValidateEmail(email);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task ValidateEmail_WithPossibleTypo_ThrowsInvalidEmailException()
    {
        // Arrange
        var typoResponse = new EmailValidationResponseBuilder()
            .WithPossibleTypo("gmail.com", "yahoo.com")
            .Build();
        
        var mockMailCheckClient = CreateMockMailCheckClient(typoResponse);
        var sut = new EmailService(
            _mockGraphClient.Object,
            _config,
            _mockTemplateService.Object,
            mockMailCheckClient.Object,
            _mockLogger.Object);

        var email = "user@gmial.com";

        // Act
        Func<Task> act = async () => await sut.ValidateEmail(email);

        // Assert
        var exception = await act.Should().ThrowAsync<InvalidEmailException>();
        exception.Which.Reason.Should().Be(InvalidEmailReason.PossibleTypo);
    }

    #endregion

    #region SendErrorNotification Tests

    [Fact]
    public async Task SendErrorNotification_WithBasicException_CallsTemplateService()
    {
        // Arrange
        var mockMailCheckClient = CreateMockMailCheckClient();
        var sut = new EmailService(
            _mockGraphClient.Object,
            _config,
            _mockTemplateService.Object,
            mockMailCheckClient.Object,
            _mockLogger.Object);

        var stage = "data-processing";
        var serviceName = "OrderService";
        var exception = new InvalidOperationException("Test error message");

        // Act
        await sut.SendErrorNotification(stage, exception, serviceName);

        // Assert
        _mockTemplateService.Verify(x => x.ApplyTemplate(
            "error-log",
            It.Is<Dictionary<string, object>>(data => 
                data.ContainsKey("stage") && data["stage"].Equals(stage) &&
                data.ContainsKey("serviceName") && data["serviceName"].Equals(serviceName) &&
                data.ContainsKey("errorType") && data["errorType"].Equals("InvalidOperationException")),
            $"{serviceName} Error - {stage}"), Times.Once);
    }

    [Fact]
    public async Task SendErrorNotification_WithAdditionalContext_IncludesContextInTemplate()
    {
        // Arrange
        var mockMailCheckClient = CreateMockMailCheckClient();
        var sut = new EmailService(
            _mockGraphClient.Object,
            _config,
            _mockTemplateService.Object,
            mockMailCheckClient.Object,
            _mockLogger.Object);

        var stage = "payment-processing";
        var serviceName = "PaymentService";
        var exception = new ArgumentException("Invalid payment amount");
        var additionalContext = new Dictionary<string, string>
        {
            { "orderId", "ORDER-123" },
            { "customerId", "CUST-456" },
            { "amount", "$99.99" }
        };

        // Act
        await sut.SendErrorNotification(stage, exception, serviceName, additionalContext);

        // Assert
        _mockTemplateService.Verify(x => x.ApplyTemplate(
            "error-log",
            It.Is<Dictionary<string, object>>(data => 
                data.ContainsKey("additionalContext") &&
                data["additionalContext"] != null &&
                ((Dictionary<string, string>)data["additionalContext"]).ContainsKey("orderId")),
            $"{serviceName} Error - {stage}"), Times.Once);
    }

    #endregion

    #region MakeSafeFileName Tests

    [Theory]
    [InlineData("test@example.com", "test_at_example_dot_com")]
    [InlineData("user.name@domain.co.uk", "user_dot_name_at_domain_dot_co_dot_uk")]
    [InlineData("email with spaces@test.com", "email_with_spaces_at_test_dot_com")]
    [InlineData("complex:email;with@special.chars", "complex_email_with_at_special_dot_chars")]
    public void MakeSafeFileName_WithVariousEmailFormats_ReplacesSpecialCharacters(string input, string expected)
    {
        // Act
        var result = EmailService.MakeSafeFileName(input);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void MakeSafeFileName_WithEmptyString_ReturnsEmptyString()
    {
        // Act
        var result = EmailService.MakeSafeFileName(string.Empty);

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region Edge Cases and Integration Tests

    [Fact]
    public async Task ValidateEmail_WithComplexEmailDomain_ExtractsDomainCorrectly()
    {
        // Arrange
        var validResponse = new EmailValidationResponseBuilder()
            .WithValidDefaults()
            .WithDomain("complex.subdomain.example.co.uk")
            .Build();
        
        var mockMailCheckClient = CreateMockMailCheckClient(validResponse);
        var sut = new EmailService(
            _mockGraphClient.Object,
            _config,
            _mockTemplateService.Object,
            mockMailCheckClient.Object,
            _mockLogger.Object);

        var email = "user@complex.subdomain.example.co.uk";

        // Act
        var result = await sut.ValidateEmail(email);

        // Assert
        result.Should().BeTrue();
        mockMailCheckClient.Verify(x => x.GetValidationData("complex.subdomain.example.co.uk"), Times.Once);
    }

    #endregion

    #region Helper Methods

    private Mock<IMailCheckClient> CreateMockMailCheckClient(EmailValidationResponse? response = null)
    {
        var mockMailCheckClient = new Mock<IMailCheckClient>();
        var validationResponse = response ?? new EmailValidationResponseBuilder().WithValidDefaults().Build();
        
        mockMailCheckClient.Setup(x => x.GetValidationData(It.IsAny<string>()))
            .ReturnsAsync(validationResponse);

        return mockMailCheckClient;
    }

    private void VerifyLoggerCalled(LogLevel level, string containsMessage)
    {
        _mockLogger.Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(containsMessage)),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    #endregion
}