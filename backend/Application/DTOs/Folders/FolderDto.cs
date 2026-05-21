namespace JurisApp.Application.DTOs.Folders;

public class FolderDto
{
    public Guid Id { get; set; }
    public Guid LawyerProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? LegalContext { get; set; }
}
