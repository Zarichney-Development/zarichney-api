namespace Zarichney.Config;

/// <summary>
/// Simple class to deserialize the Swagger JSON output for testing.
/// </summary>
public class SwaggerDocument
{
  public string? OpenApi { get; set; }
  public SwaggerInfo? Info { get; set; }
  public Dictionary<string, SwaggerPathItem> Paths { get; set; } = new();
}

public class SwaggerInfo
{
  public string? Title { get; set; }
  public string? Version { get; set; }
  public string? Description { get; set; }
}

public class SwaggerPathItem
{
  public Dictionary<string, SwaggerOperation> Operations { get; set; } = new();

  [System.Text.Json.Serialization.JsonPropertyName("get")]
  public SwaggerOperation? Get
  {
    get => Operations.TryGetValue("get", out var op) ? op : null;
    set { if (value != null) Operations["get"] = value; }
  }

  [System.Text.Json.Serialization.JsonPropertyName("post")]
  public SwaggerOperation? Post
  {
    get => Operations.TryGetValue("post", out var op) ? op : null;
    set { if (value != null) Operations["post"] = value; }
  }

  [System.Text.Json.Serialization.JsonPropertyName("put")]
  public SwaggerOperation? Put
  {
    get => Operations.TryGetValue("put", out var op) ? op : null;
    set { if (value != null) Operations["put"] = value; }
  }

  [System.Text.Json.Serialization.JsonPropertyName("delete")]
  public SwaggerOperation? Delete
  {
    get => Operations.TryGetValue("delete", out var op) ? op : null;
    set { if (value != null) Operations["delete"] = value; }
  }
}

public class SwaggerOperation
{
  public string? Summary { get; set; }
  public string? Description { get; set; }
  public object? Parameters { get; set; }
  public Dictionary<string, object>? Responses { get; set; }
}
