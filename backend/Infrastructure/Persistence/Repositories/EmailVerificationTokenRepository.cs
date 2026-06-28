using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace JurisApp.Infrastructure.Persistence.Repositories;

public class EmailVerificationTokenRepository : IEmailVerificationTokenRepository
{
    private readonly AppDbContext _context;

    public EmailVerificationTokenRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default)
        => await _context.EmailVerificationTokens.AddAsync(token, cancellationToken);

    public async Task<EmailVerificationToken?> GetValidForUserAsync(Guid userId, string codeHash, CancellationToken cancellationToken = default)
        => await _context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(
                t => t.UserId == userId &&
                     t.TokenHash == codeHash &&
                     t.UsedAt == null &&
                     t.ExpiresAt > DateTime.UtcNow,
                cancellationToken);

    public async Task InvalidateAllForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.EmailVerificationTokens
            .Where(t => t.UserId == userId && t.UsedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
            token.MarkAsUsed();
    }
}
