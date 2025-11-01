namespace BiteForm.Infrastructure.AI.Options;

public sealed class GeminiOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string Endpoint { get; set; } = "https://generativelanguage.googleapis.com";
    public string Model { get; set; } = "gemini-1.5-pro";
}

