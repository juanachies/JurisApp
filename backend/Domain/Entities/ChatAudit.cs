using JurisApp.Domain.Common;

namespace JurisApp.Domain.Entities;

public class ChatAudit : BaseEntity
{
    public Guid ChatId { get; private set; }
    public string Model { get; private set; } = string.Empty;
    public string PromptVersion { get; private set; } = string.Empty;

    public Chat Chat { get; private set; } = null!;

    protected ChatAudit() { }

    public ChatAudit(Guid id, Guid chatId, string model, string promptVersion)
        : base(id)
    {
        ChatId = chatId;
        Model = model;
        PromptVersion = promptVersion;
    }

    public void Update(string model, string promptVersion)
    {
        Model = model;
        PromptVersion = promptVersion;
        Touch();
    }
}
