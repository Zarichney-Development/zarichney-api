using System.Reflection;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace Zarichney.Tests.Helpers;

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

    public override async Task<RunSummary> RunAsync(
        IMessageSink diagnosticMessageSink,
        IMessageBus messageBus,
        object[] constructorArguments,
        ExceptionAggregator aggregator,
        CancellationTokenSource cancellationTokenSource)
    {
        // Create the test class instance
        var testClass = TestMethod.TestClass.Class.ToRuntimeType();
        var testClassInstance = Activator.CreateInstance(testClass, constructorArguments);
        
        // Check if the test class has a ShouldSkip property
        var shouldSkipProperty = testClass.GetProperty("ShouldSkip", 
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        
        var skipReasonProperty = testClass.GetProperty("SkipReason",
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        
        var shouldSkip = false;
        string? skipReason = null!;
        
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
            
            // Skip the test
            var test = new XunitTest(this, DisplayName);
            if (!messageBus.QueueMessage(new TestSkipped(test, skipReason)))
            {
                cancellationTokenSource.Cancel();
            }
            
            return new RunSummary
            {
                Total = 1,
                Skipped = 1
            };
        }
        
        // Run the test normally
        return await base.RunAsync(
            diagnosticMessageSink, 
            messageBus, 
            constructorArguments, 
            aggregator, 
            cancellationTokenSource);
    }
}