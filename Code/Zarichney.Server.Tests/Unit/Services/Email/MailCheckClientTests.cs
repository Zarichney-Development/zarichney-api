using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Zarichney.Config;
using Zarichney.Services.Email;
using Zarichney.Services.Status;
using Zarichney.Server.Tests.TestData.Builders;

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment.

namespace Zarichney.Server.Tests.Unit.Services.Email;

/// <summary>
/// Unit tests for MailCheckClient.
/// Note: RestSharp's RestClient is difficult to mock, so these tests focus on
/// scenarios where the cache is hit and configuration validation logic.
/// </summary>
public class MailCheckClientTests
{
  private readonly Mock<IMemoryCache> _mockCache;
  private readonly Mock<ILogger<MailCheckClient>> _mockLogger;
  private readonly EmailConfig _emailConfig;
  private readonly MailCheckClient _sut;

  public MailCheckClientTests()
  {
    _mockCache = new Mock<IMemoryCache>();
    _mockLogger = new Mock<ILogger<MailCheckClient>>();
    _emailConfig = CreateValidEmailConfig();
    _sut = new MailCheckClient(_emailConfig, _mockCache.Object, _mockLogger.Object);
  }

  private static EmailConfig CreateValidEmailConfig(string? mailCheckApiKey = "valid-api-key")
  {
    return new EmailConfig
    {
      MailCheckApiKey = mailCheckApiKey ?? "valid-api-key",
      AzureTenantId = "test-tenant-id",
      AzureAppId = "test-app-id",
      AzureAppSecret = "test-app-secret",
      FromEmail = "test@example.com"
    };
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetValidationData_WithCachedResult_ReturnsCachedValue()
  {
    // Arrange
    const string domain = "cached.com";
    var cachedResponse = new EmailValidationResponseBuilder()
      .WithValidDefaults()
      .WithDomain(domain)
      .Build();

    object cacheValue = cachedResponse;
    _mockCache.Setup(x => x.TryGetValue(domain, out cacheValue))
      .Returns(true);

    // Act
    var result = await _sut.GetValidationData(domain);

    // Assert
    result.Should().BeEquivalentTo(cachedResponse,
      "the cached value should be returned without making an API call");

    _mockCache.Verify(x => x.TryGetValue(domain, out It.Ref<object>.IsAny), Times.Once);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetValidationData_WithDisposableDomain_ReturnsCachedDisposableResponse()
  {
    // Arrange
    const string domain = "tempmail.org";
    var disposableResponse = new EmailValidationResponseBuilder()
      .WithDisposableEmail()
      .WithDomain(domain)
      .Build();

    object cacheValue = disposableResponse;
    _mockCache.Setup(x => x.TryGetValue(domain, out cacheValue))
      .Returns(true);

    // Act
    var result = await _sut.GetValidationData(domain);

    // Assert
    result.Should().NotBeNull();
    result.Disposable.Should().BeTrue("tempmail.org is a disposable email domain");
    result.Domain.Should().Be(domain);
    result.Risk.Should().BeGreaterThan(80, "disposable emails are high risk");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetValidationData_WithBlockedDomain_ReturnsCachedBlockedResponse()
  {
    // Arrange
    const string domain = "blocked.com";
    var blockedResponse = new EmailValidationResponseBuilder()
      .WithBlockedEmail()
      .WithDomain(domain)
      .Build();

    object cacheValue = blockedResponse;
    _mockCache.Setup(x => x.TryGetValue(domain, out cacheValue))
      .Returns(true);

    // Act
    var result = await _sut.GetValidationData(domain);

    // Assert
    result.Should().NotBeNull();
    result.Block.Should().BeTrue("the domain is blocked");
    result.Valid.Should().BeTrue("blocked emails can be syntactically valid");
    result.Domain.Should().Be(domain);
    result.Risk.Should().BeGreaterThan(85, "blocked domains are high risk");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetValidationData_WithHighRiskDomain_ReturnsCachedHighRiskResponse()
  {
    // Arrange
    const string domain = "highrisk.net";
    const int riskScore = 95;
    var highRiskResponse = new EmailValidationResponseBuilder()
      .WithHighRiskEmail(riskScore)
      .WithDomain(domain)
      .Build();

    object cacheValue = highRiskResponse;
    _mockCache.Setup(x => x.TryGetValue(domain, out cacheValue))
      .Returns(true);

    // Act
    var result = await _sut.GetValidationData(domain);

    // Assert
    result.Should().NotBeNull();
    result.Risk.Should().Be(riskScore, "this is a high-risk domain");
    result.Domain.Should().Be(domain);
    result.Valid.Should().BeTrue("high-risk emails can still be valid");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetValidationData_WithTypoDomain_ReturnsCachedTypoResponse()
  {
    // Arrange
    const string domain = "gmai.com";
    var typoSuggestions = new[] { "gmail.com" };
    var typoResponse = new EmailValidationResponseBuilder()
      .WithPossibleTypo(typoSuggestions)
      .WithDomain(domain)
      .Build();

    object cacheValue = typoResponse;
    _mockCache.Setup(x => x.TryGetValue(domain, out cacheValue))
      .Returns(true);

    // Act
    var result = await _sut.GetValidationData(domain);

    // Assert
    result.Should().NotBeNull();
    result.Valid.Should().BeFalse("the domain likely has a typo");
    result.PossibleTypo.Should().BeEquivalentTo(typoSuggestions);
    result.Domain.Should().Be(domain);
    result.Reason.Should().Contain("typo", "this is a typo detection scenario");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetValidationData_WithInvalidSyntaxDomain_ReturnsCachedInvalidResponse()
  {
    // Arrange
    const string domain = "invalid@domain";
    var invalidResponse = new EmailValidationResponseBuilder()
      .WithInvalidEmail()
      .WithDomain(domain)
      .WithReason("invalid domain syntax")
      .Build();

    object cacheValue = invalidResponse;
    _mockCache.Setup(x => x.TryGetValue(domain, out cacheValue))
      .Returns(true);

    // Act
    var result = await _sut.GetValidationData(domain);

    // Assert
    result.Should().NotBeNull();
    result.Valid.Should().BeFalse("the domain syntax is invalid");
    result.Domain.Should().Be(domain);
    result.Reason.Should().Contain("syntax", "this is a syntax error");
    result.Risk.Should().BeGreaterThan(90, "invalid emails are very high risk");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetValidationData_MultipleCalls_SameDomain_UsesCache()
  {
    // Arrange
    const string domain = "cached.com";
    var cachedResponse = new EmailValidationResponseBuilder()
      .WithValidDefaults()
      .WithDomain(domain)
      .Build();

    object cacheValue = cachedResponse;
    _mockCache.Setup(x => x.TryGetValue(domain, out cacheValue))
      .Returns(true);

    // Act
    var result1 = await _sut.GetValidationData(domain);
    var result2 = await _sut.GetValidationData(domain);
    var result3 = await _sut.GetValidationData(domain);

    // Assert
    result1.Should().BeEquivalentTo(cachedResponse);
    result2.Should().BeEquivalentTo(cachedResponse);
    result3.Should().BeEquivalentTo(cachedResponse);

    _mockCache.Verify(x => x.TryGetValue(domain, out It.Ref<object>.IsAny), Times.Exactly(3),
      "each call should check the cache");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetValidationData_DifferentDomains_ChecksCacheSeparately()
  {
    // Arrange
    const string domain1 = "domain1.com";
    const string domain2 = "domain2.com";

    var response1 = new EmailValidationResponseBuilder()
      .WithValidDefaults()
      .WithDomain(domain1)
      .Build();

    var response2 = new EmailValidationResponseBuilder()
      .WithDisposableEmail()
      .WithDomain(domain2)
      .Build();

    object cacheValue1 = response1;
    object cacheValue2 = response2;

    _mockCache.Setup(x => x.TryGetValue(domain1, out cacheValue1))
      .Returns(true);
    _mockCache.Setup(x => x.TryGetValue(domain2, out cacheValue2))
      .Returns(true);

    // Act
    var result1 = await _sut.GetValidationData(domain1);
    var result2 = await _sut.GetValidationData(domain2);

    // Assert
    result1.Domain.Should().Be(domain1);
    result1.Disposable.Should().BeFalse();

    result2.Domain.Should().Be(domain2);
    result2.Disposable.Should().BeTrue();

    _mockCache.Verify(x => x.TryGetValue(It.IsAny<object>(), out It.Ref<object>.IsAny), Times.Exactly(2));
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetValidationData_WithEmailForwarderDomain_ReturnsCachedForwarderResponse()
  {
    // Arrange
    const string domain = "forwarder.net";
    var forwarderResponse = new EmailValidationResponseBuilder()
      .WithValidDefaults()
      .WithDomain(domain)
      .Build();
    forwarderResponse.EmailForwarder = true;
    forwarderResponse.Risk = 75;
    forwarderResponse.Reason = "email forwarder detected";

    object cacheValue = forwarderResponse;
    _mockCache.Setup(x => x.TryGetValue(domain, out cacheValue))
      .Returns(true);

    // Act
    var result = await _sut.GetValidationData(domain);

    // Assert
    result.Should().NotBeNull();
    result.EmailForwarder.Should().BeTrue("this is an email forwarder domain");
    result.Domain.Should().Be(domain);
    result.Risk.Should().Be(75, "email forwarders have elevated risk");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetValidationData_WithLowRiskValidDomain_ReturnsCachedValidResponse()
  {
    // Arrange
    const string domain = "trusted.org";
    var trustedResponse = new EmailValidationResponseBuilder()
      .WithValidDefaults()
      .WithDomain(domain)
      .WithRisk(5)
      .WithMxHost("mx.trusted.org")
      .Build();

    object cacheValue = trustedResponse;
    _mockCache.Setup(x => x.TryGetValue(domain, out cacheValue))
      .Returns(true);

    // Act
    var result = await _sut.GetValidationData(domain);

    // Assert
    result.Should().NotBeNull();
    result.Valid.Should().BeTrue("this is a valid domain");
    result.Block.Should().BeFalse("trusted domains should not be blocked");
    result.Disposable.Should().BeFalse("trusted domains are not disposable");
    result.Risk.Should().BeLessThan(10, "trusted domains have low risk");
    result.MxHost.Should().Be("mx.trusted.org");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetValidationData_WithComplexMxInfo_ReturnsCachedComplexResponse()
  {
    // Arrange
    const string domain = "complex.com";
    var complexResponse = new EmailValidationResponseBuilder()
      .WithValidDefaults()
      .WithDomain(domain)
      .WithMxHost("mx1.complex.com, mx2.complex.com")
      .Build();
    complexResponse.MxIp = "192.168.1.1, 192.168.1.2";
    complexResponse.MxInfo = "Multiple MX records with failover";
    complexResponse.LastChangedAt = DateTime.UtcNow.AddMonths(-6);

    object cacheValue = complexResponse;
    _mockCache.Setup(x => x.TryGetValue(domain, out cacheValue))
      .Returns(true);

    // Act
    var result = await _sut.GetValidationData(domain);

    // Assert
    result.Should().NotBeNull();
    result.MxHost.Should().Contain("mx1.complex.com");
    result.MxHost.Should().Contain("mx2.complex.com");
    result.MxIp.Should().Contain("192.168.1.1");
    result.MxInfo.Should().Contain("failover");
    result.LastChangedAt.Should().BeBefore(DateTime.UtcNow.AddMonths(-5));
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetValidationData_WithRecentlyChangedDomain_ReturnsCachedRecentResponse()
  {
    // Arrange
    const string domain = "recentchange.com";
    var recentChangeResponse = new EmailValidationResponseBuilder()
      .WithValidDefaults()
      .WithDomain(domain)
      .Build();
    recentChangeResponse.LastChangedAt = DateTime.UtcNow.AddDays(-2);
    recentChangeResponse.Reason = "recent DNS changes detected";
    recentChangeResponse.Risk = 45;

    object cacheValue = recentChangeResponse;
    _mockCache.Setup(x => x.TryGetValue(domain, out cacheValue))
      .Returns(true);

    // Act
    var result = await _sut.GetValidationData(domain);

    // Assert
    result.Should().NotBeNull();
    result.LastChangedAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(-2), TimeSpan.FromHours(1));
    result.Reason.Should().Contain("recent");
    result.Risk.Should().Be(45, "recent changes indicate moderate risk");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetValidationData_WithNoMxRecordDomain_ReturnsCachedNoMxResponse()
  {
    // Arrange
    const string domain = "nomx.com";
    var noMxResponse = new EmailValidationResponseBuilder()
      .WithInvalidEmail()
      .WithDomain(domain)
      .WithReason("no MX record found")
      .Build();
    noMxResponse.MxHost = string.Empty;
    noMxResponse.MxIp = string.Empty;
    noMxResponse.MxInfo = "No MX records configured";

    object cacheValue = noMxResponse;
    _mockCache.Setup(x => x.TryGetValue(domain, out cacheValue))
      .Returns(true);

    // Act
    var result = await _sut.GetValidationData(domain);

    // Assert
    result.Should().NotBeNull();
    result.Valid.Should().BeFalse("domains without MX records cannot receive email");
    result.MxHost.Should().BeEmpty();
    result.MxIp.Should().BeEmpty();
    result.MxInfo.Should().Contain("No MX");
    result.Reason.Should().Contain("no MX");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Constructor_WithValidDependencies_CreatesInstance()
  {
    // Arrange
    var config = CreateValidEmailConfig();
    var cache = new Mock<IMemoryCache>().Object;
    var logger = new Mock<ILogger<MailCheckClient>>().Object;

    // Act
    var client = new MailCheckClient(config, cache, logger);

    // Assert
    client.Should().NotBeNull();
    client.Should().BeOfType<MailCheckClient>();
    client.Should().BeAssignableTo<IMailCheckClient>();
  }

  // Note: Testing API key validation with cache miss would require refactoring
  // the MailCheckClient to accept IRestClient as a dependency for proper mocking.
  // Currently, RestSharp's RestClient is instantiated directly in the method,
  // making it impossible to test scenarios where cache miss occurs with invalid API keys.

  [Fact]
  [Trait("Category", "Unit")]
  public async Task GetValidationData_WithSeparateCacheSetups_WorksCorrectly()
  {
    // Arrange - Test that multiple cache setups for different keys work
    const string domain1 = "first.com";
    const string domain2 = "second.com";
    const string domain3 = "third.com";

    var response1 = new EmailValidationResponseBuilder().WithValidDefaults().WithDomain(domain1).Build();
    var response2 = new EmailValidationResponseBuilder().WithDisposableEmail().WithDomain(domain2).Build();
    var response3 = new EmailValidationResponseBuilder().WithBlockedEmail().WithDomain(domain3).Build();

    object cache1 = response1;
    object cache2 = response2;
    object cache3 = response3;

    _mockCache.Setup(x => x.TryGetValue(domain1, out cache1)).Returns(true);
    _mockCache.Setup(x => x.TryGetValue(domain2, out cache2)).Returns(true);
    _mockCache.Setup(x => x.TryGetValue(domain3, out cache3)).Returns(true);

    // Act & Assert - Call in different order to verify independent caching
    var result2 = await _sut.GetValidationData(domain2);
    result2.Disposable.Should().BeTrue();

    var result3 = await _sut.GetValidationData(domain3);
    result3.Block.Should().BeTrue();

    var result1 = await _sut.GetValidationData(domain1);
    result1.Valid.Should().BeTrue();
    result1.Block.Should().BeFalse();
  }
}
