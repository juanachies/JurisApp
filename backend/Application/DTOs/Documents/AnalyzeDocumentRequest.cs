using JurisApp.Domain.Enums;

namespace JurisApp.Application.DTOs.Documents;

public class AnalyzeDocumentRequest
{
    public Guid DocumentId { get; set; }
    public DocumentAnalysisType? Type { get; set; }
    public IReadOnlyList<DocumentAnalysisType>? Types { get; set; }
    public IReadOnlyList<Guid>? CustomSkillIds { get; set; }
}
