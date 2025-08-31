using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;

namespace Zarichney.Services.Security;

/// <summary>
/// Service for validating outbound URLs to prevent SSRF attacks
/// </summary>
public class OutboundUrlSecurity : IOutboundUrlSecurity
{
  private readonly NetworkSecurityOptions _options;
  private readonly ILogger<OutboundUrlSecurity> _logger;
  private readonly HttpClient _httpClient;

  // Private IP ranges for SSRF protection
  private static readonly string[] PrivateIpRanges = {
    "10.0.0.0/8",      // Class A private range
    "172.16.0.0/12",   // Class B private range  
    "192.168.0.0/16",  // Class C private range
    "169.254.0.0/16",  // Link-local range
    "127.0.0.0/8",     // Loopback range (will be handled separately for localhost allowlist)
    "::1/128",         // IPv6 loopback
    "fc00::/7",        // IPv6 private range
    "fe80::/10"        // IPv6 link-local
  };

  private static readonly string[] DefaultLocalhostHosts = {
    "localhost",
    "127.0.0.1",
    "::1"
  };

  public OutboundUrlSecurity(
    IOptions<NetworkSecurityOptions> options,
    ILogger<OutboundUrlSecurity> logger,
    HttpClient httpClient)
  {
    _options = options.Value;
    _logger = logger;
    _httpClient = httpClient;
  }

  /// <inheritdoc />
  public async Task<UrlValidationResult> ValidateSeqUrlAsync(string? url, CancellationToken cancellationToken = default)
  {
    if (string.IsNullOrWhiteSpace(url))
    {
      return UrlValidationResult.Failure(ValidationReasonCodes.InvalidScheme, "null-or-empty");
    }

    // Parse and normalize the URL
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
    {
      return UrlValidationResult.Failure(ValidationReasonCodes.InvalidScheme, "invalid-uri");
    }

    var sanitizedHost = uri.Host.ToLowerInvariant();

    // Validate scheme
    if (uri.Scheme != "http" && uri.Scheme != "https")
    {
      _logger.LogWarning(SecurityEventIds.UrlValidationBlocked,
        "URL validation blocked: Invalid scheme {Scheme} for host {Host} - reason: {ReasonCode}",
        uri.Scheme, sanitizedHost, ValidationReasonCodes.InvalidScheme);
      return UrlValidationResult.Failure(ValidationReasonCodes.InvalidScheme, sanitizedHost);
    }

    // Check for credentials in URL
    if (!string.IsNullOrEmpty(uri.UserInfo))
    {
      _logger.LogWarning(SecurityEventIds.UrlValidationBlocked,
        "URL validation blocked: Credentials present in URL for host {Host} - reason: {ReasonCode}",
        sanitizedHost, ValidationReasonCodes.CredentialsPresent);
      return UrlValidationResult.Failure(ValidationReasonCodes.CredentialsPresent, sanitizedHost);
    }

    // Check allowlist
    var allowedHosts = GetEffectiveAllowedHosts();
    if (!IsHostAllowed(sanitizedHost, allowedHosts))
    {
      _logger.LogWarning(SecurityEventIds.UrlValidationBlocked,
        "URL validation blocked: Host {Host} not in allowlist - reason: {ReasonCode}",
        sanitizedHost, ValidationReasonCodes.HostNotAllowed);
      return UrlValidationResult.Failure(ValidationReasonCodes.HostNotAllowed, sanitizedHost);
    }

    // For localhost hosts, we can skip DNS resolution
    if (IsLocalhostHost(sanitizedHost))
    {
      _logger.LogInformation(SecurityEventIds.UrlValidationSuccess,
        "URL validation successful: Localhost host {Host} allowed", sanitizedHost);
      return UrlValidationResult.Success(sanitizedHost);
    }

    // DNS resolution validation (if enabled)
    if (_options.EnableDnsResolutionValidation)
    {
      var dnsResult = await ValidateDnsResolutionAsync(sanitizedHost, cancellationToken);
      if (!dnsResult.IsValid)
      {
        return dnsResult;
      }
    }

    _logger.LogInformation(SecurityEventIds.UrlValidationSuccess,
      "URL validation successful: Host {Host} passed all security checks", sanitizedHost);
    return UrlValidationResult.Success(sanitizedHost);
  }

  /// <inheritdoc />
  public IReadOnlyList<string> GetEffectiveAllowedHosts()
  {
    var allowedHosts = new List<string>(_options.AllowedSeqHosts);

    if (_options.EnableDefaultLocalhost)
    {
      allowedHosts.AddRange(DefaultLocalhostHosts);
    }

    return allowedHosts.AsReadOnly();
  }

  private static bool IsHostAllowed(string host, IReadOnlyList<string> allowedHosts)
  {
    // Handle IPv6 addresses enclosed in brackets for comparison
    var hostForComparison = host;
    if (host.StartsWith('[') && host.EndsWith(']'))
    {
      hostForComparison = host.Substring(1, host.Length - 2);
    }

    foreach (var allowedHost in allowedHosts)
    {
      var allowedHostForComparison = allowedHost;
      if (allowedHost.StartsWith('[') && allowedHost.EndsWith(']'))
      {
        allowedHostForComparison = allowedHost.Substring(1, allowedHost.Length - 2);
      }

      // Exact match
      if (string.Equals(hostForComparison, allowedHostForComparison, StringComparison.OrdinalIgnoreCase))
      {
        return true;
      }

      // Suffix match for subdomains (if allowedHost starts with '.')
      if (allowedHostForComparison.StartsWith('.') && 
          hostForComparison.EndsWith(allowedHostForComparison, StringComparison.OrdinalIgnoreCase))
      {
        return true;
      }
    }

    return false;
  }

  private static bool IsLocalhostHost(string host)
  {
    // Handle IPv6 addresses enclosed in brackets
    if (host.StartsWith('[') && host.EndsWith(']'))
    {
      host = host.Substring(1, host.Length - 2);
    }
    
    return DefaultLocalhostHosts.Contains(host, StringComparer.OrdinalIgnoreCase);
  }

  private async Task<UrlValidationResult> ValidateDnsResolutionAsync(string host, CancellationToken cancellationToken)
  {
    try
    {
      using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
      cts.CancelAfter(TimeSpan.FromSeconds(_options.DnsResolutionTimeoutSeconds));

      var addresses = await Dns.GetHostAddressesAsync(host, cts.Token);

      foreach (var address in addresses)
      {
        if (IsPrivateIpAddress(address))
        {
          _logger.LogWarning(SecurityEventIds.UrlValidationBlocked,
            "URL validation blocked: Host {Host} resolved to private IP {IP} - reason: {ReasonCode}",
            host, address, ValidationReasonCodes.DnsResolvedToPrivateIp);
          return UrlValidationResult.Failure(ValidationReasonCodes.DnsResolvedToPrivateIp, host);
        }
      }

      return UrlValidationResult.Success(host);
    }
    catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
    {
      throw;
    }
    catch (OperationCanceledException)
    {
      _logger.LogWarning(SecurityEventIds.DnsResolutionError,
        "URL validation blocked: DNS resolution timeout for host {Host} - reason: {ReasonCode}",
        host, ValidationReasonCodes.DnsResolutionFailed);
      return UrlValidationResult.Failure(ValidationReasonCodes.DnsResolutionFailed, host);
    }
    catch (Exception ex)
    {
      _logger.LogWarning(ex, 
        "URL validation blocked: DNS resolution error for host {Host} - reason: {ReasonCode} - EventId: {EventId}",
        host, ValidationReasonCodes.DnsResolutionFailed, SecurityEventIds.DnsResolutionError);
      return UrlValidationResult.Failure(ValidationReasonCodes.DnsResolutionFailed, host);
    }
  }

  private static bool IsPrivateIpAddress(IPAddress address)
  {
    // Convert to bytes for easier checking
    var bytes = address.GetAddressBytes();

    if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
    {
      // IPv4 private ranges
      return 
        // 10.0.0.0/8
        bytes[0] == 10 ||
        // 172.16.0.0/12  
        (bytes[0] == 172 && bytes[1] >= 16 && bytes[1] <= 31) ||
        // 192.168.0.0/16
        (bytes[0] == 192 && bytes[1] == 168) ||
        // 169.254.0.0/16 (link-local)
        (bytes[0] == 169 && bytes[1] == 254) ||
        // 127.0.0.0/8 (loopback)
        bytes[0] == 127;
    }
    else if (address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
    {
      // IPv6 private ranges
      return 
        // ::1 (loopback)
        address.Equals(IPAddress.IPv6Loopback) ||
        // fc00::/7 (private)
        (bytes[0] >= 0xfc && bytes[0] <= 0xfd) ||
        // fe80::/10 (link-local)
        (bytes[0] == 0xfe && (bytes[1] & 0xc0) == 0x80);
    }

    return false;
  }
}