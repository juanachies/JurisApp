using JurisApp.Domain.Enums;

namespace JurisApp.Application.DTOs.LawyerProfiles;

public class LawyerProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string BarAssociation { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public LawyerVerificationStatus VerificationStatus { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string? LicenseDocumentUrl { get; set; }
}
