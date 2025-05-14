using Microsoft.Extensions.Caching.Memory;
using RestSharp;
using Zarichney.Config;
using Zarichney.Services.Status;

namespace Zarichney.Services.Email;

/// <summary>
/// Interface for interacting with the MailCheck API for email validation
/// </summary>
public interface IMailCheckClient
{
  /// <summary>
  /// Gets email validation data from the MailCheck API
  /// </summary>
  /// <param name="domain">The domain portion of an email address to validate</param>
  /// <returns>Email validation response from the API</returns>
  /// <exception cref="InvalidOperationException">Thrown when the API returns unexpected results</exception>
  /// <exception cref="ConfigurationMissingException">Thrown when the MailCheck API key is missing or invalid</exception>
  Task<EmailValidationResponse> GetValidationData(string domain);
}

/// <summary>
/// Client for interacting with the RapidAPI MailCheck service
/// </summary>
public class MailCheckClient(
  EmailConfig config,
  IMemoryCache cache,
  ILogger<MailCheckClient> logger) : IMailCheckClient
{
  private const string ApiBaseUrl = "https://mailcheck.p.rapidapi.com";
  private const string ApiHost = "mailcheck.p.rapidapi.com";

  /// <summary>
  /// Gets email validation data from the MailCheck API and caches the result
  /// </summary>
  /// <param name="domain">The domain to validate</param>
  /// <returns>The validation response from the API</returns>
  /// <exception cref="InvalidOperationException">Thrown when the API returns unexpected results</exception>
  /// <exception cref="ConfigurationMissingException">Thrown when the MailCheck API key is missing or invalid</exception>
  public async Task<EmailValidationResponse> GetValidationData(string domain)
  {
    if (string.IsNullOrEmpty(config.MailCheckApiKey) ||
        config.MailCheckApiKey == StatusService.PlaceholderMessage)
    {
      throw new ConfigurationMissingException(nameof(EmailConfig), nameof(config.MailCheckApiKey));
    }

    // Check cache first
    if (cache.TryGetValue(domain, out EmailValidationResponse? cachedResult))
    {
      return cachedResult!;
    }

    logger.LogInformation("Validating domain {Domain} via MailCheck API", domain);

    using var client = new RestClient(ApiBaseUrl);
    var request = new RestRequest($"/?domain={domain}");
    request.AddHeader("x-rapidapi-host", ApiHost);
    request.AddHeader("x-rapidapi-key", config.MailCheckApiKey);
    var response = await client.ExecuteAsync(request);

    if (response.StatusCode != System.Net.HttpStatusCode.OK)
    {
      logger.LogError("Email validation service returned non-success status code: {StatusCode}", response.StatusCode);
      throw new InvalidOperationException("Email validation service error");
    }

    var result = Utils.Deserialize<EmailValidationResponse>(response.Content!);

    if (result == null)
    {
      logger.LogError("Failed to deserialize email validation response");
      throw new InvalidOperationException("Email validation response deserialization error");
    }

    // Cache the result
    cache.Set(domain, result, TimeSpan.FromHours(24));

    return result;
  }
}