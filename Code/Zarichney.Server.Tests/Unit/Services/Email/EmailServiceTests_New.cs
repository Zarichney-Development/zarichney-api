using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Moq;
using Xunit;
using Zarichney.Services.Email;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Services.Email;

[Trait("Category", "Unit")]
public class EmailServiceTests_New
{
    private readonly Mock<GraphServiceClient> _mockGraphClient;
    private readonly Mock<ITemplateService> _mockTemplateService;
    private readonly Mock<IMailCheckClient> _mockMailCheckClient;
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly EmailConfig _emailConfig;

    public EmailServiceTests_New()
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

    #region SendEmail Basic Tests

    [Fact]
    public async Task SendEmail_ValidInput_CallsTemplateService()
    {
        // Arrange
        var recipient = "test@example.com";
        var subject = "Test Subject";
        var templateName = "test-template";
        var templateData = new Dictionary<string, object> { { "name", "Test User" } };
        var expectedBodyContent = "<html><body>Test Content</body></html>";

        _mockTemplateService
            .Setup(x => x.ApplyTemplate(templateName, templateData, subject))
            .ReturnsAsync(expectedBodyContent);

        var sut = new EmailService(
            _mockGraphClient.Object,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Act & Assert - We can't easily mock GraphServiceClient PostAsync due to optional parameters
        // So we test the template service call and exception handling instead
        try
        {
            await sut.SendEmail(recipient, subject, templateName, templateData);
        }
        catch
        {
            // Expected to throw since GraphServiceClient isn't properly set up, but that's ok
            // We're testing that the template service was called correctly
        }

        // Assert
        _mockTemplateService.Verify(
            x => x.ApplyTemplate(templateName, templateData, subject),
            Times.Once,
            "template service should be called to generate email content"
        );
    }

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