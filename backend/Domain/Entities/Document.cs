using JurisApp.Domain.Common;

namespace JurisApp.Domain.Entities;

public class Document : BaseEntity
{
    public Guid ChatId { get; private set; }
    public Guid? FolderId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;

    public Chat Chat { get; private set; } = null!;
    public Folder? Folder { get; private set; }
    public DocumentAnalysis? Analysis { get; private set; }

    protected Document() { }

    public Document(Guid id, Guid chatId, string title, string url, Guid? folderId = null)
        : base(id)
    {
        ChatId = chatId;
        Title = title;
        Url = url;
        FolderId = folderId;
    }
}
