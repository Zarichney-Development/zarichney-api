using System.Diagnostics;

namespace Zarichney.Services.ProcessExecution;

/// <summary>
/// Service for executing system processes and commands
/// </summary>
public class ProcessExecutor(ILogger<ProcessExecutor> logger) : IProcessExecutor
{
  /// <inheritdoc />
  public async Task<(int exitCode, string output)> RunCommandAsync(
    string command, 
    string arguments, 
    int timeoutMs = 5000,
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
        timeoutCts.CancelAfter(timeoutMs);
        
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
      logger.LogDebug("Command execution timed out: {Command} {Arguments}", command, arguments);
      return (-1, "");
    }
    catch (Exception ex)
    {
      logger.LogDebug(ex, "Error running command: {Command} {Arguments}", command, arguments);
      return (-1, "");
    }
  }
}