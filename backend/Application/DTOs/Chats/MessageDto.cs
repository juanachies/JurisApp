using JurisApp.Domain.Enums;

namespace JurisApp.Application.DTOs.Chats;

public class MessageDto
{
    public Guid Id { get; set; }
    public Guid ChatId { get; set; }
    public MessageRole Role { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    /// <summary>Debug: skills activas en el chat al generar esta respuesta.</summary>
    public IReadOnlyList<string> SkillsUsed { get; set; } = Array.Empty<string>();
}
