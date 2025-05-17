using Microsoft.Graph;

namespace Zarichney.Services.Status.Proxies;

/// <summary>
/// Proxy implementation of GraphServiceClient that throws ServiceUnavailableException
/// when any of its methods are called.
/// </summary>
internal class GraphServiceClientProxy(List<string>? reasons = null) : GraphServiceClient(_sharedHttpClient)
{
  private static readonly HttpClient _sharedHttpClient = new();

  // Use shared HttpClient instance

  // GraphServiceClient's methods may not be virtual, so we can't override them directly.
  // Instead, we'll intercept method calls by implementing service methods that our tests need.
  // In a test context, when Me, Users, or other properties are accessed, the exception will be thrown.

  // Method used by tests to verify proxy behavior
  public new object Me => throw CreateException();

  private ServiceUnavailableException CreateException()
  {
    return new ServiceUnavailableException(
      "Email service is unavailable due to missing configuration",
      reasons
    );
  }
}
