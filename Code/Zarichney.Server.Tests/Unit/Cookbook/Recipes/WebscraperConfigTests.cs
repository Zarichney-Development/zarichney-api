using FluentAssertions;
using Xunit;
using Zarichney.Cookbook.Recipes;

namespace Zarichney.Tests.Unit.Cookbook.Recipes;

/// <summary>
/// Unit tests for WebscraperConfig ensuring proper default values and configuration validation.
/// </summary>
[Trait("Category", "Unit")]
public class WebscraperConfigTests
{
  [Fact]
  public void WebscraperConfig_DefaultValues_AreCorrect()
  {
    // Arrange & Act
    var config = new WebscraperConfig();

    // Assert
    config.MaxNumResultsPerQuery.Should().Be(3, "because default max results per query should be 3");
    config.MaxParallelTasks.Should().Be(5, "because default max parallel tasks should be 5");
    config.MaxParallelSites.Should().Be(5, "because default max parallel sites should be 5");
    config.MaxWaitTimeMs.Should().Be(10000, "because default max wait time should be 10 seconds");
    config.MaxParallelPages.Should().Be(2, "because default max parallel pages should be 2");
    config.ErrorBuffer.Should().Be(5, "because default error buffer should be 5");
  }

  [Fact]
  public void WebscraperConfig_InitWithCustomValues_StoresCorrectly()
  {
    // Arrange
    const int customMaxResults = 10;
    const int customMaxTasks = 3;
    const int customMaxSites = 2;
    const int customWaitTime = 5000;
    const int customMaxPages = 1;
    const int customErrorBuffer = 3;

    // Act
    var config = new WebscraperConfig
    {
      MaxNumResultsPerQuery = customMaxResults,
      MaxParallelTasks = customMaxTasks,
      MaxParallelSites = customMaxSites,
      MaxWaitTimeMs = customWaitTime,
      MaxParallelPages = customMaxPages,
      ErrorBuffer = customErrorBuffer
    };

    // Assert
    config.MaxNumResultsPerQuery.Should().Be(customMaxResults, "because custom max results should be preserved");
    config.MaxParallelTasks.Should().Be(customMaxTasks, "because custom max tasks should be preserved");
    config.MaxParallelSites.Should().Be(customMaxSites, "because custom max sites should be preserved");
    config.MaxWaitTimeMs.Should().Be(customWaitTime, "because custom wait time should be preserved");
    config.MaxParallelPages.Should().Be(customMaxPages, "because custom max pages should be preserved");
    config.ErrorBuffer.Should().Be(customErrorBuffer, "because custom error buffer should be preserved");
  }

  [Theory]
  [InlineData(0)]
  [InlineData(-1)]
  [InlineData(-10)]
  public void WebscraperConfig_NegativeOrZeroValues_AreAllowed(int negativeValue)
  {
    // Arrange & Act
    var config = new WebscraperConfig
    {
      MaxNumResultsPerQuery = negativeValue,
      MaxParallelTasks = negativeValue,
      MaxParallelSites = negativeValue,
      MaxWaitTimeMs = negativeValue,
      MaxParallelPages = negativeValue,
      ErrorBuffer = negativeValue
    };

    // Assert
    // Configuration class doesn't enforce validation - it's a simple data container
    config.MaxNumResultsPerQuery.Should().Be(negativeValue, "because config allows negative values");
    config.MaxParallelTasks.Should().Be(negativeValue, "because config allows negative values");
    config.MaxParallelSites.Should().Be(negativeValue, "because config allows negative values");
    config.MaxWaitTimeMs.Should().Be(negativeValue, "because config allows negative values");
    config.MaxParallelPages.Should().Be(negativeValue, "because config allows negative values");
    config.ErrorBuffer.Should().Be(negativeValue, "because config allows negative values");
  }

  [Theory]
  [InlineData(1, 1, 1)]
  [InlineData(100, 50, 25)]
  [InlineData(1000, 500, 250)]
  public void WebscraperConfig_VariousPositiveValues_AreStoredCorrectly(
    int maxResults, int maxTasks, int maxSites)
  {
    // Arrange & Act
    var config = new WebscraperConfig
    {
      MaxNumResultsPerQuery = maxResults,
      MaxParallelTasks = maxTasks,
      MaxParallelSites = maxSites
    };

    // Assert
    config.MaxNumResultsPerQuery.Should().Be(maxResults, "because specified max results should be preserved");
    config.MaxParallelTasks.Should().Be(maxTasks, "because specified max tasks should be preserved");
    config.MaxParallelSites.Should().Be(maxSites, "because specified max sites should be preserved");
  }

  [Fact]
  public void WebscraperConfig_ImplementsIConfig_Interface()
  {
    // Arrange
    var config = new WebscraperConfig();

    // Assert
    config.Should().BeAssignableTo<Zarichney.Config.IConfig>("because WebscraperConfig should implement IConfig interface");
  }

  [Theory]
  [InlineData(500, 1000, 2000)] // Short timeouts
  [InlineData(30000, 60000, 120000)] // Longer timeouts
  public void WebscraperConfig_TimeoutValues_AreFlexible(int shortTimeout, int mediumTimeout, int longTimeout)
  {
    // Arrange & Act
    var shortConfig = new WebscraperConfig { MaxWaitTimeMs = shortTimeout };
    var mediumConfig = new WebscraperConfig { MaxWaitTimeMs = mediumTimeout };
    var longConfig = new WebscraperConfig { MaxWaitTimeMs = longTimeout };

    // Assert
    shortConfig.MaxWaitTimeMs.Should().Be(shortTimeout, "because short timeout should be configurable");
    mediumConfig.MaxWaitTimeMs.Should().Be(mediumTimeout, "because medium timeout should be configurable");
    longConfig.MaxWaitTimeMs.Should().Be(longTimeout, "because long timeout should be configurable");
  }

  [Fact]
  public void WebscraperConfig_ErrorBuffer_HandlesVariousScenarios()
  {
    // Arrange & Act
    var noBufferConfig = new WebscraperConfig { ErrorBuffer = 0 };
    var smallBufferConfig = new WebscraperConfig { ErrorBuffer = 2 };
    var largeBufferConfig = new WebscraperConfig { ErrorBuffer = 20 };

    // Assert
    noBufferConfig.ErrorBuffer.Should().Be(0, "because no error buffer should be allowed");
    smallBufferConfig.ErrorBuffer.Should().Be(2, "because small error buffer should be preserved");
    largeBufferConfig.ErrorBuffer.Should().Be(20, "because large error buffer should be preserved");
  }

  [Theory]
  [InlineData(1)]
  [InlineData(5)]
  [InlineData(10)]
  [InlineData(50)]
  public void WebscraperConfig_ParallelismSettings_SupportVariousScales(int parallelismLevel)
  {
    // Arrange & Act
    var config = new WebscraperConfig
    {
      MaxParallelTasks = parallelismLevel,
      MaxParallelSites = parallelismLevel,
      MaxParallelPages = parallelismLevel
    };

    // Assert
    config.MaxParallelTasks.Should().Be(parallelismLevel, "because parallel tasks should scale appropriately");
    config.MaxParallelSites.Should().Be(parallelismLevel, "because parallel sites should scale appropriately");
    config.MaxParallelPages.Should().Be(parallelismLevel, "because parallel pages should scale appropriately");
  }
}
