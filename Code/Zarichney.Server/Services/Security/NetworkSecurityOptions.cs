namespace Zarichney.Services.Security;

/// <summary>
/// Configuration options for network security and SSRF prevention
/// </summary>
public class NetworkSecurityOptions
{
  /// <summary>
  /// List of explicitly allowed hosts for Seq connectivity tests
  /// </summary>
  public List<string> AllowedSeqHosts { get; set; } = new();

  /// <summary>
  /// Whether to enable default localhost entries in the allowlist
  /// </summary>
  public bool EnableDefaultLocalhost { get; set; } = true;

  /// <summary>
  /// Maximum number of HTTP redirects to follow
  /// </summary>
  public int MaxRedirects { get; set; } = 3;

  /// <summary>
  /// Whether to perform DNS resolution validation to check for private IPs
  /// </summary>
  public bool EnableDnsResolutionValidation { get; set; } = true;

  /// <summary>
  /// Timeout for DNS resolution validation in seconds
  /// </summary>
  public int DnsResolutionTimeoutSeconds { get; set; } = 2;
}