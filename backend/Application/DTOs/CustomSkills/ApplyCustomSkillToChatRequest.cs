namespace JurisApp.Application.DTOs.CustomSkills;

public class ApplyCustomSkillToChatRequest
{
    public Guid ChatId { get; set; }
    public Guid CustomSkillId { get; set; }
}
