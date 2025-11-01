using BiteForm.Api.Endpoints;
using BiteForm.Application.Abstractions.Time;
using BiteForm.Domain.Entities;
using BiteForm.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using BiteForm.Api.Infrastructure;
using BiteForm.Api.Infrastructure.Validation;
using System.ComponentModel.DataAnnotations;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Any;

namespace BiteForm.Api.Endpoints;

public static class SubmissionsEndpoints
{
    public static IEndpointRouteBuilder MapSubmissionsEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/v1").WithTags("v1");
        var submissions = api.MapGroup("/forms/{formId:guid}/submissions").WithTags("Submissions");

        // List submissions (metadata only)
        submissions.MapGet("/", async (Guid formId, [FromQuery] string tenantId, [FromQuery] string? submittedBy, [FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? order, [FromQuery] int page, [FromQuery] int pageSize, AppDbContext db, CancellationToken ct) =>
        {
            var owns = await db.Forms.AnyAsync(f => f.Id == formId && f.TenantId == tenantId, ct);
            if (!owns) return Results.NotFound();

            var (p, ps) = Paging.Normalize(page, pageSize);
            var query = db.Submissions.Where(s => s.FormId == formId).AsQueryable();
            if (!string.IsNullOrWhiteSpace(submittedBy))
            {
                var term = submittedBy.Trim();
                query = query.Where(s => s.SubmittedBy != null && s.SubmittedBy.Contains(term));
            }
            if (from.HasValue) query = query.Where(s => s.SubmittedAtUtc >= from.Value);
            if (to.HasValue) query = query.Where(s => s.SubmittedAtUtc <= to.Value);

            var descending = string.Equals(order, "asc", StringComparison.OrdinalIgnoreCase) ? false : true; // default desc
            query = descending ? query.OrderByDescending(s => s.SubmittedAtUtc) : query.OrderBy(s => s.SubmittedAtUtc);

            var shaped = query.Select(s => new SubmissionDto(s.Id, s.FormId, s.TenantId, s.SubmittedAtUtc, s.SubmittedBy, new List<FormResponseDto>()));
            var paged = await shaped.ToPagedResultAsync(p, ps, ct);
            return Results.Ok(paged);
        })
        .WithName("ListSubmissions")
        .WithOpenApi(op =>
        {
            foreach (var p in op.Parameters)
            {
                switch (p.Name)
                {
                    case "tenantId":
                        p.Description = "Tenant identifier (required)";
                        p.Required = true;
                        break;
                    case "submittedBy":
                        p.Description = "Filter by submittedBy (contains)";
                        break;
                    case "from":
                        p.Description = "Filter submissions from this UTC timestamp (inclusive)";
                        break;
                    case "to":
                        p.Description = "Filter submissions up to this UTC timestamp (inclusive)";
                        break;
                    case "order":
                        p.Description = "Sort order by submittedAt (asc | desc). Default desc";
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

        // Get submission with responses
        submissions.MapGet("/{submissionId:guid}", async (Guid formId, Guid submissionId, [FromQuery] string tenantId, AppDbContext db, CancellationToken ct) =>
        {
            var item = await db.Submissions
                .Where(s => s.Id == submissionId && s.FormId == formId)
                .Join(db.Forms, s => s.FormId, f => f.Id, (s, f) => new { s, f })
                .Where(x => x.f.TenantId == tenantId)
                .Select(x => x.s)
                .FirstOrDefaultAsync(ct);
            if (item is null) return Results.NotFound();

            var responses = await db.Responses
                .Where(r => r.SubmissionId == item.Id)
                .Select(r => new FormResponseDto(r.Id, r.SubmissionId, r.FieldId, r.Value ?? string.Empty))
                .ToListAsync(ct);

            return Results.Ok(new SubmissionDto(item.Id, item.FormId, item.TenantId, item.SubmittedAtUtc, item.SubmittedBy, responses));
        }).WithName("GetSubmission");

        // Create submission (with responses)
        submissions.MapPost("/", async (Guid formId, [FromQuery] string tenantId, CreateSubmissionRequest req, AppDbContext db, IDateTimeProvider clock, CancellationToken ct) =>
        {
            var form = await db.Forms.FirstOrDefaultAsync(f => f.Id == formId && f.TenantId == tenantId, ct);
            if (form is null) return Results.NotFound();

            var subId = Guid.NewGuid();
            var submission = new FormSubmission
            {
                Id = subId,
                FormId = formId,
                TenantId = tenantId,
                SubmittedAtUtc = clock.UtcNow,
                SubmittedBy = string.IsNullOrWhiteSpace(req.SubmittedBy) ? null : req.SubmittedBy!.Trim()
            };

            db.Submissions.Add(submission);

            if (req.Responses is { Count: > 0 })
            {
                // Validate field ownership and create responses
                var fieldIds = req.Responses.Select(r => r.FieldId).ToHashSet();
                var validFieldIds = await db.Fields.Where(ff => ff.FormId == formId && fieldIds.Contains(ff.Id)).Select(ff => ff.Id).ToListAsync(ct);
                var invalid = fieldIds.Except(validFieldIds).ToList();
                if (invalid.Count > 0)
                {
                    var errors = new Dictionary<string, string[]>
                    {
                        ["fieldIds"] = new[] { "One or more fieldIds are invalid for this form" }
                    };
                    return Results.ValidationProblem(errors);
                }

                foreach (var r in req.Responses)
                {
                    db.Responses.Add(new FormResponse
                    {
                        Id = Guid.NewGuid(),
                        SubmissionId = subId,
                        FieldId = r.FieldId,
                        Value = r.Value ?? string.Empty
                    });
                }
            }

            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/forms/{formId}/submissions/{subId}", new { id = subId });
        })
        .WithRequestValidation<CreateSubmissionRequest>()
        .WithName("CreateSubmission")
        .WithOpenApi(op =>
        {
            // request example
            var json = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["submittedBy"] = new Microsoft.OpenApi.Any.OpenApiString("user@example.com"),
                ["responses"] = new Microsoft.OpenApi.Any.OpenApiArray
                {
                    new Microsoft.OpenApi.Any.OpenApiObject
                    {
                        ["fieldId"] = new Microsoft.OpenApi.Any.OpenApiString("6fa459ea-ee8a-3ca4-894e-db77e160355e"),
                        ["value"] = new Microsoft.OpenApi.Any.OpenApiString("hello")
                    }
                }
            };
            if (op.RequestBody?.Content.TryGetValue("application/json", out var media) == true)
            {
                media.Examples ??= new Dictionary<string, Microsoft.OpenApi.Models.OpenApiExample>();
                media.Examples["sample"] = new Microsoft.OpenApi.Models.OpenApiExample
                {
                    Summary = "Create submission",
                    Value = json
                };
            }
            return op;
        });

        // Update submission (metadata only)
        submissions.MapPut("/{submissionId:guid}", async (Guid formId, Guid submissionId, [FromQuery] string tenantId, UpdateSubmissionRequest req, AppDbContext db, CancellationToken ct) =>
        {
            var submission = await db.Submissions
                .Where(s => s.Id == submissionId && s.FormId == formId)
                .Join(db.Forms, s => s.FormId, f => f.Id, (s, f) => new { s, f })
                .Where(x => x.f.TenantId == tenantId)
                .Select(x => x.s)
                .FirstOrDefaultAsync(ct);
            if (submission is null) return Results.NotFound();

            if (req.SubmittedBy is not null)
                submission.SubmittedBy = string.IsNullOrWhiteSpace(req.SubmittedBy) ? null : req.SubmittedBy.Trim();

            await db.SaveChangesAsync(ct);
            return Results.Ok(new { submission.Id, submission.FormId, submission.TenantId, submission.SubmittedAtUtc, submission.SubmittedBy });
        }).WithRequestValidation<UpdateSubmissionRequest>().WithName("UpdateSubmission");

        // Delete submission (cascades responses)
        submissions.MapDelete("/{submissionId:guid}", async (Guid formId, Guid submissionId, [FromQuery] string tenantId, AppDbContext db, CancellationToken ct) =>
        {
            var submission = await db.Submissions
                .Where(s => s.Id == submissionId && s.FormId == formId)
                .Join(db.Forms, s => s.FormId, f => f.Id, (s, f) => new { s, f })
                .Where(x => x.f.TenantId == tenantId)
                .Select(x => x.s)
                .FirstOrDefaultAsync(ct);
            if (submission is null) return Results.NotFound();

            db.Submissions.Remove(submission);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("DeleteSubmission");

        // Nested responses group
        var responses = submissions.MapGroup("/{submissionId:guid}/responses").WithTags("Responses");

        // List responses
        responses.MapGet("/", async (Guid formId, Guid submissionId, [FromQuery] string tenantId, [FromQuery] int page, [FromQuery] int pageSize, AppDbContext db, CancellationToken ct) =>
        {
            var own = await db.Submissions
                .Where(s => s.Id == submissionId && s.FormId == formId)
                .Join(db.Forms, s => s.FormId, f => f.Id, (s, f) => new { s, f })
                .AnyAsync(x => x.f.TenantId == tenantId, ct);
            if (!own) return Results.NotFound();

            var (p, ps) = Paging.Normalize(page, pageSize);
            var shaped = db.Responses
                .Where(r => r.SubmissionId == submissionId)
                .Select(r => new FormResponseDto(r.Id, r.SubmissionId, r.FieldId, r.Value ?? string.Empty));
            var paged = await shaped.ToPagedResultAsync(p, ps, ct);
            return Results.Ok(paged);
        })
        .WithName("ListResponses")
        .WithOpenApi(op =>
        {
            foreach (var p in op.Parameters)
            {
                switch (p.Name)
                {
                    case "tenantId":
                        p.Description = "Tenant identifier (required)";
                        p.Required = true;
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

        // Create response
        responses.MapPost("/", async (Guid formId, Guid submissionId, [FromQuery] string tenantId, CreateResponseRequest req, AppDbContext db, CancellationToken ct) =>
        {
            var sub = await db.Submissions
                .Where(s => s.Id == submissionId && s.FormId == formId)
                .Join(db.Forms, s => s.FormId, f => f.Id, (s, f) => new { s, f })
                .Where(x => x.f.TenantId == tenantId)
                .Select(x => x.s)
                .FirstOrDefaultAsync(ct);
            if (sub is null) return Results.NotFound();

            var fieldOwns = await db.Fields.AnyAsync(ff => ff.Id == req.FieldId && ff.FormId == formId, ct);
            if (!fieldOwns)
            {
                var errors = new Dictionary<string, string[]>
                {
                    ["fieldId"] = new[] { "Invalid fieldId for form" }
                };
                return Results.ValidationProblem(errors);
            }

            // avoid duplicate response per field per submission
            var exists = await db.Responses.AnyAsync(r => r.SubmissionId == submissionId && r.FieldId == req.FieldId, ct);
            if (exists)
            {
                return Results.Problem(
                    title: "Conflict",
                    detail: "response for this field already exists",
                    statusCode: 409,
                    type: "https://tools.ietf.org/html/rfc9110#section-15.5.10");
            }

            var id = Guid.NewGuid();
            db.Responses.Add(new FormResponse { Id = id, SubmissionId = submissionId, FieldId = req.FieldId, Value = req.Value ?? string.Empty });
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/forms/{formId}/submissions/{submissionId}/responses/{id}", new { id });
        })
        .WithRequestValidation<CreateResponseRequest>()
        .WithName("CreateResponse")
        .WithOpenApi(op =>
        {
            var json = new Microsoft.OpenApi.Any.OpenApiObject
            {
                ["fieldId"] = new Microsoft.OpenApi.Any.OpenApiString("6fa459ea-ee8a-3ca4-894e-db77e160355e"),
                ["value"] = new Microsoft.OpenApi.Any.OpenApiString("hello")
            };
            if (op.RequestBody?.Content.TryGetValue("application/json", out var media) == true)
            {
                media.Examples ??= new Dictionary<string, Microsoft.OpenApi.Models.OpenApiExample>();
                media.Examples["sample"] = new Microsoft.OpenApi.Models.OpenApiExample
                {
                    Summary = "Create response",
                    Value = json
                };
            }
            return op;
        });

        // Update response
        responses.MapPut("/{responseId:guid}", async (Guid formId, Guid submissionId, Guid responseId, [FromQuery] string tenantId, UpdateResponseRequest req, AppDbContext db, CancellationToken ct) =>
        {
            var resp = await db.Responses
                .Where(r => r.Id == responseId && r.SubmissionId == submissionId)
                .Join(db.Submissions, r => r.SubmissionId, s => s.Id, (r, s) => new { r, s })
                .Join(db.Forms, rs => rs.s.FormId, f => f.Id, (rs, f) => new { rs, f })
                .Where(x => x.f.Id == formId && x.f.TenantId == tenantId)
                .Select(x => x.rs.r)
                .FirstOrDefaultAsync(ct);
            if (resp is null) return Results.NotFound();

            if (req.Value is not null) resp.Value = req.Value;
            await db.SaveChangesAsync(ct);
            return Results.Ok(new { resp.Id, resp.SubmissionId, resp.FieldId, resp.Value });
        }).WithRequestValidation<UpdateResponseRequest>().WithName("UpdateResponse");

        // Delete response
        responses.MapDelete("/{responseId:guid}", async (Guid formId, Guid submissionId, Guid responseId, [FromQuery] string tenantId, AppDbContext db, CancellationToken ct) =>
        {
            var resp = await db.Responses
                .Where(r => r.Id == responseId && r.SubmissionId == submissionId)
                .Join(db.Submissions, r => r.SubmissionId, s => s.Id, (r, s) => new { r, s })
                .Join(db.Forms, rs => rs.s.FormId, f => f.Id, (rs, f) => new { rs, f })
                .Where(x => x.f.Id == formId && x.f.TenantId == tenantId)
                .Select(x => x.rs.r)
                .FirstOrDefaultAsync(ct);
            if (resp is null) return Results.NotFound();

            db.Responses.Remove(resp);
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        }).WithName("DeleteResponse");

        return app;
    }

    public record SubmissionDto(Guid Id, Guid FormId, string TenantId, DateTime SubmittedAtUtc, string? SubmittedBy, List<FormResponseDto> Responses);
    public record CreateSubmissionRequest(
        [property: MaxLength(256)] string? SubmittedBy,
        List<CreateResponseItem> Responses);
    public record UpdateSubmissionRequest(
        [property: MaxLength(256)] string? SubmittedBy);
    public record CreateResponseItem(
        [property: Required] Guid FieldId,
        [property: MaxLength(4000)] string? Value);
    public record CreateResponseRequest(
        [property: Required] Guid FieldId,
        [property: MaxLength(4000)] string? Value);
    public record UpdateResponseRequest(
        [property: MaxLength(4000)] string? Value);
    public record FormResponseDto(Guid Id, Guid SubmissionId, Guid FieldId, string Value);
}
