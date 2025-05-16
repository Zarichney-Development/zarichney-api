using Zarichney.Services.Status;
using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using Xunit;

namespace Zarichney.Tests.Unit.Config;

public class ServiceAvailabilityOperationFilterTests
{
  private readonly Mock<IStatusService> _mockStatusService;
  private readonly Mock<ILogger<ServiceAvailabilityOperationFilter>> _mockLogger;
  private readonly ServiceAvailabilityOperationFilter _filter;

  public ServiceAvailabilityOperationFilterTests()
  {
    _mockStatusService = new Mock<IStatusService>();
    _mockLogger = new Mock<ILogger<ServiceAvailabilityOperationFilter>>();
    _filter = new ServiceAvailabilityOperationFilter(_mockStatusService.Object, _mockLogger.Object);
  }

  [Fact]
  public void Apply_NoDependsOnServiceAttribute_NoChangesToOperation()
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
    operation.Summary.Should().Be("Original summary", "Operation without DependsOnService attribute should not have its summary modified");
    operation.Description.Should().Be("Original description", "Operation without DependsOnService attribute should not have its description modified");
  }

  [Fact]
  public void Apply_DependsOnServiceAttributeButFeatureAvailable_NoChangesToOperation()
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
    _mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.OpenAiApi))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    // Act
    _filter.Apply(operation, context);

    // Assert
    operation.Summary.Should().Be("Original summary", "Operation with available feature should not have its summary modified");
    operation.Description.Should().Be("Original description", "Operation with available feature should not have its description modified");
  }

  [Fact]
  public void Apply_DependsOnServiceAttributeFeatureUnavailable_ModifiesOperation()
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
    var missingConfigs = new List<string> { "LlmConfig:ApiKey" };
    _mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.OpenAiApi))
        .Returns(new ServiceStatusInfo(IsAvailable: false, missingConfigs));

    // Act
    _filter.Apply(operation, context);

    // Assert
    operation.Summary.Should().StartWith("⚠️", "Operation with unavailable feature should have a warning symbol in the summary");
    operation.Summary.Should().Contain("Original summary", "Original summary should still be present");
    operation.Summary.Should().Contain("LlmConfig:ApiKey", "Missing configuration should be mentioned");

    operation.Description.Should().Contain("**This endpoint is currently unavailable**", "Description should indicate endpoint is unavailable");
    operation.Description.Should().Contain("Original description", "Original description should still be present");
    operation.Description.Should().Contain("LlmConfig:ApiKey", "Missing configuration should be mentioned");
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
    _mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.OpenAiApi))
        .Returns(new ServiceStatusInfo(IsAvailable: false, ["LlmConfig:ApiKey"]));

    _mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.EmailValidation))
        .Returns(new ServiceStatusInfo(IsAvailable: false, ["EmailConfig:MailCheckApiKey"]));

    // Act
    _filter.Apply(operation, context);

    // Assert
    operation.Summary.Should().StartWith("⚠️", "Operation with unavailable features should have a warning symbol");
    operation.Summary.Should().Contain("OpenAiApi", "First feature name should be in the warning");
    operation.Summary.Should().Contain("EmailValidation", "Second feature name should be in the warning");
    operation.Summary.Should().Contain("LlmConfig:ApiKey", "First missing config should be mentioned");
    operation.Summary.Should().Contain("EmailConfig:MailCheckApiKey", "Second missing config should be mentioned");
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
    _mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.OpenAiApi))
        .Returns(new ServiceStatusInfo(IsAvailable: true, []));

    _mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.EmailValidation))
        .Returns(new ServiceStatusInfo(IsAvailable: false, ["EmailConfig:MailCheckApiKey"]));

    // Act
    _filter.Apply(operation, context);

    // Assert
    operation.Summary.Should().StartWith("⚠️", "Operation with unavailable feature should have a warning symbol");
    operation.Summary.Should().NotContain("OpenAiApi", "Available feature should not be in warning");
    operation.Summary.Should().Contain("EmailValidation", "Unavailable feature should be in warning");
    operation.Summary.Should().Contain("EmailConfig:MailCheckApiKey", "Missing config should be mentioned");
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
    _mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.FrontEnd))
        .Returns(new ServiceStatusInfo(IsAvailable: false, ["ServerConfig:BaseUrl"]));

    // Act
    _filter.Apply(operation, context);

    // Assert
    operation.Summary.Should().StartWith("⚠️", "Operation with class-level unavailable feature should have a warning symbol");
    // todo update to not be so brittle, refer to enum ExternalServices
    operation.Summary.Should().Contain("FrontEnd", "Class-level feature name should be in the warning");
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
    _mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.FrontEnd))
        .Returns(new ServiceStatusInfo(IsAvailable: false, ["ServerConfig:BaseUrl"]));

    _mockStatusService.Setup(s => s.GetFeatureStatus(ExternalServices.EmailSending))
        .Returns(new ServiceStatusInfo(IsAvailable: false, ["EmailConfig:AzureAppId"]));

    // Act
    _filter.Apply(operation, context);

    // Assert
    operation.Summary.Should().StartWith("⚠️", "Operation with unavailable features should have a warning symbol");
    // todo update to not be so brittle, refer to enum ExternalServices
    operation.Summary.Should().Contain("FrontEnd", "Class-level feature name should be in the warning");
    operation.Summary.Should().Contain("EmailSending", "Method-level feature name should be in the warning");
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

    [DependsOnService(ExternalServices.OpenAiApi)]
    public void MethodWithAttribute() { }

    [DependsOnService(ExternalServices.OpenAiApi)]
    [DependsOnService(ExternalServices.EmailValidation)]
    public void MethodWithMultipleAttributes() { }
  }

  [DependsOnService(ExternalServices.FrontEnd)]
  private class ClassWithAttribute
  {
    public void MethodWithoutAttribute() { }

    [DependsOnService(ExternalServices.EmailSending)]
    public void MethodWithAttribute() { }
  }
}
