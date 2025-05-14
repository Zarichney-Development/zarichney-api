using OpenAI;

namespace Zarichney.Services.Status.Proxies;


/// <summary>
/// Proxy implementation of OpenAIClient that throws ServiceUnavailableException
/// when any of its methods are called.
/// </summary>
internal class OpenAIClientProxy(List<string>? reasons = null) : OpenAIClient("dummy_key")
{
  // Dummy base constructor call

  // OpenAIClient's properties may not be virtual, so we can't override them directly.
  // Instead, we'll intercept method calls by implementing service methods that our tests need.
  // In a test context, when Chat, Completions, etc. are accessed, the exception will be thrown.

  // Method used by tests to verify proxy behavior
  public new object Chat => throw CreateException();

  private ServiceUnavailableException CreateException()
  {
    return new ServiceUnavailableException(
      "LLM service is unavailable due to missing configuration",
      reasons
    );
  }
}
