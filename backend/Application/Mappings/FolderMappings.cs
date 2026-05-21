using JurisApp.Application.DTOs.Folders;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Mappings;

public static class FolderMappings
{
    public static FolderDto ToDto(this Folder folder) => new()
    {
        Id = folder.Id,
        LawyerProfileId = folder.LawyerProfileId,
        Name = folder.Name,
        LegalContext = folder.LegalContext
    };
}
