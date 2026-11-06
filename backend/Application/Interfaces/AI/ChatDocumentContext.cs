namespace JurisApp.Application.Interfaces.AI;

public class ChatDocumentContext
{
    public const string ChatFilePrefix = "Documento del chat: ";
    public const string CaseFilePrefix = "Documento del caso: ";
    public const string CaseContextPrefix = "Contexto del caso: ";

    public string Title { get; init; } = string.Empty;
    public string Content { get; init; } = string.Empty;
}
