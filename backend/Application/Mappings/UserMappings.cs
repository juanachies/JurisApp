using JurisApp.Application.DTOs.Users;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Mappings;

public static class UserMappings
{
    public static UserDto ToDto(this User user) => new()
    {
        Id = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        Role = user.Role
    };

    public static CurrentUserDto ToCurrentUserDto(this User user) => new()
    {
        Id = user.Id,
        Email = user.Email,
        Role = user.Role
    };
}
