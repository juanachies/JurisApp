using JurisApp.Domain.Entities;

namespace JurisApp.Application.Interfaces.Persistence;

public interface ICustomSkillRepository
{
    Task<CustomSkill?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomSkill>> GetByLawyerProfileIdAsync(Guid lawyerProfileId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CustomSkill>> GetActiveByChatIdAsync(Guid chatId, CancellationToken cancellationToken = default);
    Task<bool> IsAppliedToChatAsync(Guid chatId, Guid customSkillId, CancellationToken cancellationToken = default);
    Task ApplyToChatAsync(Guid chatId, Guid customSkillId, CancellationToken cancellationToken = default);
    Task RemoveFromChatAsync(Guid chatId, Guid customSkillId, CancellationToken cancellationToken = default);
    Task AddAsync(CustomSkill customSkill, CancellationToken cancellationToken = default);
    void Update(CustomSkill customSkill);
    void Delete(CustomSkill customSkill);
}
