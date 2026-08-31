namespace JurisApp.Infrastructure.AI;

public class DeepSeekOptions
{
    public const string SectionName = "AI:DeepSeek";

    public bool Enabled { get; set; } = true;
    public string? ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://api.deepseek.com";
    public string Model { get; set; } = "deepseek-v4-pro";
    public int MaxTokens { get; set; } = 8192;
    /// <summary>Tiempo máximo de espera HTTP a DeepSeek (respuestas con thinking pueden tardar).</summary>
    public int HttpTimeoutSeconds { get; set; } = 600;
}
