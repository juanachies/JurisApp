using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Auth;

namespace JurisApp.Application.Interfaces.Services;

public interface IAuthService
{
    Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
