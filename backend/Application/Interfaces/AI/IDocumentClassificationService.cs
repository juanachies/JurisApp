namespace JurisApp.Application.Interfaces.AI;

public interface IDocumentClassificationService
{
    Task<DocumentClassificationResult> ClassifyAsync(
        string input,
        CancellationToken cancellationToken = default);
}
