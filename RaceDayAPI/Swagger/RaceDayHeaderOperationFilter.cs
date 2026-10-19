using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace RaceDayAPI.Swagger;

// Exposes the anti-CSRF request header in Swagger's Try it out form.
public sealed class RaceDayHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        string? method = context.ApiDescription.HttpMethod;
        if (method is not ("POST" or "PUT" or "PATCH" or "DELETE")) return;
        string path = "/" + (context.ApiDescription.RelativePath ?? "").TrimStart('/');
        if (path.Equals("/api/auth/login", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/api/auth/register", StringComparison.OrdinalIgnoreCase)) return;

        operation.Parameters ??= new List<OpenApiParameter>();
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "X-RaceDay-Request",
            In = ParameterLocation.Header,
            Required = false,
            Description = "Enter 1 when using a logged-in session to change data (CSRF protection).",
            Schema = new OpenApiSchema { Type = "string", Default = new Microsoft.OpenApi.Any.OpenApiString("1") }
        });
    }
}
