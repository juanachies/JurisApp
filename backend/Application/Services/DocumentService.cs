using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Documents;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Application.Interfaces.Files;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Mappings;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;

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
    private readonly IPlanLimitService _planLimitService;
    private readonly IChatAuditService _chatAuditService;
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
        IPlanLimitService planLimitService,
        IChatAuditService chatAuditService,
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
        _planLimitService = planLimitService;
        _chatAuditService = chatAuditService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DocumentDto>> UploadAsync(Guid userId, UploadDocumentRequest request, CancellationToken cancellationToken = default)
    {
        var hasChat = request.ChatId.HasValue && request.ChatId.Value != Guid.Empty;
        var hasFolder = request.FolderId.HasValue && request.FolderId.Value != Guid.Empty;

        if (hasChat == hasFolder)
        {
            return Result<DocumentDto>.Failure(Error.Validation(
                "El documento debe asociarse a un chat o a una carpeta, no a ambos ni a ninguno. Seleccioná o creá un destino antes de continuar."));
        }

        if (string.IsNullOrWhiteSpace(request.Title))
            return Result<DocumentDto>.Failure(Error.Validation("El título es obligatorio."));

        if (request.FileStream == Stream.Null)
            return Result<DocumentDto>.Failure(Error.Validation("El archivo es obligatorio."));

        var limit = await _planLimitService.EnsureCanUploadDocumentAsync(userId, cancellationToken);
        if (!limit.IsSuccess)
            return Result<DocumentDto>.Failure(limit.Error);

        if (hasChat)
        {
            var chat = await _chatRepository.GetByIdAsync(request.ChatId!.Value, cancellationToken);
            if (chat is null)
                return Result<DocumentDto>.Failure(Error.NotFound("Chat no encontrado. Seleccioná o creá uno antes de continuar."));

            if (chat.UserId != userId)
                return Result<DocumentDto>.Failure(Error.Unauthorized("No tenés acceso a este chat."));
        }
        else
        {
            var folderError = await FolderOwnershipValidator.ValidateAsync(
                userId, request.FolderId!.Value, _folderRepository, _lawyerProfileRepository, cancellationToken);
            if (folderError is not null)
                return Result<DocumentDto>.Failure(folderError);
        }

        var url = await _fileStorageService.SaveFileAsync(
            request.FileStream,
            request.FileName,
            request.ContentType,
            cancellationToken);

        var document = new Document(
            Guid.NewGuid(),
            request.Title,
            url,
            hasChat ? request.ChatId : null,
            hasFolder ? request.FolderId : null);

        await _documentRepository.AddAsync(document, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<DocumentDto>.Success(document.ToDto());
    }

    public async Task<Result<DocumentAnalysisDto>> AnalyzeAsync(Guid userId, AnalyzeDocumentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.DocumentId == Guid.Empty)
            return Result<DocumentAnalysisDto>.Failure(Error.Validation("El documento es obligatorio."));

        var document = await _documentRepository.GetByIdAsync(request.DocumentId, cancellationToken);
        if (document is null)
            return Result<DocumentAnalysisDto>.Failure(Error.NotFound("Documento no encontrado."));

        var accessError = await EnsureDocumentAccessAsync(userId, document, cancellationToken);
        if (accessError is not null)
            return Result<DocumentAnalysisDto>.Failure(accessError);

        var types = ResolveAnalysisTypes(request);
        if (types.Count == 0)
            return Result<DocumentAnalysisDto>.Failure(Error.Validation("Indicá al menos un tipo de análisis."));

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

        var skills = await ResolveSkillsAsync(userId, document, request.CustomSkillIds, cancellationToken);

        var promptType = types.Count == 1 ? types[0] : DocumentAnalysisType.Custom;
        DocumentAnalysisResult aiResult;
        try
        {
            aiResult = await _aiService.AnalyzeDocumentAsync(
                documentText,
                promptType,
                skills,
                (await _lawyerProfileRepository.GetByUserIdAsync(userId, cancellationToken))?.Province,
                cancellationToken);
        }
        catch (AIServiceException ex)
        {
            return Result<DocumentAnalysisDto>.Failure(Error.ExternalService(ex.Message));
        }

        var existing = await _documentAnalysisRepository.GetByDocumentIdAsync(request.DocumentId, cancellationToken);
        var storedType = types.Count == 1 ? types[0] : DocumentAnalysisType.Custom;

        if (existing is null)
        {
            existing = new DocumentAnalysis(
                Guid.NewGuid(),
                request.DocumentId,
                ShouldFill(types, DocumentAnalysisType.Summary) ? aiResult.Summary : string.Empty,
                ShouldFill(types, DocumentAnalysisType.RiskAnalysis) ? aiResult.Risks : string.Empty,
                ShouldFill(types, DocumentAnalysisType.Recommendations) ? aiResult.Recommendations : string.Empty,
                aiResult.References,
                storedType);
            await _documentAnalysisRepository.AddAsync(existing, cancellationToken);
        }
        else
        {
            existing.ApplyPartial(
                storedType,
                ShouldFill(types, DocumentAnalysisType.Summary) ? aiResult.Summary : null,
                ShouldFill(types, DocumentAnalysisType.RiskAnalysis) ? aiResult.Risks : null,
                ShouldFill(types, DocumentAnalysisType.Recommendations) ? aiResult.Recommendations : null,
                aiResult.References);
            _documentAnalysisRepository.Update(existing);
        }

        if (document.ChatId.HasValue)
            await _chatAuditService.RecordAsync(document.ChatId.Value, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return Result<DocumentAnalysisDto>.Success(existing.ToDto());
    }

    public async Task<Result<DocumentDto>> GetByIdAsync(Guid userId, Guid documentId, CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
            return Result<DocumentDto>.Failure(Error.NotFound("Documento no encontrado."));

        var accessError = await EnsureDocumentAccessAsync(userId, document, cancellationToken);
        if (accessError is not null)
            return Result<DocumentDto>.Failure(accessError);

        return Result<DocumentDto>.Success(document.ToDto());
    }

    public async Task<Result<IReadOnlyList<DocumentDto>>> GetByChatIdAsync(Guid userId, Guid chatId, CancellationToken cancellationToken = default)
    {
        var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
        if (chat is null)
            return Result<IReadOnlyList<DocumentDto>>.Failure(Error.NotFound("Chat no encontrado."));

        if (chat.UserId != userId)
            return Result<IReadOnlyList<DocumentDto>>.Failure(Error.Unauthorized("No tenés acceso a este chat."));

        var documents = await _documentRepository.GetByChatIdAsync(chatId, cancellationToken);
        return Result<IReadOnlyList<DocumentDto>>.Success(documents.Select(d => d.ToDto()).ToList());
    }

    public async Task<Result<IReadOnlyList<DocumentDto>>> GetByFolderIdAsync(Guid userId, Guid folderId, CancellationToken cancellationToken = default)
    {
        var folderError = await FolderOwnershipValidator.ValidateAsync(
            userId, folderId, _folderRepository, _lawyerProfileRepository, cancellationToken);
        if (folderError is not null)
            return Result<IReadOnlyList<DocumentDto>>.Failure(folderError);

        var documents = await _documentRepository.GetByFolderIdAsync(folderId, cancellationToken);
        return Result<IReadOnlyList<DocumentDto>>.Success(documents.Select(d => d.ToDto()).ToList());
    }

    private async Task<Error?> EnsureDocumentAccessAsync(Guid userId, Document document, CancellationToken cancellationToken)
    {
        if (document.ChatId.HasValue)
        {
            var chat = await _chatRepository.GetByIdAsync(document.ChatId.Value, cancellationToken);
            if (chat is null || chat.UserId != userId)
                return Error.Unauthorized("No tenés acceso a este documento.");
            return null;
        }

        if (document.FolderId.HasValue)
        {
            return await FolderOwnershipValidator.ValidateAsync(
                userId, document.FolderId.Value, _folderRepository, _lawyerProfileRepository, cancellationToken);
        }

        return Error.Unauthorized("No tenés acceso a este documento.");
    }

    private async Task<IReadOnlyList<CustomSkill>> ResolveSkillsAsync(
        Guid userId,
        Document document,
        IReadOnlyList<Guid>? customSkillIds,
        CancellationToken cancellationToken)
    {
        var skills = new List<CustomSkill>();

        if (document.ChatId.HasValue)
        {
            var applied = await _customSkillRepository.GetAppliedByChatIdAsync(document.ChatId.Value, cancellationToken);
            skills.AddRange(applied);
        }

        if (customSkillIds is { Count: > 0 })
        {
            var requested = await _customSkillRepository.GetByIdsAsync(customSkillIds, cancellationToken);
            var profile = await _lawyerProfileRepository.GetByUserIdAsync(userId, cancellationToken);
            foreach (var skill in requested)
            {
                if (profile is not null && profile.IsVerifiedLawyer && skill.LawyerProfileId == profile.Id
                    && skills.All(s => s.Id != skill.Id))
                {
                    skills.Add(skill);
                }
            }
        }

        return skills;
    }

    private static IReadOnlyList<DocumentAnalysisType> ResolveAnalysisTypes(AnalyzeDocumentRequest request)
    {
        if (request.Types is { Count: > 0 })
            return request.Types.Distinct().ToList();

        if (request.Type.HasValue)
            return new[] { request.Type.Value };

        return new[]
        {
            DocumentAnalysisType.Summary,
            DocumentAnalysisType.RiskAnalysis,
            DocumentAnalysisType.Recommendations
        };
    }

    private static bool ShouldFill(IReadOnlyList<DocumentAnalysisType> types, DocumentAnalysisType field)
    {
        if (types.Contains(DocumentAnalysisType.Custom) || types.Contains(DocumentAnalysisType.ContractReview))
            return true;

        return types.Contains(field);
    }
}
