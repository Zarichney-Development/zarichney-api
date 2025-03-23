using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Zarichney.Server.Services.Sessions;

[AttributeUsage(AttributeTargets.Method)]
public class AcceptsSessionAttribute : Attribute, IOperationFilter
{
  public void Apply(OpenApiOperation operation, OperationFilterContext context)
  {
    operation.Parameters ??= new List<OpenApiParameter>();

    operation.Parameters.Add(new OpenApiParameter
    {
      Name = "X-Session-Id",
      In = ParameterLocation.Header,
      Required = false,
      Schema = new OpenApiSchema
      {
        Type = "string"
      },
      Description = "Session ID"
    });
  }
}