namespace Zarichney.Services.Status;

/// <summary>
/// Defines all the features available in the application that can be enabled or disabled
/// based on configuration availability.
/// </summary>
public enum ExternalServices
{
  /// <summary>
  /// Functionality required by the front-end website (https://zarichney.com).
  /// </summary>
  FrontEnd,

  /// <summary>
  /// Audio transcription services.
  /// </summary>
  OpenAiApi,

  /// <summary>
  /// Email sending capabilities.
  /// </summary>
  EmailSending,

  /// <summary>
  /// Payment processing functionality.
  /// </summary>
  Payments,

  /// <summary>
  /// GitHub integration for code storage and retrieval.
  /// </summary>
  GitHubAccess,

  /// <summary>
  /// A service to ensure a given email is legitimate
  /// </summary>
  EmailValidation
}
