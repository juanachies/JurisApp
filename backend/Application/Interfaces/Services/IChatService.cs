using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Chats;

namespace JurisApp.Application.Interfaces.Services;

public interface IChatService
{
    Task<Result<ChatDto>> CreateAsync(Guid userId, CreateChatRequest request, CancellationToken cancellationToken = default);
    Task<Result<MessageDto>> SendMessageAsync(Guid userId, Guid chatId, SendMessageRequest request, CancellationToken cancellationToken = default);
    Task<Result<ChatDto>> GetByIdAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ChatSummaryDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default);
}
