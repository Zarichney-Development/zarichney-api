using Zarichney.Services.Logging.Models;

namespace Zarichney.Services.Logging;

/// <summary>
/// Service interface for managing logging system status, configuration, and Seq connectivity
/// </summary>
public interface ILoggingService
{
  /// <summary>
  /// Gets the current logging system status and configuration
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Detailed logging status information</returns>
  Task<LoggingStatusResult> GetLoggingStatusAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Tests connectivity to the specified Seq URL
  /// </summary>
  /// <param name="url">The Seq URL to test (optional, uses configured URL if not provided)</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Connectivity test results</returns>
  Task<SeqConnectivityResult> TestSeqConnectivityAsync(string? url = null, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets information about all available logging methods
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Information about all available logging options</returns>
  Task<LoggingMethodsResult> GetAvailableLoggingMethodsAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Finds the best available Seq instance using intelligent detection and fallback
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The URL of the best available Seq instance, or null if none found</returns>
  Task<string?> GetBestAvailableSeqUrlAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Tests connectivity to a specific Seq instance
  /// </summary>
  /// <param name="url">The Seq URL to test</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>True if Seq is accessible, false otherwise</returns>
  Task<bool> TryConnectToSeqAsync(string? url, CancellationToken cancellationToken = default);

  /// <summary>
  /// Attempts to start a Docker Seq container as fallback
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>True if Docker command executed successfully, false otherwise</returns>
  Task<bool> TryStartDockerSeqAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the current active Seq URL if available
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The active Seq URL or null if not available</returns>
  Task<string?> GetActiveSeqUrlAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Determines the current logging method being used
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Description of the current logging method</returns>
  Task<string> GetLoggingMethodAsync(CancellationToken cancellationToken = default);
}