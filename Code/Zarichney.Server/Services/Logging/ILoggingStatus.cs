using Zarichney.Services.Logging.Models;

namespace Zarichney.Services.Logging;

/// <summary>
/// Service interface for logging system status and configuration operations
/// </summary>
public interface ILoggingStatus
{
  /// <summary>
  /// Gets the current logging system status and configuration
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Detailed logging status information</returns>
  Task<LoggingStatusResult> GetLoggingStatusAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets information about all available logging methods
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Information about all available logging options</returns>
  Task<LoggingMethodsResult> GetAvailableLoggingMethodsAsync(CancellationToken cancellationToken = default);

  /// <summary>
  /// Determines the current logging method being used
  /// </summary>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Description of the current logging method</returns>
  Task<string> GetLoggingMethodAsync(CancellationToken cancellationToken = default);
}