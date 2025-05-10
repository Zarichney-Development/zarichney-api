namespace Zarichney.Config;

public class ServerConfig : IConfig
{
  [RequiresConfiguration("ServerConfig:BaseUrl")]
  public string BaseUrl { get; init; } = string.Empty;
}
public class ClientConfig : IConfig
{
  [RequiresConfiguration("ClientConfig:BaseUrl")]
  public string BaseUrl { get; init; } = string.Empty;
}
