using Microsoft.Extensions.DependencyInjection;
namespace BiteForm.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register application-level services, validators, and abstractions
        services.AddOptions();
        return services;
    }
}
