namespace Zarichney.Services.ProcessExecution;

/// <summary>
/// Service interface for executing system processes and commands
/// </summary>
public interface IProcessExecutor
{
  /// <summary>
  /// Executes a system command asynchronously with timeout support
  /// </summary>
  /// <param name="command">The command to execute (e.g., "docker", "systemctl")</param>
  /// <param name="arguments">The arguments to pass to the command</param>
  /// <param name="timeoutMs">Timeout in milliseconds for command execution</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>A tuple containing the exit code and output of the command</returns>
  Task<(int exitCode, string output)> RunCommandAsync(
    string command, 
    string arguments, 
    int timeoutMs = 5000,
    CancellationToken cancellationToken = default);
}