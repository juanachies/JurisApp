using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Folders;

namespace JurisApp.Application.Interfaces.Services;

public interface IFolderService
{
    Task<Result<FolderDto>> CreateAsync(Guid userId, CreateFolderRequest request, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<FolderDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid userId, Guid folderId, CancellationToken cancellationToken = default);
}
