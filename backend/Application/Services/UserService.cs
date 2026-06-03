using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Users;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Interfaces.Services;

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
            return Result<UserDto>.Failure(Error.NotFound("User not found."));

        return Result<UserDto>.Success(new UserDto
        {
            Id        = user.Id,
            FirstName = user.FirstName,
            LastName  = user.LastName,
            Email     = user.Email,
            Role      = user.Role
        });
    }

    public async Task<Result<IReadOnlyList<UserDto>>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        var dtos = users.Select(u => new UserDto
        {
            Id        = u.Id,
            FirstName = u.FirstName,
            LastName  = u.LastName,
            Email     = u.Email,
            Role      = u.Role
        }).ToList();

        return Result<IReadOnlyList<UserDto>>.Success(dtos);
    }

    public async Task<Result> DeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result.Failure(Error.NotFound("User not found."));

        _userRepository.Delete(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}