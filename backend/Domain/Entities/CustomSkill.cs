using JurisApp.Domain.Common;

namespace JurisApp.Domain.Entities;

public class CustomSkill : BaseEntity
{
    public Guid LawyerProfileId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string WhenToUse { get; private set; } = string.Empty;
    public string Instructions { get; private set; } = string.Empty;
    public string Examples { get; private set; } = string.Empty;
    public string RedFlags { get; private set; } = string.Empty;
    public string OutputFormat { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;

    public LawyerProfile LawyerProfile { get; private set; } = null!;
    public ICollection<ChatCustomSkill> ChatUsages { get; private set; } = new List<ChatCustomSkill>();

    protected CustomSkill() { }

    public CustomSkill(
        Guid id,
        Guid lawyerProfileId,
        string name,
        string whenToUse,
        string instructions,
        string examples,
        string redFlags,
        string outputFormat)
        : base(id)
    {
        LawyerProfileId = lawyerProfileId;
        Name = name;
        WhenToUse = whenToUse;
        Instructions = instructions;
        Examples = examples;
        RedFlags = redFlags;
        OutputFormat = outputFormat;
        IsActive = true;
    }

    public void Activate()
    {
        IsActive = true;
        Touch();
    }

    public void Deactivate()
    {
        IsActive = false;
        Touch();
    }

    public void Update(
        string name,
        string whenToUse,
        string instructions,
        string examples,
        string redFlags,
        string outputFormat)
    {
        Name = name;
        WhenToUse = whenToUse;
        Instructions = instructions;
        Examples = examples;
        RedFlags = redFlags;
        OutputFormat = outputFormat;
        Touch();
    }
}
