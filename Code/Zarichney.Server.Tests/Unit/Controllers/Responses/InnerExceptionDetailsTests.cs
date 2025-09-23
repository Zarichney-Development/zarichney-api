using FluentAssertions;
using Xunit;
using Zarichney.Controllers.Responses;
using Zarichney.Server.Tests.TestData.Builders;

namespace Zarichney.Server.Tests.Unit.Controllers.Responses;

public class InnerExceptionDetailsTests
{
    [Fact]
    [Trait("Category", "Unit")]
    public void InnerExceptionDetails_Construction_CreatesValidInstance()
    {
        // Arrange
        var message = "Database connection failed";
        var type = "SqlException";
        var stackTrace = new List<string> { "at Database.Connect()", "at Service.Execute()" };

        // Act
        var innerException = new InnerExceptionDetails(message, type, stackTrace);

        // Assert
        innerException.Should().NotBeNull();
        innerException.Message.Should().Be(message);
        innerException.Type.Should().Be(type);
        innerException.StackTrace.Should().BeEquivalentTo(stackTrace);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void InnerExceptionDetails_WithoutStackTrace_CreatesValidInstance()
    {
        // Arrange
        var message = "Network timeout occurred";
        var type = "TimeoutException";

        // Act
        var innerException = new InnerExceptionDetails(message, type);

        // Assert
        innerException.Should().NotBeNull();
        innerException.Message.Should().Be(message);
        innerException.Type.Should().Be(type);
        innerException.StackTrace.Should().BeNull();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void InnerExceptionDetails_RecordEquality_WorksCorrectly()
    {
        // Arrange
        var message = "File not found";
        var type = "FileNotFoundException";
        var stackTrace = new List<string> { "at FileReader.Read()" };

        // Act
        var exception1 = new InnerExceptionDetails(message, type, stackTrace);
        var exception2 = new InnerExceptionDetails(message, type, stackTrace);
        var exception3 = new InnerExceptionDetails(message, "IOException", stackTrace);

        // Assert
        exception1.Should().Be(exception2,
            "because records with same values should be equal");
        exception1.Should().NotBe(exception3,
            "because records with different types should not be equal");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void InnerExceptionDetails_Builder_CreatesExpectedInstance()
    {
        // Arrange & Act
        var innerException = new InnerExceptionDetailsBuilder()
            .WithTimeoutException()
            .Build();

        // Assert
        innerException.Should().NotBeNull();
        innerException.Message.Should().Be("The operation has timed out");
        innerException.Type.Should().Be("TimeoutException");
        innerException.StackTrace.Should().NotBeNull();
        innerException.StackTrace.Should().HaveCount(2);
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void InnerExceptionDetails_RecordWith_CreatesModifiedCopy()
    {
        // Arrange
        var original = new InnerExceptionDetailsBuilder()
            .WithDefaults()
            .Build();
        var newMessage = "Modified exception message";

        // Act
        var modified = original with { Message = newMessage };

        // Assert
        modified.Should().NotBe(original,
            "because with expression should create a new instance");
        modified.Message.Should().Be(newMessage);
        modified.Type.Should().Be(original.Type,
            "because other properties should remain the same");
        modified.StackTrace.Should().BeEquivalentTo(original.StackTrace,
            "because other properties should remain the same");
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void InnerExceptionDetails_CommonExceptionTypes_CreateValidInstances()
    {
        // Arrange
        var exceptionTypes = new[]
        {
            ("Null reference error", "NullReferenceException", new List<string> { "at Object.Method()" }),
            ("Invalid operation", "InvalidOperationException", new List<string> { "at Service.Validate()" }),
            ("Argument out of range", "ArgumentOutOfRangeException", null),
            ("Not implemented", "NotImplementedException", new List<string>())
        };

        // Act & Assert
        foreach (var (message, type, stackTrace) in exceptionTypes)
        {
            var innerException = new InnerExceptionDetails(message, type, stackTrace);

            innerException.Message.Should().Be(message);
            innerException.Type.Should().Be(type);
            innerException.StackTrace.Should().BeEquivalentTo(stackTrace);
        }
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void InnerExceptionDetails_EmptyStackTrace_HandledCorrectly()
    {
        // Arrange
        var message = "Generic error";
        var type = "Exception";
        var emptyStackTrace = new List<string>();

        // Act
        var innerException = new InnerExceptionDetails(message, type, emptyStackTrace);

        // Assert
        innerException.StackTrace.Should().NotBeNull();
        innerException.StackTrace.Should().BeEmpty();
    }

    [Fact]
    [Trait("Category", "Unit")]
    public void InnerExceptionDetails_LongMessage_PreservedCompletely()
    {
        // Arrange
        var longMessage = string.Join(" ", Enumerable.Repeat("Error occurred in the system.", 20));
        var type = "SystemException";

        // Act
        var innerException = new InnerExceptionDetails(longMessage, type);

        // Assert
        innerException.Message.Should().Be(longMessage,
            "because long messages should be preserved completely");
        innerException.Message.Length.Should().BeGreaterThan(500);
    }

}