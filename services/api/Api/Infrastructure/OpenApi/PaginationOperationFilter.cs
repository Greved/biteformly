using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BiteForm.Api.Infrastructure.OpenApi;

public sealed class PaginationOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (operation.Parameters is null || operation.Parameters.Count == 0)
            return;

        foreach (var p in operation.Parameters)
        {
            switch (p.Name)
            {
                case "tenantId":
                    if (string.IsNullOrWhiteSpace(p.Description)) p.Description = "Tenant identifier (required)";
                    p.Required = true;
                    break;
                case "page":
                    p.Description ??= "Page number (>=1). Default 1";
                    p.Schema ??= new OpenApiSchema();
                    p.Schema.Minimum ??= 1;
                    p.Schema.Default ??= new OpenApiInteger(1);
                    break;
                case "pageSize":
                    p.Description ??= "Page size (1..100). Default 20";
                    p.Schema ??= new OpenApiSchema();
                    p.Schema.Minimum ??= 1;
                    p.Schema.Maximum ??= 100;
                    p.Schema.Default ??= new OpenApiInteger(20);
                    break;
                case "order":
                    p.Description ??= "Sort order (asc | desc). Default desc";
                    p.Schema ??= new OpenApiSchema();
                    p.Schema.Enum ??= new List<IOpenApiAny> { new OpenApiString("asc"), new OpenApiString("desc") };
                    p.Schema.Default ??= new OpenApiString("desc");
                    break;
            }
        }
    }
}

