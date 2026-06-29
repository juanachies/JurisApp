using JurisApp.Application.DTOs.CustomSkills;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Mappings;

public static class CustomSkillMappings
{
    public static CustomSkillDto ToDto(this CustomSkill skill) => new()
    {
        Id = skill.Id,
        LawyerProfileId = skill.LawyerProfileId,
        Name = skill.Name,
        WhenToUse = skill.WhenToUse,
        Instructions = skill.Instructions,
        Examples = skill.Examples,
        RedFlags = skill.RedFlags,
        OutputFormat = skill.OutputFormat
    };
}
