using System.Diagnostics;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Server.Tests.Framework.Attributes;
using Zarichney.Server.Tests.Framework.Fixtures;

namespace Zarichney.Server.Tests.Integration.Performance;

/// <summary>
/// Performance tests for the API.
/// </summary>
[Collection("IntegrationQA")]
[Trait(TestCategories.Category, TestCategories.Integration)]
[Trait(TestCategories.Category, TestCategories.Performance)]
[Trait(TestCategories.Feature, TestCategories.Cookbook)]
[Trait(TestCategories.Dependency, TestCategories.Database)]
[Trait(TestCategories.Dependency, TestCategories.Docker)]
public class ApiPerformanceTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper) : IntegrationTestBase(apiClientFixture, testOutputHelper)
{
  [DependencyFact(InfrastructureDependency.Database)]
  public async Task GetRecipes_Performance_CompletesWithinTimeLimit()
  {

    // Arrange
    var client = _apiClientFixture.AuthenticatedCookbookApi;

    // Set an acceptable performance threshold
    const int maxAcceptableMilliseconds = 500;
    var stopwatch = new Stopwatch();

    // Act
    stopwatch.Start();
    var recipes = await client.Recipe("burger", false, default(int?), default(int?));
    stopwatch.Stop();

    // Assert
    Assert.NotNull(recipes);
    Assert.True(stopwatch.ElapsedMilliseconds <= maxAcceptableMilliseconds,
      $"API call took {stopwatch.ElapsedMilliseconds}ms, which exceeds the acceptable threshold of {maxAcceptableMilliseconds}ms");
  }

  [DependencyFact(InfrastructureDependency.Database)]
  public async Task MultipleRequests_Performance_MaintainsConsistentResponseTime()
  {

    // Arrange
    var client = _apiClientFixture.AuthenticatedCookbookApi;

    const int requestCount = 10;
    var responseTimes = new List<long>(requestCount);

    // Act - Send multiple requests and record response times
    for (int i = 0; i < requestCount; i++)
    {
      var stopwatch = Stopwatch.StartNew();
      var recipes = await client.Recipe("burger", false, default(int?), default(int?));
      stopwatch.Stop();

      Assert.NotNull(recipes);
      responseTimes.Add(stopwatch.ElapsedMilliseconds);
    }

    // Assert - Check for consistency in response times
    var averageTime = responseTimes.Average();
    var maxTime = responseTimes.Max();
    var minTime = responseTimes.Min();
    var standardDeviation = CalculateStandardDeviation(responseTimes);

    // Validate performance metrics
    Assert.True(maxTime <= 1000, $"Maximum response time {maxTime}ms exceeds acceptable threshold of 1000ms");
    Assert.True(standardDeviation <= 100,
      $"Response time standard deviation {standardDeviation} exceeds acceptable threshold of 100ms");

    // Output performance statistics
    OutputPerformanceStatistics(responseTimes, averageTime, maxTime, minTime, standardDeviation);
  }

  // Helper method to calculate standard deviation
  private double CalculateStandardDeviation(List<long> values)
  {
    var avg = values.Average();
    var sum = values.Sum(d => Math.Pow(d - avg, 2));
    return Math.Sqrt(sum / values.Count);
  }

  // Helper method to output performance statistics
  private void OutputPerformanceStatistics(List<long> responseTimes, double averageTime, long maxTime, long minTime,
    double standardDeviation)
  {
    // In a real test, this would write to a log file or report
    Console.WriteLine($"Performance Statistics:");
    Console.WriteLine($"Requests: {responseTimes.Count}");
    Console.WriteLine($"Average Time: {averageTime:F2}ms");
    Console.WriteLine($"Maximum Time: {maxTime}ms");
    Console.WriteLine($"Minimum Time: {minTime}ms");
    Console.WriteLine($"Standard Deviation: {standardDeviation:F2}ms");
  }
}
