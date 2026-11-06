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
                $"{ChatDocumentContext.CaseContextPrefix}{folder.Name}",
                folder.LegalContext);
        }

        foreach (var document in documents)
        {
            if (totalChars >= MaxTotalChars)
                break;

            string content;
            try
            {
                content = await ExtractDocumentTextAsync(document, cancellationToken);
            }
            catch
            {
                content = Unreadable(document.Title);
            }

            if (string.IsNullOrWhiteSpace(content))
                content = EmptyText(document.Title);

            var prefix = document.FolderId.HasValue
                ? ChatDocumentContext.CaseFilePrefix
                : ChatDocumentContext.ChatFilePrefix;
            AddContext(contexts, ref totalChars, $"{prefix}{document.Title}", content);
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
            content = MarkPartial(content, maxForDoc);

        totalChars += content.Length;
        contexts.Add(new ChatDocumentContext
        {
            Title = title,
            Content = content
        });
    }

    private const string PartialNotice =
        "[Extracción parcial: el texto siguiente es solo un fragmento. No afirmes que algo falta en el resto del documento.]\n";

    private const string TruncationSuffix = "\n[... contenido truncado ...]";

    private static string Unreadable(string title) =>
        $"[No se pudo leer el archivo «{title}». No hay texto disponible.]";

    private static string EmptyText(string title) =>
        $"[El archivo «{title}» no tiene texto extraíble. No hay texto disponible.]";

    private static string MarkPartial(string content, int maxForDoc)
    {
        var budget = maxForDoc - PartialNotice.Length - TruncationSuffix.Length;
        if (budget < 1)
            return PartialNotice;

        return PartialNotice + content[..budget] + TruncationSuffix;
    }

    private async Task<string> ExtractDocumentTextAsync(
        Document document,
        CancellationToken cancellationToken)
    {
        var extracted = await ExtractFileTextAsync(document, cancellationToken);
        return extracted.Trim();
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
}
