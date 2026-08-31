using System.Text.Json;
using JurisApp.Application.Common;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Domain.Enums;

namespace JurisApp.Application.Services;

public class PlanLimitService : IPlanLimitService
{
    public const int Unlimited = -1;

    private readonly IPlanRepository _planRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IAITaskRepository _aiTaskRepository;

    public PlanLimitService(
        IPlanRepository planRepository,
        ISubscriptionRepository subscriptionRepository,
        IChatRepository chatRepository,
        IDocumentRepository documentRepository,
        IAITaskRepository aiTaskRepository)
    {
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
        _chatRepository = chatRepository;
        _documentRepository = documentRepository;
        _aiTaskRepository = aiTaskRepository;
    }

    public Task<Result> EnsureCanCreateChatAsync(Guid userId, CancellationToken cancellationToken = default)
        => EnsureWithinLimitAsync(userId, "chats", () => _chatRepository.CountByUserIdAsync(userId, cancellationToken), "chats", cancellationToken);

    public Task<Result> EnsureCanUploadDocumentAsync(Guid userId, CancellationToken cancellationToken = default)
        => EnsureWithinLimitAsync(userId, "documents", () => _documentRepository.CountOwnedByUserIdAsync(userId, cancellationToken), "documentos", cancellationToken);

    public Task<Result> EnsureCanCreateAiTaskAsync(Guid userId, CancellationToken cancellationToken = default)
        => EnsureWithinLimitAsync(userId, "aiTasks", () => _aiTaskRepository.CountByUserIdAsync(userId, cancellationToken), "tareas IA", cancellationToken);

    private async Task<Result> EnsureWithinLimitAsync(
        Guid userId,
        string jsonKey,
        Func<Task<int>> countAsync,
        string resourceName,
        CancellationToken cancellationToken)
    {
        var limits = await GetLimitsAsync(userId, cancellationToken);
        if (limits is null || !limits.TryGetValue(jsonKey, out var limit))
            return Result.Success();

        if (limit == Unlimited)
            return Result.Success();

        var count = await countAsync();
        if (count >= limit)
        {
            return Result.Failure(Error.Validation(
                $"Alcanzaste el límite de {resourceName} de tu plan ({limit})."));
        }

        return Result.Success();
    }

    private async Task<Dictionary<string, int>?> GetLimitsAsync(Guid userId, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        var plan = subscription is not null
            ? await _planRepository.GetByIdAsync(subscription.PlanId, cancellationToken)
            : await _planRepository.GetByTypeAsync(PlanType.Free, cancellationToken);

        if (plan is null || string.IsNullOrWhiteSpace(plan.LimitsJson))
            return null;

        try
        {
            using var doc = JsonDocument.Parse(plan.LimitsJson);
            var map = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            foreach (var prop in doc.RootElement.EnumerateObject())
            {
                if (prop.Value.ValueKind == JsonValueKind.Number && prop.Value.TryGetInt32(out var value))
                    map[prop.Name] = value;
            }

            return map;
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
