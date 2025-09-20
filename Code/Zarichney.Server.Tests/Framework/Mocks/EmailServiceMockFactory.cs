using Microsoft.Graph;
using Microsoft.Graph.Models;
using Moq;
using Zarichney.Config;
using Zarichney.Services.Email;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Server.Tests.Framework.Mocks;

/// <summary>
/// Factory for creating mock dependencies for EmailService testing.
/// Provides common mock configurations to reduce test setup duplication.
/// Register as singleton in DI container for access via GetService&lt;EmailServiceMockFactory&gt;().
/// </summary>
public class EmailServiceMockFactory
{
  /// <summary>
  /// Creates a mock GraphServiceClient for unit testing.
  /// Note: Complex Graph API interactions are better tested in integration tests.
  /// </summary>
  public Mock<GraphServiceClient> CreateMockGraphServiceClient()
  {
    var mockGraphClient = new Mock<GraphServiceClient>();
    // Basic mock - complex Graph API mocking is done in integration tests
    return mockGraphClient;
  }

  /// <summary>
  /// Creates a mock EmailConfig with test-friendly default values.
  /// </summary>
  public EmailConfig CreateMockEmailConfig()
  {
    return new EmailConfig
    {
      AzureTenantId = "test-tenant-id",
      AzureAppId = "test-app-id",
      AzureAppSecret = "test-app-secret",
      FromEmail = "test@zarichney.com",
      TemplateDirectory = "/test/templates",
      MailCheckApiKey = "test-mailcheck-api-key"
    };
  }

  /// <summary>
  /// Creates a mock ITemplateService with configurable template responses.
  /// </summary>
  public Mock<ITemplateService> CreateMockTemplateService()
  {
    var mockTemplateService = new Mock<ITemplateService>();

    // Default successful template application
    mockTemplateService.Setup(x => x.ApplyTemplate(
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<string>()))
        .ReturnsAsync((string templateName, Dictionary<string, object> templateData, string title) =>
            $"<html><body><h1>{title}</h1><p>Template: {templateName}</p></body></html>");

    return mockTemplateService;
  }

  /// <summary>
  /// Creates a mock IMailCheckClient with configurable validation responses.
  /// </summary>
  public Mock<IMailCheckClient> CreateMockMailCheckClient(EmailValidationResponse? defaultResponse = null)
  {
    var mockMailCheckClient = new Mock<IMailCheckClient>();

    // Use provided response or create a valid default
    var response = defaultResponse ?? new EmailValidationResponseBuilder().WithValidDefaults().Build();

    mockMailCheckClient.Setup(x => x.GetValidationData(It.IsAny<string>()))
        .ReturnsAsync(response);

    return mockMailCheckClient;
  }

  /// <summary>
  /// Creates a mock IMailCheckClient that throws a specific exception.
  /// </summary>
  public Mock<IMailCheckClient> CreateMockMailCheckClientWithException<TException>()
      where TException : Exception, new()
  {
    var mockMailCheckClient = new Mock<IMailCheckClient>();

    mockMailCheckClient.Setup(x => x.GetValidationData(It.IsAny<string>()))
        .ThrowsAsync(new TException());

    return mockMailCheckClient;
  }

  /// <summary>
  /// Creates a mock ITemplateService that throws an exception for template processing.
  /// </summary>
  public Mock<ITemplateService> CreateMockTemplateServiceWithException<TException>()
      where TException : Exception, new()
  {
    var mockTemplateService = new Mock<ITemplateService>();

    mockTemplateService.Setup(x => x.ApplyTemplate(
            It.IsAny<string>(),
            It.IsAny<Dictionary<string, object>>(),
            It.IsAny<string>()))
        .ThrowsAsync(new TException());

    return mockTemplateService;
  }

}
