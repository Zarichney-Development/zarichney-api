using FluentAssertions;
using Xunit;
using Zarichney.Config;

namespace Zarichney.Tests.Unit.Config;

public class ServiceUnavailableExceptionTests
{
  [Trait("Category", "Unit")]
  [Fact]
  public void Constructor_WithMessage_SetsMessageProperty()
  {
    // Arrange
    var message = "Service is unavailable";

    // Act
    var exception = new ServiceUnavailableException(message);

    // Assert
    exception.Message.Should().Be(message);
    exception.Reasons.Should().BeEmpty();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void Constructor_WithMessageAndReasons_SetsMessageAndReasonsProperties()
  {
    // Arrange
    var message = "Service is unavailable";
    var reasons = new List<string> { "Config A is missing", "Config B is invalid" };

    // Act
    var exception = new ServiceUnavailableException(message, reasons);

    // Assert
    exception.Message.Should().Be(message);
    exception.Reasons.Should().BeEquivalentTo(reasons);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void Constructor_WithInnerException_SetsInnerExceptionProperty()
  {
    // Arrange
    var message = "Service is unavailable";
    var innerException = new InvalidOperationException("Inner exception");

    // Act
    var exception = new ServiceUnavailableException(message, innerException);

    // Assert
    exception.Message.Should().Be(message);
    exception.InnerException.Should().Be(innerException);
    exception.Reasons.Should().BeEmpty();
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void Constructor_WithMessageReasonsAndInnerException_SetsAllProperties()
  {
    // Arrange
    var message = "Service is unavailable";
    var reasons = new List<string> { "Config A is missing", "Config B is invalid" };
    var innerException = new InvalidOperationException("Inner exception");

    // Act
    var exception = new ServiceUnavailableException(message, reasons, innerException);

    // Assert
    exception.Message.Should().Be(message);
    exception.InnerException.Should().Be(innerException);
    exception.Reasons.Should().BeEquivalentTo(reasons);
  }

  [Trait("Category", "Unit")]
  [Fact]
  public void AddReason_WithValidReason_AddsReasonToCollection()
  {
    // Arrange
    var exception = new ServiceUnavailableException("Service is unavailable");
    var reason = "Config C is missing";

    // Act
    exception.AddReason(reason);

    // Assert
    exception.Reasons.Should().Contain(reason);
  }
}
