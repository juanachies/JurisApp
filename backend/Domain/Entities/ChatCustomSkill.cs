using JurisApp.Domain.Common;

namespace JurisApp.Domain.Entities;

public class ChatCustomSkill : BaseEntity
{
    public Guid ChatId { get; private set; }
    public Guid CustomSkillId { get; private set; }
    public DateTime AppliedAt { get; private set; }

    public Chat Chat { get; private set; } = null!;
    public CustomSkill CustomSkill { get; private set; } = null!;

    protected ChatCustomSkill() { }

    public ChatCustomSkill(Guid id, Guid chatId, Guid customSkillId)
        : base(id)
    {
        ChatId = chatId;
        CustomSkillId = customSkillId;
        AppliedAt = DateTime.UtcNow;
    }
}
