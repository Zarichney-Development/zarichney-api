namespace Zarichney.Services.Auth;

public static class MiddlewareConfiguration
{
  public static class Routes
  {
    private static readonly HashSet<string> PrefixBypassPaths = new(StringComparer.OrdinalIgnoreCase)
    {
      "/api/swagger",
      "/api/auth"
    };

    public static bool ShouldBypass(string path)
      => PrefixBypassPaths.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase));
  }
}
