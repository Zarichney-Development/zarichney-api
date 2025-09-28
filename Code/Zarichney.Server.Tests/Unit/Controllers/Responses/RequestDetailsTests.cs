using FluentAssertions;
using Xunit;
using Zarichney.Controllers.Responses;
using Zarichney.Tests.TestData.Builders;

namespace Zarichney.Tests.Unit.Controllers.Responses;

public class RequestDetailsTests
{
  [Fact]
  [Trait("Category", "Unit")]
  public void RequestDetails_Construction_CreatesValidInstance()
  {
    // Arrange
    var path = "/api/users";
    var method = "POST";
    var controller = "UsersController.CreateUser";

    // Act
    var requestDetails = new RequestDetails(path, method, controller);

    // Assert
    requestDetails.Should().NotBeNull();
    requestDetails.Path.Should().Be(path);
    requestDetails.Method.Should().Be(method);
    requestDetails.Controller.Should().Be(controller);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void RequestDetails_WithoutController_CreatesValidInstance()
  {
    // Arrange
    var path = "/api/health";
    var method = "GET";

    // Act
    var requestDetails = new RequestDetails(path, method, null);

    // Assert
    requestDetails.Should().NotBeNull();
    requestDetails.Path.Should().Be(path);
    requestDetails.Method.Should().Be(method);
    requestDetails.Controller.Should().BeNull();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void RequestDetails_WithNullPath_CreatesValidInstance()
  {
    // Arrange
    var method = "HEAD";
    var controller = "HealthController.Ping";

    // Act
    var requestDetails = new RequestDetails(null, method, controller);

    // Assert
    requestDetails.Should().NotBeNull();
    requestDetails.Path.Should().BeNull();
    requestDetails.Method.Should().Be(method);
    requestDetails.Controller.Should().Be(controller);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void RequestDetails_RecordEquality_WorksCorrectly()
  {
    // Arrange
    var path = "/api/products/123";
    var method = "PUT";
    var controller = "ProductsController.UpdateProduct";

    // Act
    var request1 = new RequestDetails(path, method, controller);
    var request2 = new RequestDetails(path, method, controller);
    var request3 = new RequestDetails(path, "DELETE", controller);

    // Assert
    request1.Should().Be(request2,
        "because records with same values should be equal");
    request1.Should().NotBe(request3,
        "because records with different methods should not be equal");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void RequestDetails_Builder_CreatesExpectedInstance()
  {
    // Arrange & Act
    var requestDetails = new RequestDetailsBuilder()
        .WithPostRequest("/api/orders")
        .WithController("OrdersController.CreateOrder")
        .Build();

    // Assert
    requestDetails.Should().NotBeNull();
    requestDetails.Path.Should().Be("/api/orders");
    requestDetails.Method.Should().Be("POST");
    requestDetails.Controller.Should().Be("OrdersController.CreateOrder");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void RequestDetails_AllHttpMethods_CreateValidInstances()
  {
    // Arrange
    var httpMethods = new[] { "GET", "POST", "PUT", "DELETE", "PATCH", "HEAD", "OPTIONS" };
    var path = "/api/test";

    // Act & Assert
    foreach (var method in httpMethods)
    {
      var requestDetails = new RequestDetails(path, method, null);

      requestDetails.Method.Should().Be(method,
          $"because RequestDetails should support {method} HTTP method");
    }
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void RequestDetails_RecordWith_CreatesModifiedCopy()
  {
    // Arrange
    var original = new RequestDetailsBuilder()
        .WithGetRequest("/api/users")
        .WithController("UsersController.GetUsers")
        .Build();
    var newMethod = "POST";

    // Act
    var modified = original with { Method = newMethod };

    // Assert
    modified.Should().NotBe(original,
        "because with expression should create a new instance");
    modified.Method.Should().Be(newMethod);
    modified.Path.Should().Be(original.Path,
        "because other properties should remain the same");
    modified.Controller.Should().Be(original.Controller,
        "because other properties should remain the same");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void RequestDetails_ComplexPath_PreservedCorrectly()
  {
    // Arrange
    var complexPath = "/api/v2/users/{id}/orders/{orderId}/items";
    var method = "GET";
    var controller = "OrderItemsController.GetUserOrderItems";

    // Act
    var requestDetails = new RequestDetails(complexPath, method, controller);

    // Assert
    requestDetails.Path.Should().Be(complexPath,
        "because complex paths with multiple segments should be preserved");
    requestDetails.Controller.Should().Be(controller);
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void RequestDetails_BuilderWithDefaults_SetsExpectedValues()
  {
    // Arrange & Act
    var requestDetails = new RequestDetailsBuilder()
        .WithDefaults()
        .Build();

    // Assert
    requestDetails.Path.Should().Be("/api/email/validate");
    requestDetails.Method.Should().Be("POST");
    requestDetails.Controller.Should().Be("ApiController.ValidateEmail");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void RequestDetails_BuilderWithCompleteContext_SetsAllFields()
  {
    // Arrange & Act
    var requestDetails = new RequestDetailsBuilder()
        .WithCompleteContext()
        .Build();

    // Assert
    requestDetails.Path.Should().Be("/api/users/123");
    requestDetails.Method.Should().Be("PUT");
    requestDetails.Controller.Should().Be("UsersController.UpdateUser");
  }
}
