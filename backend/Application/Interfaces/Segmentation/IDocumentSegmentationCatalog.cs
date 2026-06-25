using JurisApp.Application.Models.Segmentation;

namespace JurisApp.Application.Interfaces.Segmentation;

public interface IDocumentSegmentationCatalog
{
    Task<DocumentCategoryDefinition?> GetByCategoryKeyAsync(string categoryKey, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<DocumentCategoryDefinition>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<string>> GetCategoryKeysAsync(CancellationToken cancellationToken = default);
    bool IsValidCategoryKey(string categoryKey);
}
