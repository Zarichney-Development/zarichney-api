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
        // Check for pre-cancelled token to ensure immediate cancellation behavior
        cancellationToken.ThrowIfCancellationRequested();

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
                // Start reading stdout and stderr asynchronously and in parallel
                var outputTask = process.StandardOutput.ReadToEndAsync();
                var errorTask = process.StandardError.ReadToEndAsync();

                using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeoutCts.CancelAfter(timeoutMs);

                // Wait for process exit and both output tasks
                await process.WaitForExitAsync(timeoutCts.Token).ConfigureAwait(false);
                var output = await outputTask.ConfigureAwait(false);
                var error = await errorTask.ConfigureAwait(false);

                // Log error output if present (for debugging)
                if (!string.IsNullOrWhiteSpace(error))
                {
                    logger.LogDebug("Command stderr output: {Error}", error);
                }

                return (process.ExitCode, output);
            }

            return (-1, "");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Rethrow cancellation exceptions when the original token was cancelled
            throw;
        }
        catch (OperationCanceledException ex)
        {
            // Timeout from linked token source
            logger.LogDebug(ex, "Command execution timed out: {Command} {Arguments}", command, arguments);
            return (-1, "");
        }
        catch (Exception ex)
        {
            logger.LogDebug(ex, "Error running command: {Command} {Arguments}", command, arguments);
            return (-1, "");
        }
    }
}
