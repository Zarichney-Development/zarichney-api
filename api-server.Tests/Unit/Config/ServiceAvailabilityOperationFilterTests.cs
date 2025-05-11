using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;
using Zarichney.Config;
using Zarichney.Services.Status;

namespace Zarichney.Tests.Unit.Config;

public class ServiceAvailabilityOperationFilterTests
{
  private readonly Mock<IConfigurationStatusService> _mockStatusService;
  private readonly Mock<ILogger<ServiceAvailabilityOperationFilter>> _mockLogger;
  private readonly ServiceAvailabilityOperationFilter _filter;

  public ServiceAvailabilityOperationFilterTests()
  {
    _mockStatusService = new Mock<IConfigurationStatusService>();
    _mockLogger = new Mock<ILogger<ServiceAvailabilityOperationFilter>>();
    _filter = new ServiceAvailabilityOperationFilter(_mockStatusService.Object, _mockLogger.Object);
  }

  [Fact]
  public void Apply_NoRequiresFeatureEnabledAttribute_NoChangesToOperation()
  {
    // Arrange
    var operation = new OpenApiOperation
    {
      Summary = "Original summary",
      Description = "Original description"
    };

    var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MethodWithoutAttribute))!;
    var context = CreateOperationFilterContext(methodInfo);

    // Act
    _filter.Apply(operation, context);

    // Assert
    operation.Summary.Should().Be("Original summary", "Operation without RequiresFeatureEnabled attribute should not have its summary modified");
    operation.Description.Should().Be("Original description", "Operation without RequiresFeatureEnabled attribute should not have its description modified");
  }

  [Fact]
  public void Apply_RequiresFeatureEnabledAttributeButFeatureAvailable_NoChangesToOperation()
  {
    // Arrange
    var operation = new OpenApiOperation
    {
      Summary = "Original summary",
      Description = "Original description"
    };

    var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MethodWithAttribute))!;
    var context = CreateOperationFilterContext(methodInfo);

    // Setup status service to indicate the feature is available
    _mockStatusService.Setup(s => s.GetFeatureStatus("TestFeature"))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    // Act
    _filter.Apply(operation, context);

    // Assert
    operation.Summary.Should().Be("Original summary", "Operation with available feature should not have its summary modified");
    operation.Description.Should().Be("Original description", "Operation with available feature should not have its description modified");
  }

  [Fact]
  public void Apply_RequiresFeatureEnabledAttributeFeatureUnavailable_ModifiesOperation()
  {
    // Arrange
    var operation = new OpenApiOperation
    {
      Summary = "Original summary",
      Description = "Original description"
    };

    var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MethodWithAttribute))!;
    var context = CreateOperationFilterContext(methodInfo);

    // Setup status service to indicate the feature is unavailable
    var missingConfigs = new List<string> { "TestFeature:ApiKey" };
    _mockStatusService.Setup(s => s.GetFeatureStatus("TestFeature"))
        .Returns(new ServiceStatusInfo(IsAvailable: false, missingConfigs));

    // Act
    _filter.Apply(operation, context);

    // Assert
    operation.Summary.Should().StartWith("⚠️", "Operation with unavailable feature should have a warning symbol in the summary");
    operation.Summary.Should().Contain("Original summary", "Original summary should still be present");
    operation.Summary.Should().Contain("TestFeature", "Feature name should be in the warning");
    operation.Summary.Should().Contain("TestFeature:ApiKey", "Missing configuration should be mentioned");

    operation.Description.Should().Contain("**This endpoint is currently unavailable**", "Description should indicate endpoint is unavailable");
    operation.Description.Should().Contain("Original description", "Original description should still be present");
    operation.Description.Should().Contain("TestFeature", "Feature name should be in the description");
    operation.Description.Should().Contain("TestFeature:ApiKey", "Missing configuration should be mentioned");
  }

  [Fact]
  public void Apply_MultipleFeatures_AllUnavailable_ModifiesOperationWithAllFeatures()
  {
    // Arrange
    var operation = new OpenApiOperation
    {
      Summary = "Original summary",
      Description = "Original description"
    };

    var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MethodWithMultipleAttributes))!;
    var context = CreateOperationFilterContext(methodInfo);

    // Setup status service to indicate both features are unavailable
    _mockStatusService.Setup(s => s.GetFeatureStatus("Feature1"))
        .Returns(new ServiceStatusInfo(IsAvailable: false, ["Feature1:ApiKey"]));

    _mockStatusService.Setup(s => s.GetFeatureStatus("Feature2"))
        .Returns(new ServiceStatusInfo(IsAvailable: false, ["Feature2:Secret"]));

    // Act
    _filter.Apply(operation, context);

    // Assert
    operation.Summary.Should().StartWith("⚠️", "Operation with unavailable features should have a warning symbol");
    operation.Summary.Should().Contain("Feature1", "First feature name should be in the warning");
    operation.Summary.Should().Contain("Feature2", "Second feature name should be in the warning");
    operation.Summary.Should().Contain("Feature1:ApiKey", "First missing config should be mentioned");
    operation.Summary.Should().Contain("Feature2:Secret", "Second missing config should be mentioned");
  }

  [Fact]
  public void Apply_MultipleFeatures_OneUnavailable_ModifiesOperationWithOnlyUnavailableFeature()
  {
    // Arrange
    var operation = new OpenApiOperation
    {
      Summary = "Original summary",
      Description = "Original description"
    };

    var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MethodWithMultipleAttributes))!;
    var context = CreateOperationFilterContext(methodInfo);

    // Setup status service to indicate one feature is available and one is unavailable
    _mockStatusService.Setup(s => s.GetFeatureStatus("Feature1"))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    _mockStatusService.Setup(s => s.GetFeatureStatus("Feature2"))
        .Returns(new ServiceStatusInfo(IsAvailable: false, ["Feature2:Secret"]));

    // Act
    _filter.Apply(operation, context);

    // Assert
    operation.Summary.Should().StartWith("⚠️", "Operation with unavailable feature should have a warning symbol");
    operation.Summary.Should().NotContain("Feature1", "Available feature should not be in warning");
    operation.Summary.Should().Contain("Feature2", "Unavailable feature should be in warning");
    operation.Summary.Should().Contain("Feature2:Secret", "Missing config should be mentioned");
  }

  [Fact]
  public void Apply_ClassLevelAttribute_ModifiesOperation()
  {
    // Arrange
    var operation = new OpenApiOperation
    {
      Summary = "Original summary",
      Description = "Original description"
    };

    var methodInfo = typeof(ClassWithAttribute).GetMethod(nameof(ClassWithAttribute.MethodWithoutAttribute))!;
    var context = CreateOperationFilterContext(methodInfo);

    // Setup status service to indicate the feature is unavailable
    _mockStatusService.Setup(s => s.GetFeatureStatus("ClassLevelFeature"))
        .Returns(new ServiceStatusInfo(IsAvailable: false, ["ClassLevelFeature:Setting"]));

    // Act
    _filter.Apply(operation, context);

    // Assert
    operation.Summary.Should().StartWith("⚠️", "Operation with class-level unavailable feature should have a warning symbol");
    operation.Summary.Should().Contain("ClassLevelFeature", "Class-level feature name should be in the warning");
  }

  [Fact]
  public void Apply_BothClassAndMethodLevelAttributes_CombinesAll()
  {
    // Arrange
    var operation = new OpenApiOperation
    {
      Summary = "Original summary",
      Description = "Original description"
    };

    var methodInfo = typeof(ClassWithAttribute).GetMethod(nameof(ClassWithAttribute.MethodWithAttribute))!;
    var context = CreateOperationFilterContext(methodInfo);

    // Setup status service to indicate both features are unavailable
    _mockStatusService.Setup(s => s.GetFeatureStatus("ClassLevelFeature"))
        .Returns(new ServiceStatusInfo(IsAvailable: false, ["ClassLevelFeature:Setting"]));

    _mockStatusService.Setup(s => s.GetFeatureStatus("MethodLevelFeature"))
        .Returns(new ServiceStatusInfo(IsAvailable: false, ["MethodLevelFeature:ApiKey"]));

    // Act
    _filter.Apply(operation, context);

    // Assert
    operation.Summary.Should().StartWith("⚠️", "Operation with unavailable features should have a warning symbol");
    operation.Summary.Should().Contain("ClassLevelFeature", "Class-level feature name should be in the warning");
    operation.Summary.Should().Contain("MethodLevelFeature", "Method-level feature name should be in the warning");
  }

  [Fact]
  public void Apply_NullOperation_DoesNotThrow()
  {
    // Arrange
    OpenApiOperation? operation = null;
    var methodInfo = typeof(TestController).GetMethod(nameof(TestController.MethodWithAttribute))!;
    var context = CreateOperationFilterContext(methodInfo);

    // Act & Assert
    _filter.Invoking(f => f.Apply(operation!, context))
           .Should().NotThrow("The filter should handle null operation gracefully");
  }

  [Fact]
  public void Apply_NullContext_DoesNotThrow()
  {
    // Arrange
    var operation = new OpenApiOperation();
    OperationFilterContext? context = null;

    // Act & Assert
    _filter.Invoking(f => f.Apply(operation, context!))
           .Should().NotThrow("The filter should handle null context gracefully");
  }

  private static OperationFilterContext? CreateOperationFilterContext(MethodInfo methodInfo)
  {
    // Simple mock of OperationFilterContext that contains the method info
    var apiDescription = new Mock<Microsoft.AspNetCore.Mvc.ApiExplorer.ApiDescription>();

    // Create SchemaRepository directly instead of using Mock.Of since it doesn't have a parameterless constructor
    var schemaRepository = new SchemaRepository();

    return new OperationFilterContext(
        apiDescription.Object,
        Mock.Of<ISchemaGenerator>(),
        schemaRepository,
        methodInfo
    );
  }

  // Test classes for attribute testing
  private class TestController
  {
    public void MethodWithoutAttribute() { }

    [RequiresFeatureEnabled("TestFeature")]
    public void MethodWithAttribute() { }

    [RequiresFeatureEnabled("Feature1")]
    [RequiresFeatureEnabled("Feature2")]
    public void MethodWithMultipleAttributes() { }
  }

  [RequiresFeatureEnabled("ClassLevelFeature")]
  private class ClassWithAttribute
  {
    public void MethodWithoutAttribute() { }

    [RequiresFeatureEnabled("MethodLevelFeature")]
    public void MethodWithAttribute() { }
  }
}
