namespace JurisApp.Application.Interfaces.AI;

public sealed class DocumentClassificationResult
{
    public string CategoryKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public string Reason { get; set; } = string.Empty;
    public Dictionary<string, object> MainFields { get; set; } = new();
}
