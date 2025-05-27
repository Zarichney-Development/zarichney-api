using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;
using Zarichney.Services.Status;
using Zarichney.TestingFramework.Fixtures;

namespace Zarichney.TestingFramework.Helpers;

/// <summary>
/// A test case that skips tests when the specified external service is AVAILABLE.
/// This test case runs only when the specified service is UNAVAILABLE, enabling testing
/// of HTTP 503 responses from dependency-aware API endpoints.
/// </summary>
public class ServiceUnavailableTestCase : XunitTestCase
{
  private ExternalServices RequiredUnavailableService { get; set; }

  [Obsolete("Called by the de-serializer", true)]
  public ServiceUnavailableTestCase()
  {
    RequiredUnavailableService = default;
  }

  public ServiceUnavailableTestCase(
      IMessageSink diagnosticMessageSink,
      TestMethodDisplay defaultMethodDisplay,
      TestMethodDisplayOptions defaultMethodDisplayOptions,
      ITestMethod testMethod,
      ExternalServices requiredUnavailableService)
      : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
  {
    RequiredUnavailableService = requiredUnavailableService;
  }

  public override void Serialize(IXunitSerializationInfo data)
  {
    base.Serialize(data);
    data.AddValue("RequiredUnavailableService", (int)RequiredUnavailableService);
  }

  public override void Deserialize(IXunitSerializationInfo data)
  {
    base.Deserialize(data);
    RequiredUnavailableService = (ExternalServices)data.GetValue<int>("RequiredUnavailableService");
  }

  protected override string GetDisplayName(IAttributeInfo factAttribute, string displayName)
  {
    return $"{TestMethod.TestClass.Class.Name}.{TestMethod.Method.Name}";
  }

  public override async Task<RunSummary> RunAsync(
      IMessageSink diagnosticMessageSink,
      IMessageBus messageBus,
      object[] constructorArguments,
      ExceptionAggregator aggregator,
      CancellationTokenSource cancellationTokenSource)
  {
    try
    {
      // We need to get access to the CustomWebApplicationFactory to check service availability
      // The factory should be available through the constructor arguments (ApiClientFixture)
      var apiClientFixture = constructorArguments.OfType<ApiClientFixture>().FirstOrDefault();

      if (apiClientFixture?.Factory == null)
      {
        // If we can't get the factory, skip the test with an explanation
        return await SkipTest(messageBus, cancellationTokenSource,
          "Unable to access CustomWebApplicationFactory for service availability check");
      }

      // Get the IStatusService from the factory's service provider
      IStatusService statusService;
      try
      {
        using var scope = apiClientFixture.Factory.Services.CreateScope();
        statusService = scope.ServiceProvider.GetRequiredService<IStatusService>();
      }
      catch (Exception ex)
      {
        return await SkipTest(messageBus, cancellationTokenSource,
          $"Unable to resolve IStatusService: {ex.Message}");
      }

      // Check if the service is available
      var isServiceAvailable = statusService.IsFeatureAvailable(RequiredUnavailableService);

      if (isServiceAvailable)
      {
        // The service is available, so skip this test (it should only run when unavailable)
        return await SkipTest(messageBus, cancellationTokenSource,
          $"Skipping test because '{RequiredUnavailableService}' is AVAILABLE. This test runs only when the service is unavailable.");
      }

      // The service is unavailable, so run the test normally
      return await base.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator, cancellationTokenSource);
    }
    catch (Exception ex)
    {
      // If we get an exception at the test case level, convert it to a skipped test
      return await SkipTest(messageBus, cancellationTokenSource,
        $"Test setup failed during service availability check: {ex.Message}");
    }
  }

  /// <summary>
  /// Helper method to skip a test with the specified reason.
  /// </summary>
  private async Task<RunSummary> SkipTest(IMessageBus messageBus, CancellationTokenSource cancellationTokenSource, string skipReason)
  {
    var test = new XunitTest(this, DisplayName);

    // Send test starting message
    if (!messageBus.QueueMessage(new TestStarting(test)))
      await cancellationTokenSource.CancelAsync();

    // Send skip message
    if (!messageBus.QueueMessage(new TestSkipped(test, skipReason)))
      await cancellationTokenSource.CancelAsync();

    // Send test finished message
    if (!messageBus.QueueMessage(new TestFinished(test, 0, test.TestCase.TestMethod.Method.ToString())))
      await cancellationTokenSource.CancelAsync();

    return new RunSummary { Total = 1, Skipped = 1 };
  }
}
