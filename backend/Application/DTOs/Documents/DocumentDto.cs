namespace JurisApp.Application.DTOs.Documents;

public class DocumentDto
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public Guid? FolderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
