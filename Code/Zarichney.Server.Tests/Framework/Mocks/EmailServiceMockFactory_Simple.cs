using Microsoft.Extensions.Logging;
using Moq;
using Zarichney.Services.Email;

namespace Zarichney.Tests.Framework.Mocks;

/// <summary>
/// Simple factory for creating mock instances related to email services.
/// Provides pre-configured mocks with common behaviors for testing email functionality.
/// </summary>
public static class EmailServiceMockFactorySimple
{
    /// <summary>
    /// Creates a mock ITemplateService configured with default successful template processing.
    /// </summary>
    public static Mock<ITemplateService> CreateTemplateServiceMock()
    {
        var mockTemplateService = new Mock<ITemplateService>();
        
        mockTemplateService
            .Setup(x => x.ApplyTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
            .ReturnsAsync((string templateName, Dictionary<string, object> data, string title) =>
                $"<html><head><title>{title}</title></head><body><h1>Template: {templateName}</h1><p>Content processed</p></body></html>");

        return mockTemplateService;
    }

    /// <summary>
    /// Creates a mock ITemplateService that throws an exception during template processing.
    /// </summary>
    public static Mock<ITemplateService> CreateTemplateServiceMockWithFailure(Exception exception)
    {
        var mockTemplateService = new Mock<ITemplateService>();
        
        mockTemplateService
            .Setup(x => x.ApplyTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
            .ThrowsAsync(exception);

        return mockTemplateService;
    }

    /// <summary>
    /// Creates a mock ITemplateService that returns empty content (simulating template processing failure).
    /// </summary>
    public static Mock<ITemplateService> CreateTemplateServiceMockWithEmptyContent()
    {
        var mockTemplateService = new Mock<ITemplateService>();
        
        mockTemplateService
            .Setup(x => x.ApplyTemplate(It.IsAny<string>(), It.IsAny<Dictionary<string, object>>(), It.IsAny<string>()))
            .ReturnsAsync(string.Empty);

        return mockTemplateService;
    }

    /// <summary>
    /// Creates a mock IMailCheckClient configured with default valid email responses.
    /// </summary>
    public static Mock<IMailCheckClient> CreateMailCheckClientMock()
    {
        var mockMailCheckClient = new Mock<IMailCheckClient>();
        
        mockMailCheckClient
            .Setup(x => x.GetValidationData(It.IsAny<string>()))
            .ReturnsAsync((string domain) => new EmailValidationResponse
            {
                Valid = true,
                Block = false,
                Disposable = false,
                EmailForwarder = false,
                Domain = domain,
                Text = "Valid domain",
                Reason = "Domain has good reputation",
                Risk = 10,
                MxHost = $"mx.{domain}",
                PossibleTypo = Array.Empty<string>(),
                MxIp = "192.168.1.1",
                MxInfo = "Valid MX record",
                LastChangedAt = DateTime.UtcNow.AddDays(-30)
            });

        return mockMailCheckClient;
    }

    /// <summary>
    /// Creates a mock IMailCheckClient that returns custom validation responses.
    /// </summary>
    public static Mock<IMailCheckClient> CreateMailCheckClientMockWithResponse(EmailValidationResponse response)
    {
        var mockMailCheckClient = new Mock<IMailCheckClient>();
        
        mockMailCheckClient
            .Setup(x => x.GetValidationData(It.IsAny<string>()))
            .ReturnsAsync(response);

        return mockMailCheckClient;
    }

    /// <summary>
    /// Creates a mock IMailCheckClient that throws an exception during validation.
    /// </summary>
    public static Mock<IMailCheckClient> CreateMailCheckClientMockWithFailure(Exception exception)
    {
        var mockMailCheckClient = new Mock<IMailCheckClient>();
        
        mockMailCheckClient
            .Setup(x => x.GetValidationData(It.IsAny<string>()))
            .ThrowsAsync(exception);

        return mockMailCheckClient;
    }

    /// <summary>
    /// Creates a default EmailConfig for testing with all required properties set.
    /// </summary>
    public static EmailConfig CreateDefaultEmailConfig(string? fromEmail = null)
    {
        return new EmailConfig
        {
            AzureTenantId = "test-tenant-id",
            AzureAppId = "test-app-id",
            AzureAppSecret = "test-app-secret",
            FromEmail = fromEmail ?? "test@zarichney.dev",
            TemplateDirectory = "/test/templates",
            MailCheckApiKey = "test-mailcheck-key"
        };
    }

    /// <summary>
    /// Creates an EmailConfig with missing configuration (for testing configuration validation).
    /// </summary>
    public static EmailConfig CreateIncompleteEmailConfig()
    {
        return new EmailConfig
        {
            AzureTenantId = "test-tenant-id",
            AzureAppId = "test-app-id",
            AzureAppSecret = "test-app-secret",
            FromEmail = "test@zarichney.dev",
            TemplateDirectory = "/test/templates",
            MailCheckApiKey = string.Empty // Missing/invalid API key
        };
    }

    /// <summary>
    /// Creates a mock ILogger&lt;EmailService&gt; for testing logging behavior.
    /// </summary>
    public static Mock<ILogger<EmailService>> CreateEmailServiceLoggerMock()
    {
        return new Mock<ILogger<EmailService>>();
    }
}