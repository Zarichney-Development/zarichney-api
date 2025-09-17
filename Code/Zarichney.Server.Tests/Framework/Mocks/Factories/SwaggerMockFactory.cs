using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Moq;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Reflection;

namespace Zarichney.Server.Tests.Framework.Mocks.Factories;

/// <summary>
/// Factory for creating Swagger/OpenAPI mock objects for testing operation filters.
/// </summary>
public static class SwaggerMockFactory
{
    /// <summary>
    /// Creates a mock OperationFilterContext for testing operation filters.
    /// </summary>
    public static Mock<OperationFilterContext> CreateOperationFilterContext(
        MethodInfo? methodInfo = null,
        List<ApiParameterDescription>? parameters = null)
    {
        var mockContext = new Mock<OperationFilterContext>();
        
        // Setup ApiDescription
        var apiDescription = new ApiDescription();
        if (parameters != null)
        {
            foreach (var param in parameters)
            {
                apiDescription.ParameterDescriptions.Add(param);
            }
        }
        mockContext.Setup(x => x.ApiDescription).Returns(apiDescription);
        
        // Setup MethodInfo
        if (methodInfo != null)
        {
            mockContext.Setup(x => x.MethodInfo).Returns(methodInfo);
        }
        else
        {
            // Use a dummy method if none provided
            var dummyMethodInfo = typeof(SwaggerMockFactory).GetMethod(nameof(DummyMethod));
            mockContext.Setup(x => x.MethodInfo).Returns(dummyMethodInfo!);
        }
        
        // Setup SchemaGenerator and Repository
        var schemaRepository = new SchemaRepository();
        var schemaGenerator = new SchemaGenerator(
            new SchemaGeneratorOptions(),
            new JsonSerializerDataContractResolver(new System.Text.Json.JsonSerializerOptions())
        );
        
        mockContext.Setup(x => x.SchemaGenerator).Returns(schemaGenerator);
        mockContext.Setup(x => x.SchemaRepository).Returns(schemaRepository);
        
        return mockContext;
    }
    
    /// <summary>
    /// Creates an ApiParameterDescription for an IFormFile parameter.
    /// </summary>
    public static ApiParameterDescription CreateFormFileParameter(
        string name = "file",
        bool isRequired = false,
        BindingSource? bindingSource = null)
    {
        return new ApiParameterDescription
        {
            Name = name,
            Type = typeof(IFormFile),
            Source = bindingSource ?? BindingSource.FormFile,
            IsRequired = isRequired,
            ParameterDescriptor = new ControllerParameterDescriptor
            {
                Name = name,
                ParameterType = typeof(IFormFile),
                BindingInfo = new BindingInfo
                {
                    BindingSource = bindingSource ?? BindingSource.FormFile
                }
            }
        };
    }
    
    /// <summary>
    /// Creates an ApiParameterDescription for a regular form parameter.
    /// </summary>
    public static ApiParameterDescription CreateFormParameter(
        string name,
        Type type,
        bool isRequired = false)
    {
        return new ApiParameterDescription
        {
            Name = name,
            Type = type,
            Source = BindingSource.Form,
            IsRequired = isRequired,
            ParameterDescriptor = new ControllerParameterDescriptor
            {
                Name = name,
                ParameterType = type,
                BindingInfo = new BindingInfo
                {
                    BindingSource = BindingSource.Form
                }
            }
        };
    }
    
    /// <summary>
    /// Creates an OpenApiOperation for testing.
    /// </summary>
    public static OpenApiOperation CreateOperation(
        bool includeParameters = false,
        bool includeRequestBody = false)
    {
        var operation = new OpenApiOperation
        {
            Parameters = includeParameters ? new List<OpenApiParameter>() : new List<OpenApiParameter>(),
            RequestBody = includeRequestBody ? new OpenApiRequestBody
            {
                Content = new Dictionary<string, OpenApiMediaType>()
            } : null
        };
        
        if (includeParameters)
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "testParam",
                In = ParameterLocation.Query,
                Schema = new OpenApiSchema { Type = "string" }
            });
        }
        
        return operation;
    }
    
    public static void DummyMethod(IFormFile file) { }
    
    public static void MethodWithConsumes(IFormFile file) { }
}