using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Users;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Application.Mappings;
using JurisApp.Domain.Enums;

namespace JurisApp.Application.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<UserDto>> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<UserDto>.Failure(Error.NotFound("Usuario no encontrado."));

        return Result<UserDto>.Success(user.ToDto());
    }

    public async Task<Result<IReadOnlyList<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);
        var dtos = users.Select(u => u.ToDto()).ToList();
        return Result<IReadOnlyList<UserDto>>.Success(dtos);
    }

    public async Task<Result<UserDto>> UpdateProfileAsync(
        Guid userId,
        UpdateUserProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return Result<UserDto>.Failure(Error.Validation("Nombre y apellido son obligatorios."));
        }

        var themeError = ValidateTheme(request.Theme);
        if (themeError is not null)
            return Result<UserDto>.Failure(themeError);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<UserDto>.Failure(Error.NotFound("Usuario no encontrado."));

        user.UpdateProfile(request.FirstName.Trim(), request.LastName.Trim());
        user.ChangeTheme(request.Theme);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserDto>.Success(user.ToDto());
    }

    public async Task<Result<UserDto>> AdminUpdateAsync(
        Guid targetUserId,
        Guid adminUserId,
        AdminUpdateUserRequest request,
        CancellationToken cancellationToken = default)
    {
        if (targetUserId == adminUserId)
            return Result<UserDto>.Failure(Error.Conflict("No podés modificar tu propia cuenta."));

        if (request.Role is null && request.IsActive is null)
            return Result<UserDto>.Failure(Error.Validation("Debés indicar el rol o el estado a modificar."));

        var user = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (user is null)
            return Result<UserDto>.Failure(Error.NotFound("Usuario no encontrado."));

        if (request.Role is not null)
            user.ChangeRole(request.Role.Value);

        if (request.IsActive is not null)
        {
            if (request.IsActive.Value)
                user.Activate();
            else
                user.Deactivate();
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserDto>.Success(user.ToDto());
    }

    private static Error? ValidateTheme(UserTheme theme) =>
        Enum.IsDefined(theme)
            ? null
            : Error.Validation("El tema debe ser Bright o Dark.");
}
