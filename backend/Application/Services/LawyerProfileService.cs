using JurisApp.Application.Common;
using JurisApp.Application.DTOs.LawyerProfiles;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Mappings;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;

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

    public async Task<Result<LawyerProfileDto>> CreateVerificationRequestAsync(
        Guid userId,
        CreateLawyerProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateProfessionalData(request.LicenseNumber, request.BarAssociation, request.Province, request.Specialty);
        if (validationError is not null)
            return Result<LawyerProfileDto>.Failure(validationError);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<LawyerProfileDto>.Failure(Error.NotFound("Usuario no encontrado."));

        var existingProfile = await _lawyerProfileRepository.GetByUserIdWithDetailsAsync(userId, cancellationToken);

        if (existingProfile?.IsVerifiedLawyer == true)
            return Result<LawyerProfileDto>.Failure(Error.Conflict("Ya sos un abogado verificado."));

        if (existingProfile?.IsPending == true)
            return Result<LawyerProfileDto>.Failure(Error.Conflict("Ya tenés una solicitud de verificación pendiente."));

        LawyerProfile profile;

        if (existingProfile is not null)
        {
            if (existingProfile.VerificationStatus == LawyerVerificationStatus.Rejected)
            {
                existingProfile.Resubmit(
                    request.LicenseNumber,
                    request.BarAssociation,
                    request.Province,
                    request.Specialty);
            }
            else if (existingProfile.CanSubmitRequest)
            {
                existingProfile.Update(
                    request.LicenseNumber,
                    request.BarAssociation,
                    request.Province,
                    request.Specialty);
                existingProfile.MarkAsPendingVerification();
            }
            else
            {
                return Result<LawyerProfileDto>.Failure(Error.Conflict("No podés enviar una nueva solicitud en este momento."));
            }

            profile = existingProfile;
            _lawyerProfileRepository.Update(profile);
        }
        else
        {
            profile = new LawyerProfile(
                Guid.NewGuid(),
                userId,
                request.LicenseNumber,
                request.BarAssociation,
                request.Province,
                request.Specialty);

            await _lawyerProfileRepository.AddAsync(profile, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var savedProfile = await _lawyerProfileRepository.GetByUserIdWithDetailsAsync(userId, cancellationToken);
        return Result<LawyerProfileDto>.Success(savedProfile!.ToDto());
    }

    public async Task<Result<LawyerProfileDto>> UpdateAsync(
        Guid userId,
        UpdateLawyerProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateProfessionalData(request.LicenseNumber, request.BarAssociation, request.Province, request.Specialty);
        if (validationError is not null)
            return Result<LawyerProfileDto>.Failure(validationError);

        var profile = await _lawyerProfileRepository.GetByUserIdWithDetailsAsync(userId, cancellationToken);
        if (profile is null)
            return Result<LawyerProfileDto>.Failure(Error.NotFound("Solicitud de verificación no encontrada."));

        if (profile.IsVerifiedLawyer)
            return Result<LawyerProfileDto>.Failure(Error.Conflict("No podés modificar un perfil ya verificado."));

        if (profile.VerificationStatus is not (LawyerVerificationStatus.Pending or LawyerVerificationStatus.Rejected))
            return Result<LawyerProfileDto>.Failure(Error.Validation("Solo podés modificar solicitudes pendientes o rechazadas."));

        profile.Update(
            request.LicenseNumber,
            request.BarAssociation,
            request.Province,
            request.Specialty);

        _lawyerProfileRepository.Update(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LawyerProfileDto>.Success(profile.ToDto());
    }

    public async Task<Result<LawyerProfileDto>> ApproveAsync(
        Guid requestId,
        Guid adminUserId,
        CancellationToken cancellationToken = default)
    {
        if (requestId == Guid.Empty || adminUserId == Guid.Empty)
            return Result<LawyerProfileDto>.Failure(Error.Validation("Datos de verificación inválidos."));

        var profile = await _lawyerProfileRepository.GetByIdWithDetailsAsync(requestId, cancellationToken);
        if (profile is null)
            return Result<LawyerProfileDto>.Failure(Error.NotFound("Solicitud de verificación no encontrada."));

        if (!profile.IsPending && profile.VerificationStatus != LawyerVerificationStatus.NotSubmitted)
            return Result<LawyerProfileDto>.Failure(Error.Validation("Solo se pueden aprobar solicitudes pendientes."));

        var user = await _userRepository.GetByIdAsync(profile.UserId, cancellationToken);
        if (user is null)
            return Result<LawyerProfileDto>.Failure(Error.NotFound("Usuario no encontrado."));

        profile.Verify(adminUserId);
        user.UpgradeToLawyer();

        _lawyerProfileRepository.Update(profile);
        _userRepository.Update(user);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LawyerProfileDto>.Success(profile.ToDto());
    }

    public async Task<Result<LawyerProfileDto>> RejectAsync(
        Guid requestId,
        Guid adminUserId,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        if (requestId == Guid.Empty || adminUserId == Guid.Empty)
            return Result<LawyerProfileDto>.Failure(Error.Validation("Datos de rechazo inválidos."));

        var profile = await _lawyerProfileRepository.GetByIdWithDetailsAsync(requestId, cancellationToken);
        if (profile is null)
            return Result<LawyerProfileDto>.Failure(Error.NotFound("Solicitud de verificación no encontrada."));

        if (!profile.IsPending && profile.VerificationStatus != LawyerVerificationStatus.NotSubmitted)
            return Result<LawyerProfileDto>.Failure(Error.Validation("Solo se pueden rechazar solicitudes pendientes."));

        profile.RejectVerification(reason);
        _lawyerProfileRepository.Update(profile);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<LawyerProfileDto>.Success(profile.ToDto());
    }

    public async Task<Result<LawyerProfileDto>> GetByUserIdAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
            return Result<LawyerProfileDto>.Failure(Error.Validation("Usuario inválido."));

        var profile = await _lawyerProfileRepository.GetByUserIdWithDetailsAsync(userId, cancellationToken);
        if (profile is null)
            return Result<LawyerProfileDto>.Failure(Error.NotFound("Solicitud de verificación no encontrada."));

        return Result<LawyerProfileDto>.Success(profile.ToDto());
    }

    public async Task<Result<IReadOnlyList<LawyerVerificationRequestSummaryDto>>> GetAllRequestsAsync(
        LawyerVerificationStatus? status,
        CancellationToken cancellationToken = default)
    {
        var profiles = await _lawyerProfileRepository.GetAllWithDetailsAsync(status, cancellationToken);
        var summaries = profiles.Select(p => p.ToSummaryDto()).ToList();
        return Result<IReadOnlyList<LawyerVerificationRequestSummaryDto>>.Success(summaries);
    }

    public async Task<Result<LawyerVerificationRequestDetailDto>> GetRequestDetailAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        if (requestId == Guid.Empty)
            return Result<LawyerVerificationRequestDetailDto>.Failure(Error.Validation("Solicitud inválida."));

        var profile = await _lawyerProfileRepository.GetByIdWithDetailsAsync(requestId, cancellationToken);
        if (profile is null)
            return Result<LawyerVerificationRequestDetailDto>.Failure(Error.NotFound("Solicitud de verificación no encontrada."));

        return Result<LawyerVerificationRequestDetailDto>.Success(profile.ToDetailDto());
    }

    private static Error? ValidateProfessionalData(
        string licenseNumber,
        string barAssociation,
        string province,
        string specialty)
    {
        if (string.IsNullOrWhiteSpace(licenseNumber) ||
            string.IsNullOrWhiteSpace(barAssociation) ||
            string.IsNullOrWhiteSpace(province) ||
            string.IsNullOrWhiteSpace(specialty))
        {
            return Error.Validation("Todos los campos profesionales son obligatorios.");
        }

        return null;
    }

}
