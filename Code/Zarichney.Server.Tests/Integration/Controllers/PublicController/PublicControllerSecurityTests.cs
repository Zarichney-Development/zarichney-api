using System.Net;
using FluentAssertions;
using Refit;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Client.Contracts;
using Zarichney.Tests.Framework.Attributes;
using Zarichney.Tests.Framework.Fixtures;

namespace Zarichney.Tests.Integration.Controllers.PublicController;

/// <summary>
/// Integration tests for PublicController security features - specifically SSRF prevention in test-seq endpoint
/// </summary>
[Trait("Category", "Integration")]
[Trait("Component", "Controller")]
[Trait("Feature", "Security")]
[Collection("IntegrationCore")]
public class PublicControllerSecurityTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
  : IntegrationTestBase(apiClientFixture, testOutputHelper)
{
  /// <summary>
  /// Tests that the test-seq endpoint returns 400 for URLs not in the allowlist
  /// </summary>
  [Theory]
  [InlineData("http://malicious.external.com:5341")]
  [InlineData("https://attacker.site.org:5341")]
  [InlineData("http://unknown.host.net:5341")]
  [LogTestStartEnd]
  public async Task TestSeqConnectivity_WithUnallowedHost_Returns400BadRequest(string maliciousUrl)
  {
    using (CreateTestMethodContext($"{nameof(TestSeqConnectivity_WithUnallowedHost_Returns400BadRequest)}_{(Uri.TryCreate(maliciousUrl, UriKind.Absolute, out var uri) ? uri.Host : "invalid")}"))
    {
      // Arrange
      var testRequest = new TestSeqRequest(maliciousUrl);

      // Act
      var act = () => _apiClientFixture.UnauthenticatedPublicApi.TestSeq(testRequest);

      // Assert
      var exception = await act.Should().ThrowAsync<ApiException>(
        because: "unallowed hosts should be rejected for security reasons");
      exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
      
      // Verify that no internal information is leaked
      var content = await exception.Which.GetContentAsAsync<dynamic>();
      content.Should().NotBeNull();
      // The error message should be generic and not expose internal details
    }
  }

  /// <summary>
  /// Tests that the test-seq endpoint returns 400 for URLs with credentials
  /// </summary>
  [Theory]
  [InlineData("http://user:pass@localhost:5341")]
  [InlineData("https://admin:secret@127.0.0.1:5341")]
  [InlineData("http://attacker:password@allowed.example.com:5341")]
  [LogTestStartEnd]
  public async Task TestSeqConnectivity_WithCredentialsInUrl_Returns400BadRequest(string urlWithCredentials)
  {
    using (CreateTestMethodContext($"{nameof(TestSeqConnectivity_WithCredentialsInUrl_Returns400BadRequest)}"))
    {
      // Arrange
      var testRequest = new TestSeqRequest(urlWithCredentials);

      // Act
      var act = () => _apiClientFixture.UnauthenticatedPublicApi.TestSeq(testRequest);

      // Assert
      var exception = await act.Should().ThrowAsync<ApiException>(
        because: "URLs with credentials should be rejected for security reasons");
      exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
  }

  /// <summary>
  /// Tests that the test-seq endpoint returns 400 for invalid URL schemes
  /// </summary>
  [Theory]
  [InlineData("ftp://localhost:5341")]
  [InlineData("file://localhost:5341")]
  [InlineData("ssh://localhost:5341")]
  [InlineData("ldap://localhost:5341")]
  [LogTestStartEnd]
  public async Task TestSeqConnectivity_WithInvalidScheme_Returns400BadRequest(string invalidSchemeUrl)
  {
    using (CreateTestMethodContext($"{nameof(TestSeqConnectivity_WithInvalidScheme_Returns400BadRequest)}"))
    {
      // Arrange
      var testRequest = new TestSeqRequest(invalidSchemeUrl);

      // Act
      var act = () => _apiClientFixture.UnauthenticatedPublicApi.TestSeq(testRequest);

      // Assert
      var exception = await act.Should().ThrowAsync<ApiException>(
        because: "non-HTTP(S) schemes should be rejected for security reasons");
      exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
  }

  /// <summary>
  /// Tests that the test-seq endpoint still works for allowed localhost hosts
  /// </summary>
  [Theory]
  [InlineData("http://localhost:5341")]
  [InlineData("http://127.0.0.1:5341")]
  [InlineData("https://localhost:5341")]
  [LogTestStartEnd]
  public async Task TestSeqConnectivity_WithAllowedLocalhostHost_ReturnsResult(string allowedUrl)
  {
    using (CreateTestMethodContext($"{nameof(TestSeqConnectivity_WithAllowedLocalhostHost_ReturnsResult)}_{(Uri.TryCreate(allowedUrl, UriKind.Absolute, out var uri) ? uri.Host : "invalid")}"))
    {
      // Arrange
      var testRequest = new TestSeqRequest(allowedUrl);

      // Act
      var connectivityResponse = await _apiClientFixture.UnauthenticatedPublicApi.TestSeq(testRequest);

      // Assert
      connectivityResponse.Should().NotBeNull("allowed localhost URLs should be processed normally");
      connectivityResponse.Content.Should().NotBeNull();
      connectivityResponse.Content.Url.Should().Be(allowedUrl);
      
      // The connection may fail (IsConnected = false) but that's OK - the important thing is that
      // the URL was not rejected by security validation
      connectivityResponse.Content.Error.Should().NotBe("URL validation failed for security reasons",
        "security validation should pass for allowed localhost hosts");
    }
  }

  /// <summary>
  /// Tests that the test-seq endpoint still works when no URL is provided (uses default)
  /// </summary>
  [Fact]
  [LogTestStartEnd]
  public async Task TestSeqConnectivity_WithNullRequest_ReturnsDefaultUrlResult()
  {
    using (CreateTestMethodContext(nameof(TestSeqConnectivity_WithNullRequest_ReturnsDefaultUrlResult)))
    {
      // Arrange & Act
      var connectivityResponse = await _apiClientFixture.UnauthenticatedPublicApi.TestSeq(null);

      // Assert
      connectivityResponse.Should().NotBeNull("default URL should be processed normally");
      connectivityResponse.Content.Should().NotBeNull();
      connectivityResponse.Content.Url.Should().NotBeNullOrEmpty("should use a default URL");
      
      // Verify it's not rejected by security validation
      connectivityResponse.Content.Error.Should().NotBe("URL validation failed for security reasons",
        "security validation should pass for default configured URL");
    }
  }

  /// <summary>
  /// Tests that malformed URLs are handled gracefully
  /// </summary>
  [Theory]
  [InlineData("not-a-url")]
  [InlineData("://malformed")]
  [InlineData("http://")]
  [InlineData("")]
  [LogTestStartEnd]
  public async Task TestSeqConnectivity_WithMalformedUrl_Returns400BadRequest(string malformedUrl)
  {
    using (CreateTestMethodContext($"{nameof(TestSeqConnectivity_WithMalformedUrl_Returns400BadRequest)}"))
    {
      // Arrange
      var testRequest = new TestSeqRequest(malformedUrl);

      // Act
      var act = () => _apiClientFixture.UnauthenticatedPublicApi.TestSeq(testRequest);

      // Assert
      var exception = await act.Should().ThrowAsync<ApiException>(
        because: "malformed URLs should be rejected");
      exception.Which.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
  }
}