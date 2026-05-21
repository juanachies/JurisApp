using JurisApp.Domain.Enums;

namespace JurisApp.Application.DTOs.Users;

public class CurrentUserDto
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public UserRole Role { get; set; }
}
