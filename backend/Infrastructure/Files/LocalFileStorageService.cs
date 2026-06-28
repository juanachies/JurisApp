using JurisApp.Application.Interfaces.Files;
using Microsoft.Extensions.Configuration;

namespace JurisApp.Infrastructure.Files;

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _basePath;

    public LocalFileStorageService(IConfiguration configuration)
    {
        _basePath = configuration["FileStorage:BasePath"]
            ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads");

        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var uniqueName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(_basePath, uniqueName);

        await using var output = File.Create(fullPath);
        await fileStream.CopyToAsync(output, cancellationToken);

        return uniqueName;
    }

    public Task<StoredFileContent?> OpenReadAsync(
        string fileUrl,
        CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, fileUrl);

        if (!File.Exists(fullPath))
            return Task.FromResult<StoredFileContent?>(null);

        Stream stream = File.OpenRead(fullPath);
        var fileName = Path.GetFileName(fileUrl);
        var contentType = "application/octet-stream";

        return Task.FromResult<StoredFileContent?>(new StoredFileContent
        {
            Stream = stream,
            FileName = fileName,
            ContentType = contentType
        });
    }

    public Task DeleteFileAsync(string fileUrl, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, fileUrl);

        if (File.Exists(fullPath))
            File.Delete(fullPath);

        return Task.CompletedTask;
    }
}