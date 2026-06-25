namespace JurisApp.Application.Models.Segmentation;

public sealed class DocumentCategoryDefinition
{
    public string DisplayName { get; set; } = string.Empty;
    public List<DocumentSegmentDefinition> Segments { get; set; } = new();
}
