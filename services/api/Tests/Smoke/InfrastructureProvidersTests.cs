using BiteForm.Application.AI;
using BiteForm.Infrastructure;
using BiteForm.Infrastructure.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;

namespace BiteForm.Tests.Smoke;

[TestFixture]
public class InfrastructureProvidersTests
{
    [Test]
    public async Task Uses_NoOp_As_Default_Provider()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();

        services.AddInfrastructure(config);
        var sp = services.BuildServiceProvider();

        var provider = sp.GetRequiredService<IAIProvider>();
        var response = await provider.CompleteAsync(new AiRequest("ping"));

        Assert.That(provider, Is.InstanceOf<NoOpAiProvider>());
        Assert.That(response.Output, Does.Contain("echo:"));
    }

    [Test]
    public async Task Binds_OpenAI_When_Configured()
    {
        var dict = new Dictionary<string, string?>
        {
            ["AI:Provider"] = "OpenAI",
            ["AI:OpenAI:Model"] = "gpt-4o-mini"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        var services = new ServiceCollection().AddInfrastructure(config);
        var sp = services.BuildServiceProvider();

        var provider = sp.GetRequiredService<IAIProvider>();
        var response = await provider.CompleteAsync(new AiRequest("ping"));

        Assert.That(provider, Is.InstanceOf<OpenAiProvider>());
        Assert.That(response.Output, Does.StartWith("openai("));
    }

    [Test]
    public async Task Binds_Gemini_When_Configured()
    {
        var dict = new Dictionary<string, string?>
        {
            ["AI:Provider"] = "Gemini",
            ["AI:Gemini:Model"] = "gemini-1.5-pro"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(dict!).Build();
        var services = new ServiceCollection().AddInfrastructure(config);
        var sp = services.BuildServiceProvider();

        var provider = sp.GetRequiredService<IAIProvider>();
        var response = await provider.CompleteAsync(new AiRequest("ping"));

        Assert.That(provider, Is.InstanceOf<GeminiAiProvider>());
        Assert.That(response.Output, Does.StartWith("gemini("));
    }
}

