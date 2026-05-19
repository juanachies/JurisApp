using JurisApp.Domain.Common;
using JurisApp.Domain.Enums;

namespace JurisApp.Domain.Entities;

public class Message : BaseEntity
{
    public Guid ChatId { get; private set; }
    public DateTime Date { get; private set; }
    public MessageRole Role { get; private set; }
    public string Content { get; private set; } = string.Empty;

    public Chat Chat { get; private set; } = null!;

    protected Message() { }

    public Message(Guid id, Guid chatId, DateTime date, MessageRole role, string content)
        : base(id)
    {
        ChatId = chatId;
        Date = date;
        Role = role;
        Content = content;
    }
}
