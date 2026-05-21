using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Documents;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Application.Interfaces.Files;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Mappings;
using JurisApp.Application.Services.Interfaces;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Services;

public class DocumentService : IDocumentService
{
    private readonly IChatRepository _chatRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentAnalysisRepository _documentAnalysisRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly ILawyerProfileRepository _lawyerProfileRepository;
    private readonly ICustomSkillRepository _customSkillRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IAIService _aiService;
    private readonly IUnitOfWork _unitOfWork;

    public DocumentService(
        IChatRepository chatRepository,
        IDocumentRepository documentRepository,
        IDocumentAnalysisRepository documentAnalysisRepository,
        IFolderRepository folderRepository,
        ILawyerProfileRepository lawyerProfileRepository,
        ICustomSkillRepository customSkillRepository,
        IFileStorageService fileStorageService,
        IAIService aiService,
        IUnitOfWork unitOfWork)
    {
        _chatRepository = chatRepository;
        _documentRepository = documentRepository;
        _documentAnalysisRepository = documentAnalysisRepository;
        _folderRepository = folderRepository;
        _lawyerProfileRepository = lawyerProfileRepository;
        _customSkillRepository = customSkillRepository;
        _fileStorageService = fileStorageService;
        _aiService = aiService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DocumentDto>> UploadAsync(Guid userId, UploadDocumentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.ChatId == Guid.Empty || string.IsNullOrWhiteSpace(request.Title))
        {
            return Result<DocumentDto>.Failure(Error.Validation("Chat y título son obligatorios."));
        }

        if (request.FileStream == Stream.Null)
        {
            return Result<DocumentDto>.Failure(Error.Validation("El archivo es obligatorio."));
        }

        var chat = await _chatRepository.GetByIdAsync(request.ChatId, cancellationToken);
        if (chat is null)
        {
            return Result<DocumentDto>.Failure(Error.NotFound("Chat no encontrado."));
        }

        if (chat.UserId != userId)
        {
            return Result<DocumentDto>.Failure(Error.Unauthorized("No tenés acceso a este chat."));
        }

        if (request.FolderId.HasValue)
        {
            var folderError = await ValidateFolderOwnershipAsync(userId, request.FolderId.Value, cancellationToken);
            if (folderError is not null)
            {
                return Result<DocumentDto>.Failure(folderError);
            }
        }

        var url = await _fileStorageService.SaveFileAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            cancellationToken);

        var document = new Document(
            Guid.NewGuid(),
            request.ChatId,
            request.Title,
            url,
            request.FolderId);

        await _documentRepository.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DocumentDto>.Success(document.ToDto());
    }

    public async Task<Result<DocumentAnalysisDto>> AnalyzeAsync(Guid userId, AnalyzeDocumentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.DocumentId == Guid.Empty || string.IsNullOrWhiteSpace(request.ExtractedText))
        {
            return Result<DocumentAnalysisDto>.Failure(Error.Validation("Documento y texto extraído son obligatorios."));
        }

        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null)
        {
            return Result<DocumentAnalysisDto>.Failure(Error.NotFound("Documento no encontrado."));
        }

        var chat = await _chatRepository.GetByIdAsync(document.ChatId, cancellationToken);
        if (chat is null || chat.UserId != userId)
        {
            return Result<DocumentAnalysisDto>.Failure(Error.Unauthorized("No tenés acceso a este documento."));
        }

        var existingAnalysis = await _documentAnalysisRepository.GetByDocumentIdAsync(request.DocumentId, cancellationToken);
        if (existingAnalysis is not null)
        {
            return Result<DocumentAnalysisDto>.Failure(Error.Conflict("El documento ya tiene un análisis."));
        }

        var activeSkills = await _customSkillRepository.GetActiveByChatIdAsync(document.ChatId, cancellationToken);

        var aiResult = await _aiService.AnalyzeDocumentAsync(
            request.ExtractedText,
            request.Type,
            activeSkills,
            cancellationToken);

        var analysis = new DocumentAnalysis(
            Guid.NewGuid(),
            request.DocumentId,
            aiResult.Summary,
            aiResult.Risks,
            aiResult.Recommendations,
            aiResult.References,
            request.Type);

        await _documentAnalysisRepository.AddAsync(analysis, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DocumentAnalysisDto>.Success(analysis.ToDto());
    }

    public async Task<Result<IReadOnlyList<DocumentDto>>> GetByChatIdAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
        if (chat is null)
        {
            return Result<IReadOnlyList<DocumentDto>>.Failure(Error.NotFound("Chat no encontrado."));
        }

        if (chat.UserId != userId)
        {
            return Result<IReadOnlyList<DocumentDto>>.Failure(Error.Unauthorized("No tenés acceso a este chat."));
        }

        var documents = await _documentRepository.GetByChatIdAsync(chatId, cancellationToken);
        var dtos = documents.Select(d => d.ToDto()).ToList();
        return Result<IReadOnlyList<DocumentDto>>.Success(dtos);
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
