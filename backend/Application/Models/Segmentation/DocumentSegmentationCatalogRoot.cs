namespace JurisApp.Application.Models.Segmentation;

public sealed class DocumentSegmentationCatalogRoot
{
    public Dictionary<string, DocumentCategoryDefinition> DocumentSegmentations { get; set; } = new();
}
