using JurisApp.Domain.Common;

namespace JurisApp.Domain.Entities;

public class Folder : BaseEntity
{
    public Guid LawyerProfileId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? LegalContext { get; private set; }

    public LawyerProfile LawyerProfile { get; private set; } = null!;
    public ICollection<Chat> Chats { get; private set; } = new List<Chat>();
    public ICollection<Document> Documents { get; private set; } = new List<Document>();

    protected Folder() { }

    public Folder(Guid id, Guid lawyerProfileId, string name, string? legalContext = null)
        : base(id)
    {
        LawyerProfileId = lawyerProfileId;
        Name = name;
        LegalContext = legalContext;
    }

    public void Update(string name, string? legalContext = null)
    {
        Name = name;
        LegalContext = legalContext;
        Touch();
    }
}
