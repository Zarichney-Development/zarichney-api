using Zarichney.Cookbook.Recipes;

namespace Zarichney.Tests.TestData.Builders;

/// <summary>
/// Builder for creating WebscraperConfig instances for testing
/// </summary>
public class WebscraperConfigBuilder : BaseBuilder<WebscraperConfigBuilder, WebscraperConfig>
{
  public WebscraperConfigBuilder()
  {
    Entity.MaxParallelPages = 5;
    Entity.MaxWaitTimeMs = 10000;
  }

  public WebscraperConfigBuilder WithMaxParallelPages(int maxParallelPages)
  {
    Entity.MaxParallelPages = maxParallelPages;
    return Self();
  }

  public WebscraperConfigBuilder WithMaxWaitTimeMs(int maxWaitTimeMs)
  {
    Entity.MaxWaitTimeMs = maxWaitTimeMs;
    return Self();
  }

  public WebscraperConfigBuilder WithDefaults()
  {
    Entity.MaxParallelPages = 5;
    Entity.MaxWaitTimeMs = 10000;
    return Self();
  }

  public WebscraperConfigBuilder WithMinimalTimeout()
  {
    Entity.MaxWaitTimeMs = 100;
    return Self();
  }

  public WebscraperConfigBuilder WithHighConcurrency()
  {
    Entity.MaxParallelPages = 10;
    return Self();
  }

  public WebscraperConfigBuilder WithLowConcurrency()
  {
    Entity.MaxParallelPages = 1;
    return Self();
  }
}