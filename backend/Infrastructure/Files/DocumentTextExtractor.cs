using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using JurisApp.Application.Interfaces.Files;
using UglyToad.PdfPig;

namespace JurisApp.Infrastructure.Files;

public class DocumentTextExtractor : IDocumentTextExtractor
{
    private static readonly HashSet<string> PlainTextExtensions =
        new(StringComparer.OrdinalIgnoreCase)
        {
            ".txt", ".md", ".csv", ".json", ".xml", ".html", ".htm", ".log"
        };

    public async Task<string> ExtractTextAsync(
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(fileName);

        if (PlainTextExtensions.Contains(extension))
            return await ReadPlainTextAsync(fileStream, cancellationToken);

        return extension.ToLowerInvariant() switch
        {
            ".pdf" => ExtractPdfText(fileStream),
            ".docx" => ExtractDocxText(fileStream),
            ".rtf" => await ReadPlainTextAsync(fileStream, cancellationToken),
            _ => throw new NotSupportedException(
                $"El formato '{extension}' no está soportado. Usá PDF, DOCX o texto plano.")
        };
    }

    private static async Task<string> ReadPlainTextAsync(Stream stream, CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private static string ExtractDocxText(Stream stream)
    {
        using var seekable = EnsureSeekable(stream);
        using var document = WordprocessingDocument.Open(seekable, false);
        var body = document.MainDocumentPart?.Document.Body;
        if (body is null)
            return string.Empty;

        var paragraphs = body.Descendants<Paragraph>()
            .Select(p => string.Concat(p.Descendants<Text>().Select(t => t.Text)))
            .Where(t => !string.IsNullOrWhiteSpace(t));

        return string.Join(Environment.NewLine, paragraphs);
    }

    private static string ExtractPdfText(Stream stream)
    {
        using var seekable = EnsureSeekable(stream);
        using var pdf = PdfDocument.Open(seekable);
        var builder = new StringBuilder();

        foreach (var page in pdf.GetPages())
        {
            var text = page.Text;
            if (!string.IsNullOrWhiteSpace(text))
            {
                builder.AppendLine(text);
            }
        }

        return builder.ToString().Trim();
    }

    private static MemoryStream EnsureSeekable(Stream stream)
    {
        var copy = new MemoryStream();
        if (stream.CanSeek)
            stream.Position = 0;
        stream.CopyTo(copy);
        copy.Position = 0;
        return copy;
    }
}
