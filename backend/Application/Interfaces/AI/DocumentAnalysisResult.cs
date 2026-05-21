namespace JurisApp.Application.Interfaces.AI;

public class DocumentAnalysisResult
{
    public string Summary { get; set; } = string.Empty;
    public string Risks { get; set; } = string.Empty;
    public string Recommendations { get; set; } = string.Empty;
    public string References { get; set; } = string.Empty;
}
