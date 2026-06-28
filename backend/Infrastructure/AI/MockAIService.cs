using JurisApp.Application.DTOs.AITasks;
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
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        CancellationToken cancellationToken = default)
    {
        var skillNote = activeSkills.Any(s => s.IsActive)
            ? $" (con {activeSkills.Count(s => s.IsActive)} skill(s) activa(s))"
            : string.Empty;

        var docNote = chatDocuments is { Count: > 0 }
            ? $" Se tienen en cuenta {chatDocuments.Count} documento(s) adjunto(s)."
            : string.Empty;

        var reply =
            $"[Respuesta simulada{skillNote}]{docNote} Recibí tu mensaje: \"{userMessage}\". " +
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

    public Task<StructuredTaskPlan> CreateStructuredTaskPlanAsync(
        string description,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(TaskPlanParser.BuildMockPlan(description));

    public Task<string> ExecuteTaskStepAsync(
        string taskDescription,
        TaskStepDto step,
        IReadOnlyList<TaskStepDto> completedSteps,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        CancellationToken cancellationToken = default)
    {
        var result =
            $"[Paso {step.Order} simulado: {step.Title}]\n\n" +
            $"Desarrollo para el encargo: {taskDescription[..Math.Min(taskDescription.Length, 200)]}...\n\n" +
            $"Entregable simulado:\n- {step.Description}\n\n" +
            (completedSteps.Count > 0
                ? $"Pasos previos completados: {string.Join(", ", completedSteps.Select(s => s.Order))}."
                : "Primer paso del plan.");

        return Task.FromResult(result);
    }
}
