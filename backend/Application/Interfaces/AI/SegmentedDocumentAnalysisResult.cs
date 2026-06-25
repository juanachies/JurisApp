namespace JurisApp.Application.Interfaces.AI;

public sealed class SegmentedDocumentAnalysisResult
{
    public string CategoryKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public Dictionary<string, object> MainFields { get; set; } = new();
    public List<DocumentAnalysisSegmentResult> Segments { get; set; } = new();
    public List<SuggestedActionResult> SuggestedActions { get; set; } = new();
}

public sealed class DocumentAnalysisSegmentResult
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool Countable { get; set; }
    public int? ItemsCount { get; set; }
    public string Severity { get; set; } = "neutral";
    public string Content { get; set; } = string.Empty;
    public List<DocumentAnalysisSegmentItemResult> Items { get; set; } = new();
}

public sealed class DocumentAnalysisSegmentItemResult
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "neutral";
    public string Recommendation { get; set; } = string.Empty;
}

public sealed class SuggestedActionResult
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
