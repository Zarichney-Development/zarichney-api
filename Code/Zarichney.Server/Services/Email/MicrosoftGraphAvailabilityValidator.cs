using Zarichney.Services.Status;

namespace Zarichney.Services.Email;

/// <summary>
/// Utility class for validating Microsoft Graph service availability and configuration
/// </summary>
public static class MicrosoftGraphAvailabilityValidator
{
  /// <summary>
  /// Validates Microsoft Graph configuration and returns availability status with missing configuration details
  /// </summary>
  /// <param name="emailConfig">The email configuration to validate</param>
  /// <returns>A tuple containing availability status and list of missing configurations</returns>
  public static (bool IsAvailable, List<string>? MissingConfigs) ValidateConfiguration(EmailConfig emailConfig)
  {
    var missingConfigs = new List<string>();

    if (IsConfigurationMissing(emailConfig.AzureTenantId))
      missingConfigs.Add(EmailConfigConstants.AzureTenantIdKey);

    if (IsConfigurationMissing(emailConfig.AzureAppId))
      missingConfigs.Add(EmailConfigConstants.AzureAppIdKey);

    if (IsConfigurationMissing(emailConfig.AzureAppSecret))
      missingConfigs.Add(EmailConfigConstants.AzureAppSecretKey);

    return (missingConfigs.Count == 0, missingConfigs.Count > 0 ? missingConfigs : null);
  }

  /// <summary>
  /// Determines if a configuration value is missing or contains placeholder text
  /// </summary>
  /// <param name="value">The configuration value to check</param>
  /// <returns>True if the configuration is missing or placeholder, false otherwise</returns>
  private static bool IsConfigurationMissing(string? value)
  {
    return string.IsNullOrWhiteSpace(value) || value == StatusService.PlaceholderMessage;
  }
}
