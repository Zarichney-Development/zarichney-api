using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Moq;
using Xunit;
using Zarichney.Services.Email;

namespace Zarichney.Tests.Unit.Services.Email;

[Trait("Category", "Unit")]
public class EmailServiceErrorNotificationTests
{
    private readonly Mock<GraphServiceClient> _mockGraphClient;
    private readonly Mock<ITemplateService> _mockTemplateService;
    private readonly Mock<IMailCheckClient> _mockMailCheckClient;
    private readonly Mock<ILogger<EmailService>> _mockLogger;
    private readonly EmailConfig _emailConfig;

    public EmailServiceErrorNotificationTests()
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

    [Fact]
    public async Task SendErrorNotification_ValidInput_CallsTemplateServiceWithCorrectData()
    {
        // Arrange
        var stage = "TestStage";
        var serviceName = "TestService";
        var exception = new InvalidOperationException("Test exception");
        var additionalContext = new Dictionary<string, string> { { "RequestId", "123456" } };
        var expectedSubject = $"{serviceName} Error - {stage}";
        var expectedBodyContent = "<html><body>Error Content</body></html>";

        _mockTemplateService
            .Setup(x => x.ApplyTemplate("error-log", It.IsAny<Dictionary<string, object>>(), expectedSubject))
            .ReturnsAsync(expectedBodyContent);

        var sut = new EmailService(
            _mockGraphClient.Object,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Act
        await sut.SendErrorNotification(stage, exception, serviceName, additionalContext);

        // Assert
        _mockTemplateService.Verify(
            x => x.ApplyTemplate(
                "error-log",
                It.Is<Dictionary<string, object>>(data =>
                    data.ContainsKey("stage") &&
                    data["stage"].Equals(stage) &&
                    data.ContainsKey("serviceName") &&
                    data["serviceName"].Equals(serviceName) &&
                    data.ContainsKey("additionalContext")),
                expectedSubject),
            Times.Once,
            "error template should be applied with correct data"
        );
    }

    [Fact]
    public async Task SendErrorNotification_EmailFails_LogsErrorAndSwallowsException()
    {
        // Arrange
        var stage = "TestStage";
        var serviceName = "TestService";
        var exception = new InvalidOperationException("Test exception");
        var emailException = new InvalidOperationException("Email sending failed");

        _mockTemplateService
            .Setup(x => x.ApplyTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
            .ThrowsAsync(emailException);

        var sut = new EmailService(
            _mockGraphClient.Object,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Act
        Func<Task> act = async () => await sut.SendErrorNotification(stage, exception, serviceName);

        // Assert
        await act.Should().NotThrowAsync("error notification should not throw exceptions to prevent cascading failures");

        _mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to send error notification email")),
                It.Is<Exception>(ex => ex == emailException),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once,
            "email failure should be logged"
        );
    }

    [Fact]
    public async Task SendErrorNotification_NoAdditionalContext_CallsTemplateServiceWithoutContext()
    {
        // Arrange
        var stage = "TestStage";
        var serviceName = "TestService";
        var exception = new InvalidOperationException("Test exception");
        var expectedBodyContent = "<html><body>Error Content</body></html>";

        _mockTemplateService
            .Setup(x => x.ApplyTemplate("error-log", It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
            .ReturnsAsync(expectedBodyContent);

        var sut = new EmailService(
            _mockGraphClient.Object,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Act
        await sut.SendErrorNotification(stage, exception, serviceName);

        // Assert
        _mockTemplateService.Verify(
            x => x.ApplyTemplate(
                "error-log",
                It.Is<Dictionary<string, object>>(data =>
                    data.ContainsKey("stage") &&
                    data.ContainsKey("serviceName")),
                It.IsAny<string>()),
            Times.Once,
            "error template should be applied even without additional context"
        );
    }

    [Fact]
    public async Task SendErrorNotification_WithAdditionalContext_MergesContextCorrectly()
    {
        // Arrange
        var stage = "TestStage";
        var serviceName = "TestService";
        var exception = new InvalidOperationException("Test exception");
        var additionalContext = new Dictionary<string, string>
        {
            { "RequestId", "REQ-12345" },
            { "UserId", "user-789" }
        };
        var expectedBodyContent = "<html><body>Error Content</body></html>";

        Dictionary<string, object>? capturedTemplateData = null;
        _mockTemplateService
            .Setup(x => x.ApplyTemplate("error-log", It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
            .Callback<string, Dictionary<string, object>, string>((_, data, _) => capturedTemplateData = data)
            .ReturnsAsync(expectedBodyContent);

        var sut = new EmailService(
            _mockGraphClient.Object,
            _emailConfig,
            _mockTemplateService.Object,
            _mockMailCheckClient.Object,
            _mockLogger.Object
        );

        // Act
        await sut.SendErrorNotification(stage, exception, serviceName, additionalContext);

        // Assert
        capturedTemplateData.Should().NotBeNull("template data should be captured");
        capturedTemplateData!["stage"].Should().Be(stage, "stage should be included in template data");
        capturedTemplateData["serviceName"].Should().Be(serviceName, "service name should be included in template data");
        
        var contextInTemplate = capturedTemplateData["additionalContext"] as Dictionary<string, string>;
        contextInTemplate.Should().NotBeNull("additional context should be included");
        contextInTemplate!["RequestId"].Should().Be("REQ-12345", "request ID should be in additional context");
        contextInTemplate["UserId"].Should().Be("user-789", "user ID should be in additional context");
    }
}