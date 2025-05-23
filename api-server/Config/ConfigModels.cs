using Zarichney.Services.Status;

namespace Zarichney.Config;

public class ServerConfig : IConfig
{
  [RequiresConfiguration(ExternalServices.FrontEnd)]
  public string BaseUrl { get; init; } = string.Empty;
}
public class ClientConfig : IConfig
{
  [RequiresConfiguration(ExternalServices.FrontEnd)]
  public string BaseUrl { get; init; } = string.Empty;
}

/// <summary>
/// Configuration for mock authentication system used in non-Production environments
/// when the Identity Database is unavailable
/// </summary>
public class MockAuthConfig : IConfig
{
  /// <summary>
  /// Default roles to assign to the mock user
  /// </summary>
  public List<string> DefaultRoles { get; init; } = ["User"];

  /// <summary>
  /// Default username for the mock user
  /// </summary>
  public string DefaultUsername { get; init; } = "MockUser";

  /// <summary>
  /// Default email for the mock user
  /// </summary>
  public string DefaultEmail { get; init; } = "mock@example.com";

  /// <summary>
  /// Default user ID for the mock user
  /// </summary>
  public string DefaultUserId { get; init; } = "mock-user-id";
}
