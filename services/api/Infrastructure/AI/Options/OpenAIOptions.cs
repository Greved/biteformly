namespace BiteForm.Infrastructure.AI.Options;

public sealed class OpenAIOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string BaseUrl { get; set; } = "https://api.openai.com";
    public string Model { get; set; } = "gpt-4o-mini";
}

