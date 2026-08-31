using JurisApp.Application.Auth;
using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Auth;
using JurisApp.Application.Interfaces.Auth;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Mappings;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;
using Microsoft.Extensions.Configuration;

namespace JurisApp.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IEmailSender _emailSender;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(
        IUserRepository userRepository,
        IPasswordResetTokenRepository passwordResetTokenRepository,
        IEmailVerificationTokenRepository emailVerificationTokenRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenGenerator jwtTokenGenerator,
        IEmailSender emailSender,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _passwordResetTokenRepository = passwordResetTokenRepository;
        _emailVerificationTokenRepository = emailVerificationTokenRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenGenerator = jwtTokenGenerator;
        _emailSender = emailSender;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<Result<AuthResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.FirstName) ||
            string.IsNullOrWhiteSpace(request.LastName))
        {
            return Result<AuthResponse>.Failure(Error.Validation("Nombre y apellido son obligatorios."));
        }

        var emailError = AuthValidators.ValidateEmail(request.Email);
        if (emailError is not null)
            return Result<AuthResponse>.Failure(emailError);

        var passwordError = AuthValidators.ValidatePassword(request.Password);
        if (passwordError is not null)
            return Result<AuthResponse>.Failure(passwordError);

        var normalizedEmail = AuthValidators.NormalizeEmail(request.Email);
        if (await _userRepository.EmailExistsAsync(normalizedEmail, cancellationToken))
        {
            return Result<AuthResponse>.Failure(Error.Conflict("El email ya está registrado."));
        }

        var passwordHash = _passwordHasher.HashPassword(request.Password);
        var user = new User(
            Guid.NewGuid(),
            request.FirstName,
            request.LastName,
            normalizedEmail,
            passwordHash,
            UserRole.User);

        await _userRepository.AddAsync(user, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await SendVerificationEmailAsync(user, cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            Token = _jwtTokenGenerator.GenerateToken(user),
            User = user.ToDto()
        });
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var emailError = AuthValidators.ValidateEmail(request.Email);
        if (emailError is not null)
            return Result<AuthResponse>.Failure(emailError);

        if (string.IsNullOrWhiteSpace(request.Password))
            return Result<AuthResponse>.Failure(Error.Validation("La contraseña es obligatoria."));

        var user = await _userRepository.GetByEmailAsync(AuthValidators.NormalizeEmail(request.Email), cancellationToken);
        if (user is null || !_passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return Result<AuthResponse>.Failure(Error.Unauthorized("Credenciales inválidas."));
        }

        if (!user.IsActive)
        {
            return Result<AuthResponse>.Failure(Error.Unauthorized("Cuenta desactivada."));
        }

        return Result<AuthResponse>.Success(new AuthResponse
        {
            Token = _jwtTokenGenerator.GenerateToken(user),
            User = user.ToDto()
        });
    }

    public async Task<Result<AuthResponse>> VerifyEmailAsync(VerifyEmailRequest request, CancellationToken cancellationToken = default)
    {
        var emailError = AuthValidators.ValidateEmail(request.Email);
        if (emailError is not null)
            return Result<AuthResponse>.Failure(emailError);

        var codeError = AuthValidators.ValidateVerificationCode(request.Code);
        if (codeError is not null)
            return Result<AuthResponse>.Failure(codeError);

        var user = await _userRepository.GetByEmailAsync(AuthValidators.NormalizeEmail(request.Email), cancellationToken);
        if (user is null)
            return Result<AuthResponse>.Failure(Error.Validation("El código es inválido o ha expirado."));

        if (user.IsEmailVerified)
        {
            return Result<AuthResponse>.Success(new AuthResponse
            {
                Token = _jwtTokenGenerator.GenerateToken(user),
                User = user.ToDto()
            });
        }

        var codeHash = AuthValidators.HashToken(request.Code.Trim());
        var verificationToken = await _emailVerificationTokenRepository.GetValidForUserAsync(user.Id, codeHash, cancellationToken);
        if (verificationToken is null)
            return Result<AuthResponse>.Failure(Error.Validation("El código es inválido o ha expirado."));

        user.VerifyEmail();
        verificationToken.MarkAsUsed();

        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<AuthResponse>.Success(new AuthResponse
        {
            Token = _jwtTokenGenerator.GenerateToken(user),
            User = user.ToDto()
        });
    }

    public async Task<Result> ResendVerificationAsync(ResendVerificationRequest request, CancellationToken cancellationToken = default)
    {
        var emailError = AuthValidators.ValidateEmail(request.Email);
        if (emailError is not null)
            return Result.Failure(emailError);

        var user = await _userRepository.GetByEmailAsync(AuthValidators.NormalizeEmail(request.Email), cancellationToken);
        if (user is not null && !user.IsEmailVerified)
            await SendVerificationEmailAsync(user, cancellationToken);

        return Result.Success();
    }

    public async Task<Result> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var emailError = AuthValidators.ValidateEmail(request.Email);
        if (emailError is not null)
            return Result.Failure(emailError);

        var user = await _userRepository.GetByEmailAsync(AuthValidators.NormalizeEmail(request.Email), cancellationToken);
        if (user is not null)
        {
            await _passwordResetTokenRepository.InvalidateAllForUserAsync(user.Id, cancellationToken);

            var token = AuthValidators.GenerateSecureToken();
            var tokenHash = AuthValidators.HashToken(token);
            var expirationMinutes = int.Parse(_configuration["PasswordReset:TokenExpirationMinutes"] ?? "60");

            var resetToken = new PasswordResetToken(
                Guid.NewGuid(),
                user.Id,
                tokenHash,
                DateTime.UtcNow.AddMinutes(expirationMinutes));

            await _passwordResetTokenRepository.AddAsync(resetToken, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var frontendBaseUrl = _configuration["PasswordReset:FrontendBaseUrl"] ?? "http://localhost:5173";
            var resetLink = $"{frontendBaseUrl.TrimEnd('/')}/reset-password?token={token}";
            await _emailSender.SendPasswordResetEmailAsync(user.Email, resetLink, cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Token))
            return Result.Failure(Error.Validation("El token es obligatorio."));

        var passwordError = AuthValidators.ValidatePassword(request.NewPassword, "nueva contraseña");
        if (passwordError is not null)
            return Result.Failure(passwordError);

        var tokenHash = AuthValidators.HashToken(request.Token);
        var resetToken = await _passwordResetTokenRepository.GetValidByTokenHashAsync(tokenHash, cancellationToken);
        if (resetToken is null)
            return Result.Failure(Error.Validation("El token es inválido o ha expirado."));

        var passwordHash = _passwordHasher.HashPassword(request.NewPassword);
        resetToken.User.ChangePassword(passwordHash);
        resetToken.MarkAsUsed();

        _userRepository.Update(resetToken.User);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task SendVerificationEmailAsync(User user, CancellationToken cancellationToken)
    {
        await _emailVerificationTokenRepository.InvalidateAllForUserAsync(user.Id, cancellationToken);

        var code = AuthValidators.GenerateVerificationCode();
        var codeHash = AuthValidators.HashToken(code);
        var expirationMinutes = int.Parse(
            _configuration["EmailVerification:CodeExpirationMinutes"]
            ?? _configuration["EmailVerification:TokenExpirationMinutes"]
            ?? "15");

        var verificationToken = new EmailVerificationToken(
            Guid.NewGuid(),
            user.Id,
            codeHash,
            DateTime.UtcNow.AddMinutes(expirationMinutes));

        await _emailVerificationTokenRepository.AddAsync(verificationToken, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _emailSender.SendEmailVerificationCodeAsync(user.Email, code, cancellationToken);
    }
}
