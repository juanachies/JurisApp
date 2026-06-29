using JurisApp.Application.Interfaces.Persistence;

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
        if (profile is null || folder.LawyerProfileId != profile.Id)
            return Error.Unauthorized("No tenés acceso a esta carpeta.");

        return null;
    }
}
