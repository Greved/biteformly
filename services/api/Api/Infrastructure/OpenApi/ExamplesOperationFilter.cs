using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace BiteForm.Api.Infrastructure.OpenApi;

public sealed class ExamplesOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var path = context.ApiDescription.RelativePath?.ToLowerInvariant() ?? string.Empty;
        var method = context.ApiDescription.HttpMethod?.ToUpperInvariant() ?? "";

        // Helper to set a 200 example if empty
        void Set200(OpenApiObject value, string summary)
        {
            if (operation.Responses.TryGetValue("200", out var resp) && resp.Content.TryGetValue("application/json", out var media))
            {
                media.Examples ??= new Dictionary<string, OpenApiExample>();
                if (media.Examples.Count == 0)
                {
                    media.Examples["sample"] = new OpenApiExample { Summary = summary, Value = value };
                }
            }
        }

        void Set201(OpenApiObject value, string summary)
        {
            if (operation.Responses.TryGetValue("201", out var resp) && resp.Content.TryGetValue("application/json", out var media))
            {
                media.Examples ??= new Dictionary<string, OpenApiExample>();
                if (media.Examples.Count == 0)
                {
                    media.Examples["sample"] = new OpenApiExample { Summary = summary, Value = value };
                }
            }
        }

        // Forms list
        if (method == "GET" && path.EndsWith("/api/v1/forms"))
        {
            var item = new OpenApiObject
            {
                ["id"] = new OpenApiString("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                ["tenantId"] = new OpenApiString("tenant_123"),
                ["name"] = new OpenApiString("Contact"),
                ["description"] = new OpenApiString("Basic contact form"),
                ["createdAtUtc"] = new OpenApiString("2025-01-01T00:00:00Z")
            };
            Set200(new OpenApiObject
            {
                ["items"] = new OpenApiArray { item },
                ["total"] = new OpenApiInteger(1),
                ["page"] = new OpenApiInteger(1),
                ["pageSize"] = new OpenApiInteger(20)
            }, "Paged forms");
        }

        // Fields list
        if (method == "GET" && path.Contains("/fields") && path.EndsWith("/fields"))
        {
            var item = new OpenApiObject
            {
                ["id"] = new OpenApiString("6fa459ea-ee8a-3ca4-894e-db77e160355e"),
                ["formId"] = new OpenApiString("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                ["key"] = new OpenApiString("email"),
                ["label"] = new OpenApiString("Email"),
                ["type"] = new OpenApiString("text"),
                ["required"] = new OpenApiBoolean(true),
                ["order"] = new OpenApiInteger(1)
            };
            Set200(new OpenApiObject
            {
                ["items"] = new OpenApiArray { item },
                ["total"] = new OpenApiInteger(2),
                ["page"] = new OpenApiInteger(1),
                ["pageSize"] = new OpenApiInteger(20)
            }, "Paged fields");
        }

        // Submissions list
        if (method == "GET" && path.Contains("/submissions") && path.EndsWith("/submissions"))
        {
            var item = new OpenApiObject
            {
                ["id"] = new OpenApiString("9a7b3306-a5e4-4d63-9d16-1baf6d8a8d2f"),
                ["formId"] = new OpenApiString("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                ["tenantId"] = new OpenApiString("tenant_123"),
                ["submittedAtUtc"] = new OpenApiString("2025-01-01T00:00:00Z"),
                ["submittedBy"] = new OpenApiString("user@example.com"),
                ["responses"] = new OpenApiArray()
            };
            Set200(new OpenApiObject
            {
                ["items"] = new OpenApiArray { item },
                ["total"] = new OpenApiInteger(1),
                ["page"] = new OpenApiInteger(1),
                ["pageSize"] = new OpenApiInteger(20)
            }, "Paged submissions");
        }

        // Get form by id
        if (method == "GET" && path.Contains("/api/v1/forms/") && !path.EndsWith("/forms") && !path.Contains("/fields"))
        {
            Set200(new OpenApiObject
            {
                ["id"] = new OpenApiString("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                ["tenantId"] = new OpenApiString("tenant_123"),
                ["name"] = new OpenApiString("Contact"),
                ["description"] = new OpenApiString("Basic contact form"),
                ["createdAtUtc"] = new OpenApiString("2025-01-01T00:00:00Z")
            }, "Form");
        }

        // Get field by id
        if (method == "GET" && path.Contains("/fields/") && !path.EndsWith("/fields"))
        {
            Set200(new OpenApiObject
            {
                ["id"] = new OpenApiString("6fa459ea-ee8a-3ca4-894e-db77e160355e"),
                ["formId"] = new OpenApiString("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                ["key"] = new OpenApiString("email"),
                ["label"] = new OpenApiString("Email"),
                ["type"] = new OpenApiString("text"),
                ["required"] = new OpenApiBoolean(true),
                ["order"] = new OpenApiInteger(1)
            }, "Field");
        }

        // Get submission
        if (method == "GET" && path.Contains("/submissions/") && !path.EndsWith("/responses") && !path.EndsWith("/responses/{responseId}"))
        {
            var response1 = new OpenApiObject
            {
                ["id"] = new OpenApiString("d5a2c2a1-1b2c-4d5e-9f8a-6c7b8d9e0f1a"),
                ["submissionId"] = new OpenApiString("9a7b3306-a5e4-4d63-9d16-1baf6d8a8d2f"),
                ["fieldId"] = new OpenApiString("6fa459ea-ee8a-3ca4-894e-db77e160355e"),
                ["value"] = new OpenApiString("hello")
            };
            Set200(new OpenApiObject
            {
                ["id"] = new OpenApiString("9a7b3306-a5e4-4d63-9d16-1baf6d8a8d2f"),
                ["formId"] = new OpenApiString("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                ["tenantId"] = new OpenApiString("tenant_123"),
                ["submittedAtUtc"] = new OpenApiString("2025-01-01T00:00:00Z"),
                ["submittedBy"] = new OpenApiString("user@example.com"),
                ["responses"] = new OpenApiArray { response1 }
            }, "Submission with responses");
        }

        // Responses list
        if (method == "GET" && path.EndsWith("/responses"))
        {
            var item = new OpenApiObject
            {
                ["id"] = new OpenApiString("d5a2c2a1-1b2c-4d5e-9f8a-6c7b8d9e0f1a"),
                ["submissionId"] = new OpenApiString("9a7b3306-a5e4-4d63-9d16-1baf6d8a8d2f"),
                ["fieldId"] = new OpenApiString("6fa459ea-ee8a-3ca4-894e-db77e160355e"),
                ["value"] = new OpenApiString("hello")
            };
            Set200(new OpenApiObject
            {
                ["items"] = new OpenApiArray { item },
                ["total"] = new OpenApiInteger(2),
                ["page"] = new OpenApiInteger(1),
                ["pageSize"] = new OpenApiInteger(20)
            }, "Paged responses");
        }

        // Create submission 201
        if (method == "POST" && path.EndsWith("/submissions"))
        {
            Set201(new OpenApiObject { ["id"] = new OpenApiString("9a7b3306-a5e4-4d63-9d16-1baf6d8a8d2f") }, "Created submission id");
        }

        // Create response 201
        if (method == "POST" && path.EndsWith("/responses"))
        {
            Set201(new OpenApiObject { ["id"] = new OpenApiString("d5a2c2a1-1b2c-4d5e-9f8a-6c7b8d9e0f1a") }, "Created response id");
        }

        // Update form 200
        if (method == "PUT" && path.EndsWith("/api/v1/forms/{id}"))
        {
            Set200(new OpenApiObject
            {
                ["id"] = new OpenApiString("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                ["tenantId"] = new OpenApiString("tenant_123"),
                ["name"] = new OpenApiString("Contact Updated"),
                ["description"] = new OpenApiString("Updated"),
                ["createdAtUtc"] = new OpenApiString("2025-01-01T00:00:00Z")
            }, "Updated form");
        }

        // Update field 200
        if (method == "PUT" && path.EndsWith("/fields/{fieldid}"))
        {
            Set200(new OpenApiObject
            {
                ["id"] = new OpenApiString("6fa459ea-ee8a-3ca4-894e-db77e160355e"),
                ["formId"] = new OpenApiString("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                ["key"] = new OpenApiString("email"),
                ["label"] = new OpenApiString("Email Address"),
                ["type"] = new OpenApiString("text"),
                ["required"] = new OpenApiBoolean(true),
                ["order"] = new OpenApiInteger(1)
            }, "Updated field");
        }

        // Update submission 200
        if (method == "PUT" && path.EndsWith("/submissions/{submissionid}") && !path.EndsWith("/responses/{responseid}"))
        {
            Set200(new OpenApiObject
            {
                ["id"] = new OpenApiString("9a7b3306-a5e4-4d63-9d16-1baf6d8a8d2f"),
                ["formId"] = new OpenApiString("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                ["tenantId"] = new OpenApiString("tenant_123"),
                ["submittedAtUtc"] = new OpenApiString("2025-01-01T00:00:00Z"),
                ["submittedBy"] = new OpenApiString("updated@example.com")
            }, "Updated submission");
        }

        // Update response 200
        if (method == "PUT" && path.EndsWith("/responses/{responseid}"))
        {
            Set200(new OpenApiObject
            {
                ["id"] = new OpenApiString("d5a2c2a1-1b2c-4d5e-9f8a-6c7b8d9e0f1a"),
                ["submissionId"] = new OpenApiString("9a7b3306-a5e4-4d63-9d16-1baf6d8a8d2f"),
                ["fieldId"] = new OpenApiString("6fa459ea-ee8a-3ca4-894e-db77e160355e"),
                ["value"] = new OpenApiString("hello-updated")
            }, "Updated response");
        }

        // Generic 204 description for deletes
        if (method == "DELETE" && operation.Responses.TryGetValue("204", out var noContent))
        {
            if (string.IsNullOrWhiteSpace(noContent.Description))
            {
                noContent.Description = "Deleted";
            }
        }

        // Generic 404 description
        if (operation.Responses.TryGetValue("404", out var notFound))
        {
            if (string.IsNullOrWhiteSpace(notFound.Description))
            {
                notFound.Description = "Not Found";
            }
        }

        // Generic 400 ValidationProblem example
        if (operation.Responses.TryGetValue("400", out var badRequest))
        {
            badRequest.Description ??= "Bad Request";
            badRequest.Content ??= new Dictionary<string, OpenApiMediaType>();
            if (!badRequest.Content.TryGetValue("application/problem+json", out var media400))
            {
                media400 = new OpenApiMediaType();
                badRequest.Content["application/problem+json"] = media400;
            }
            media400.Examples ??= new Dictionary<string, OpenApiExample>();
            if (!media400.Examples.ContainsKey("validation"))
            {
                media400.Examples["validation"] = new OpenApiExample
                {
                    Summary = "Validation errors",
                    Value = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.5.1"),
                        ["title"] = new OpenApiString("One or more validation errors occurred."),
                        ["status"] = new OpenApiInteger(400),
                        ["errors"] = new OpenApiObject
                        {
                            ["name"] = new OpenApiArray { new OpenApiString("The Name field is required.") },
                            ["tenantId"] = new OpenApiArray { new OpenApiString("The TenantId field is required.") }
                        }
                    }
                };
            }
        }

        // Generic 409 conflict example (problem+json)
        if (operation.Responses.TryGetValue("409", out var conflict))
        {
            conflict.Description ??= "Conflict";
            conflict.Content ??= new Dictionary<string, OpenApiMediaType>();
            if (!conflict.Content.TryGetValue("application/problem+json", out var media409))
            {
                media409 = new OpenApiMediaType();
                conflict.Content["application/problem+json"] = media409;
            }
            media409.Examples ??= new Dictionary<string, OpenApiExample>();
            if (!media409.Examples.ContainsKey("conflict"))
            {
                media409.Examples["conflict"] = new OpenApiExample
                {
                    Summary = "Conflict message",
                    Value = new OpenApiObject
                    {
                        ["type"] = new OpenApiString("https://tools.ietf.org/html/rfc9110#section-15.5.10"),
                        ["title"] = new OpenApiString("Conflict"),
                        ["status"] = new OpenApiInteger(409),
                        ["detail"] = new OpenApiString("field key already exists for this form")
                    }
                };
            }
        }
    }
}
