namespace JurisApp.Application.DTOs.LawyerProfiles;

public class RejectLawyerRequest
{
    public Guid LawyerProfileId { get; set; }
    public string? Reason { get; set; }
}
