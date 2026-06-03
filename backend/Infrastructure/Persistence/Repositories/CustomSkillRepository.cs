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

    public async Task<IReadOnlyList<CustomSkill>> GetActiveByChatIdAsync(Guid chatId, CancellationToken cancellationToken = default)
        => await _context.ChatCustomSkills
            .Where(cs => cs.ChatId == chatId && cs.CustomSkill.IsActive)
            .Select(cs => cs.CustomSkill)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(CustomSkill customSkill, CancellationToken cancellationToken = default)
        => await _context.CustomSkills.AddAsync(customSkill, cancellationToken);

    public void Update(CustomSkill customSkill)
        => _context.CustomSkills.Update(customSkill);

    public void Delete(CustomSkill customSkill)
        => _context.CustomSkills.Remove(customSkill);
}