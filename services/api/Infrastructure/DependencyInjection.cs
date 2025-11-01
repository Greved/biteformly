using BiteForm.Application.AI;
using BiteForm.Application.Abstractions.Time;
using BiteForm.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BiteForm.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Time provider
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();

        // Options binding
        services.Configure<AI.Options.OpenAIOptions>(configuration.GetSection("AI:OpenAI"));
        services.Configure<AI.Options.GeminiOptions>(configuration.GetSection("AI:Gemini"));

        // DbContext (PostgreSQL via Npgsql) – add only if connection string is present and not in testing override
        var useInMemoryOverride = string.Equals(configuration["UseInMemoryDb"], "true", StringComparison.OrdinalIgnoreCase);
        var connString = configuration.GetConnectionString("Default");
        if (useInMemoryOverride)
        {
            services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("biteform_tests"));
        }
        else if (!string.IsNullOrWhiteSpace(connString))
        {
            services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connString));
        }

        // AI provider selection (env-driven via AI:Provider or AI__Provider)
        var provider = configuration["AI:Provider"]?.Trim() ?? "NoOp";
        switch (provider)
        {
            case "OpenAI":
                services.AddSingleton<IAIProvider, AI.OpenAiProvider>();
                break;
            case "Gemini":
                services.AddSingleton<IAIProvider, AI.GeminiAiProvider>();
                break;
            default:
                services.AddSingleton<IAIProvider, NoOpAiProvider>();
                break;
        }

        return services;
    }
}
