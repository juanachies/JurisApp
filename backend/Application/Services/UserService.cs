using JurisApp.Application.Auth;
using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Users;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Application.Mappings;

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

        var emailError = AuthValidators.ValidateEmail(request.Email);
        if (emailError is not null)
            return Result<UserDto>.Failure(emailError);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<UserDto>.Failure(Error.NotFound("Usuario no encontrado."));

        var normalizedEmail = request.Email.Trim();
        if (!string.Equals(user.Email, normalizedEmail, StringComparison.OrdinalIgnoreCase) &&
            await _userRepository.EmailExistsAsync(normalizedEmail, userId, cancellationToken))
        {
            return Result<UserDto>.Failure(Error.Conflict("El email ya está registrado."));
        }

        user.UpdateProfile(request.FirstName.Trim(), request.LastName.Trim(), normalizedEmail);
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
        var user = await _userRepository.GetByIdAsync(targetUserId, cancellationToken);
        if (user is null)
            return Result<UserDto>.Failure(Error.NotFound("Usuario no encontrado."));

        if (request.FirstName is not null || request.LastName is not null || request.Email is not null)
        {
            var firstName = request.FirstName?.Trim() ?? user.FirstName;
            var lastName = request.LastName?.Trim() ?? user.LastName;
            var email = request.Email?.Trim() ?? user.Email;

            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
                return Result<UserDto>.Failure(Error.Validation("Nombre y apellido son obligatorios."));

            var emailError = AuthValidators.ValidateEmail(email);
            if (emailError is not null)
                return Result<UserDto>.Failure(emailError);

            if (!string.Equals(user.Email, email, StringComparison.OrdinalIgnoreCase) &&
                await _userRepository.EmailExistsAsync(email, targetUserId, cancellationToken))
            {
                return Result<UserDto>.Failure(Error.Conflict("El email ya está registrado."));
            }

            user.UpdateProfile(firstName, lastName, email);
        }

        if (request.Role is not null)
            user.ChangeRole(request.Role.Value);

        if (request.IsActive is not null)
        {
            if (targetUserId == adminUserId && !request.IsActive.Value)
                return Result<UserDto>.Failure(Error.Conflict("No puedes desactivar tu propia cuenta."));

            if (request.IsActive.Value)
                user.Activate();
            else
                user.Deactivate();
        }

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<UserDto>.Success(user.ToDto());
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid adminUserId, CancellationToken cancellationToken = default)
    {
        if (userId == adminUserId)
            return Result.Failure(Error.Conflict("No puedes eliminar tu propia cuenta."));

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure(Error.NotFound("Usuario no encontrado."));

        _userRepository.Delete(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
