namespace JurisApp.Infrastructure.AI;

public class OpenAIOptions
{
    public const string SectionName = "AI:OpenAI";

    public bool Enabled { get; set; } = true;
    public string? ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://api.openai.com/v1";
    public string Model { get; set; } = "gpt-4o-mini";
    public int MaxTokens { get; set; } = 1024;
    public int HttpTimeoutSeconds { get; set; } = 120;
}
