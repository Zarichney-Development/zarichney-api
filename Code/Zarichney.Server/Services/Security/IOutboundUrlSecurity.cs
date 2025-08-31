namespace Zarichney.Services.Security;

/// <summary>
/// Service for validating outbound URLs to prevent SSRF attacks
/// </summary>
public interface IOutboundUrlSecurity
{
  /// <summary>
  /// Validates a URL for Seq connectivity testing
  /// </summary>
  /// <param name="url">The URL to validate</param>
  /// <param name="cancellationToken">Cancellation token</param>
  /// <returns>Validation result indicating if the URL is safe to use</returns>
  Task<UrlValidationResult> ValidateSeqUrlAsync(string? url, CancellationToken cancellationToken = default);

  /// <summary>
  /// Gets the list of effective allowed hosts (including defaults if enabled)
  /// </summary>
  /// <returns>List of allowed hosts</returns>
  IReadOnlyList<string> GetEffectiveAllowedHosts();
}