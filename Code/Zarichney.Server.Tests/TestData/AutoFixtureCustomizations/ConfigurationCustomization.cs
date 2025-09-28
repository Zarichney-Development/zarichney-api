using AutoFixture;
using Zarichney.Config;
using Zarichney.Tests.TestData.Builders;
using Zarichney.Services.Email;

namespace Zarichney.Tests.TestData.AutoFixtureCustomizations;

/// <summary>
/// AutoFixture customization for configuration objects used throughout the application.
/// Provides consistent test data for PaymentConfig, ClientConfig, and EmailConfig instances.
/// </summary>
public class ConfigurationCustomization : ICustomization
{
  public void Customize(IFixture fixture)
  {
    // Create PaymentConfig using the builder for consistent test data
    // Include all required configuration for payment service functionality
    fixture.Register(() => new PaymentConfigBuilder().Build());

    // Create ClientConfig with valid test data
    fixture.Register(() => new ClientConfig
    {
      BaseUrl = "https://test.example.com"
    });

    // Create EmailConfig with comprehensive test configuration
    fixture.Register(() => new EmailConfig
    {
      AzureTenantId = "test-tenant-id",
      AzureAppId = "test-app-id",
      AzureAppSecret = "test-app-secret",
      FromEmail = "test@zarichney.com",
      TemplateDirectory = "/test/templates",
      MailCheckApiKey = "test-mailcheck-api-key"
    });
  }
}
