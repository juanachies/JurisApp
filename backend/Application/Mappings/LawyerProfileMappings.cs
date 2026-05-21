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
        VerifiedAt = profile.VerifiedAt
    };
}
