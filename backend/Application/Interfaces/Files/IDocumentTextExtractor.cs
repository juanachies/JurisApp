namespace JurisApp.Application.Interfaces.Files;

public interface IDocumentTextExtractor
{
    Task<string> ExtractTextAsync(
        string fileName,
        Stream fileStream,
        CancellationToken cancellationToken = default);
}
