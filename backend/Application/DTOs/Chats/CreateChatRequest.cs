namespace JurisApp.Application.DTOs.Chats;

public class CreateChatRequest
{
    public string Title { get; set; } = string.Empty;
    public Guid? FolderId { get; set; }
}
