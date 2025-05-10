using System;
using Xunit;

namespace Zarichney.Tests.Framework.Attributes;

/// <summary>
/// Skip attribute specifically for Swagger integration tests which are currently not stable.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class SkipSwaggerIntegrationFactAttribute : FactAttribute
{
  private const string SkipReason = "Swagger integration tests are skipped because they are currently not stable";

  public SkipSwaggerIntegrationFactAttribute()
  {
    Skip = SkipReason;
  }
}
