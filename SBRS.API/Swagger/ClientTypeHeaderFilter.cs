using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SBRS.Swagger;

/// <summary>Adds the optional X-Client-Type header parameter to the auth endpoints.</summary>
public class ClientTypeHeaderFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor action ||
            action.ControllerName != "Auth")
            return;

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-Client-Type",
            In = ParameterLocation.Header,
            Required = false,
            Description = "web enables the HttpOnly refresh-token cookie flow; " +
                          "anything else (or the header absent) behaves as a mobile client.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        });
    }
}
