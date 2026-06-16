using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JurisApp.Infrastructure.Persistence.Repositories;

public class ChatRepository : IChatRepository
{
    private readonly AppDbContext _context;

    public ChatRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Chat?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Chats
            .Include(c => c.Messages)
            .Include(c => c.AppliedSkills)
                .ThenInclude(cs => cs.CustomSkill)
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<Chat?> GetByIdLightAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Chats.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Chat>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Chats
            .Where(c => c.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Chat chat, CancellationToken cancellationToken = default)
        => await _context.Chats.AddAsync(chat, cancellationToken);

    public void Update(Chat chat)
    {
        // Si el chat ya está tracked (p. ej. tras GetByIdAsync con Includes),
        // Update() marcaría todo el grafo como Modified y provoca DbUpdateConcurrencyException.
        if (_context.Entry(chat).State == EntityState.Detached)
            _context.Chats.Update(chat);
    }

    public void Delete(Chat chat)
        => _context.Chats.Remove(chat);
}