using JurisApp.Application.Interfaces.AI;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;

namespace JurisApp.Infrastructure.AI;

public class MockAIService : IAIService
{
    public Task<string> SendChatMessageAsync(
        string userMessage,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        CancellationToken cancellationToken = default)
    {
        var skillNote = activeSkills.Any(s => s.IsActive)
            ? $" (con {activeSkills.Count(s => s.IsActive)} skill(s) activa(s))"
            : string.Empty;

        var reply =
            $"[Respuesta simulada{skillNote}] Recibí tu mensaje: \"{userMessage}\". " +
            "Esta es una respuesta de desarrollo. Configurá AI:UseMock=false y una API key real para usar Claude.";

        return Task.FromResult(reply);
    }

    public Task<DocumentAnalysisResult> AnalyzeDocumentAsync(
        string documentText,
        DocumentAnalysisType analysisType,
        IReadOnlyList<CustomSkill> activeSkills,
        CancellationToken cancellationToken = default)
    {
        var preview = documentText.Length > 100
            ? documentText[..100] + "..."
            : documentText;

        return Task.FromResult(new DocumentAnalysisResult
        {
            Summary = "Análisis simulado: el documento fue recibido correctamente.",
            Risks = $"[Simulado] Tipo de análisis: {analysisType}. Texto recibido ({documentText.Length} caracteres): {preview}",
            Recommendations = "Revisar el documento con un abogado matriculado antes de tomar decisiones.",
            References = "N/A (modo desarrollo)"
        });
    }

    public Task<string> CreateTaskPlanAsync(
        string description,
        CancellationToken cancellationToken = default)
    {
        var plan =
            $"[Plan simulado para: {description}]\n\n" +
            "1. Revisar la documentación relevante\n" +
            "2. Identificar partes involucradas y plazos\n" +
            "3. Elaborar borrador de acciones\n" +
            "4. Consultar con el cliente\n" +
            "5. Ejecutar y dar seguimiento";

        return Task.FromResult(plan);
    }
}
