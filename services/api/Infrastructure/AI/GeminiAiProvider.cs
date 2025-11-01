using BiteForm.Application.AI;
using BiteForm.Infrastructure.AI.Options;
using Microsoft.Extensions.Options;

namespace BiteForm.Infrastructure.AI;

public sealed class GeminiAiProvider : IAIProvider
{
    private readonly GeminiOptions _options;

    public GeminiAiProvider(IOptions<GeminiOptions> options)
    {
        _options = options.Value;
    }

    public Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        // Placeholder implementation to keep vendor-agnostic and offline-friendly
        var output = $"gemini({_options.Model}): {request.Prompt}";
        return Task.FromResult(new AiResponse(output));
    }
}
