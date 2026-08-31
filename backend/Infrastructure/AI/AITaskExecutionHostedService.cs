using JurisApp.Application.Interfaces.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace JurisApp.Infrastructure.AI;

public sealed class AITaskExecutionHostedService : BackgroundService
{
    private readonly IAITaskExecutionQueue _queue;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<AITaskExecutionHostedService> _logger;

    public AITaskExecutionHostedService(
        IAITaskExecutionQueue queue,
        IServiceScopeFactory scopeFactory,
        ILogger<AITaskExecutionHostedService> logger)
    {
        _queue = queue;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var (userId, taskId) in _queue.ReadAllAsync(stoppingToken))
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var aiTaskService = scope.ServiceProvider.GetRequiredService<IAITaskService>();
                await aiTaskService.RunQueuedPipelineAsync(userId, taskId, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando la tarea IA {TaskId}.", taskId);
            }
        }
    }
}
