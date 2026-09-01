using JurisApp.Application.Common;
using JurisApp.Application.DTOs.AITasks;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Application.Mappings;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;

namespace JurisApp.Application.Services;

public class AITaskService : IAITaskService
{
    private readonly IChatRepository _chatRepository;
    private readonly IAITaskRepository _aiTaskRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly ICustomSkillRepository _customSkillRepository;
    private readonly ILawyerProfileRepository _lawyerProfileRepository;
    private readonly IChatDocumentContextService _chatDocumentContextService;
    private readonly IAIService _aiService;
    private readonly IPlanLimitService _planLimitService;
    private readonly IChatAuditService _chatAuditService;
    private readonly IAITaskExecutionQueue _executionQueue;
    private readonly IUnitOfWork _unitOfWork;

    public AITaskService(
        IChatRepository chatRepository,
        IAITaskRepository aiTaskRepository,
        IMessageRepository messageRepository,
        ICustomSkillRepository customSkillRepository,
        ILawyerProfileRepository lawyerProfileRepository,
        IChatDocumentContextService chatDocumentContextService,
        IAIService aiService,
        IPlanLimitService planLimitService,
        IChatAuditService chatAuditService,
        IAITaskExecutionQueue executionQueue,
        IUnitOfWork unitOfWork)
    {
        _chatRepository = chatRepository;
        _aiTaskRepository = aiTaskRepository;
        _messageRepository = messageRepository;
        _customSkillRepository = customSkillRepository;
        _lawyerProfileRepository = lawyerProfileRepository;
        _chatDocumentContextService = chatDocumentContextService;
        _aiService = aiService;
        _planLimitService = planLimitService;
        _chatAuditService = chatAuditService;
        _executionQueue = executionQueue;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AITaskDto>> CreateAsync(
        Guid userId,
        CreateAITaskRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ChatId == Guid.Empty || string.IsNullOrWhiteSpace(request.Description))
            return Result<AITaskDto>.Failure(Error.Validation("Chat y descripción son obligatorios."));

        var chat = await _chatRepository.GetByIdAsync(request.ChatId, cancellationToken);
        if (chat is null)
            return Result<AITaskDto>.Failure(Error.NotFound("Chat no encontrado."));

        if (chat.UserId != userId)
            return Result<AITaskDto>.Failure(Error.Unauthorized("No tenés acceso a este chat."));

        var limit = await _planLimitService.EnsureCanCreateAiTaskAsync(userId, cancellationToken);
        if (!limit.IsSuccess)
            return Result<AITaskDto>.Failure(limit.Error);

        var activeSkills = await _customSkillRepository.GetAppliedByChatIdAsync(request.ChatId, cancellationToken);
        var skillNames = activeSkills.Select(s => s.Name).ToList();
        var previousMessages = await _messageRepository.GetByChatIdAsync(request.ChatId, cancellationToken);
        var chatDocuments = await _chatDocumentContextService.BuildForChatAsync(request.ChatId, cancellationToken);
        var userProvince = (await _lawyerProfileRepository.GetByUserIdAsync(userId, cancellationToken))?.Province;

        var userMessage = new Message(
            Guid.NewGuid(),
            request.ChatId,
            DateTime.UtcNow,
            MessageRole.User,
            request.Description);
        userMessage.SetSkillsUsed(skillNames);
        await _messageRepository.AddAsync(userMessage, cancellationToken);

        StructuredTaskPlan structuredPlan;
        try
        {
            structuredPlan = await _aiService.CreateStructuredTaskPlanAsync(
                request.Description,
                previousMessages,
                activeSkills,
                chatDocuments,
                userProvince,
                cancellationToken);
        }
        catch (AIServiceException ex)
        {
            return Result<AITaskDto>.Failure(Error.ExternalService(ex.Message));
        }

        await _chatAuditService.RecordAsync(request.ChatId, cancellationToken);

        var planSummary = string.IsNullOrWhiteSpace(structuredPlan.Summary)
            ? structuredPlan.Objective
            : structuredPlan.Summary;

        var taskId = Guid.NewGuid();
        var task = new AITask(taskId, request.ChatId, request.Description, planSummary);

        foreach (var step in structuredPlan.Steps.OrderBy(s => s.Order))
        {
            task.Steps.Add(new AITaskStep(
                Guid.NewGuid(),
                taskId,
                step.Order,
                step.Title,
                step.Description));
        }

        await _aiTaskRepository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var created = await _aiTaskRepository.GetByIdWithStepsAsync(taskId, cancellationToken);
        return Result<AITaskDto>.Success(created!.ToDto());
    }

    public async Task<Result<AITaskDto>> GetByIdAsync(
        Guid userId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await _aiTaskRepository.GetByIdWithStepsAsync(taskId, cancellationToken);
        if (task is null)
            return Result<AITaskDto>.Failure(Error.NotFound("Tarea no encontrada."));

        var accessError = await EnsureTaskAccessAsync(userId, task, cancellationToken);
        if (accessError is not null)
            return Result<AITaskDto>.Failure(accessError);

        return Result<AITaskDto>.Success(task.ToDto());
    }

    public async Task<Result<IReadOnlyList<AITaskDto>>> GetByChatIdAsync(
        Guid userId,
        Guid chatId,
        CancellationToken cancellationToken = default)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
        if (chat is null)
            return Result<IReadOnlyList<AITaskDto>>.Failure(Error.NotFound("Chat no encontrado."));

        if (chat.UserId != userId)
            return Result<IReadOnlyList<AITaskDto>>.Failure(Error.Unauthorized("No tenés acceso a este chat."));

        var tasks = await _aiTaskRepository.GetByChatIdWithStepsAsync(chatId, cancellationToken);
        var dtos = tasks.Select(t => t.ToDto()).ToList();
        return Result<IReadOnlyList<AITaskDto>>.Success(dtos);
    }

    public async Task<Result<AITaskDto>> UpdatePlanAsync(
        Guid userId,
        Guid taskId,
        UpdateAITaskPlanRequest request,
        CancellationToken cancellationToken = default)
    {
        var task = await _aiTaskRepository.GetByIdWithStepsAsync(taskId, cancellationToken);
        if (task is null)
            return Result<AITaskDto>.Failure(Error.NotFound("Tarea no encontrada."));

        var accessError = await EnsureTaskAccessAsync(userId, task, cancellationToken);
        if (accessError is not null)
            return Result<AITaskDto>.Failure(accessError);

        if (task.Status != AITaskStatus.AwaitingApproval)
            return Result<AITaskDto>.Failure(Error.Validation("Solo se puede editar el plan antes de aprobarlo."));

        if (request.Steps.Count == 0)
            return Result<AITaskDto>.Failure(Error.Validation("El plan debe tener al menos un paso."));

        foreach (var update in request.Steps)
        {
            var step = task.Steps.FirstOrDefault(s => s.Order == update.Order);
            if (step is null)
                return Result<AITaskDto>.Failure(Error.Validation($"No existe el paso {update.Order}."));

            if (string.IsNullOrWhiteSpace(update.Title) || string.IsNullOrWhiteSpace(update.Description))
                return Result<AITaskDto>.Failure(Error.Validation($"El paso {update.Order} necesita título y descripción."));

            step.UpdateContent(update.Title.Trim(), update.Description.Trim());
        }

        task.SetPlanSummary(string.Join("\n", task.Steps.OrderBy(s => s.Order).Select(s => $"{s.Order}. {s.Title}")));
        _aiTaskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AITaskDto>.Success(task.ToDto());
    }

    public async Task<Result<AITaskDto>> ApprovePlanAsync(
        Guid userId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await _aiTaskRepository.GetByIdWithStepsAsync(taskId, cancellationToken);
        if (task is null)
            return Result<AITaskDto>.Failure(Error.NotFound("Tarea no encontrada."));

        var accessError = await EnsureTaskAccessAsync(userId, task, cancellationToken);
        if (accessError is not null)
            return Result<AITaskDto>.Failure(accessError);

        if (task.Status != AITaskStatus.AwaitingApproval)
            return Result<AITaskDto>.Failure(Error.Validation("La tarea no está pendiente de aprobación."));

        if (!task.Steps.Any())
            return Result<AITaskDto>.Failure(Error.Validation("La tarea no tiene pasos para ejecutar."));

        task.ApprovePlan();
        _aiTaskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _executionQueue.EnqueueAsync(userId, taskId, cancellationToken);

        var queued = await _aiTaskRepository.GetByIdWithStepsAsync(taskId, cancellationToken);
        return Result<AITaskDto>.Success(queued!.ToDto());
    }

    public async Task<Result<AITaskDto>> PauseAsync(
        Guid userId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await _aiTaskRepository.GetByIdWithStepsAsync(taskId, cancellationToken);
        if (task is null)
            return Result<AITaskDto>.Failure(Error.NotFound("Tarea no encontrada."));

        var accessError = await EnsureTaskAccessAsync(userId, task, cancellationToken);
        if (accessError is not null)
            return Result<AITaskDto>.Failure(accessError);

        if (task.Status != AITaskStatus.InProgress)
            return Result<AITaskDto>.Failure(Error.Validation("Solo se puede pausar una tarea en progreso."));

        task.Pause();
        _aiTaskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AITaskDto>.Success(task.ToDto());
    }

    public async Task<Result<AITaskDto>> ResumeAsync(
        Guid userId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await _aiTaskRepository.GetByIdWithStepsAsync(taskId, cancellationToken);
        if (task is null)
            return Result<AITaskDto>.Failure(Error.NotFound("Tarea no encontrada."));

        var accessError = await EnsureTaskAccessAsync(userId, task, cancellationToken);
        if (accessError is not null)
            return Result<AITaskDto>.Failure(accessError);

        if (task.Status != AITaskStatus.InProgress)
            return Result<AITaskDto>.Failure(Error.Validation("Solo se puede reanudar una tarea en progreso."));

        if (!task.IsPaused)
            return Result<AITaskDto>.Failure(Error.Validation("La tarea no está pausada."));

        task.Resume();
        _aiTaskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        await _executionQueue.EnqueueAsync(userId, taskId, cancellationToken);

        var queued = await _aiTaskRepository.GetByIdWithStepsAsync(taskId, cancellationToken);
        return Result<AITaskDto>.Success(queued!.ToDto());
    }

    public async Task<Result<AITaskDto>> CancelAsync(
        Guid userId,
        Guid taskId,
        CancellationToken cancellationToken = default)
    {
        var task = await _aiTaskRepository.GetByIdWithStepsAsync(taskId, cancellationToken);
        if (task is null)
            return Result<AITaskDto>.Failure(Error.NotFound("Tarea no encontrada."));

        var accessError = await EnsureTaskAccessAsync(userId, task, cancellationToken);
        if (accessError is not null)
            return Result<AITaskDto>.Failure(accessError);

        if (task.Status is AITaskStatus.Completed or AITaskStatus.Cancelled)
            return Result<AITaskDto>.Failure(Error.Validation("La tarea ya está finalizada."));

        task.Cancel();
        _aiTaskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AITaskDto>.Success(task.ToDto());
    }

    public Task RunQueuedPipelineAsync(Guid userId, Guid taskId, CancellationToken cancellationToken = default)
        => RunPipelineAsync(userId, taskId, cancellationToken);

    private async Task<Result<AITaskDto>> RunPipelineAsync(
        Guid userId,
        Guid taskId,
        CancellationToken cancellationToken)
    {
        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var task = await _aiTaskRepository.GetByIdWithStepsAsync(taskId, cancellationToken);
            if (task is null)
                return Result<AITaskDto>.Failure(Error.NotFound("Tarea no encontrada."));

            var accessError = await EnsureTaskAccessAsync(userId, task, cancellationToken);
            if (accessError is not null)
                return Result<AITaskDto>.Failure(accessError);

            if (task.Status != AITaskStatus.InProgress || task.IsPaused)
                return Result<AITaskDto>.Success(task.ToDto());

            var stepResult = await ExecuteSingleStepAsync(task, cancellationToken);
            if (!stepResult.IsSuccess)
                return stepResult;

            task = await _aiTaskRepository.GetByIdWithStepsAsync(taskId, cancellationToken);
            if (task is null)
                return Result<AITaskDto>.Failure(Error.NotFound("Tarea no encontrada."));

            if (task.Status != AITaskStatus.InProgress || task.IsPaused)
                return Result<AITaskDto>.Success(task.ToDto());
        }
    }

    private async Task<Result<AITaskDto>> ExecuteSingleStepAsync(
        AITask task,
        CancellationToken cancellationToken)
    {
        var currentStep = task.Steps.FirstOrDefault(s => s.Order == task.CurrentStepIndex);
        if (currentStep is null)
            return Result<AITaskDto>.Failure(Error.Validation("No hay un paso actual para ejecutar."));

        if (currentStep.Status == AITaskStepStatus.Completed)
            return Result<AITaskDto>.Failure(Error.Validation("El paso actual ya fue completado."));

        var activeSkills = await _customSkillRepository.GetAppliedByChatIdAsync(task.ChatId, cancellationToken);
        var previousMessages = await _messageRepository.GetByChatIdAsync(task.ChatId, cancellationToken);
        var chatDocuments = await _chatDocumentContextService.BuildForChatAsync(task.ChatId, cancellationToken);
        var chat = await _chatRepository.GetByIdAsync(task.ChatId, cancellationToken);
        var userProvince = chat is null
            ? null
            : (await _lawyerProfileRepository.GetByUserIdAsync(chat.UserId, cancellationToken))?.Province;

        var completedSteps = task.Steps
            .Where(s => s.Status == AITaskStepStatus.Completed)
            .OrderBy(s => s.Order)
            .Select(s => s.ToDto())
            .ToList();

        currentStep.MarkAsInProgress();
        _aiTaskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        try
        {
            var stepResult = await _aiService.ExecuteTaskStepAsync(
                task.Description,
                currentStep.ToDto(),
                completedSteps,
                previousMessages,
                activeSkills,
                chatDocuments,
                userProvince,
                cancellationToken);

            currentStep.MarkAsCompleted(stepResult);

            var assistantMessage = new Message(
                Guid.NewGuid(),
                task.ChatId,
                DateTime.UtcNow,
                MessageRole.Assistant,
                $"**Tarea IA — Paso {currentStep.Order}: {currentStep.Title}**\n\n{stepResult}");
            assistantMessage.SetSkillsUsed(activeSkills.Select(s => s.Name));
            await _messageRepository.AddAsync(assistantMessage, cancellationToken);
            await _chatAuditService.RecordAsync(task.ChatId, cancellationToken);

            var nextStep = task.Steps
                .Where(s => s.Order > currentStep.Order && s.Status == AITaskStepStatus.Pending)
                .OrderBy(s => s.Order)
                .FirstOrDefault();

            if (nextStep is null)
                task.MarkAsCompleted($"Plan completado. Último paso: {currentStep.Title}.");
            else
                task.AdvanceToNextStep();

            _aiTaskRepository.Update(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var updated = await _aiTaskRepository.GetByIdWithStepsAsync(task.Id, cancellationToken);
            return Result<AITaskDto>.Success(updated!.ToDto());
        }
        catch (AIServiceException ex)
        {
            currentStep.MarkAsFailed(ex.Message);
            task.MarkAsFailed($"Error en paso {currentStep.Order}: {ex.Message}");
            _aiTaskRepository.Update(task);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<AITaskDto>.Failure(Error.ExternalService(ex.Message));
        }
    }

    private async Task<Error?> EnsureTaskAccessAsync(
        Guid userId,
        AITask task,
        CancellationToken cancellationToken)
    {
        var chat = await _chatRepository.GetByIdAsync(task.ChatId, cancellationToken);
        if (chat is null || chat.UserId != userId)
            return Error.Unauthorized("No tenés acceso a esta tarea.");

        return null;
    }
}
