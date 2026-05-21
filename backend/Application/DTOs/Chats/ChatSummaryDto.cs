namespace JurisApp.Application.DTOs.Chats;

public class ChatSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public Guid? FolderId { get; set; }
}
