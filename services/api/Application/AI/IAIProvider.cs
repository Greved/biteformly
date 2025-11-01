namespace BiteForm.Application.AI;

public record AiRequest(string Prompt);
public record AiResponse(string Output, int PromptTokens = 0, int CompletionTokens = 0, decimal EstimatedCost = 0m);

public interface IAIProvider
{
    Task<AiResponse> CompleteAsync(AiRequest request, CancellationToken cancellationToken = default);
}

