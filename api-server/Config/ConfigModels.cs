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
