namespace JurisApp.Application.DTOs.Analysis;

public sealed class SegmentedDocumentAnalysisDto
{
    public Guid? Id { get; set; }
    public Guid? DocumentId { get; set; }
    public string CategoryKey { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public decimal Confidence { get; set; }
    public Dictionary<string, object> MainFields { get; set; } = new();
    public List<DocumentAnalysisSegmentDto> Segments { get; set; } = new();
    public List<SuggestedActionDto> SuggestedActions { get; set; } = new();
}

public sealed class DocumentAnalysisSegmentDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public bool Countable { get; set; }
    public int? ItemsCount { get; set; }
    public string Severity { get; set; } = "neutral";
    public string Content { get; set; } = string.Empty;
    public List<DocumentAnalysisSegmentItemDto> Items { get; set; } = new();
}

public sealed class DocumentAnalysisSegmentItemDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Severity { get; set; } = "neutral";
    public string Recommendation { get; set; } = string.Empty;
}

public sealed class SuggestedActionDto
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
}
