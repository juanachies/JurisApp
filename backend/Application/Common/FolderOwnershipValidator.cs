using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Domain.Enums;

namespace JurisApp.Application.Common;

public static class FolderOwnershipValidator
{
    public static async Task<Error?> ValidateAsync(
        Guid userId,
        Guid folderId,
        IFolderRepository folderRepository,
        ILawyerProfileRepository lawyerProfileRepository,
        CancellationToken cancellationToken = default)
    {
        var folder = await folderRepository.GetByIdAsync(folderId, cancellationToken);
        if (folder is null)
            return Error.NotFound("Carpeta no encontrada.");

        var profile = await lawyerProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null || !profile.IsVerifiedLawyer || folder.LawyerProfileId != profile.Id)
            return Error.Unauthorized("No tenés acceso a esta carpeta.");

        return null;
    }

    public static async Task<Error?> EnsureVerifiedLawyerAsync(
        Guid userId,
        ILawyerProfileRepository lawyerProfileRepository,
        CancellationToken cancellationToken = default)
    {
        var profile = await lawyerProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null)
            return Error.NotFound("Perfil de abogado no encontrado.");

        if (!profile.IsVerifiedLawyer)
            return Error.Unauthorized("Solo los abogados verificados pueden usar esta funcionalidad.");

        return null;
    }
}
