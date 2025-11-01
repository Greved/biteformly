using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BiteForm.Api.Infrastructure.OpenApi;

public sealed class FieldTypeSchemaFilter : ISchemaFilter
{
    private static readonly string[] FieldTypes = new[]
    {
        "text", "textarea", "number", "email", "url", "phone", "date", "time", "datetime", "select", "radio", "checkbox"
    };

    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        // Only adjust field DTOs: CreateFieldRequest, UpdateFieldRequest, FormFieldDto
        var tname = context.Type.FullName ?? string.Empty;
        if (!(tname.Contains("FormsEndpoints+CreateFieldRequest") ||
              tname.Contains("FormsEndpoints+UpdateFieldRequest") ||
              tname.Contains("FormsEndpoints+FormFieldDto")))
        {
            return;
        }

        if (schema.Properties is null) return;
        if (!schema.Properties.TryGetValue("type", out var typeProp)) return;

        typeProp.Description = string.IsNullOrEmpty(typeProp.Description)
            ? "Field input type"
            : typeProp.Description;

        typeProp.Enum = FieldTypes.Select(v => (IOpenApiAny)new OpenApiString(v)).ToList();
        typeProp.Default = new OpenApiString("text");
    }
}

