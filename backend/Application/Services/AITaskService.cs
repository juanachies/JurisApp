using JurisApp.Application.Common;
using JurisApp.Application.DTOs.AITasks;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Mappings;
using JurisApp.Application.Services.Interfaces;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Services;

public class AITaskService : IAITaskService
{
    private readonly IChatRepository _chatRepository;
    private readonly IAITaskRepository _aiTaskRepository;
    private readonly IAIService _aiService;
    private readonly IUnitOfWork _unitOfWork;

    public AITaskService(
        IChatRepository chatRepository,
        IAITaskRepository aiTaskRepository,
        IAIService aiService,
        IUnitOfWork unitOfWork)
    {
        _chatRepository = chatRepository;
        _aiTaskRepository = aiTaskRepository;
        _aiService = aiService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AITaskDto>> CreateAsync(Guid userId, CreateAITaskRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ChatId == Guid.Empty || string.IsNullOrWhiteSpace(request.Description))
        {
            return Result<AITaskDto>.Failure(Error.Validation("Chat y descripción son obligatorios."));
        }

        var chat = await _chatRepository.GetByIdAsync(request.ChatId, cancellationToken);
        if (chat is null)
        {
            return Result<AITaskDto>.Failure(Error.NotFound("Chat no encontrado."));
        }

        if (chat.UserId != userId)
        {
            return Result<AITaskDto>.Failure(Error.Unauthorized("No tenés acceso a este chat."));
        }

        var plan = await _aiService.CreateTaskPlanAsync(request.Description, cancellationToken);

        var task = new AITask(Guid.NewGuid(), request.ChatId, request.Description, plan);
        await _aiTaskRepository.AddAsync(task, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AITaskDto>.Success(task.ToDto());
    }

    public async Task<Result<AITaskDto>> CompleteAsync(Guid userId, Guid taskId, string result, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(result))
        {
            return Result<AITaskDto>.Failure(Error.Validation("El resultado es obligatorio."));
        }

        var task = await _aiTaskRepository.GetByIdAsync(taskId, cancellationToken);
        if (task is null)
        {
            return Result<AITaskDto>.Failure(Error.NotFound("Tarea no encontrada."));
        }

        var chat = await _chatRepository.GetByIdAsync(task.ChatId, cancellationToken);
        if (chat is null || chat.UserId != userId)
        {
            return Result<AITaskDto>.Failure(Error.Unauthorized("No tenés acceso a esta tarea."));
        }

        task.MarkAsCompleted(result);
        _aiTaskRepository.Update(task);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AITaskDto>.Success(task.ToDto());
    }

    public async Task<Result<IReadOnlyList<AITaskDto>>> GetByChatIdAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
        if (chat is null)
        {
            return Result<IReadOnlyList<AITaskDto>>.Failure(Error.NotFound("Chat no encontrado."));
        }

        if (chat.UserId != userId)
        {
            return Result<IReadOnlyList<AITaskDto>>.Failure(Error.Unauthorized("No tenés acceso a este chat."));
        }

        var tasks = await _aiTaskRepository.GetByChatIdAsync(chatId, cancellationToken);
        var dtos = tasks.Select(t => t.ToDto()).ToList();
        return Result<IReadOnlyList<AITaskDto>>.Success(dtos);
    }
}
