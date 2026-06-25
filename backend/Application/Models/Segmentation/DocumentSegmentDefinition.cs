namespace JurisApp.Application.Models.Segmentation;

public sealed class DocumentSegmentDefinition
{
    public string Key { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool Countable { get; set; }
}
