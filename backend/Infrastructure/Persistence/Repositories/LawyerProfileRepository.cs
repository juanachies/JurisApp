using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Entities;
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

    public async Task<LawyerProfile?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.LawyerProfiles.FirstOrDefaultAsync(lp => lp.UserId == userId, cancellationToken);

    public async Task AddAsync(LawyerProfile lawyerProfile, CancellationToken cancellationToken = default)
        => await _context.LawyerProfiles.AddAsync(lawyerProfile, cancellationToken);

    public void Update(LawyerProfile lawyerProfile)
        => _context.LawyerProfiles.Update(lawyerProfile);
}