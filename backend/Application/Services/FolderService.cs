using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Folders;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Mappings;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Services;

public class FolderService : IFolderService
{
    private readonly ILawyerProfileRepository _lawyerProfileRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public FolderService(
        ILawyerProfileRepository lawyerProfileRepository,
        IFolderRepository folderRepository,
        IUnitOfWork unitOfWork)
    {
        _lawyerProfileRepository = lawyerProfileRepository;
        _folderRepository = folderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FolderDto>> CreateAsync(Guid userId, CreateFolderRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<FolderDto>.Failure(Error.Validation("El nombre de la carpeta es obligatorio."));
        }

        var verifiedError = await FolderOwnershipValidator.EnsureVerifiedLawyerAsync(
            userId, _lawyerProfileRepository, cancellationToken);
        if (verifiedError is not null)
            return Result<FolderDto>.Failure(verifiedError);

        var profile = await _lawyerProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
        {
            return Result<FolderDto>.Failure(Error.NotFound("Perfil de abogado no encontrado."));
        }

        var folder = new Folder(Guid.NewGuid(), profile.Id, request.Name, request.LegalContext);
        await _folderRepository.AddAsync(folder, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<FolderDto>.Success(folder.ToDto());
    }

    public async Task<Result<FolderDto>> UpdateAsync(
        Guid userId,
        Guid folderId,
        UpdateFolderRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<FolderDto>.Failure(Error.Validation("El nombre de la carpeta es obligatorio."));
        }

        var folder = await _folderRepository.GetByIdAsync(folderId, cancellationToken);
        if (folder is null)
        {
            return Result<FolderDto>.Failure(Error.NotFound("Carpeta no encontrada."));
        }

        var profile = await _lawyerProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null || !profile.IsVerifiedLawyer || folder.LawyerProfileId != profile.Id)
        {
            return Result<FolderDto>.Failure(Error.Unauthorized("No tenés acceso a esta carpeta."));
        }

        folder.Update(request.Name, request.LegalContext);
        _folderRepository.Update(folder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<FolderDto>.Success(folder.ToDto());
    }

    public async Task<Result<IReadOnlyList<FolderDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var verifiedError = await FolderOwnershipValidator.EnsureVerifiedLawyerAsync(
            userId, _lawyerProfileRepository, cancellationToken);
        if (verifiedError is not null)
            return Result<IReadOnlyList<FolderDto>>.Failure(verifiedError);

        var profile = await _lawyerProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
        {
            return Result<IReadOnlyList<FolderDto>>.Failure(Error.NotFound("Perfil de abogado no encontrado."));
        }

        var folders = await _folderRepository.GetByLawyerProfileIdAsync(profile.Id, cancellationToken);
        var dtos = folders.Select(f => f.ToDto()).ToList();
        return Result<IReadOnlyList<FolderDto>>.Success(dtos);
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid folderId, CancellationToken cancellationToken = default)
    {
        var folder = await _folderRepository.GetByIdAsync(folderId, cancellationToken);
        if (folder is null)
        {
            return Result.Failure(Error.NotFound("Carpeta no encontrada."));
        }

        var profile = await _lawyerProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null || !profile.IsVerifiedLawyer || folder.LawyerProfileId != profile.Id)
        {
            return Result.Failure(Error.Unauthorized("No tenés acceso a esta carpeta."));
        }

        _folderRepository.Delete(folder);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
