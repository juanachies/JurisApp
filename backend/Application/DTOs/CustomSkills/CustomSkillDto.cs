namespace JurisApp.Application.DTOs.CustomSkills;

public class CustomSkillDto
{
    public Guid Id { get; set; }
    public Guid LawyerProfileId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string WhenToUse { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public string Examples { get; set; } = string.Empty;
    public string RedFlags { get; set; } = string.Empty;
    public string OutputFormat { get; set; } = string.Empty;
}
