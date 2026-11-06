using JurisApp.Application.DTOs.AITasks;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace JurisApp.TpiTests.Fixtures;

public sealed record CapturedChatTurn(
    string UserMessage,
    IReadOnlyList<string> PreviousContents,
    IReadOnlyList<ChatDocumentContext> Documents);

public sealed class CapturingAIService : IAIService
{
    public const string InventedAnalysisSentence = "CAPITALIZACION_INVENTADA_POR_EL_ANALISIS";

    private readonly List<CapturedChatTurn> _turns = new();

    public IReadOnlyList<CapturedChatTurn> Turns
    {
        get
        {
            lock (_turns)
                return _turns.ToList();
        }
    }

    public void Clear()
    {
        lock (_turns)
            _turns.Clear();
    }

    public Task<string> SendChatMessageAsync(
        string userMessage,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        string? userProvince = null,
        CancellationToken cancellationToken = default)
    {
        lock (_turns)
        {
            _turns.Add(new CapturedChatTurn(
                userMessage,
                previousMessages.Select(m => m.Content).ToList(),
                chatDocuments?.ToList() ?? []));
        }

        return Task.FromResult("Respuesta de prueba.");
    }

    public Task<DocumentAnalysisResult> AnalyzeDocumentAsync(
        string documentText,
        DocumentAnalysisType analysisType,
        IReadOnlyList<CustomSkill> activeSkills,
        string? userProvince = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new DocumentAnalysisResult
        {
            Summary = InventedAnalysisSentence,
            Risks = InventedAnalysisSentence,
            Recommendations = InventedAnalysisSentence,
            References = string.Empty
        });

    public Task<StructuredTaskPlan> CreateStructuredTaskPlanAsync(
        string description,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        string? userProvince = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult(new StructuredTaskPlan
        {
            Objective = description,
            Summary = "Plan de prueba.",
            Steps = [new StructuredTaskStep { Order = 1, Title = "Paso", Description = "Paso de prueba." }]
        });

    public Task<string> ExecuteTaskStepAsync(
        string taskDescription,
        TaskStepDto step,
        IReadOnlyList<TaskStepDto> completedSteps,
        IReadOnlyList<Message> previousMessages,
        IReadOnlyList<CustomSkill> activeSkills,
        IReadOnlyList<ChatDocumentContext>? chatDocuments = null,
        string? userProvince = null,
        CancellationToken cancellationToken = default)
        => Task.FromResult("Paso de prueba.");
}

public sealed class CapturingAiFactory : JurisAppApiFactory
{
    public CapturingAIService Ai { get; } = new();

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IAIService>();
            services.AddSingleton<IAIService>(Ai);
        });
    }
}
