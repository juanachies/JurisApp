using System.Text.Json;
using JurisApp.Application.Common;
using JurisApp.Application.DTOs.Analysis;
using JurisApp.Application.Interfaces.AI;
using JurisApp.Application.Interfaces.Files;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Interfaces.Segmentation;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Application.Mappings;
using JurisApp.Application.Models.Segmentation;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Services;

public sealed class SegmentedAnalysisService : ISegmentedAnalysisService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly IChatRepository _chatRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IDocumentAnalysisRepository _documentAnalysisRepository;
    private readonly ICustomSkillRepository _customSkillRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentTextExtractor _textExtractor;
    private readonly IDocumentClassificationService _classificationService;
    private readonly IDocumentSegmentationCatalog _segmentationCatalog;
    private readonly ISegmentedDocumentAnalysisService _segmentedAnalysisService;
    private readonly IUnitOfWork _unitOfWork;

    public SegmentedAnalysisService(
        IChatRepository chatRepository,
        IDocumentRepository documentRepository,
        IDocumentAnalysisRepository documentAnalysisRepository,
        ICustomSkillRepository customSkillRepository,
        IFileStorageService fileStorageService,
        IDocumentTextExtractor textExtractor,
        IDocumentClassificationService classificationService,
        IDocumentSegmentationCatalog segmentationCatalog,
        ISegmentedDocumentAnalysisService segmentedAnalysisService,
        IUnitOfWork unitOfWork)
    {
        _chatRepository = chatRepository;
        _documentRepository = documentRepository;
        _documentAnalysisRepository = documentAnalysisRepository;
        _customSkillRepository = customSkillRepository;
        _fileStorageService = fileStorageService;
        _textExtractor = textExtractor;
        _classificationService = classificationService;
        _segmentationCatalog = segmentationCatalog;
        _segmentedAnalysisService = segmentedAnalysisService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SegmentedDocumentAnalysisDto>> AnalyzeAsync(
        Guid userId,
        AnalyzeSegmentedRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.ChatId == Guid.Empty)
            return Result<SegmentedDocumentAnalysisDto>.Failure(Error.Validation("El chat es obligatorio."));

        var hasDocument = request.DocumentId.HasValue && request.DocumentId.Value != Guid.Empty;
        var hasInput = !string.IsNullOrWhiteSpace(request.Input);

        if (hasDocument == hasInput)
        {
            return Result<SegmentedDocumentAnalysisDto>.Failure(
                Error.Validation("Debés enviar exactamente uno: DocumentId o Input."));
        }

        var chat = await _chatRepository.GetByIdAsync(request.ChatId, cancellationToken);
        if (chat is null || chat.UserId != userId)
            return Result<SegmentedDocumentAnalysisDto>.Failure(Error.Unauthorized("No tenés acceso a este chat."));

        string input;
        Guid? documentId = null;

        if (hasDocument)
        {
            documentId = request.DocumentId!.Value;
            var resolveResult = await ResolveDocumentInputAsync(userId, request.ChatId, documentId.Value, cancellationToken);
            if (!resolveResult.IsSuccess)
                return Result<SegmentedDocumentAnalysisDto>.Failure(resolveResult.Error);

            input = resolveResult.Value!;
        }
        else
        {
            input = request.Input!.Trim();
        }

        var activeSkills = await _customSkillRepository.GetActiveByChatIdAsync(request.ChatId, cancellationToken);

        var classification = await _classificationService.ClassifyAsync(input, cancellationToken);
        var categoryDefinition = await ResolveCategoryDefinitionAsync(classification, cancellationToken);

        SegmentedDocumentAnalysisResult analysis;
        try
        {
            analysis = await _segmentedAnalysisService.AnalyzeAsync(
                input,
                classification,
                categoryDefinition,
                activeSkills,
                cancellationToken);
        }
        catch (AIServiceException ex)
        {
            return Result<SegmentedDocumentAnalysisDto>.Failure(Error.ExternalService(ex.Message));
        }

        if (!documentId.HasValue)
            return Result<SegmentedDocumentAnalysisDto>.Success(analysis.ToDto());

        var persisted = await PersistSegmentedAnalysisAsync(documentId.Value, analysis, cancellationToken);
        return Result<SegmentedDocumentAnalysisDto>.Success(persisted.ToSegmentedDto());
    }

    public async Task<Result<SegmentedDocumentAnalysisDto>> GetByDocumentIdAsync(
        Guid userId,
        Guid documentId,
        CancellationToken cancellationToken = default)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
            return Result<SegmentedDocumentAnalysisDto>.Failure(Error.NotFound("Documento no encontrado."));

        var chat = await _chatRepository.GetByIdAsync(document.ChatId, cancellationToken);
        if (chat is null || chat.UserId != userId)
            return Result<SegmentedDocumentAnalysisDto>.Failure(Error.Unauthorized("No tenés acceso a este documento."));

        if (document.Analysis is null || !document.Analysis.IsSegmented)
        {
            return Result<SegmentedDocumentAnalysisDto>.Failure(
                Error.NotFound("El documento no tiene un análisis segmentado."));
        }

        return Result<SegmentedDocumentAnalysisDto>.Success(document.Analysis.ToSegmentedDto());
    }

    private async Task<Result<string>> ResolveDocumentInputAsync(
        Guid userId,
        Guid chatId,
        Guid documentId,
        CancellationToken cancellationToken)
    {
        var document = await _documentRepository.GetByIdAsync(documentId, cancellationToken);
        if (document is null)
            return Result<string>.Failure(Error.NotFound("Documento no encontrado."));

        if (document.ChatId != chatId)
        {
            return Result<string>.Failure(
                Error.Validation("El documento no pertenece al chat indicado."));
        }

        var chat = await _chatRepository.GetByIdAsync(chatId, cancellationToken);
        if (chat is null || chat.UserId != userId)
            return Result<string>.Failure(Error.Unauthorized("No tenés acceso a este documento."));

        var existingAnalysis = await _documentAnalysisRepository.GetByDocumentIdAsync(documentId, cancellationToken);
        if (existingAnalysis is not null)
        {
            return Result<string>.Failure(Error.Conflict("El documento ya tiene un análisis."));
        }

        try
        {
            await using var stream = await _fileStorageService.OpenReadAsync(document.Url, cancellationToken);
            var documentText = await _textExtractor.ExtractTextAsync(document.Title, stream, cancellationToken);

            if (string.IsNullOrWhiteSpace(documentText))
            {
                return Result<string>.Failure(
                    Error.Validation("No se pudo extraer texto del documento. Verificá que el archivo no esté vacío o sea legible."));
            }

            return Result<string>.Success(documentText);
        }
        catch (FileNotFoundException)
        {
            return Result<string>.Failure(Error.NotFound("Archivo del documento no encontrado."));
        }
        catch (NotSupportedException ex)
        {
            return Result<string>.Failure(Error.Validation(ex.Message));
        }
    }

    private async Task<DocumentCategoryDefinition> ResolveCategoryDefinitionAsync(
        DocumentClassificationResult classification,
        CancellationToken cancellationToken)
    {
        var category = await _segmentationCatalog.GetByCategoryKeyAsync(classification.CategoryKey, cancellationToken);
        if (category is not null)
            return category;

        classification.CategoryKey = "pregunta_libre";
        category = await _segmentationCatalog.GetByCategoryKeyAsync("pregunta_libre", cancellationToken);
        return category ?? new DocumentCategoryDefinition
        {
            DisplayName = "Pregunta libre",
            Segments = []
        };
    }

    private async Task<DocumentAnalysis> PersistSegmentedAnalysisAsync(
        Guid documentId,
        SegmentedDocumentAnalysisResult analysis,
        CancellationToken cancellationToken)
    {
        var (summary, risks, recommendations) = DeriveFlatFields(analysis);

        var entity = DocumentAnalysis.CreateSegmented(
            Guid.NewGuid(),
            documentId,
            analysis.CategoryKey,
            analysis.DisplayName,
            analysis.Confidence,
            JsonSerializer.Serialize(analysis.MainFields, JsonOptions),
            JsonSerializer.Serialize(analysis.Segments.Select(ToPayload).ToList(), JsonOptions),
            JsonSerializer.Serialize(analysis.SuggestedActions.Select(a => new DocumentAnalysisSuggestedActionPayload
            {
                Key = a.Key,
                Title = a.Title
            }).ToList(), JsonOptions),
            summary,
            risks,
            recommendations,
            string.Empty);

        await _documentAnalysisRepository.AddAsync(entity, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return entity;
    }

    private static (string Summary, string Risks, string Recommendations) DeriveFlatFields(
        SegmentedDocumentAnalysisResult analysis)
    {
        var summary = FindSegmentContent(analysis, "resumen")
            ?? analysis.Segments.FirstOrDefault()?.Content
            ?? string.Empty;

        var risks = FormatCountableSegment(analysis, "riesgos");
        var recommendations = FormatCountableSegment(analysis, "recomendaciones");

        return (summary, risks, recommendations);
    }

    private static string? FindSegmentContent(SegmentedDocumentAnalysisResult analysis, string key) =>
        analysis.Segments.FirstOrDefault(s => s.Key == key)?.Content;

    private static string FormatCountableSegment(SegmentedDocumentAnalysisResult analysis, string key)
    {
        var segment = analysis.Segments.FirstOrDefault(s => s.Key == key);
        if (segment is null)
            return string.Empty;

        if (segment.Items.Count == 0)
            return segment.Content;

        return string.Join(
            "\n",
            segment.Items.Select(item =>
                $"- {item.Title}: {item.Description}" +
                (string.IsNullOrWhiteSpace(item.Recommendation) ? string.Empty : $" Recomendación: {item.Recommendation}")));
    }

    private static DocumentAnalysisSegmentPayload ToPayload(DocumentAnalysisSegmentResult segment) => new()
    {
        Key = segment.Key,
        Title = segment.Title,
        Countable = segment.Countable,
        ItemsCount = segment.ItemsCount,
        Severity = segment.Severity,
        Content = segment.Content,
        Items = segment.Items.Select(item => new DocumentAnalysisSegmentItemPayload
        {
            Title = item.Title,
            Description = item.Description,
            Severity = item.Severity,
            Recommendation = item.Recommendation
        }).ToList()
    };
}
