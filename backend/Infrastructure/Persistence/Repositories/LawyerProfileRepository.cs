using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JurisApp.Infrastructure.Persistence.Repositories;

public class LawyerProfileRepository : ILawyerProfileRepository
{
    private readonly AppDbContext _context;

    public LawyerProfileRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<LawyerProfile?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.LawyerProfiles.FirstOrDefaultAsync(lp => lp.Id == id, cancellationToken);

    public async Task<LawyerProfile?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.LawyerProfiles
            .Include(lp => lp.User)
            .FirstOrDefaultAsync(lp => lp.Id == id, cancellationToken);

    public async Task<LawyerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.LawyerProfiles.FirstOrDefaultAsync(lp => lp.UserId == userId, cancellationToken);

    public async Task<LawyerProfile?> GetByUserIdWithDetailsAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.LawyerProfiles
            .Include(lp => lp.User)
            .FirstOrDefaultAsync(lp => lp.UserId == userId, cancellationToken);

    public async Task<IReadOnlyList<LawyerProfile>> GetAllWithDetailsAsync(
        LawyerVerificationStatus? status,
        CancellationToken cancellationToken = default)
    {
        var query = _context.LawyerProfiles
            .Include(lp => lp.User)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(lp => lp.VerificationStatus == status.Value);

        return await query
            .OrderByDescending(lp => lp.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(LawyerProfile lawyerProfile, CancellationToken cancellationToken = default)
        => await _context.LawyerProfiles.AddAsync(lawyerProfile, cancellationToken);

    public void Update(LawyerProfile lawyerProfile)
        => _context.LawyerProfiles.Update(lawyerProfile);
}
