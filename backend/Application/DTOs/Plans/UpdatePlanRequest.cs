using JurisApp.Domain.Enums;

namespace JurisApp.Application.DTOs.Plans;

public class UpdatePlanRequest
{
    public string Name { get; set; } = string.Empty;
    public PlanType Type { get; set; }
    public decimal Price { get; set; }
    public string LimitsJson { get; set; } = string.Empty;
}
