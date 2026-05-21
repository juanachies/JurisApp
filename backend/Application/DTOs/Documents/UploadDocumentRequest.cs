namespace JurisApp.Application.DTOs.Documents;

public class UploadDocumentRequest
{
    public Guid ChatId { get; set; }
    public Guid? FolderId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public Stream FileStream { get; set; } = Stream.Null;
}
