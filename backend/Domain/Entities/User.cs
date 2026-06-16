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
    public bool IsActive { get; private set; } = true;

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
        IsActive = true;
    }

    public void UpdateProfile(string firstName, string lastName, string email)
    {
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Touch();
    }

    public void ChangePassword(string passwordHash)
    {
        PasswordHash = passwordHash;
        Touch();
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
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
