using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Moq;
using FluentAssertions;
using Xunit;
using Zarichney.Services.Email;
using Zarichney.Tests.TestData.Builders;
using Zarichney.Tests.TestData.AutoFixtureCustomizations;
using AutoFixture.Xunit2;

namespace Zarichney.Tests.Unit.Services.Email;

/// <summary>
/// Unit tests for EmailService covering email validation and error notification functionality.
/// Uses AutoMoqData for proper dependency injection and mocking of complex Graph API dependencies.
/// </summary>
[Trait("Category", "Unit")]
public class EmailServiceTests
{
  #region SendEmail Tests - Constructor and Input Validation

  [Theory, AutoMoqData]
  public void Constructor_WithNullGraphClient_ThrowsArgumentNullException(
      EmailConfig config,
      ITemplateService templateService,
      IMailCheckClient mailCheckClient,
      ILogger<EmailService> logger)
  {
    // Act
    Action act = () => new EmailService(
        null!,
        config,
        templateService,
        mailCheckClient,
        logger);

    // Assert
    act.Should().Throw<ArgumentNullException>()
        .WithParameterName("graphClient");
  }

  [Theory, AutoMoqData]
  public async Task SendEmail_WithEmptyTemplateContent_ThrowsException(
      [Frozen] Mock<ITemplateService> mockTemplateService,
      EmailService sut)
  {
    // Arrange
    mockTemplateService.Setup(x => x.ApplyTemplate(
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<string>()))
        .ReturnsAsync(string.Empty);

    var recipient = "test@example.com";
    var subject = "Test Subject";
    var templateName = "empty";

    // Act
    Func<Task> act = async () => await sut.SendEmail(recipient, subject, templateName);

    // Assert
    await act.Should().ThrowAsync<Exception>()
        .WithMessage("Email body content is empty");
  }

  [Theory, AutoMoqData]
  public async Task SendEmail_WithTemplateProcessingError_LogsErrorAndRethrows(
      [Frozen] Mock<ITemplateService> mockTemplateService,
      [Frozen] Mock<ILogger<EmailService>> mockLogger,
      EmailService sut)
  {
    // Arrange
    mockTemplateService.Setup(x => x.ApplyTemplate(
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<string>()))
        .ThrowsAsync(new InvalidOperationException("Template processing failed"));

    var recipient = "test@example.com";
    var subject = "Test Subject";
    var templateName = "failing-template";

    // Act
    Func<Task> act = async () => await sut.SendEmail(recipient, subject, templateName);

    // Assert
    await act.Should().ThrowAsync<InvalidOperationException>();

    // Verify error logging occurred
    VerifyLoggerCalled(mockLogger, LogLevel.Error, "Error applying email template");
  }

  #endregion

  #region ValidateEmail Tests

  [Theory, AutoMoqData]
  public async Task ValidateEmail_WithValidEmail_ReturnsTrue(
      [Frozen] Mock<IMailCheckClient> mockMailCheckClient,
      EmailService sut)
  {
    // Arrange
    var validResponse = new EmailValidationResponseBuilder()
        .WithValidDefaults()
        .Build();

    mockMailCheckClient.Setup(x => x.GetValidationData(It.IsAny<string>()))
        .ReturnsAsync(validResponse);

    var email = "valid@example.com";

    // Act
    var result = await sut.ValidateEmail(email);

    // Assert
    result.Should().BeTrue();
    mockMailCheckClient.Verify(x => x.GetValidationData("example.com"), Times.Once);
  }

  [Theory, AutoMoqData]
  public async Task ValidateEmail_WithInvalidEmail_ThrowsInvalidEmailException(
      [Frozen] Mock<IMailCheckClient> mockMailCheckClient,
      EmailService sut)
  {
    // Arrange
    var invalidResponse = new EmailValidationResponseBuilder()
        .WithInvalidEmail()
        .Build();

    mockMailCheckClient.Setup(x => x.GetValidationData(It.IsAny<string>()))
        .ReturnsAsync(invalidResponse);

    var email = "invalid@example.com";

    // Act
    Func<Task> act = async () => await sut.ValidateEmail(email);

    // Assert
    var exception = await act.Should().ThrowAsync<InvalidEmailException>();
    exception.Which.Email.Should().Be(email);
    exception.Which.Reason.Should().Be(InvalidEmailReason.InvalidSyntax);
  }

  [Theory, AutoMoqData]
  public async Task ValidateEmail_WithBlockedEmail_ThrowsInvalidEmailException(
      [Frozen] Mock<IMailCheckClient> mockMailCheckClient,
      EmailService sut)
  {
    // Arrange
    var blockedResponse = new EmailValidationResponseBuilder()
        .WithBlockedEmail()
        .Build();

    mockMailCheckClient.Setup(x => x.GetValidationData(It.IsAny<string>()))
        .ReturnsAsync(blockedResponse);

    var email = "blocked@example.com";

    // Act
    Func<Task> act = async () => await sut.ValidateEmail(email);

    // Assert
    var exception = await act.Should().ThrowAsync<InvalidEmailException>()
        .WithMessage("Blocked email detected");
    exception.Which.Reason.Should().Be(InvalidEmailReason.InvalidDomain);
  }

  [Theory, AutoMoqData]
  public async Task ValidateEmail_WithDisposableEmail_ThrowsInvalidEmailException(
      [Frozen] Mock<IMailCheckClient> mockMailCheckClient,
      EmailService sut)
  {
    // Arrange
    var disposableResponse = new EmailValidationResponseBuilder()
        .WithDisposableEmail()
        .Build();

    mockMailCheckClient.Setup(x => x.GetValidationData(It.IsAny<string>()))
        .ReturnsAsync(disposableResponse);

    var email = "user@tempmail.org";

    // Act
    Func<Task> act = async () => await sut.ValidateEmail(email);

    // Assert
    var exception = await act.Should().ThrowAsync<InvalidEmailException>()
        .WithMessage("Disposable email detected");
    exception.Which.Reason.Should().Be(InvalidEmailReason.DisposableEmail);
  }

  [Theory, AutoMoqData]
  public async Task ValidateEmail_WithHighRiskEmail_ThrowsInvalidEmailException(
      [Frozen] Mock<IMailCheckClient> mockMailCheckClient,
      EmailService sut)
  {
    // Arrange
    var highRiskResponse = new EmailValidationResponseBuilder()
        .WithHighRiskEmail(80)
        .Build();

    mockMailCheckClient.Setup(x => x.GetValidationData(It.IsAny<string>()))
        .ReturnsAsync(highRiskResponse);

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

    var mockGraphClient = new GraphServiceClient(new HttpClient());
    var emailConfig = new EmailConfig
    {
      AzureTenantId = "test-tenant-id",
      AzureAppId = "test-app-id",
      AzureAppSecret = "test-app-secret",
      FromEmail = "test@zarichney.com",
      TemplateDirectory = "/test/templates",
      MailCheckApiKey = "test-mailcheck-api-key"
    };
    var mockTemplateService = new Mock<ITemplateService>();
    var mockMailCheckClient = new Mock<IMailCheckClient>();
    var mockLogger = new Mock<ILogger<EmailService>>();

    mockMailCheckClient.Setup(x => x.GetValidationData(It.IsAny<string>()))
        .ReturnsAsync(acceptableRiskResponse);

    var sut = new EmailService(
        mockGraphClient,
        emailConfig,
        mockTemplateService.Object,
        mockMailCheckClient.Object,
        mockLogger.Object);

    var email = "acceptable@example.com";

    // Act
    var result = await sut.ValidateEmail(email);

    // Assert
    result.Should().BeTrue();
  }

  [Theory, AutoMoqData]
  public async Task ValidateEmail_WithPossibleTypo_ThrowsInvalidEmailException(
      [Frozen] Mock<IMailCheckClient> mockMailCheckClient,
      EmailService sut)
  {
    // Arrange
    var typoResponse = new EmailValidationResponseBuilder()
        .WithPossibleTypo("gmail.com", "yahoo.com")
        .Build();

    mockMailCheckClient.Setup(x => x.GetValidationData(It.IsAny<string>()))
        .ReturnsAsync(typoResponse);

    var email = "user@gmial.com";

    // Act
    Func<Task> act = async () => await sut.ValidateEmail(email);

    // Assert
    var exception = await act.Should().ThrowAsync<InvalidEmailException>();
    exception.Which.Reason.Should().Be(InvalidEmailReason.PossibleTypo);
  }

  [Theory, AutoMoqData]
  public async Task ValidateEmail_WithComplexEmailDomain_ExtractsDomainCorrectly(
      [Frozen] Mock<IMailCheckClient> mockMailCheckClient,
      EmailService sut)
  {
    // Arrange
    var validResponse = new EmailValidationResponseBuilder()
        .WithValidDefaults()
        .WithDomain("complex.subdomain.example.co.uk")
        .Build();

    mockMailCheckClient.Setup(x => x.GetValidationData(It.IsAny<string>()))
        .ReturnsAsync(validResponse);

    var email = "user@complex.subdomain.example.co.uk";

    // Act
    var result = await sut.ValidateEmail(email);

    // Assert
    result.Should().BeTrue();
    mockMailCheckClient.Verify(x => x.GetValidationData("complex.subdomain.example.co.uk"), Times.Once);
  }

  #endregion

  #region SendErrorNotification Tests

  [Theory, AutoMoqData]
  public async Task SendErrorNotification_WithBasicException_CallsTemplateService(
      [Frozen] Mock<ITemplateService> mockTemplateService,
      EmailService sut)
  {
    // Arrange
    var stage = "data-processing";
    var serviceName = "OrderService";
    var exception = new InvalidOperationException("Test error message");

    // Act
    await sut.SendErrorNotification(stage, exception, serviceName);

    // Assert
    mockTemplateService.Verify(x => x.ApplyTemplate(
        "error-log",
        It.Is<Dictionary<string, object>>(data =>
            data.ContainsKey("stage") && data["stage"].Equals(stage) &&
            data.ContainsKey("serviceName") && data["serviceName"].Equals(serviceName) &&
            data.ContainsKey("errorType") && data["errorType"].Equals("InvalidOperationException")),
        $"{serviceName} Error - {stage}"), Times.Once);
  }

  [Theory, AutoMoqData]
  public async Task SendErrorNotification_WithAdditionalContext_IncludesContextInTemplate(
      [Frozen] Mock<ITemplateService> mockTemplateService,
      EmailService sut)
  {
    // Arrange
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
    mockTemplateService.Verify(x => x.ApplyTemplate(
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

  #region Helper Methods

  private static void VerifyLoggerCalled(Mock<ILogger<EmailService>> mockLogger, LogLevel level, string containsMessage)
  {
    mockLogger.Verify(
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
