using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JurisApp.Infrastructure.Persistence.Repositories;

public class ChatAuditRepository : IChatAuditRepository
{
    private readonly AppDbContext _context;

    public ChatAuditRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ChatAudit?> GetByChatIdAsync(Guid chatId, CancellationToken cancellationToken = default)
        => await _context.ChatAudits.FirstOrDefaultAsync(a => a.ChatId == chatId, cancellationToken);

    public async Task AddAsync(ChatAudit audit, CancellationToken cancellationToken = default)
        => await _context.ChatAudits.AddAsync(audit, cancellationToken);

    public void Update(ChatAudit audit)
        => _context.ChatAudits.Update(audit);
}
