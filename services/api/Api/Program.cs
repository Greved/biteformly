using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using BiteForm.Application;
using BiteForm.Infrastructure;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Security.Claims;
using BiteForm.Api.Endpoints;
using FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.OperationFilter<BiteForm.Api.Infrastructure.OpenApi.PaginationOperationFilter>();
    c.OperationFilter<BiteForm.Api.Infrastructure.OpenApi.ExamplesOperationFilter>();
    c.SchemaFilter<BiteForm.Api.Infrastructure.OpenApi.FieldTypeSchemaFilter>();
});
// Standardize ProblemDetails responses for exceptions and error mapping
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<BiteForm.Api.Validators.CreateFormRequestValidator>();

// AuthN/Z (Supabase JWT)
var supabaseSection = builder.Configuration.GetSection("Supabase");
var supabaseJwtSecret = supabaseSection["JwtSecret"] ?? string.Empty;

if (!string.IsNullOrWhiteSpace(supabaseJwtSecret))
{
    builder.Services
        .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false; // dev convenience
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(supabaseJwtSecret)),
                ClockSkew = TimeSpan.FromMinutes(2)
            };
        });
}

builder.Services.AddAuthorization();

// Register Application/Infrastructure services via extension methods
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Middleware
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "BiteForm API v1");
        options.RoutePrefix = "swagger";
    });
}

// In non-development, convert unhandled exceptions to RFC 7807 problem+json
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
}

// Return RFC 7807 problem+json for common status codes (e.g., 404, 405)
app.UseStatusCodePages(async context =>
{
    var response = context.HttpContext.Response;
    switch (response.StatusCode)
    {
        case StatusCodes.Status404NotFound:
            await Results.Problem(
                    title: "Not Found",
                    statusCode: StatusCodes.Status404NotFound,
                    type: "https://www.rfc-editor.org/rfc/rfc9110.html#name-404-not-found")
                .ExecuteAsync(context.HttpContext);
            break;
        case StatusCodes.Status405MethodNotAllowed:
            await Results.Problem(
                    title: "Method Not Allowed",
                    statusCode: StatusCodes.Status405MethodNotAllowed,
                    type: "https://www.rfc-editor.org/rfc/rfc9110.html#name-405-method-not-allowed")
                .ExecuteAsync(context.HttpContext);
            break;
    }
});

if (!string.IsNullOrWhiteSpace(supabaseJwtSecret))
{
    app.UseAuthentication();
}
app.UseAuthorization();

// Health
app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .WithName("Health")
    .WithTags("System");

// API v1 group (sample)
app.MapFormsEndpoints();
app.MapSubmissionsEndpoints();

// Auth check
app.MapGet("/api/v1/me", (ClaimsPrincipal user) =>
{
    if (user.Identity?.IsAuthenticated != true)
    {
        return Results.Unauthorized();
    }

    var sub = user.FindFirst("sub")?.Value;
    var email = user.FindFirst("email")?.Value;
    var name = user.Identity?.Name;
    return Results.Ok(new { sub, email, name });
})
.RequireAuthorization()
.WithName("Me")
.WithTags("Auth");

// Version
app.MapGet("/version", () =>
{
    var asm = Assembly.GetExecutingAssembly().GetName();
    return Results.Ok(new { name = asm.Name, version = asm.Version?.ToString() ?? "unknown" });
}).WithName("Version").WithTags("System");

app.Run();
