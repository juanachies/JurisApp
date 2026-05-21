using JurisApp.Domain.Entities;

namespace JurisApp.Application.Interfaces.Persistence;

public interface ICustomSkillRepository
{
    Task<CustomSkill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomSkill>> GetByLawyerProfileIdAsync(Guid lawyerProfileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomSkill>> GetActiveByChatIdAsync(Guid chatId, CancellationToken cancellationToken = default);
    Task AddAsync(CustomSkill customSkill, CancellationToken cancellationToken = default);
    void Update(CustomSkill customSkill);
    void Delete(CustomSkill customSkill);
}
