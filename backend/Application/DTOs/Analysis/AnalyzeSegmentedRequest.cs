namespace JurisApp.Application.DTOs.Analysis;

public sealed class AnalyzeSegmentedRequest
{
    public Guid ChatId { get; set; }
    public Guid? DocumentId { get; set; }
    public string? Input { get; set; }
}
