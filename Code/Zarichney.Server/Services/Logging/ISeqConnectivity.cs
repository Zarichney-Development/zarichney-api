using Zarichney.Services.Logging.Models;

namespace Zarichney.Services.Logging;

/// <summary>
/// Service interface for testing Seq connectivity and managing Seq connections
/// </summary>
public interface ISeqConnectivity
{
  /// <summary>
  /// Tests connectivity to the specified Seq URL
  /// </summary>
  /// <param name="url">The Seq URL to test (optional, uses configured URL if not provided)</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Connectivity test results</returns>
  Task<SeqConnectivityResult> TestSeqConnectivityAsync(string? url = null, CancellationToken cancellationToken = default);

  /// <summary>
  /// Tests connectivity to a specific Seq instance
  /// </summary>
  /// <param name="url">The Seq URL to test</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>True if Seq is accessible, false otherwise</returns>
  Task<bool> TryConnectToSeqAsync(string? url, CancellationToken cancellationToken = default);

  /// <summary>
  /// Finds the best available Seq instance using intelligent detection and fallback
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The URL of the best available Seq instance, or null if none found</returns>
  Task<string?> GetBestAvailableSeqUrlAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the current active Seq URL if available
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>The active Seq URL or null if not available</returns>
  Task<string?> GetActiveSeqUrlAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Attempts to start a Docker Seq container as fallback
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>True if Docker command executed successfully, false otherwise</returns>
  Task<bool> TryStartDockerSeqAsync(CancellationToken cancellationToken = default);
}