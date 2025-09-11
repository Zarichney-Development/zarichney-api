using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Moq;
using FluentAssertions;
using Xunit;
using Zarichney.Services.Email;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Services.Email;

/// <summary>
/// Unit tests focusing on EmailService email validation logic.
/// Tests the core business logic without complex Graph API dependencies.
/// </summary>
[Trait("Category", "Unit")]
public class EmailValidationTests
{
  #region ValidateEmail Tests

  [Fact]
  public async Task ValidateEmail_WithValidEmail_ReturnsTrue()
  {
    // Arrange
    var validResponse = new EmailValidationResponseBuilder()
        .WithValidDefaults()
        .Build();

    var mockMailCheckClient = CreateMockMailCheckClient(validResponse);
    var sut = CreateEmailService(mockMailCheckClient.Object);

    var email = "valid@example.com";

    // Act
    var result = await sut.ValidateEmail(email);

    // Assert
    result.Should().BeTrue("valid email should pass validation");
    mockMailCheckClient.Verify(x => x.GetValidationData("example.com"), Times.Once,
        "should extract domain from email for validation");
  }

  [Fact]
  public async Task ValidateEmail_WithInvalidEmail_ThrowsInvalidEmailException()
  {
    // Arrange
    var invalidResponse = new EmailValidationResponseBuilder()
        .WithInvalidEmail()
        .Build();

    var mockMailCheckClient = CreateMockMailCheckClient(invalidResponse);
    var sut = CreateEmailService(mockMailCheckClient.Object);

    var email = "invalid@example.com";

    // Act
    Func<Task> act = async () => await sut.ValidateEmail(email);

    // Assert
    var exception = await act.Should().ThrowAsync<InvalidEmailException>();
    exception.Which.Email.Should().Be(email, "exception should include the invalid email");
    exception.Which.Reason.Should().Be(InvalidEmailReason.InvalidSyntax, "should determine syntax error reason correctly");
  }

  [Fact]
  public async Task ValidateEmail_WithBlockedEmail_ThrowsInvalidEmailException()
  {
    // Arrange
    var blockedResponse = new EmailValidationResponseBuilder()
        .WithBlockedEmail()
        .Build();

    var mockMailCheckClient = CreateMockMailCheckClient(blockedResponse);
    var sut = CreateEmailService(mockMailCheckClient.Object);

    var email = "blocked@example.com";

    // Act
    Func<Task> act = async () => await sut.ValidateEmail(email);

    // Assert
    var exception = await act.Should().ThrowAsync<InvalidEmailException>()
        .WithMessage("Blocked email detected");
    exception.Which.Reason.Should().Be(InvalidEmailReason.InvalidDomain, "blocked emails use InvalidDomain reason");
  }

  [Fact]
  public async Task ValidateEmail_WithDisposableEmail_ThrowsInvalidEmailException()
  {
    // Arrange
    var disposableResponse = new EmailValidationResponseBuilder()
        .WithDisposableEmail()
        .Build();

    var mockMailCheckClient = CreateMockMailCheckClient(disposableResponse);
    var sut = CreateEmailService(mockMailCheckClient.Object);

    var email = "user@tempmail.org";

    // Act
    Func<Task> act = async () => await sut.ValidateEmail(email);

    // Assert
    var exception = await act.Should().ThrowAsync<InvalidEmailException>()
        .WithMessage("Disposable email detected");
    exception.Which.Reason.Should().Be(InvalidEmailReason.DisposableEmail, "disposable emails have specific reason");
  }

  [Fact]
  public async Task ValidateEmail_WithHighRiskEmail_ThrowsInvalidEmailException()
  {
    // Arrange
    var highRiskResponse = new EmailValidationResponseBuilder()
        .WithHighRiskEmail(80)
        .Build();

    var mockMailCheckClient = CreateMockMailCheckClient(highRiskResponse);
    var sut = CreateEmailService(mockMailCheckClient.Object);

    var email = "risky@example.com";

    // Act
    Func<Task> act = async () => await sut.ValidateEmail(email);

    // Assert
    var exception = await act.Should().ThrowAsync<InvalidEmailException>()
        .WithMessage("High risk email detected. Risk score: 80");
    exception.Which.Reason.Should().Be(InvalidEmailReason.InvalidDomain, "high risk emails use InvalidDomain reason");
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
    var sut = CreateEmailService(mockMailCheckClient.Object);

    var email = "acceptable@example.com";

    // Act
    var result = await sut.ValidateEmail(email);

    // Assert
    result.Should().BeTrue($"risk score {riskScore} should be acceptable (below 70 threshold)");
  }

  [Fact]
  public async Task ValidateEmail_WithPossibleTypo_ThrowsInvalidEmailException()
  {
    // Arrange
    var typoResponse = new EmailValidationResponseBuilder()
        .WithPossibleTypo("gmail.com", "yahoo.com")
        .Build();

    var mockMailCheckClient = CreateMockMailCheckClient(typoResponse);
    var sut = CreateEmailService(mockMailCheckClient.Object);

    var email = "user@gmial.com";

    // Act
    Func<Task> act = async () => await sut.ValidateEmail(email);

    // Assert
    var exception = await act.Should().ThrowAsync<InvalidEmailException>();
    exception.Which.Reason.Should().Be(InvalidEmailReason.PossibleTypo, "should identify typo reason correctly");
  }

  [Fact]
  public async Task ValidateEmail_WithComplexEmailDomain_ExtractsDomainCorrectly()
  {
    // Arrange
    var validResponse = new EmailValidationResponseBuilder()
        .WithValidDefaults()
        .WithDomain("complex.subdomain.example.co.uk")
        .Build();

    var mockMailCheckClient = CreateMockMailCheckClient(validResponse);
    var sut = CreateEmailService(mockMailCheckClient.Object);

    var email = "user@complex.subdomain.example.co.uk";

    // Act
    var result = await sut.ValidateEmail(email);

    // Assert
    result.Should().BeTrue("valid complex domain should pass validation");
    mockMailCheckClient.Verify(x => x.GetValidationData("complex.subdomain.example.co.uk"), Times.Once,
        "should extract the complete domain correctly");
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
    result.Should().Be(expected, "special characters should be replaced with underscores for safe file names");
  }

  [Fact]
  public void MakeSafeFileName_WithEmptyString_ReturnsEmptyString()
  {
    // Act
    var result = EmailService.MakeSafeFileName(string.Empty);

    // Assert
    result.Should().BeEmpty("empty input should return empty result");
  }

  #endregion

  #region Helper Methods

  private EmailService CreateEmailService(IMailCheckClient mailCheckClient)
  {
    // Create a mock GraphServiceClient without using Moq (since it can't be mocked easily)
    // For unit testing email validation, we only need the EmailService to be instantiable
    var mockGraphClient = MockGraphServiceClient.Create();
    var mockLogger = new Mock<ILogger<EmailService>>();
    var mockTemplateService = new Mock<ITemplateService>();

    var config = new EmailConfig
    {
      AzureTenantId = "test-tenant-id",
      AzureAppId = "test-app-id",
      AzureAppSecret = "test-app-secret",
      FromEmail = "test@zarichney.com",
      TemplateDirectory = "/test/templates",
      MailCheckApiKey = "test-mailcheck-api-key"
    };

    return new EmailService(
        mockGraphClient,
        config,
        mockTemplateService.Object,
        mailCheckClient,
        mockLogger.Object);
  }

  private Mock<IMailCheckClient> CreateMockMailCheckClient(EmailValidationResponse? response = null)
  {
    var mockMailCheckClient = new Mock<IMailCheckClient>();
    var validationResponse = response ?? new EmailValidationResponseBuilder().WithValidDefaults().Build();

    mockMailCheckClient.Setup(x => x.GetValidationData(It.IsAny<string>()))
        .ReturnsAsync(validationResponse);

    return mockMailCheckClient;
  }

  #endregion
}

/// <summary>
/// Simple mock wrapper for GraphServiceClient to avoid Moq issues.
/// </summary>
public static class MockGraphServiceClient
{
  public static GraphServiceClient Create()
  {
    // Since GraphServiceClient is difficult to mock, we create a simple instance
    // that won't be used in these validation-focused tests
    // Integration tests will handle the full Graph API interaction testing
    return new GraphServiceClient(new System.Net.Http.HttpClient());
  }
}
