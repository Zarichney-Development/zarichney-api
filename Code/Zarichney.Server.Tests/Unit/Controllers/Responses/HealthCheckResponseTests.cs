using FluentAssertions;
using System.Text.Json;
using Xunit;
using Zarichney.Controllers.Responses;

namespace Zarichney.Server.Tests.Unit.Controllers.Responses;

/// <summary>
/// Unit tests for HealthCheckResponse record - tests record behavior, equality, and serialization
/// </summary>
[Trait("Category", "Unit")]
[Trait("Component", "Controllers")]
public class HealthCheckResponseTests
{
  #region Constructor Tests

  [Fact]
  public void Constructor_WithAllParameters_CreatesResponseCorrectly()
  {
    // Arrange
    var success = true;
    var time = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
    var environment = "Production";

    // Act
    var response = new HealthCheckResponse(success, time, environment);

    // Assert
    response.Should().NotBeNull("because the response should be created");
    response.Success.Should().BeTrue("because success was set to true");
    response.Time.Should().Be(time, "because the time should match the provided value");
    response.Environment.Should().Be(environment, "because the environment should match");
  }

  [Fact]
  public void Constructor_WithFailureStatus_CreatesResponseCorrectly()
  {
    // Arrange
    var success = false;
    var time = DateTime.UtcNow;
    var environment = "Development";

    // Act
    var response = new HealthCheckResponse(success, time, environment);

    // Assert
    response.Success.Should().BeFalse("because success was set to false");
    response.Time.Should().BeCloseTo(time, TimeSpan.FromSeconds(1),
        "because the time should be close to the provided value");
    response.Environment.Should().Be(environment, "because the environment should match");
  }

  #endregion

  #region Record Equality Tests

  [Fact]
  public void Equals_WithIdenticalValues_ReturnsTrue()
  {
    // Arrange
    var time = DateTime.UtcNow;
    var response1 = new HealthCheckResponse(true, time, "Production");
    var response2 = new HealthCheckResponse(true, time, "Production");

    // Act
    var areEqual = response1.Equals(response2);

    // Assert
    areEqual.Should().BeTrue("because records with identical values should be equal");
    (response1 == response2).Should().BeTrue("because == operator should work for records");
  }

  [Fact]
  public void Equals_WithDifferentSuccess_ReturnsFalse()
  {
    // Arrange
    var time = DateTime.UtcNow;
    var response1 = new HealthCheckResponse(true, time, "Production");
    var response2 = new HealthCheckResponse(false, time, "Production");

    // Act
    var areEqual = response1.Equals(response2);

    // Assert
    areEqual.Should().BeFalse("because records with different success values should not be equal");
    (response1 != response2).Should().BeTrue("because != operator should work for records");
  }

  [Fact]
  public void Equals_WithDifferentTime_ReturnsFalse()
  {
    // Arrange
    var response1 = new HealthCheckResponse(true, DateTime.UtcNow, "Production");
    var response2 = new HealthCheckResponse(true, DateTime.UtcNow.AddSeconds(1), "Production");

    // Act
    var areEqual = response1.Equals(response2);

    // Assert
    areEqual.Should().BeFalse("because records with different times should not be equal");
  }

  [Fact]
  public void Equals_WithDifferentEnvironment_ReturnsFalse()
  {
    // Arrange
    var time = DateTime.UtcNow;
    var response1 = new HealthCheckResponse(true, time, "Production");
    var response2 = new HealthCheckResponse(true, time, "Development");

    // Act
    var areEqual = response1.Equals(response2);

    // Assert
    areEqual.Should().BeFalse("because records with different environments should not be equal");
  }

  #endregion

  #region GetHashCode Tests

  [Fact]
  public void GetHashCode_WithIdenticalValues_ReturnsSameHashCode()
  {
    // Arrange
    var time = DateTime.UtcNow;
    var response1 = new HealthCheckResponse(true, time, "Production");
    var response2 = new HealthCheckResponse(true, time, "Production");

    // Act
    var hash1 = response1.GetHashCode();
    var hash2 = response2.GetHashCode();

    // Assert
    hash1.Should().Be(hash2, "because identical records should have the same hash code");
  }

  [Fact]
  public void GetHashCode_WithDifferentValues_UsuallyReturnsDifferentHashCode()
  {
    // Arrange
    var response1 = new HealthCheckResponse(true, DateTime.UtcNow, "Production");
    var response2 = new HealthCheckResponse(false, DateTime.UtcNow.AddHours(1), "Development");

    // Act
    var hash1 = response1.GetHashCode();
    var hash2 = response2.GetHashCode();

    // Assert
    hash1.Should().NotBe(hash2, "because different records should usually have different hash codes");
  }

  #endregion

  #region ToString Tests

  [Fact]
  public void ToString_ReturnsReadableRepresentation()
  {
    // Arrange
    var time = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
    var response = new HealthCheckResponse(true, time, "Production");

    // Act
    var result = response.ToString();

    // Assert
    result.Should().NotBeNullOrEmpty("because ToString should return a value");
    result.Should().Contain("Success = True", "because it should include the Success property");
    result.Should().Contain("Environment = Production", "because it should include the Environment");
    result.Should().Contain("2024", "because it should include part of the time");
  }

  #endregion

  #region With Expression Tests (Record Feature)

  [Fact]
  public void With_ModifyingSuccess_CreatesNewInstanceWithChangedValue()
  {
    // Arrange
    var original = new HealthCheckResponse(true, DateTime.UtcNow, "Production");

    // Act
    var modified = original with { Success = false };

    // Assert
    modified.Success.Should().BeFalse("because Success was modified");
    modified.Time.Should().Be(original.Time, "because Time should remain unchanged");
    modified.Environment.Should().Be(original.Environment, "because Environment should remain unchanged");
    original.Success.Should().BeTrue("because the original should remain unchanged");
  }

  [Fact]
  public void With_ModifyingEnvironment_CreatesNewInstanceWithChangedValue()
  {
    // Arrange
    var original = new HealthCheckResponse(true, DateTime.UtcNow, "Production");

    // Act
    var modified = original with { Environment = "Staging" };

    // Assert
    modified.Environment.Should().Be("Staging", "because Environment was modified");
    modified.Success.Should().Be(original.Success, "because Success should remain unchanged");
    modified.Time.Should().Be(original.Time, "because Time should remain unchanged");
    original.Environment.Should().Be("Production", "because the original should remain unchanged");
  }

  #endregion

  #region JSON Serialization Tests

  [Fact]
  public void JsonSerialize_ProducesCorrectJson()
  {
    // Arrange
    var time = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
    var response = new HealthCheckResponse(true, time, "Production");

    // Act
    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    // Assert
    json.Should().Contain("\"success\":true", "because Success should be serialized as camelCase");
    json.Should().Contain("\"environment\":\"Production\"", "because Environment should be included");
    json.Should().Contain("\"time\":\"2024-01-15T10:30:45", "because Time should be in ISO format");
  }

  [Fact]
  public void JsonDeserialize_ReconstructsObjectCorrectly()
  {
    // Arrange
    var original = new HealthCheckResponse(true, DateTime.UtcNow, "Development");
    var json = JsonSerializer.Serialize(original);

    // Act
    var deserialized = JsonSerializer.Deserialize<HealthCheckResponse>(json);

    // Assert
    deserialized.Should().NotBeNull("because deserialization should succeed");
    deserialized!.Success.Should().Be(original.Success, "because Success should match");
    deserialized.Time.Should().BeCloseTo(original.Time, TimeSpan.FromMilliseconds(1),
        "because Time should be preserved through serialization");
    deserialized.Environment.Should().Be(original.Environment, "because Environment should match");
  }

  #endregion

  #region Edge Cases and Boundary Tests

  [Fact]
  public void Constructor_WithMinDateTime_HandlesCorrectly()
  {
    // Arrange & Act
    var response = new HealthCheckResponse(true, DateTime.MinValue, "Test");

    // Assert
    response.Time.Should().Be(DateTime.MinValue, "because minimum DateTime should be handled");
  }

  [Fact]
  public void Constructor_WithMaxDateTime_HandlesCorrectly()
  {
    // Arrange & Act
    var response = new HealthCheckResponse(true, DateTime.MaxValue, "Test");

    // Assert
    response.Time.Should().Be(DateTime.MaxValue, "because maximum DateTime should be handled");
  }

  [Fact]
  public void Constructor_WithEmptyEnvironment_HandlesCorrectly()
  {
    // Arrange & Act
    var response = new HealthCheckResponse(true, DateTime.UtcNow, string.Empty);

    // Assert
    response.Environment.Should().BeEmpty("because empty environment string should be allowed");
  }

  [Fact]
  public void Constructor_WithVeryLongEnvironment_HandlesCorrectly()
  {
    // Arrange
    var longEnvironment = new string('x', 1000);

    // Act
    var response = new HealthCheckResponse(true, DateTime.UtcNow, longEnvironment);

    // Assert
    response.Environment.Should().Be(longEnvironment, "because long strings should be handled");
    response.Environment.Length.Should().Be(1000, "because the full string should be preserved");
  }

  #endregion

  #region Different Environment Scenarios

  [Theory]
  [InlineData("Development")]
  [InlineData("Staging")]
  [InlineData("Production")]
  [InlineData("Testing")]
  [InlineData("Local")]
  public void Constructor_WithVariousEnvironments_CreatesCorrectly(string environment)
  {
    // Act
    var response = new HealthCheckResponse(true, DateTime.UtcNow, environment);

    // Assert
    response.Environment.Should().Be(environment,
        $"because the environment '{environment}' should be set correctly");
  }

  #endregion
}
