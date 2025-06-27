using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Zarichney.Config;

/// <summary>
/// Custom IOperationFilter to correctly handle IFormFile parameters in Swagger/OpenAPI generation,
/// ensuring they are represented as file uploads (type: string, format: binary)
/// when used with [FromForm].
/// </summary>
public class FormFileOperationFilter : IOperationFilter
{
  // Helper method to get underlying types, potentially unwrapping Nullable<>
  private static Type? GetUnderlyingType(Type type)
  {
    if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
    {
      return Nullable.GetUnderlyingType(type);
    }

    return type;
  }

  public void Apply(OpenApiOperation operation, OperationFilterContext context)
  {
    var formFileParams = context.ApiDescription.ParameterDescriptions
      .Where(p => GetUnderlyingType(p.Type) ==
                  typeof(IFormFile)) // Check if the parameter is IFormFile (or Nullable<IFormFile>)
      .ToList();

    if (formFileParams.Count == 0)
    {
      // No IFormFile parameters in this action, nothing to do.
      return;
    }

    // Check if the action expects "multipart/form-data"
    // This check might be optional depending on how strictly you want to apply the filter,
    // but it's good practice. Assumes you've added [Consumes("multipart/form-data")]
    // or that it's inferred correctly by ASP.NET Core.
    var consumesAttribute =
      context.MethodInfo.GetCustomAttributes(typeof(ConsumesAttribute), true).FirstOrDefault() as ConsumesAttribute;
    var expectsFormData = consumesAttribute?.ContentTypes.Any(ct => ct == "multipart/form-data") ?? false;

    // Alternative check if [Consumes] isn't always present but [FromForm] is used on IFormFile
    var hasFromFormOnFile = formFileParams.Any(p =>
      p.Source == BindingSource.FormFile ||
      (p.ParameterDescriptor.BindingInfo?.BindingSource == BindingSource.Form
       && GetUnderlyingType(p.Type) == typeof(IFormFile))
    );


    if (!expectsFormData && !hasFromFormOnFile)
    {
      // If it doesn't explicitly consume form-data AND doesn't have [FromForm] on an IFormFile,
      // maybe skip modification. Adjust this logic if needed.
      // return;
    }


    // --- Modification Logic ---

    // 1. Ensure the request body media type is 'multipart/form-data'.
    operation.RequestBody ??= new OpenApiRequestBody();
    if (!operation.RequestBody.Content.TryGetValue("multipart/form-data", out var value))
    {
      value = new OpenApiMediaType();
      operation.RequestBody.Content["multipart/form-data"] = value;
    }

    var mediaType = value;
    mediaType.Schema ??= new OpenApiSchema { Type = "object" };

    // 2. Adjust parameters: Remove the IFormFile parameters from the 'parameters' list
    //    as they should be part of the request body schema for form data.
    var parametersToRemove = operation.Parameters
      .Where(p => formFileParams.Any(fp => fp.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
      .ToList();

    foreach (var param in parametersToRemove)
    {
      operation.Parameters.Remove(param);
    }

    // 3. Add properties to the 'multipart/form-data' schema for each IFormFile
    //    and any other [FromForm] parameters.
    foreach (var paramDesc in context.ApiDescription.ParameterDescriptions)
    {
      // Only process parameters expected from the form
      if (paramDesc.Source == BindingSource.Form || paramDesc.Source == BindingSource.FormFile)
      {
        // Avoid adding duplicates if already present
        if (!mediaType.Schema.Properties.ContainsKey(paramDesc.Name))
        {
          var schema = context.SchemaGenerator.GenerateSchema(paramDesc.Type, context.SchemaRepository);

          // **Crucial Part**: If it's an IFormFile, override the type/format
          if (GetUnderlyingType(paramDesc.Type) == typeof(IFormFile))
          {
            schema.Type = "string";
            schema.Format = "binary"; // Indicates file upload
          }

          mediaType.Schema.Properties.Add(paramDesc.Name, schema);

          // Mark as required if necessary (check attributes or model annotations)
          if (paramDesc.IsRequired)
          {
            mediaType.Schema.Required ??= new HashSet<string>();
            mediaType.Schema.Required.Add(paramDesc.Name);
          }
        }
      }
    }
  }
}
