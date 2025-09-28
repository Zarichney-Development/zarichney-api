using FluentAssertions;
using Xunit;
using Zarichney.Controllers.Responses;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Controllers.Responses;

public class ApiErrorResponseTests
{
  [Fact]
  [Trait("Category", "Unit")]
  public void ApiErrorResponse_Construction_CreatesValidResponse()
  {
    // Arrange
    var error = new ErrorDetailsBuilder().WithDefaults().Build();
    var request = new RequestDetailsBuilder().WithDefaults().Build();
    var traceId = "trace-123";
    var innerException = new InnerExceptionDetailsBuilder().WithDefaults().Build();

    // Act
    var response = new ApiErrorResponse(error, request, traceId, innerException);

    // Assert
    response.Should().NotBeNull();
    response.Error.Should().Be(error);
    response.Request.Should().Be(request);
    response.TraceId.Should().Be(traceId);
    response.InnerException.Should().Be(innerException);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ApiErrorResponse_WithoutInnerException_CreatesValidResponse()
  {
    // Arrange
    var error = new ErrorDetailsBuilder().WithDefaults().Build();
    var request = new RequestDetailsBuilder().WithDefaults().Build();
    var traceId = "trace-456";

    // Act
    var response = new ApiErrorResponse(error, request, traceId);

    // Assert
    response.Should().NotBeNull();
    response.Error.Should().Be(error);
    response.Request.Should().Be(request);
    response.TraceId.Should().Be(traceId);
    response.InnerException.Should().BeNull();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ApiErrorResponse_RecordEquality_WorksCorrectly()
  {
    // Arrange
    var error = new ErrorDetailsBuilder().WithDefaults().Build();
    var request = new RequestDetailsBuilder().WithDefaults().Build();
    var traceId = "trace-789";
    var innerException = new InnerExceptionDetailsBuilder().WithDefaults().Build();

    // Act
    var response1 = new ApiErrorResponse(error, request, traceId, innerException);
    var response2 = new ApiErrorResponse(error, request, traceId, innerException);
    var response3 = new ApiErrorResponse(error, request, "different-trace", innerException);

    // Assert
    response1.Should().Be(response2);
    response1.Should().NotBe(response3);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ApiErrorResponse_Builder_CreatesExpectedResponse()
  {
    // Arrange & Act
    var response = new ApiErrorResponseBuilder()
        .WithDefaults()
        .WithServerError()
        .Build();

    // Assert
    response.Should().NotBeNull();
    response.Error.Message.Should().Be("Internal server error occurred");
    response.Error.Type.Should().Be("InvalidOperationException");
    response.Request.Should().NotBeNull();
    response.TraceId.Should().Be("test-trace-id");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void ApiErrorResponse_RecordWith_CreatesModifiedCopy()
  {
    // Arrange
    var original = new ApiErrorResponseBuilder().WithDefaults().Build();
    var newTraceId = "modified-trace";

    // Act
    var modified = original with { TraceId = newTraceId };

    // Assert
    modified.Should().NotBe(original);
    modified.TraceId.Should().Be(newTraceId);
    modified.Error.Should().Be(original.Error,
        "because other properties should remain the same");
    modified.Request.Should().Be(original.Request,
        "because other properties should remain the same");
  }
}
