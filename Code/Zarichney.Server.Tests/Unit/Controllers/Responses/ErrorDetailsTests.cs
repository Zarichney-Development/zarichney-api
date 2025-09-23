using FluentAssertions;
using Xunit;
using Zarichney.Controllers.Responses;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Server.Tests.Unit.Controllers.Responses;

public class ErrorDetailsTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void ErrorDetails_Construction_CreatesValidInstance()
    {
        // Arrange
        var message = "Test error message";
        var type = "TestException";
        var details = "Detailed error information";
        var source = "TestSource";
        var stackTrace = new List<string> { "at Line 1", "at Line 2" };

        // Act
        var errorDetails = new ErrorDetails(message, type, details, source, stackTrace);

        // Assert
        errorDetails.Should().NotBeNull();
        errorDetails.Message.Should().Be(message);
        errorDetails.Type.Should().Be(type);
        errorDetails.Details.Should().Be(details);
        errorDetails.Source.Should().Be(source);
        errorDetails.StackTrace.Should().BeEquivalentTo(stackTrace);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ErrorDetails_MinimalConstruction_CreatesValidInstance()
    {
        // Arrange
        var message = "Simple error";

        // Act
        var errorDetails = new ErrorDetails(message);

        // Assert
        errorDetails.Should().NotBeNull();
        errorDetails.Message.Should().Be(message);
        errorDetails.Type.Should().BeNull(
            "because type is optional and not provided");
        errorDetails.Details.Should().BeNull(
            "because details is optional and not provided");
        errorDetails.Source.Should().BeNull(
            "because source is optional and not provided");
        errorDetails.StackTrace.Should().BeNull(
            "because stack trace is optional and not provided");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ErrorDetails_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var message = "Error occurred";
        var type = "Exception";

        // Act
        var error1 = new ErrorDetails(message, type);
        var error2 = new ErrorDetails(message, type);
        var error3 = new ErrorDetails("Different message", type);

        // Assert
        error1.Should().Be(error2,
            "because records with same values should be equal");
        error1.Should().NotBe(error3,
            "because records with different messages should not be equal");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ErrorDetails_WithStackTrace_HandlesEmptyList()
    {
        // Arrange
        var message = "Error with empty stack";
        var emptyStackTrace = new List<string>();

        // Act
        var errorDetails = new ErrorDetails(message, StackTrace: emptyStackTrace);

        // Assert
        errorDetails.StackTrace.Should().NotBeNull();
        errorDetails.StackTrace.Should().BeEmpty(
            "because empty stack trace list should be preserved");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ErrorDetails_Builder_CreatesExpectedInstance()
    {
        // Arrange & Act
        var errorDetails = new ErrorDetailsBuilder()
            .WithValidationError()
            .Build();

        // Assert
        errorDetails.Should().NotBeNull();
        errorDetails.Message.Should().Be("Validation failed");
        errorDetails.Type.Should().Be("ArgumentException");
        errorDetails.Details.Should().Be("The provided value is invalid");
        errorDetails.Source.Should().Be("ValidationService");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void ErrorDetails_RecordWith_CreatesModifiedCopy()
    {
        // Arrange
        var original = new ErrorDetailsBuilder().WithDefaults().Build();
        var newMessage = "Modified error message";

        // Act
        var modified = original with { Message = newMessage };

        // Assert
        modified.Should().NotBe(original,
            "because with expression should create a new instance");
        modified.Message.Should().Be(newMessage);
        modified.Type.Should().Be(original.Type,
            "because other properties should remain the same");
        modified.Details.Should().Be(original.Details);
        modified.Source.Should().Be(original.Source);
        modified.StackTrace.Should().BeEquivalentTo(original.StackTrace);
    }

}