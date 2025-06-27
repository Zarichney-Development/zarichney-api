namespace Zarichney.Services.Status;

/// <summary>
/// Exception thrown when a service is unavailable due to missing configuration or other reasons.
/// This exception is used to signal that a service cannot operate normally and should result
/// in a 503 Service Unavailable response.
/// </summary>
public class ServiceUnavailableException : Exception
{
  /// <summary>
  /// A list of reasons why the service is unavailable.
  /// </summary>
  public List<string>? Reasons { get; } = [];

  /// <summary>
  /// Initializes a new instance of the <see cref="ServiceUnavailableException"/> class.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  public ServiceUnavailableException(string message)
      : base(message)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="ServiceUnavailableException"/> class.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  /// <param name="reasons">A list of reasons why the service is unavailable.</param>
  public ServiceUnavailableException(string message, List<string>? reasons)
      : base(message)
  {
    if (reasons != null)
    {
      Reasons = reasons;
    }
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="ServiceUnavailableException"/> class.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  /// <param name="innerException">The exception that is the cause of the current exception.</param>
  public ServiceUnavailableException(string message, Exception innerException)
      : base(message, innerException)
  {
  }

  /// <summary>
  /// Initializes a new instance of the <see cref="ServiceUnavailableException"/> class.
  /// </summary>
  /// <param name="message">The error message that explains the reason for the exception.</param>
  /// <param name="reasons">A list of reasons why the service is unavailable.</param>
  /// <param name="innerException">The exception that is the cause of the current exception.</param>
  public ServiceUnavailableException(string message, List<string>? reasons, Exception innerException)
      : base(message, innerException)
  {
    if (reasons != null)
    {
      Reasons = reasons;
    }
  }

  /// <summary>
  /// Adds a reason why the service is unavailable.
  /// </summary>
  /// <param name="reason">The reason to add.</param>
  public void AddReason(string reason)
  {
    if (!string.IsNullOrWhiteSpace(reason))
    {
      Reasons?.Add(reason);
    }
  }
}
