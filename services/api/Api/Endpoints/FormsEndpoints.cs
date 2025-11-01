using BiteForm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using BiteForm.Domain.Entities;
using BiteForm.Application.Abstractions.Time;
using BiteForm.Api.Infrastructure;
using BiteForm.Api.Infrastructure.Validation;
using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;

namespace BiteForm.Api.Endpoints;

public static class FormsEndpoints
{
    public static IEndpointRouteBuilder MapFormsEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1").WithTags("v1");

        var forms = api.MapGroup("/forms").WithTags("Forms");

        // List
        forms.MapGet("/", async ([FromQuery] string tenantId, [FromQuery] string? q, [FromQuery] string? sort, [FromQuery] string? order, [FromQuery] int page, [FromQuery] int pageSize, AppDbContext db, CancellationToken ct) =>
            {
                var (p, ps) = Paging.Normalize(page, pageSize);
                var query = db.Forms
                    .Where(f => f.TenantId == tenantId)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var term = q.Trim();
                    query = query.Where(f => f.Name.Contains(term) || (f.Description != null && f.Description.Contains(term)));
                }

                var descending = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase);
                query = sort switch
                {
                    "name" => (descending ? query.OrderByDescending(f => f.Name) : query.OrderBy(f => f.Name)).ThenByDescending(f => f.CreatedAtUtc),
                    _ => descending ? query.OrderByDescending(f => f.CreatedAtUtc) : query.OrderBy(f => f.CreatedAtUtc)
                };

                var shaped = query.Select(f => new FormDto(f.Id, f.TenantId, f.Name, f.Description, f.CreatedAtUtc));
                var paged = await shaped.ToPagedResultAsync(p, ps, ct);
                return Results.Ok(paged);
            })
            .WithName("ListForms")
            .WithOpenApi(op =>
            {
                // 200 example (paged forms)
                if (op.Responses.TryGetValue("200", out var ok) && ok.Content.TryGetValue("application/json", out var media))
                {
                    var item = new OpenApiObject
                    {
                        ["id"] = new OpenApiString("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                        ["tenantId"] = new OpenApiString("tenant_123"),
                        ["name"] = new OpenApiString("Contact"),
                        ["description"] = new OpenApiString("Basic contact form"),
                        ["createdAtUtc"] = new OpenApiString("2025-01-01T00:00:00Z")
                    };
                    media.Examples ??= new Dictionary<string, OpenApiExample>();
                    media.Examples["sample"] = new OpenApiExample
                    {
                        Summary = "Paged forms",
                        Value = new OpenApiObject
                        {
                            ["items"] = new OpenApiArray { item },
                            ["total"] = new OpenApiInteger(1),
                            ["page"] = new OpenApiInteger(1),
                            ["pageSize"] = new OpenApiInteger(20)
                        }
                    };
                }
                foreach (var p in op.Parameters)
                {
                    switch (p.Name)
                    {
                        case "tenantId":
                            p.Description = "Tenant identifier (required)";
                            p.Required = true;
                            break;
                        case "q":
                            p.Description = "Optional search term (matches name/description)";
                            break;
                        case "sort":
                            p.Description = "Sort field (name | createdAt). Default createdAt";
                            p.Schema ??= new OpenApiSchema();
                            p.Schema.Enum = new List<IOpenApiAny> { new OpenApiString("name"), new OpenApiString("createdAt") };
                            break;
                        case "order":
                            p.Description = "Sort order (asc | desc). Default desc";
                            p.Schema ??= new OpenApiSchema();
                            p.Schema.Enum = new List<IOpenApiAny> { new OpenApiString("asc"), new OpenApiString("desc") };
                            p.Schema.Default = new OpenApiString("desc");
                            break;
                        case "page":
                            p.Description = "Page number (>=1). Default 1";
                            p.Schema ??= new OpenApiSchema();
                            p.Schema.Minimum = 1;
                            p.Schema.Default = new OpenApiInteger(1);
                            break;
                        case "pageSize":
                            p.Description = "Page size (1..100). Default 20";
                            p.Schema ??= new OpenApiSchema();
                            p.Schema.Minimum = 1;
                            p.Schema.Maximum = 100;
                            p.Schema.Default = new OpenApiInteger(20);
                            break;
                    }
                }
                return op;
            });

        // Get by id
        forms.MapGet("/{id:guid}", async (Guid id, [FromQuery] string tenantId, AppDbContext db, CancellationToken ct) =>
            {
                var f = await db.Forms
                    .Where(x => x.Id == id && x.TenantId == tenantId)
                    .Select(x => new FormDto(x.Id, x.TenantId, x.Name, x.Description, x.CreatedAtUtc))
                    .FirstOrDefaultAsync(ct);
                return f is null ? Results.NotFound() : Results.Ok(f);
            })
            .WithName("GetForm");

        // Create
        forms.MapPost("/", async (CreateFormRequest req, AppDbContext db, IDateTimeProvider clock, CancellationToken ct) =>
            {
                var id = Guid.NewGuid();
                var form = new Form
                {
                    Id = id,
                    TenantId = req.TenantId,
                    Name = req.Name.Trim(),
                    Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim(),
                    CreatedAtUtc = clock.UtcNow
                };

                db.Forms.Add(form);
                await db.SaveChangesAsync(ct);

                var dto = new FormDto(form.Id, form.TenantId, form.Name, form.Description, form.CreatedAtUtc);
                return Results.Created($"/api/v1/forms/{id}", dto);
            })
            .WithRequestValidation<CreateFormRequest>()
            .WithName("CreateForm")
            .WithOpenApi(op =>
            {
                // request example
                var json = new Microsoft.OpenApi.Any.OpenApiObject
                {
                    ["tenantId"] = new Microsoft.OpenApi.Any.OpenApiString("tenant_123"),
                    ["name"] = new Microsoft.OpenApi.Any.OpenApiString("Contact"),
                    ["description"] = new Microsoft.OpenApi.Any.OpenApiString("Basic contact form")
                };
                if (op.RequestBody?.Content.TryGetValue("application/json", out var media) == true)
                {
                    media.Examples = media.Examples ?? new Dictionary<string, Microsoft.OpenApi.Models.OpenApiExample>();
                    media.Examples["sample"] = new Microsoft.OpenApi.Models.OpenApiExample
                    {
                        Summary = "Create form",
                        Value = json
                    };
                }
                // response 201 example
                if (op.Responses.TryGetValue("201", out var created) && created.Content.TryGetValue("application/json", out var cmedia))
                {
                    cmedia.Examples ??= new Dictionary<string, OpenApiExample>();
                    cmedia.Examples["sample"] = new OpenApiExample
                    {
                        Summary = "Created form",
                        Value = new OpenApiObject
                        {
                            ["id"] = new OpenApiString("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                            ["tenantId"] = new OpenApiString("tenant_123"),
                            ["name"] = new OpenApiString("Contact"),
                            ["description"] = new OpenApiString("Basic contact form"),
                            ["createdAtUtc"] = new OpenApiString("2025-01-01T00:00:00Z")
                        }
                    };
                }
                return op;
            });

        // Update
        forms.MapPut("/{id:guid}", async (Guid id, UpdateFormRequest req, AppDbContext db, CancellationToken ct) =>
            {
                var form = await db.Forms.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == req.TenantId, ct);
                if (form is null) return Results.NotFound();

                form.Name = req.Name.Trim();
                form.Description = string.IsNullOrWhiteSpace(req.Description) ? null : req.Description.Trim();
                await db.SaveChangesAsync(ct);

                var dto = new FormDto(form.Id, form.TenantId, form.Name, form.Description, form.CreatedAtUtc);
                return Results.Ok(dto);
            })
            .WithRequestValidation<UpdateFormRequest>()
            .WithName("UpdateForm")
            .WithOpenApi(op =>
            {
                // 200 example
                if (op.Responses.TryGetValue("200", out var ok) && ok.Content.TryGetValue("application/json", out var media))
                {
                    media.Examples ??= new Dictionary<string, OpenApiExample>();
                    media.Examples["sample"] = new OpenApiExample
                    {
                        Summary = "Updated form",
                        Value = new OpenApiObject
                        {
                            ["id"] = new OpenApiString("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                            ["tenantId"] = new OpenApiString("tenant_123"),
                            ["name"] = new OpenApiString("Contact Updated"),
                            ["description"] = new OpenApiString("Updated"),
                            ["createdAtUtc"] = new OpenApiString("2025-01-01T00:00:00Z")
                        }
                    };
                }
                return op;
            });

        // Delete
        forms.MapDelete("/{id:guid}", async (Guid id, [FromQuery] string tenantId, AppDbContext db, CancellationToken ct) =>
            {
                var form = await db.Forms.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId, ct);
                if (form is null) return Results.NotFound();
                db.Forms.Remove(form);
                await db.SaveChangesAsync(ct);
                return Results.NoContent();
            })
            .WithName("DeleteForm");


        // List fields for a form (absolute route)
        app.MapGet("/api/v1/forms/{formId:guid}/fields", async (Guid formId, [FromQuery] string tenantId, [FromQuery] string? q, [FromQuery] string? sort, [FromQuery] string? order, [FromQuery] int page, [FromQuery] int pageSize, AppDbContext db, CancellationToken ct) =>
            {
                var owns = await db.Forms.AnyAsync(f => f.Id == formId && f.TenantId == tenantId, ct);
                if (!owns) return Results.NotFound();

                var (p, ps) = Paging.Normalize(page, pageSize);
                var query = db.Fields.Where(ff => ff.FormId == formId).AsQueryable();

                if (!string.IsNullOrWhiteSpace(q))
                {
                    var term = q.Trim();
                    query = query.Where(ff => ff.Key.Contains(term) || ff.Label.Contains(term));
                }

                var descending = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase);
                query = sort switch
                {
                    "key" => (descending ? query.OrderByDescending(ff => ff.Key) : query.OrderBy(ff => ff.Key)).ThenBy(ff => ff.Order),
                    "order" or _ => descending ? query.OrderByDescending(ff => ff.Order).ThenBy(ff => ff.Key) : query.OrderBy(ff => ff.Order).ThenBy(ff => ff.Key)
                };

                var shaped = query.Select(ff => new FormFieldDto(ff.Id, ff.FormId, ff.Key, ff.Label, ff.Type, ff.Required, ff.Order));
                var paged = await shaped.ToPagedResultAsync(p, ps, ct);
                return Results.Ok(paged);
            })
            .WithName("ListFormFields")
            .WithTags("FormFields")
            .WithOpenApi(op =>
            {
                // 200 example (paged fields)
                if (op.Responses.TryGetValue("200", out var ok) && ok.Content.TryGetValue("application/json", out var media))
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
                    media.Examples ??= new Dictionary<string, OpenApiExample>();
                    media.Examples["sample"] = new OpenApiExample
                    {
                        Summary = "Paged fields",
                        Value = new OpenApiObject
                        {
                            ["items"] = new OpenApiArray { item },
                            ["total"] = new OpenApiInteger(1),
                            ["page"] = new OpenApiInteger(1),
                            ["pageSize"] = new OpenApiInteger(20)
                        }
                    };
                }
                foreach (var p in op.Parameters)
                {
                    switch (p.Name)
                    {
                        case "tenantId":
                            p.Description = "Tenant identifier (required)";
                            p.Required = true;
                            break;
                        case "q":
                            p.Description = "Optional search term (matches key/label)";
                            break;
                        case "sort":
                            p.Description = "Sort field (order | key). Default order";
                            p.Schema ??= new OpenApiSchema();
                            p.Schema.Enum = new List<IOpenApiAny> { new OpenApiString("order"), new OpenApiString("key") };
                            p.Schema.Default = new OpenApiString("order");
                            break;
                        case "order":
                            p.Description = "Sort order (asc | desc). Default asc";
                            p.Schema ??= new OpenApiSchema();
                            p.Schema.Enum = new List<IOpenApiAny> { new OpenApiString("asc"), new OpenApiString("desc") };
                            p.Schema.Default = new OpenApiString("asc");
                            break;
                        case "page":
                            p.Description = "Page number (>=1). Default 1";
                            p.Schema ??= new OpenApiSchema();
                            p.Schema.Minimum = 1;
                            p.Schema.Default = new OpenApiInteger(1);
                            break;
                        case "pageSize":
                            p.Description = "Page size (1..100). Default 20";
                            p.Schema ??= new OpenApiSchema();
                            p.Schema.Minimum = 1;
                            p.Schema.Maximum = 100;
                            p.Schema.Default = new OpenApiInteger(20);
                            break;
                    }
                }
                return op;
            });

        // Get a single field
        app.MapGet("/api/v1/forms/{formId:guid}/fields/{fieldId:guid}", async (Guid formId, Guid fieldId, [FromQuery] string tenantId, AppDbContext db, CancellationToken ct) =>
            {
                var item = await db.Fields
                    .Where(ff => ff.Id == fieldId && ff.FormId == formId)
                    .Join(db.Forms, ff => ff.FormId, f => f.Id, (ff, f) => new { ff, f })
                    .Where(x => x.f.TenantId == tenantId)
                    .Select(x => new FormFieldDto(x.ff.Id, x.ff.FormId, x.ff.Key, x.ff.Label, x.ff.Type, x.ff.Required, x.ff.Order))
                    .FirstOrDefaultAsync(ct);
                return item is null ? Results.NotFound() : Results.Ok(item);
            })
            .WithName("GetFormField");

        // Create field (absolute route)
        app.MapPost("/api/v1/forms/{formId:guid}/fields", async (Guid formId, CreateFieldRequest req, [FromQuery] string tenantId, AppDbContext db, CancellationToken ct) =>
        {
            var form = await db.Forms.FirstOrDefaultAsync(f => f.Id == formId && f.TenantId == tenantId, ct);
            if (form is null) return Results.NotFound();

            var key = req.Key.Trim();
            var existsKey = await db.Fields.AnyAsync(ff => ff.FormId == formId && ff.Key == key, ct);
            if (existsKey)
            {
                return Results.Problem(
                    title: "Conflict",
                    detail: "field key already exists for this form",
                    statusCode: 409,
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.10");
            }

            var field = new FormField
            {
                Id = Guid.NewGuid(),
                FormId = formId,
                Key = key,
                Label = req.Label.Trim(),
                Type = req.Type.Trim(),
                Required = req.Required,
                Order = req.Order
            };

            db.Fields.Add(field);
            await db.SaveChangesAsync(ct);

            var dto = new FormFieldDto(field.Id, field.FormId, field.Key, field.Label, field.Type, field.Required, field.Order);
            return Results.Created($"/api/v1/forms/{formId}/fields/{field.Id}", dto);
        })
        .WithTags("FormFields")
        .WithRequestValidation<CreateFieldRequest>()
        .WithName("CreateFormField")
        .WithOpenApi(op =>
        {
            var json = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["key"] = new Microsoft.OpenApi.Any.OpenApiString("email"),
                ["label"] = new Microsoft.OpenApi.Any.OpenApiString("Email"),
                ["type"] = new Microsoft.OpenApi.Any.OpenApiString("text"),
                ["required"] = new Microsoft.OpenApi.Any.OpenApiBoolean(true),
                ["order"] = new Microsoft.OpenApi.Any.OpenApiInteger(1)
            };
            if (op.RequestBody?.Content.TryGetValue("application/json", out var media) == true)
            {
                media.Examples ??= new Dictionary<string, Microsoft.OpenApi.Models.OpenApiExample>();
                media.Examples["sample"] = new Microsoft.OpenApi.Models.OpenApiExample
                {
                    Summary = "Create field",
                    Value = json
                };
            }
            // response 201 example
            if (op.Responses.TryGetValue("201", out var created) && created.Content.TryGetValue("application/json", out var cmedia))
            {
                cmedia.Examples ??= new Dictionary<string, OpenApiExample>();
                cmedia.Examples["sample"] = new OpenApiExample
                {
                    Summary = "Created field",
                    Value = new OpenApiObject
                    {
                        ["id"] = new OpenApiString("6fa459ea-ee8a-3ca4-894e-db77e160355e"),
                        ["formId"] = new OpenApiString("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                        ["key"] = new OpenApiString("email"),
                        ["label"] = new OpenApiString("Email"),
                        ["type"] = new OpenApiString("text"),
                        ["required"] = new OpenApiBoolean(true),
                        ["order"] = new OpenApiInteger(1)
                    }
                };
            }
            return op;
        });

        // (legacy /fields/create route removed)

        // Update field
        app.MapPut("/api/v1/forms/{formId:guid}/fields/{fieldId:guid}", async (Guid formId, Guid fieldId, UpdateFieldRequest req, [FromQuery] string tenantId, AppDbContext db, CancellationToken ct) =>
            {
                var field = await db.Fields
                    .Where(ff => ff.Id == fieldId && ff.FormId == formId)
                    .Join(db.Forms, ff => ff.FormId, f => f.Id, (ff, f) => new { ff, f })
                    .Where(x => x.f.TenantId == tenantId)
                    .Select(x => x.ff)
                    .FirstOrDefaultAsync(ct);
                if (field is null) return Results.NotFound();

                if (!string.IsNullOrWhiteSpace(req.Key))
                {
                    var newKey = req.Key.Trim();
                    if (!string.Equals(newKey, field.Key, StringComparison.Ordinal))
                    {
                        var existsKey = await db.Fields.AnyAsync(ff => ff.FormId == formId && ff.Key == newKey && ff.Id != fieldId, ct);
                        if (existsKey)
                        {
                            return Results.Problem(
                                title: "Conflict",
                                detail: "field key already exists for this form",
                                statusCode: 409,
                                type: "https://tools.ietf.org/html/rfc9110#section-15.5.10");
                        }
                        field.Key = newKey;
                    }
                }

                if (!string.IsNullOrWhiteSpace(req.Label)) field.Label = req.Label.Trim();
                if (!string.IsNullOrWhiteSpace(req.Type)) field.Type = req.Type.Trim();
                if (req.Required.HasValue) field.Required = req.Required.Value;
                if (req.Order.HasValue) field.Order = req.Order.Value;

                await db.SaveChangesAsync(ct);

                var dto = new FormFieldDto(field.Id, field.FormId, field.Key, field.Label, field.Type, field.Required, field.Order);
                return Results.Ok(dto);
            })
            .WithRequestValidation<UpdateFieldRequest>()
            .WithName("UpdateFormField")
            .WithOpenApi(op =>
            {
                if (op.Responses.TryGetValue("200", out var ok) && ok.Content.TryGetValue("application/json", out var media))
                {
                    media.Examples ??= new Dictionary<string, OpenApiExample>();
                    media.Examples["sample"] = new OpenApiExample
                    {
                        Summary = "Updated field",
                        Value = new OpenApiObject
                        {
                            ["id"] = new OpenApiString("6fa459ea-ee8a-3ca4-894e-db77e160355e"),
                            ["formId"] = new OpenApiString("3f2504e0-4f89-11d3-9a0c-0305e82c3301"),
                            ["key"] = new OpenApiString("email"),
                            ["label"] = new OpenApiString("Email Address"),
                            ["type"] = new OpenApiString("text"),
                            ["required"] = new OpenApiBoolean(true),
                            ["order"] = new OpenApiInteger(1)
                        }
                    };
                }
                return op;
            });

        // Delete field
        app.MapDelete("/api/v1/forms/{formId:guid}/fields/{fieldId:guid}", async (Guid formId, Guid fieldId, [FromQuery] string tenantId, AppDbContext db, CancellationToken ct) =>
            {
                var field = await db.Fields
                    .Where(ff => ff.Id == fieldId && ff.FormId == formId)
                    .Join(db.Forms, ff => ff.FormId, f => f.Id, (ff, f) => new { ff, f })
                    .Where(x => x.f.TenantId == tenantId)
                    .Select(x => x.ff)
                    .FirstOrDefaultAsync(ct);
                if (field is null) return Results.NotFound();

                db.Fields.Remove(field);
                await db.SaveChangesAsync(ct);
                return Results.NoContent();
            })
            .WithName("DeleteFormField");

        return app;
    }

    public record FormDto(
        Guid Id,
        string TenantId,
        string Name,
        string? Description,
        DateTime CreatedAtUtc);

    public record CreateFormRequest(
        [property: Required, MaxLength(128)] string TenantId,
        [property: Required, MaxLength(256)] string Name,
        [property: MaxLength(2000)] string? Description);

    public record UpdateFormRequest(
        [property: Required, MaxLength(128)] string TenantId,
        [property: Required, MaxLength(256)] string Name,
        [property: MaxLength(2000)] string? Description);

    public record FormFieldDto(
        Guid Id,
        Guid FormId,
        string Key,
        string Label,
        string Type,
        bool Required,
        int Order);

    public record CreateFieldRequest(
        [property: Required, MaxLength(128)] string Key,
        [property: Required, MaxLength(256)] string Label,
        [property: Required, MaxLength(64)] string Type,
        bool Required,
        [property: Range(0, int.MaxValue)] int Order);

    public record UpdateFieldRequest(
        [property: MaxLength(128)] string? Key,
        [property: MaxLength(256)] string? Label,
        [property: MaxLength(64)] string? Type,
        bool? Required,
        [property: Range(0, int.MaxValue)] int? Order);
}
