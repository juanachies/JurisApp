using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Documents;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Application.Interfaces.Files;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Mappings;
using JurisApp.Application.Interfaces.Services;
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
    private readonly IDocumentTextExtractor _textExtractor;
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
        IDocumentTextExtractor textExtractor,
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
        _textExtractor = textExtractor;
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
            var folderError = await FolderOwnershipValidator.ValidateAsync(
                userId, request.FolderId.Value, _folderRepository, _lawyerProfileRepository, cancellationToken);
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
        if (request.DocumentId == Guid.Empty)
        {
            return Result<DocumentAnalysisDto>.Failure(Error.Validation("El documento es obligatorio."));
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

        string documentText;
        try
        {
            await using var stream = await _fileStorageService.OpenReadAsync(document.Url, cancellationToken);
            documentText = await _textExtractor.ExtractTextAsync(document.Title, stream, cancellationToken);
        }
        catch (FileNotFoundException)
        {
            return Result<DocumentAnalysisDto>.Failure(Error.NotFound("Archivo del documento no encontrado."));
        }
        catch (NotSupportedException ex)
        {
            return Result<DocumentAnalysisDto>.Failure(Error.Validation(ex.Message));
        }

        if (string.IsNullOrWhiteSpace(documentText))
        {
            return Result<DocumentAnalysisDto>.Failure(
                Error.Validation("No se pudo extraer texto del documento. Verificá que el archivo no esté vacío o sea legible."));
        }

        var activeSkills = await _customSkillRepository.GetAppliedByChatIdAsync(document.ChatId, cancellationToken);

        DocumentAnalysisResult aiResult;
        try
        {
            aiResult = await _aiService.AnalyzeDocumentAsync(
                documentText,
                request.Type,
                activeSkills,
                cancellationToken);
        }
        catch (AIServiceException ex)
        {
            return Result<DocumentAnalysisDto>.Failure(Error.ExternalService(ex.Message));
        }

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

    public async Task<Result<DocumentDto>> GetByIdAsync(Guid userId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
        {
            return Result<DocumentDto>.Failure(Error.NotFound("Documento no encontrado."));
        }

        var chat = await _chatRepository.GetByIdAsync(document.ChatId, cancellationToken);
        if (chat is null || chat.UserId != userId)
        {
            return Result<DocumentDto>.Failure(Error.Unauthorized("No tenés acceso a este documento."));
        }

        return Result<DocumentDto>.Success(document.ToDto());
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

}
