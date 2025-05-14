using Zarichney.Services.Email;

namespace Zarichney.Services.Status.Proxies;

/// <summary>
/// Proxy implementation of IMailCheckClient that throws ServiceUnavailableException
/// when any of its methods are called.
/// </summary>
internal class MailCheckClientProxy(List<string>? reasons = null) : IMailCheckClient
{
  /// <summary>
  /// Throws ServiceUnavailableException when called, indicating the MailCheck API is unavailable
  /// </summary>
  /// <param name="domain">The domain to validate</param>
  /// <returns>Never returns, always throws an exception</returns>
  public Task<EmailValidationResponse> GetValidationData(string domain)
  {
    throw new ServiceUnavailableException(
      "Email validation service is unavailable due to missing configuration",
      reasons
    );
  }
}