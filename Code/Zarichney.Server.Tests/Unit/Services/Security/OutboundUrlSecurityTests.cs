using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using Zarichney.Services.Security;

namespace Zarichney.Tests.Unit.Services.Security;

/// <summary>
/// Unit tests for OutboundUrlSecurity service - tests SSRF prevention and URL validation
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Security")]
[Trait("Feature", "OutboundUrlSecurity")]
public class OutboundUrlSecurityTests
{
  private readonly IFixture _fixture;
  private readonly Mock<ILogger<OutboundUrlSecurity>> _mockLogger;
  private readonly Mock<IOptions<NetworkSecurityOptions>> _mockOptions;
  private readonly Mock<HttpClient> _mockHttpClient;
  private readonly NetworkSecurityOptions _securityOptions;
  private readonly OutboundUrlSecurity _sut;

  public OutboundUrlSecurityTests()
  {
    _fixture = new Fixture();
    _mockLogger = new Mock<ILogger<OutboundUrlSecurity>>();
    _mockOptions = new Mock<IOptions<NetworkSecurityOptions>>();
    _mockHttpClient = new Mock<HttpClient>();

    _securityOptions = new NetworkSecurityOptions
    {
      AllowedSeqHosts = ["allowed.example.com", ".trusted.domain.com"],
      EnableDefaultLocalhost = true,
      MaxRedirects = 3,
      EnableDnsResolutionValidation = false, // Disable for unit tests to avoid network calls
      DnsResolutionTimeoutSeconds = 2
    };

    _mockOptions.Setup(x => x.Value).Returns(_securityOptions);

    _sut = new OutboundUrlSecurity(
      _mockOptions.Object,
      _mockLogger.Object,
      _mockHttpClient.Object);
  }

  #region ValidateSeqUrlAsync Tests

  [Theory]
  [InlineData(null)]
  [InlineData("")]
  [InlineData("   ")]
  public async Task ValidateSeqUrlAsync_WithNullOrEmptyUrl_ReturnsInvalidScheme(string? url)
  {
    // Act
    var result = await _sut.ValidateSeqUrlAsync(url);

    // Assert
    result.Should().NotBeNull();
    result.IsValid.Should().BeFalse();
    result.ReasonCode.Should().Be(ValidationReasonCodes.InvalidScheme);
    result.SanitizedHost.Should().Be("null-or-empty");
  }

  [Theory]
  [InlineData("not-a-url")]
  [InlineData("://malformed")]
  [InlineData("ftp://example.com")]
  public async Task ValidateSeqUrlAsync_WithInvalidUrl_ReturnsInvalidScheme(string url)
  {
    // Act
    var result = await _sut.ValidateSeqUrlAsync(url);

    // Assert
    result.Should().NotBeNull();
    result.IsValid.Should().BeFalse();
    result.ReasonCode.Should().Be(ValidationReasonCodes.InvalidScheme);
  }

  [Theory]
  [InlineData("ftp://example.com:5341")]
  [InlineData("ssh://example.com:5341")]
  [InlineData("file://example.com:5341")]
  public async Task ValidateSeqUrlAsync_WithInvalidScheme_ReturnsInvalidScheme(string url)
  {
    // Act
    var result = await _sut.ValidateSeqUrlAsync(url);

    // Assert
    result.Should().NotBeNull();
    result.IsValid.Should().BeFalse();
    result.ReasonCode.Should().Be(ValidationReasonCodes.InvalidScheme);
  }

  [Theory]
  [InlineData("http://user:pass@example.com:5341")]
  [InlineData("https://admin@example.com:5341")]
  [InlineData("http://user:password@localhost:5341")]
  public async Task ValidateSeqUrlAsync_WithCredentials_ReturnsCredentialsPresent(string url)
  {
    // Act
    var result = await _sut.ValidateSeqUrlAsync(url);

    // Assert
    result.Should().NotBeNull();
    result.IsValid.Should().BeFalse();
    result.ReasonCode.Should().Be(ValidationReasonCodes.CredentialsPresent);
  }

  [Theory]
  [InlineData("http://notallowed.example.com:5341")]
  [InlineData("https://malicious.com:5341")]
  [InlineData("http://external.site.com:5341")]
  public async Task ValidateSeqUrlAsync_WithHostNotInAllowlist_ReturnsHostNotAllowed(string url)
  {
    // Act
    var result = await _sut.ValidateSeqUrlAsync(url);

    // Assert
    result.Should().NotBeNull();
    result.IsValid.Should().BeFalse();
    result.ReasonCode.Should().Be(ValidationReasonCodes.HostNotAllowed);
  }

  [Theory]
  [InlineData("http://allowed.example.com:5341")]
  [InlineData("https://allowed.example.com:5341")]
  [InlineData("http://ALLOWED.EXAMPLE.COM:5341")] // Case insensitive
  public async Task ValidateSeqUrlAsync_WithAllowedExactHost_ReturnsValid(string url)
  {
    // Act
    var result = await _sut.ValidateSeqUrlAsync(url);

    // Assert
    result.Should().NotBeNull();
    result.IsValid.Should().BeTrue();
    result.ReasonCode.Should().BeNull();
    result.SanitizedHost.Should().Be("allowed.example.com");
  }

  [Theory]
  [InlineData("http://sub.trusted.domain.com:5341")]
  [InlineData("https://api.trusted.domain.com:5341")]
  [InlineData("http://nested.sub.trusted.domain.com:5341")]
  public async Task ValidateSeqUrlAsync_WithAllowedSuffixHost_ReturnsValid(string url)
  {
    // Act
    var result = await _sut.ValidateSeqUrlAsync(url);

    // Assert
    result.Should().NotBeNull();
    result.IsValid.Should().BeTrue();
    result.ReasonCode.Should().BeNull();
  }

  [Theory]
  [InlineData("http://localhost:5341")]
  [InlineData("https://127.0.0.1:5341")]
  public async Task ValidateSeqUrlAsync_WithLocalhostAndEnabledDefault_ReturnsValid(string url)
  {
    // Act
    var result = await _sut.ValidateSeqUrlAsync(url);

    // Assert
    result.Should().NotBeNull();
    result.IsValid.Should().BeTrue();
    result.ReasonCode.Should().BeNull();
  }

  [Theory]
  [InlineData("http://localhost:5341")]
  [InlineData("https://127.0.0.1:5341")]
  public async Task ValidateSeqUrlAsync_WithLocalhostAndDisabledDefault_ReturnsHostNotAllowed(string url)
  {
    // Arrange
    _securityOptions.EnableDefaultLocalhost = false;

    // Act
    var result = await _sut.ValidateSeqUrlAsync(url);

    // Assert
    result.Should().NotBeNull();
    result.IsValid.Should().BeFalse();
    result.ReasonCode.Should().Be(ValidationReasonCodes.HostNotAllowed);
  }

  #endregion

  #region GetEffectiveAllowedHosts Tests

  [Fact]
  public void GetEffectiveAllowedHosts_WithDefaultLocalhostEnabled_IncludesLocalhostEntries()
  {
    // Act
    var allowedHosts = _sut.GetEffectiveAllowedHosts();

    // Assert
    allowedHosts.Should().NotBeNull();
    allowedHosts.Should().Contain("localhost");
    allowedHosts.Should().Contain("127.0.0.1");
    allowedHosts.Should().Contain("::1");
    allowedHosts.Should().Contain("allowed.example.com");
    allowedHosts.Should().Contain(".trusted.domain.com");
  }

  [Fact]
  public void GetEffectiveAllowedHosts_WithDefaultLocalhostDisabled_ExcludesLocalhostEntries()
  {
    // Arrange
    _securityOptions.EnableDefaultLocalhost = false;

    // Act
    var allowedHosts = _sut.GetEffectiveAllowedHosts();

    // Assert
    allowedHosts.Should().NotBeNull();
    allowedHosts.Should().NotContain("localhost");
    allowedHosts.Should().NotContain("127.0.0.1");
    allowedHosts.Should().NotContain("::1");
    allowedHosts.Should().Contain("allowed.example.com");
    allowedHosts.Should().Contain(".trusted.domain.com");
  }

  [Fact]
  public void GetEffectiveAllowedHosts_WithEmptyConfiguration_ReturnsOnlyDefaults()
  {
    // Arrange
    _securityOptions.AllowedSeqHosts.Clear();

    // Act
    var allowedHosts = _sut.GetEffectiveAllowedHosts();

    // Assert
    allowedHosts.Should().NotBeNull();
    allowedHosts.Should().Contain("localhost");
    allowedHosts.Should().Contain("127.0.0.1");
    allowedHosts.Should().Contain("::1");
    allowedHosts.Should().HaveCount(3);
  }

  #endregion
}