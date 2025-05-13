using Zarichney.Services.Status;

namespace Zarichney.Config;

public class ServerConfig : IConfig
{
  [RequiresConfiguration(ApiFeature.Core)]
  public string BaseUrl { get; init; } = string.Empty;
}
public class ClientConfig : IConfig
{
  [RequiresConfiguration(ApiFeature.Core)]
  public string BaseUrl { get; init; } = string.Empty;
}
