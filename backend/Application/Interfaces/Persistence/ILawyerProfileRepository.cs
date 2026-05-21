using JurisApp.Domain.Entities;

namespace JurisApp.Application.Interfaces.Persistence;

public interface ILawyerProfileRepository
{
    Task<LawyerProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<LawyerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task AddAsync(LawyerProfile lawyerProfile, CancellationToken cancellationToken = default);
    void Update(LawyerProfile lawyerProfile);
}
