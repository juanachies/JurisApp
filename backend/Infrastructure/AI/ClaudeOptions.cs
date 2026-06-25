namespace JurisApp.Infrastructure.AI;

public class ClaudeOptions
{
    public const string SectionName = "AI:Claude";

    public bool Enabled { get; set; } = true;
    public string? ApiKey { get; set; }
    public string BaseUrl { get; set; } = "https://api.anthropic.com";
    // Snapshot válido. Evitá hardcodear si ya configuraste AI:Claude:Model.
    public string Model { get; set; } = "claude-sonnet-4-6";
    public int MaxTokens { get; set; } = 2048;
    public int SegmentedAnalysisMaxTokens { get; set; } = 16384;
    /// <summary>Tiempo máximo de espera HTTP a Anthropic (análisis segmentado puede tardar varios minutos).</summary>
    public int HttpTimeoutSeconds { get; set; } = 600;
}
