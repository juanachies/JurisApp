using JurisApp.Domain.Common;

namespace JurisApp.Domain.Entities;

public class Chat : BaseEntity
{
    public Guid UserId { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public Guid? FolderId { get; private set; }

    public User User { get; private set; } = null!;
    public Folder? Folder { get; private set; }
    public ICollection<Message> Messages { get; private set; } = new List<Message>();
    public ICollection<Document> Documents { get; private set; } = new List<Document>();
    public ICollection<AITask> Tasks { get; private set; } = new List<AITask>();
    public ICollection<ChatCustomSkill> AppliedSkills { get; private set; } = new List<ChatCustomSkill>();

    protected Chat() { }

    public Chat(Guid id, Guid userId, string title)
        : base(id)
    {
        UserId = userId;
        Title = title;
    }

    public void AssignToFolder(Guid? folderId)
    {
        FolderId = folderId;
        Touch();
    }

    public void ApplySkill(Guid customSkillId)
    {
        if (AppliedSkills.Any(s => s.CustomSkillId == customSkillId))
            return;

        AppliedSkills.Add(new ChatCustomSkill(Guid.NewGuid(), Id, customSkillId));
        Touch();
    }

    public void RemoveSkill(Guid customSkillId)
    {
        var skill = AppliedSkills.FirstOrDefault(s => s.CustomSkillId == customSkillId);
        if (skill is null)
            return;

        AppliedSkills.Remove(skill);
        Touch();
    }
}
