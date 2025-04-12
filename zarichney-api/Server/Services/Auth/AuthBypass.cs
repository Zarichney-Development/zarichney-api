namespace Zarichney.Server.Services.Auth;

public static class MiddlewareConfiguration
{
  public static class Routes
  {
    // Explicitly define all bypass paths
    private static readonly HashSet<string> ExactBypassPaths = new(StringComparer.OrdinalIgnoreCase)
    {
      "/api/health",
      "/api/key/validate"
    };

    private static readonly HashSet<string> PrefixBypassPaths = new(StringComparer.OrdinalIgnoreCase)
    {
      "/api/swagger",
      "/api/auth"
    };

    public static bool ShouldBypass(string path)
    {
      return ExactBypassPaths.Contains(path) ||
             PrefixBypassPaths.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
    }
  }
}