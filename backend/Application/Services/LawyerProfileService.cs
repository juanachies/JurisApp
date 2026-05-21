using JurisApp.Application.Common;
using JurisApp.Application.DTOs.LawyerProfiles;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Mappings;
using JurisApp.Application.Services.Interfaces;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Services;

public class LawyerProfileService : ILawyerProfileService
{
    private readonly IUserRepository _userRepository;
    private readonly ILawyerProfileRepository _lawyerProfileRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LawyerProfileService(
        IUserRepository userRepository,
        ILawyerProfileRepository lawyerProfileRepository,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _lawyerProfileRepository = lawyerProfileRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LawyerProfileDto>> CreateAsync(Guid userId, CreateLawyerProfileRequest request, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return Result<LawyerProfileDto>.Failure(Error.Validation("Usuario inválido."));
        }

        if (string.IsNullOrWhiteSpace(request.LicenseNumber) ||
            string.IsNullOrWhiteSpace(request.BarAssociation) ||
            string.IsNullOrWhiteSpace(request.Province) ||
            string.IsNullOrWhiteSpace(request.Specialty))
        {
            return Result<LawyerProfileDto>.Failure(Error.Validation("Todos los campos del perfil son obligatorios."));
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<LawyerProfileDto>.Failure(Error.NotFound("Usuario no encontrado."));
        }

        var existingProfile = await _lawyerProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (existingProfile is not null)
        {
            return Result<LawyerProfileDto>.Failure(Error.Conflict("El usuario ya tiene un perfil de abogado."));
        }

        var profile = new LawyerProfile(
            Guid.NewGuid(),
            userId,
            request.LicenseNumber,
            request.BarAssociation,
            request.Province,
            request.Specialty);

        user.UpgradeToLawyer();
        _userRepository.Update(user);
        await _lawyerProfileRepository.AddAsync(profile, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LawyerProfileDto>.Success(profile.ToDto());
    }

    public async Task<Result<LawyerProfileDto>> VerifyAsync(VerifyLawyerRequest request, CancellationToken cancellationToken = default)
    {
        if (request.LawyerProfileId == Guid.Empty || request.VerifiedById == Guid.Empty)
        {
            return Result<LawyerProfileDto>.Failure(Error.Validation("Datos de verificación inválidos."));
        }

        var profile = await _lawyerProfileRepository.GetByIdAsync(request.LawyerProfileId, cancellationToken);
        if (profile is null)
        {
            return Result<LawyerProfileDto>.Failure(Error.NotFound("Perfil de abogado no encontrado."));
        }

        profile.Verify(request.VerifiedById);
        _lawyerProfileRepository.Update(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LawyerProfileDto>.Success(profile.ToDto());
    }

    public async Task<Result<LawyerProfileDto>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return Result<LawyerProfileDto>.Failure(Error.Validation("Usuario inválido."));
        }

        var profile = await _lawyerProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
        {
            return Result<LawyerProfileDto>.Failure(Error.NotFound("Perfil de abogado no encontrado."));
        }

        return Result<LawyerProfileDto>.Success(profile.ToDto());
    }
}
