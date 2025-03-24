namespace Zarichney.Server.Config;

public class ServerConfig : IConfig
{
  public string BaseUrl { get; init; } = string.Empty;
}
public class ClientConfig : IConfig
{
  public string BaseUrl { get; init; } = string.Empty;
}