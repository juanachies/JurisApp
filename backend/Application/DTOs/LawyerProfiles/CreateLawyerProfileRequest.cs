namespace JurisApp.Application.DTOs.LawyerProfiles;

public class CreateLawyerProfileRequest
{
    public string LicenseNumber { get; set; } = string.Empty;
    public string BarAssociation { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string Specialty { get; set; } = string.Empty;
}
