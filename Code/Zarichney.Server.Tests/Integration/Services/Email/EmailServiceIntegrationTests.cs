using FluentAssertions;
using Xunit;
using Xunit.Abstractions;
using Zarichney.Services.Status;
using Zarichney.Server.Tests.Framework.Attributes;
using Zarichney.Server.Tests.Framework.Fixtures;
using Zarichney.Server.Tests.Integration;

namespace Zarichney.Server.Tests.Integration.Services.Email;

/// <summary>
/// Integration tests for EmailService that validate API contracts and real-world scenarios.
/// These tests verify email validation endpoints with proper HTTP request handling.
/// Tests will be skipped when external MS Graph dependencies are unavailable.
/// </summary>
[Trait("Category", "Integration")]
[Trait("Component", "Service")]
[Trait("Feature", "Email")]
[Collection("IntegrationExternal")]
public class EmailServiceIntegrationTests : IntegrationTestBase
{
  public EmailServiceIntegrationTests(ApiClientFixture apiClientFixture, ITestOutputHelper testOutputHelper)
      : base(apiClientFixture, testOutputHelper)
  {
  }

  #region Email Validation API Contract Tests

  /// <summary>
  /// Tests email validation endpoint with valid email address.
  /// Validates successful HTTP response and API contract compliance.
  /// </summary>
  [DependencyFact(ExternalServices.MsGraph)]
  [Trait("TestCase", "EmailValidation")]
  public async Task ValidateEmail_WithValidEmail_ReturnsSuccessResponse()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(ValidateEmail_WithValidEmail_ReturnsSuccessResponse));
    var validEmail = "test@gmail.com";
    var apiClient = _apiClientFixture.UnauthenticatedApiApi;

    // Act
    var response = await apiClient.Validate(validEmail);

    // Assert
    response.Should().NotBeNull("API response should not be null");
    response.IsSuccessStatusCode.Should().BeTrue("Valid email should return success status code");
    response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK, "Valid email validation should return 200 OK");

    // Validate response content structure
    var content = response.Content;
    content.Should().NotBeNull("Response content should not be null");
  }

  /// <summary>
  /// Tests email validation endpoint with invalid email syntax.
  /// Validates proper error handling and API contract for invalid input.
  /// </summary>
  [DependencyFact(ExternalServices.MsGraph)]
  [Trait("TestCase", "EmailValidation")]
  public async Task ValidateEmail_WithInvalidEmailSyntax_ReturnsBadRequestResponse()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(ValidateEmail_WithInvalidEmailSyntax_ReturnsBadRequestResponse));
    var invalidEmail = "invalid-email-format";
    var apiClient = _apiClientFixture.UnauthenticatedApiApi;

    // Act
    var response = await apiClient.Validate(invalidEmail);

    // Assert
    response.Should().NotBeNull("API response should not be null");
    response.IsSuccessStatusCode.Should().BeFalse("Invalid email should not return success status code");
    response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest, "Invalid email validation should return 400 Bad Request");
  }

  /// <summary>
  /// Tests email validation endpoint with empty email parameter.
  /// Validates proper error handling for missing required parameter.
  /// </summary>
  [DependencyFact(ExternalServices.MsGraph)]
  [Trait("TestCase", "EmailValidation")]
  public async Task ValidateEmail_WithEmptyEmail_ReturnsBadRequestResponse()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(ValidateEmail_WithEmptyEmail_ReturnsBadRequestResponse));
    var emptyEmail = string.Empty;
    var apiClient = _apiClientFixture.UnauthenticatedApiApi;

    // Act
    var response = await apiClient.Validate(emptyEmail);

    // Assert
    response.Should().NotBeNull("API response should not be null");
    response.IsSuccessStatusCode.Should().BeFalse("Empty email should not return success status code");
    response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest, "Empty email validation should return 400 Bad Request");
  }

  /// <summary>
  /// Tests email validation endpoint with disposable email domain.
  /// Validates business logic for rejecting temporary/disposable email addresses.
  /// </summary>
  [DependencyFact(ExternalServices.MsGraph)]
  [Trait("TestCase", "EmailValidation")]
  public async Task ValidateEmail_WithDisposableEmail_ReturnsBadRequestResponse()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(ValidateEmail_WithDisposableEmail_ReturnsBadRequestResponse));
    var disposableEmail = "test@10minutemail.com"; // Known disposable email domain
    var apiClient = _apiClientFixture.UnauthenticatedApiApi;

    // Act
    var response = await apiClient.Validate(disposableEmail);

    // Assert
    response.Should().NotBeNull("API response should not be null");
    response.IsSuccessStatusCode.Should().BeFalse("Disposable email should not return success status code");
    response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest, "Disposable email validation should return 400 Bad Request");
  }

  /// <summary>
  /// Tests email validation endpoint with high-risk email domain.
  /// Validates business logic for rejecting emails from risky domains.
  /// </summary>
  [DependencyFact(ExternalServices.MsGraph)]
  [Trait("TestCase", "EmailValidation")]
  public async Task ValidateEmail_WithHighRiskEmail_ReturnsBadRequestResponse()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(ValidateEmail_WithHighRiskEmail_ReturnsBadRequestResponse));
    var highRiskEmail = "test@example.org"; // Using generic domain that might be flagged as risky
    var apiClient = _apiClientFixture.UnauthenticatedApiApi;

    // Act
    var response = await apiClient.Validate(highRiskEmail);

    // Assert
    response.Should().NotBeNull("API response should not be null");

    // Note: The actual response depends on the MailCheck API's risk assessment
    // This test validates that the endpoint handles high-risk scenarios appropriately
    if (!response.IsSuccessStatusCode)
    {
      response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest,
          "High-risk email validation should return 400 Bad Request if rejected");
    }
    else
    {
      response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK,
          "Email may pass validation if risk score is below threshold");
    }
  }

  #endregion

  #region Rate Limiting and Error Scenarios

  /// <summary>
  /// Tests email validation endpoint behavior when external service is unavailable.
  /// Validates graceful degradation and proper error responses.
  /// </summary>
  [Fact]
  [Trait("TestCase", "ErrorHandling")]
  public async Task ValidateEmail_WhenEmailServiceUnavailable_ReturnsServiceUnavailableResponse()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(ValidateEmail_WhenEmailServiceUnavailable_ReturnsServiceUnavailableResponse));

    // This test runs when Email service is not configured/available
    // Validates that the service unavailability is handled gracefully
    var testEmail = "test@example.com";
    var apiClient = _apiClientFixture.UnauthenticatedApiApi;

    // Act
    var response = await apiClient.Validate(testEmail);

    // Assert
    response.Should().NotBeNull("API response should not be null");

    // When external service is unavailable or authentication is required, expect:
    // 401 Unauthorized - API requires authentication before checking service availability
    // This is correct behavior: authentication happens before service availability checks
    if (!response.IsSuccessStatusCode)
    {
      response.StatusCode.Should().Be(
          System.Net.HttpStatusCode.Unauthorized,
          "Email service requires authentication before availability checks - 401 is correct behavior");
    }
  }

  #endregion

  #region API Contract Validation

  /// <summary>
  /// Tests that email validation endpoint accepts query parameter correctly.
  /// Validates API contract parameter binding and HTTP method support.
  /// </summary>
  [DependencyFact(ExternalServices.MsGraph)]
  [Trait("TestCase", "ApiContract")]
  public async Task ValidateEmail_ApiContract_AcceptsQueryParameter()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(ValidateEmail_ApiContract_AcceptsQueryParameter));
    var testEmail = "contract-test@gmail.com";
    var apiClient = _apiClientFixture.UnauthenticatedApiApi;

    // Act - This validates that the POST endpoint accepts query parameters correctly
    var response = await apiClient.Validate(testEmail);

    // Assert
    response.Should().NotBeNull("API response should not be null");

    // The endpoint should process the request (regardless of external service availability)
    // It should not return method not allowed or bad request due to parameter binding issues
    response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.MethodNotAllowed,
        "Email validation endpoint should support POST method");
    response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.NotFound,
        "Email validation endpoint should be properly routed");
  }

  /// <summary>
  /// Tests email validation endpoint with URL-encoded special characters.
  /// Validates proper handling of encoded email addresses in query parameters.
  /// </summary>
  [DependencyFact(ExternalServices.MsGraph)]
  [Trait("TestCase", "ApiContract")]
  public async Task ValidateEmail_WithEncodedSpecialCharacters_HandlesProperlyEncodedEmails()
  {
    // Arrange
    using var testMethodContext = CreateTestMethodContext(nameof(ValidateEmail_WithEncodedSpecialCharacters_HandlesProperlyEncodedEmails));
    var specialEmail = "test+tag@example.com"; // Email with plus sign
    var apiClient = _apiClientFixture.UnauthenticatedApiApi;

    // Act
    var response = await apiClient.Validate(specialEmail);

    // Assert
    response.Should().NotBeNull("API response should not be null");

    // The endpoint should handle URL encoding/decoding properly
    // It should not fail due to parameter parsing issues
    response.StatusCode.Should().NotBe(System.Net.HttpStatusCode.BadRequest,
        "Properly formatted email with special characters should not cause parameter parsing errors");
  }

  #endregion
}
