using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;

namespace JurisApp.Application.Interfaces.Persistence;

public interface ILawyerProfileRepository
{
    Task<LawyerProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LawyerProfile?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LawyerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<LawyerProfile?> GetByUserIdWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<LawyerProfile>> GetAllWithDetailsAsync(
        LawyerVerificationStatus? status,
        CancellationToken cancellationToken = default);
    Task AddAsync(LawyerProfile lawyerProfile, CancellationToken cancellationToken = default);
    void Update(LawyerProfile lawyerProfile);
}
