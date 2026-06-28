namespace JurisApp.Application.Interfaces.Files;

public interface IFileStorageService
{
    Task<string> SaveFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task<StoredFileContent?> OpenReadAsync(
        string fileUrl,
        CancellationToken cancellationToken = default);

    Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default);
}

public sealed class StoredFileContent
{
    public required Stream Stream { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
}
