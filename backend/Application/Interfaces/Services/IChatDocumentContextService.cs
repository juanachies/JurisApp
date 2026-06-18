using JurisApp.Application.Interfaces.AI;

namespace JurisApp.Application.Interfaces.Services;

public interface IChatDocumentContextService
{
    Task<IReadOnlyList<ChatDocumentContext>> BuildForChatAsync(
        Guid chatId,
        CancellationToken cancellationToken = default);
}
