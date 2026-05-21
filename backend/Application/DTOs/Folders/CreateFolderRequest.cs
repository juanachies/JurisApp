namespace JurisApp.Application.DTOs.Folders;

public class CreateFolderRequest
{
    public string Name { get; set; } = string.Empty;
    public string? LegalContext { get; set; }
}
