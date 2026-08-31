using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JurisApp.Infrastructure.Persistence.Repositories;

public class AITaskRepository : IAITaskRepository
{
    private readonly AppDbContext _context;

    public AITaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<AITask?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.AITasks.FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<AITask?> GetByIdWithStepsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.AITasks
            .Include(t => t.Steps)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<IReadOnlyList<AITask>> GetByChatIdAsync(Guid chatId, CancellationToken cancellationToken = default)
        => await _context.AITasks
            .Where(t => t.ChatId == chatId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<AITask>> GetByChatIdWithStepsAsync(Guid chatId, CancellationToken cancellationToken = default)
        => await _context.AITasks
            .Include(t => t.Steps)
            .Where(t => t.ChatId == chatId)
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(AITask aiTask, CancellationToken cancellationToken = default)
        => await _context.AITasks.AddAsync(aiTask, cancellationToken);

    public void Update(AITask aiTask)
        => _context.AITasks.Update(aiTask);

    public async Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.AITasks
            .Where(t => t.Chat.UserId == userId)
            .CountAsync(cancellationToken);
}