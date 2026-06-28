using JurisApp.Application.DTOs.LawyerProfiles;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Mappings;

public static class LawyerProfileMappings
{
    public static LawyerProfileDto ToDto(this LawyerProfile profile) => new()
    {
        Id = profile.Id,
        UserId = profile.UserId,
        LicenseNumber = profile.LicenseNumber,
        BarAssociation = profile.BarAssociation,
        Province = profile.Province,
        Specialty = profile.Specialty,
        IsVerified = profile.IsVerified,
        VerificationStatus = profile.VerificationStatus,
        RejectionReason = profile.RejectionReason,
        VerifiedAt = profile.VerifiedAt,
        ResolvedAt = profile.ResolvedAt
    };

    public static LawyerVerificationRequestSummaryDto ToSummaryDto(this LawyerProfile profile) => new()
    {
        Id = profile.Id,
        UserId = profile.UserId,
        UserFirstName = profile.User.FirstName,
        UserLastName = profile.User.LastName,
        UserEmail = profile.User.Email,
        LicenseNumber = profile.LicenseNumber,
        BarAssociation = profile.BarAssociation,
        Province = profile.Province,
        Specialty = profile.Specialty,
        VerificationStatus = profile.VerificationStatus,
        CreatedAt = profile.CreatedAt,
        VerifiedAt = profile.VerifiedAt,
        ResolvedAt = profile.ResolvedAt
    };

    public static LawyerVerificationRequestDetailDto ToDetailDto(this LawyerProfile profile) => new()
    {
        Id = profile.Id,
        UserId = profile.UserId,
        UserFirstName = profile.User.FirstName,
        UserLastName = profile.User.LastName,
        UserEmail = profile.User.Email,
        LicenseNumber = profile.LicenseNumber,
        BarAssociation = profile.BarAssociation,
        Province = profile.Province,
        Specialty = profile.Specialty,
        VerificationStatus = profile.VerificationStatus,
        IsVerified = profile.IsVerified,
        RejectionReason = profile.RejectionReason,
        CreatedAt = profile.CreatedAt,
        VerifiedAt = profile.VerifiedAt,
        ResolvedAt = profile.ResolvedAt
    };
}
