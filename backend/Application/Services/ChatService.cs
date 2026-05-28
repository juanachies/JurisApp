using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Chats;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Mappings;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;

namespace JurisApp.Application.Services;

public class ChatService : IChatService
{
    private readonly IUserRepository _userRepository;
    private readonly IChatRepository _chatRepository;
    private readonly IMessageRepository _messageRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly ILawyerProfileRepository _lawyerProfileRepository;
    private readonly ICustomSkillRepository _customSkillRepository;
    private readonly IAIService _aiService;
    private readonly IUnitOfWork _unitOfWork;

    public ChatService(
        IUserRepository userRepository,
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        IFolderRepository folderRepository,
        ILawyerProfileRepository lawyerProfileRepository,
        ICustomSkillRepository customSkillRepository,
        IAIService aiService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _folderRepository = folderRepository;
        _lawyerProfileRepository = lawyerProfileRepository;
        _customSkillRepository = customSkillRepository;
        _aiService = aiService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<ChatDto>> CreateAsync(Guid userId, CreateChatRequest request, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return Result<ChatDto>.Failure(Error.Validation("Usuario inválido."));
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Result<ChatDto>.Failure(Error.Validation("El título del chat es obligatorio."));
        }

        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result<ChatDto>.Failure(Error.NotFound("Usuario no encontrado."));
        }

        if (request.FolderId.HasValue)
        {
            var folderError = await ValidateFolderOwnershipAsync(userId, request.FolderId.Value, cancellationToken);
            if (folderError is not null)
            {
                return Result<ChatDto>.Failure(folderError);
            }
        }

        var chat = new Chat(Guid.NewGuid(), userId, request.Title);
        if (request.FolderId.HasValue)
        {
            chat.AssignToFolder(request.FolderId.Value);
        }

        await _chatRepository.AddAsync(chat, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<ChatDto>.Success(chat.ToDto(Array.Empty<Message>()));
    }

    public async Task<Result<MessageDto>> SendMessageAsync(Guid userId, Guid chatId, SendMessageRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
        {
            return Result<MessageDto>.Failure(Error.Validation("El mensaje no puede estar vacío."));
        }

        var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
        if (chat is null)
        {
            return Result<MessageDto>.Failure(Error.NotFound("Chat no encontrado."));
        }

        var ownershipError = EnsureChatOwnership(chat, userId);
        if (ownershipError is not null)
        {
            return Result<MessageDto>.Failure(ownershipError);
        }

        var userMessage = new Message(
            Guid.NewGuid(),
            chatId,
            DateTime.UtcNow,
            MessageRole.User,
            request.Content);

        await _messageRepository.AddAsync(userMessage, cancellationToken);

        var previousMessages = await _messageRepository.GetByChatIdAsync(chatId, cancellationToken);
        var activeSkills = await _customSkillRepository.GetActiveByChatIdAsync(chatId, cancellationToken);

        var aiReply = await _aiService.SendChatMessageAsync(
            request.Content,
            previousMessages,
            activeSkills,
            cancellationToken);

        var assistantMessage = new Message(
            Guid.NewGuid(),
            chatId,
            DateTime.UtcNow,
            MessageRole.Assistant,
            aiReply);

        await _messageRepository.AddAsync(assistantMessage, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<MessageDto>.Success(assistantMessage.ToDto());
    }

    public async Task<Result<ChatDto>> GetByIdAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
        if (chat is null)
        {
            return Result<ChatDto>.Failure(Error.NotFound("Chat no encontrado."));
        }

        var ownershipError = EnsureChatOwnership(chat, userId);
        if (ownershipError is not null)
        {
            return Result<ChatDto>.Failure(ownershipError);
        }

        var messages = await _messageRepository.GetByChatIdAsync(chatId, cancellationToken);
        return Result<ChatDto>.Success(chat.ToDto(messages));
    }

    public async Task<Result<IReadOnlyList<ChatSummaryDto>>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (userId == Guid.Empty)
        {
            return Result<IReadOnlyList<ChatSummaryDto>>.Failure(Error.Validation("Usuario inválido."));
        }

        var chats = await _chatRepository.GetByUserIdAsync(userId, cancellationToken);
        var summaries = chats.Select(c => c.ToSummaryDto()).ToList();
        return Result<IReadOnlyList<ChatSummaryDto>>.Success(summaries);
    }

    public async Task<Result> DeleteAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
        if (chat is null)
        {
            return Result.Failure(Error.NotFound("Chat no encontrado."));
        }

        var ownershipError = EnsureChatOwnership(chat, userId);
        if (ownershipError is not null)
        {
            return Result.Failure(ownershipError);
        }

        _chatRepository.Delete(chat);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private static Error? EnsureChatOwnership(Chat chat, Guid userId)
    {
        if (chat.UserId != userId)
        {
            return Error.Unauthorized("No tenés acceso a este chat.");
        }

        return null;
    }

    private async Task<Error?> ValidateFolderOwnershipAsync(Guid userId, Guid folderId, CancellationToken cancellationToken)
    {
        var folder = await _folderRepository.GetByIdAsync(folderId, cancellationToken);
        if (folder is null)
        {
            return Error.NotFound("Carpeta no encontrada.");
        }

        var profile = await _lawyerProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (profile is null || folder.LawyerProfileId != profile.Id)
        {
            return Error.Unauthorized("No tenés acceso a esta carpeta.");
        }

        return null;
    }
}
