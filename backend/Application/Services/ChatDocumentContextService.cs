using JurisApp.Application.Interfaces.AI;
using JurisApp.Application.Interfaces.Files;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Services;

public class ChatDocumentContextService : IChatDocumentContextService
{
    private const int MaxCharsPerDocument = 24_000;
    private const int MaxTotalChars = 60_000;

    private readonly IChatRepository _chatRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentTextExtractor _textExtractor;

    public ChatDocumentContextService(
        IChatRepository chatRepository,
        IFolderRepository folderRepository,
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService,
        IDocumentTextExtractor textExtractor)
    {
        _chatRepository = chatRepository;
        _folderRepository = folderRepository;
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _textExtractor = textExtractor;
    }

    public async Task<IReadOnlyList<ChatDocumentContext>> BuildForChatAsync(
        Guid chatId,
        CancellationToken cancellationToken = default)
    {
        var chat = await _chatRepository.GetByIdLightAsync(chatId, cancellationToken);
        if (chat is null)
            return Array.Empty<ChatDocumentContext>();

        var documents = (await _documentRepository.GetByChatIdAsync(chatId, cancellationToken)).ToList();

        Folder? folder = null;
        if (chat.FolderId is Guid folderId)
        {
            folder = await _folderRepository.GetByIdAsync(folderId, cancellationToken);
            var folderDocuments = await _documentRepository.GetByFolderIdAsync(folderId, cancellationToken);
            documents.AddRange(folderDocuments);
        }

        documents = documents
            .DistinctBy(d => d.Id)
            .ToList();

        var contexts = new List<ChatDocumentContext>();
        var totalChars = 0;

        if (folder is not null && !string.IsNullOrWhiteSpace(folder.LegalContext))
        {
            AddContext(
                contexts,
                ref totalChars,
                $"Contexto del caso: {folder.Name}",
                folder.LegalContext);
        }

        foreach (var document in documents)
        {
            if (totalChars >= MaxTotalChars)
                break;

            string content;
            try
            {
                content = await BuildDocumentContentAsync(document, cancellationToken);
            }
            catch
            {
                content = $"[No se pudo leer el archivo «{document.Title}».]";
            }

            if (string.IsNullOrWhiteSpace(content))
                content = $"[El archivo «{document.Title}» no tiene texto extraíble.]";

            var origin = document.FolderId.HasValue ? "Documento del caso" : "Documento del chat";
            AddContext(contexts, ref totalChars, $"{origin}: {document.Title}", content);
        }

        return contexts;
    }

    private static void AddContext(
        List<ChatDocumentContext> contexts,
        ref int totalChars,
        string title,
        string content)
    {
        if (string.IsNullOrWhiteSpace(content) || totalChars >= MaxTotalChars)
            return;

        var remaining = MaxTotalChars - totalChars;
        var maxForDoc = Math.Min(MaxCharsPerDocument, remaining);
        if (maxForDoc <= 0)
            return;

        if (content.Length > maxForDoc)
            content = content[..maxForDoc] + "\n[... contenido truncado ...]";

        totalChars += content.Length;
        contexts.Add(new ChatDocumentContext
        {
            Title = title,
            Content = content
        });
    }

    private async Task<string> BuildDocumentContentAsync(
        Document document,
        CancellationToken cancellationToken)
    {
        var extracted = await ExtractFileTextAsync(document, cancellationToken);
        var analysis = document.Analysis is not null ? FormatAnalysis(document.Analysis) : null;

        if (!string.IsNullOrWhiteSpace(extracted) && !string.IsNullOrWhiteSpace(analysis))
            return extracted + "\n\n--- Análisis previo del documento ---\n" + analysis;

        if (!string.IsNullOrWhiteSpace(extracted))
            return extracted;

        return analysis ?? string.Empty;
    }

    private async Task<string> ExtractFileTextAsync(
        Document document,
        CancellationToken cancellationToken)
    {
        await using var stream = await _fileStorageService.OpenReadAsync(document.Url, cancellationToken);
        return await _textExtractor.ExtractTextAsync(
            FileNameForExtraction(document),
            stream,
            cancellationToken);
    }

    private static string FileNameForExtraction(Document document)
    {
        var fromUrl = Path.GetFileName(document.Url);
        if (!string.IsNullOrEmpty(Path.GetExtension(fromUrl)))
            return fromUrl;

        return document.Title;
    }

    private static string FormatAnalysis(DocumentAnalysis analysis)
    {
        var sections = new List<string>();

        if (!string.IsNullOrWhiteSpace(analysis.Summary))
            sections.Add($"Resumen:\n{analysis.Summary}");

        if (!string.IsNullOrWhiteSpace(analysis.Risks))
            sections.Add($"Riesgos:\n{analysis.Risks}");

        if (!string.IsNullOrWhiteSpace(analysis.Recommendations))
            sections.Add($"Recomendaciones:\n{analysis.Recommendations}");

        if (!string.IsNullOrWhiteSpace(analysis.References))
            sections.Add($"Referencias:\n{analysis.References}");

        return string.Join("\n\n", sections);
    }
}
