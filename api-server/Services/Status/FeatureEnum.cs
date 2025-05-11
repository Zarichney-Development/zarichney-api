namespace Zarichney.Services.Status;

/// <summary>
/// Defines all the features available in the application that can be enabled or disabled
/// based on configuration availability.
/// </summary>
public enum Feature
{
  /// <summary>
  /// Core functionality required by the application.
  /// </summary>
  Core,

  /// <summary>
  /// Language Model services for text generation and completions.
  /// </summary>
  LLM,

  /// <summary>
  /// Audio transcription services.
  /// </summary>
  Transcription,

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
  /// AI-related services (combines LLM, Transcription, etc.)
  /// </summary>
  AiServices
}
