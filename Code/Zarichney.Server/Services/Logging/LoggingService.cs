using Zarichney.Services.Logging.Models;

namespace Zarichney.Services.Logging;

/// <summary>
/// Main logging service that orchestrates logging operations through focused services
/// </summary>
public class LoggingService(
  ILogger<LoggingService> logger,
  ILoggingStatus loggingStatus,
  ISeqConnectivity seqConnectivity) : ILoggingService
{

  /// <inheritdoc />
  public async Task<LoggingStatusResult> GetLoggingStatusAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      logger.LogDebug("Delegating logging status request to LoggingStatus service");
      return await loggingStatus.GetLoggingStatusAsync(cancellationToken);
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error retrieving logging status through orchestrated services");
      throw;
    }
  }

  /// <inheritdoc />
  public async Task<SeqConnectivityResult> TestSeqConnectivityAsync(string? url = null, CancellationToken cancellationToken = default)
  {
    logger.LogDebug("Delegating Seq connectivity test to SeqConnectivity service");
    return await seqConnectivity.TestSeqConnectivityAsync(url, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<LoggingMethodsResult> GetAvailableLoggingMethodsAsync(CancellationToken cancellationToken = default)
  {
    logger.LogDebug("Delegating available logging methods request to LoggingStatus service");
    return await loggingStatus.GetAvailableLoggingMethodsAsync(cancellationToken);
  }

  /// <inheritdoc />
  public async Task<string?> GetBestAvailableSeqUrlAsync(CancellationToken cancellationToken = default)
  {
    logger.LogDebug("Delegating best available Seq URL request to SeqConnectivity service");
    return await seqConnectivity.GetBestAvailableSeqUrlAsync(cancellationToken);
  }

  /// <inheritdoc />
  public async Task<bool> TryConnectToSeqAsync(string? url, CancellationToken cancellationToken = default)
  {
    logger.LogDebug("Delegating Seq connection attempt to SeqConnectivity service");
    return await seqConnectivity.TryConnectToSeqAsync(url, cancellationToken);
  }

  /// <inheritdoc />
  public async Task<bool> TryStartDockerSeqAsync(CancellationToken cancellationToken = default)
  {
    logger.LogDebug("Delegating Docker Seq startup to SeqConnectivity service");
    return await seqConnectivity.TryStartDockerSeqAsync(cancellationToken);
  }

  /// <inheritdoc />
  public async Task<string?> GetActiveSeqUrlAsync(CancellationToken cancellationToken = default)
  {
    logger.LogDebug("Delegating active Seq URL request to SeqConnectivity service");
    return await seqConnectivity.GetActiveSeqUrlAsync(cancellationToken);
  }

  /// <inheritdoc />
  public async Task<string> GetLoggingMethodAsync(CancellationToken cancellationToken = default)
  {
    logger.LogDebug("Delegating logging method request to LoggingStatus service");
    return await loggingStatus.GetLoggingMethodAsync(cancellationToken);
  }
}