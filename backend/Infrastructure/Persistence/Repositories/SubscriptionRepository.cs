using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace JurisApp.Infrastructure.Persistence.Repositories;

public class SubscriptionRepository : ISubscriptionRepository
{
    private readonly AppDbContext _context;

    public SubscriptionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Subscription?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Subscriptions.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

    public async Task<Subscription?> GetActiveByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Subscriptions.FirstOrDefaultAsync(
            s => s.UserId == userId && s.Status == SubscriptionStatus.Active,
            cancellationToken);

    public async Task<IReadOnlyList<Subscription>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        => await _context.Subscriptions
            .Where(s => s.UserId == userId)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Subscription subscription, CancellationToken cancellationToken = default)
        => await _context.Subscriptions.AddAsync(subscription, cancellationToken);

    public void Update(Subscription subscription)
        => _context.Subscriptions.Update(subscription);
}