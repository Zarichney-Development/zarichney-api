using System.Reflection;
using Xunit;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Zarichney.Server.Tests.Framework.Helpers;

/// <summary>
/// A test case decorator that skips tests when the skip condition is met.
/// </summary>
public class SkipMissingDependencyTestCase : XunitTestCase
{
  [Obsolete("Called by the de-serializer", true)]
  public SkipMissingDependencyTestCase() { }

  public SkipMissingDependencyTestCase(
      IMessageSink diagnosticMessageSink,
      TestMethodDisplay defaultMethodDisplay,
      TestMethodDisplayOptions defaultMethodDisplayOptions,
      ITestMethod testMethod)
      : base(diagnosticMessageSink, defaultMethodDisplay, defaultMethodDisplayOptions, testMethod)
  {
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
      // Create the test class instance
      var testClass = TestMethod.TestClass.Class.ToRuntimeType();
      var testClassInstance = Activator.CreateInstance(testClass, constructorArguments);

      // If the test class has asynchronous initialization, invoke it to set up SkipReason
      if (testClassInstance is IAsyncLifetime asyncInit)
      {
        try
        {
          await asyncInit.InitializeAsync();
        }
        catch (Exception ex)
        {
          // If initialization fails, assume it's due to missing dependencies
          var test = new XunitTest(this, DisplayName);

          if (!messageBus.QueueMessage(new TestStarting(test)))
            await cancellationTokenSource.CancelAsync();

          if (!messageBus.QueueMessage(new TestSkipped(test,
                $"Test initialization failed, likely due to missing dependencies: {ex.Message}")))
            await cancellationTokenSource.CancelAsync();

          if (!messageBus.QueueMessage(new TestFinished(test, 0, test.TestCase.TestMethod.Method.ToString())))
            await cancellationTokenSource.CancelAsync();

          return new RunSummary { Total = 1, Skipped = 1 };
        }
      }

      // Check if the test class has a ShouldSkip property
      var shouldSkipProperty = testClass.GetProperty("ShouldSkip",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

      var skipReasonProperty = testClass.GetProperty("SkipReason",
        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

      var shouldSkip = false;
      string? skipReason = null;

      if (shouldSkipProperty != null)
      {
        shouldSkip = (bool)(shouldSkipProperty.GetValue(testClassInstance) ?? false);

        if (shouldSkip && skipReasonProperty != null)
        {
          skipReason = (string?)skipReasonProperty.GetValue(testClassInstance);
        }
      }

      if (shouldSkip)
      {
        skipReason ??= "Test dependencies missing.";

        // Create test with our properly formatted display name
        var test = new XunitTest(this, DisplayName);

        // Send test starting message
        if (!messageBus.QueueMessage(new TestStarting(test)))
          await cancellationTokenSource.CancelAsync();

        // Send skip message
        if (!messageBus.QueueMessage(new TestSkipped(test, skipReason)))
          await cancellationTokenSource.CancelAsync();

        // Send test finished message with the correct parameter order
        if (!messageBus.QueueMessage(new TestFinished(test, 0, test.TestCase.TestMethod.Method.ToString())))
          await cancellationTokenSource.CancelAsync();

        return new RunSummary { Total = 1, Skipped = 1 };
      }

      // Run the test normally if not skipped during class init
      return await base.RunAsync(diagnosticMessageSink, messageBus, constructorArguments, aggregator,
        cancellationTokenSource);
    }
    catch (Exception ex)
    {
      // If we get an exception at the test case level, convert it to a skipped test
      var test = new XunitTest(this, DisplayName);

      if (!messageBus.QueueMessage(new TestStarting(test)))
        await cancellationTokenSource.CancelAsync();

      if (!messageBus.QueueMessage(new TestSkipped(test, $"Test setup failed, likely due to missing dependencies: {ex.Message}")))
        await cancellationTokenSource.CancelAsync();

      if (!messageBus.QueueMessage(new TestFinished(test, 0, test.TestCase.TestMethod.Method.ToString())))
        await cancellationTokenSource.CancelAsync();

      return new RunSummary { Total = 1, Skipped = 1 };
    }
  }
}
