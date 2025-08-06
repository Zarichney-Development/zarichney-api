using System.Diagnostics;
using Microsoft.Extensions.Options;
using Zarichney.Services.Logging.Models;

namespace Zarichney.Services.Logging;

/// <summary>
/// Service for managing logging system status, configuration, and Seq connectivity
/// </summary>
public class LoggingService : ILoggingService
{
  private readonly ILogger<LoggingService> _logger;
  private readonly IConfiguration _configuration;
  private readonly LoggingConfig _config;
  private readonly HttpClient _httpClient;

  public LoggingService(
    ILogger<LoggingService> logger,
    IConfiguration configuration,
    IOptions<LoggingConfig> config,
    HttpClient httpClient)
  {
    _logger = logger;
    _configuration = configuration;
    _config = config.Value;
    _httpClient = httpClient;
  }

  /// <inheritdoc />
  public async Task<LoggingStatusResult> GetLoggingStatusAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var seqAvailable = await IsSeqAvailableAsync(cancellationToken);
      var activeSeqUrl = await GetActiveSeqUrlAsync(cancellationToken);
      var method = await GetLoggingMethodAsync(cancellationToken);

      var result = new LoggingStatusResult
      {
        SeqAvailable = seqAvailable,
        SeqUrl = activeSeqUrl,
        Method = method,
        FallbackActive = !seqAvailable,
        ConfiguredSeqUrl = _config.SeqUrl,
        LogLevel = _configuration["Serilog:MinimumLevel:Default"] ?? "Warning",
        FileLoggingPath = GetFileLoggingPath(),
        Timestamp = DateTime.UtcNow
      };

      _logger.LogInformation(
        "Logging status requested - Method: {Method}, SeqAvailable: {SeqAvailable}",
        result.Method, result.SeqAvailable);

      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error retrieving logging status");
      throw;
    }
  }

  /// <inheritdoc />
  public async Task<SeqConnectivityResult> TestSeqConnectivityAsync(string? url = null, CancellationToken cancellationToken = default)
  {
    var urlToTest = url ?? _config.SeqUrl ?? "http://localhost:5341";

    try
    {
      var stopwatch = Stopwatch.StartNew();
      var isConnected = await TryConnectToSeqAsync(urlToTest, cancellationToken);
      stopwatch.Stop();

      var result = new SeqConnectivityResult
      {
        Url = urlToTest,
        IsConnected = isConnected,
        TestedAt = DateTime.UtcNow,
        ResponseTime = isConnected ? stopwatch.ElapsedMilliseconds : -1
      };

      _logger.LogInformation(
        "Seq connectivity test - URL: {Url}, Connected: {IsConnected}, ResponseTime: {ResponseTime}ms",
        urlToTest, isConnected, result.ResponseTime);

      return result;
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Error testing Seq connectivity to {Url}", urlToTest);
      
      return new SeqConnectivityResult
      {
        Url = urlToTest,
        IsConnected = false,
        Error = ex.Message,
        TestedAt = DateTime.UtcNow,
        ResponseTime = -1
      };
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
  public async Task<string?> GetBestAvailableSeqUrlAsync(CancellationToken cancellationToken = default)
  {
    // Try configured URL first (could be native or Docker)
    if (!string.IsNullOrEmpty(_config.SeqUrl) && await TryConnectToSeqAsync(_config.SeqUrl, cancellationToken))
    {
      _logger.LogInformation("Using configured Seq at: {SeqUrl}", _config.SeqUrl);
      return _config.SeqUrl;
    }

    // Try common native Seq ports
    foreach (var url in _config.CommonSeqUrls)
    {
      if (await TryConnectToSeqAsync(url, cancellationToken))
      {
        _logger.LogInformation("Found native Seq at: {SeqUrl}", url);
        return url;
      }
    }

    // Try to start Docker Seq as fallback
    if (_config.EnableDockerFallback && await TryStartDockerSeqAsync(cancellationToken))
    {
      _logger.LogInformation("Attempting to start Docker Seq container...");
      
      // Wait for container startup
      await Task.Delay(3000, cancellationToken);
      
      if (await TryConnectToSeqAsync("http://localhost:5341", cancellationToken))
      {
        _logger.LogInformation("Successfully started Docker Seq container");
        return "http://localhost:5341";
      }
    }

    _logger.LogWarning("No Seq instance found - will use file logging");
    return null;
  }

  /// <inheritdoc />
  public async Task<bool> TryConnectToSeqAsync(string? url, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrEmpty(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
      return false;

    try
    {
      using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      timeoutCts.CancelAfter(TimeSpan.FromSeconds(_config.SeqTimeoutSeconds));
      
      var response = await _httpClient.GetAsync($"{url}/api/events/raw", timeoutCts.Token);
      return response.IsSuccessStatusCode;
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      throw;
    }
    catch (OperationCanceledException)
    {
      // Timeout
      return false;
    }
    catch (Exception ex)
    {
      _logger.LogDebug(ex, "Failed to connect to Seq at {Url}", url);
      return false;
    }
  }

  /// <inheritdoc />
  public async Task<bool> TryStartDockerSeqAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      // First check if a Seq container already exists
      if (await IsDockerSeqContainerRunningAsync(cancellationToken))
      {
        _logger.LogInformation("Docker Seq container already running");
        return true;
      }

      // Try to start new container
      var processInfo = new ProcessStartInfo
      {
        FileName = "docker",
        Arguments = $"run --name {_config.DockerContainerName} -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 {_config.DockerImage}",
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
      };

      using var process = Process.Start(processInfo);
      if (process != null)
      {
        await process.WaitForExitAsync(cancellationToken);
        if (process.ExitCode == 0)
        {
          _logger.LogInformation("Docker Seq container started successfully");
          return true;
        }
      }

      _logger.LogWarning("Failed to start Docker Seq container");
      return false;
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, "Error attempting to start Docker Seq container");
      return false;
    }
  }

  /// <inheritdoc />
  public async Task<string?> GetActiveSeqUrlAsync(CancellationToken cancellationToken = default)
  {
    var configuredUrl = _config.SeqUrl;
    
    if (!string.IsNullOrEmpty(configuredUrl) && await TryConnectToSeqAsync(configuredUrl, cancellationToken))
      return configuredUrl;

    // Test common URLs
    foreach (var url in _config.CommonSeqUrls)
    {
      if (await TryConnectToSeqAsync(url, cancellationToken))
        return url;
    }

    return null;
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

  #region Private Helper Methods

  private async Task<bool> IsSeqAvailableAsync(CancellationToken cancellationToken = default)
  {
    var configuredUrl = _config.SeqUrl;
    if (string.IsNullOrEmpty(configuredUrl))
      return false;

    return await TryConnectToSeqAsync(configuredUrl, cancellationToken);
  }

  private async Task<bool> IsNativeSeqRunningAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      // Check user service first
      var userServiceCheck = await RunCommandAsync("systemctl", "--user is-active seq", cancellationToken);
      if (userServiceCheck.exitCode == 0 && userServiceCheck.output.Trim() == "active")
        return true;

      // Check system service
      var systemServiceCheck = await RunCommandAsync("systemctl", "is-active seq", cancellationToken);
      return systemServiceCheck.exitCode == 0 && systemServiceCheck.output.Trim() == "active";
    }
    catch (Exception ex)
    {
      _logger.LogDebug(ex, "Error checking native Seq service status");
      return false;
    }
  }

  private async Task<bool> IsDockerSeqRunningAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var result = await RunCommandAsync("docker", "ps --filter name=seq --format {{.Names}}", cancellationToken);
      return result.exitCode == 0 && !string.IsNullOrWhiteSpace(result.output);
    }
    catch (Exception ex)
    {
      _logger.LogDebug(ex, "Error checking Docker Seq status");
      return false;
    }
  }

  private async Task<bool> IsDockerSeqContainerRunningAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var result = await RunCommandAsync("docker", "ps --filter name=seq --format {{.Names}}", cancellationToken);
      return !string.IsNullOrWhiteSpace(result.output);
    }
    catch (Exception ex)
    {
      _logger.LogDebug(ex, "Error checking Docker Seq container status");
      return false;
    }
  }

  private async Task<string?> GetDockerSeqContainerNameAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var result = await RunCommandAsync("docker", "ps --filter name=seq --format {{.Names}}", cancellationToken);
      if (result.exitCode == 0 && !string.IsNullOrWhiteSpace(result.output))
        return result.output.Trim().Split('\n').FirstOrDefault();
    }
    catch (Exception ex)
    {
      _logger.LogDebug(ex, "Error getting Docker Seq container name");
    }
    return null;
  }

  private string GetFileLoggingPath()
  {
    return Path.Combine(Environment.CurrentDirectory, _config.DefaultFileLoggingPath);
  }

  private async Task<(int exitCode, string output)> RunCommandAsync(
    string command, 
    string arguments, 
    CancellationToken cancellationToken = default)
  {
    try
    {
      var processInfo = new ProcessStartInfo
      {
        FileName = command,
        Arguments = arguments,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        CreateNoWindow = true
      };

      using var process = Process.Start(processInfo);
      if (process != null)
      {
        var output = await process.StandardOutput.ReadToEndAsync();
        
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(_config.ProcessTimeoutMs);
        
        await process.WaitForExitAsync(timeoutCts.Token);
        return (process.ExitCode, output);
      }

      return (-1, "");
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      throw;
    }
    catch (OperationCanceledException)
    {
      // Timeout
      return (-1, "");
    }
    catch (Exception ex)
    {
      _logger.LogDebug(ex, "Error running command: {Command} {Arguments}", command, arguments);
      return (-1, "");
    }
  }

  #endregion
}