namespace JurisApp.Application.DTOs.LawyerProfiles;

public class VerifyLawyerRequest
{
    public Guid LawyerProfileId { get; set; }
    public Guid VerifiedById { get; set; }
}
