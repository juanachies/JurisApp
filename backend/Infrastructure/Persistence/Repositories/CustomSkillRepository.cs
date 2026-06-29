using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JurisApp.Infrastructure.Persistence.Repositories;

public class CustomSkillRepository : ICustomSkillRepository
{
    private readonly AppDbContext _context;

    public CustomSkillRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<CustomSkill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.CustomSkills.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<IReadOnlyList<CustomSkill>> GetByLawyerProfileIdAsync(Guid lawyerProfileId, CancellationToken cancellationToken = default)
        => await _context.CustomSkills
            .Where(s => s.LawyerProfileId == lawyerProfileId)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<CustomSkill>> GetAppliedByChatIdAsync(Guid chatId, CancellationToken cancellationToken = default)
        => await _context.ChatCustomSkills
            .Where(cs => cs.ChatId == chatId)
            .Select(cs => cs.CustomSkill)
            .ToListAsync(cancellationToken);

    public async Task<bool> IsAppliedToChatAsync(
        Guid chatId,
        Guid customSkillId,
        CancellationToken cancellationToken = default)
        => await _context.ChatCustomSkills
            .AnyAsync(cs => cs.ChatId == chatId && cs.CustomSkillId == customSkillId, cancellationToken);

    public async Task ApplyToChatAsync(
        Guid chatId,
        Guid customSkillId,
        CancellationToken cancellationToken = default)
    {
        if (await IsAppliedToChatAsync(chatId, customSkillId, cancellationToken))
            return;

        await _context.ChatCustomSkills.AddAsync(
            new ChatCustomSkill(Guid.NewGuid(), chatId, customSkillId),
            cancellationToken);
    }

    public async Task RemoveFromChatAsync(
        Guid chatId,
        Guid customSkillId,
        CancellationToken cancellationToken = default)
    {
        var link = await _context.ChatCustomSkills
            .FirstOrDefaultAsync(
                cs => cs.ChatId == chatId && cs.CustomSkillId == customSkillId,
                cancellationToken);

        if (link is not null)
            _context.ChatCustomSkills.Remove(link);
    }

    public async Task AddAsync(CustomSkill customSkill, CancellationToken cancellationToken = default)
        => await _context.CustomSkills.AddAsync(customSkill, cancellationToken);

    public void Update(CustomSkill customSkill)
        => _context.CustomSkills.Update(customSkill);

    public void Delete(CustomSkill customSkill)
        => _context.CustomSkills.Remove(customSkill);
}