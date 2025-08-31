using System.Diagnostics;
using Microsoft.Extensions.Options;
using Zarichney.Services.Logging.Models;
using Zarichney.Services.ProcessExecution;
using Zarichney.Services.Security;

namespace Zarichney.Services.Logging;

/// <summary>
/// Service for managing Seq connectivity and container management
/// </summary>
public class SeqConnectivity(
  ILogger<SeqConnectivity> logger,
  IOptions<LoggingConfig> config,
  HttpClient httpClient,
  IProcessExecutor processExecutor,
  IOutboundUrlSecurity urlSecurity) : ISeqConnectivity
{
  private readonly LoggingConfig _config = config.Value;

  /// <inheritdoc />
  public async Task<SeqConnectivityResult> TestSeqConnectivityAsync(string? url = null, CancellationToken cancellationToken = default)
  {
    var urlToTest = url ?? _config.SeqUrl ?? "http://localhost:5341";

    try
    {
      // Validate URL for security before testing
      var validationResult = await urlSecurity.ValidateSeqUrlAsync(urlToTest, cancellationToken);
      
      if (!validationResult.IsValid)
      {
        logger.LogWarning("Seq connectivity test blocked: {ReasonCode} for host {Host}",
          validationResult.ReasonCode, validationResult.SanitizedHost);
          
        return new SeqConnectivityResult
        {
          Url = urlToTest,
          IsConnected = false,
          Error = "URL validation failed for security reasons",
          TestedAt = DateTime.UtcNow,
          ResponseTime = -1
        };
      }

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

      logger.LogInformation(
        "Seq connectivity test - URL: {Url}, Connected: {IsConnected}, ResponseTime: {ResponseTime}ms",
        urlToTest, isConnected, result.ResponseTime);

      return result;
    }
    catch (Exception ex)
    {
      logger.LogError(ex, "Error testing Seq connectivity to {Url}", urlToTest);
      
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
  public async Task<bool> TryConnectToSeqAsync(string? url, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrEmpty(url) || !Uri.IsWellFormedUriString(url, UriKind.Absolute))
      return false;

    try
    {
      using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      timeoutCts.CancelAfter(TimeSpan.FromSeconds(_config.SeqTimeoutSeconds));
      
      var response = await httpClient.GetAsync($"{url}/api/events/raw", timeoutCts.Token);
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
      logger.LogDebug(ex, "Failed to connect to Seq at {Url}", url);
      return false;
    }
  }

  /// <inheritdoc />
  public async Task<string?> GetBestAvailableSeqUrlAsync(CancellationToken cancellationToken = default)
  {
    // Try configured URL first (could be native or Docker)
    if (!string.IsNullOrEmpty(_config.SeqUrl) && await TryConnectToSeqAsync(_config.SeqUrl, cancellationToken))
    {
      logger.LogInformation("Using configured Seq at: {SeqUrl}", _config.SeqUrl);
      return _config.SeqUrl;
    }

    // Try common native Seq ports
    foreach (var url in _config.CommonSeqUrls)
    {
      if (await TryConnectToSeqAsync(url, cancellationToken))
      {
        logger.LogInformation("Found native Seq at: {SeqUrl}", url);
        return url;
      }
    }

    // Try to start Docker Seq as fallback
    if (_config.EnableDockerFallback && await TryStartDockerSeqAsync(cancellationToken))
    {
      logger.LogInformation("Attempting to start Docker Seq container...");
      
      // Wait for container startup with proper cancellation token propagation
      await Task.Delay(_config.DockerStartupDelayMs, cancellationToken);
      
      if (await TryConnectToSeqAsync("http://localhost:5341", cancellationToken))
      {
        logger.LogInformation("Successfully started Docker Seq container");
        return "http://localhost:5341";
      }
    }

    logger.LogWarning("No Seq instance found - will use file logging");
    return null;
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
  public async Task<bool> TryStartDockerSeqAsync(CancellationToken cancellationToken = default)
  {
    if (!_config.EnableDockerFallback)
    {
      logger.LogInformation("Docker fallback is disabled");
      return false;
    }

    try
    {
      // First check if a Seq container already exists
      if (await IsDockerSeqContainerRunningAsync(cancellationToken))
      {
        logger.LogInformation("Docker Seq container already running");
        return true;
      }

      // Try to start new container
      var arguments = $"run --name {_config.DockerContainerName} -d --restart unless-stopped -e ACCEPT_EULA=Y -p 5341:80 {_config.DockerImage}";
      var result = await processExecutor.RunCommandAsync("docker", arguments, _config.ProcessTimeoutMs, cancellationToken);

      if (result.exitCode == 0)
      {
        logger.LogInformation("Docker Seq container started successfully");
        return true;
      }

      logger.LogWarning("Failed to start Docker Seq container");
      return false;
    }
    catch (Exception ex)
    {
      logger.LogWarning(ex, "Error attempting to start Docker Seq container");
      return false;
    }
  }

  private async Task<bool> IsDockerSeqContainerRunningAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      var result = await processExecutor.RunCommandAsync(
        "docker", 
        "ps --filter name=seq --format {{.Names}}", 
        _config.ProcessTimeoutMs, 
        cancellationToken);
      return !string.IsNullOrWhiteSpace(result.output);
    }
    catch (Exception ex)
    {
      logger.LogDebug(ex, "Error checking Docker Seq container status");
      return false;
    }
  }
}