using JurisApp.Application.DTOs.Users;

namespace JurisApp.Application.DTOs.Auth;

public class RegisterResponse
{
    public string Message { get; set; } = string.Empty;
    public UserDto User { get; set; } = null!;
}
