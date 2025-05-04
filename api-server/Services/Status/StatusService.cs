using Zarichney.Services.AI;
using Zarichney.Services.Email;
using Zarichney.Services.GitHub;
using Zarichney.Services.Payment;

namespace Zarichney.Services.Status;

public interface IStatusService
{
  Task<List<ConfigurationItemStatus>> GetConfigurationStatusAsync();
}

public class StatusService(
    LlmConfig llmConfig,
    EmailConfig emailConfig,
    GitHubConfig gitHubConfig,
    PaymentConfig paymentConfig,
    IConfiguration configuration)
    : IStatusService
{
  private const string Placeholder = "recommended to set in app secrets";

  private ConfigurationItemStatus CheckConfigurationItem(string itemName, string? itemValue, string propertyName)
  {
    if (!string.IsNullOrWhiteSpace(itemValue) && itemValue != Placeholder)
      return new ConfigurationItemStatus(itemName, "Configured");

    return new ConfigurationItemStatus(itemName, "Missing/Invalid", $"{propertyName} is missing or placeholder");
  }

  public Task<List<ConfigurationItemStatus>> GetConfigurationStatusAsync()
  {
    var statusList = new List<ConfigurationItemStatus>
        {
            // OpenAI
            CheckConfigurationItem("OpenAI API Key", llmConfig.ApiKey, nameof(llmConfig.ApiKey)),

            // Email
            CheckConfigurationItem("Email AzureTenantId", emailConfig.AzureTenantId, nameof(emailConfig.AzureTenantId)),
            CheckConfigurationItem("Email AzureAppId", emailConfig.AzureAppId, nameof(emailConfig.AzureAppId)),
            CheckConfigurationItem("Email AzureAppSecret", emailConfig.AzureAppSecret, nameof(emailConfig.AzureAppSecret)),
            CheckConfigurationItem("Email FromEmail", emailConfig.FromEmail, nameof(emailConfig.FromEmail)),
            CheckConfigurationItem("Email MailCheckApiKey", emailConfig.MailCheckApiKey, nameof(emailConfig.MailCheckApiKey)),

            // GitHub
            CheckConfigurationItem("GitHub Access Token", gitHubConfig.AccessToken, nameof(gitHubConfig.AccessToken)),

            // Stripe
            CheckConfigurationItem("Stripe Secret Key", paymentConfig.StripeSecretKey, nameof(paymentConfig.StripeSecretKey)),
            CheckConfigurationItem("Stripe Webhook Secret", paymentConfig.StripeWebhookSecret, nameof(paymentConfig.StripeWebhookSecret)),

            // Database Connection
            CheckConfigurationItem("Database Connection", configuration["ConnectionStrings:IdentityConnection"], "IdentityConnection")
        };

    return Task.FromResult(statusList);
  }
}
