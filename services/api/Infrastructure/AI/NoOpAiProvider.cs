using BiteForm.Application.AI;

namespace BiteForm.Infrastructure;

public sealed class NoOpAiProvider : IAIProvider
{
    public Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken = default)
    {
        // Echo back the prompt to keep things deterministic and cost-free
        var response = new AiResponse($"echo: {request.Prompt}", 0, 0, 0m);
        return Task.FromResult(response);
    }
}

