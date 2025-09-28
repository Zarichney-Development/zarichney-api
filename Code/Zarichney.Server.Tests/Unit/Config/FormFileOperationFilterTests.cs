using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;
using Xunit;
using Zarichney.Config;
using Zarichney.Tests.Framework.Mocks.Factories;

namespace Zarichney.Tests.Unit.Config;

public class FormFileOperationFilterTests
{
  private readonly FormFileOperationFilter _sut;

  public FormFileOperationFilterTests()
  {
    _sut = new FormFileOperationFilter();
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Apply_NoFormFileParameters_DoesNotModifyOperation()
  {
    // Arrange
    var operation = SwaggerMockFactory.CreateOperation(includeParameters: true);
    var context = CreateContextWithoutFormFile();
    var originalParameterCount = operation.Parameters.Count;

    // Act
    _sut.Apply(operation, context);

    // Assert
    operation.Parameters.Count.Should().Be(originalParameterCount,
        "because the operation should not be modified when no IFormFile parameters exist");
    operation.RequestBody.Should().BeNull(
        "because no request body should be created for operations without form files");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Apply_WithFormFileParameter_CreatesMultipartFormDataRequestBody()
  {
    // Arrange
    var operation = new OpenApiOperation();
    var context = CreateContextWithFormFile();

    // Act
    _sut.Apply(operation, context);

    // Assert
    operation.RequestBody.Should().NotBeNull(
        "because a request body should be created for form file uploads");
    operation.RequestBody.Content.Should().ContainKey("multipart/form-data",
        "because file uploads use multipart/form-data content type");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Apply_WithFormFileParameter_SetsCorrectSchemaTypeAndFormat()
  {
    // Arrange
    var operation = new OpenApiOperation();
    var context = CreateContextWithFormFile();

    // Act
    _sut.Apply(operation, context);

    // Assert
    var mediaType = operation.RequestBody.Content["multipart/form-data"];
    mediaType.Schema.Should().NotBeNull("because a schema should be created");
    mediaType.Schema.Type.Should().Be("object", "because form data is represented as an object");
    mediaType.Schema.Properties.Should().ContainKey("file",
        "because the form file parameter should be in the schema");

    var fileSchema = mediaType.Schema.Properties["file"];
    fileSchema.Type.Should().Be("string",
        "because file uploads are represented as string in OpenAPI");
    fileSchema.Format.Should().Be("binary",
        "because binary format indicates file upload");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Apply_WithNullableFormFile_HandlesCorrectly()
  {
    // Arrange
    var operation = new OpenApiOperation();
    var parameters = new List<ApiParameterDescription>
        {
            new ApiParameterDescription
            {
                Name = "optionalFile",
                Type = typeof(IFormFile),
                Source = BindingSource.FormFile,
                IsRequired = false,
                ParameterDescriptor = new ControllerParameterDescriptor
                {
                    Name = "optionalFile",
                    ParameterType = typeof(IFormFile),
                    BindingInfo = new BindingInfo { BindingSource = BindingSource.FormFile }
                }
            }
        };

    var context = CreateContext(parameters);

    // Act
    _sut.Apply(operation, context);

    // Assert
    var mediaType = operation.RequestBody.Content["multipart/form-data"];
    mediaType.Schema.Properties.Should().ContainKey("optionalFile",
        "because nullable form files should still be included");
    mediaType.Schema.Required.Should().BeNullOrEmpty(
        "because the file is not required");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Apply_WithRequiredFormFile_AddsToRequiredList()
  {
    // Arrange
    var operation = new OpenApiOperation();
    var parameters = new List<ApiParameterDescription>
        {
            SwaggerMockFactory.CreateFormFileParameter("requiredFile", isRequired: true)
        };

    var context = CreateContext(parameters);

    // Act
    _sut.Apply(operation, context);

    // Assert
    var mediaType = operation.RequestBody.Content["multipart/form-data"];
    mediaType.Schema.Required.Should().NotBeNull(
        "because required properties need a required list");
    mediaType.Schema.Required.Should().Contain("requiredFile",
        "because the file parameter is marked as required");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Apply_RemovesFormFileFromParametersList()
  {
    // Arrange
    var operation = new OpenApiOperation
    {
      Parameters = new List<OpenApiParameter>
            {
                new OpenApiParameter { Name = "file" },
                new OpenApiParameter { Name = "otherParam" }
            }
    };

    var context = CreateContextWithFormFile();

    // Act
    _sut.Apply(operation, context);

    // Assert
    operation.Parameters.Should().HaveCount(1,
        "because the form file parameter should be removed");
    operation.Parameters.Should().NotContain(p => p.Name == "file",
        "because form file parameters belong in the request body, not parameters");
    operation.Parameters.Should().Contain(p => p.Name == "otherParam",
        "because non-form-file parameters should be preserved");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Apply_WithMultipleFormFiles_HandlesAllFiles()
  {
    // Arrange
    var operation = new OpenApiOperation();
    var parameters = new List<ApiParameterDescription>
        {
            SwaggerMockFactory.CreateFormFileParameter("file1"),
            SwaggerMockFactory.CreateFormFileParameter("file2"),
            SwaggerMockFactory.CreateFormParameter("textField", typeof(string))
        };

    var context = CreateContext(parameters);

    // Act
    _sut.Apply(operation, context);

    // Assert
    var mediaType = operation.RequestBody.Content["multipart/form-data"];
    mediaType.Schema.Properties.Should().ContainKey("file1",
        "because all form files should be included");
    mediaType.Schema.Properties.Should().ContainKey("file2",
        "because all form files should be included");
    mediaType.Schema.Properties.Should().ContainKey("textField",
        "because form fields should also be included");

    mediaType.Schema.Properties["file1"].Format.Should().Be("binary",
        "because file1 should be marked as binary");
    mediaType.Schema.Properties["file2"].Format.Should().Be("binary",
        "because file2 should be marked as binary");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Apply_WithExistingRequestBody_PreservesExistingContent()
  {
    // Arrange
    var operation = new OpenApiOperation
    {
      RequestBody = new OpenApiRequestBody
      {
        Content = new Dictionary<string, OpenApiMediaType>
        {
          ["application/json"] = new OpenApiMediaType()
        }
      }
    };

    var context = CreateContextWithFormFile();

    // Act
    _sut.Apply(operation, context);

    // Assert
    operation.RequestBody.Content.Should().ContainKey("application/json",
        "because existing content types should be preserved");
    operation.RequestBody.Content.Should().ContainKey("multipart/form-data",
        "because form data content type should be added");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Apply_WithFromFormAttribute_RecognizesFormFiles()
  {
    // Arrange
    var operation = new OpenApiOperation();
    var parameters = new List<ApiParameterDescription>
        {
            new ApiParameterDescription
            {
                Name = "formFile",
                Type = typeof(IFormFile),
                Source = BindingSource.Form,  // FromForm instead of FormFile
                ParameterDescriptor = new ControllerParameterDescriptor
                {
                    Name = "formFile",
                    ParameterType = typeof(IFormFile),
                    BindingInfo = new BindingInfo { BindingSource = BindingSource.Form }
                }
            }
        };

    var context = CreateContext(parameters);

    // Act
    _sut.Apply(operation, context);

    // Assert
    var mediaType = operation.RequestBody.Content["multipart/form-data"];
    mediaType.Schema.Properties.Should().ContainKey("formFile",
        "because IFormFile with FromForm should be recognized");
    mediaType.Schema.Properties["formFile"].Format.Should().Be("binary",
        "because it should be treated as a file upload");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Apply_WithConsumesAttribute_RespectsContentType()
  {
    // Arrange
    var operation = new OpenApiOperation();
    var methodInfo = typeof(TestController).GetMethod(nameof(TestController.UploadWithConsumes));
    var context = CreateContext(
        new List<ApiParameterDescription> { SwaggerMockFactory.CreateFormFileParameter() },
        methodInfo);

    // Act
    _sut.Apply(operation, context);

    // Assert
    operation.RequestBody.Should().NotBeNull(
        "because methods with Consumes attribute should have request body configured");
    operation.RequestBody.Content.Should().ContainKey("multipart/form-data",
        "because the Consumes attribute specifies multipart/form-data");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Apply_DuplicateParameters_AvoidsAddingDuplicates()
  {
    // Arrange
    var operation = new OpenApiOperation();
    var parameters = new List<ApiParameterDescription>
        {
            SwaggerMockFactory.CreateFormFileParameter("file"),
            SwaggerMockFactory.CreateFormFileParameter("file") // Duplicate
        };

    var context = CreateContext(parameters);

    // Act
    _sut.Apply(operation, context);

    // Assert
    var mediaType = operation.RequestBody.Content["multipart/form-data"];
    mediaType.Schema.Properties.Should().ContainKey("file",
        "because the parameter should exist");
    mediaType.Schema.Properties.Should().HaveCount(1,
        "because duplicates should not be added");
  }

  [Fact]
  [Trait("Category", "Unit")]
  public void Apply_CaseInsensitiveParameterRemoval_WorksCorrectly()
  {
    // Arrange
    var operation = new OpenApiOperation
    {
      Parameters = new List<OpenApiParameter>
            {
                new OpenApiParameter { Name = "FILE" },
                new OpenApiParameter { Name = "other" }
            }
    };

    var parameters = new List<ApiParameterDescription>
        {
            SwaggerMockFactory.CreateFormFileParameter("file") // lowercase
        };

    var context = CreateContext(parameters);

    // Act
    _sut.Apply(operation, context);

    // Assert
    operation.Parameters.Should().HaveCount(1,
        "because the file parameter should be removed regardless of case");
    operation.Parameters.Should().NotContain(p => p.Name.Equals("FILE", StringComparison.OrdinalIgnoreCase),
        "because case-insensitive matching should remove the parameter");
  }

  private OperationFilterContext CreateContext(
      List<ApiParameterDescription> parameters,
      MethodInfo? methodInfo = null)
  {
    var apiDescription = new ApiDescription();
    foreach (var param in parameters)
    {
      apiDescription.ParameterDescriptions.Add(param);
    }

    var schemaGenerator = new SchemaGenerator(
        new SchemaGeneratorOptions(),
        new JsonSerializerDataContractResolver(new System.Text.Json.JsonSerializerOptions())
    );

    var actualMethodInfo = methodInfo ?? typeof(TestController).GetMethod(nameof(TestController.Upload));
    return new OperationFilterContext(
        apiDescription,
        schemaGenerator,
        new SchemaRepository(),
        actualMethodInfo!);
  }

  private OperationFilterContext CreateContextWithFormFile()
  {
    var parameters = new List<ApiParameterDescription>
        {
            SwaggerMockFactory.CreateFormFileParameter()
        };

    return CreateContext(parameters);
  }

  private OperationFilterContext CreateContextWithoutFormFile()
  {
    var parameters = new List<ApiParameterDescription>
        {
            new ApiParameterDescription
            {
                Name = "id",
                Type = typeof(int),
                Source = BindingSource.Query
            }
        };

    return CreateContext(parameters);
  }

  private class TestController
  {
    public void Upload(IFormFile file) { }

    [Consumes("multipart/form-data")]
    public void UploadWithConsumes(IFormFile file) { }
  }
}
