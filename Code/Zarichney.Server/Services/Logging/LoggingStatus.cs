using Microsoft.Extensions.Options;
using Zarichney.Services.Logging.Models;
using Zarichney.Services.ProcessExecution;

namespace Zarichney.Services.Logging;

/// <summary>
/// Service for managing logging system status and configuration information
/// </summary>
public class LoggingStatus(
  ILogger<LoggingStatus> logger,
  IConfiguration configuration,
  IOptions<LoggingConfig> config,
  ISeqConnectivity seqConnectivity,
  IProcessExecutor processExecutor) : ILoggingStatus
{
  private readonly LoggingConfig _config = config.Value;

  /// <inheritdoc />
  public async Task<LoggingStatusResult> GetLoggingStatusAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var seqAvailable = await IsSeqAvailableAsync(cancellationToken);
      var activeSeqUrl = await seqConnectivity.GetActiveSeqUrlAsync(cancellationToken);
      var method = await GetLoggingMethodAsync(cancellationToken);

      var result = new LoggingStatusResult
      {
        SeqAvailable = seqAvailable,
        SeqUrl = activeSeqUrl,
        Method = method,
        FallbackActive = !seqAvailable,
        ConfiguredSeqUrl = _config.SeqUrl,
        LogLevel = configuration["Serilog:MinimumLevel:Default"] ?? "Warning",
        FileLoggingPath = GetFileLoggingPath(),
        Timestamp = DateTime.UtcNow
      };

      logger.LogInformation(
        "Logging status requested - Method: {Method}, SeqAvailable: {SeqAvailable}",
        result.Method, result.SeqAvailable);

      return result;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error retrieving logging status");
      throw;
    }
  }

  /// <inheritdoc />
  public async Task<LoggingMethodsResult> GetAvailableLoggingMethodsAsync(CancellationToken cancellationToken = default)
  {
    var currentMethod = await GetLoggingMethodAsync(cancellationToken);

    var result = new LoggingMethodsResult
    {
      NativeSeq = new LoggingMethodInfo
      {
        Available = await IsNativeSeqRunningAsync(cancellationToken),
        ServiceName = "seq",
        Port = 5341,
        Method = "Native systemd service"
      },
      DockerSeq = new LoggingMethodInfo
      {
        Available = await IsDockerSeqRunningAsync(cancellationToken),
        ServiceName = await GetDockerSeqContainerNameAsync(cancellationToken),
        Port = 5341,
        Method = "Docker container"
      },
      FileLogging = new LoggingMethodInfo
      {
        Available = true,
        Path = GetFileLoggingPath(),
        Method = "File-based logging (always available)"
      },
      CurrentMethod = currentMethod
    };

    return result;
  }

  /// <inheritdoc />
  public async Task<string> GetLoggingMethodAsync(CancellationToken cancellationToken = default)
  {
    if (!await IsSeqAvailableAsync(cancellationToken))
      return "File Logging (Fallback)";

    if (await IsNativeSeqRunningAsync(cancellationToken))
      return "Native Seq Service";

    if (await IsDockerSeqRunningAsync(cancellationToken))
      return "Docker Container";

    return "Unknown Seq Method";
  }

  private async Task<bool> IsSeqAvailableAsync(CancellationToken cancellationToken = default)
  {
    var configuredUrl = _config.SeqUrl;
    if (string.IsNullOrEmpty(configuredUrl))
      return false;

    return await seqConnectivity.TryConnectToSeqAsync(configuredUrl, cancellationToken);
  }

  private async Task<bool> IsNativeSeqRunningAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      // Check user service first
      var userServiceCheck = await processExecutor.RunCommandAsync("systemctl", "--user is-active seq", _config.ProcessTimeoutMs, cancellationToken);
      if (userServiceCheck.exitCode == 0 && userServiceCheck.output.Trim() == "active")
        return true;

      // Check system service
      var systemServiceCheck = await processExecutor.RunCommandAsync("systemctl", "is-active seq", _config.ProcessTimeoutMs, cancellationToken);
      return systemServiceCheck.exitCode == 0 && systemServiceCheck.output.Trim() == "active";
    }
    catch (Exception ex)
    {
      logger.LogDebug(ex, "Error checking native Seq service status");
      return false;
    }
  }

  private async Task<bool> IsDockerSeqRunningAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var result = await processExecutor.RunCommandAsync("docker", "ps --filter name=seq --format {{.Names}}", _config.ProcessTimeoutMs, cancellationToken);
      return result.exitCode == 0 && !string.IsNullOrWhiteSpace(result.output);
    }
    catch (Exception ex)
    {
      logger.LogDebug(ex, "Error checking Docker Seq status");
      return false;
    }
  }

  private async Task<string?> GetDockerSeqContainerNameAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var result = await processExecutor.RunCommandAsync("docker", "ps --filter name=seq --format {{.Names}}", _config.ProcessTimeoutMs, cancellationToken);
      if (result.exitCode == 0 && !string.IsNullOrWhiteSpace(result.output))
        return result.output.Trim().Split('\n').FirstOrDefault();
    }
    catch (Exception ex)
    {
      logger.LogDebug(ex, "Error getting Docker Seq container name");
    }
    return null;
  }

  private string GetFileLoggingPath()
  {
    return Path.Combine(Environment.CurrentDirectory, _config.DefaultFileLoggingPath);
  }
}