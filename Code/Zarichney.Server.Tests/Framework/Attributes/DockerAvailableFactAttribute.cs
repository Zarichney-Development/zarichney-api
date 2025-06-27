using System.Diagnostics;
using Xunit;

namespace Zarichney.Tests.Framework.Attributes
{
  /// <summary>
  /// A Fact attribute that skips the test when Docker is not available or misconfigured.
  /// </summary>
  public sealed class DockerAvailableFactAttribute : FactAttribute
  {
    public DockerAvailableFactAttribute()
    {
      if (!IsDockerAvailable())
      {
        Skip = "Docker is not running or misconfigured, skipping test.";
      }
    }

    private bool IsDockerAvailable()
    {
      try
      {
        var psi = new ProcessStartInfo("docker", "info")
        {
          RedirectStandardOutput = true,
          RedirectStandardError = true,
          UseShellExecute = false,
          CreateNoWindow = true
        };
        using var proc = Process.Start(psi);
        if (proc == null) return false;

        // Set a shorter timeout to avoid long waits
        if (!proc.WaitForExit(1000))
        {
          try
          {
            proc.Kill();
          }
          catch
          {
            // Ignore errors when killing the process
          }
          return false;
        }

        return proc.ExitCode == 0;
      }
      catch
      {
        return false;
      }
    }
  }
}
