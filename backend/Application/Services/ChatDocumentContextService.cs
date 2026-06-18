using JurisApp.Application.Interfaces.AI;
using JurisApp.Application.Interfaces.Files;
using JurisApp.Application.Interfaces.Persistence;
using JurisApp.Application.Interfaces.Services;
using JurisApp.Domain.Entities;

namespace JurisApp.Application.Services;

public class ChatDocumentContextService : IChatDocumentContextService
{
    private const int MaxCharsPerDocument = 12_000;
    private const int MaxTotalChars = 30_000;

    private readonly IDocumentRepository _documentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly IDocumentTextExtractor _textExtractor;

    public ChatDocumentContextService(
        IDocumentRepository documentRepository,
        IFileStorageService fileStorageService,
        IDocumentTextExtractor textExtractor)
    {
        _documentRepository = documentRepository;
        _fileStorageService = fileStorageService;
        _textExtractor = textExtractor;
    }

    public async Task<IReadOnlyList<ChatDocumentContext>> BuildForChatAsync(
        Guid chatId,
        CancellationToken cancellationToken = default)
    {
        var documents = await _documentRepository.GetByChatIdAsync(chatId, cancellationToken);
        if (documents.Count == 0)
            return Array.Empty<ChatDocumentContext>();

        var contexts = new List<ChatDocumentContext>();
        var totalChars = 0;

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
                continue;
            }

            if (string.IsNullOrWhiteSpace(content))
                continue;

            var remaining = MaxTotalChars - totalChars;
            var maxForDoc = Math.Min(MaxCharsPerDocument, remaining);
            if (content.Length > maxForDoc)
                content = content[..maxForDoc] + "\n[... contenido truncado ...]";

            totalChars += content.Length;
            contexts.Add(new ChatDocumentContext
            {
                Title = document.Title,
                Content = content
            });
        }

        return contexts;
    }

    private async Task<string> BuildDocumentContentAsync(
        Document document,
        CancellationToken cancellationToken)
    {
        if (document.Analysis is not null)
            return FormatAnalysis(document.Analysis);

        await using var stream = await _fileStorageService.OpenReadAsync(document.Url, cancellationToken);
        return await _textExtractor.ExtractTextAsync(document.Title, stream, cancellationToken);
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
