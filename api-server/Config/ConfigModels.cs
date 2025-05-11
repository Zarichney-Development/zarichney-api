namespace Zarichney.Config;

public class ServerConfig : IConfig
{
  [RequiresConfiguration(Feature.Core)]
  public string BaseUrl { get; init; } = string.Empty;
}
public class ClientConfig : IConfig
{
  [RequiresConfiguration(Feature.Core)]
  public string BaseUrl { get; init; } = string.Empty;
}
