using JurisApp.Application.Common;
using JurisApp.Application.DTOs.LawyerProfiles;
using JurisApp.Application.Interfaces.Files;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Mappings;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;

namespace JurisApp.Application.Services;

public class LawyerProfileService : ILawyerProfileService
{
    private static readonly HashSet<string> AllowedLicenseContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "image/jpeg",
        "image/jpg",
        "image/png",
        "application/pdf"
    };

    private const long MaxLicenseFileBytes = 5 * 1024 * 1024;

    private readonly IUserRepository _userRepository;
    private readonly ILawyerProfileRepository _lawyerProfileRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IUnitOfWork _unitOfWork;

    public LawyerProfileService(
        IUserRepository userRepository,
        ILawyerProfileRepository lawyerProfileRepository,
        ISubscriptionRepository subscriptionRepository,
        IPlanRepository planRepository,
        IFileStorageService fileStorageService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _lawyerProfileRepository = lawyerProfileRepository;
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _fileStorageService = fileStorageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<LawyerProfileDto>> CreateVerificationRequestAsync(
        Guid userId,
        CreateLawyerProfileRequest request,
        Stream? licenseDocument,
        string? fileName,
        string? contentType,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateProfessionalData(request.LicenseNumber, request.BarAssociation, request.Province, request.Specialty);
        if (validationError is not null)
            return Result<LawyerProfileDto>.Failure(validationError);

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
            return Result<LawyerProfileDto>.Failure(Error.NotFound("Usuario no encontrado."));

        var planError = await EnsureProOrMaxAsync(userId, cancellationToken);
        if (planError is not null)
            return Result<LawyerProfileDto>.Failure(planError);

        var fileError = ValidateLicenseFile(licenseDocument, fileName, contentType);
        if (fileError is not null)
            return Result<LawyerProfileDto>.Failure(fileError);

        var existingProfile = await _lawyerProfileRepository.GetByUserIdWithDetailsAsync(userId, cancellationToken);

        if (existingProfile?.IsVerifiedLawyer == true)
            return Result<LawyerProfileDto>.Failure(Error.Conflict("Ya sos un abogado verificado."));

        if (existingProfile?.IsPending == true)
            return Result<LawyerProfileDto>.Failure(Error.Conflict("Ya tenés una solicitud de verificación pendiente."));

        var licenseUrl = await _fileStorageService.SaveFileAsync(
            licenseDocument!,
            fileName!,
            contentType ?? "application/octet-stream",
            cancellationToken);

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

            existingProfile.SetLicenseDocumentUrl(licenseUrl);
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
            profile.SetLicenseDocumentUrl(licenseUrl);

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

    private async Task<Error?> EnsureProOrMaxAsync(Guid userId, CancellationToken cancellationToken)
    {
        var subscription = await _subscriptionRepository.GetActiveByUserIdAsync(userId, cancellationToken);
        if (subscription is null)
            return Error.Validation("Necesitás un plan Pro activo antes de solicitar la verificación como abogado.");

        var plan = await _planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);
        if (plan is null || (plan.Type != PlanType.Pro && plan.Type != PlanType.Max))
            return Error.Validation("Necesitás un plan Pro activo antes de solicitar la verificación como abogado.");

        return null;
    }

    private static Error? ValidateLicenseFile(Stream? stream, string? fileName, string? contentType)
    {
        if (stream is null || stream == Stream.Null || string.IsNullOrWhiteSpace(fileName))
            return Error.Validation("La foto o el documento de matrícula es obligatorio.");

        if (!string.IsNullOrWhiteSpace(contentType) && !AllowedLicenseContentTypes.Contains(contentType))
            return Error.Validation("El documento de matrícula debe ser JPG, PNG o PDF.");

        if (stream.CanSeek && stream.Length > MaxLicenseFileBytes)
            return Error.Validation("El documento de matrícula no puede superar los 5 MB.");

        return null;
    }
}
