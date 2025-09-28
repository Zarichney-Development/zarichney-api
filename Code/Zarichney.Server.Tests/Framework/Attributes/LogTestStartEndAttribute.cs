using System.Reflection;
using Serilog;
using Xunit.Sdk;

namespace Zarichney.Tests.Framework.Attributes;

/// <summary>
/// Attribute that logs test method start and end information to aid in debugging test execution timing.
/// Applied to individual test methods to track execution timing.
/// </summary>
public class LogTestStartEndAttribute : BeforeAfterTestAttribute
{
  /// <summary>
  /// Logs test start information before the test method executes.
  /// </summary>
  /// <param name="methodUnderTest">The test method being executed</param>
  public override void Before(MethodInfo methodUnderTest)
  {
    var testName = $"{methodUnderTest.DeclaringType?.Name}.{methodUnderTest.Name}";
    Log.Information("=== START TEST: {TestName} ===", testName);
  }

  /// <summary>
  /// Logs test end information after the test method executes.
  /// </summary>
  /// <param name="methodUnderTest">The test method that was executed</param>
  public override void After(MethodInfo methodUnderTest)
  {
    var testName = $"{methodUnderTest.DeclaringType?.Name}.{methodUnderTest.Name}";
    Log.Information("=== END TEST: {TestName} ===", testName);
  }
}
