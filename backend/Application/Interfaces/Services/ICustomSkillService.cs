using JurisApp.Application.Common;
using JurisApp.Application.DTOs.CustomSkills;

namespace JurisApp.Application.Interfaces.Services;

public interface ICustomSkillService
{
    Task<Result<CustomSkillDto>> CreateAsync(Guid userId, CreateCustomSkillRequest request, CancellationToken cancellationToken = default);
    Task<Result<CustomSkillDto>> UpdateAsync(Guid userId, Guid customSkillId, UpdateCustomSkillRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<CustomSkillDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> ApplyToChatAsync(Guid userId, ApplyCustomSkillToChatRequest request, CancellationToken cancellationToken = default);
    Task<Result> RemoveFromChatAsync(Guid userId, ApplyCustomSkillToChatRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid userId, Guid customSkillId, CancellationToken cancellationToken = default);
}
