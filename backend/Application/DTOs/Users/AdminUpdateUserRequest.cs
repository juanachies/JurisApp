using JurisApp.Domain.Enums;

namespace JurisApp.Application.DTOs.Users;

public class AdminUpdateUserRequest
{
    public UserRole? Role { get; set; }
    public bool? IsActive { get; set; }
}
