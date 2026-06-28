using JurisApp.Domain.Entities;

namespace JurisApp.Application.Interfaces.Persistence;

public interface IEmailVerificationTokenRepository
{
    Task AddAsync(EmailVerificationToken token, CancellationToken cancellationToken = default);
    Task<EmailVerificationToken?> GetValidForUserAsync(Guid userId, string codeHash, CancellationToken cancellationToken = default);
    Task InvalidateAllForUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
