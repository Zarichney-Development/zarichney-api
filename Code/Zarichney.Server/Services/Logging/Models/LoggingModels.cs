namespace Zarichney.Services.Logging.Models;

/// <summary>
/// Result model for logging system status
/// </summary>
public class LoggingStatusResult
{
  /// <summary>
  /// Whether Seq is available and accessible
  /// </summary>
  public bool SeqAvailable { get; set; }

  /// <summary>
  /// The active Seq URL if available
  /// </summary>
  public string? SeqUrl { get; set; }

  /// <summary>
  /// Description of the current logging method
  /// </summary>
  public string Method { get; set; } = string.Empty;

  /// <summary>
  /// Whether fallback logging is currently active
  /// </summary>
  public bool FallbackActive { get; set; }

  /// <summary>
  /// The configured Seq URL from configuration
  /// </summary>
  public string? ConfiguredSeqUrl { get; set; }

  /// <summary>
  /// The current log level setting
  /// </summary>
  public string LogLevel { get; set; } = string.Empty;

  /// <summary>
  /// The file logging path when using file fallback
  /// </summary>
  public string FileLoggingPath { get; set; } = string.Empty;

  /// <summary>
  /// Timestamp when the status was generated
  /// </summary>
  public DateTime Timestamp { get; set; }
}

/// <summary>
/// Result model for Seq connectivity testing
/// </summary>
public class SeqConnectivityResult
{
  /// <summary>
  /// The URL that was tested
  /// </summary>
  public string Url { get; set; } = string.Empty;

  /// <summary>
  /// Whether the connection was successful
  /// </summary>
  public bool IsConnected { get; set; }

  /// <summary>
  /// Timestamp when the test was performed
  /// </summary>
  public DateTime TestedAt { get; set; }

  /// <summary>
  /// Response time in milliseconds (-1 if failed)
  /// </summary>
  public long ResponseTime { get; set; }

  /// <summary>
  /// Error message if connection failed
  /// </summary>
  public string? Error { get; set; }
}

/// <summary>
/// Result model for available logging methods
/// </summary>
public class LoggingMethodsResult
{
  /// <summary>
  /// Native Seq service information
  /// </summary>
  public LoggingMethodInfo NativeSeq { get; set; } = new();

  /// <summary>
  /// Docker Seq container information
  /// </summary>
  public LoggingMethodInfo DockerSeq { get; set; } = new();

  /// <summary>
  /// File logging information
  /// </summary>
  public LoggingMethodInfo FileLogging { get; set; } = new();

  /// <summary>
  /// Description of the currently active method
  /// </summary>
  public string CurrentMethod { get; set; } = string.Empty;
}

/// <summary>
/// Information about a specific logging method
/// </summary>
public class LoggingMethodInfo
{
  /// <summary>
  /// Whether this logging method is available
  /// </summary>
  public bool Available { get; set; }

  /// <summary>
  /// Service name or container name (if applicable)
  /// </summary>
  public string? ServiceName { get; set; }

  /// <summary>
  /// Port number (if applicable)
  /// </summary>
  public int? Port { get; set; }

  /// <summary>
  /// Description of the method
  /// </summary>
  public string Method { get; set; } = string.Empty;

  /// <summary>
  /// File path (for file logging)
  /// </summary>
  public string? Path { get; set; }
}

/// <summary>
/// Request model for testing Seq connectivity
/// </summary>
public class TestSeqRequest
{
  /// <summary>
  /// The Seq URL to test
  /// </summary>
  public string? Url { get; set; }
}