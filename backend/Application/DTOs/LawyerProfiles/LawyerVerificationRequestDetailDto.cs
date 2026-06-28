using JurisApp.Domain.Enums;

namespace JurisApp.Application.DTOs.LawyerProfiles;

public class LawyerVerificationRequestDetailDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserFirstName { get; set; } = string.Empty;
    public string UserLastName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string BarAssociation { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
    public LawyerVerificationStatus VerificationStatus { get; set; }
    public bool IsVerified { get; set; }
    public string? RejectionReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? VerifiedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
