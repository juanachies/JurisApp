using JurisApp.Domain.Enums;

namespace JurisApp.Application.DTOs.Documents;

public class DocumentAnalysisDto
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public DocumentAnalysisType Type { get; set; }
    public string Summary { get; set; } = string.Empty;
    public string Risks { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public string References { get; set; } = string.Empty;
}
