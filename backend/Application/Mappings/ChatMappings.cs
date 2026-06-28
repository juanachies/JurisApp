using JurisApp.Application.DTOs.Chats;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Mappings;

public static class ChatMappings
{
    public static ChatDto ToDto(this Chat chat, IEnumerable<Message> messages) => new()
    {
        Id = chat.Id,
        UserId = chat.UserId,
        Title = chat.Title,
        FolderId = chat.FolderId,
        AppliedSkills = chat.AppliedSkills
            .Select(cs => new ChatAppliedSkillDto
            {
                Id = cs.CustomSkillId,
                Name = cs.CustomSkill?.Name ?? string.Empty
            })
            .ToList(),
        Messages = messages.Select(m => m.ToDto()).ToList()
    };

    public static ChatSummaryDto ToSummaryDto(this Chat chat) => new()
    {
        Id = chat.Id,
        Title = chat.Title,
        CreatedAt = chat.CreatedAt,
        FolderId = chat.FolderId
    };

    public static MessageDto ToDto(this Message message) => new()
    {
        Id = message.Id,
        ChatId = message.ChatId,
        Role = message.Role,
        Content = message.Content,
        Date = message.Date,
        SkillsUsed = message.GetSkillsUsed()
    };
}
