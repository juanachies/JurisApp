namespace JurisApp.Application.DTOs.Folders;

public class UpdateFolderRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LegalContext { get; set; }
}
