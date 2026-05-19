using JurisApp.Domain.Common;
using JurisApp.Domain.Enums;

namespace JurisApp.Domain.Entities;

public class User : BaseEntity
{
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public UserRole Role { get; private set; }

    public LawyerProfile? LawyerProfile { get; private set; }
    public ICollection<Chat> Chats { get; private set; } = new List<Chat>();
    public ICollection<Subscription> Subscriptions { get; private set; } = new List<Subscription>();

    protected User() { }

    public User(Guid id, string firstName, string lastName, string email, string passwordHash, UserRole role)
        : base(id)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        PasswordHash = passwordHash;
        Role = role;
    }

    public void UpgradeToLawyer()
    {
        Role = UserRole.Lawyer;
        Touch();
    }

    public void ChangeRole(UserRole role)
    {
        Role = role;
        Touch();
    }
}
