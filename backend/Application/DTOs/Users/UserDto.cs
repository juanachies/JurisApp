using JurisApp.Domain.Enums;

namespace JurisApp.Application.DTOs.Users;

public class UserDto
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; }
    public bool IsEmailVerified { get; set; }
    public UserTheme Theme { get; set; }
    public DateTime CreatedAt { get; set; }
}
