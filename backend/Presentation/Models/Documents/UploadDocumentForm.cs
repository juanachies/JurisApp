namespace JurisApp.Presentation.Models.Documents;

public class UploadDocumentForm
{
    public IFormFile File { get; set; } = null!;
    public Guid ChatId { get; set; }
    public Guid? FolderId { get; set; }
}
