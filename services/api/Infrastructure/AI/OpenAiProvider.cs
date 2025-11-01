using BiteForm.Application.AI;
using BiteForm.Infrastructure.AI.Options;
using Microsoft.Extensions.Options;

namespace BiteForm.Infrastructure.AI;

public sealed class OpenAiProvider : IAIProvider
{
    private readonly OpenAIOptions _options;

    public OpenAiProvider(IOptions<OpenAIOptions> options)
    {
        _options = options.Value;
    }

    public Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        // Placeholder implementation to keep vendor-agnostic and offline-friendly
        var output = $"openai({_options.Model}): {request.Prompt}";
        return Task.FromResult(new AiResponse(output));
    }
}

