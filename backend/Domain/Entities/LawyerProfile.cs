using JurisApp.Domain.Common;
using JurisApp.Domain.Enums;

namespace JurisApp.Domain.Entities;

public class LawyerProfile : BaseEntity
{
    public Guid UserId { get; private set; }
    public string LicenseNumber { get; private set; } = string.Empty;
    public string BarAssociation { get; private set; } = string.Empty;
    public string Province { get; private set; } = string.Empty;
    public string Specialty { get; private set; } = string.Empty;
    public bool IsVerified { get; private set; }
    public Guid? VerifiedById { get; private set; }
    public DateTime? VerifiedAt { get; private set; }
    public LawyerVerificationStatus VerificationStatus { get; private set; }
    public string? RejectionReason { get; private set; }
    public DateTime? ResolvedAt { get; private set; }

    public User User { get; private set; } = null!;
    public ICollection<Folder> Folders { get; private set; } = new List<Folder>();
    public ICollection<CustomSkill> CustomSkills { get; private set; } = new List<CustomSkill>();

    protected LawyerProfile() { }

    public LawyerProfile(
        Guid id,
        Guid userId,
        string licenseNumber,
        string barAssociation,
        string province,
        string specialty)
        : base(id)
    {
        UserId = userId;
        LicenseNumber = licenseNumber;
        BarAssociation = barAssociation;
        Province = province;
        Specialty = specialty;
        VerificationStatus = LawyerVerificationStatus.Pending;
    }

    public void Verify(Guid verifiedById)
    {
        VerificationStatus = LawyerVerificationStatus.Verified;
        IsVerified = true;
        VerifiedById = verifiedById;
        VerifiedAt = DateTime.UtcNow;
        Touch();
    }

    public void RejectVerification(string? reason = null)
    {
        VerificationStatus = LawyerVerificationStatus.Rejected;
        IsVerified = false;
        VerifiedById = null;
        VerifiedAt = null;
        RejectionReason = reason;
        ResolvedAt = DateTime.UtcNow;
        Touch();
    }

    public void MarkAsPendingVerification()
    {
        VerificationStatus = LawyerVerificationStatus.Pending;
        IsVerified = false;
        VerifiedById = null;
        VerifiedAt = null;
        RejectionReason = null;
        ResolvedAt = null;
        Touch();
    }

    public void Resubmit(
        string licenseNumber,
        string barAssociation,
        string province,
        string specialty)
    {
        if (VerificationStatus != LawyerVerificationStatus.Rejected)
            throw new InvalidOperationException("Solo se puede reenviar una solicitud rechazada.");

        Update(licenseNumber, barAssociation, province, specialty);
        MarkAsPendingVerification();
    }

    public bool CanSubmitRequest =>
        VerificationStatus is LawyerVerificationStatus.Rejected or LawyerVerificationStatus.NotSubmitted;

    public bool IsPending => VerificationStatus == LawyerVerificationStatus.Pending;

    public bool IsVerifiedLawyer => VerificationStatus == LawyerVerificationStatus.Verified;

    public void Update(
        string licenseNumber,
        string barAssociation,
        string province,
        string specialty)
    {
        LicenseNumber = licenseNumber;
        BarAssociation = barAssociation;
        Province = province;
        Specialty = specialty;
        Touch();
    }
}
