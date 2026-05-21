using JurisApp.Domain.Entities;

namespace JurisApp.Application.Interfaces.Auth;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
