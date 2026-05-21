using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Auth;
using JurisApp.Application.Interfaces.Auth;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Mappings;
using JurisApp.Application.Services.Interfaces;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;

namespace JurisApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure(Error.Validation("Todos los campos son obligatorios."));
        }

        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
        {
            return Result<AuthResponse>.Failure(Error.Conflict("El email ya está registrado."));
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = new User(
            Guid.NewGuid(),
            request.FirstName,
            request.LastName,
            request.Email,
            passwordHash,
            UserRole.User);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            Token = _jwtTokenGenerator.GenerateToken(user),
            User = user.ToDto()
        });
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<AuthResponse>.Failure(Error.Validation("Email y contraseña son obligatorios."));
        }

        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure(Error.Unauthorized("Credenciales inválidas."));
        }

        return Result<AuthResponse>.Success(new AuthResponse
        {
            Token = _jwtTokenGenerator.GenerateToken(user),
            User = user.ToDto()
        });
    }
}
