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
    private readonly IChatDocumentContextService _chatDocumentContextService;
    private readonly IAIService _aiService;
    private readonly IPlanLimitService _planLimitService;
    private readonly IChatAuditService _chatAuditService;
    private readonly IUnitOfWork _unitOfWork;

    public ChatService(
        IUserRepository userRepository,
        IChatRepository chatRepository,
        IMessageRepository messageRepository,
        IFolderRepository folderRepository,
        ILawyerProfileRepository lawyerProfileRepository,
        ICustomSkillRepository customSkillRepository,
        IChatDocumentContextService chatDocumentContextService,
        IAIService aiService,
        IPlanLimitService planLimitService,
        IChatAuditService chatAuditService,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _chatRepository = chatRepository;
        _messageRepository = messageRepository;
        _folderRepository = folderRepository;
        _lawyerProfileRepository = lawyerProfileRepository;
        _customSkillRepository = customSkillRepository;
        _chatDocumentContextService = chatDocumentContextService;
        _aiService = aiService;
        _planLimitService = planLimitService;
        _chatAuditService = chatAuditService;
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

        var limit = await _planLimitService.EnsureCanCreateChatAsync(userId, cancellationToken);
        if (!limit.IsSuccess)
            return Result<ChatDto>.Failure(limit.Error);

        if (request.FolderId.HasValue)
        {
            var folderError = await FolderOwnershipValidator.ValidateAsync(
                userId, request.FolderId.Value, _folderRepository, _lawyerProfileRepository, cancellationToken);
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

        var activeSkills = await _customSkillRepository.GetAppliedByChatIdAsync(chatId, cancellationToken);
        var skillNames = activeSkills.Select(s => s.Name).ToList();

        var previousMessages = await _messageRepository.GetByChatIdAsync(chatId, cancellationToken);
        var chatDocuments = await _chatDocumentContextService.BuildForChatAsync(chatId, cancellationToken);

        var userMessage = new Message(
            Guid.NewGuid(),
            chatId,
            DateTime.UtcNow,
            MessageRole.User,
            request.Content);
        userMessage.SetSkillsUsed(skillNames);

        await _messageRepository.AddAsync(userMessage, cancellationToken);

        string aiReply;
        try
        {
            aiReply = await _aiService.SendChatMessageAsync(
                request.Content,
                previousMessages,
                activeSkills,
                chatDocuments,
                cancellationToken);
        }
        catch (AIServiceException ex)
        {
            return Result<MessageDto>.Failure(Error.ExternalService(ex.Message));
        }

        await _chatAuditService.RecordAsync(chatId, cancellationToken);

        var assistantMessage = new Message(
            Guid.NewGuid(),
            chatId,
            DateTime.UtcNow,
            MessageRole.Assistant,
            aiReply);
        assistantMessage.SetSkillsUsed(skillNames);

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

}
