using System.ComponentModel.DataAnnotations;
using Zarichney.Config;

namespace Zarichney.Services.Logging;

/// <summary>
/// Configuration model for logging settings
/// </summary>
public class LoggingConfig : IConfig
{
  /// <summary>
  /// The configured Seq URL for structured logging
  /// </summary>
  public string? SeqUrl { get; set; }

  /// <summary>
  /// Timeout in seconds for Seq connectivity tests
  /// </summary>
  public int SeqTimeoutSeconds { get; set; } = 3;

  /// <summary>
  /// Common URLs to test for Seq availability
  /// </summary>
  public string[] CommonSeqUrls { get; set; } = 
  [
    "http://localhost:5341",
    "http://127.0.0.1:5341",
    "http://localhost:8080"
  ];

  /// <summary>
  /// Whether to attempt starting Docker Seq as fallback
  /// </summary>
  public bool EnableDockerFallback { get; set; } = true;

  /// <summary>
  /// Docker container name for Seq fallback
  /// </summary>
  public string DockerContainerName { get; set; } = "seq-fallback";

  /// <summary>
  /// Docker image to use for Seq container
  /// </summary>
  public string DockerImage { get; set; } = "datalust/seq:latest";

  /// <summary>
  /// Default file logging path when Seq is not available
  /// </summary>
  public string DefaultFileLoggingPath { get; set; } = "logs/Zarichney.Server.log";

  /// <summary>
  /// Process execution timeout in milliseconds
  /// </summary>
  public int ProcessTimeoutMs { get; set; } = 5000;

  /// <summary>
  /// Docker container startup delay in milliseconds
  /// </summary>
  public int DockerStartupDelayMs { get; set; } = 3000;
}