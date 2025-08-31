namespace Zarichney.Services.Security;

/// <summary>
/// Result of URL validation for SSRF prevention
/// </summary>
public class UrlValidationResult
{
  /// <summary>
  /// Whether the URL is valid and safe to use
  /// </summary>
  public bool IsValid { get; set; }

  /// <summary>
  /// Reason code for validation failure
  /// </summary>
  public string? ReasonCode { get; set; }

  /// <summary>
  /// Sanitized host for logging (without query parameters or credentials)
  /// </summary>
  public string? SanitizedHost { get; set; }

  /// <summary>
  /// Creates a successful validation result
  /// </summary>
  public static UrlValidationResult Success(string sanitizedHost) => new()
  {
    IsValid = true,
    SanitizedHost = sanitizedHost
  };

  /// <summary>
  /// Creates a failed validation result
  /// </summary>
  public static UrlValidationResult Failure(string reasonCode, string sanitizedHost) => new()
  {
    IsValid = false,
    ReasonCode = reasonCode,
    SanitizedHost = sanitizedHost
  };
}

/// <summary>
/// Standard reason codes for URL validation failures
/// </summary>
public static class ValidationReasonCodes
{
  public const string InvalidScheme = "INVALID_SCHEME";
  public const string CredentialsPresent = "CREDENTIALS_PRESENT";
  public const string HostNotAllowed = "HOST_NOT_ALLOWED";
  public const string PrivateIpAddress = "PRIVATE_IP_ADDRESS";
  public const string DnsResolutionFailed = "DNS_RESOLUTION_FAILED";
  public const string DnsResolvedToPrivateIp = "DNS_RESOLVED_TO_PRIVATE_IP";
  public const string ExcessiveRedirects = "EXCESSIVE_REDIRECTS";
  public const string ProtocolDowngrade = "PROTOCOL_DOWNGRADE";
}

/// <summary>
/// Event IDs for structured security logging
/// </summary>
public static class SecurityEventIds
{
  public const int UrlValidationSuccess = 5700;
  public const int UrlValidationBlocked = 5701;
  public const int RedirectLimitExceeded = 5702;
  public const int DnsResolutionError = 5703;
}