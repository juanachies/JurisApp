namespace JurisApp.Application.DTOs.Chats;

public class ChatDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public Guid? FolderId { get; set; }
    public IReadOnlyList<ChatAppliedSkillDto> AppliedSkills { get; set; } = Array.Empty<ChatAppliedSkillDto>();
    public IReadOnlyList<MessageDto> Messages { get; set; } = Array.Empty<MessageDto>();
}
