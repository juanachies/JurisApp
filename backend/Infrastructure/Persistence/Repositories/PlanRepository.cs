using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JurisApp.Infrastructure.Persistence.Repositories;

public class PlanRepository : IPlanRepository
{
    private readonly AppDbContext _context;

    public PlanRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Plan?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Plans.FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

    public async Task<Plan?> GetByTypeAsync(PlanType type, CancellationToken cancellationToken = default)
        => await _context.Plans.FirstOrDefaultAsync(p => p.Type == type, cancellationToken);

    public async Task<IReadOnlyList<Plan>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Plans.ToListAsync(cancellationToken);

    public async Task AddAsync(Plan plan, CancellationToken cancellationToken = default)
        => await _context.Plans.AddAsync(plan, cancellationToken);

    public void Update(Plan plan)
        => _context.Plans.Update(plan);
}