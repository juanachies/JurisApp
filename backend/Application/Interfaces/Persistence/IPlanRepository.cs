using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;

namespace JurisApp.Application.Interfaces.Persistence;

public interface IPlanRepository
{
    Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Plan?> GetByTypeAsync(PlanType type, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Plan>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Plan plan, CancellationToken cancellationToken = default);
    void Update(Plan plan);
    void Delete(Plan plan);
}
