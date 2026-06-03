using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JurisApp.Infrastructure.Persistence.Repositories;

public class MessageRepository : IMessageRepository
{
    private readonly AppDbContext _context;

    public MessageRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<Message>> GetByChatIdAsync(Guid chatId, CancellationToken cancellationToken = default)
        => await _context.Messages
            .Where(m => m.ChatId == chatId)
            .OrderBy(m => m.Date)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Message message, CancellationToken cancellationToken = default)
        => await _context.Messages.AddAsync(message, cancellationToken);
}