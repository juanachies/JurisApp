using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Users;

namespace JurisApp.Application.Interfaces.Services;

public interface IUserService
{
    Task<Result<UserDto>> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Result<UserDto>> UpdateProfileAsync(Guid userId, UpdateUserProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result<UserDto>> AdminUpdateAsync(Guid targetUserId, Guid adminUserId, AdminUpdateUserRequest request, CancellationToken cancellationToken = default);
}
