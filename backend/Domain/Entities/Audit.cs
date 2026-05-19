using JurisApp.Domain.Common;

namespace JurisApp.Domain.Entities;

public class Audit : BaseEntity
{
    public Guid ChatId { get; private set; }
    public string Model { get; private set; } = string.Empty;
    public string PromptVersion { get; private set; } = string.Empty;

    public Chat Chat { get; private set; } = null!;

    protected Audit() { }

    public Audit(Guid id, Guid chatId, string model, string promptVersion)
        : base(id)
    {
        ChatId = chatId;
        Model = model;
        PromptVersion = promptVersion;
    }
}
