using JurisApp.Domain.Common;

namespace JurisApp.Domain.Entities;

public class PasswordResetToken : BaseEntity
{
    public Guid UserId { get; private set; }
    public string TokenHash { get; private set; } = string.Empty;
    public DateTime ExpiresAt { get; private set; }
    public DateTime? UsedAt { get; private set; }

    public User User { get; private set; } = null!;

    protected PasswordResetToken() { }

    public PasswordResetToken(Guid id, Guid userId, string tokenHash, DateTime expiresAt)
        : base(id)
    {
        UserId = userId;
        TokenHash = tokenHash;
        ExpiresAt = expiresAt;
    }

    public bool IsValid() =>
        UsedAt is null && ExpiresAt > DateTime.UtcNow;

    public void MarkAsUsed()
    {
        UsedAt = DateTime.UtcNow;
        Touch();
    }
}
