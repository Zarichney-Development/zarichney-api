using Microsoft.IdentityModel.Protocols.Configuration;

namespace Zarichney.Config;

/// <summary>
/// Exception thrown when a required configuration value is missing or invalid at runtime.
/// Unlike <see cref="InvalidConfigurationException"/>, this exception is thrown at the
/// time a service attempts to use a configuration value, not during application startup.
/// </summary>
public class ConfigurationMissingException : InvalidOperationException
{
  /// <summary>
  /// The name of the configuration section containing the missing or invalid configuration.
  /// </summary>
  public string ConfigurationSection { get; }

  /// <summary>
  /// Details about the specific configuration key(s) that are missing or invalid.
  /// </summary>
  public string MissingKeyDetails { get; }

  /// <summary>
  /// Initializes a new instance of the <see cref="ConfigurationMissingException"/> class.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  public ConfigurationMissingException(string message)
      : base(message)
  {
    ConfigurationSection = string.Empty;
    MissingKeyDetails = string.Empty;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="ConfigurationMissingException"/> class.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  /// <param name="innerException">The exception that is the cause of the current exception.</param>
  public ConfigurationMissingException(string message, Exception innerException)
      : base(message, innerException)
  {
    ConfigurationSection = string.Empty;
    MissingKeyDetails = string.Empty;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="ConfigurationMissingException"/> class.
  /// </summary>
  /// <param name="configurationSection">The name of the configuration section containing the missing configuration.</param>
  /// <param name="missingKeyDetails">Details about the specific configuration key(s) that are missing.</param>
  public ConfigurationMissingException(string configurationSection, string missingKeyDetails)
      : base($"Configuration is missing or invalid for '{configurationSection}'. Required details: {missingKeyDetails}. The service cannot operate.")
  {
    ConfigurationSection = configurationSection;
    MissingKeyDetails = missingKeyDetails;
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="ConfigurationMissingException"/> class.
  /// </summary>
  /// <param name="configurationSection">The name of the configuration section containing the missing configuration.</param>
  /// <param name="missingKeyDetails">Details about the specific configuration key(s) that are missing.</param>
  /// <param name="innerException">The exception that is the cause of the current exception.</param>
  public ConfigurationMissingException(string configurationSection, string missingKeyDetails, Exception innerException)
      : base($"Configuration is missing or invalid for '{configurationSection}'. Required details: {missingKeyDetails}. The service cannot operate.", innerException)
  {
    ConfigurationSection = configurationSection;
    MissingKeyDetails = missingKeyDetails;
  }
}
