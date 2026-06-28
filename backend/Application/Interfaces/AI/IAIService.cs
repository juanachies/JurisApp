using JurisApp.Application.DTOs.AITasks;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;

namespace JurisApp.Application.Interfaces.AI;

public interface IAIService
{
    Task<string> SendChatMessageAsync(
        string userMessage,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        CancellationToken cancellationToken = default);

    Task<DocumentAnalysisResult> AnalyzeDocumentAsync(
        string documentText,
        DocumentAnalysisType analysisType,
        IReadOnlyList<CustomSkill> activeSkills,
        CancellationToken cancellationToken = default);

    Task<StructuredTaskPlan> CreateStructuredTaskPlanAsync(
        string description,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        CancellationToken cancellationToken = default);

    Task<string> ExecuteTaskStepAsync(
        string taskDescription,
        TaskStepDto step,
        IReadOnlyList<TaskStepDto> completedSteps,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        CancellationToken cancellationToken = default);
}
