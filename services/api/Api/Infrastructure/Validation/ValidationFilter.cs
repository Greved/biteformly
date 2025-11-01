using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace BiteForm.Api.Infrastructure.Validation;

public sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var validator = context.HttpContext.RequestServices.GetService(typeof(IValidator<T>)) as IValidator<T>;
        if (validator is null)
            return await next(context);

        var instance = context.Arguments.OfType<T>().FirstOrDefault();
        if (instance is null)
            return await next(context);

        var result = await validator.ValidateAsync(instance);
        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            return Results.ValidationProblem(errors);
        }

        return await next(context);
    }
}

public static class ValidationExtensions
{
    public static RouteHandlerBuilder WithRequestValidation<T>(this RouteHandlerBuilder builder) where T : class
        => builder.AddEndpointFilter(new ValidationFilter<T>());
}
